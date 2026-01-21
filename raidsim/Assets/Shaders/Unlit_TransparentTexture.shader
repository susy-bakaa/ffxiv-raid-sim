// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/TransparentTexture"
{
    Properties
    {
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}

        [MainColor]
        _Color ("Color", Color) = (1,1,1,1)

        _Emission ("Emission", Color) = (0,0,0,0)

        _Cutoff ("Cutoff", Range(0,1)) = 0.5

        _Alpha ("Alpha", Range(0,1)) = 1.0

        [Enum(Off,0,Front,1,Back,2)]
        _DoubleSided ("Cull", Float) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }

        LOD 200
        Cull [_DoubleSided]
        ZWrite On

        AlphaToMask On

        CGPROGRAM
        #pragma surface surf Unlit alphatest:_Cutoff noshadow

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _Emission;
        float _Alpha;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos; // needed for dithering
        };

        half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, s.Alpha);
        }

        // 4x4 Bayer dither threshold in [0..1)
        float Dither4x4(float2 pixelPos)
        {
            int2 p = int2(floor(pixelPos)) & 3;

            float4 row;
            if (p.y == 0) row = float4(0,  8,  2, 10);
            else if (p.y == 1) row = float4(12, 4, 14,  6);
            else if (p.y == 2) row = float4(3, 11,  1,  9);
            else               row = float4(15, 7, 13,  5);

            float v = row[p.x];
            return (v + 0.5) / 16.0;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 tinted = tex * _Color;

            o.Albedo   = tinted.rgb;
            o.Emission = _Emission.rgb;

            // Base cutout comes from tinted alpha (texture alpha * color alpha)
            o.Alpha = tinted.a;

            // skip dithering work when fully opaque
            UNITY_BRANCH
            if (_Alpha < 0.999)
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 pixelPos = screenUV * _ScreenParams.xy;
                float d = Dither4x4(pixelPos);

                clip(_Alpha - d);
            }
        }
        ENDCG

        // Custom shadow caster so the same cutout + dither controls shadow casting
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull [_DoubleSided]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Cutoff;
            float _Alpha;

            float Dither4x4(float2 pixelPos)
            {
                int2 p = int2(floor(pixelPos)) & 3;

                float4 row;
                if (p.y == 0) row = float4(0,  8,  2, 10);
                else if (p.y == 1) row = float4(12, 4, 14,  6);
                else if (p.y == 2) row = float4(3, 11,  1,  9);
                else               row = float4(15, 7, 13,  5);

                float v = row[p.x];
                return (v + 0.5) / 16.0;
            }

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv       : TEXCOORD1;
                float4 screenPos: TEXCOORD2;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                // computes o.pos + bias/normal offset for shadow casting
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);

                // screenPos for the dither (in the shadow caster's clip space)
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tinted = tex2D(_MainTex, i.uv) * _Color;

                // Same base cutout as the main pass
                clip(tinted.a - _Cutoff);

                // skip dithering work when fully opaque
                UNITY_BRANCH
                if (_Alpha < 0.999)
                {
                    float2 screenUV = i.screenPos.xy / i.screenPos.w;
                    float2 pixelPos = screenUV * _ScreenParams.xy;
                    float d = Dither4x4(pixelPos);
                    clip(_Alpha - d);
                }
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Legacy Shaders/Transparent/Cutout/Diffuse"
}