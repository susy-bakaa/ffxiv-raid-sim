using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActive : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjects;
    [SerializeField] private bool state;
    [SerializeField] private float delay = 0.1f;

    private Coroutine ieSetActive;

    private void Start()
    {
        if (delay > 0f)
        {
            if (ieSetActive == null)
                ieSetActive = StartCoroutine(IE_SetActive(new WaitForSeconds(delay)));
        }
        else
        {
            SetActiveInternal();
        }
    }

    private IEnumerator IE_SetActive(WaitForSeconds wait)
    {
        yield return wait;
        SetActiveInternal();
        ieSetActive = null;
    }

    private void SetActiveInternal()
    {
        foreach (GameObject go in gameObjects)
        {
            go.SetActive(state);
        }
    }
}
