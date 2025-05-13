using Unity.Collections;
using UnityEngine;

namespace Common
{
    public static class RenderMeshUtility
    {
        public static void DrawAll(Mesh mesh, RenderParams renderParams, NativeArray<Matrix4x4> transformMatrixArray)
        {
            const int instanceCountPerDraw = 1023;
            var length = transformMatrixArray.Length;

            for (var i = 0; i < length; i += instanceCountPerDraw)
            {
                var instanceCount = Mathf.Min(instanceCountPerDraw, length - i);
                Graphics.RenderMeshInstanced(
                    renderParams, 
                    mesh,
                    0,
                    transformMatrixArray,
                    instanceCount,
                    i);
            }
        }
    }
}