using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Part4
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
        // MEMO: Part5の衝突判定で使用
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsActive() => RadiusSqr != 0;

        public float3 Position;
        public float RadiusSqr;
        
        public SphereObstacleData(float3 position, float radius)
        {
            Position = position;
            RadiusSqr = radius*radius;
        }
    }
}
