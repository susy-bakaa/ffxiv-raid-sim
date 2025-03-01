using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MultiModelLoader : MonoBehaviour
{
    [SerializeField] private string[] modelNames;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Vector3[] rotations;
    [SerializeField] private Vector3[] scales;
    [SerializeField] private GameObject[] models;

    private GameObject[] tempModels;
    private Coroutine ieUpdateModels;
    private ModelHandler modelHandler;
    private FightSelector fightSelector;
    private string currentFightBundleName;
    private bool modelsLoaded = false;
    private bool preventLoading = false;

    private void Awake()
    {
        modelHandler = GetComponent<ModelHandler>();
        fightSelector = FindObjectOfType<FightSelector>();

        ieUpdateModels = null;
        tempModels = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            tempModels[i] = transform.GetChild(i).gameObject;
        }

        models = new GameObject[modelNames.Length];

        if (modelNames.Length != positions.Length || modelNames.Length != rotations.Length || modelNames.Length != scales.Length)
        {
            preventLoading = true;
            Debug.LogWarning($"[MultiModelLoader ({transform.parent.gameObject.name}/{gameObject.name})] the amount of positions, rotations or scales does not match the amount of modelNames, which is not allowed and loading will be prevented!");
            return;
        }
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
        if (preventLoading)
            return;

        if (modelNames == null || modelNames.Length < 1)
            return;

        if (AssetHandler.Instance == null || modelsLoaded)
            return;

        if (AssetHandler.Instance.HasBundleLoaded(currentFightBundleName))
        {
            modelsLoaded = true;

            for (int i = 0; i < tempModels.Length; i++)
            {
                Destroy(tempModels[i]);
            }

            for (int i = 0; i < modelNames.Length; i++)
            {
                GameObject model = AssetHandler.Instance.GetAsset(modelNames[i]);
                model.transform.SetParent(transform);
                model.transform.localPosition = positions[i];
                model.transform.localEulerAngles = rotations[i];
                model.transform.localScale = scales[i];
                models[i] = model;
                models[i].SetActive(true);
            }

            if (modelHandler != null)
            {
                if (ieUpdateModels == null)
                    ieUpdateModels = StartCoroutine(IE_UpdateModels(new WaitForSeconds(1f)));
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
