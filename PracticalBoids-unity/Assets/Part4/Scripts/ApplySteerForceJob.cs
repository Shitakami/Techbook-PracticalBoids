using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Part4
{
    [BurstCompile]
    public struct ApplySteerForceJob : IJobParallelFor
    {
        private NativeArray<BoidsData> _boidsDataArrayWrite;
        [ReadOnly] private readonly NativeArray<float3> _boidsForceRead;
        [WriteOnly] private NativeArray<SpherecastCommand> _spherecastCommandsWrite;
        [ReadOnly] private readonly NativeArray<SphereObstacleData> _sphereObstacleDataArrayRead;

        private readonly float3 _simulationAreaCenter;
        private readonly float3 _simulationAreaScaleHalf;
        private readonly float _returnSimulationAreaWeight;

        private readonly float _deltaTime;
        private readonly float _maxSpeed;
        private readonly float _spherecastRadius;
        private readonly float _spherecastDistance;
        private readonly float _escapeObstaclesWeight;
        private readonly float _escapeMaxSpeed;

        public ApplySteerForceJob(
            NativeArray<BoidsData> boidsDataArrayWrite,
            NativeArray<float3> boidsForceRead,
            NativeArray<SphereObstacleData> sphereObstacleDataArray,
            NativeArray<SpherecastCommand> spherecastCommandsWrite,
            float3 simulationAreaCenter,
            float3 simulationAreaScaleHalf,
            float returnSimulationAreaWeight,
            float deltaTime,
            float maxSpeed,
            float spherecastRadius,
            float spherecastDistance,
            float escapeObstaclesWeight,
            float escapeMaxSpeed
        )
        {
            _boidsDataArrayWrite = boidsDataArrayWrite;
            _boidsForceRead = boidsForceRead;
            _sphereObstacleDataArrayRead = sphereObstacleDataArray;
            _spherecastCommandsWrite = spherecastCommandsWrite;
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
            var boidsData = _boidsDataArrayWrite[ownIndex];
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
            
            if (escapeForce is not { x: 0, y: 0, z: 0 })
            {
                escapeForce *= _escapeObstaclesWeight;
                boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + escapeForce * _deltaTime, _escapeMaxSpeed);
            }
            else
            {
                // 原稿から修正：動的オブジェクトから逃げない場合、通常の最高速度に制限する
                // 逃げない場合にのみ制限しないと、次フレームで逃げる速度が消えるため
                boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + force * _deltaTime, _maxSpeed);
            }

            _boidsDataArrayWrite[ownIndex] = boidsData;
            
            _spherecastCommandsWrite[ownIndex] = new SpherecastCommand(
                origin: boidsData.Position, 
                radius: _spherecastRadius, 
                direction: math.normalize(boidsData.Velocity),
                queryParameters: QueryParameters.Default,
                distance: _spherecastDistance);
        }
    }
}
