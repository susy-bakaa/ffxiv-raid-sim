// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
Shader "Custom/Unlit/Particle"
{
    Properties
    {
        [MainTexture] _MainTex ("Particle Texture", 2D) = "white" {}
        [MainColor]   _Color   ("Tint", Color) = (1,1,1,1)

        // Choose blending by setting these two:
        // Alpha:    SrcBlend = SrcAlpha, DstBlend = OneMinusSrcAlpha
        // Additive: SrcBlend = SrcAlpha, DstBlend = One
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5  // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10 // OneMinusSrcAlpha

        // Set Depth Test to Always to draw over meshes.
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _ZTest ("Depth Test", Float) = 4 // LEqual default; set to 8 (Always)

        [Enum(Off,0,On,1)]
        _ZWrite ("Depth Write", Float) = 0

        // IMPORTANT: keep this OFF for "always on top" particles.
        [Toggle] _UseSoftParticles ("Use Soft Particles", Float) = 0
        _InvFade ("Soft Particles Factor", Range(0.01, 3.0)) = 1.0

        _Alpha ("Alpha", Range(0,1)) = 1.0
        
        // This is not used for anything in this shader but since particles are quite often paired up with tethers, 
        // which use the tether shader with this parameter, it can cause an error in our setup.
        // Simplest fix is to include this property here to avoid errors when both are used under the same object,
        // and the tether uses the SimpleShaderRandomization component to randomize this parameter.
        // For a similar reason most of the custom shaders in this project have a _Alpha property.
        [HideInInspector]
        _Distortion_Offset ("Distortion Offset", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            Cull Off
            Lighting Off
            ZWrite [_ZWrite]
            ZTest  [_ZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_particles

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            // Soft particles
            sampler2D_float _CameraDepthTexture;
            float _InvFade;
            float _UseSoftParticles;

            float _Alpha;

            // Not used in this shader
            fixed4 _Distortion_Offset;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv    : TEXCOORD0;
                UNITY_FOG_COORDS(1)

                #if defined(SOFTPARTICLES_ON)
                float4 projPos : TEXCOORD2;
                #endif
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                UNITY_TRANSFER_FOG(o, o.pos);

                #if defined(SOFTPARTICLES_ON)
                o.projPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Soft particle fade (OPTIONAL). Leave _UseSoftParticles = 0 for "always on top".
                #if defined(SOFTPARTICLES_ON)
                if (_UseSoftParticles > 0.5)
                {
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                    float partZ  = i.projPos.z;
                    float fade   = saturate(_InvFade * (sceneZ - partZ));
                    i.color.a *= fade;
                }
                #endif

                if (_Alpha < 1.0)
                {
                    i.color.a = i.color.a * _Alpha;
                }

                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.color;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}