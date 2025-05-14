using Unity.Mathematics;
using UnityEngine;

namespace Shitakami.PracticalBoids
{
    [CreateAssetMenu(fileName = "BoidsSetting_", menuName = "Part2/BoidsSetting", order = 3002)]
    public class BoidsSetting : ScriptableObject
    {
        [Header("結合")]
        [SerializeField] private float _cohesionWeight;
        [SerializeField] private float _cohesionAffectedRadius;
        [SerializeField, Range(0, 360)] private float _cohesionViewAngle;

        [Header("分離")]
        [SerializeField] private float _separationWeight;
        [SerializeField] private float _separationAffectedRadius;
        [SerializeField, Range(0, 360)] private float _separationViewAngle;

        [Header("整列")]
        [SerializeField] private float _alignmentWeight;
        [SerializeField] private float _alignmentAffectedRadius;
        [SerializeField, Range(0, 360)] private float _alignmentViewAngle;

        [Header("シミュレーション空間外から戻る力")]
        [SerializeField] private float _returnSimulationAreaWeight;

        [Header("個体のスケール")]
        [Space(20)]
        [SerializeField] private float3 _instanceScale;
        
        [Header("速度、加速度設定")]
        [SerializeField] private float _initializedSpeed;
        [SerializeField] private float _maxSpeed;
        [SerializeField] private float _maxSteerForce;

        public float CohesionWeight => _cohesionWeight;
        public float CohesionAffectedRadius => _cohesionAffectedRadius;
        public float CohesionViewDot => AngleToDot(_cohesionViewAngle);
        
        public float SeparationWeight => _separationWeight;
        public float SeparationAffectedRadius => _separationAffectedRadius;
        public float SeparationViewDot => AngleToDot(_separationViewAngle);

        public float AlignmentWeight => _alignmentWeight;
        public float AlignmentAffectedRadius => _alignmentAffectedRadius;
        public float AlignmentViewDot => AngleToDot(_alignmentViewAngle);
        
        public float ReturnSimulationAreaWeight => _returnSimulationAreaWeight;

        public float3 InstanceScale => _instanceScale;

        public float InitializedSpeed => _initializedSpeed;
        public float MaxSpeed => _maxSpeed;
        public float MaxSteerForce => _maxSteerForce;

        public float MaxAffectedRadius => Mathf.Max(_cohesionAffectedRadius, _separationAffectedRadius, _alignmentAffectedRadius);

        private static float AngleToDot(float angle)
        {
            return Mathf.Cos(angle * Mathf.PI / 360);
        }
    }
}