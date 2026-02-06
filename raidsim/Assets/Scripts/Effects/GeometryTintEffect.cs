// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using NaughtyAttributes;

namespace dev.susybaka.raidsim.Visuals
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class GeometryTintEffect : MonoBehaviour
    {
        public enum MaskSampleMode
        {
            Full,
            Half,
            Quarter,
            Eighth
        }

        [Header("Effect")]
        [Tooltip("Color to tint the geometry with.")]
        public Color tintColor = Color.red;
        [Tooltip("Overall alpha of the tint effect.")]
        [Range(0f, 1f)] public float alpha = 0f;

        [Header("Masking")]
        [Tooltip("If true, only geometry (not skybox/background) is tinted.")]
        public bool geometryOnly = true;
        [Tooltip("How close to 1.0 depth counts as skybox/background.")]
        [Range(0f, 0.01f)] public float depthEpsilon = 0.0005f;

        [Tooltip("If true, a solid geometry mask is rendered to avoid tinting being inconsistent on transparent or alpha clipped objects.\nThis increases the performance cost.")]
        public bool useSolidMask = true;
        [Tooltip("Layers to include in the solid geometry mask.")]
        [ShowIf(nameof(useSolidMask))] public LayerMask solidMaskLayers = ~0;
        [Tooltip("If true, the solid mask will use the alpha texture of objects to determine coverage.")]
        [ShowIf(nameof(useSolidMask))] public bool solidMaskUseAlphaTex = true;
        [Tooltip("Cutoff threshold for alpha texture usage in solid mask.")]
        [ShowIf(nameof(useSolidMask)), Range(0f, 1f)] public float solidAlphaTexCutoff = 0.001f;
        [Tooltip("How close depths must be to count as occluded in the solid mask.")]
        [ShowIf(nameof(useSolidMask)), Range(0f, 0.5f)] public float solidMaskEpsilon = 0.01f;
        [Tooltip("Downsampling for the solid mask render texture to improve performance.")]
        [ShowIf(nameof(useSolidMask))] public MaskSampleMode solidMaskDownsample = MaskSampleMode.Half; // 1=full, 2=half, 4=quarter, etc. [Range(1, 8)]

        [Header("Setup")]
        [Tooltip("Shader used for the tint effect.")]
        public Shader shader;
        [Tooltip("Shader used to render the solid depth mask.")]
        [ShowIf(nameof(useSolidMask))] public Shader solidDepthShader;

        // Private
        Material _mat;
        Camera _cam;
        Camera _maskCam;
        RenderTexture _solidDepthRT;

        // Shader property IDs
        int _tintColor = Shader.PropertyToID("_TintColor");
        int _alpha = Shader.PropertyToID("_Alpha");
        int _geometryOnly = Shader.PropertyToID("_GeometryOnly");
        int _depthEpsilon = Shader.PropertyToID("_DepthEpsilon");
        int _useAlphaTexMask = Shader.PropertyToID("_UseAlphaTexMask");
        int _alphaTexCutoff = Shader.PropertyToID("_AlphaTexCutoff");
        int _useSolidMask = Shader.PropertyToID("_UseSolidMask");
        int _solidMaskEpsilon = Shader.PropertyToID("_SolidMaskEpsilon");
        int _solidDepthTex = Shader.PropertyToID("_SolidDepthTex");

        void OnEnable()
        {
            _cam = GetComponent<Camera>();
            EnsureMaterial();
            // Needed for _CameraDepthTexture
            _cam.depthTextureMode |= DepthTextureMode.Depth;
        }

        void OnDisable()
        {
            if (_mat != null)
            {
                if (Application.isPlaying)
                    Destroy(_mat);
                else
                    DestroyImmediate(_mat);
            }
            if (_solidDepthRT != null)
            { 
                _solidDepthRT.Release(); 
                _solidDepthRT = null; 
            }
            if (_maskCam != null)
            {
                if (Application.isPlaying)
                    Destroy(_maskCam.gameObject);
                else
                    DestroyImmediate(_maskCam.gameObject);
                _maskCam = null;
            }
        }

        void EnsureMaterial()
        {
            if (shader == null)
                shader = Shader.Find("Hidden/GeometryTint");
            if (shader == null)
                return;

            if (_mat == null || _mat.shader != shader)
            {
                _mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            EnsureMaterial();
            if (_mat == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            _mat.SetColor(_tintColor, tintColor);
            _mat.SetFloat(_alpha, alpha);
            _mat.SetFloat(_geometryOnly, geometryOnly ? 1f : 0f);
            _mat.SetFloat(_depthEpsilon, depthEpsilon);

            // Render solid mask only when needed
            bool wantSolid = useSolidMask && alpha > 0.0001f;

            if (wantSolid)
            {
                if (solidDepthShader == null)
                    solidDepthShader = Shader.Find("Hidden/SolidDepthMask");

                if (_maskCam == null) // NEW:
                {
                    var go = new GameObject("~SolidMaskCam");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _maskCam = go.AddComponent<Camera>();
                    _maskCam.enabled = false;
                }

                int ds = Mathf.Clamp(((int)solidMaskDownsample + 1), 1, 8);
                int w = Mathf.Max(1, src.width / ds);
                int h = Mathf.Max(1, src.height / ds);

                // Choose a supported single-channel format when possible (WebGL friendliness varies)
                RenderTextureFormat fmt = RenderTextureFormat.RHalf;
                if (!SystemInfo.SupportsRenderTextureFormat(fmt))
                    fmt = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat)
                        ? RenderTextureFormat.RFloat
                        : RenderTextureFormat.ARGBHalf;

                if (_solidDepthRT == null || _solidDepthRT.width != w || _solidDepthRT.height != h || _solidDepthRT.format != fmt)
                {
                    if (_solidDepthRT != null)
                        _solidDepthRT.Release();
                    _solidDepthRT = new RenderTexture(w, h, 24, fmt);
                    _solidDepthRT.name = "_SolidDepthRT";
                    _solidDepthRT.filterMode = FilterMode.Bilinear; // smoother upsample
                    _solidDepthRT.wrapMode = TextureWrapMode.Clamp;
                    _solidDepthRT.Create();
                }

                _maskCam.CopyFrom(_cam);
                _maskCam.cullingMask = solidMaskLayers;
                _maskCam.targetTexture = _solidDepthRT;
                _maskCam.clearFlags = CameraClearFlags.SolidColor;
                _maskCam.backgroundColor = Color.black;
                _maskCam.depthTextureMode = DepthTextureMode.None;

                // Feed replacement shader params globally
                Shader.SetGlobalFloat(_useAlphaTexMask, solidMaskUseAlphaTex ? 1f : 0f);
                Shader.SetGlobalFloat(_alphaTexCutoff, solidAlphaTexCutoff);

                _maskCam.RenderWithShader(solidDepthShader, "");

                // Feed tint shader
                _mat.SetFloat(_useSolidMask, 1f);
                _mat.SetFloat(_solidMaskEpsilon, solidMaskEpsilon);
                _mat.SetTexture(_solidDepthTex, _solidDepthRT);
            }
            else
            {
                _mat.SetFloat(_useSolidMask, 0f);
            }

            Graphics.Blit(src, dest, _mat);
        }
    }
}