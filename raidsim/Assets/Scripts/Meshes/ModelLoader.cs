using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModelLoader : MonoBehaviour
{
    [SerializeField] private string modelName = string.Empty;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private Vector3 rotation = Vector3.zero;
    [SerializeField] private Vector3 scale = Vector3.one;
    [SerializeField] private GameObject model;
    [SerializeField] private UnityEvent onModelLoaded;

    private GameObject tempModel;
    private Coroutine ieUpdateModels;
    private ModelHandler modelHandler;
    private FightSelector fightSelector;
    private string currentFightBundleName;
    private bool modelLoaded = false;

    private void Awake()
    {
        fightSelector = FindObjectOfType<FightSelector>();
        modelHandler = GetComponent<ModelHandler>();
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
                onModelLoaded.Invoke();
                modelLoaded = true;

                Destroy(tempModel);

                GameObject model = AssetHandler.Instance.GetAsset(modelName);
                model.transform.SetParent(transform);
                model.transform.localPosition = position;
                model.transform.localEulerAngles = rotation;
                model.transform.localScale = scale;
                this.model = model;
                this.model.SetActive(true);

                if (modelHandler != null)
                {
                    if (ieUpdateModels == null)
                        ieUpdateModels = StartCoroutine(IE_UpdateModels(new WaitForSeconds(1f)));
                }
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
