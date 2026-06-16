// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.Shared.Attributes;
using dev.susybaka.raidsim.Visuals;

namespace dev.susybaka.raidsim.Core
{
    public class EnvironmentHandler : MonoBehaviour
    {
        public bool log = false;

        public bool disableFogForWebGL = true;
        public bool disableFogForWindows = false;
        public bool disableFogForLinux = false;

        public int currentArenaIndex = 0;
        private int originalArenaIndex = -1;
        [Tooltip("If enabled, will fade between arena models when changing them instead of turning them on and off, this requires a SimpleShaderFade component to be present on each of the models and the materials have to support the standard '_Alpha' value for transparency.")]
        public bool fadeBetweenArenaModels = false;
        public Light[] environmentalLights;

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
        private bool onLoadInvoked = false;
        private Dictionary<ArenaModelData, GameObject> loadedArenaModels = new Dictionary<ArenaModelData, GameObject>();
        private Dictionary<GameObject, SimpleShaderFade> arenaShaderFades = new Dictionary<GameObject, SimpleShaderFade>();

#if UNITY_EDITOR
        [Header("Editor")]
        public int editorArenaIndex = 0;
        [NaughtyAttributes.Button("Load Arena")]
        public void ChangeArenaButton()
        {
            ChangeArenaModel(editorArenaIndex);
        }

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
                temp.collision = null; // No collision data in old format
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
#if (UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_EDITOR_WEBGL) && !UNITY_EDITOR_WIN && !UNITY_EDITOR_LINUX && !UNITY_EDITOR_OSX
            if (disableFogForWebGL)
            {
                RenderSettings.fog = false;
            }
#endif
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && !UNITY_EDITOR_LINUX && !UNITY_EDITOR_WEBGL && !UNITY_EDITOR_OSX
            if (disableFogForWindows)
            {
                RenderSettings.fog = false;
            }
#endif
#if (UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX) && !UNITY_EDITOR_WIN && !UNITY_EDITOR_WEBGL && !UNITY_EDITOR_OSX
            if (disableFogForLinux)
            {
                RenderSettings.fog = false;
            }
#endif
            originalArenaIndex = currentArenaIndex;
            ChangeArenaModel(originalArenaIndex);
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

            if (AssetHandler.Instance == null)
                return;

            bool allLoaded = true;

            // Remove any possible temporary arena placeholders
            for (int i = 0; i < arenaModelData.Length; i++)
            {
                bool isLoaded = loadedArenaModels.ContainsKey(arenaModelData[i]);

                if (!isLoaded)
                {
                    // Skip loading if no bundle is assigned or if the bundle is set to "<None>" to allow for temporary arenas to coexist with final ones
                    if (string.IsNullOrEmpty(arenaModelData[i].bundle) || arenaModelData[i].bundle == "<None>")
                    {
                        if (log)
                            Debug.Log($"[EnvironmentHandler] Arena model at index {i} has no asset bundle assigned, skipping...");
                        continue;
                    }

                    if (!AssetHandler.Instance.HasBundleLoaded(arenaModelData[i].bundle))
                    {
                        allLoaded = false;
                        continue;
                    }

                    if (string.IsNullOrEmpty(arenaModelData[i].name))
                    {
                        allLoaded = false;

                        if (log)
                            Debug.LogWarning($"[EnvironmentHandler] Arena model at index {i} has no asset name assigned.");

                        continue;
                    }

                    if (arenaModelData[i].model != null)
                    {
                        Destroy(arenaModelData[i].model);
                    }

                    GameObject arena = AssetHandler.Instance.GetAsset(arenaModelData[i].bundle, arenaModelData[i].name);

                    if (arena == null)
                    {
                        allLoaded = false;

                        if (log)
                            Debug.LogWarning($"[EnvironmentHandler] Failed to load arena model '{arenaModelData[i].name}' from bundle '{arenaModelData[i].bundle}'.");

                        continue;
                    }

                    arena.transform.SetParent(transform);

                    if (!fadeBetweenArenaModels)
                        arena.SetActive(false);

                    arena.transform.localPosition = arenaModelData[i].position;
                    arena.transform.localEulerAngles = arenaModelData[i].rotation;
                    arena.transform.localScale = arenaModelData[i].scale;

                    arenaModelData[i].model = arena;
                    loadedArenaModels[arenaModelData[i]] = arena;

                    if (arenaModelData[i].collision != null)
                        arenaModelData[i].collision.SetActive(false);

                    if (log)
                        Debug.Log($"[EnvironmentHandler] Loaded arena model '{arenaModelData[i].name}' from bundle '{arenaModelData[i].bundle}'.");

                    ChangeArenaModel(currentArenaIndex);
                }

                if (!loadedArenaModels.ContainsKey(arenaModelData[i]))
                    allLoaded = false;
            }

            allArenaModelsLoaded = allLoaded;

