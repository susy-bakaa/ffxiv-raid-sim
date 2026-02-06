// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/RoundAoeMasked"
{
    Properties
    {
        [MainTexture]
        [HideInInspector]
        _MainTex ("Main Texture", 2D) = "white" {}

        /* ===================== V2 SDF MASK ADDITIONS (BEGIN)
           SDF mask:
           - Use msdfgen (mode: sdf) to generate a grayscale SDF where the edge is around 0.5.
           - _MaskSDFPxRange MUST match the pxrange you used during generation.
           - _MaskSDFInvert lets you choose whether the SDF "shape" is the kept area or the cut-out area.
           ===================== */
        _MaskSDF ("Cutout Mask (SDF)", 2D) = "gray" {}
        _MaskSDFEdge ("SDF Edge Value", Range(0.0, 1.0)) = 0.5
        _MaskSDFPxRange ("SDF Pixel Range", Float) = 32.0
        _MaskSDFInvert ("Invert Mask", Range(0.0, 1.0)) = 1.0
        /* ===================== V2 SDF MASK ADDITIONS (END) ===================== */
        /* ===================== V2 MASK EDGE CONTROLS (BEGIN)
           Separate thickness controls for the SDF-cut edges.
           These are multipliers so you can “boost slightly” per material without recalculating absolute values.
           ===================== */
        _MaskEdgeGlowScale ("Mask Edge Glow Scale", Range(0.0, 4.0)) = 1.0
        _MaskEdgeOutlineScale ("Mask Edge Outline Scale", Range(0.0, 4.0)) = 1.0
        /* ===================== V2 MASK EDGE CONTROLS (END) ======================= */

        _OuterRadius ("Outer Radius", Float) = 0.45
        _InnerRadius ("Inner Radius", Float) = 0.0
        _InnerRatio ("Inner Ratio", Range(0.0, 1.0)) = 1.0
        _MaxFill ("Max Fill", Range(0.0, 1.0)) = 0.6
        _MinFill ("Min Fill", Range(0.0, 1.0)) = 0.2
        _Glow ("Glow", Float) = 0.05
        _GlowOpacity ("Glow Opacity", Range(0.0, 1.0)) = 0.5
        _Outline ("Outline", Float) = 0.005
        _OutlineOpacity ("Outline Opacity", Range(0.0, 1.0)) = 1.0
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        _FadeDuration ("Fade Duration", Float) = 0.2
        _Angle ("Angle", Range(0.0, 360.0)) = 360.0
        _AngularOutline ("Angular Outline Thickness", Float) = 0.0075

        [MainColor]
        _TintColor ("Tint Color", Color) = (1,1,1,1)

        _InnerTintColor ("Inner Tint Color", Color) = (1,1,1,1)
        _GlowTintColor ("Glow Tint Color", Color) = (1,1,1,1)
        _OutlineTintColor ("Outline Tint Color", Color) = (1,1,1,1)
        _InnerOpacity ("Inner Opacity", Range(0.0, 1.0)) = 1.0
        _Alpha ("Alpha", Range(0.0, 1.0)) = 1.0
        _DoubleSided ("Double Sided", Float) = 2.0
    }

    //CustomEditor "dev.susybaka.raidsim.Editor.RoundAoeShaderInspector"

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        Cull [_DoubleSided]

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;

        /* ===================== V2 SDF MASK UNIFORMS (BEGIN) ===================== */
        sampler2D _MaskSDF;
        float4 _MaskSDF_TexelSize;
        float _MaskSDFEdge;
        float _MaskSDFPxRange;
        float _MaskSDFInvert;
        /* ===================== V2 SDF MASK UNIFORMS (END) ======================= */
        /* ===================== V2 MASK EDGE UNIFORMS (BEGIN) ===================== */
        float _MaskEdgeGlowScale;
        float _MaskEdgeOutlineScale;
        /* ===================== V2 MASK EDGE UNIFORMS (END) ======================= */

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

            /* ===================== V2 SDF MASK LOGIC (BEGIN)
               Convert SDF sample into a signed distance in UV units so _Glow/_Outline match.
               Convention assumed: edge is around _MaskSDFEdge (~0.5), with normalization based on _MaskSDFPxRange.
               ===================== */
            float sdfVal = tex2D(_MaskSDF, IN.uv_MainTex).r;

            // Normalize around the edge so roughly [-1..1] corresponds to [-pxrange..pxrange] pixels.
            float signedNorm = (sdfVal - _MaskSDFEdge) * 2.0;
            float signedDistPx = signedNorm * _MaskSDFPxRange;

            // Convert pixel distance to UV distance (approx isotropic)
            float pxToUv = min(_MaskSDF_TexelSize.x, _MaskSDF_TexelSize.y);
            float signedDistUv = signedDistPx * pxToUv;
            float edgeDistUv = abs(signedDistUv);

            // Anti-aliased inside test
            float aa = max(fwidth(signedDistUv), 1e-5);
            float maskInside = smoothstep(-aa, aa, signedDistUv);

            // Optional inversion: 1 -> treat SDF "shape" as the cut-out (holes), i.e. keep the outside.
            float maskKeep = lerp(maskInside, 1.0 - maskInside, saturate(_MaskSDFInvert));
            /* ===================== V2 SDF MASK LOGIC (END) ======================= */

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

            /* ===================== V2 APPLY MASK TO FILLS (BEGIN)
               The carve-out should remove the filled areas (outer + pulsing inner) exactly.
               ===================== */
            outerOpacity *= maskKeep;
            innerOpacity *= maskKeep;
            /* ===================== V2 APPLY MASK TO FILLS (END) =================== */

            float glowOpacity = 0.0f;
            if (_Glow > 0.0f)
            {
                float outerDiff = abs(dist - _OuterRadius);
                float innerDiff = abs(dist - innerRadius);
                if (outerDiff < _Glow && angle <= angleToRad(_Angle))
                {
                    glowOpacity = _GlowOpacity * pow((1.0 - outerDiff / _Glow), 4.0);
                }
                if (innerRadius > 0.0 && innerDiff < _Glow && angle <= angleToRad(_Angle))
                {
                    glowOpacity += _GlowOpacity * pow((1.0 - innerDiff / _Glow), 4.0);
                }
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
            float leftAngularOutlineThickness = ((_AngularOutline * 300.0) / 2);
            float rightAngularOutlineThickness = _AngularOutline * 300.0;
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

            /* ===================== V2 APPLY MASK TO RADIAL/ANGULAR EDGES (BEGIN)
               Carved-out pixels should not show the original radial glow/outline either.
               Mask-edge glow/outline is added separately next.
               ===================== */
            glowOpacity *= maskKeep;
            outlineOpacity *= maskKeep;
            /* ===================== V2 APPLY MASK TO RADIAL/ANGULAR EDGES (END) ===== */

            /* ===================== V2 SDF EDGE GLOW + OUTLINE (BEGIN)
               Adds proper glow/outline along the mask cut edges using true distance (edgeDistUv),
               reusing the existing _Glow/_Outline parameters for consistent thickness.
               ===================== */
            float inBounds = (dist <= _OuterRadius && dist >= innerRadius && angle <= angleToRad(_Angle)) ? 1.0 : 0.0;

            /* ===================== V2 MASK EDGE THICKNESS APPLY (BEGIN)
               Apply per-material thickness scaling for the mask cut edges only.
               ===================== */
            float maskOutlineThickness = _Outline * _MaskEdgeOutlineScale;
            float maskGlowThickness = _Glow * _MaskEdgeGlowScale;

            /* ===================== V2 SDF RANGE CLAMP (BEGIN)
                Prevent "whole surface glow" when edgeDistUv is capped by pxrange.
                If we're near the SDF saturation (farther than pxrange), force edge effects to 0.
                ===================== */
            float maxDistUv = _MaskSDFPxRange * pxToUv;

            // 0 near edge, 1 when we're basically saturated/clamped (farther than pxrange)
            float sdfSaturated = smoothstep(0.98, 1.0, abs(signedNorm));

            // Optional: also clamp thickness so you can't ask for a glow wider than representable distance
            maskGlowThickness = min(maskGlowThickness, maxDistUv);
            maskOutlineThickness = min(maskOutlineThickness, maxDistUv);
            /* ===================== V2 SDF RANGE CLAMP (END) =========================== */

            float sdfOutline = 0.0;
            if (maskOutlineThickness > 0.0)
            {
                sdfOutline = step(edgeDistUv, maskOutlineThickness) * _OutlineOpacity;
            }

            float sdfGlow = 0.0;
            if (maskGlowThickness > 0.0)
            {
                sdfGlow = _GlowOpacity * pow(saturate(1.0 - edgeDistUv / maskGlowThickness), 4.0);
            }
            /* ===================== V2 APPLY SDF SATURATION MASK (BEGIN) ===================== */
            sdfOutline *= (1.0 - sdfSaturated);
            sdfGlow *= (1.0 - sdfSaturated);
            /* ===================== V2 APPLY SDF SATURATION MASK (END) ======================= */
            /* ===================== V2 MASK EDGE THICKNESS APPLY (END) ================= */

            outlineOpacity = max(outlineOpacity, sdfOutline * inBounds);
            glowOpacity = max(glowOpacity, sdfGlow * inBounds);
            /* ===================== V2 SDF EDGE GLOW + OUTLINE (END) ================= */

            // Calculate the final color and opacity
            float combinedOpacity = saturate(outerOpacity + innerOpacity + glowOpacity + outlineOpacity);

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
                finalColor = _OutlineTintColor.rgb;
            }

            o.Albedo = finalColor;

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
