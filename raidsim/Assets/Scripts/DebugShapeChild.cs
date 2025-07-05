using UnityEngine;

namespace dev.susybaka.raidsim.Editor
{
    public class DebugShapeChild : MonoBehaviour
    {
#if UNITY_EDITOR
        DebugShape debugShape;

        private void OnDrawGizmosSelected()
        {
            if (debugShape == null)
            {
                debugShape = GetComponentInParent<DebugShape>();
            }
            if (debugShape != null)
            {
                if (debugShape.whenSelected)
                    debugShape.OnDrawGizmosSelected();
            }
        }
#endif
    }
}