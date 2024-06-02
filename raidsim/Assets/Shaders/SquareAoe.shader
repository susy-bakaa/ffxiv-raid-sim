Shader "Custom/SquareAoe"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _SideLength ("Side Length", Float) = 0.5
        _InnerRatio ("Inner Ratio", Float) = 1.0
        _MaxFill ("Max Fill", Float) = 0.4
        _MinFill ("Min Fill", Float) = 0.3
        _Outline ("Outline", Float) = 0.05
        _OutlineOpacity ("Outline Opacity", Float) = 0.5
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        _FadeDuration ("Fade Duration", Float) = 0.2
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _InnerTintColor ("Inner Tint Color", Color) = (1,1,1,1)
        _OutlineTintColor ("Outline Tint Color", Color) = (1,1,1,1)
        _InnerOpacity ("Inner Opacity", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        float _SideLength;
        float _InnerRatio;
        float _MaxFill;
        float _MinFill;
        float _Outline;
        float _OutlineOpacity;
        float _PulseSpeed;
        float _FadeDuration;
        float _InnerOpacity;
        fixed4 _TintColor;
        fixed4 _InnerTintColor;
        fixed4 _OutlineTintColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            half2 uv = IN.uv_MainTex - 0.5;
            float halfSide = _SideLength / 2.0;

            float time = _Time.y * _PulseSpeed;
            float cycleTime = frac(time);

            float pulse = cycleTime * _SideLength;
            float innerSide = pulse * _InnerRatio;

            float fadeStart = 1.0 - _FadeDuration;
            float fadeInEffect = (cycleTime < _FadeDuration) ? cycleTime / _FadeDuration : 1.0;
            float fadeOutEffect = (cycleTime > fadeStart) ? 1.0 - ((cycleTime - fadeStart) / _FadeDuration) : 1.0;
            float fadeEffect = min(fadeInEffect, fadeOutEffect);

            float outerOpacity = 0.0f;
            if (abs(uv.x) < halfSide && abs(uv.y) < halfSide)
            {
                float mult = max(abs(uv.x), abs(uv.y)) / halfSide;
                outerOpacity = lerp(_MinFill, _MaxFill, mult);
            }

            float innerOpacity = 0.0f;
            if (abs(uv.x) < innerSide / 2.0 && abs(uv.y) < innerSide / 2.0)
            {
                float mult = max(abs(uv.x), abs(uv.y)) / (innerSide / 2.0);
                innerOpacity = lerp(_MinFill, _MaxFill, mult) * fadeEffect * _InnerOpacity;
            }

            float outlineOpacity = 0.0f;
            if (_Outline > 0.0f)
            {
                float diff = max(abs(abs(uv.x) - halfSide), abs(abs(uv.y) - halfSide));
                if (diff < _Outline)
                {
                    outlineOpacity = _OutlineOpacity * pow((1.0 - diff / _Outline), 4.0);
                }
            }

            float combinedOpacity = saturate(outerOpacity + innerOpacity + outlineOpacity);

            fixed3 finalColor = 0;
            if (innerOpacity > 0)
            {
                finalColor = _InnerTintColor.rgb * innerOpacity;
            }
            if (outerOpacity > 0)
            {
                finalColor += _TintColor.rgb * outerOpacity * (1 - innerOpacity);
            }
            if (outlineOpacity > 0)
            {
                finalColor += _OutlineTintColor.rgb * outlineOpacity * (1 - innerOpacity) * (1 - outerOpacity);
            }

            o.Albedo = finalColor;
            o.Alpha = combinedOpacity * ((innerOpacity > 0) ? _InnerTintColor.a : (outerOpacity > 0) ? _TintColor.a : _OutlineTintColor.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
