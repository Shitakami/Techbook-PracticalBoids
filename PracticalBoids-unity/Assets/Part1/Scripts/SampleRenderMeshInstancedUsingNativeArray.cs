using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Part1
{
    public class SampleRenderMeshInstancedUsingNativeArray : MonoBehaviour
    {
        [Header("モデル設定")] 
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Vector3 _instanceScale;
        
        [Header("描画設定")]
        [SerializeField] private Material _material;
        [SerializeField] private ShadowCastingMode _shadowCastingMode;
        [SerializeField] private bool _receiveShadows;
        
        [Header("インスタンス数")]
        [SerializeField] private int _instanceCount;

        private NativeArray<Matrix4x4> _instanceMatrix4x4Array;
        
        private void Start()
        {
            var simulationScale = transform.lossyScale;
            var instanceRange = simulationScale / 2;
            _instanceMatrix4x4Array = new NativeArray<Matrix4x4>(_instanceCount, Allocator.Domain);
            
            // インスタンスの位置、回転、スケールをランダムに設定
            for (var i = 0; i < _instanceCount; i++)
            {
                var position = new Vector3(
                    Random.Range(-instanceRange.x, instanceRange.x),
                    Random.Range(-instanceRange.y, instanceRange.y),
                    Random.Range(-instanceRange.z, instanceRange.z)
                );
                var rotation = Random.rotation;
                var scale = _instanceScale;
                _instanceMatrix4x4Array[i] = Matrix4x4.TRS(position, rotation, scale);
            }
        }

        private void Update()
        {
            var renderParams = new RenderParams(_material)
            {
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode,
                worldBounds = new Bounds(center: transform.position, size: transform.lossyScale)
            };

            Graphics.RenderMeshInstanced(
                rparams: renderParams,
                mesh: _mesh,
                submeshIndex: 0,
                instanceData: _instanceMatrix4x4Array,
                instanceCount: _instanceCount
            );

            const int instanceCountPerDraw = 1023;
            for (int startIndex = 0; startIndex < _instanceCount; startIndex += instanceCountPerDraw)
            {
                var length = Mathf.Min(instanceCountPerDraw, _instanceCount - startIndex);
                Graphics.RenderMeshInstanced(
                    rparams: renderParams,
                    mesh: _mesh,
                    submeshIndex: 0,
                    instanceData: _instanceMatrix4x4Array,
                    instanceCount: length,
                    startInstance: startIndex // 指定したIndexから描画を開始
                );
            }
        }
    }
}