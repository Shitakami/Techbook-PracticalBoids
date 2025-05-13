using System;
using UnityEngine;

namespace Part5
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private SphereCollider _collider;
        [SerializeField] private TrailRenderer _trail;

        private float _aliveTime;
        private Action<Bullet> _onDeactivated;

        private void OnEnable() => _aliveTime = 0;

        private void OnDisable() => _trail?.Clear();

        private void Update()
        {
            transform.position += transform.forward * _speed * Time.deltaTime;

            _aliveTime += Time.deltaTime;
            if (_aliveTime >= _lifetime)
            {
                Deactivate();
            }
        }

        private void Deactivate()
        {
            _onDeactivated?.Invoke(this);
            gameObject.SetActive(false);
        }

        public void SetDeactivationCallback(Action<Bullet> callback)
        {
            _onDeactivated = callback;
        }

        public BulletData GetData()
        {
            if (!gameObject.activeInHierarchy)
                return BulletData.CreateEmpty();

            return new BulletData
            {
                Position = transform.position,
                Velocity = transform.forward * _speed,
                Radius = _collider.radius * transform.lossyScale.x
            };
        }
    }
}