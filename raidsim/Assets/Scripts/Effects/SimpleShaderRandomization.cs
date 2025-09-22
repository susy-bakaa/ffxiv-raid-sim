// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using UnityEngine;

namespace dev.susybaka.raidsim.Visuals
{
    [RequireComponent(typeof(SimpleShaderFade))]
    public class SimpleShaderRandomization : MonoBehaviour
    {
        SimpleShaderFade shaderFade;

        [SerializeField] private bool log = false;
        [SerializeField] private bool randomizeOnStart = false;
        [SerializeField] private List<ShaderPropertyInfo> randomizedShaderProperties = new List<ShaderPropertyInfo>();

        private Dictionary<string, int> shaderPropertyHashes = new Dictionary<string, int>();
        private Dictionary<string, ShaderPropertyInfo> originalShaderProperties = new Dictionary<string, ShaderPropertyInfo>();

        private void Awake()
        {
            shaderFade = GetComponent<SimpleShaderFade>();

            shaderPropertyHashes = new Dictionary<string, int>();
            for (int i = 0; i < randomizedShaderProperties.Count; i++)
            {
                if (shaderPropertyHashes.ContainsKey(randomizedShaderProperties[i].name))
                {
                    Debug.LogWarning($"Shader property '{randomizedShaderProperties[i].name}' is already defined. Skipping duplicate.");
                    continue;
                }

                shaderPropertyHashes.Add(randomizedShaderProperties[i].name, Shader.PropertyToID(randomizedShaderProperties[i].name));
            }
        }

        private void Start()
        {
            originalShaderProperties = new Dictionary<string, ShaderPropertyInfo>();
            foreach (ShaderPropertyInfo property in randomizedShaderProperties)
            {
                for (int i = 0; i < shaderFade.Materials.Count; i++)
                {
                    switch (property.type)
                    {
                        case PropertyType.Float:
                            originalShaderProperties.Add(property.name, new ShaderPropertyInfo(property.name, property.type, new Vector4(shaderFade.Materials[i].GetFloat(shaderPropertyHashes[property.name]), 0, 0, 0)));
                            break;
                        case PropertyType.Int:
                            originalShaderProperties.Add(property.name, new ShaderPropertyInfo(property.name, property.type, new Vector4(shaderFade.Materials[i].GetInt(shaderPropertyHashes[property.name]), 0, 0, 0)));
                            break;
                        case PropertyType.Color:
                            originalShaderProperties.Add(property.name, new ShaderPropertyInfo(property.name, property.type, shaderFade.Materials[i].GetColor(shaderPropertyHashes[property.name])));
                            break;
                        default:
                            originalShaderProperties.Add(property.name, new ShaderPropertyInfo(property.name, property.type, shaderFade.Materials[i].GetVector(shaderPropertyHashes[property.name])));
                            break;
                    }
                }              
            }

            if (randomizeOnStart)
            {
                RandomizeShaderProperties();
            }
        }

        public void RandomizeShaderProperties()
        {
            foreach (ShaderPropertyInfo property in randomizedShaderProperties)
            {
                for (int i = 0; i < shaderFade.Materials.Count; i++)
                {
                    if (log)
                        Debug.Log($"Randomizing shader property '{property.name}' on material {i} with type {property.type}");

                    switch (property.type)
                    {
                        case PropertyType.Float:
                            shaderFade.Materials[i].SetFloat(shaderPropertyHashes[property.name], Random.Range(property.minValue.x, property.maxValue.x));
                            break;
                        case PropertyType.Int:
                            shaderFade.Materials[i].SetInt(shaderPropertyHashes[property.name], Random.Range((int)Mathf.Round(property.minValue.x), (int)Mathf.Round(property.maxValue.x)));
                            break;
                        case PropertyType.Color:
                            shaderFade.Materials[i].SetColor(shaderPropertyHashes[property.name], new Color(
                                Random.Range(property.minValue.x, property.maxValue.x),
                                Random.Range(property.minValue.y, property.maxValue.y),
                                Random.Range(property.minValue.z, property.maxValue.z),
                                Random.Range(property.minValue.w, property.maxValue.w)));
                            break;
                        default:
                            shaderFade.Materials[i].SetVector(shaderPropertyHashes[property.name], new Vector4(
                                Random.Range(property.minValue.x, property.maxValue.x),
                                Random.Range(property.minValue.y, property.maxValue.y),
                                Random.Range(property.minValue.z, property.maxValue.z),
                                Random.Range(property.minValue.w, property.maxValue.w)));
                            break;
                    }

                }
            }
        }

        public void ResetShaderProperties()
        {
            foreach (ShaderPropertyInfo property in randomizedShaderProperties)
            {
                for (int i = 0; i < shaderFade.Materials.Count; i++)
                {
                    if (log)
                        Debug.Log($"Resetting shader property '{property.name}' on material {i} with type {property.type} to its original value");

                    switch (property.type)
                    {
                        case PropertyType.Float:
                            shaderFade.Materials[i].SetFloat(shaderPropertyHashes[property.name], originalShaderProperties[property.name].minValue.x);
                            break;
                        case PropertyType.Int:
                            shaderFade.Materials[i].SetInt(shaderPropertyHashes[property.name], (int)Mathf.Round(originalShaderProperties[property.name].minValue.x));
                            break;
                        case PropertyType.Color:
                            shaderFade.Materials[i].SetColor(shaderPropertyHashes[property.name], originalShaderProperties[property.name].minValue);
                            break;
                        default:
                            shaderFade.Materials[i].SetVector(shaderPropertyHashes[property.name], originalShaderProperties[property.name].minValue);
                            break;
                    }
                }
            }
        }

        public enum PropertyType
        {
            Float,
            Int,
            Vector2,
            Vector3,
            Vector4,
            Color
        }
        [System.Serializable]
        public struct ShaderPropertyInfo
        {
            public string name;
            public PropertyType type;
            public Vector4 minValue;
            public Vector4 maxValue;

            public ShaderPropertyInfo(string name, PropertyType type, Vector4 minValue, Vector4 maxValue)
            {
                this.name = name;
                this.type = type;
                this.minValue = minValue;
                this.maxValue = maxValue;
            }

            public ShaderPropertyInfo(string name, PropertyType type, Vector4 value)
            {
                this.name = name;
                this.type = type;
                this.minValue = value;
                this.maxValue = value;
            }
        }
    }
}