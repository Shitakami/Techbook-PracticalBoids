using System.Collections.Generic;
using Common;
using Part4;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part5
{
    public class BoidsSystemCore
    {
        private readonly int _instanceCount;
        private NativeArray<BoidsData> _boidsDataArray;
        private NativeArray<bool> _aliveFlagDataArray;
        private NativeParallelMultiHashMap<int3, int> _gridHashMap;
        private NativeArray<SpherecastCommand> _spherecastCommands;
        private NativeArray<RaycastHit> _raycastHits;
        private NativeArray<float3> _boidsSteers;
        private NativeArray<Matrix4x4> _boidsTransformMatrices;
        private NativeArray<SphereObstacleData> _sphereObstacleDataArray;
        private NativeArray<BulletData> _bulletDataArray;
        private NativeArray<CollisionData> _collisionDataArray;

        public NativeArray<Matrix4x4> BoidsTransformMatrices => _boidsTransformMatrices;
        private JobHandle _jobHandle;
        private JobHandle _collisionCheckJobHandle;

        public BoidsSystemCore(
            int instanceCount,
            int sphereObstaclesCount,
            int bulletsCount)
        {
            _instanceCount = instanceCount;

            _boidsDataArray = new NativeArray<BoidsData>(_instanceCount, Allocator.Domain);
            _aliveFlagDataArray = new NativeArray<bool>(_instanceCount, Allocator.Domain);
            _gridHashMap = new NativeParallelMultiHashMap<int3, int>(_instanceCount, Allocator.Domain);
            _spherecastCommands = new NativeArray<SpherecastCommand>(_instanceCount, Allocator.Domain);
            _raycastHits = new NativeArray<RaycastHit>(_instanceCount, Allocator.Domain);
            _boidsSteers = new NativeArray<float3>(_instanceCount, Allocator.Domain);
            _boidsTransformMatrices = new NativeArray<Matrix4x4>(_instanceCount, Allocator.Domain);
            _sphereObstacleDataArray = new NativeArray<SphereObstacleData>(sphereObstaclesCount, Allocator.Domain);
            _bulletDataArray = new NativeArray<BulletData>(bulletsCount, Allocator.Domain);
            _collisionDataArray = new NativeArray<CollisionData>(bulletsCount, Allocator.Domain);
        }

        public void Setup(
            Part2.BoidsSetting boidsSetting,
            float3 simulationAreaCenter, 
            float3 simulationAreaScale)
        {
            var simulationAreaScaleHalf = simulationAreaScale / 2;
            var initializeVelocity = boidsSetting.InitializedSpeed;

            for (var i = 0; i < _boidsDataArray.Length; ++i)
            {
                _boidsDataArray[i] = new BoidsData
                {
                    Position = simulationAreaScaleHalf * UnityEngine.Random.insideUnitSphere + simulationAreaCenter,
                    Velocity = UnityEngine.Random.insideUnitSphere * initializeVelocity
                };

                _aliveFlagDataArray[i] = true;
            }
        }

        public void ExecuteUpdate(
            Part2.BoidsSetting boidsSetting,
            AvoidObstacleSetting avoidObstacleSetting,
            float boidsColliderRadius,
            float3 simulationAreaCenter,
            float3 simulationAreaScale,
            BoidsBehaviour.JobBatchSetting jobBatchSetting)
        {
            _gridHashMap.Clear();

            var registerInstanceToGridJob = new RegisterBoidsDataToGridJob
            (
                _gridHashMap.AsParallelWriter(),
                _boidsDataArray,
                _aliveFlagDataArray,
                boidsSetting.MaxAffectedRadius
            );

            var registerInstanceToGridHandle = registerInstanceToGridJob.Schedule(
                _instanceCount,
                jobBatchSetting.RegisterInstanceToGridBatchCount);

            var collisionDetectionJob = new CollisionDetectionJob
            (
                _bulletDataArray,
                _boidsDataArray,
                _gridHashMap,
                boidsSetting.MaxAffectedRadius,
                boidsRadius: boidsColliderRadius,
                Time.deltaTime,
                _aliveFlagDataArray,
                _collisionDataArray
            );

            _collisionCheckJobHandle = collisionDetectionJob.Schedule(
                _bulletDataArray.Length,
                jobBatchSetting.CollisionDetectionBatchCount,
                registerInstanceToGridHandle);

            var calculateBoidsSteerForceJob = new CalculateBoidsSteerForceJob
            (
                _boidsDataArray,
                _boidsSteers,
                _gridHashMap,
                _aliveFlagDataArray,
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
                JobHandle.CombineDependencies(registerInstanceToGridHandle, _collisionCheckJobHandle));

            var applySteerForce = new ApplySteerForceJob
            (
                _boidsDataArray,
                _boidsSteers,
                _sphereObstacleDataArray,
                _spherecastCommands,
                _aliveFlagDataArray,
                simulationAreaCenter,
                simulationAreaScale / 2,
                boidsSetting.ReturnSimulationAreaWeight,
                Time.deltaTime,
                boidsSetting.MaxSpeed,
                avoidObstacleSetting.SpherecastDistance,
                avoidObstacleSetting.SpherecastRadius,
                avoidObstacleSetting.EscapeObstaclesWeight,
                avoidObstacleSetting.EscapeMaxSpeed
            );

            var applySteerForceHandle = applySteerForce.Schedule(
                _instanceCount,
                jobBatchSetting.ApplySteerForceBatchCount,
                calculateBoidsSteerForceHandle);

            var spherecastHandle = SpherecastCommand.ScheduleBatch(
                _spherecastCommands,
                _raycastHits,
                jobBatchSetting.SpherecastCommandBatchCount,
                applySteerForceHandle);

            var avoidObstacleAndUpdateBoids = new AvoidObstaclesAndUpdateBoidsJob(
                _raycastHits,
                _aliveFlagDataArray,
                avoidObstacleSetting.AvoidRotateVelocity,
                Time.deltaTime,
                _boidsDataArray
            );

            var avoidObstacleAndUpdateBoidsJobHandle = avoidObstacleAndUpdateBoids.Schedule(
                _instanceCount,
                jobBatchSetting.AvoidObstacleAndUpdateBoidsJobBatchCount,
                spherecastHandle);
            
            var translateMatrix4x4Job = new TranslateMatrix4x4Job
            (
                _aliveFlagDataArray,
                _boidsDataArray,
                _boidsTransformMatrices,
                boidsSetting.InstanceScale
            );
            
            _jobHandle = translateMatrix4x4Job.Schedule(
                _instanceCount,
                jobBatchSetting.TranslateMatrix4x4BatchCount,
                avoidObstacleAndUpdateBoidsJobHandle);

            JobHandle.ScheduleBatchedJobs();
        }

        public void Complete()
        {
            _jobHandle.Complete();
        }

        public void UpdateBullets(IReadOnlyList<Bullet> bullets)
        {
            for (var i = 0; i < bullets.Count; i++)
            {
                _bulletDataArray[i] = bullets[i].GetData();
            }
        }
        
        public void UpdateSphereObstacles(IReadOnlyList<SphereObstacle> explosionObstacles)
        {
            for (var i = 0; i < explosionObstacles.Count; i++)
            {
                _sphereObstacleDataArray[i] = explosionObstacles[i].ObstacleData;
            }
        }

        public NativeArray<CollisionData> GetCollisionData()
        {
            // 衝突判定のJobが完了するまで待つ
            _collisionCheckJobHandle.Complete();

            return _collisionDataArray;
        }
    }
}