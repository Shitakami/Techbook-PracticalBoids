using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Shitakami.PracticalBoids
{
    public class BoidsSystemCore : IDisposable
    {
        private readonly int _instanceCount;
        private NativeArray<BoidsData> _boidsDataArray;
        private NativeParallelMultiHashMap<int3, int> _gridHashMap;
        private NativeArray<float3> _boidsSteers;
        private NativeArray<Matrix4x4> _boidsTransformMatrixArray;
        private NativeArray<SpherecastCommand> _spherecastCommands;
        private NativeArray<RaycastHit> _raycastHitArray;
        private NativeArray<SphereObstacleData> _sphereObstacleDataArray;
        
        public NativeArray<Matrix4x4> BoidsTransformMatrixArray => _boidsTransformMatrixArray;
        private JobHandle _jobHandle;
        private JobHandle _registerInstanceToGridHandle;

        public BoidsSystemCore(int instanceCount, int sphereObstacleCount)
        {
            _instanceCount = instanceCount;

            _boidsDataArray = new NativeArray<BoidsData>(_instanceCount, Allocator.Persistent);
            _gridHashMap = new NativeParallelMultiHashMap<int3, int>(_instanceCount, Allocator.Persistent);
            _boidsSteers = new NativeArray<float3>(_instanceCount, Allocator.Persistent);
            _boidsTransformMatrixArray = new NativeArray<Matrix4x4>(_instanceCount, Allocator.Persistent);
            _spherecastCommands = new NativeArray<SpherecastCommand>(_instanceCount, Allocator.Persistent);
            _raycastHitArray = new NativeArray<RaycastHit>(_instanceCount, Allocator.Persistent);
            _sphereObstacleDataArray = new NativeArray<SphereObstacleData>(sphereObstacleCount, Allocator.Persistent);
        }

        public void Setup(
            BoidsSetting boidsSetting,
            float3 simulationAreaCenter, 
            float3 simulationAreaScale)
        {
            BoidsUtility.InitializeBoidsData(
                _boidsDataArray,
                simulationAreaCenter,
                simulationAreaScale / 2,
                boidsSetting.InitializedSpeed
            );
        }

        public void ExecuteUpdate(
            BoidsSetting boidsSetting,
            AvoidObstacleSetting avoidObstacleSetting,
            float3 simulationAreaCenter,
            float3 simulationAreaScale,
            BoidsBehaviour.JobBatchSetting jobBatchSetting)
        {
            _gridHashMap.Clear();

            var registerInstanceToGridJob = new RegisterBoidsIndexToGridJob
            (
                _gridHashMap.AsParallelWriter(),
                _boidsDataArray,
                boidsSetting.MaxAffectedRadius
            );

            _registerInstanceToGridHandle = registerInstanceToGridJob.Schedule(
                _instanceCount,
                jobBatchSetting.RegisterInstanceToGridBatchCount);

            var calculateBoidsSteerForceJob = new CalculateBoidsSteerForceJob
            (
                _boidsDataArray,
                _boidsSteers,
                _gridHashMap,
                boidsSetting.CohesionWeight,
                boidsSetting.CohesionAffectedRadius,
                boidsSetting.CohesionViewDot,
                boidsSetting.SeparationWeight,
                boidsSetting.SeparationAffectedRadius,
                boidsSetting.SeparationViewDot,
                boidsSetting.AlignmentWeight,
                boidsSetting.AlignmentAffectedRadius,
                boidsSetting.AlignmentViewDot,
                boidsSetting.MaxSpeed,
                boidsSetting.MaxSteerForce,
                boidsSetting.MaxAffectedRadius
            );

            var calculateBoidsSteerForceHandle = calculateBoidsSteerForceJob.Schedule(
                _instanceCount,
                jobBatchSetting.CalculateSteerForceBatchCount,
                _registerInstanceToGridHandle);

            var applySteerForceJob = new ApplySteerForceJob
            (
                _boidsDataArray,
                _boidsSteers,
                _sphereObstacleDataArray,
                _spherecastCommands,
                simulationAreaCenter,
                simulationAreaScale / 2,
                boidsSetting.ReturnSimulationAreaWeight,
                Time.deltaTime,
                boidsSetting.MaxSpeed,
                avoidObstacleSetting.SpherecastRadius,
                avoidObstacleSetting.SpherecastDistance,
                avoidObstacleSetting.EscapeObstaclesWeight,
                avoidObstacleSetting.EscapeMaxSpeed
            );

            var applySteerForceJobHandle = applySteerForceJob.Schedule(
                _instanceCount,
                jobBatchSetting.ApplySteerForceBatchCount,
                calculateBoidsSteerForceHandle);

            var spherecastJobHandle = SpherecastCommand.ScheduleBatch(
                _spherecastCommands,
                _raycastHitArray,
                jobBatchSetting.SpherecastCommandBatchCount,
                applySteerForceJobHandle
            );

            var avoidObstacleAndUpdateBoidsJob = new AvoidObstaclesAndUpdateBoidsJob(
                _raycastHitArray,
                _boidsDataArray,
                avoidObstacleSetting.AvoidRotateVelocity,
                Time.deltaTime
            );
            
            var avoidObstacleAndUpdateBoidsJobHandle = avoidObstacleAndUpdateBoidsJob.Schedule(
                _instanceCount,
                jobBatchSetting.AvoidObstacleAndUpdateBoidsJobBatchCount,
                spherecastJobHandle);
            
            var translateMatrix4x4Job = new TranslateMatrix4x4Job
            (
                _boidsDataArray,
                _boidsTransformMatrixArray,
                boidsSetting.InstanceScale
            );
            
            _jobHandle = translateMatrix4x4Job.Schedule(
                _instanceCount,
                jobBatchSetting.TranslateMatrix4x4BatchCount,
                avoidObstacleAndUpdateBoidsJobHandle);
            
            JobHandle.ScheduleBatchedJobs();
        }

        public void UpdateSphereObstacles(IReadOnlyList<SphereObstacle> sphereObstacles)
        {
            for (var i = 0; i < sphereObstacles.Count; i++)
            {
                _sphereObstacleDataArray[i] = sphereObstacles[i].SphereObstacleData;
            }
        }

        public void Complete()
        {
            _jobHandle.Complete();
        }

        public void Dispose()
        {
            _boidsDataArray.Dispose();
            _gridHashMap.Dispose();
            _boidsSteers.Dispose();
            _boidsTransformMatrixArray.Dispose();
            _spherecastCommands.Dispose();
            _raycastHitArray.Dispose();
            _sphereObstacleDataArray.Dispose();
        }
    }
}
