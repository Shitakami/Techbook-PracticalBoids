using System;
using Common;
using Part4;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Part5
{
    public class BoidsBehaviour : MonoBehaviour
    {
        [Header("Boids設定")] 
        [SerializeField] private Part2.BoidsSetting _boidsSetting;
        [SerializeField] private AvoidObstacleSetting _avoidObstacleSetting;
        [SerializeField] private int _instanceCount;

        [Header("モデル, 描画設定")] 
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
        [SerializeField] private ShadowCastingMode _shadowCastingMode;
        [SerializeField] private bool _receiveShadows;

        [Header("STG設定")] 
        [SerializeField] private float _boidsColliderRadius;
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private ExplosionEffectPool _explosionEffectPool;

        [Header("JobのBatchCount")] [SerializeField]
        private JobBatchSetting _jobBatchSetting;
        
        [Serializable]
        public struct JobBatchSetting
        {
            public int RegisterInstanceToGridBatchCount;
            public int CollisionDetectionBatchCount;
            public int CalculateSteerForceBatchCount;
            public int ApplySteerForceBatchCount;
            public int SpherecastCommandBatchCount;
            public int AvoidObstacleAndUpdateBoidsJobBatchCount;
            public int TranslateMatrix4x4BatchCount;
        }
        
        private BoidsSystemCore _boidsSystemCore;

        public void Start()
        {
            _bulletPool.Setup();
            _explosionEffectPool.Setup();

            _boidsSystemCore = new BoidsSystemCore(
                _instanceCount,
                _explosionEffectPool.ExplosionObstacleArray.Count,
                _bulletPool.Bullets.Count
            );

            _boidsSystemCore.Setup(
                _boidsSetting,
                transform.position,
                transform.localScale);
        }

        private void Update()
        {
            var collisionData = _boidsSystemCore.GetCollisionData();
            ApplyCollisionData(collisionData);
        }

        private void LateUpdate()
        {
            _boidsSystemCore.Complete();

            var renderParams = new RenderParams(_material)
            {
                receiveShadows = _receiveShadows,
                shadowCastingMode = _shadowCastingMode,
                worldBounds = new Bounds(center: transform.position, size: transform.lossyScale)
            };
            
            RenderMeshUtility.DrawAll(_mesh, renderParams, _boidsSystemCore.BoidsTransformMatrices);

            _boidsSystemCore.UpdateSphereObstacles(_explosionEffectPool.ExplosionObstacleArray);
            _boidsSystemCore.UpdateBullets(_bulletPool.Bullets);
            _boidsSystemCore.ExecuteUpdate(
                _boidsSetting,
                _avoidObstacleSetting,
                _boidsColliderRadius,
                transform.position,
                transform.localScale,
                _jobBatchSetting);
        }

        private void ApplyCollisionData(NativeArray<CollisionData> collisionData)
        {
            for (var bulletIndex = 0; bulletIndex < collisionData.Length; bulletIndex++)
            {
                var data = collisionData[bulletIndex];
                if (!data.IsCollided)
                {
                    continue;
                }
                
                _bulletPool.ReturnByIndex(bulletIndex);
                if (_explosionEffectPool.TryGetEffect(out var effect))
                {
                    effect.transform.position = data.Position;
                }
            }
        }
    }
}