using UnityEngine;
using UnityEngine.Events;

namespace dev.susybaka.raidsim.Events
{
    public class GameObjectEvent : MonoBehaviour
    {
        public GameObject target;
        public UnityEvent<GameObject> m_event;

        private void Awake()
        {
            if (target == null)
            {
                target = gameObject;
            }
        }

        public void BasicGameObjectEvent()
        {
            m_event.Invoke(target);
        }
    }
}