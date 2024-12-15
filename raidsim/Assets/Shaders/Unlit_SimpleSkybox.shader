Shader "Custom/SimpleSkybox"
{
    Properties
    {
        _MainColor ("Top Color", Color) = (0.0, 0.5, 1.0, 1.0)
        _SecondaryColor ("Bottom Color", Color) = (1.0, 0.5, 0.0, 1.0)
        _MixFactor ("Mix", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _MainColor;
            float4 _SecondaryColor;
            float _MixFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                return o;
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
                return lerp(_SecondaryColor, _MainColor, t);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}