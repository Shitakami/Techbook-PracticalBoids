using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part1
{
    [BurstCompile]
    public struct CubeAnimationJob : IJobParallelFor
    {
        private readonly float _instancedHeight;
        private readonly float _destroyHeight;
        private readonly float _moveVelocity;
        private readonly float _rotationVelocity;
        private readonly float _deltaTime;
        private NativeArray<Matrix4x4> _transformMatrixArray;

        public CubeAnimationJob(
            float instancedHeight,
            float destroyHeight,
            float moveVelocity,
            float rotationVelocity,
            float deltaTime,
            NativeArray<Matrix4x4> transformMatrixArray
        )
        {
            _instancedHeight = instancedHeight;
            _destroyHeight = destroyHeight;
            _moveVelocity = moveVelocity;
            _rotationVelocity = rotationVelocity;
            _deltaTime = deltaTime;
            _transformMatrixArray = transformMatrixArray;
        }

        public void Execute(int index)
        {
            var position = _transformMatrixArray[index].GetPosition();
            var rotation = _transformMatrixArray[index].rotation;
            var scale = _transformMatrixArray[index].lossyScale;

            // 乱数を使って回転軸を設定
            var random = new Unity.Mathematics.Random((uint)index + 1);
            var rotateAxis = random.NextFloat3Direction();

            // Cubeの移動と回転
            position.y -= _moveVelocity * _deltaTime;
            rotation *= quaternion.AxisAngle(rotateAxis, _rotationVelocity * _deltaTime);

            // Cubeの位置が一定の高さを下回ったら再配置
            if (position.y < _destroyHeight)
            {
                position.y = _instancedHeight;
            }

            _transformMatrixArray[index] = Matrix4x4.TRS(position, rotation, scale);
        }
    }
}