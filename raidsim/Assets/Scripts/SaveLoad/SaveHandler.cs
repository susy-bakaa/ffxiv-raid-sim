using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveHandler : MonoBehaviour
{
    public static SaveHandler Instance;

    private string[] saveData;
#if UNITY_EDITOR
    public string[] SaveData;
    public string[] SavedData;
    public string savedDataString;

    [NaughtyAttributes.Button]
    public void TestSave() 
    {
        SaveToPlayerPrefs();
    }
#endif

    private void Awake()
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
        if (PlayerPrefs.HasKey("Config"))
        {
            string loadedString = PlayerPrefs.GetString("Config");

            List<string> loadedData = new List<string>(loadedString.Split('&'));

            for (int i = 0; i < loadedData.Count; i++)
            {
                if (loadedData[i] == "%")
                {
                    loadedData[i] = string.Empty;
                }
            }

            saveData = loadedData.ToArray();
        }
#else
        Debug.Log("Not a WebGL build -> SaveHandler was destroyed!");
        Destroy(gameObject);
        return;
#endif
    }

#if UNITY_EDITOR
    private void Update()
    {
        SaveData = saveData;
    }
#endif

    private void SaveToPlayerPrefs()
    {
        string[] dataToSave = new string[saveData.Length];

        for (int i = 0; i < dataToSave.Length; i++)
        {
            dataToSave[i] = saveData[i];

            if (string.IsNullOrEmpty(dataToSave[i]))
            {
                dataToSave[i] = "%";
            }
        }

#if UNITY_EDITOR
        SavedData = dataToSave;
        savedDataString = string.Join('&', dataToSave);
#endif

        PlayerPrefs.SetString("Config", string.Join('&', dataToSave));
        PlayerPrefs.Save();
    }

    public void Write(string[] lines)
    {
        if (lines != null && lines.Length > 0)
        {
            saveData = lines;
        }

        SaveToPlayerPrefs();
    }

    public string[] Load()
    {
        return saveData;
    }
}
