Shader "Custom/Unlit/RoundAoeShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _OuterRadius ("Outer Radius", Float) = 0.5
        _InnerRadius ("Inner Radius", Float) = 0.0
        _InnerRatio ("Inner Ratio", Float) = 1.0
        _MaxFill ("Max Fill", Float) = 0.4
        _MinFill ("Min Fill", Float) = 0.3
        _Glow ("Glow", Float) = 0.05
        _GlowOpacity ("Glow Opacity", Float) = 0.5
        _Outline ("Outline", Float) = 0.02
        _OutlineOpacity ("Outline Opacity", Float) = 1.0
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        _FadeDuration ("Fade Duration", Float) = 0.2
        _Angle ("Angle", Float) = 360.0
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _InnerTintColor ("Inner Tint Color", Color) = (1,1,1,1)
        _GlowTintColor ("Glow Tint Color", Color) = (1,1,1,1)
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
        float _OuterRadius;
        float _InnerRadius;
        float _InnerRatio;
        float _MaxFill;
        float _MinFill;
        float _Glow;
        float _GlowOpacity;
        float _Outline;
        float _OutlineOpacity;
        float _PulseSpeed;
        float _FadeDuration;
        float _Angle;
        float _InnerOpacity;
        fixed4 _TintColor;
        fixed4 _InnerTintColor;
        fixed4 _GlowTintColor;
        fixed4 _OutlineTintColor;

        struct Input
        {
            float2 uv_MainTex;
        };

        float angleToRad(float angle)
        {
            return angle * 0.0174532925;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            half2 uv = IN.uv_MainTex - 0.5;
            float dist = length(uv);
            float angle = atan2(uv.y, uv.x) + 3.14159265359; // angle in radians

            // Use the built-in _Time.y to create a sawtooth wave for the inner circle growth
            float time = _Time.y * _PulseSpeed;
            float cycleTime = frac(time);

            float innerRadius = _InnerRadius > 0.0 ? _InnerRadius : 0.0;
            float pulse = innerRadius + cycleTime * (_OuterRadius - innerRadius);
            float fadeRadius = pulse * _InnerRatio;

            // Calculate fading effect for the growing circle
            float fadeStart = 1.0 - _FadeDuration;
            float fadeInEffect = (cycleTime < _FadeDuration) ? cycleTime / _FadeDuration : 1.0;
            float fadeOutEffect = (cycleTime > fadeStart) ? 1.0 - ((cycleTime - fadeStart) / _FadeDuration) : 1.0;
            float fadeEffect = min(fadeInEffect, fadeOutEffect);

            float outerOpacity = 0.0f;
            if (dist < _OuterRadius && dist > innerRadius && angle <= angleToRad(_Angle))
            {
                float mult = (dist - innerRadius) / (_OuterRadius - innerRadius);
                outerOpacity = lerp(_MinFill, _MaxFill, mult);
            }

            float innerOpacity = 0.0f;
            if (innerRadius > 0.0)
            {
                if (dist < fadeRadius && dist > innerRadius && angle <= angleToRad(_Angle))
                {
                    float mult = (dist - innerRadius) / (fadeRadius - innerRadius);
                    innerOpacity = lerp(_MinFill, _MaxFill, mult) * fadeEffect * _InnerOpacity;
                }
            }
            else
            {
                if (dist < fadeRadius && angle <= angleToRad(_Angle))
                {
                    float mult = dist / fadeRadius;
                    innerOpacity = lerp(_MinFill, _MaxFill, mult) * fadeEffect * _InnerOpacity;
                }
            }

            float glowOpacity = 0.0f;
            if (_Glow > 0.0f)
            {
                float outerDiff = abs(dist - _OuterRadius);
                float innerDiff = abs(dist - innerRadius);
                if (outerDiff < _Glow && angle <= angleToRad(_Angle))
                {
                    // Sharpen the glow by using a power function
                    glowOpacity = _GlowOpacity * pow((1.0 - outerDiff / _Glow), 4.0);
                }
                if (innerRadius > 0.0 && innerDiff < _Glow && angle <= angleToRad(_Angle))
                {
                    // Sharpen the glow by using a power function
                    glowOpacity += _GlowOpacity * pow((1.0 - innerDiff / _Glow), 4.0);
                }
                // Apply glow to angular edges within circle bounds
                if ((abs(angle - angleToRad(_Angle)) < angleToRad(_Glow) || abs(angle) < angleToRad(_Glow)) && dist <= _OuterRadius && dist >= innerRadius)
                {
                    glowOpacity = max(glowOpacity, _GlowOpacity * pow((1.0 - min(abs(angle - angleToRad(_Angle)), abs(angle)) / angleToRad(_Glow)), 4.0));
                }
            }

            float outlineOpacity = 0.0f;
            if (_Outline > 0.0f)
            {
                float outerDiff = abs(dist - _OuterRadius);
                float innerDiff = abs(dist - innerRadius);
                if (outerDiff < _Outline && angle <= angleToRad(_Angle))
                {
                    outlineOpacity = _OutlineOpacity;
                }
                if (innerRadius > 0.0 && innerDiff < _Outline && angle <= angleToRad(_Angle))
                {
                    outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                }
            }

            // Apply outline to angular edges within circle bounds
            float angularOutlineThickness = _Outline * 300; // Adjust the thickness of the angular outline, we multiply it by 300 or else it looks too thin compared to the other outlines
            if ((abs(angle - angleToRad(_Angle)) < angleToRad(angularOutlineThickness) || abs(angle) < angleToRad(angularOutlineThickness)) && dist <= _OuterRadius && dist >= innerRadius)
            {
                outlineOpacity = max(outlineOpacity, _OutlineOpacity);
            }

            // Calculate the final color and opacity
            float combinedOpacity = saturate(outerOpacity + innerOpacity + glowOpacity + outlineOpacity);

            // Apply tints based on the contribution of inner, outer, glow, and outline
            fixed3 finalColor = 0;
            if (innerOpacity > 0)
            {
                finalColor = _InnerTintColor.rgb * innerOpacity;
            }
            if (outerOpacity > 0)
            {
                finalColor += _TintColor.rgb * outerOpacity * (1 - innerOpacity);
            }
            if (glowOpacity > 0)
            {
                finalColor += _GlowTintColor.rgb * glowOpacity * (1 - innerOpacity) * (1 - outerOpacity);
            }
            if (outlineOpacity > 0)
            {
                finalColor = _OutlineTintColor.rgb; // Outline color overrides other colors
            }

            o.Albedo = finalColor;
            o.Alpha = outlineOpacity > 0 ? _OutlineOpacity : combinedOpacity * ((innerOpacity > 0) ? _InnerTintColor.a : (outerOpacity > 0) ? _TintColor.a : _GlowTintColor.a);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
