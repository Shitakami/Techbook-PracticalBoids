using System;
using Common;
using UnityEngine;
using UnityEngine.Rendering;
using Part2;

namespace Part3
{
    public class BoidsBehaviour : MonoBehaviour
    {
        [Header("Boids設定")]
        [SerializeField] private BoidsSetting _boidsSetting;
        [SerializeField] private int _instanceCount;

        [Header("モデル, 描画設定")] 
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private ShadowCastingMode _shadowCastingMode;
        [SerializeField] private bool _receiveShadows;

        [Header("JobのBatchCount")] 
        [SerializeField] private JobBatchSetting _jobBatchSetting;

        private BoidsSystemCore _boidsSystemCore;
        
        [Serializable]
        public struct JobBatchSetting
        {
            public int RegisterInstanceToGridBatchCount;
            public int CalculateSteerForceBatchCount;
            public int ApplySteerForceBatchCount;
            public int TranslateMatrix4x4BatchCount;
        }

        private void Start()
        {
            _boidsSystemCore = new BoidsSystemCore(_instanceCount);
            _boidsSystemCore.Setup(
                _boidsSetting,
                transform.position,
                transform.lossyScale);
        }

        private void LateUpdate()
        {
            _boidsSystemCore.Complete();

            var renderParams = new RenderParams(_material)
            {
                worldBounds = new Bounds(transform.position, transform.lossyScale),
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode
            };

            RenderMeshUtility.DrawAll(_mesh, renderParams, _boidsSystemCore.BoidsTransformMatrixArray);

            _boidsSystemCore.ExecuteUpdate(
                _boidsSetting,
                transform.position,
                transform.localScale,
                _jobBatchSetting);
        }
    }
}