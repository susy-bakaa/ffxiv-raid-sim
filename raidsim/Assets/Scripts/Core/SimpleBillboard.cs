using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleBillboard : MonoBehaviour
    {
        public bool x = true;
        public bool y = true;
        public bool z = true;

        private Transform target;

        private void Awake()
        {
            if (target == null)
                target = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 rot = transform.localEulerAngles;
                transform.LookAt(target);
                transform.localEulerAngles = new Vector3(x ? transform.localEulerAngles.x : rot.x, y ? transform.localEulerAngles.y : rot.y, z ? transform.localEulerAngles.z : rot.z);
            }
        }
    }
}