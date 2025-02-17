using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
