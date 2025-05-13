#if UNITY_EDITOR
using NUnit.Framework;
using Unity.Mathematics;

namespace Common.Tests
{
    [TestFixture]
    public class MathematicsUtilityTests
    {
        [Test]
        public void Test_CalculateCellIndex_PositiveCoordinates()
        {
            float3 position = new float3(2.5f, 3.7f, 4.1f);
            float gridScale = 1.0f;
            
            int3 result = MathematicsUtility.CalculateCellIndex(position, gridScale);
            
            Assert.AreEqual(new int3(2, 3, 4), result);
        }
        
        [Test]
        public void Test_CalculateCellIndex_NegativeCoordinates()
        {
            float3 position = new float3(-2.5f, -3.7f, -4.1f);
            float gridScale = 1.0f;
            
            int3 result = MathematicsUtility.CalculateCellIndex(position, gridScale);
            
            Assert.AreEqual(new int3(-3, -4, -5), result);
        }
        
        [Test]
        public void Test_CalculateCellIndex_ZeroCoordinates()
        {
            float3 position = new float3(0f, 0f, 0f);
            float gridScale = 1.0f;
            
            int3 result = MathematicsUtility.CalculateCellIndex(position, gridScale);
            
            Assert.AreEqual(new int3(0, 0, 0), result);
        }
        
        [Test]
        public void Test_CalculateCellIndex_DifferentGridScales()
        {
            float3 position = new float3(5.2f, -3.7f, 8.1f);
            
            // 異なるグリッドスケールでテスト
            Assert.AreEqual(new int3(5, -4, 8), MathematicsUtility.CalculateCellIndex(position, 1.0f));
            Assert.AreEqual(new int3(2, -2, 4), MathematicsUtility.CalculateCellIndex(position, 2.0f));
            Assert.AreEqual(new int3(1, -1, 2), MathematicsUtility.CalculateCellIndex(position, 4.0f));
        }
        
        [Test]
        public void Test_CalculateCellIndex_GridBoundaries()
        {
            float gridScale = 1.0f;
            
            // グリッド境界上の値のテスト
            Assert.AreEqual(new int3(1, 1, 1), MathematicsUtility.CalculateCellIndex(new float3(1f, 1f, 1f), gridScale));
            Assert.AreEqual(new int3(0, 0, 0), MathematicsUtility.CalculateCellIndex(new float3(0.999f, 0.999f, 0.999f), gridScale));
            
            // 負の境界値
            Assert.AreEqual(new int3(-1, -1, -1), MathematicsUtility.CalculateCellIndex(new float3(-0.001f, -0.001f, -0.001f), gridScale));
            Assert.AreEqual(new int3(-1, -1, -1), MathematicsUtility.CalculateCellIndex(new float3(-0.999f, -0.999f, -0.999f), gridScale));
        }
    }
}
#endif