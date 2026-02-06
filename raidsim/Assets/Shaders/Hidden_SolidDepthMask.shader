// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Hidden/SolidDepthMask"
{
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Optional alpha texture silhouette support
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UseAlphaTexMask;
            float _AlphaTexCutoff;     // fixed cutoff for mask

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float  eyeDepth : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
                o.eyeDepth = -viewPos.z;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Optional alpha mask so we don't tint whole quads for cutout textures
                if (_UseAlphaTexMask > 0.5)
                {
                    half a = tex2D(_MainTex, i.uv).a;
                    clip(a - _AlphaTexCutoff);
                }

                // Store eye depth in R. Clear RT should be 0.
                return half4(i.eyeDepth, 0, 0, 1);
            }
            ENDCG
        }
    }
}
