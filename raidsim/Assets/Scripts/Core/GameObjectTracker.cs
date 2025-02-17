using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectTracker : MonoBehaviour
{
    public Dictionary<GameObject, GameObjectInfo> tracked = new Dictionary<GameObject, GameObjectInfo>();
#if UNITY_EDITOR
    [SerializeField] private List<GameObjectInfo> m_tracked = new List<GameObjectInfo>();
#endif

    private void OnValidate()
    {
        for (int i = 0; i < tracked.Values.Count; i++)
        {
            GameObjectInfo goi = tracked.Values.ToArray()[i];
            if (goi.gameObject != null)
            {
                goi.name = goi.gameObject.name;
            }
            tracked[goi.gameObject] = goi;
        }

#if UNITY_EDITOR
        m_tracked = tracked.Values.ToList();
#endif
    }

    public void AddTrackedObject(GameObject gameObject)
    {
        if (tracked.ContainsKey(gameObject))
            return;

        if (gameObject == null)
            return;

        TrackedGameObject[] childTracked = gameObject.GetComponentsInChildren<TrackedGameObject>(true);

        List<GameObject> childObjects = new List<GameObject>();

        for (int i = 0; i < childTracked.Length; i++)
        {
            if (childTracked[i].master == gameObject)
            {
                if (childTracked[i].relatives != null && childTracked[i].relatives.Length > 0)
                    childObjects.AddRange(childTracked[i].relatives);
            }
        }

        if (childObjects == null || childObjects.Count < 1)
            tracked.Add(gameObject, new GameObjectInfo(gameObject.name, gameObject));
        else
            tracked.Add(gameObject, new GameObjectInfo(gameObject.name, gameObject, childObjects));

#if UNITY_EDITOR 
        if (childObjects == null || childObjects.Count < 1)
            m_tracked.Add(new GameObjectInfo(gameObject.name, gameObject));
        else
            m_tracked.Add(new GameObjectInfo(gameObject.name, gameObject, childObjects));
#endif
    }

    public void RemoveTrackedObject(GameObject gameObject)
    {
        if (tracked.ContainsKey(gameObject))
            tracked.Remove(gameObject);

#if UNITY_EDITOR
        m_tracked.RemoveAll(goi => goi.gameObject == gameObject);
#endif
    }

    public void AddTrackedRelatedObject(GameObject master, GameObject relative)
    {
        if (!tracked.ContainsKey(master))
            return;

        if (tracked[master].relatedObjects == null)
        {
            GameObjectInfo goi = tracked[master];
            goi.relatedObjects = new List<GameObject>();
            tracked[master] = goi;
        }

        if (tracked[master].relatedObjects.Contains(relative))
            return;

        tracked[master].relatedObjects.Add(relative);

#if UNITY_EDITOR
        m_tracked = tracked.Values.ToList();
#endif
    }

    public void RemoveTrackedRelatedObject(GameObject master, GameObject relative)
    {
        if (!tracked.ContainsKey(master))
            return;

        if (tracked[master].relatedObjects == null || tracked[master].relatedObjects.Count < 1)
            return;

        if (!tracked[master].relatedObjects.Contains(relative))
            return;

        tracked[master].relatedObjects.Remove(relative);

#if UNITY_EDITOR
        m_tracked = tracked.Values.ToList();
#endif
    }

    public void DestroyObjectInstantly(GameObject gameObject)
    {
        if (!tracked.ContainsKey(gameObject))
            return;

        tracked[gameObject].relatedObjects.ForEach(go => Destroy(go));
        tracked[gameObject].relatedObjects.Clear();
        GameObject temp = tracked[gameObject].gameObject;
        tracked.Remove(gameObject);
        Destroy(temp);

#if UNITY_EDITOR
        m_tracked = tracked.Values.ToList();
#endif
    }

    public void DestroyObject(GameObject gameObject)
    {
        if (!tracked.ContainsKey(gameObject))
            return;

        tracked[gameObject].relatedObjects.ForEach(go => HandleObjectDestruction(go));
        tracked[gameObject].relatedObjects.Clear();
        GameObject temp = tracked[gameObject].gameObject;
        tracked.Remove(gameObject);
        HandleObjectDestruction(temp);

#if UNITY_EDITOR
        m_tracked = tracked.Values.ToList();
#endif
    }

    private void HandleObjectDestruction(GameObject gameObject)
    {
        if (gameObject.TryGetComponentInChildren(true, out SimpleShaderFade shaderFade))
        {
            shaderFade.FadeOut();
            Destroy(gameObject, 0.75f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DestroyAllTrackedObjectsInstantly()
    {
        GameObjectInfo[] gameObjectInfos = tracked.Values.ToArray();
        for (int i = 0; i < gameObjectInfos.Length; i++)
        {
            DestroyObjectInstantly(gameObjectInfos[i].gameObject);
        }
        tracked.Clear();
#if UNITY_EDITOR
        m_tracked.Clear();
#endif
    }

    public void DestroyAllTrackedObjects()
    {
        GameObjectInfo[] gameObjectInfos = tracked.Values.ToArray();
        for (int i = 0; i < gameObjectInfos.Length; i++)
        {
            DestroyObject(gameObjectInfos[i].gameObject);
        }
        tracked.Clear();
#if UNITY_EDITOR
        m_tracked.Clear();
#endif
    }

#if UNITY_EDITOR
    [System.Serializable]
#endif
    public struct GameObjectInfo
    {
        public string name;
        public GameObject gameObject;
        public List<GameObject> relatedObjects;

        public GameObjectInfo(string name, GameObject gameObject)
        {
            this.name = name;
            this.gameObject = gameObject;
            relatedObjects = new List<GameObject>();
        }

        public GameObjectInfo(string name, GameObject gameObject, List<GameObject> relatedObjects)
        {
            this.name = name;
            this.gameObject = gameObject;
            this.relatedObjects = relatedObjects;
        }
    }
}
