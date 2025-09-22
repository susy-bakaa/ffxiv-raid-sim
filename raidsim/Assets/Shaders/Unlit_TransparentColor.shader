// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/TransparentColor"
{
    Properties
    {
        [HideInInspector]
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}
        [MainColor]
        _Color ("Color", Color) = (1,1,1,1)
        _Emission ("Emission", Color) = (0,0,0,0)
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _DoubleSided ("Double Sided", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        Cull [_DoubleSided]

        CGPROGRAM
        // Use the Unlit shading model
        #pragma surface surf Unlit alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _Emission;
        float _Alpha;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Unlit lighting model
        half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            // Directly return the color without any lighting calculations
            return half4(s.Albedo, s.Alpha);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Emission = _Emission;

            o.Alpha = _Alpha * _Color.a;
            
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
