using Unity.Mathematics;
using UnityEngine;

namespace Shitakami.PracticalBoids
{
    [RequireComponent(typeof(SphereCollider))]
    public class SphereObstacle : MonoBehaviour
    {
        private SphereCollider _sphereCollider;
        private float _lossyScale;

        private void Awake()
        {
            _sphereCollider = GetComponent<SphereCollider>();
            _lossyScale = transform.lossyScale.x;
        }
        
        public SphereObstacleData SphereObstacleData => new(transform.position, _sphereCollider.radius * _lossyScale);
    }
    
    public struct SphereObstacleData
    {
        public float3 Position;
        public float RadiusSqr;
        
        public SphereObstacleData(float3 position, float radius)
        {
            Position = position;
            RadiusSqr = radius*radius;
        }
    }
}
