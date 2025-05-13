using Common;
using Part4;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part5
{
    // MEMO: Part4のApplySteerForceJobをほぼコピペ
    // Execute()のメソッド最初に個体の生存状態をチェックしている
    [BurstCompile]
    public struct ApplySteerForceJob : IJobParallelFor
    {
        private NativeArray<BoidsData> _boidsDataWrite;
        [ReadOnly] private readonly NativeArray<float3> _boidsForceRead;
        [WriteOnly] private NativeArray<SpherecastCommand> _spherecastCommandsWrite;
        [ReadOnly] private readonly NativeArray<SphereObstacleData> _sphereObstacleDataArrayRead;
        [ReadOnly] private readonly NativeArray<bool> _aliveFlagDataArrayRead;

        private readonly float3 _simulationAreaCenter;
        private readonly float3 _simulationAreaScaleHalf;
        private readonly float _returnSimulationAreaWeight;

        private readonly float _deltaTime;
        private readonly float _maxSpeed;
        private readonly float _spherecastDistance;
        private readonly float _spherecastRadius;
        private readonly float _escapeObstaclesWeight;
        private readonly float _escapeMaxSpeed;

        public ApplySteerForceJob(
            NativeArray<BoidsData> boidsDataWrite,
            NativeArray<float3> boidsForceRead,
            NativeArray<SphereObstacleData> sphereObstacleDataArray,
            NativeArray<SpherecastCommand> spherecastCommandsWrite,
            NativeArray<bool> aliveFlagDataArrayRead,
            float3 simulationAreaCenter,
            float3 simulationAreaScaleHalf,
            float returnSimulationAreaWeight,
            float deltaTime,
            float maxSpeed,
            float spherecastDistance,
            float spherecastRadius,
            float escapeObstaclesWeight,
            float escapeMaxSpeed
        )
        {
            _boidsDataWrite = boidsDataWrite;
            _boidsForceRead = boidsForceRead;
            _sphereObstacleDataArrayRead = sphereObstacleDataArray;
            _spherecastCommandsWrite = spherecastCommandsWrite;
            _aliveFlagDataArrayRead = aliveFlagDataArrayRead;
            _simulationAreaCenter = simulationAreaCenter;
            _simulationAreaScaleHalf = simulationAreaScaleHalf;
            _returnSimulationAreaWeight = returnSimulationAreaWeight;
            _deltaTime = deltaTime;
            _maxSpeed = maxSpeed;
            _spherecastDistance = spherecastDistance;
            _spherecastRadius = spherecastRadius;
            _escapeObstaclesWeight = escapeObstaclesWeight;
            _escapeMaxSpeed = escapeMaxSpeed;
        }
        
        public void Execute(int ownIndex)
        {
            // MEMO: 個体の生存状態をチェック
            // 生存していない場合は何もしない
            if (!_aliveFlagDataArrayRead[ownIndex])
            {
                _boidsDataWrite[ownIndex] = new BoidsData();
                _spherecastCommandsWrite[ownIndex] = new SpherecastCommand();
                return;
            }
            
            var boidsData = _boidsDataWrite[ownIndex];
            var force = _boidsForceRead[ownIndex];

            force += MathematicsUtility.CalculateReturnAreaForce(boidsData.Position, _simulationAreaCenter, _simulationAreaScaleHalf) * _returnSimulationAreaWeight;

            var velocity = boidsData.Velocity + (force * _deltaTime);

            var escapeForce = float3.zero;
            
            foreach (var obstacleData in _sphereObstacleDataArrayRead)
            {
                var diff = boidsData.Position - obstacleData.Position; 
                var distanceSqr = math.lengthsq(diff);
                if (distanceSqr < obstacleData.RadiusSqr)
                {
                    escapeForce += diff / distanceSqr; // 距離の2乗に反比例する力を加える
                }
            }
            
            escapeForce *= _escapeObstaclesWeight;
            if (escapeForce is not { x: 0, y: 0, z: 0 })
            {
                boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + escapeForce * _deltaTime, _escapeMaxSpeed);
            }
            else
            {
                boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + force * _deltaTime, _maxSpeed);
            }

            _boidsDataWrite[ownIndex] = boidsData;
            
            _spherecastCommandsWrite[ownIndex] = new SpherecastCommand(
                origin: boidsData.Position, 
                radius: _spherecastRadius, 
                direction: math.normalize(boidsData.Velocity),
                queryParameters: QueryParameters.Default,
                distance: _spherecastDistance);
        }
    }
}