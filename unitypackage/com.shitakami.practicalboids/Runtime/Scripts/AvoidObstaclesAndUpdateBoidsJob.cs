using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Shitakami.PracticalBoids
{
    [BurstCompile]
    public struct AvoidObstaclesAndUpdateBoidsJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<RaycastHit> _raycastHitsRead;
        private NativeArray<BoidsData> _boidsDataArrayWrite;
        
        private readonly float _avoidRotationVelocity;
        private readonly float _deltaTime;

        public AvoidObstaclesAndUpdateBoidsJob(
            NativeArray<RaycastHit> raycastHitsRead,
            NativeArray<BoidsData> boidsDataArrayWrite,
            float avoidRotationVelocity,
            float deltaTime
        )
        {
            _raycastHitsRead = raycastHitsRead;
            _boidsDataArrayWrite = boidsDataArrayWrite;
            _avoidRotationVelocity = avoidRotationVelocity;
            _deltaTime = deltaTime;
        }

        public void Execute(int ownIndex)
        {
            var boidsData = _boidsDataArrayWrite[ownIndex];
            var velocity = boidsData.Velocity;
            var raycastHit = _raycastHitsRead[ownIndex];
            
            if (raycastHit.IsHit())
            {
                var forward = math.normalize(velocity);
                var axis = math.cross(forward, raycastHit.normal);
                if (axis is { x: 0, y: 0, z: 0 })
                {
                    axis = new float3(0, 1, 0);
                }

                var avoidRotation = quaternion.AxisAngle(
                    math.normalize(axis), 
                    _avoidRotationVelocity * _deltaTime);
                velocity = math.mul(avoidRotation, velocity);
            }
            
            boidsData.Velocity = velocity;
            boidsData.Position += velocity * _deltaTime;

            _boidsDataArrayWrite[ownIndex] = boidsData;
        }
    }
}
