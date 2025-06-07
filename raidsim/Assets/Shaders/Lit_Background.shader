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

        _useSecondLayer ("Use Second Layer", Float) = 1.0
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

        _useVertexColor ("Use Vertex Color", Float) = 0.0
        _Double_Sided ("Double Sided", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Cull [_Double_Sided]

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
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

        float _useVertexColor;

        struct Input
        {
            float2 uv_Main;
            float2 uv_Blend;
            fixed4 color : COLOR; // Vertex color (for blending alpha)
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_Main = v.texcoord.xy;
            o.uv_Blend = v.texcoord.xy;
            o.color = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Sample first layer
            float2 uvMain = IN.uv_Main * _Tiling.xy + _Offset.xy;
            fixed4 mainColor = tex2D(_Main, uvMain) * _Tint;
            fixed4 mainEmission = tex2D(_Emission, uvMain) * _Emission_Tint;
            fixed4 mainNormal = tex2D(_Normal, uvMain);
            fixed3 mainNormalUnpacked = UnpackNormal(mainNormal);
            fixed4 mainSpecular = tex2D(_Specular, uvMain);

            fixed3 combinedColor;
            fixed3 combinedEmission;
            fixed3 combinedNormal;
            float combinedMetallic;
            float combinedSmoothness;

            if (_useSecondLayer >= 1.0)
            {
                // Sample second layer
                float2 uvBlend = IN.uv_Blend * _Blend_Tiling.xy + _Blend_Offset.xy;
                fixed4 blendColor = tex2D(_Blend, uvBlend) * _Blend_Tint;
                fixed4 blendEmission = tex2D(_Emission_Blend, uvBlend) * _Emission_Blend_Tint;
                fixed4 blendNormal = tex2D(_Blend_Normal, uvBlend);
                fixed3 blendNormalUnpacked = UnpackNormal(blendNormal);
                fixed4 blendSpecular = tex2D(_Blend_Specular, uvBlend);

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
                fixed3 vertexColorRGB = IN.color.rgb;
                float vertexAlpha = IN.color.a;

                // Blend between original combinedColor and combinedColor * vertexColor
                combinedColor = lerp(combinedColor, combinedColor * vertexColorRGB, vertexAlpha);
                combinedEmission = lerp(combinedEmission, combinedEmission * vertexColorRGB, vertexAlpha);
            }

            o.Albedo = combinedColor;
            o.Emission = combinedEmission;
            o.Normal = combinedNormal;
            o.Metallic = combinedMetallic;
            o.Smoothness = combinedSmoothness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}