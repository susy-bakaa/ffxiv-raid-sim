using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace dev.susybaka.Shared.UserInterface
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CursorHandler : MonoBehaviour
    {
        public static CursorHandler Instance { get; private set; }

        [Header("Cursor Settings")]
        public bool visible = true;
        public bool useSoftwareCursor = false;

        [Header("Cursor Definitions")]
        public List<CursorData> cursors;

        [Header("Debug Settings")]
        public bool forceShowHardwareCursor = true;

        private CanvasGroup softwareCursorCanvasGroup;
        private RawImage softwareCursorImage;
        private CursorData currentCursor;
        private Dictionary<string, CursorData> nameLookup;
        private Dictionary<int, CursorData> idLookup;

#if UNITY_STANDALONE_LINUX
        private readonly Dictionary<string, Texture2D> linuxScaledCursorCache = new Dictionary<string, Texture2D>();
#endif

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < cursors.Count; i++)
            {
                CursorData c = cursors[i];
                c.id = i;
                cursors[i] = c;
            }
        }
#endif

        private void Awake()
        {
            Instance = this;

            softwareCursorCanvasGroup = GetComponent<CanvasGroup>();
            softwareCursorImage = GetComponentInChildren<RawImage>(true);

            // Build lookups for quick access
            nameLookup = new Dictionary<string, CursorData>();
            idLookup = new Dictionary<int, CursorData>();

            foreach (var cursor in cursors)
            {
                if (!nameLookup.ContainsKey(cursor.name))
                    nameLookup.Add(cursor.name, cursor);

                if (!idLookup.ContainsKey(cursor.id))
                    idLookup.Add(cursor.id, cursor);
            }

#if UNITY_WEBPLAYER
            Cursor.visible = false;
            useSoftwareCursor = true; // Force software cursor on WebGL
#else
            SetCursorByID(0); // Default cursor
            SetSoftwareCursorVisibility(useSoftwareCursor);
#endif
        }

        private void Update()
        {
#if UNITY_WEBPLAYER
            Cursor.visible = false;
            useSoftwareCursor = true; // Force software cursor on WebGL
#endif
            if (useSoftwareCursor)
            {
                if (softwareCursorCanvasGroup != null)
                {
#if !UNITY_WEBPLAYER
                    if (!forceShowHardwareCursor)
                        Cursor.visible = false;
                    else
                        Cursor.visible = true;
#endif

                    if (visible)
                    {
                        if (softwareCursorCanvasGroup.alpha < 1f)
                        {
                            softwareCursorCanvasGroup.alpha = 1f;
                        }
                    }
                    else
                    {
                        if (softwareCursorCanvasGroup.alpha > 0f)
                        {
                            softwareCursorCanvasGroup.alpha = 0f;
                        }
                    }
                }

                transform.position = new Vector3(Input.mousePosition.x + currentCursor.softwareOffset.x, Input.mousePosition.y + currentCursor.softwareOffset.y, Input.mousePosition.z);
            }
            else
            {
                if (softwareCursorCanvasGroup != null && softwareCursorCanvasGroup.alpha > 0f)
                {
                    softwareCursorCanvasGroup.alpha = 0f;

#if !UNITY_WEBPLAYER
                    if (!forceShowHardwareCursor)
                        Cursor.visible = visible;
                    else
                        Cursor.visible = true;
#endif
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

#if UNITY_STANDALONE_LINUX
            foreach (var kv in linuxScaledCursorCache)
            {
                if (kv.Value != null)
                    Destroy(kv.Value);
            }
            linuxScaledCursorCache.Clear();
#endif
        }

        public void SetSoftwareCursorVisibility(bool visible)
        {
#if UNITY_WEBPLAYER
            useSoftwareCursor = true; // Force software cursor on WebGL
            return;
#endif
            useSoftwareCursor = visible;
        }

        public void SetCursorVisibility(bool visible)
        {
            this.visible = visible;
        }

        public void SetCursorByID(int id)
        {
            if (id < 0)
                return;

            if (idLookup.TryGetValue(id, out var cursor))
            {
                ApplyCursor(cursor);
            }
            else
            {
                Debug.LogWarning($"Cursor with ID {id} not found.");
            }
        }

        public void SetCursorByName(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "<None>")
                return;

            if (nameLookup.TryGetValue(name, out var cursor))
            {
                ApplyCursor(cursor);
            }
            else
            {
                Debug.LogWarning($"Cursor with name '{name}' not found.");
            }
        }

        private void ApplyCursor(CursorData cursor)
        {
            if (cursor.texture == null)
            {
                Debug.LogWarning($"Cursor '{cursor.name}' has no texture assigned.");
                return;
            }

#if !UNITY_WEBPLAYER
            if (!useSoftwareCursor)
            {
#if UNITY_STANDALONE_LINUX
                // Decide target cursor size: compositor hint or sensible default (32)
                int target;
                if (!TryGetLinuxCursorTargetSize(out target))
                    target = 32;

                // Downscale hi-res art to compositor size (or close) and scale hotspot to match
                float scale;
                Texture2D linuxTex = GetScaledCursorTexture(cursor.texture, target, out scale);
                Vector2 hsScaled = new Vector2(Mathf.Round(cursor.hotspot.x * scale), Mathf.Round(cursor.hotspot.y * scale));

                // Force Unity to draw it (bypasses hardware cursor scaling issues)
                Cursor.SetCursor(linuxTex, hsScaled, CursorMode.ForceSoftware);
#else
                Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
#endif
            }
#endif

            if (softwareCursorImage != null)
            {
                softwareCursorImage.texture = cursor.texture;
            }
            currentCursor = cursor;
        }

#if UNITY_STANDALONE_LINUX
        private static bool TryGetLinuxCursorTargetSize(out int size)
        {
            size = 0;
            try
            {
                var env = System.Environment.GetEnvironmentVariable("XCURSOR_SIZE");
                if (!string.IsNullOrEmpty(env) && int.TryParse(env, out var s) && s > 0)
                {
                    size = Mathf.Clamp(s, 16, 128);
                    return true;
                }
            }
            catch { /* ignore */ }
            return false;
        }

        // Returns a scaled copy and the scale factor used (relative to src).
        private Texture2D GetScaledCursorTexture(Texture2D src, int targetMaxEdge, out float scale)
        {
            // Preserve aspect; scale so the longest edge == targetMaxEdge
            int srcW = src.width, srcH = src.height;
            int longest = Mathf.Max(srcW, srcH);
            scale = (longest > 0) ? (targetMaxEdge / (float)longest) : 1f;

            int dstW = Mathf.Max(1, Mathf.RoundToInt(srcW * scale));
            int dstH = Mathf.Max(1, Mathf.RoundToInt(srcH * scale));

            string key = $"{src.GetInstanceID()}:{dstW}x{dstH}";
            if (linuxScaledCursorCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            // GPU downscale (no need for src to be readable)
            var rt = RenderTexture.GetTemporary(dstW, dstH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(src, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(dstW, dstH, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, dstW, dstH), 0, 0, false);
            tex.Apply(false, true);
            tex.filterMode = FilterMode.Point; // crisp cursor

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            linuxScaledCursorCache[key] = tex;
            return tex;
        }
#endif

        [System.Serializable]
        public struct CursorData
        {
            public string name;
            [Min(0)] public int id;
            public Texture2D texture;
            public Vector2 hotspot;
            public Vector2 softwareOffset;
        }
    }
}