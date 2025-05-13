using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Common
{
    [BurstCompile]
    public struct TranslateMatrix4x4Job : IJobParallelFor
    {
        [ReadOnly] private NativeArray<BoidsData> _boidsDataArray;
        [WriteOnly] private NativeArray<Matrix4x4> _boidsTransformMatrixArray;
        private readonly float3 _instanceScale;

        public TranslateMatrix4x4Job(
            NativeArray<BoidsData> boidsDataArray,
            NativeArray<Matrix4x4> boidsTransformMatrixArray,
            float3 instanceScale
        )
        {
            _boidsDataArray = boidsDataArray;
            _boidsTransformMatrixArray = boidsTransformMatrixArray;
            _instanceScale = instanceScale;
        }
        
        public void Execute(int ownIndex)
        {
            var boidsData = _boidsDataArray[ownIndex];
            var position = boidsData.Position;
            var rotation = quaternion.LookRotation(boidsData.Velocity, new float3(0, 1, 0));
            
            _boidsTransformMatrixArray[ownIndex] = float4x4.TRS(position, rotation, _instanceScale);
        }
    }
}