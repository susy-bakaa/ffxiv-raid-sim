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
            Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
#endif

            if (softwareCursorImage != null)
            {
                softwareCursorImage.texture = cursor.texture;
            }
            currentCursor = cursor;
        }

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