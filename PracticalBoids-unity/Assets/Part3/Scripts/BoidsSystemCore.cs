using Common;
using Part2;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part3
{
    public class BoidsSystemCore
    {
        private readonly int _instanceCount;
        private NativeArray<BoidsData> _boidsDataArray;
        private NativeParallelMultiHashMap<int3, int> _gridHashMap;
        private NativeArray<float3> _boidsSteerArray;
        private NativeArray<Matrix4x4> _boidsTransformMatrixArray;
        
        public NativeArray<Matrix4x4> BoidsTransformMatrixArray => _boidsTransformMatrixArray;

        private JobHandle _jobHandle;
        
        public BoidsSystemCore(int instanceCount)
        {
            _instanceCount = instanceCount;
            
            _boidsDataArray = new NativeArray<BoidsData>(instanceCount, Allocator.Domain);
            _gridHashMap = new NativeParallelMultiHashMap<int3, int>(instanceCount, Allocator.Domain);
            _boidsSteerArray = new NativeArray<float3>(instanceCount, Allocator.Domain);
            _boidsTransformMatrixArray = new NativeArray<Matrix4x4>(instanceCount, Allocator.Domain);
        }
        
        public void Setup(
            BoidsSetting boidsSetting,
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
            }
        }

        public void ExecuteUpdate(
            BoidsSetting boidsSetting,
            float3 simulationAreaCenter,
            float3 simulationAreaScale,
            BoidsBehaviour.JobBatchSetting jobBatchSetting)
        {
            _gridHashMap.Clear();
            var cellScale = boidsSetting.MaxAffectedRadius;
            
            var registerInstanceToGridJob = new RegisterBoidsIndexToGridJob
            (
                _gridHashMap.AsParallelWriter(),
                _boidsDataArray,
                cellScale
            );

            var registerInstanceToGridHandle = registerInstanceToGridJob.Schedule(
                _instanceCount,
                jobBatchSetting.RegisterInstanceToGridBatchCount
            );

            var boidsJob = new CalculateBoidsSteerForceJob
            (
                _boidsDataArray,
                _boidsSteerArray,
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

            var calculateBoidsSteerForceJob = boidsJob.Schedule(
                _instanceCount,
                jobBatchSetting.CalculateSteerForceBatchCount,
                registerInstanceToGridHandle
            );

            var applySteerForceJob = new ApplySteerForceJob
            (
                _boidsDataArray,
                _boidsSteerArray,
                simulationAreaCenter,
                simulationAreaScale/2,
                boidsSetting.ReturnSimulationAreaWeight,
                Time.deltaTime,
                boidsSetting.MaxSpeed
            );

            var applySteerForceJobHandle = applySteerForceJob.Schedule(
                _instanceCount,
                jobBatchSetting.ApplySteerForceBatchCount,
                calculateBoidsSteerForceJob
            );
            
            var translateMatrix4x4Job = new TranslateMatrix4x4Job
            (
                _boidsDataArray,
                _boidsTransformMatrixArray,
                boidsSetting.InstanceScale
            );

            _jobHandle = translateMatrix4x4Job.Schedule(
                _instanceCount,
                jobBatchSetting.TranslateMatrix4x4BatchCount,
                applySteerForceJobHandle
            );
            
            JobHandle.ScheduleBatchedJobs();
        }

        public void Complete()
        {
            _jobHandle.Complete();
        }
    }
}