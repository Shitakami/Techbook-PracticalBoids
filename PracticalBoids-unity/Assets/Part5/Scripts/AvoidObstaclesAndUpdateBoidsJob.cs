using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part5
{
    // MEMO: Part4のApplySteerForceJobをほぼコピペ
    // Execute()のメソッド最初に個体の生存状態をチェックしている
    [BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
    public struct AvoidObstaclesAndUpdateBoidsJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<RaycastHit> _raycastHitsRead;
        [ReadOnly] private readonly NativeArray<bool> _aliveFlagDatasRead;
        [ReadOnly] private readonly float _avoidRotationVelocity;
        [ReadOnly] private readonly float _deltaTime;
        private NativeArray<BoidsData> _boidsDatasWrite;

        public AvoidObstaclesAndUpdateBoidsJob(
            NativeArray<RaycastHit> raycastHitsRead,
            NativeArray<bool> aliveFlagDatasRead,
            float avoidRotationVelocity,
            float deltaTime,
            NativeArray<BoidsData> boidsDatasWrite
        )
        {
            _raycastHitsRead = raycastHitsRead;
            _aliveFlagDatasRead = aliveFlagDatasRead;
            _avoidRotationVelocity = avoidRotationVelocity;
            _deltaTime = deltaTime;
            _boidsDatasWrite = boidsDatasWrite;
        }

        public void Execute(int ownIndex)
        {
            // MEMO: 個体が生存していない場合は何もしない
            if (!_aliveFlagDatasRead[ownIndex])
            {
                return;
            }

            var boidsData = _boidsDatasWrite[ownIndex];
            var velocity = boidsData.Velocity;
            var raycastHit = _raycastHitsRead[ownIndex];

            if (raycastHit.IsHit())
            {
                var forward = math.normalize(velocity);
                var axis = math.cross(forward, raycastHit.normal);
                if (axis is { x: 0, y: 0, z: 0 })
                {
                    axis = new float3(0, 1, 0); // MEMO: 回転軸がない場合はY軸を回転軸とする
                }

                var avoidObstacleRotation = quaternion.AxisAngle(math.normalize(axis), _avoidRotationVelocity * _deltaTime);
                velocity = math.mul(avoidObstacleRotation, velocity);
            }

            boidsData.Velocity = velocity;
            boidsData.Position += velocity * _deltaTime;

            _boidsDatasWrite[ownIndex] = boidsData;
        }
    }
}