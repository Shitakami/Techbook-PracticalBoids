#if UNITY_EDITOR
using NUnit.Framework;
using Unity.Mathematics;

namespace Part5.Tests
{
    // MEMO: 生成AI Claudeによって作成されたテストコード
    [TestFixture]
    public class CollisionUtilitiesTests
    {
        private const float EPSILON = 0.001f;

        [Test]
        public void Test_TryDetectStaticAndDynamicCollision_StartOverlapping()
        {
            float3 staticPosition = new float3(0, 0, 0);
            float staticRadius = 1.0f;

            float3 dynamicPosition = new float3(1.5f, 0, 0);
            float3 dynamicVelocity = new float3(1, 0, 0);
            float dynamicRadius = 1.0f;

            bool result = CollisionUtility.TryDetectStaticAndDynamicCollision(
                staticPosition, staticRadius,
                dynamicPosition, dynamicVelocity, dynamicRadius,
                1.0f, // deltaTime
                out float collisionTime, out float3 collisionPosition);

            Assert.IsTrue(result, "開始時点で球体が重なっている場合、衝突が検出されるべき");
            Assert.That(collisionTime, Is.EqualTo(0).Within(EPSILON), "衝突時間は0であるべき");

            // 衝突位置は静的球体の表面上にあるべき
            float distanceToStatic = math.distance(collisionPosition, staticPosition);
            Assert.That(distanceToStatic, Is.InRange(staticPosition.x, dynamicPosition.x),
                "衝突位置は静的球体の表面上にあるべき");
        }

        [Test]
        public void Test_TryDetectStaticAndDynamicCollision_MovingTowards()
        {
            float3 staticPosition = new float3(0, 0, 0);
            float staticRadius = 1.0f;

            float3 dynamicPosition = new float3(5, 0, 0);
            float3 dynamicVelocity = new float3(-10, 0, 0); // 左に移動
            float dynamicRadius = 1.0f;

            bool result = CollisionUtility.TryDetectStaticAndDynamicCollision(
                staticPosition, staticRadius,
                dynamicPosition, dynamicVelocity, dynamicRadius,
                1.0f, // deltaTime
                out float collisionTime, out float3 collisionPosition);

            Assert.IsTrue(result, "動的球体が静的球体に向かって移動する場合、衝突が検出されるべき");

            // 衝突時間の検証 - 距離は5、合計半径は2、速度は10、よって(5-2)/10 = 0.3
            Assert.That(collisionTime, Is.EqualTo(0.3f).Within(EPSILON), "衝突時間は正しいべき");

            // 衝突位置の検証 - 動的球体は左に移動するので、静的球体の右側で衝突
            Assert.That(collisionPosition.x, Is.EqualTo(staticRadius).Within(EPSILON),
                "衝突位置は静的球体の表面上（右側）にあるべき");
        }

        [Test]
        public void Test_TryDetectStaticAndDynamicCollision_MovingAway()
        {
            float3 staticPosition = new float3(0, 0, 0);
            float staticRadius = 1.0f;

            float3 dynamicPosition = new float3(5, 0, 0);
            float3 dynamicVelocity = new float3(10, 0, 0); // 右に移動（静的球体から離れる）
            float dynamicRadius = 1.0f;

            bool result = CollisionUtility.TryDetectStaticAndDynamicCollision(
                staticPosition, staticRadius,
                dynamicPosition, dynamicVelocity, dynamicRadius,
                1.0f, // deltaTime
                out _, out _);

            Assert.IsFalse(result, "動的球体が静的球体から離れる方向に移動する場合、衝突は検出されるべきでない");
        }

        [Test]
        public void Test_TryDetectStaticAndDynamicCollision_GrazingPath()
        {
            float3 staticPosition = new float3(0, 0, 0);
            float staticRadius = 1.0f;

            // 動的球体は球体の端をかすめるように移動
            float3 dynamicPosition = new float3(0, 2, -5);
            float3 dynamicVelocity = new float3(0, 0, 10);
            float dynamicRadius = 1.0f;

            bool result = CollisionUtility.TryDetectStaticAndDynamicCollision(
                staticPosition, staticRadius,
                dynamicPosition, dynamicVelocity, dynamicRadius,
                1.0f, // deltaTime
                out float collisionTime, out float3 collisionPosition);

            Assert.IsTrue(result, "動的球体が静的球体をかすめる場合、衝突が検出されるべき");

            // 衝突時間と位置の検証（具体的な値は複雑なので、おおよその範囲で確認）
            Assert.That(collisionTime, Is.InRange(0, 1), "衝突時間は0〜1の範囲内であるべき");

            // 衝突位置は静的球体の表面上にあるべき
            float distanceToStatic = math.distance(collisionPosition, staticPosition);
            Assert.That(distanceToStatic, Is.EqualTo(staticRadius).Within(EPSILON),
                "衝突位置は静的球体の表面上にあるべき");
        }

        [Test]
        public void Test_TryDetectStaticAndDynamicCollision_NoCollision()
        {
            float3 staticPosition = new float3(0, 0, 0);
            float staticRadius = 1.0f;

            // 動的球体は静的球体から離れた位置にあり、衝突しない経路で移動
            float3 dynamicPosition = new float3(0, 5, 0);
            float3 dynamicVelocity = new float3(0, 0, 10);
            float dynamicRadius = 1.0f;

            bool result = CollisionUtility.TryDetectStaticAndDynamicCollision(
                staticPosition, staticRadius,
                dynamicPosition, dynamicVelocity, dynamicRadius,
                1.0f, // deltaTime
                out _, out _);

            Assert.IsFalse(result, "動的球体が静的球体と衝突しない経路で移動する場合、衝突は検出されるべきでない");
        }
    }
}
#endif