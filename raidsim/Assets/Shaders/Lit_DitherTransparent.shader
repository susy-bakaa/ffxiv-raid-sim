// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Lit/DitherTransparent"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Emission ("Emission", 2D) = "white" {}
        _EmissionTint ("Emission Tint", Color) = (0,0,0,0)
        _Normal ("Normal", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MetallicTex ("Metallic Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _SmoothnessTex ("Smoothness Texture", 2D) = "white" {}
        _AmbientOcclusionStrength ("Ambient Occlusion Strength", Range(0,1)) = 1.0
        _AmbientOcclusion ("Ambient Occlusion", 2D) = "white" {}
        _DitherSize ("Dither Size", Integer) = 1
        [Toggle(_SEPARATE_ALPHA_CUTOFF)] _SeparateAlphaCutoff ("Separate Base Alpha", Float) = 0
        _Cutoff ("Base Alpha Cutoff", Range(0,1)) = 0.5
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma shader_feature_local _SEPARATE_ALPHA_CUTOFF

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Tint;
        sampler2D _Emission;
        fixed4 _EmissionTint;
        sampler2D _Normal;
        half _Metallic;
        sampler2D _MetallicTex;
        half _Smoothness;
        sampler2D _SmoothnessTex;
        half _AmbientOcclusionStrength;
        sampler2D _AmbientOcclusion;
        int _DitherSize;
        half _SeparateAlphaCutoff;
        half _Cutoff;
        half _Alpha;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        // Bayer matrix for 4x4 dithering
        float Bayer4x4(float2 position)
        {
            const float4x4 bayerMatrix = {
                { 0,  8,  2, 10 },
                { 12, 4, 14, 6 },
                { 3, 11, 1,  9 },
                { 15, 7, 13, 5 }
            };
            uint2 p = (uint2)floor(position) & 3u;
            return bayerMatrix[(int)p.x][(int)p.y] / 16.0;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Tint;

            // Apply ambient occlusion
            float ao = tex2D(_AmbientOcclusion, IN.uv_MainTex).r * _AmbientOcclusionStrength;
            o.Albedo = c.rgb * ao;

            // Metallic from texture and slider
            float metallicTex = tex2D(_MetallicTex, IN.uv_MainTex).r;
            o.Metallic = metallicTex * _Metallic;

            // Smoothness from texture and slider
            float smoothnessTex = tex2D(_SmoothnessTex, IN.uv_MainTex).r;
            o.Smoothness = smoothnessTex * _Smoothness;

            // Emission
            o.Emission = tex2D(_Emission, IN.uv_MainTex).rgb * _EmissionTint.rgb;

            // Normal map
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));

            // Alpha handling can either use the original combined dither behavior,
            // or hard-cut the texture/tint alpha first and dither only the global alpha.
            #if defined(_SEPARATE_ALPHA_CUTOFF)
                // Standard cutout transparency from texture alpha multiplied by tint alpha.
                clip(c.a - _Cutoff);

                // Global material fade. This does not change the cutout shape; it only
                // dithers the pixels which survived the base alpha cutoff.
                if (_Alpha <= 0.0h)
                {
                    clip(-1.0f);
                }
                else if (_Alpha < 0.999h)
                {
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
                    float ditherThreshold = Bayer4x4(screenUV * _DitherSize);
                    clip(_Alpha - ditherThreshold);
                }
            #else
                // Original behavior: texture/tint alpha and global alpha are multiplied,
                // then the combined result is dithered.
                // Apply alpha clipping based on dithering
                float alpha = c.a * _Alpha;

                if (_Alpha > 0.0f)
                {
                    UNITY_BRANCH
                    if (!(_Alpha >= 0.999h && c.a >= 0.999h))
                    {
                        // Calculate dithering
                        float2 screenUV = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy; // Scale to screen resolution
                        float ditherThreshold = Bayer4x4(screenUV * _DitherSize);

                        // If alpha is completely transparent for this pixel, clip it completely to make it fully invisible. Otherwise, apply dithering.
                        if (alpha > 0.0f)
                        {
                            clip(alpha - ditherThreshold);
                        }
                        else
                        {
                            clip(-1.0f); // Clip everything, making it completely transparent
                        }
                    }
                }
                else
                {
                    clip(-1.0f); // Clip everything, making it completely transparent
                }
            #endif
        }
        ENDCG

        // Custom ShadowCaster pass so shadows disappear when dither-faded / invisible
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma shader_feature_local _SEPARATE_ALPHA_CUTOFF
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Tint;
            int _DitherSize;
            half _SeparateAlphaCutoff;
            half _Cutoff;
            half _Alpha;

            float Bayer4x4(float2 position)
            {
                const float4x4 bayerMatrix = {
                    { 0,  8,  2, 10 },
                    { 12, 4, 14, 6 },
                    { 3, 11, 1,  9 },
                    { 15, 7, 13, 5 }
                };
                uint2 p = (uint2)floor(position) & 3u;
                return bayerMatrix[(int)p.x][(int)p.y] / 16.0;
            }

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv        : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

                // Screen pos in the shadowcaster clip space (light view)
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Tint;

                #if defined(_SEPARATE_ALPHA_CUTOFF)
                    // Match the visible pass: hard-cut base alpha, then dither only
                    // the global fade so shadow coverage follows the material.
                    clip(c.a - _Cutoff);

                    if (_Alpha <= 0.0h)
                    {
                        clip(-1.0f);
                    }
                    else if (_Alpha < 0.999h)
                    {
                        float2 screenUV = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                        float ditherThreshold = Bayer4x4(screenUV * _DitherSize);
                        clip(_Alpha - ditherThreshold);
                    }
                #else
                    float alpha = c.a * _Alpha;

                    if (_Alpha > 0.0f)
                    {
                        UNITY_BRANCH
                        if (!(_Alpha >= 0.999h && c.a >= 0.999h))
                        {
                            float2 screenUV = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                            float ditherThreshold = Bayer4x4(screenUV * _DitherSize);

                            if (alpha > 0.0f)
                            {
                                clip(alpha - ditherThreshold);
                            }
                            else
                            {
                                clip(-1.0f);
                            }
                        }
                    }
                    else
                    {
                        clip(-1.0f);
                    }
                #endif

                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}