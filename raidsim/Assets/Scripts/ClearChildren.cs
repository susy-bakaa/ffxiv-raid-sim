using UnityEngine;

namespace dev.susybaka.raidsim
{
    public class ClearChildren : MonoBehaviour
    {
        public bool onAwake = true;

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}