using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.Shared.Attributes;

namespace dev.susybaka.raidsim.Core
{
    public class EnvironmentHandler : MonoBehaviour
    {
        public bool disableFogForWebGL = true;
        public bool disableFogForWindows = false;
        public bool disableFogForLinux = false;

        public int currentArenaIndex = 0;
        private int originalArenaIndex = -1;

        // Legacy variables that are not used anymore but kept for compatibility
        // to avoid breaking existing scenes in other people's forks,
        // these can be migrated by using the MigrateOldArenaData function (button) in the editor
        [Obsolete("Use arenaModelData array instead. This field is kept for compatability reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public string[] arenaModels;
        [Obsolete("Use arenaModelData array instead. This field is kept for compatability reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] public GameObject[] arenas;
        // ---
        // This is the new data structure that holds arena model data
        public ArenaModelData[] arenaModelData;
        public UnityEvent onLoad;

        private bool allArenaModelsLoaded = false;
        private Dictionary<ArenaModelData, GameObject> loadedArenaModels = new Dictionary<ArenaModelData, GameObject>();

#if UNITY_EDITOR
#pragma warning disable CS0618 // Disable obsolete warning for arenaModels and arenas for the editor tool
        [NaughtyAttributes.Button]
        public void MigrateOldArenaData()
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
#pragma warning restore CS0618 // restore obsolete warning for the rest of the script
#endif

        private void Awake()
        {
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
            loadedArenaModels = new Dictionary<ArenaModelData, GameObject>();

            if (FightTimeline.Instance != null)
                FightTimeline.Instance.onReset.AddListener(ResetArenaModel);
        }

        private void Update()
        {
            if (arenaModelData == null || arenaModelData.Length <= 0 || allArenaModelsLoaded)
                return;

            if (AssetHandler.Instance != null && !allArenaModelsLoaded)
            {
                // Remove any possible temporary arena placeholders
                if (arenaModelData != null && arenaModelData.Length > 0)
                {
                    for (int i = 0; i < arenaModelData.Length; i++)
                    {
                        if (AssetHandler.Instance.HasBundleLoaded(arenaModelData[i].bundle) && !loadedArenaModels.ContainsKey(arenaModelData[i]))
                        {
                            allArenaModelsLoaded = true;

                            if (string.IsNullOrEmpty(arenaModelData[i].name))
                            {
                                continue;
                            }

                            if (arenaModelData[i].model != null)
                            {
                                Destroy(arenaModelData[i].model);
                            }

                            GameObject arena = AssetHandler.Instance.GetAsset(arenaModelData[i].bundle, arenaModelData[i].name);
                            arena.transform.SetParent(transform);
                            arena.SetActive(false);
                            arena.transform.localPosition = arenaModelData[i].position;
                            arena.transform.localEulerAngles = arenaModelData[i].rotation;
                            arena.transform.localScale = arenaModelData[i].scale;
                            arenaModelData[i].model = arena;
                            loadedArenaModels[arenaModelData[i]] = arenaModelData[i].model;
                        }
                        else if (!loadedArenaModels.ContainsKey(arenaModelData[i]))
                        { 
                            allArenaModelsLoaded = false;
                        }
                    }

                    if (originalArenaIndex >= 0 && originalArenaIndex < arenaModelData.Length)
                    {
                        arenaModelData[originalArenaIndex].model?.SetActive(true);
                    }
                    else
                    {
                        arenaModelData[0].model?.SetActive(true);
                    }
                }

                onLoad.Invoke();
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
            [AssetBundleName] public string bundle;
            public GameObject model;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;

            public ArenaModelData(string name, string bundle, GameObject model, Vector3 position, Vector3 rotation, Vector3 scale)
            {
                this.name = name;
                this.bundle = bundle;
                this.model = model;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }
    }

}