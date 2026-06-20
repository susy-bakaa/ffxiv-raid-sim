// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Lit/Background"
{
    Properties
    {
        [MainTexture] _Main ("Main Texture", 2D) = "white" {}
        [MainColor] _Tint ("Tint", Color) = (1,1,1,1)
        _Normal ("Normal Texture", 2D) = "bump" {}
        _Emission ("Emission Texture", 2D) = "black" {}
        _Emission_Tint ("Emission Tint", Color) = (0,0,0,1)
        _Specular ("Specular Texture", 2D) = "grey" {}
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _Tiling ("Tiling", Vector) = (1, 1, 0, 0)
        _Offset ("Offset", Vector) = (0, 0, 0, 0)

        _useSecondLayer ("Use Second Layer", Range(0, 1)) = 1.0
        _Blend_Strength ("Blend Strength", Float) = 0.5
        _Blend ("Blended Texture", 2D) = "white" {}
        _Blend_Tint ("Blended Tint", Color) = (1,1,1,1)
        _Blend_Normal ("Blended Normal Texture", 2D) = "bump" {}
        _Emission_Blend ("Blended Emission Texture", 2D) = "black" {}
        _Emission_Blend_Tint ("Blended Emission Tint", Color) = (0,0,0,1)
        _Blend_Specular ("Blended Specular Texture", 2D) = "grey" {}
        _Blend_Metallic ("Blended Metallic", Range(0, 1)) = 0.0
        _Blend_Tiling ("Blended Tiling", Vector) = (1, 1, 0, 0)
        _Blend_Offset ("Blended Offset", Vector) = (0, 0, 0, 0)

        _AlphaClip ("Main Texture Alpha Clip Threshold", Range(0, 1)) = 0.0
        _DitherSize ("Dither Size", Integer) = 1
        _Alpha ("Alpha", Range(0, 1)) = 1.0

        _useVertexColor ("Use Vertex Color", Range(0, 1)) = 0.0
        _VertexColorMultiplier ("Vertex Color Multiplier", Range(0, 4)) = 1.0
        _VertexColorPower ("Vertex Color Power", Range(0.1, 4)) = 1.0
        _VertexColorSaturation ("Vertex Color Saturation", Range(0, 4)) = 1.0
        _VertexColorUseAlpha ("Use Vertex Alpha As Tint Strength", Range(0, 1)) = 1.0
        _VertexColorDebug ("Draw Only Vertex Colors", Range(0, 1)) = 0.0
        _Double_Sided ("Double Sided", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Cull [_Double_Sided]

        CGPROGRAM
        //#pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _Main;
        fixed4 _Tint;
        sampler2D _Normal;
        sampler2D _Emission;
        fixed4 _Emission_Tint;
        sampler2D _Specular;
        float _Metallic;
        float4 _Tiling;
        float4 _Offset;

        float _useSecondLayer;
        float _Blend_Strength;
        sampler2D _Blend;
        fixed4 _Blend_Tint;
        sampler2D _Blend_Normal;
        sampler2D _Emission_Blend;
        fixed4 _Emission_Blend_Tint;
        sampler2D _Blend_Specular;
        float _Blend_Metallic;
        float4 _Blend_Tiling;
        float4 _Blend_Offset;

        float _AlphaClip;
        int _DitherSize;
        half _Alpha;

        float _useVertexColor;
        float _VertexColorMultiplier;
        float _VertexColorPower;
        float _VertexColorSaturation;
        float _VertexColorUseAlpha;
        float _VertexColorDebug;

        float4 _Main_ST;
        float4 _Normal_ST;
        float4 _Emission_ST;
        float4 _Specular_ST;

        float4 _Blend_ST;
        float4 _Blend_Normal_ST;
        float4 _Emission_Blend_ST;
        float4 _Blend_Specular_ST;

        struct Input
        {
            float2 baseUV;
            float4 screenPos;
            float4 color : COLOR; // Vertex color (for blending alpha)
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.baseUV = v.texcoord.xy;
            o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
            o.color = v.color;
        }

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

        float2 ApplyTextureTilingOffset(float2 uv, float4 textureST, float4 layerTiling, float4 layerOffset)
        {
            return uv * layerTiling.xy * textureST.xy + layerOffset.xy + textureST.zw;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            if (_VertexColorDebug > 0.0f)
            {
                o.Albedo = 0;
                o.Emission = IN.color.rgb;
                o.Metallic = 0;
                o.Smoothness = 0;
                return;
            }

            // Sample first layer
            float2 uvMain = ApplyTextureTilingOffset(IN.baseUV, _Main_ST, _Tiling, _Offset);
            float2 uvEmission = ApplyTextureTilingOffset(IN.baseUV, _Emission_ST, _Tiling, _Offset);
            float2 uvNormal = ApplyTextureTilingOffset(IN.baseUV, _Normal_ST, _Tiling, _Offset);
            float2 uvSpecular = ApplyTextureTilingOffset(IN.baseUV, _Specular_ST, _Tiling, _Offset);

            fixed4 mainTex = tex2D(_Main, uvMain);
            clip(mainTex.a - _AlphaClip);

            fixed4 mainColor = mainTex * _Tint;
            fixed4 mainEmission = tex2D(_Emission, uvEmission) * _Emission_Tint;
            fixed4 mainNormal = tex2D(_Normal, uvNormal);
            fixed3 mainNormalUnpacked = UnpackNormal(mainNormal);
            fixed4 mainSpecular = tex2D(_Specular, uvSpecular);

            fixed3 combinedColor;
            fixed3 combinedEmission;
            fixed3 combinedNormal;
            float combinedMetallic;
            float combinedSmoothness;

            if (_useSecondLayer >= 1.0)
            {
                // Sample second layer
                float2 uvBlend = ApplyTextureTilingOffset(IN.baseUV, _Blend_ST, _Blend_Tiling, _Blend_Offset);
                float2 uvBlendEmission = ApplyTextureTilingOffset(IN.baseUV, _Emission_Blend_ST, _Blend_Tiling, _Blend_Offset);
                float2 uvBlendNormal = ApplyTextureTilingOffset(IN.baseUV, _Blend_Normal_ST, _Blend_Tiling, _Blend_Offset);
                float2 uvBlendSpecular = ApplyTextureTilingOffset(IN.baseUV, _Blend_Specular_ST, _Blend_Tiling, _Blend_Offset);

                fixed4 blendColor = tex2D(_Blend, uvBlend) * _Blend_Tint;
                fixed4 blendEmission = tex2D(_Emission_Blend, uvBlendEmission) * _Emission_Blend_Tint;
                fixed4 blendNormal = tex2D(_Blend_Normal, uvBlendNormal);
                fixed3 blendNormalUnpacked = UnpackNormal(blendNormal);
                fixed4 blendSpecular = tex2D(_Blend_Specular, uvBlendSpecular);

                // Construct blend factor
                float blendFac = clamp(IN.color.a * _Blend_Strength, 0.0, 1.0);

                // Blend between textures
                combinedColor = lerp(mainColor.rgb, blendColor.rgb, blendFac);
                combinedEmission = lerp(mainEmission.rgb, blendEmission.rgb, blendFac);
                combinedNormal = normalize(lerp(mainNormalUnpacked, blendNormalUnpacked, blendFac));
                combinedMetallic = lerp(_Metallic, _Blend_Metallic, blendFac);
                combinedSmoothness = lerp(mainSpecular.r, blendSpecular.r, blendFac);
            }
            else
            {
                // No second layer, use first layer values directly
                combinedColor = mainColor.rgb;
                combinedEmission = mainEmission.rgb;
                combinedNormal = mainNormalUnpacked;
                combinedMetallic = _Metallic;
                combinedSmoothness = mainSpecular.r;
            }

            if (_useVertexColor >= 1.0)
            {
                float3 vertexColorRGB = saturate((float3)IN.color.rgb);

                // Use values below 1.0 to brighten/expand muted vertex colors.
                // Example: 0.4545 is roughly linear-to-gamma-style brightening.
                vertexColorRGB = pow(max(vertexColorRGB, 1e-5), _VertexColorPower);

                // Useful if the original game shader effectively expands 0.5-ish values.
                vertexColorRGB = saturate(vertexColorRGB * _VertexColorMultiplier);

                // Optional saturation boost.
                float luma = dot(vertexColorRGB, float3(0.2126, 0.7152, 0.0722));
                vertexColorRGB = saturate(lerp(luma.xxx, vertexColorRGB, _VertexColorSaturation));

                // Preserve old behavior when _VertexColorUseAlpha = 1.
                // Set this to 0 if alpha should only control texture blending, not RGB tint strength.
                float tintStrength = lerp(1.0, saturate(IN.color.a), saturate(_VertexColorUseAlpha));

                float3 vertexMultiplier = lerp(1.0.xxx, vertexColorRGB, tintStrength);

                combinedColor *= vertexMultiplier;
                combinedEmission *= vertexMultiplier;
            }

            //if (_useVertexColor >= 1.0)
            //{
            //    float3 vertexColorRGB = IN.color.rgb;
            //    float vertexAlpha = IN.color.a;

                // Blend between original combinedColor and combinedColor * vertexColor
            //    combinedColor = lerp(combinedColor, combinedColor * vertexColorRGB, vertexAlpha);
            //    combinedEmission = lerp(combinedEmission, combinedEmission * vertexColorRGB, vertexAlpha);
            //}

            // Apply alpha clipping based on dithering
            float alpha = mainColor.a * _Alpha;

            if (_Alpha > 0.0f)
            {
                UNITY_BRANCH
                if (!(_Alpha >= 0.999h && mainColor.a >= 0.999h))
                {
                    // Calculate dithering
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy; // Scale to screen resolution
                    float ditherThreshold = Bayer4x4(screenUV * _DitherSize);

                    clip(alpha - ditherThreshold);
                }
            }
            else
            {
                clip(-1.0f); // Clip everything, making it completely transparent
            }

            o.Albedo = combinedColor;
            o.Emission = combinedEmission;
            o.Normal = combinedNormal;
            o.Metallic = combinedMetallic;
            o.Smoothness = combinedSmoothness;
            o.Alpha = mainColor.a;
        }
        ENDCG

        // Custom ShadowCaster pass so shadows disappear when alpha-clipped / dither-faded / invisible
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
            #include "UnityCG.cginc"

            sampler2D _Main;
            float4 _Main_ST;
            fixed4 _Tint;
            float4 _Tiling;
            float4 _Offset;
            float _AlphaClip;
            int _DitherSize;
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
                //o.uv = v.texcoord.xy * _Tiling.xy + _Offset.xy;
                o.uv = v.texcoord.xy * _Tiling.xy * _Main_ST.xy + _Offset.xy + _Main_ST.zw;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

                // Screen pos in the shadowcaster clip space (light view)
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_Main, i.uv);
                clip(mainTex.a - _AlphaClip);

                fixed4 c = mainTex * _Tint;
                float alpha = c.a * _Alpha;

                if (_Alpha > 0.0f)
                {
                    UNITY_BRANCH
                    if (!(_Alpha >= 0.999h && c.a >= 0.999h))
                    {
                        float2 screenUV = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                        float ditherThreshold = Bayer4x4(screenUV * _DitherSize);
                        clip(alpha - ditherThreshold);
                    }
                }
                else
                {
                    clip(-1.0f);
                }

                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}