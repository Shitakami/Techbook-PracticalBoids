using Part4;
using UnityEngine;

namespace Part5
{
    [RequireComponent(typeof(SphereCollider))]
    public class SphereObstacle : MonoBehaviour
    {
        private Transform _transform;
        private SphereCollider _sphereCollider;
        private float _lossyScale;

        private void Awake()
        {
            _transform = transform;
            _sphereCollider = GetComponent<SphereCollider>();
            _lossyScale = _transform.lossyScale.x;
        }

        public SphereObstacleData ObstacleData
            => gameObject.activeInHierarchy
                ? new SphereObstacleData(_transform.position, _sphereCollider.radius * _lossyScale)
                : new SphereObstacleData();
    }
}