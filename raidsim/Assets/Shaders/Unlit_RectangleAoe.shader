Shader "Custom/Unlit/RectangleAoe"
{
    Properties
    {
        [HideInInspector]
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "white" {}
        [HideInInspector]
        _OuterWidth ("Outer Width", Float) = 0.9
        [HideInInspector]
        _OuterHeight ("Outer Height", Float) = 0.9
        [HideInInspector]
        _InnerSize ("Inner Size", Range(0.0, 1.0)) = 0.2 // Single variable for inner shape
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
    CustomEditor "dev.susybaka.raidsim.Editor.RectangleAoeShaderInspector"
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200

        Cull [_DoubleSided]

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        float _OuterWidth;
        float _OuterHeight;
        float _InnerSize; // Single variable for inner shape
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
            float distX = abs(uv.x);
            float distY = abs(uv.y);
            float angle = atan2(uv.y, uv.x) + 3.14159265359; // angle in radians

            // Calculate inner rectangle dimensions based on _InnerSize
            float innerWidth = max(0.0, _OuterWidth - _InnerSize);
            float innerHeight = max(0.0, _OuterHeight - _InnerSize);

            // Use the built-in _Time.y to create a sawtooth wave for the inner rectangle growth
            float time = _Time.y * _PulseSpeed;
            float cycleTime = frac(time);

            // Calculate the pulsing effect
            float pulseWidth = innerWidth + cycleTime * (_OuterWidth - innerWidth);
            float pulseHeight = innerHeight + cycleTime * (_OuterHeight - innerHeight);
            float fadeWidth = pulseWidth * _InnerRatio;
            float fadeHeight = pulseHeight * _InnerRatio;

            // Calculate fading effect for the growing rectangle
            float fadeStart = 1.0 - _FadeDuration;
            float fadeInEffect = (cycleTime < _FadeDuration) ? cycleTime / _FadeDuration : 1.0;
            float fadeOutEffect = (cycleTime > fadeStart) ? 1.0 - ((cycleTime - fadeStart) / _FadeDuration) : 1.0;
            float fadeEffect = min(fadeInEffect, fadeOutEffect);

            // Outer rectangle opacity
            float outerOpacity = 0.0f;
            if (distX < _OuterWidth && distY < _OuterHeight && angle <= angleToRad(_Angle))
            {
                float multX = (distX - innerWidth) / (_OuterWidth - innerWidth);
                float multY = (distY - innerHeight) / (_OuterHeight - innerHeight);
                outerOpacity = lerp(_MinFill, _MaxFill, max(multX, multY));
            }

            // Inner rectangle opacity
            float innerOpacity = 0.0f;
            if (innerWidth > 0.0 && innerHeight > 0.0)
            {
                if (distX < fadeWidth && distY < fadeHeight && angle <= angleToRad(_Angle))
                {
                    float multX = (distX - (innerWidth - _Outline)) / (fadeWidth - (innerWidth - _Outline));
                    float multY = (distY - (innerHeight - _Outline)) / (fadeHeight - (innerHeight - _Outline));
                    innerOpacity = lerp(_MinFill, _MaxFill, max(multX, multY)) * fadeEffect * _InnerOpacity;
                }
            }
            else
            {
                if (distX < fadeWidth && distY < fadeHeight && angle <= angleToRad(_Angle))
                {
                    float multX = distX / fadeWidth;
                    float multY = distY / fadeHeight;
                    innerOpacity = lerp(_MinFill, _MaxFill, max(multX, multY)) * fadeEffect * _InnerOpacity;
                }
            }

            // Glow effect (clamped to the rectangle's boundaries)
            float glowOpacity = 0.0f;
            if (_Glow > 0.0f)
            {
                // Calculate glow for the X-axis (sides A and C)
                float glowDistX = abs(distX - _OuterWidth);
                float glowX = (glowDistX < _Glow && distY <= _OuterHeight && angle <= angleToRad(_Angle)) 
                    ? _GlowOpacity * pow((1.0 - glowDistX / _Glow), 4.0) 
                    : 0.0;

                // Calculate glow for the Y-axis (sides B and D)
                float glowDistY = abs(distY - _OuterHeight);
                float glowY = (glowDistY < _Glow && distX <= _OuterWidth && angle <= angleToRad(_Angle)) 
                    ? _GlowOpacity * pow((1.0 - glowDistY / _Glow), 4.0) 
                    : 0.0;

                // Calculate glow for the corners (where both X and Y glows overlap)
                float glowCornerX = (glowDistX < _Glow && distY > _OuterHeight && distY <= _OuterHeight + _Glow && angle <= angleToRad(_Angle)) 
                    ? _GlowOpacity * pow((1.0 - glowDistX / _Glow), 4.0) 
                    : 0.0;
                float glowCornerY = (glowDistY < _Glow && distX > _OuterWidth && distX <= _OuterWidth + _Glow && angle <= angleToRad(_Angle)) 
                    ? _GlowOpacity * pow((1.0 - glowDistY / _Glow), 4.0) 
                    : 0.0;

                // Show the inner glow only if the inner shape is bigger than zero
                if (_InnerSize < 1.0f && innerWidth > 0.0f && innerHeight > 0.0f)
                {
                    // Calculate glow for the inner X-axis (sides A and C)
                    float glowDistXInner = abs(distX - innerWidth);
                    float glowXInner = (glowDistXInner < _Glow && distY <= innerHeight && angle <= angleToRad(_Angle)) 
                        ? _GlowOpacity * pow((1.0 - glowDistXInner / _Glow), 4.0) 
                        : 0.0;

                    // Calculate glow for the inner Y-axis (sides B and D)
                    float glowDistYInner = abs(distY - innerHeight);
                    float glowYInner = (glowDistYInner < _Glow && distX <= innerWidth && angle <= angleToRad(_Angle)) 
                        ? _GlowOpacity * pow((1.0 - glowDistYInner / _Glow), 4.0) 
                        : 0.0;

                    // Calculate glow for the inner corners (where both X and Y glows overlap)
                    float glowCornerXInner = (glowDistXInner < _Glow && distY > innerHeight && distY <= innerHeight + _Glow && angle <= angleToRad(_Angle)) 
                        ? _GlowOpacity * pow((1.0 - glowDistXInner / _Glow), 4.0) 
                        : 0.0;
                    float glowCornerYInner = (glowDistYInner < _Glow && distX > innerWidth && distX <= innerWidth + _Glow && angle <= angleToRad(_Angle)) 
                        ? _GlowOpacity * pow((1.0 - glowDistYInner / _Glow), 4.0) 
                        : 0.0;

                    // Combine the glows smoothly
                    glowOpacity = max(glowX, glowY) + max(glowXInner, glowYInner) + min(glowCornerX, glowCornerY) + min(glowCornerXInner, glowCornerYInner);

                    // Smoothly reduce glow in the corners to avoid overlapping artifacts
                    float cornerFactor = smoothstep(_Glow, 0.0, min(min(glowDistX, glowDistY), min(glowDistXInner, glowDistYInner)));
                    glowOpacity *= cornerFactor;
                }
                else
                {
                    // Combine the glows smoothly
                    glowOpacity = max(glowX, glowY) + min(glowCornerX, glowCornerY);

                    // Smoothly reduce glow in the corners to avoid overlapping artifacts
                    float cornerFactor = smoothstep(_Glow, 0.0, min(glowDistX, glowDistY));
                    glowOpacity *= cornerFactor;
                }
            }

            // Outline effect (clamped to the rectangle's boundaries)
            float outlineOpacity = 0.0f;
            if (_Outline > 0.0f)
            {
                // Calculate outline for the X-axis (sides A and C)
                if (distX < _OuterWidth + _Outline && distY <= _OuterHeight + _Outline && angle <= angleToRad(_Angle))
                {
                    float outlineDistX = abs(distX - _OuterWidth);
                    if (outlineDistX < _Outline)
                    {
                        outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                    }
                }

                // Calculate outline for the Y-axis (sides B and D)
                if (distY < _OuterHeight + _Outline && distX <= _OuterWidth + _Outline && angle <= angleToRad(_Angle))
                {
                    float outlineDistY = abs(distY - _OuterHeight);
                    if (outlineDistY < _Outline)
                    {
                        outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                    }
                }

                // Show the inner outline only if the inner shape is bigger than zero
                if (_InnerSize < 1.0f && innerWidth > 0.0f && innerHeight > 0.0f)
                {
                    // Calculate outline for the inner X-axis (sides A and C)
                    if (distX < innerWidth + _Outline && distY <= innerHeight + _Outline && angle <= angleToRad(_Angle))
                    {
                        float outlineDistXInner = abs(distX - innerWidth);
                        if (outlineDistXInner < _Outline)
                        {
                            outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                        }
                    }

                    // Calculate outline for the inner Y-axis (sides B and D)
                    if (distY < innerHeight + _Outline && distX <= innerWidth + _Outline && angle <= angleToRad(_Angle))
                    {
                        float outlineDistYInner = abs(distY - innerHeight);
                        if (outlineDistYInner < _Outline)
                        {
                            outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                        }
                    }
                }
            }

            // Apply outline to angular edges within rectangle bounds if angle is less than 360
            float leftAngularOutlineThickness = ((_AngularOutline * 300.0) / 2); // Adjust the thickness of the left angular outline
            float rightAngularOutlineThickness = _AngularOutline * 300.0; // Adjust the thickness of the right angular outline
            if (_Angle < 360.0)
            {
                // Angular outlines for the outer rectangle (only between inner and outer rectangles)
                if (abs((angle + (angleToRad(leftAngularOutlineThickness))) - angleToRad(_Angle)) < angleToRad(leftAngularOutlineThickness) &&
                    ((distX >= innerWidth && distX <= _OuterWidth) || (distY >= innerHeight && distY <= _OuterHeight)))
                {
                    outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                }
                if (abs(angle) < angleToRad(rightAngularOutlineThickness) && 
                    ((distX >= innerWidth && distX <= _OuterWidth) || (distY >= innerHeight && distY <= _OuterHeight)))
                {
                    outlineOpacity = max(outlineOpacity, _OutlineOpacity);
                }
            }

            // Ensure angular outlines do not extend beyond defined rectangle bounds
            if (_Angle < 360.0)
            {
                //Clamp angular outlines for the outer rectangle
                if (abs(angle - angleToRad(_Angle)) < angleToRad(leftAngularOutlineThickness) &&
                    (distX > _OuterWidth + _Outline || distY > _OuterHeight + _Outline)) //  || distX < innerWidth - _Outline || distY < innerHeight - _Outline
                {
                    outlineOpacity = 0.0;
                }
                if (abs(angle) < angleToRad(rightAngularOutlineThickness) &&
                    (distX > _OuterWidth + _Outline || distY > _OuterHeight + _Outline)) // || distX < innerWidth - _Outline || distY < innerHeight - _Outline
                {
                    outlineOpacity = 0.0;
                }
                if (distX > _OuterWidth + _Outline || distY > _OuterHeight + _Outline)
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
            if (glowOpacity > 0)
            {
                finalColor += _GlowTintColor.rgb * glowOpacity * (1 - innerOpacity);
            }
            if (outerOpacity > 0)
            {
                finalColor += _TintColor.rgb * outerOpacity  * (1 - glowOpacity) * (1 - innerOpacity);
            }
            if (outlineOpacity > 0)
            {
                finalColor = _OutlineTintColor.rgb; // Outline color overrides other colors
            }

            o.Albedo = finalColor;

            // Alpha calculations for proper blending of the different components
            float innerAlpha = innerOpacity > 0 ? _InnerTintColor.a * innerOpacity : 0.0;
            float outerAlpha = outerOpacity > 0 ? _TintColor.a * outerOpacity : 0.0;
            float glowAlpha = glowOpacity > 0 ? _GlowTintColor.a * glowOpacity : 0.0;
            float outlineAlpha = outlineOpacity > 0 ? _OutlineOpacity : 0.0;

            o.Alpha = saturate(innerAlpha + outerAlpha + glowAlpha + outlineAlpha);
            o.Alpha *= _Alpha;

            // Ensure everything outside the outer rectangle (plus glow/outline) is transparent
            if (distX > _OuterWidth + _Glow + _Outline || distY > _OuterHeight + _Glow + _Outline)
            {
                o.Alpha = 0.0;
            }
            // Ensure everything inside the inner rectangle (plus glow/outline) is transparent
            if (distX < (innerWidth - _Glow - _Outline) && distY < (innerHeight - _Glow - _Outline))
            {
                o.Alpha = 0.0;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}