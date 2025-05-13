using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part2
{
    [BurstCompile]
    public struct ApplySteerForceJob : IJobParallelFor
    {
        private NativeArray<BoidsData> _boidsDataArray;
        [ReadOnly] private readonly NativeArray<float3> _boidsForceRead;

        private readonly float3 _simulationAreaCenter;
        private readonly float3 _simulationAreaScaleHalf;
        private readonly float _returnSimulationAreaWeight;

        private readonly float _deltaTime;
        private readonly float _maxSpeed;

        public ApplySteerForceJob(
            NativeArray<BoidsData> boidsDataArray,
            NativeArray<float3> boidsForceRead,
            float3 simulationAreaCenter,
            float3 simulationAreaScaleHalf,
            float returnSimulationAreaWeight,
            float deltaTime,
            float maxSpeed
        )
        {
            _boidsDataArray = boidsDataArray;
            _boidsForceRead = boidsForceRead;
            _simulationAreaCenter = simulationAreaCenter;
            _simulationAreaScaleHalf = simulationAreaScaleHalf;
            _returnSimulationAreaWeight = returnSimulationAreaWeight;
            _deltaTime = deltaTime;
            _maxSpeed = maxSpeed;
        }

        public void Execute(int ownIndex)
        {
            var boidsData = _boidsDataArray[ownIndex];
            var force = _boidsForceRead[ownIndex];

            var returnForce = MathematicsUtility.CalculateReturnAreaForce(
                boidsData.Position, 
                _simulationAreaCenter,
                _simulationAreaScaleHalf);
            force += returnForce * _returnSimulationAreaWeight;
            
            var velocity = boidsData.Velocity + (force * _deltaTime);
            boidsData.Velocity = MathematicsUtility.Limit(velocity, _maxSpeed);
            boidsData.Position += velocity * _deltaTime;

            _boidsDataArray[ownIndex] = boidsData;
        }
    }
}