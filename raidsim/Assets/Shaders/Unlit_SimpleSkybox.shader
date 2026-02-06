// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/SimpleSkybox"
{
    Properties
    {
        [MainTexture]
        _MainTexture ("Sky Texture", 2D) = "white" {}
        [Toggle] _UseTexture ("Use Texture", Float) = 0
        [MainColor]
        _MainColor ("Top Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _SecondaryColor ("Bottom Color", Color) = (1.0, 0.5, 0.0, 1.0)
        _MixFactor ("Mix", Range(0, 1)) = 0.5
        _Alpha ("Alpha", Range(0,1)) = 1.0
        [Toggle] _UseDither ("Use Dithering", Float) = 0
        _DitherScale ("Dither Scale", Range(0.25, 8)) = 1
        [Enum(Off,0,Front,1,Back,2)]
        _DoubleSided ("Cull", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Transparent" }
        Pass
        {
            Cull [_DoubleSided]

            AlphaToMask On

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float4 _MainColor;
            float4 _SecondaryColor;
            float _MixFactor;

            sampler2D _MainTexture;
            float4 _MainTexture_ST;
            float _UseTexture;

            float _Alpha;
            float _UseDither;
            float _DitherScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.position);
                return o;
            }

            float Bayer4x4(float2 pixel)
            {
                int2 p = int2(pixel) & 3;
                int idx = p.x + p.y * 4;

                // 4x4 Bayer matrix values / 16:
                //  0  8  2 10
                // 12  4 14  6
                //  3 11  1  9
                // 15  7 13  5
                float v = 0.0;
                if      (idx == 0)  v = 0;
                else if (idx == 1)  v = 8;
                else if (idx == 2)  v = 2;
                else if (idx == 3)  v = 10;
                else if (idx == 4)  v = 12;
                else if (idx == 5)  v = 4;
                else if (idx == 6)  v = 14;
                else if (idx == 7)  v = 6;
                else if (idx == 8)  v = 3;
                else if (idx == 9)  v = 11;
                else if (idx == 10) v = 1;
                else if (idx == 11) v = 9;
                else if (idx == 12) v = 15;
                else if (idx == 13) v = 7;
                else if (idx == 14) v = 13;
                else                v = 5;

                return v / 16.0;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Normalize world direction
                float3 worldDir = normalize(i.worldPos);

                // Map y from [-1, 1] to [0, 1] for interpolation
                float t = saturate(worldDir.y * 0.5 + 0.5);

                // Adjust the interpolation factor by the mix factor
                t = lerp(0.5, t, _MixFactor);

                // Interpolate between bottom and top colors
                float4 col = lerp(_SecondaryColor, _MainColor, t);

                // Build alpha from gradient alpha and the explicit alpha control
                float outAlpha = col.a * _Alpha;

                if (_UseTexture > 0.5)
                {
                    // Lat-long mapping from direction -> UV
                    const float PI = 3.14159265359;
                    const float TWO_PI = 6.28318530718;

                    float u = 0.5 + atan2(worldDir.z, worldDir.x) / TWO_PI;
                    float v = 0.5 - asin(worldDir.y) / PI;

                    float2 uv = float2(u, v);
                    uv = uv * _MainTexture_ST.xy + _MainTexture_ST.zw;

                    float4 texCol = tex2D(_MainTexture, uv);

                    col.rgb *= texCol.rgb;

                    // Texture alpha participates in transparency when enabled
                    outAlpha *= texCol.a;
                }

                // Optional Bayer dithering (alpha-clip) mode
                if (_UseDither > 0.5)
                {
                    // Ensure exact 0 opacity discards all pixels (Bayer includes a 0 threshold value)
                    if (outAlpha <= 0.000001)
                        clip(-1);

                    float2 screenUV = i.screenPos.xy / i.screenPos.w;
                    float2 pixel = floor(screenUV * _ScreenParams.xy / max(_DitherScale, 0.0001));
                    float threshold = Bayer4x4(pixel);
                    clip(outAlpha - threshold);
                    outAlpha = 1.0; // keep surviving pixels fully opaque
                }

                col.a = saturate(outAlpha);
                return col;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}