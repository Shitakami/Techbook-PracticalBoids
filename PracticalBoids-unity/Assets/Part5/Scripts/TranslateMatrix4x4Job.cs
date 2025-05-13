using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part5
{
    // MEMO: Part2のTranslateMatrix4x4Jobをほぼコピペ
    // Execute()のメソッド最初に個体の生存状態をチェックしている
    [BurstCompile]
    public struct TranslateMatrix4x4Job : IJobParallelFor 
    {
        [ReadOnly] private readonly NativeArray<bool> _aliveFlagDataArray;
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDataArray;
        [WriteOnly] private NativeArray<Matrix4x4> _boidsTransformMatrixArray;
        private readonly float3 _instanceScale;

        public TranslateMatrix4x4Job(
            NativeArray<bool> aliveFlagDataArray,
            NativeArray<BoidsData> boidsDataArray,
            NativeArray<Matrix4x4> boidsTransformMatrixArray,
            float3 instanceScale
        )
        {
            _aliveFlagDataArray = aliveFlagDataArray;
            this._boidsDataArray = boidsDataArray;
            this._boidsTransformMatrixArray = boidsTransformMatrixArray;
            _instanceScale = instanceScale;
        }
        
        public void Execute(int ownIndex)
        {
            // MEMO: 生存状態をチェック
            // 個体が生存していない場合は、Matrix4x4.zero（スケール0）にして描画させない
            if (!_aliveFlagDataArray[ownIndex])
            {
                _boidsTransformMatrixArray[ownIndex] = Matrix4x4.zero;
                return;
            }
            
            var boidsData = _boidsDataArray[ownIndex];
            var position = boidsData.Position;
            var rotation = quaternion.LookRotation(boidsData.Velocity, new float3(0, 1, 0));
            
            _boidsTransformMatrixArray[ownIndex] = float4x4.TRS(position, rotation, _instanceScale);
        }
    }
}