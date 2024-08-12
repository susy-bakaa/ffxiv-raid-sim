using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleShaderFade : MonoBehaviour
{
    public List<Material> materials;
    public float outValue = 0f;
    public float inValue = 1f;

    public void FadeOut(float time)
    {
        List<float> originalAlpha = new List<float>();

        for (int i = 0; i < materials.Count; i++)
        {
            originalAlpha.Add(materials[i].GetFloat("_Alpha"));

            //Debug.Log($"result {materials[i].GetFloat("_Alpha")} originalAlpha {originalAlpha.Count} current {originalAlpha[i]}");

            int index = i;
            if (time > 0)
                LeanTween.value(originalAlpha[i], outValue, time).setOnUpdate((float f) => materials[index].SetFloat("_Alpha", Mathf.Clamp01(f)));
            else
                materials[i].SetFloat("_Alpha", outValue);
        }
    }

    public void FadeIn(float time)
    {
        List<float> originalAlpha = new List<float>();

        for (int i = 0; i < materials.Count; i++)
        {
            originalAlpha.Add(materials[i].GetFloat("_Alpha"));

            //Debug.Log($"result {materials[i].GetFloat("_Alpha")} originalAlpha {originalAlpha.Count} current {originalAlpha[i]}");

            int index = i;
            if (time > 0)
                LeanTween.value(originalAlpha[i], inValue, time).setOnUpdate((float f) => materials[index].SetFloat("_Alpha", Mathf.Clamp01(f)));
            else
                materials[i].SetFloat("_Alpha", inValue);
        }
    }
}
