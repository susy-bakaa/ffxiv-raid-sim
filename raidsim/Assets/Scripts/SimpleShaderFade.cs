using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using NaughtyAttributes;
#endif
using UnityEngine;

public class SimpleShaderFade : MonoBehaviour
{
    private List<Material> materials;
    public float outValue = 0f;
    public float inValue = 1f;
    public bool outOnStart = false;
    public bool log = false;

    private int shaderAlphaPropertyHash = Shader.PropertyToID("_Alpha");

#if UNITY_EDITOR
    [Header("Editor")]
    public float fadeTime = 0.5f;

    [Button("Fade Out")]
    public void FadeOutButton()
    {
        FadeOut(fadeTime);
    }

    [Button("Fade In")]
    public void FadeInButton()
    {
        FadeIn(fadeTime);
    }
#endif

    private void Awake()
    {
        CreateLocalSharedMaterials();
        if (outOnStart)
            FadeOut(0);
    }

    private void CreateLocalSharedMaterials()
    {
        materials = new List<Material>();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Create a local shared material for this hierarchy
        Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();

        foreach (Renderer renderer in renderers)
        {
            Material[] rendererMaterials = renderer.sharedMaterials;
            for (int i = 0; i < rendererMaterials.Length; i++)
            {
                Material originalMaterial = rendererMaterials[i];

                // Check if we've already duplicated this material
                if (!materialMap.ContainsKey(originalMaterial))
                {
                    Material newMaterial = new Material(originalMaterial);
                    materialMap[originalMaterial] = newMaterial;
                }

                // Assign the locally shared material to the renderer
                rendererMaterials[i] = materialMap[originalMaterial];
            }

            // Update the renderer's materials to the locally shared ones
            renderer.sharedMaterials = rendererMaterials;

            // Add the materials to the list for later modification
            foreach (var mat in renderer.sharedMaterials)
            {
                if (!materials.Contains(mat) && mat.HasFloat(shaderAlphaPropertyHash))
                {
                    materials.Add(mat);
                }
            }
        }
    }

    public void FadeOut(float time)
    {
        if (log)
            Debug.Log($"[SimpleShaderFade ({gameObject.name})] FadeOut was called!");

        List<float> originalAlpha = new List<float>();

        for (int i = 0; i < materials.Count; i++)
        {
            originalAlpha.Add(materials[i].GetFloat(shaderAlphaPropertyHash));

            //Debug.Log($"result {materials[i].GetFloat("_Alpha")} originalAlpha {originalAlpha.Count} current {originalAlpha[i]}");

            int index = i;
            if (time > 0)
                LeanTween.value(originalAlpha[i], outValue, time).setOnUpdate((float f) => materials[index].SetFloat(shaderAlphaPropertyHash, Mathf.Clamp01(f)));
            else
                materials[i].SetFloat(shaderAlphaPropertyHash, outValue);
        }
    }

    public void FadeIn(float time)
    {
        if (log)
            Debug.Log($"[SimpleShaderFade ({gameObject.name})] FadeIn was called!");

        List<float> originalAlpha = new List<float>();

        for (int i = 0; i < materials.Count; i++)
        {
            originalAlpha.Add(materials[i].GetFloat(shaderAlphaPropertyHash));

            //Debug.Log($"result {materials[i].GetFloat("_Alpha")} originalAlpha {originalAlpha.Count} current {originalAlpha[i]}");

            int index = i;
            if (time > 0)
                LeanTween.value(originalAlpha[i], inValue, time).setOnUpdate((float f) => materials[index].SetFloat(shaderAlphaPropertyHash, Mathf.Clamp01(f)));
            else
                materials[i].SetFloat(shaderAlphaPropertyHash, inValue);
        }
    }
}
