// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using dev.susybaka.raidsim.Core;
using dev.susybaka.Shared.Attributes;
using System;

namespace dev.susybaka.raidsim.Animations
{
    public class ModelLoader : MonoBehaviour
    {
        [Tooltip("Allow loading models directly from the Assets folder in the editor. This is useful for testing and development purposes, but should be disabled in production builds.")]
        [SerializeField] private bool allowEditorDirectModelLoading = false;

        // Legacy variables that are not used anymore but kept for compatibility
        // to avoid breaking existing scenes in other people's forks,
        // these can be migrated by using the MigrateOldModelData function (button) in the editor
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private string modelName = string.Empty;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3 position = Vector3.zero;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3 rotation = Vector3.zero;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3 scale = Vector3.one;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private GameObject model;
        // ---
        // This is the new data structure that holds the model data
        [SerializeField] private ModelData modelData;
        [SerializeField] private UnityEvent onModelLoaded;

        private GameObject tempModel;
        private Coroutine ieUpdateModels;
        private ModelHandler modelHandler;
        //private FightSelector fightSelector;
        //private string[] currentFightBundleNames;
        private bool modelLoaded = false;

#if UNITY_EDITOR
#pragma warning disable CS0618 // Disable obsolete warning for modelName, position, rotation, scale, and model for the editor tool
        [NaughtyAttributes.Button]
        public void MigrateOldModelData()
        {
            if (modelName == string.Empty)
            {
                Debug.LogWarning($"[ModelLoader ({transform.parent.gameObject.name}/{gameObject.name})] Model name is empty, migration cannot be performed!");
                return;
            }

            modelData = new ModelData
            {
                name = modelName,
                bundle = string.Empty,
                model = model,
                position = position,
                rotation = rotation,
                scale = scale
            };
        }
#pragma warning restore CS0618 // restore obsolete warning for the rest of the script
#endif

        private void Awake()
        {
            //fightSelector = FindObjectOfType<FightSelector>();
            modelHandler = GetComponent<ModelHandler>();
            tempModel = transform.GetChild(0).gameObject;
#if !UNITY_EDITOR
            allowEditorDirectModelLoading = false;
#endif
        }

        //private void Start()
        //{
        //if (fightSelector != null)
        //{
        //    currentFightBundleNames = fightSelector.CurrentScene.assetBundles;
        //}
        //}

        private void Update()
        {
            if (string.IsNullOrEmpty(modelData.name) || string.IsNullOrEmpty(modelData.bundle))
                return;

#if UNITY_EDITOR
            if (((AssetHandler.Instance != null) || (AssetHandler.Instance == null && allowEditorDirectModelLoading && GlobalVariables.isEditor)) && !modelLoaded)
            {
                if ((AssetHandler.Instance != null && AssetHandler.Instance.HasBundleLoaded(modelData.bundle)) || (allowEditorDirectModelLoading && GlobalVariables.isEditor))
                {
#else
            if (AssetHandler.Instance != null && !modelLoaded)
            {
                if (AssetHandler.Instance.HasBundleLoaded(modelData.bundle))
                {
#endif
                    //onModelLoaded.Invoke();
                    modelLoaded = true;

                    Destroy(tempModel);

#if UNITY_EDITOR
                    //Debug.Log($"Assets/FFXIV/Fights/{FightTimeline.Instance.timelineAbbreviation}/Prefabs/{modelData.name}");
                    // Load the model directly from the Assets folder in the editor
                    GameObject model = null;
                    if (AssetHandler.Instance != null && AssetHandler.Instance.HasBundleLoaded(modelData.bundle) && !allowEditorDirectModelLoading)
                    {
                        model = AssetHandler.Instance.GetAsset(modelData.bundle, modelData.name);
                    }
                    else if (allowEditorDirectModelLoading && GlobalVariables.isEditor)
                    {
                        model = GameObject.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/FFXIV/Fights/{FightTimeline.Instance.timelineAbbreviation}/Prefabs/{modelData.name}"));
                    }
#else
                    GameObject model = AssetHandler.Instance.GetAsset(modelData.bundle, modelData.name);
#endif

                    model.transform.SetParent(transform);
                    model.transform.localPosition = modelData.position;
                    model.transform.localEulerAngles = modelData.rotation;
                    model.transform.localScale = modelData.scale;
                    modelData.model = model;
                    modelData.model.SetActive(true);

                    if (modelHandler != null)
                    {
                        if (ieUpdateModels == null)
                            ieUpdateModels = StartCoroutine(IE_UpdateModels(new WaitForSeconds(1f)));
                    }
                    // Moved the onModelLoaded.Invoke() call here to ensure it is called after the model is loaded and set up
                    // Hopefully this doesn't break anything from before
                    onModelLoaded.Invoke();
                }
                else
                {
                    modelLoaded = false;
                }
            }
        }

        private IEnumerator IE_UpdateModels(WaitForSeconds wait)
        {
            yield return wait;
            modelHandler.UpdateModels();
            ieUpdateModels = null;
        }
    }

    [System.Serializable]
    public struct ModelData
    {
        public string name;
        [AssetBundleName] public string bundle;
        public GameObject model;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public ModelData(string name, string bundle, GameObject model, Vector3 position, Vector3 rotation, Vector3 scale)
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