            if (allArenaModelsLoaded)
            {
                if (currentArenaIndex < 0 || currentArenaIndex >= arenaModelData.Length)
                {
                    if (log)
                        Debug.LogWarning($"[EnvironmentHandler] Current arena index {currentArenaIndex} is invalid. Falling back to original arena index {originalArenaIndex}.");

                    currentArenaIndex = originalArenaIndex;
                }

                if (currentArenaIndex < 0 || currentArenaIndex >= arenaModelData.Length)
                    currentArenaIndex = 0;

                ChangeArenaModel(currentArenaIndex);

                if (!onLoadInvoked)
                {
                    onLoad.Invoke();
                    onLoadInvoked = true;
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

            if (index < 0 || index >= arenaModelData.Length)
            {
                int originalIndex = index;

                index = Mathf.Min(Mathf.Max(0, index), arenaModelData.Length - 1);

                if (log)
                    Debug.LogWarning($"[EnvironmentHandler] Invalid arena index {originalIndex}. Falling back to {index}.");
            }

            currentArenaIndex = index;

            for (int i = 0; i < arenaModelData.Length; i++)
            {
                if (fadeBetweenArenaModels)
                {
                    if (log)
                        Debug.Log($"[EnvironmentHandler] Fading arena model: {arenaModelData[i].name} to {i == index}.");

                    if (arenaShaderFades.TryGetValue(arenaModelData[i].model, out SimpleShaderFade shaderFade))
                    {
                        if (shaderFade != null)
                        {
                            if (log)
                                Debug.Log($"[EnvironmentHandler] Found existing SimpleShaderFade component for arena model: {arenaModelData[i].name}.");

                            if (i == index)
                            {
                                if (log)
                                    Debug.Log($"[EnvironmentHandler] Fading in arena model: {arenaModelData[i].name}.");
                                shaderFade.FadeIn();
                            }
                            else
                            {
                                if (log)
                                    Debug.Log($"[EnvironmentHandler] Fading out arena model: {arenaModelData[i].name}.");
                                shaderFade.FadeOut();
                            }
                        }
                        else
                        {
                            if (log)
                                Debug.LogWarning($"[EnvironmentHandler] No SimpleShaderFade component found for arena model: {arenaModelData[i].name}, falling back to simple enable/disable.");
                            // If no SimpleShaderFade component is found, fallback to simple enable/disable
                            arenaModelData[i].model.SetActive(i == index);
                        }
                    }
                    else if (arenaModelData[i].model != null)
                    {
                        SimpleShaderFade newShaderFade = arenaModelData[i].model.GetComponent<SimpleShaderFade>();
                        arenaShaderFades[arenaModelData[i].model] = newShaderFade;

                        if (newShaderFade != null)
                        {
                            if (log)
                                Debug.Log($"[EnvironmentHandler] Added SimpleShaderFade component for arena model: {arenaModelData[i].name} to dictionary.");

                            if (i == index)
                            {
                                if (log)
                                    Debug.Log($"[EnvironmentHandler] Fading in arena model: {arenaModelData[i].name}.");
                                newShaderFade.FadeIn();
                            }
                            else
                            {
                                if (log)
                                    Debug.Log($"[EnvironmentHandler] Fading out arena model: {arenaModelData[i].name}.");
                                newShaderFade.FadeOut();
                            }
                        }
                        else
                        {
                            if (log)
                                Debug.LogWarning($"[EnvironmentHandler] No SimpleShaderFade component found for arena model: {arenaModelData[i].name}, falling back to simple enable/disable.");

                            // If no SimpleShaderFade component is found, fallback to simple enable/disable
                            arenaModelData[i].model.SetActive(i == index);
                        }
                    }
                    else if (arenaModelData[i].model == null && log)
                    {
                        Debug.LogWarning($"[EnvironmentHandler] Arena model at index {i} is missing, skipping fade and active state change.");
                    }
        }
                else
                {
                    if (log)
                        Debug.Log($"[EnvironmentHandler] Setting arena model: {arenaModelData[i].name} active state to {i == index}.");
                    if (arenaModelData[i].model != null)
                    {
                        arenaModelData[i].model.SetActive(i == index);
                    }
                    else if (log)
                    {
                        Debug.LogWarning($"[EnvironmentHandler] Arena model at index {i} is missing, skipping active state change.");
                    }
                }
                // Check if this arena data has separate collisions defined and enable/disable them accordingly
                if (log)
                {
                    string collisionName = arenaModelData[i].collision != null ? arenaModelData[i].collision.name : "None";
                    string collisionState = i == index ? "Enabling" : "Disabling";

                    Debug.Log($"[EnvironmentHandler] {collisionState} collision ({collisionName}) for arena model: {arenaModelData[i].name}.");
                }

                if (arenaModelData[i].collision != null)
                    arenaModelData[i].collision.SetActive(i == index);

                if (i == index)
                {
                    if (log)
                        Debug.Log($"[EnvironmentHandler] Setting environmental light color for arena model: {arenaModelData[i].name}.");

                    if (environmentalLights == null || environmentalLights.Length < 1)
                    {
                        if (log)
                            Debug.LogWarning($"[EnvironmentHandler] No environmental lights assigned, skipping ambient light color change for arena model: {arenaModelData[i].name}.");
                        continue;
                    }

                    for (int j = 0; j < environmentalLights.Length; j++)
                    {
                        environmentalLights[j].color = arenaModelData[i].ambientLightColor;
                    }
                }
            }
        }

        [System.Serializable]
        public struct ArenaModelData
        {
            public string name;
            [AssetBundleName] public string bundle;
            public GameObject model;
            public GameObject collision;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
            public Color ambientLightColor;

            public ArenaModelData(string name, string bundle, GameObject model, GameObject collision, Vector3 position, Vector3 rotation, Vector3 scale, Color ambientLightColor)
            {
                this.name = name;
                this.bundle = bundle;
                this.model = model;
                this.collision = collision;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
                this.ambientLightColor = ambientLightColor;
            }
        }
    }

}