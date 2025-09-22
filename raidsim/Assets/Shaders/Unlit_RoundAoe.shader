// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/RoundAoe"
{
    Properties
    {
        [HideInInspector]
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}
        [HideInInspector]
        _OuterRadius ("Outer Radius", Float) = 0.45
        [HideInInspector]
        _InnerRadius ("Inner Radius", Float) = 0.0
        [HideInInspector]
        _InnerRatio ("Inner Ratio", Range(0.0, 1.0)) = 1.0
        [HideInInspector]
        _MaxFill ("Max Fill", Range(0.0, 1.0)) = 0.6
        [HideInInspector]
        _MinFill ("Min Fill", Range(0.0, 1.0)) = 0.2
        [HideInInspector]
        _Glow ("Glow", Float) = 0.05
        [HideInInspector]
        _GlowOpacity ("Glow Opacity", Range(0.0, 1.0)) = 0.5
        [HideInInspector]
        _Outline ("Outline", Float) = 0.005
        [HideInInspector]
        _OutlineOpacity ("Outline Opacity", Range(0.0, 1.0)) = 1.0
        [HideInInspector]
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        [HideInInspector]
        _FadeDuration ("Fade Duration", Float) = 0.2
        [HideInInspector]
        _Angle ("Angle", Range(0.0, 360.0)) = 360.0
        [HideInInspector]
        _AngularOutline ("Angular Outline Thickness", Float) = 0.0075
        [HideInInspector]
        [MainColor]
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        [HideInInspector]
        _InnerTintColor ("Inner Tint Color", Color) = (1,1,1,1)
        [HideInInspector]
        _GlowTintColor ("Glow Tint Color", Color) = (1,1,1,1)
        [HideInInspector]
        _OutlineTintColor ("Outline Tint Color", Color) = (1,1,1,1)
        [HideInInspector]
        _InnerOpacity ("Inner Opacity", Range(0.0, 1.0)) = 1.0
        [HideInInspector]
        _Alpha ("Alpha", Range(0.0, 1.0)) = 1.0
        [HideInInspector]
        _DoubleSided ("Double Sided", Float) = 2.0
    }
    CustomEditor "dev.susybaka.raidsim.Editor.RoundAoeShaderInspector"
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        Cull [_DoubleSided]

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
        float _AngularOutline;
        float _InnerOpacity;
        fixed4 _TintColor;
        fixed4 _InnerTintColor;
        fixed4 _GlowTintColor;
        fixed4 _OutlineTintColor;
        float _Alpha;

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
                // Apply glow to angular edges within circle bounds if angle is less than 360
                if (_Angle < 360.0)
                {
                    float angularDiff = min(abs(angle - angleToRad(_Angle)), abs(angle));
                    if (angularDiff < angleToRad(_Glow) && dist <= _OuterRadius && dist >= innerRadius)
                    {
                        glowOpacity = max(glowOpacity, _GlowOpacity * pow((1.0 - (angularDiff) / angleToRad(_Glow)), 4.0));
                    }
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

            // Apply outline to angular edges within circle bounds if angle is less than 360
            float leftAngularOutlineThickness = ((_AngularOutline * 300.0) / 2); // Adjust the thickness of the left angular outline
            float rightAngularOutlineThickness = _AngularOutline * 300.0; // Adjust the thickness of the right angular outline
            if (_Angle < 360.0)
            {
                if (abs((angle + (angleToRad(leftAngularOutlineThickness))) - angleToRad(_Angle)) < angleToRad(leftAngularOutlineThickness) && dist <= _OuterRadius && dist >= innerRadius)
                {
                    outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                }
                if (abs(angle) < angleToRad(rightAngularOutlineThickness) && dist <= _OuterRadius && dist >= innerRadius)
                {
                    outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                }
            }

            // Ensure angular outlines do not extend beyond defined circle bounds
            if (_Angle < 360.0)
            {
                if (abs(angle - angleToRad(_Angle)) < angleToRad(leftAngularOutlineThickness) && (dist > (_OuterRadius + _Outline) || dist < (innerRadius - _Outline)))
                {
                    outlineOpacity = 0.0;
                }
                if (abs(angle) < angleToRad(rightAngularOutlineThickness) && (dist > (_OuterRadius + _Outline) || dist < (innerRadius - _Outline)))
                {
                    outlineOpacity = 0.0;
                }
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

            // Alpha calculations for proper blending of the different components
            // we make sure everything respects _TintColor.a and is multiplied by the global _Alpha value
            float innerAlpha = innerOpacity > 0 ? _InnerTintColor.a * innerOpacity : 0.0;
            float outerAlpha = outerOpacity > 0 ? _TintColor.a * outerOpacity : 0.0;
            float glowAlpha = glowOpacity > 0 ? _GlowTintColor.a * glowOpacity : 0.0;
            float outlineAlpha = outlineOpacity > 0 ? _OutlineOpacity : 0.0;

            o.Alpha = saturate(innerAlpha + outerAlpha + glowAlpha + outlineAlpha);
            o.Alpha *= _Alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
