using UnityEngine;

namespace Shitakami.PracticalBoids
{
    public static class RaycastHitExtension
    {
        public static bool IsHit(this RaycastHit raycastHit)
        {
            return raycastHit.colliderInstanceID != 0;
        }
    }
}
