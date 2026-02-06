// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using dev.susybaka.Shared;

namespace dev.susybaka.raidsim.Visuals
{
    public class SimpleShaderFade : MonoBehaviour
    {
        private SimpleShaderFade parentFade;
        private List<Material> materials;
        public List<Material> Materials
        {
            get
            {
                if (materials == null || materials.Count == 0)
                {
                    if (parentFade == null || parentFade == this)
                    {
                        parentFade = null;
                        initialized = true;
                        CreateLocalSharedMaterials();
                    }
                    else if (parentFade != null && parentFade != this)
                    {
                        initialized = true;
                        CopyLocalSharedMaterials();
                    }
                }
                return materials;
            }
        }
        private Dictionary<Material, Material> materialDictionary = new Dictionary<Material, Material>();
        private Queue<Action> fadeQueue = new Queue<Action>();
        private bool initialized = false;
        public float outValue = 0f;
        public float inValue = 1f;
        public float defaultFadeTime = 0.33f;
        public bool outOnStart = false;
        public bool log = false;

        [Foldout("Advanced")] public string shaderAlphaPropertyName = "_Alpha";

        private int shaderAlphaPropertyHash = Shader.PropertyToID("_Alpha");

#if UNITY_EDITOR
        [Header("Editor")]
        public float fadeTime = 0.5f;
        private SimpleScale simpleScale;

        [Button("Fade Out")]
        public void FadeOutButton()
        {
            simpleScale = GetComponent<SimpleScale>();
            FadeOut(fadeTime);
            if (simpleScale != null)
            {
                simpleScale.ResetScale();
            }
        }

        [Button("Fade In")]
        public void FadeInButton()
        {
            simpleScale = GetComponent<SimpleScale>();
            FadeIn(fadeTime);
            if (simpleScale != null)
            {
                simpleScale.Scale();
            }
        }
#endif

        public Material GetMaterial(int index)
        {
            if (materials != null && materials.Count > 0)
                return materials[index];
            else
            {
                if (parentFade == null || parentFade == this)
                {
                    parentFade = null;
                    CreateLocalSharedMaterials();
                    initialized = true;
                    return GetMaterial(index);
                }
                else if (parentFade != null && parentFade != this)
                {
                    CopyLocalSharedMaterials();
                    initialized = true;
                    return GetMaterial(index);
                }
                else
                {
                    return null;
                }
            }
        }

        private void Awake()
        {
            parentFade = transform.GetComponentInParents<SimpleShaderFade>();
            if (log)
                Debug.Log($"[SimpleShaderFade ({gameObject.name})] Parent Fade: {(parentFade != null ? parentFade.gameObject.name : "null")}");
            shaderAlphaPropertyHash = Shader.PropertyToID(shaderAlphaPropertyName);
            if (parentFade == null || parentFade == this)
            {
                parentFade = null;
                CreateLocalSharedMaterials();

                if (outOnStart)
                    FadeOut(0);

                initialized = true;
            }
        }

        // For now we use Start to copy materials from parent fades to ensure all Awake calls are done
        // Should be revisited if we run into issues with timing later with multiple fades in a hierarchy
        private void Start()
        {
            if (parentFade != null && parentFade != this)
            {
                CopyLocalSharedMaterials();

                if (outOnStart)
                    FadeOut(0);

                initialized = true;
            }
        }

        private void Update()
        {
            if (initialized && fadeQueue != null && fadeQueue.Count > 0)
            {
                Action fadeAction = fadeQueue.Dequeue();
                fadeAction?.Invoke();
            }
        }

        private void CreateLocalSharedMaterials()
        {
            materials = new List<Material>();
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            // Create a local shared material for this hierarchy
            materialDictionary = new Dictionary<Material, Material>();

            foreach (Renderer renderer in renderers)
            {
                Material[] rendererMaterials = renderer.sharedMaterials;
                for (int i = 0; i < rendererMaterials.Length; i++)
                {
                    Material originalMaterial = rendererMaterials[i];

                    // Check if we've already duplicated this material
                    if (!materialDictionary.ContainsKey(originalMaterial))
                    {
                        Material newMaterial = new Material(originalMaterial);
                        materialDictionary[originalMaterial] = newMaterial;
                    }

                    // Assign the locally shared material to the renderer
                    rendererMaterials[i] = materialDictionary[originalMaterial];
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

        private void CopyLocalSharedMaterials()
        {
            if (parentFade.materials == null || parentFade.materials.Count < 1)
            {
                parentFade.Awake();
            }

            materials = new List<Material>();
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            // Copy the existing local shared material for this hierarchy
            materialDictionary = parentFade.materialDictionary;

            foreach (Renderer renderer in renderers)
            {
                Material[] rendererMaterials = renderer.sharedMaterials;
                for (int i = 0; i < rendererMaterials.Length; i++)
                {
                    for (int j = 0; j < parentFade.materials.Count; j++)
                    {
                        if (rendererMaterials[i] == parentFade.materials[j])
                        {
                            // Assign the locally shared material to the renderer from the parent fade
                            rendererMaterials[i] = parentFade.materials[j];
                            break;
                        }
                    }
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

        public void FadeOut()
        {
            FadeOutInternal(defaultFadeTime, null);
        }

        public void FadeOut(float time)
        {
            FadeOutInternal(time, null);
        }

        public void FadeOut(Action callback)
        {
            FadeOutInternal(defaultFadeTime, callback);
        }

        public void FadeOut(float time, Action callback)
        {
            FadeOutInternal(time, callback);
        }

        private void FadeOutInternal(float time, Action callback)
        {
            if (log)
                Debug.Log($"[SimpleShaderFade ({gameObject.name})] FadeOut was called!");

            if (!initialized && (materials == null || materials.Count < 1))
            {
                fadeQueue.Enqueue(() => FadeOutInternal(time, callback));
                return;
            }

            List<float> originalAlpha = new List<float>();

            //Debug.Log($"[SimpleShaderFade ({gameObject.name})] Fading out {(materials != null ? materials.Count : "null")} materials. initialized = {initialized}", gameObject);

            for (int i = 0; i < materials.Count; i++)
            {
                originalAlpha.Add(materials[i].GetFloat(shaderAlphaPropertyHash));

                if (log)
                    Debug.Log($"result {materials[i].GetFloat("_Alpha")} originalAlpha {originalAlpha.Count} current {originalAlpha[i]}");

                int index = i;
                if (time > 0)
                    LeanTween.value(originalAlpha[i], outValue, time).setOnUpdate((float f) => materials[index].SetFloat(shaderAlphaPropertyHash, Mathf.Clamp01(f))).setOnComplete(callback);
                else
                {
                    materials[i].SetFloat(shaderAlphaPropertyHash, outValue);
                    callback?.Invoke();
                }
            }
        }

        public void FadeIn()
        {
            FadeInInternal(defaultFadeTime, null);
        }

        public void FadeIn(float time)
        {
            FadeInInternal(time, null);
        }

        public void FadeIn(Action callback)
        {
            FadeInInternal(defaultFadeTime, callback);
        }

        public void FadeIn(float time, Action callback)
        {
            FadeInInternal(time, callback);
        }

        private void FadeInInternal(float time, Action callback)
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
                    LeanTween.value(originalAlpha[i], inValue, time).setOnUpdate((float f) => materials[index].SetFloat(shaderAlphaPropertyHash, Mathf.Clamp01(f))).setOnComplete(callback);
                else
                {
                    materials[i].SetFloat(shaderAlphaPropertyHash, inValue);
                    callback?.Invoke();
                }
            }
        }
    }
}