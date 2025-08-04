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

            if (AssetHandler.Instance != null && !modelLoaded)
            {
                if (AssetHandler.Instance.HasBundleLoaded(modelData.bundle))
                {
                    onModelLoaded.Invoke();
                    modelLoaded = true;

                    Destroy(tempModel);

                    GameObject model = AssetHandler.Instance.GetAsset(modelData.bundle, modelData.name);
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