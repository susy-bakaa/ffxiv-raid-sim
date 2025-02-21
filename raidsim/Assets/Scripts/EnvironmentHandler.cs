using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHandler : MonoBehaviour
{
    public bool disableFogForWebGL = true;
    public bool disableFogForWindows = false;
    public bool disableFogForLinux = false;

    public string[] arenaModels;
    public GameObject[] arenas;

    private Transform dynamicParent;
    private FightSelector fightSelector;
    private string currentFightBundleName;
    private bool arenaModelLoaded = false;

    private void Awake()
    {
        fightSelector = FindObjectOfType<FightSelector>();
        dynamicParent = GameObject.FindGameObjectWithTag("persistent").transform.Find("Environment");   
#if UNITY_WEBPLAYER
        if (disableFogForWebGL)
        {
            RenderSettings.fog = false;
        }
#endif
#if UNITY_STANDALONE_WIN
        if (disableFogForWindows)
        {
            RenderSettings.fog = false;
        }
#endif
#if UNITY_STANDALONE_LINUX
        if (disableFogForLinux)
        {
            RenderSettings.fog = false;
        }
#endif
    }

    private void Start()
    {
        if (fightSelector != null)
        {
            currentFightBundleName = fightSelector.CurrentScene.assetBundle;
        }
    }

    private void Update()
    {
        if (arenaModels == null || arenaModels.Length <= 0)
            return;

        if (dynamicParent == null)
            dynamicParent = GameObject.FindGameObjectWithTag("persistent").transform.Find("Environment");

        if (AssetHandler.Instance != null && SceneHandler.Instance != null && !arenaModelLoaded)
        {
            if (AssetHandler.Instance.HasBundleLoaded(currentFightBundleName))
            {
                arenaModelLoaded = true;

                arenas = new GameObject[arenaModels.Length];
                for (int i = 0; i < arenaModels.Length; i++)
                {
                    //GameObject arena = AssetHandler.Instance.GetAsset(arenaModels[i]);
                    GameObject arena = null;
                    if (SceneHandler.Instance.DoesPersistentObjectExist(arenaModels[i]))
                    {
                        arena = SceneHandler.Instance.GetPersistentObject(arenaModels[i]);
                    }
                    else
                    {
                        arena = AssetHandler.Instance.GetAsset(arenaModels[i]);
                    }
                    arena.name = arena.name.Replace("(Clone)", "");
                    arena.transform.SetParent(dynamicParent);
                    arena.SetActive(false);
                    arenas[i] = arena;
                    SceneHandler.Instance.AddPersistentObject(arena);
                }

                if (arenas.Length > 0)
                    arenas[0].SetActive(true);
            }
        }
    }

    public void ChangeArenaModel(int index)
    {
        if (arenas == null || arenas.Length == 0)
            return;

        for (int i = 0; i < arenas.Length; i++)
        {
            arenas[i].SetActive(i == index);
        }
    }
}
