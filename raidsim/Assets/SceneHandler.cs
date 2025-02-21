using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }

    private Dictionary<string, GameObject> persistentObjects = new Dictionary<string, GameObject>();
#if UNITY_EDITOR
    [SerializeField] private List<PersistentObject> m_persistentObjects = new List<PersistentObject>();
    [System.Serializable]
    private struct PersistentObject
    {
        public string name;
        public GameObject gameObject;
    }
#endif

    private void Awake()
    {
        Instance = this;
    }

    public int GetPersistentObjectCount()
    {
        return persistentObjects.Count;
    }

    public bool DoesPersistentObjectExist(GameObject gameObject)
    {
        return DoesPersistentObjectExist(gameObject.name);
    }

    public bool DoesPersistentObjectExist(string name)
    {
        GameObject[] pObjs = persistentObjects.Values.ToArray();
        for (int i = 0; i < pObjs.Length; i++)
        {
            Debug.Log($"persistentObjects -> object number {i} '{pObjs[i]?.name}' out of {persistentObjects.Values.Count}");
        }
        Debug.Log($"DoesPersistentObjectExist: {name} => {persistentObjects.ContainsKey(name)}");
        return persistentObjects.ContainsKey(name);
    }

    public void AddPersistentObject(GameObject gameObject)
    {
        if (!persistentObjects.ContainsKey(gameObject.name))
        {
            persistentObjects.Add(gameObject.name, gameObject);
        }
        else
        {
            persistentObjects[gameObject.name] = gameObject;
        }

#if UNITY_EDITOR
        if (!m_persistentObjects.Any(po => po.name == gameObject.name))
        {
            m_persistentObjects.Add(new PersistentObject { name = gameObject.name, gameObject = gameObject });
        }
        else
        {
            var index = m_persistentObjects.FindIndex(po => po.name == gameObject.name);
            m_persistentObjects[index] = new PersistentObject { name = gameObject.name, gameObject = gameObject };
        }
#endif
    }

    public void RemovePersistentObject(GameObject gameObject)
    {
        RemovePersistentObject(gameObject.name);
    }

    public void RemovePersistentObject(string name)
    {
        if (persistentObjects.ContainsKey(name))
        {
            persistentObjects.Remove(name);
        }

#if UNITY_EDITOR
        var index = m_persistentObjects.FindIndex(po => po.name == name);
        if (index != -1)
        {
            m_persistentObjects.RemoveAt(index);
        }
#endif
    }

    public GameObject GetPersistentObject(string name)
    {
        if (persistentObjects.ContainsKey(name))
        {
            return persistentObjects[name];
        }
        return null;
    }
}
