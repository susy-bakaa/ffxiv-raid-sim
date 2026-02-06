// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Hidden/GeometryTint"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1,0,0,1)
        _Alpha ("Alpha", Range(0,1)) = 0
        _GeometryOnly ("Geometry Only (0/1)", Range(0,1)) = 1
        _DepthEpsilon ("Depth Epsilon", Range(0,0.01)) = 0.0005
        _SolidDepthTex ("Solid Depth Tex", 2D) = "black" {}
        _UseSolidMask ("Use Solid Mask (0/1)", Range(0,1)) = 0
        _SolidMaskEpsilon ("Solid Mask Epsilon", Range(0, 0.5)) = 0.01
        _MainTex ("MainTex", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TintColor;
            float _Alpha;
            float _GeometryOnly;
            float _DepthEpsilon;
            sampler2D _SolidDepthTex;
            float _UseSolidMask;
            float _SolidMaskEpsilon;

            // Provided by Unity when camera.depthTextureMode includes Depth
            sampler2D _CameraDepthTexture;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Default: affect everything
                float mask = 1.0;

                // If geometry-only is enabled, mask out skybox/background using depth
                // Skybox typically ends up at depth ~ 1.0
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float depth01 = Linear01Depth(rawDepth);

                float geomMask = (depth01 < (1.0 - _DepthEpsilon)) ? 1.0 : 0.0;

                // Dynamically blend between "all" and "geometry only"
                mask = lerp(1.0, geomMask, saturate(_GeometryOnly));

                // Fill missing pixels using solid depth mask (ignores alpha-clip holes and such)
                if (_UseSolidMask > 0.5)
                {
                    float sceneRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                    float sceneEye = LinearEyeDepth(sceneRaw);

                    float solidEye = tex2D(_SolidDepthTex, i.uv).r;

                    // Only apply if mask pixel exists and the solid object is actually in front (visible)
                    float solidVisible = (solidEye > 0.0 && solidEye <= sceneEye + _SolidMaskEpsilon) ? 1.0 : 0.0;

                    mask = max(mask, solidVisible);
                }

                float t = saturate(_Alpha) * mask;
                col.rgb = lerp(col.rgb, _TintColor.rgb, t);

                return col;
            }
            ENDCG
        }
    }
}
