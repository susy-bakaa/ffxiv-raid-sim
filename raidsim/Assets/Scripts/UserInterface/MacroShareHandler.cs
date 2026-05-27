using dev.susybaka.raidsim.Actions;
using dev.susybaka.raidsim.Core;
using UnityEngine;

namespace dev.susybaka.raidsim.UI
{
    public sealed class MacroShareHandler : MonoBehaviour
    {
        [SerializeField] private MacroLibrary library;

        public void CopyToClipboard()
        {
            var snap = MacroSnapshots.CreateSparseSnapshot(library);
            Debug.Log($"Exporting {snap.slots?.Length ?? 0} macros out of {MacroLibrary.Count}");
            var code = MacroShareCode.Encode(snap);
#if !UNITY_WEBGL
            GUIUtility.systemCopyBuffer = code;
#else
            WebGLCopyAndPaste.WebGLCopyAndPasteAPI.CopyToClipboard(code);
#endif
            if (Core.ChatHandler.Instance != null)
            {
                Core.ChatHandler.Instance.PostSystem("Macro library share code copied to clipboard.");
            }
            Debug.Log($"Copied macro share code ({code.Length} chars) to clipboard.");
        }

        public void ImportFromClipboardOverwrite()
        {
            string text = GUIUtility.systemCopyBuffer;
            string text2 = WebGLGlobalPaste.LastPastedText;

            if (text.StartsWith("TC"))
            {
                ImportFromClipboardOverwrite(text);

            }
            else
            {
                ImportFromClipboardOverwrite(text2);
            }        
        }

        private bool ImportFromClipboardOverwrite(string text)
        {
            if (!MacroShareCode.TryDecode(text, out var snap))
            {
#if !UNITY_WEBGL
                if (Core.ChatHandler.Instance != null)
                {
                    Core.ChatHandler.Instance.PostSystem("Clipboard does not contain a valid macro library share code.", Core.ChatKind.Error);
                }
#else
                if (Core.ChatHandler.Instance != null)
                {
                    Core.ChatHandler.Instance.PostSystem("Clipboard does not contain a valid macro library share code! Try pressing Ctrl+V to paste in the game window, in order to first copy your system clipboard over to the game.", Core.ChatKind.Error);
                }
#endif
                Debug.LogWarning("Clipboard does not contain a valid macro share code.");
                return false;
            }

            MacroSnapshots.ApplySnapshotOverwrite(library, snap);
            if (Core.ChatHandler.Instance != null)
            {
                Core.ChatHandler.Instance.PostSystem("Imported macro library from clipboard.");
            }
            Debug.Log("Imported macros from clipboard (overwrite).");
            return true;
        }
    }
}