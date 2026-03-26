using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace dev.susybaka.raidsim.Core
{
    public static class WebGLGlobalPaste
    {
        public static event Action<string> Pasted;
        public static string LastPastedText { get; private set; } = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
        private delegate void StringCallback(string value);

        [DllImport("__Internal")]
        private static extern void InitWebGLGlobalPaste(StringCallback callback);
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            InitWebGLGlobalPaste(OnBrowserPaste);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [MonoPInvokeCallback(typeof(StringCallback))]
#endif
        private static void OnBrowserPaste(string value)
        {
            LastPastedText = value ?? string.Empty;

            // Mirror browser paste into Unity's clipboard-like buffer
            //GUIUtility.systemCopyBuffer = LastPastedText;

            // Let gameplay/UI react immediately if it wants
            Pasted?.Invoke(LastPastedText);
        }
    }
}