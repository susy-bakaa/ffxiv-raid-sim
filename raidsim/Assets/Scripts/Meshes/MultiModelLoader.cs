// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dev.susybaka.raidsim.Core;

namespace dev.susybaka.raidsim.Animations
{
    public class MultiModelLoader : MonoBehaviour
    {
        // Legacy variables that are not used anymore but kept for compatibility
        // to avoid breaking existing scenes in other people's forks,
        // these can be migrated by using the MigrateOldModelData function (button) in the editor
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private string[] modelNames;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3[] positions;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3[] rotations;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private Vector3[] scales;
        [Obsolete("Use modelData instead. This field is kept for compatibility reasons. It may be removed in a future version.")]
        [SerializeField, HideInInspector] private GameObject[] models;
        // ---
        // This is the new data structure that holds the model datas
        [SerializeField] private ModelData[] modelData;

        private GameObject[] tempModels;
        private Coroutine ieUpdateModels;
        private ModelHandler modelHandler;
        private bool allModelsLoaded = false;
        private bool preventLoading = false;
        private Dictionary<ModelData, GameObject> loadedModels = new Dictionary<ModelData, GameObject>();

#if UNITY_EDITOR
#pragma warning disable CS0618 // Disable obsolete warning for modelNames, positions, rotations, scales, and models for the editor tool
        [NaughtyAttributes.Button]
        public void MigrateOldModelData()
        {
            if ((modelNames == null || modelNames.Length < 1) || (positions == null || positions.Length < 1) || (rotations == null || rotations.Length < 1) || (scales == null || scales.Length < 1) || (models == null || models.Length < 1))
            {
                Debug.LogWarning($"[MultiModelLoader ({transform.parent.gameObject.name}/{gameObject.name})] One or more model data arrays are null, migration cannot be performed!");
                return;
            }

            modelData = new ModelData[modelNames.Length];

            for (int i = 0; i < modelData.Length; i++)
            {
                ModelData temp = modelData[i];
                temp.name = modelNames[i];
                temp.position = positions[i];
                temp.rotation = rotations[i];
                temp.scale = scales[i];
                temp.model = models[i];
                modelData[i] = temp;
            }
        }
#pragma warning restore CS0618 // restore obsolete warning for the rest of the script
#endif

        private void Awake()
        {
            modelHandler = GetComponent<ModelHandler>();

            ieUpdateModels = null;
            tempModels = new GameObject[transform.childCount];
            loadedModels = new Dictionary<ModelData, GameObject>();

            for (int i = 0; i < transform.childCount; i++)
            {
                tempModels[i] = transform.GetChild(i).gameObject;
            }

            if (modelData == null || modelData.Length < 1)
            {
                preventLoading = true;
                Debug.LogWarning($"[MultiModelLoader ({transform.parent.gameObject.name}/{gameObject.name})] modelData structs array is empty or null, which is not allowed and loading will be prevented!");
                return;
            }
        }

        private void Update()
        {
            if (preventLoading)
                return;

            if (modelData == null || modelData.Length < 1)
                return;

            if (AssetHandler.Instance == null || allModelsLoaded)
                return;

            for (int i = 0; i < modelData.Length; i++)
            {
                if (AssetHandler.Instance.HasBundleLoaded(modelData[i].bundle) && !loadedModels.ContainsKey(modelData[i]))
                {
                    allModelsLoaded = true;

                    for (int t = 0; t < tempModels.Length; t++)
                    {
                        Destroy(tempModels[t]);
                    }

                    GameObject model = AssetHandler.Instance.GetAsset(modelData[i].bundle, modelData[i].name);
                    model.transform.SetParent(transform);
                    model.transform.localPosition = modelData[i].position;
                    model.transform.localEulerAngles = modelData[i].rotation;
                    model.transform.localScale = modelData[i].scale;
                    modelData[i].model = model;
                    modelData[i].model.SetActive(true);
                    loadedModels[modelData[i]] = modelData[i].model;

                    if (modelHandler != null)
                    {
                        if (ieUpdateModels == null)
                            ieUpdateModels = StartCoroutine(IE_UpdateModels(new WaitForSeconds(1f)));
                    }
                }
                else if (!loadedModels.ContainsKey(modelData[i]))
                {
                    allModelsLoaded = false;
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
}