using dev.susybaka.raidsim.Actions;
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
            GUIUtility.systemCopyBuffer = code;
            Debug.Log($"Copied macro share code ({code.Length} chars) to clipboard.");
        }

        public void ImportFromClipboardOverwrite()
        {
            var text = GUIUtility.systemCopyBuffer;

            if (!MacroShareCode.TryDecode(text, out var snap))
            {
                Debug.LogWarning("Clipboard does not contain a valid macro share code.");
                return;
            }

            MacroSnapshots.ApplySnapshotOverwrite(library, snap);
            Debug.Log("Imported macros from clipboard (overwrite).");
        }
    }
}