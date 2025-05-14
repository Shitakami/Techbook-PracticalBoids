using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Shitakami.PracticalBoids
{
    public static class BoidsUtility
    {
        public static void InitializeBoidsData(
            NativeArray<BoidsData> boidsDatas, 
            float3 simulationAreaCenter,
            float3 simulationAreaScaleHalf, 
            float initializeVelocity)
        {
            for (var i = 0; i < boidsDatas.Length; ++i)
            {
                boidsDatas[i] = new BoidsData
                {
                    Position = simulationAreaScaleHalf * Random.insideUnitSphere + simulationAreaCenter,
                    Velocity = Random.insideUnitSphere * initializeVelocity
                };
            }
        }
    }
}