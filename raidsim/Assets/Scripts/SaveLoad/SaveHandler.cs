using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    public static SaveHandler Instance;

    private string[] saveData;

    void Awake()
    {
#if UNITY_WEBPLAYER
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
#else
        Debug.Log("Not a WebGL build -> SaveHandler was destroyed!");
        Destroy(gameObject);
        return;     
#endif
    }

    public void Write(string[] lines)
    {
        if (lines != null && lines.Length > 0)
        {
            saveData = lines;
        }
    }

    public string[] Load()
    {
        return saveData;
    }
}
