using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part5
{
    [BurstCompile]
    public struct CollisionDetectionJob : IJobParallelFor
    {
        [ReadOnly] private readonly NativeArray<BulletData> _bulletDatasRead;
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDatasRead;
        [ReadOnly] private NativeParallelMultiHashMap<int3, int> _gridHashMap;
        private readonly float _cellScale;
        private readonly float _boidsRadius;
        private readonly float _deltaTime;

        // 書き込み用配列はAtomicでアクセスすることを検討
        [WriteOnly] private NativeArray<bool> _aliveFlagDatasWrite;
        [WriteOnly] private NativeArray<CollisionData> _collisionPositionDatasWrite;

        private const int InvalidBoidsIndex = -1;
        
        public CollisionDetectionJob(
            NativeArray<BulletData> bulletDatasRead,
            NativeArray<BoidsData> boidsDatasRead,
            NativeParallelMultiHashMap<int3, int> gridHashMap,
            float cellScale,
            float boidsRadius,
            float deltaTime,
            NativeArray<bool> aliveFlagDatasWrite,
            NativeArray<CollisionData> collisionPositionDatasWrite
        )
        {
            _bulletDatasRead = bulletDatasRead;
            _boidsDatasRead = boidsDatasRead;
            _gridHashMap = gridHashMap;
            _cellScale = cellScale;
            _boidsRadius = boidsRadius;
            _deltaTime = deltaTime;
            _aliveFlagDatasWrite = aliveFlagDatasWrite;
            _collisionPositionDatasWrite = collisionPositionDatasWrite;
        }

        public void Execute(int bulletIndex)
        {
            var bulletData = _bulletDatasRead[bulletIndex];

            // MEMO: 弾が存在しない場合は何もしない
            if (!bulletData.IsActive())
            {
                _collisionPositionDatasWrite[bulletIndex] = new CollisionData
                {
                    Position = float3.zero,
                    IsCollided = false
                };
                return;
            }

            var radiusSum = _boidsRadius + bulletData.Radius;
            
            var bulletVelocity = bulletData.Velocity;
            var bulletPosition = bulletData.Position;
            
            // バウンディングボックスの計算 - 弾の移動経路を囲む最小の箱
            var endPosition = bulletPosition + bulletVelocity * _deltaTime;
            var minBound = math.min(bulletPosition, endPosition) - new float3(radiusSum);
            var maxBound = math.max(bulletPosition, endPosition) + new float3(radiusSum);
            
            // セルインデックスの計算
            var minCellIndex = MathematicsUtility.CalculateCellIndex(minBound, _cellScale);
            var maxCellIndex = MathematicsUtility.CalculateCellIndex(maxBound, _cellScale);
            
            var minCollisionTime = _deltaTime;
            var minCollisionPosition = float3.zero;
            var minCollisionBoidsIndex = InvalidBoidsIndex;

            // グリッドの各セルを走査
            for (var x = minCellIndex.x; x <= maxCellIndex.x; x++)
            for (var y = minCellIndex.y; y <= maxCellIndex.y; y++)
            for (var z = minCellIndex.z; z <= maxCellIndex.z; z++)
            {
                var cellIndex = new int3(x, y, z);
                
                // 現在のグリッドセル内の各Boidsを検査
                for (var success = _gridHashMap.TryGetFirstValue(cellIndex, out var boidsIndex, out var iterator);
                     success;
                     success = _gridHashMap.TryGetNextValue(out boidsIndex, ref iterator))
                {
                    var boidsPosition = _boidsDatasRead[boidsIndex].Position;
                    
                    // 衝突チェック
                    var hasCollision = CollisionUtility.TryDetectStaticAndDynamicCollision(
                        boidsPosition, _boidsRadius,
                        bulletPosition, bulletVelocity, bulletData.Radius,
                        _deltaTime,
                        out var collisionTime, out var collisionPosition
                    );

                    if (hasCollision && collisionTime < minCollisionTime)
                    {
                        minCollisionTime = collisionTime;
                        minCollisionPosition = collisionPosition;
                        minCollisionBoidsIndex = boidsIndex;
                    }
                }
            }

            // 衝突がない場合は何もしない
            if (minCollisionBoidsIndex == InvalidBoidsIndex)
            {
                _collisionPositionDatasWrite[bulletIndex] = new CollisionData
                {
                    Position = float3.zero,
                    IsCollided = false
                };
                return;
            }

            unsafe
            {
                var aliveFlagDataWritePtr = (bool*)_aliveFlagDatasWrite.GetUnsafePtr();
                aliveFlagDataWritePtr[minCollisionBoidsIndex] = false;
            }

            _collisionPositionDatasWrite[bulletIndex] = new CollisionData
            {
                Position = minCollisionPosition,
                IsCollided = true
            };
        }
    }
}