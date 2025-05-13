using Unity.Mathematics;

namespace Part5
{
    public static class CollisionUtility
    {
        public static bool TryDetectStaticAndDynamicCollision(
            float3 staticPosition, float staticRadius,
            float3 dynamicPosition, float3 dynamicVelocity, float dynamicRadius,
            float deltaTime,
            out float collisionTime, out float3 collisionPosition
        )
        {
            collisionPosition = float3.zero;
            collisionTime = 0f;

            var radiusSum = staticRadius + dynamicRadius;
            var radiusSumSquared = radiusSum * radiusSum;

            // 現在の時点で既に衝突しているかを早期判定
            var relativePosition = dynamicPosition - staticPosition;
            var distanceSq = math.lengthsq(relativePosition);
            if (distanceSq <= radiusSumSquared)
            {
                collisionPosition = staticPosition + relativePosition * staticRadius / radiusSum;
                return true;
            }

            // レイキャストのような計算で衝突を検出
            // 二次方程式の係数
            var a = math.lengthsq(dynamicVelocity);
            var b = 2.0f * math.dot(dynamicVelocity, relativePosition);
            var c = distanceSq - radiusSumSquared;
            
            // 判別式
            var discriminant = b * b - 4.0f * a * c;
            
            // 解なし
            if (discriminant < 0)
            {
                return false;
            }

            // 最初の衝突時間を計算
            var discriminantSqrt = math.sqrt(discriminant);
            var t1 = (-b - discriminantSqrt) / (2.0f * a);
            
            // 時間範囲内に衝突があるか確認
            if (0 <= t1 && t1 <= deltaTime)
            {
                collisionTime = t1;
                
                // 衝突位置を計算
                var dynamicPositionAtCollision = dynamicPosition + dynamicVelocity * t1;
                var toStatic = math.normalize(staticPosition - dynamicPositionAtCollision);
                
                // 衝突点は、静的球体の表面上にある
                collisionPosition = staticPosition - toStatic * staticRadius;
                
                return true;
            }

            return false;
        }
    }
}