using System.Collections.Generic;
using UnityEngine;

namespace Part5
{
    public class ExplosionEffectPool : MonoBehaviour
    {
        [SerializeField] private EffectLifeTime _explosionEffectPrefab;
        [SerializeField] private int _initialPoolSize;

        private readonly Queue<EffectLifeTime> _pool = new Queue<EffectLifeTime>();
        private SphereObstacle[] _explosionObstacleArray;
        
        public IReadOnlyList<SphereObstacle> ExplosionObstacleArray => _explosionObstacleArray; 
        
        public void Setup()
        {
            _explosionObstacleArray = new SphereObstacle[_initialPoolSize];
            
            for (var i = 0; i < _initialPoolSize; i++)
            {
                var explosionEffect = Instantiate(_explosionEffectPrefab, transform);
                explosionEffect.gameObject.SetActive(false);
                explosionEffect.SetEffectDestroyedEvent(ReturnEffect);
                _explosionObstacleArray[i] = explosionEffect.gameObject.AddComponent<SphereObstacle>();
                _pool.Enqueue(explosionEffect);
            }
        }

        public bool TryGetEffect(out EffectLifeTime bullet)
        {
            if (_pool.Count > 0)
            {
                bullet = _pool.Dequeue();
                bullet.gameObject.SetActive(true);
                return true;
            }

            bullet = null;
            return false;
        }

        private void ReturnEffect(EffectLifeTime effectLifeTime)
        {
            effectLifeTime.gameObject.SetActive(false);
            _pool.Enqueue(effectLifeTime);
        }

        private void OnDestroy()
        {
            foreach (var bullet in _pool)
            {
                Destroy(bullet.gameObject);
            }

            _pool.Clear();
        }
    }
}