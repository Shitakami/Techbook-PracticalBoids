using System;
using Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Part2
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

        [Serializable]
        public struct JobBatchSetting
        {
            public int CalculateSteerForceBatchCount;
            public int ApplySteerForceBatchCount;
            public int TranslateMatrix4x4BatchCount;
        }
        
        private BoidsSystemCore _boidsSystemCore;

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
            _boidsSystemCore.ExecuteUpdate(
                _boidsSetting,
                _jobBatchSetting,
                transform.position,
                transform.localScale);

            var renderParams = new RenderParams(_material)
            {
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode,
                worldBounds = new Bounds(
                    center: transform.position,
                    size: transform.lossyScale),
            };
            
            RenderMeshUtility.DrawAll(_mesh, renderParams, _boidsSystemCore.BoidsTransformMatrixArray);
        }
    }
}