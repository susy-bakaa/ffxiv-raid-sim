using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelLoader : MonoBehaviour
{
    [SerializeField] private string modelName = string.Empty;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Vector3 rotation = Vector3.zero;
    [SerializeField] private Vector3 scale = Vector3.one;
    [SerializeField] private GameObject model;

    private GameObject tempModel;
    private FightSelector fightSelector;
    private string currentFightBundleName;
    private bool modelLoaded = false;

    private void Awake()
    {
        fightSelector = FindObjectOfType<FightSelector>();
        tempModel = transform.GetChild(0).gameObject;
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
        if (string.IsNullOrEmpty(modelName))
            return;

        if (AssetHandler.Instance != null && !modelLoaded)
        {
            if (AssetHandler.Instance.HasBundleLoaded(currentFightBundleName))
            {
                modelLoaded = true;

                Destroy(tempModel);

                GameObject model = AssetHandler.Instance.GetAsset(modelName);
                model.transform.SetParent(transform);
                model.transform.localPosition = position;
                model.transform.localEulerAngles = rotation;
                model.transform.localScale = scale;
                this.model = model;
                this.model.SetActive(true);
            }
        }
    }
}
