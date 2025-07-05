using UnityEngine;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using dev.susybaka.raidsim.UI;

namespace dev.susybaka.raidsim.Core
{
    public class EnvironmentHandler : MonoBehaviour
    {
        public bool disableFogForWebGL = true;
        public bool disableFogForWindows = false;
        public bool disableFogForLinux = false;

        public int currentArenaIndex = 0;
        private int originalArenaIndex = -1;
        public string[] arenaModels;
        public GameObject[] arenas;
        public ArenaModelData[] arenaModelData;

        //private Transform dynamicParent;
        private FightSelector fightSelector;
        private string currentFightBundleName;
        private bool arenaModelLoaded = false;

#if UNITY_EDITOR
        [Button]
        public void TransferArenaData()
        {
            arenaModelData = new ArenaModelData[arenaModels.Length];

            if (arenas == null || arenaModels == null || arenas.Length != arenaModels.Length)
            {
                Debug.LogWarning("Arena models and arenas do not match in length. Please ensure they are set up correctly and contain placeholder models.");
                return;
            }

            for (int i = 0; i < arenaModelData.Length; i++)
            {
                ArenaModelData temp = arenaModelData[i];
                temp.name = arenaModels[i];
                temp.model = arenas[i];
                temp.position = Vector3.zero;
                temp.rotation = Vector3.zero;
                temp.scale = Vector3.one;
                arenaModelData[i] = temp;
            }
        }
#endif

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
            if (arenaModelData == null || arenaModelData.Length <= 0)
                return;

            if (AssetHandler.Instance != null && !arenaModelLoaded)
            {
                if (AssetHandler.Instance.HasBundleLoaded(currentFightBundleName))
                {
                    arenaModelLoaded = true;

                    // Remove any possible temporary arena placeholders
                    if (arenaModelData != null && arenaModelData.Length > 0)
                    {
                        for (int i = 0; i < arenaModelData.Length; i++)
                        {
                            if (string.IsNullOrEmpty(arenaModelData[i].name))
                            {
                                continue;
                            }

                            if (arenaModelData[i].model != null)
                            {
                                Destroy(arenaModelData[i].model);
                            }

                            GameObject arena = AssetHandler.Instance.GetAsset(arenaModelData[i].name);
                            arena.transform.SetParent(transform);
                            arena.SetActive(false);
                            arena.transform.localPosition = arenaModelData[i].position;
                            arena.transform.localEulerAngles = arenaModelData[i].rotation;
                            arena.transform.localScale = arenaModelData[i].scale;
                            arenaModelData[i].model = arena;
                        }
                    }

                    if (arenaModelData.Length > 0)
                    {
                        if (originalArenaIndex >= 0 && originalArenaIndex < arenaModelData.Length)
                        {
                            arenaModelData[originalArenaIndex].model?.SetActive(true);
                        }
                        else
                        {
                            arenaModelData[0].model?.SetActive(true);
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
            if (arenaModelData == null || arenaModelData.Length == 0)
                return;

            for (int i = 0; i < arenaModelData.Length; i++)
            {
                arenaModelData[i].model.SetActive(i == index);
            }
        }

        [System.Serializable]
        public struct ArenaModelData
        {
            public string name;
            public GameObject model;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            public ArenaModelData(string name, GameObject model, Vector3 position, Vector3 rotation, Vector3 scale)
            {
                this.name = name;
                this.model = model;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }
    }

}