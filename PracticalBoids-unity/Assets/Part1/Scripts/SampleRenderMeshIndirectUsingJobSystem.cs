using Common;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace Part1
{
    public class SampleRenderMeshIndirectUsingJobSystem : MonoBehaviour
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

        [Header("アニメーション設定")]
        [SerializeField] private float _moveVelocity;
        [SerializeField] private float _rotationVelocity;

        [Header("JobのBatchCount")]
        [SerializeField] private int _innerloopBatchCount;
        
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
            // Jobの作成
            var job = new CubeAnimationJob(
                instancedHeight: (transform.position + transform.localScale / 2).y,
                destroyHeight: (transform.position - transform.localScale / 2).y,
                _moveVelocity,
                _rotationVelocity,
                Time.deltaTime,
                _instanceMatrix4x4Array
            );

            // Jobの実行
            var jobHandle = job.Schedule(_instanceCount, _innerloopBatchCount);

            // Jobの完了待ち
            jobHandle.Complete();

            var renderParams = new RenderParams(_material)
            {
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode,
                worldBounds = new Bounds(transform.position, transform.localScale)
            };
            
            RenderMeshUtility.DrawAll(_mesh, renderParams, _instanceMatrix4x4Array);
        }
    }
}