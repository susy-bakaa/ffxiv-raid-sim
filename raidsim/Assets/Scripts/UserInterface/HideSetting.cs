using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideSetting : MonoBehaviour
{
    public Toggle toggle;
    public GameObject target;

    private void Update()
    {
        if (toggle.isOn)
        {
            target.SetActive(true);
        }
        else
        {
            target.SetActive(false);
        }
    }
}
