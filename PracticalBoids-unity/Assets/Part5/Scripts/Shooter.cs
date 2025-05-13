using UnityEngine;

namespace Part5
{
    public class Shooter : MonoBehaviour
    {
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private float _intervalSeconds;
        private float _timeSinceLastShot = 0;

        private void Update()
        {
            // スペースキーもしくはマウスの左ボタンが押されたとき
            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                if (_timeSinceLastShot >= _intervalSeconds)
                {
                    _timeSinceLastShot = 0;
                    if (_bulletPool.TryGet(out var bullet))
                    {
                        bullet.transform.position = transform.position;
                        bullet.transform.rotation = transform.rotation;
                    }
                }
            }

            _timeSinceLastShot += Time.deltaTime;
        }
    }
}