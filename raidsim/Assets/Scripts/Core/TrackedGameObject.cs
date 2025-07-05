using UnityEngine;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Events
{
    public class TrackedGameObject : MonoBehaviour
    {
        public GameObjectTracker tracker;
        public string trackerName = string.Empty;
        public GameObject master;
        public GameObject[] relatives;

        private void Awake()
        {
            if (tracker == null && !string.IsNullOrEmpty(trackerName))
            {
                Utilities.FindAnyByName(trackerName).TryGetComponent(out tracker);
            }
        }

        public void AddObjectToTracker(GameObject gameObject)
        {
            if (tracker == null)
                return;

            tracker.AddTrackedObject(gameObject);
        }

        public void RemoveObjectFromTracker(GameObject gameObject)
        {
            if (tracker == null)
                return;

            tracker.RemoveTrackedObject(gameObject);
        }

        public void RemoveAllObjectsFromTracker()
        {
            if (tracker == null)
                return;

            tracker.DestroyAllTrackedObjects();
        }
    }
}