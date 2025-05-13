using System.Collections.Generic;
using UnityEngine;

namespace Part5
{
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private int _poolSize = 100;
        
        private readonly Queue<Bullet> _availableBullets = new Queue<Bullet>();
        private Bullet[] _allBullets;
        
        public IReadOnlyList<Bullet> Bullets => _allBullets;
        
        public void Setup()
        {
            _allBullets = new Bullet[_poolSize];
            
            // プールの初期化
            for (int i = 0; i < _poolSize; i++)
            {
                var bullet = Instantiate(_bulletPrefab, transform);
                bullet.gameObject.SetActive(false);
                bullet.SetDeactivationCallback(ReturnToPool);
                
                _availableBullets.Enqueue(bullet);
                _allBullets[i] = bullet;
            }
        }

        public bool TryGet(out Bullet bullet)
        {
            if (_availableBullets.Count == 0)
            {
                bullet = null;
                return false;
            }
            
            bullet = _availableBullets.Dequeue();
            bullet.gameObject.SetActive(true);
            return true;
        }
        
        // 弾のプールへの返却（コールバック用）
        private void ReturnToPool(Bullet bullet)
        {
            if (bullet.gameObject.activeInHierarchy)
                bullet.gameObject.SetActive(false);
                
            _availableBullets.Enqueue(bullet);
        }

        public void ReturnByIndex(int index)
        {
            if (index < 0 || index >= _allBullets.Length)
                return;
                
            ReturnToPool(_allBullets[index]);
        }
    }
}