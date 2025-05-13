using Common;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part2
{
    public class BoidsSystemCore
    {
        private readonly int _instanceCount;
        private NativeArray<BoidsData> _boidsDataArray;
        private NativeArray<float3> _boidsSteerForceArray;
        private NativeArray<Matrix4x4> _boidsTransformMatrixArray;
        
        public NativeArray<Matrix4x4> BoidsTransformMatrixArray => _boidsTransformMatrixArray;

        private JobHandle _jobHandle;
        
        public BoidsSystemCore(int instanceCount)
        {
            _instanceCount = instanceCount;

            _boidsDataArray = new NativeArray<BoidsData>(instanceCount, Allocator.Domain);
            _boidsSteerForceArray = new NativeArray<float3>(instanceCount, Allocator.Domain);
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
            BoidsBehaviour.JobBatchSetting jobBatchSetting, 
            float3 simulationAreaCenter,
            float3 simulationAreaScale)
        {
            var calculateBoidsSteerForceJob = new CalculateBoidsSteerForceJob
            (
                _boidsDataArray,
                _boidsSteerForceArray,
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
                boidsSetting.MaxSteerForce
            );

            var calculateSteerForceJobHandle = calculateBoidsSteerForceJob.Schedule(
                _instanceCount,
                jobBatchSetting.CalculateSteerForceBatchCount);

            var applySteerForceJob = new ApplySteerForceJob
            (
                _boidsDataArray,
                _boidsSteerForceArray,
                simulationAreaCenter,
                simulationAreaScale/2,
                boidsSetting.ReturnSimulationAreaWeight,
                Time.deltaTime,
                boidsSetting.MaxSpeed
            );

            var applySteerForceJobHandle = applySteerForceJob.Schedule(
                _instanceCount,
                jobBatchSetting.ApplySteerForceBatchCount,
                calculateSteerForceJobHandle);
            
            var translateMatrix4x4Job = new TranslateMatrix4x4Job
            (
                _boidsDataArray,
                _boidsTransformMatrixArray,
                boidsSetting.InstanceScale
            );

            var translateMatrix4x4JobHandle = translateMatrix4x4Job.Schedule(
                _instanceCount,
                jobBatchSetting.TranslateMatrix4x4BatchCount,
                applySteerForceJobHandle);

            translateMatrix4x4JobHandle.Complete();
        }
    }
}