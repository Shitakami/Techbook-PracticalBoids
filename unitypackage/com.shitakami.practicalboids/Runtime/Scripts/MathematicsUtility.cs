using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Shitakami.PracticalBoids
{
    public static class MathematicsUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Limit(float3 vec, float max)
        {
            var length = math.length(vec);
            return length > max
                ? vec / length * max
                : vec;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 CalculateCellIndex(float3 position, float gridScale)
        {
            return (int3) math.floor(position / gridScale);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalculateReturnAreaForce(float3 position, float3 areaCenter, float3 areaScale)
        {
            return new float3(
                (position.x < areaCenter.x - areaScale.x ? 1.0f : 0.0f) +
                (position.x > areaCenter.x + areaScale.x ? -1.0f : 0.0f),
                (position.y < areaCenter.y - areaScale.y ? 1.0f : 0.0f) +
                (position.y > areaCenter.y + areaScale.y ? -1.0f : 0.0f),
                (position.z < areaCenter.z - areaScale.z ? 1.0f : 0.0f) +
                (position.z > areaCenter.z + areaScale.z ? -1.0f : 0.0f)
            );
        }
    }
}