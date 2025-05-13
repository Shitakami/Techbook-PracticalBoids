using System;
using Common;
using Part2;
using UnityEngine;
using UnityEngine.Rendering;

namespace Part4
{
    public class BoidsBehaviour : MonoBehaviour
    {
        [Header("Boids設定")]
        [SerializeField] private BoidsSetting _boidsSetting;
        [SerializeField] private AvoidObstacleSetting _avoidObstacleSetting;
        [SerializeField] private int _instanceCount;

        [Header("モデル, 描画設定")] 
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private ShadowCastingMode _shadowCastingMode;
        [SerializeField] private bool _receiveShadows;
        
        [Header("JobのBatchCount")] 
        [SerializeField] private JobBatchSetting _jobBatchSetting;
        
        private SphereObstacle[] _sphereObstacles;
        private BoidsSystemCore _boidsSystemCore;

        [Serializable]
        public struct JobBatchSetting
        {
            public int RegisterInstanceToGridBatchCount;
            public int CalculateSteerForceBatchCount;
            public int ApplySteerForceBatchCount;
            public int SpherecastCommandBatchCount;
            public int AvoidObstacleAndUpdateBoidsJobBatchCount;
            public int TranslateMatrix4x4BatchCount;
        }
        
        private void Start()
        {
            _sphereObstacles = FindObjectsByType<SphereObstacle>(FindObjectsSortMode.None);
            
            _boidsSystemCore = new BoidsSystemCore(_instanceCount, _sphereObstacles.Length);
            _boidsSystemCore.Setup(
                _boidsSetting, 
                transform.position, 
                transform.localScale);
        }

        private void LateUpdate()
        {
            _boidsSystemCore.Complete();

            var renderParams = new RenderParams(_material)
            {
                worldBounds = new Bounds(center: transform.position, size: transform.lossyScale),
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode,
            };
            
            RenderMeshUtility.DrawAll(_mesh, renderParams, _boidsSystemCore.BoidsTransformMatrixArray);

            _boidsSystemCore.UpdateSphereObstacles(_sphereObstacles);

            _boidsSystemCore.ExecuteUpdate(
                _boidsSetting,
                _avoidObstacleSetting,
                transform.position,
                transform.localScale,
                _jobBatchSetting
            );
        }
    }
}