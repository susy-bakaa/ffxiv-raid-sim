using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveLinker : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool inverse = false;

    private void OnEnable()
    {
        if (inverse)
            target.SetActive(false);
        else
            target.SetActive(true);
    }

    private void OnDisable()
    {
        if (inverse)
            target.SetActive(true);
        else
            target.SetActive(false);
    }
}
