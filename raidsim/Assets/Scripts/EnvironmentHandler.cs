using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentHandler : MonoBehaviour
{
    public bool disableFogForWebGL = true;
    public bool disableFogForWindows = false;
    public bool disableFogForLinux = false;

    public int currentArenaIndex = 0;
    private int originalArenaIndex = -1;
    public string[] arenaModels;
    public GameObject[] arenas;

    //private Transform dynamicParent;
    private FightSelector fightSelector;
    private string currentFightBundleName;
    private bool arenaModelLoaded = false;

    private void Awake()
    {
        fightSelector = FindObjectOfType<FightSelector>();
        //dynamicParent = GameObject.FindGameObjectWithTag("persistent").transform.Find("Environment");   
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
        originalArenaIndex = currentArenaIndex;
    }

    private void Start()
    {
        if (fightSelector != null)
        {
            currentFightBundleName = fightSelector.CurrentScene.assetBundle;
        }

        if (FightTimeline.Instance != null)
            FightTimeline.Instance.onReset.AddListener(ResetArenaModel);
    }

    private void Update()
    {
        if (arenaModels == null || arenaModels.Length <= 0)
            return;

        if (AssetHandler.Instance != null && !arenaModelLoaded)
        {
            if (AssetHandler.Instance.HasBundleLoaded(currentFightBundleName))
            {
                arenaModelLoaded = true;

                // Remove any possible temporary arena placeholders
                if (arenas != null && arenas.Length > 0)
                {
                    for (int i = 0; i < arenas.Length; i++)
                    {
                        Destroy(arenas[i]);
                    }
                }

                arenas = new GameObject[arenaModels.Length];
                for (int i = 0; i < arenaModels.Length; i++)
                {
                    GameObject arena = AssetHandler.Instance.GetAsset(arenaModels[i]);
                    arena.transform.SetParent(transform);
                    arena.SetActive(false);
                    arenas[i] = arena;
                }

                if (arenas.Length > 0)
                {
                    if (originalArenaIndex >= 0 && originalArenaIndex < arenas.Length)
                    {
                        arenas[originalArenaIndex].SetActive(true);
                    }
                    else
                    {
                        arenas[0].SetActive(true);
                    }
                }
            }
        }
    }

    public void ResetArenaModel()
    {
        ChangeArenaModel(originalArenaIndex);
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
