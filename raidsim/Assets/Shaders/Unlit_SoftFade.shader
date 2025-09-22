// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/SoftFade"
{
    Properties
    {
        [HideInInspector]
        [MainTexture]
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor]
        _Color ("Color", Color) = (1,1,1,1)
        _SoftFactor ("Soft Factor", Range(0.01, 1)) = 0.5
        _MinDist ("Min Distance", float) = 1.0
        _MaxDist ("Max Distance", float) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 projPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _SoftFactor;
            float _MinDist;
            float _MaxDist;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.projPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            sampler2D_float _CameraDepthTexture;
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the color
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Get screen depth
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.projPos.xy / i.projPos.w));
                float partDepth = i.projPos.z / i.projPos.w;
                
                // Compute soft fade factor with min/max distance
                float depthDiff = sceneDepth - partDepth;
                float fade = saturate((depthDiff - _MinDist) / (_MaxDist - _MinDist) * _SoftFactor);
                col.a *= fade;
                
                return col;
            }
            ENDCG
        }
    }
}