Shader "Custom/Unlit/Lightshaft"
{
    Properties
    {
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}
        _Scroll_Speed ("Scroll Speed", Vector) = (0, 0, 0, 0)
        [MainColor]
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _DoubleSided ("Double Sided", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300

        // Setup proper blend mode
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull [_Double_Sided]
        
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        float2 _Scroll_Speed;
        fixed4 _Tint;

        struct Input
        {
            float2 uv_MainTex;
            fixed4 color : COLOR; // Vertex color
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.color = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Scroll UVs
            float2 scrolledUV = IN.uv_MainTex + _Time.y * _Scroll_Speed;

            // Sample the texture
            fixed4 tex = tex2D(_MainTex, scrolledUV);

            // Tint
            tex.rgb *= _Tint.rgb;

            // Vertex Color Alpha * Texture Alpha
            float alpha = tex.a * IN.color.a;

            o.Albedo = tex.rgb;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}