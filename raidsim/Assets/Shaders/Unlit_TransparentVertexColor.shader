Shader "Custom/Unlit/TransparentVertexColor"
{
    Properties
    {
        [HideInInspector]
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}
        [MainColor]
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _VertexColorPower ("Vertex Color Power", Range(0,1)) = 1.0
        _VertexColorRange ("Vertex Color Range", Vector) = (0,1,0,0)
        _Transparency ("Transparency", Range(0,1)) = 1.0
        _Alpha ("Alpha", Range(0,1)) = 1.0
        _DoubleSided ("Double Sided", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        Cull [_DoubleSided]

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _EmissionColor;
        float _VertexColorPower;
        float4 _VertexColorRange;
        float _Transparency;
        float _Alpha;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 vertexColor = IN.color;
            fixed4 emissionMix = _EmissionColor.rgba * vertexColor.rgba;
            
            vertexColor = lerp(_VertexColorRange.x, _VertexColorRange.y, vertexColor);
            vertexColor *= _VertexColorPower;

            o.Emission = emissionMix * (vertexColor * _Transparency);

            o.Alpha = _Transparency * _Alpha * vertexColor.rgb;
            
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
