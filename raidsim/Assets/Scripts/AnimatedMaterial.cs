using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AnimatedMaterial : MonoBehaviour
{
    public string shaderParameterName = "_MainTex"; // Shader parameter name
    public Vector2 speed = new Vector2(1f, 1f); // Animation speed
    private int shaderParameterHash;
    private Material material;
    private Vector2 currentOffset; // Accumulated offset

    void Awake()
    {
        shaderParameterHash = Shader.PropertyToID(shaderParameterName);
        material = GetComponent<Renderer>().material; // Use material instead of sharedMaterial to avoid affecting all instances
    }

    void Update()
    {
        if (speed == Vector2.zero)
            return;

        // Increment the offset based on speed and elapsed time
        currentOffset += Time.deltaTime * speed;

        // Apply the new offset to the material
        material.SetTextureOffset(shaderParameterHash, currentOffset);
    }
}