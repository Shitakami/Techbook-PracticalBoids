using UnityEngine;

namespace Part4
{
    [CreateAssetMenu(fileName = "AvoidObstacleSetting_", menuName = "Part4/AvoidObstacleSetting", order = 3003)]
    public class AvoidObstacleSetting : ScriptableObject
    {
        [Header("障害物回避の設定")] 
        [SerializeField] private float _spherecastDistance;
        [SerializeField] private float _spherecastRadius;
        [SerializeField] private float _avoidRotateVelocity;
        
        [Header("動的オブジェクトからの退避設定")]
        [SerializeField] private float _escapeObstaclesWeight;
        [SerializeField] private float _escapeMaxSpeed;
        
        public float SpherecastDistance => _spherecastDistance;
        public float SpherecastRadius => _spherecastRadius;
        public float AvoidRotateVelocity => _avoidRotateVelocity;
        
        public float EscapeObstaclesWeight => _escapeObstaclesWeight;
        public float EscapeMaxSpeed => _escapeMaxSpeed;
    }
}