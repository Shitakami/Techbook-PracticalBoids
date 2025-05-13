using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part2
{
    [BurstCompile]
    public struct CalculateBoidsSteerForceJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDataArrayRead;
        [WriteOnly] private NativeArray<float3> _boidsSteerForceArrayWrite;
        
        private readonly float _cohesionWeight;
        private readonly float _cohesionAffectedRadius;
        private readonly float _cohesionViewDot;
        private readonly float _separationWeight;
        private readonly float _separationAffectedRadius;
        private readonly float _separationViewDot;
        private readonly float _alignmentWeight;
        private readonly float _alignmentAffectedRadius;
        private readonly float _alignmentViewDot;

        private readonly float _maxSpeed;
        private readonly float _maxForceSteer;

        public CalculateBoidsSteerForceJob(
            NativeArray<BoidsData> boidsDataArrayRead,
            NativeArray<float3> boidsSteerForceArrayWrite,
            float cohesionWeight,
            float cohesionAffectedRadius,
            float cohesionViewDot,
            float separationWeight,
            float separationAffectedRadius,
            float separationViewDot,
            float alignmentWeight,
            float alignmentAffectedRadius,
            float alignmentViewDot,
            float maxSpeed,
            float maxForceSteer
        )
        {
            _boidsDataArrayRead = boidsDataArrayRead;
            _boidsSteerForceArrayWrite = boidsSteerForceArrayWrite;
            _cohesionWeight = cohesionWeight;
            _cohesionAffectedRadius = cohesionAffectedRadius;
            _cohesionViewDot = cohesionViewDot;
            _separationWeight = separationWeight;
            _separationAffectedRadius = separationAffectedRadius;
            _separationViewDot = separationViewDot;
            _alignmentWeight = alignmentWeight;
            _alignmentAffectedRadius = alignmentAffectedRadius;
            _alignmentViewDot = alignmentViewDot;
            _maxSpeed = maxSpeed;
            _maxForceSteer = maxForceSteer;
        }

        public void Execute(int ownIndex)
        {
            var ownPosition = _boidsDataArrayRead[ownIndex].Position;
            var ownVelocity = _boidsDataArrayRead[ownIndex].Velocity;
            var ownForward = math.normalize(ownVelocity);
            
            var cohesionPositionSum = new float3();
            var cohesionTargetCount = 0;

            var separationRepulseSum = new float3();
            var separationTargetCount = 0;

            var alignmentVelocitySum = new float3();
            var alignmentTargetCount = 0;

            for (var targetIndex = 0; targetIndex < _boidsDataArrayRead.Length; ++targetIndex)
            {
                var targetPosition = _boidsDataArrayRead[targetIndex].Position;
                var targetVelocity = _boidsDataArrayRead[targetIndex].Velocity;

                var toTarget = targetPosition - ownPosition;
                if (toTarget is { x: 0, y: 0, z: 0 })
                {
                    continue; // 自身と同じ位置の個体は無視
                }

                var distance = math.length(toTarget);
                var toTargetDirection = toTarget / distance;
                var dot = math.dot(ownForward, toTargetDirection);

                if (distance <= _cohesionAffectedRadius && dot >= _cohesionViewDot)
                {
                    cohesionPositionSum += targetPosition;
                    cohesionTargetCount++;
                }

                if (distance <= _separationAffectedRadius && dot >= _separationViewDot)
                {
                    separationRepulseSum += -toTargetDirection / distance; // 距離に反比例する相手から自分への力
                    separationTargetCount++;
                }

                if (distance <= _alignmentAffectedRadius && dot >= _alignmentViewDot)
                {
                    alignmentVelocitySum += targetVelocity;
                    alignmentTargetCount++;
                }
            }

            var cohesionSteer = new float3();
            if (cohesionTargetCount > 0)
            {
                var cohesionPositionAverage = cohesionPositionSum / cohesionTargetCount;
                var cohesionDirection = cohesionPositionAverage - ownPosition;
                var cohesionVelocity = math.normalize(cohesionDirection) * _maxSpeed;
                cohesionSteer = MathematicsUtility.Limit(cohesionVelocity - ownVelocity, _maxForceSteer);
            }

            var separationSteer = new float3();
            if (separationTargetCount > 0)
            {
                var separationVelocity = math.normalize(separationRepulseSum) * _maxSpeed;
                separationSteer = MathematicsUtility.Limit(separationVelocity - ownVelocity, _maxForceSteer);
            }

            var alignmentSteer = new float3();
            if (alignmentTargetCount > 0)
            {
                var alignmentVelocity = math.normalize(alignmentVelocitySum) * _maxSpeed;
                alignmentSteer = MathematicsUtility.Limit(alignmentVelocity - ownVelocity, _maxForceSteer);
            }

            _boidsSteerForceArrayWrite[ownIndex] =
                cohesionSteer * _cohesionWeight +
                separationSteer * _separationWeight +
                alignmentSteer * _alignmentWeight;
        }
    }
}