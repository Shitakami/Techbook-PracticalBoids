using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part3
{
    [BurstCompile]
    public struct CalculateBoidsSteerForceJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDataArrayRead;
        [WriteOnly] private NativeArray<float3> _boidsSteerWrite;
        [ReadOnly] private NativeParallelMultiHashMap<int3, int> _grid;
        
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

        private readonly float _cellScale;

        public CalculateBoidsSteerForceJob(
            NativeArray<BoidsData> boidsDataArrayRead,
            NativeArray<float3> boidsSteerWrite,
            NativeParallelMultiHashMap<int3, int> grid,
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
            float maxForceSteer,
            float cellScale
        )
        {
            _boidsDataArrayRead = boidsDataArrayRead;
            _boidsSteerWrite = boidsSteerWrite;
            _grid = grid;
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
            _cellScale = cellScale;
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

            var cellIndex = MathematicsUtility.CalculateCellIndex(ownPosition, _cellScale);

            var minX = cellIndex.x - 1;
            var minY = cellIndex.y - 1;
            var minZ = cellIndex.z - 1;

            var maxX = cellIndex.x + 1;
            var maxY = cellIndex.y + 1;
            var maxZ = cellIndex.z + 1;

            for (int x = minX; x <= maxX; ++x)
            for (int y = minY; y <= maxY; ++y)
            for (int z = minZ; z <= maxZ; ++z)
            {
                var key = new int3(x, y, z);

                for (var success = _grid.TryGetFirstValue(key, out var targetIndex, out var iterator);
                     success;
                     success = _grid.TryGetNextValue(out targetIndex, ref iterator))
                {
                    var targetPosition = _boidsDataArrayRead[targetIndex].Position;
                    var targetVelocity = _boidsDataArrayRead[targetIndex].Velocity;

                    var toTarget = targetPosition - ownPosition;
                    if (toTarget is { x:0, y:0, z:0 })
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

            _boidsSteerWrite[ownIndex] =
                cohesionSteer * _cohesionWeight +
                separationSteer * _separationWeight +
                alignmentSteer * _alignmentWeight;
        }
    }
}