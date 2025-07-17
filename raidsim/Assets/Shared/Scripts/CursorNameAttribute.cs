using UnityEngine;

namespace dev.susybaka.Shared.Attributes
{
    public class CursorNameAttribute : PropertyAttribute
    {
        /// <summary>
        /// Relative path inside your Assets folder to the AudioManager prefab.
        /// </summary>
        public string prefabPath;

        public CursorNameAttribute(string prefabPath = "Assets/Resources/Prefabs/CursorHandler.prefab")
        {
            this.prefabPath = prefabPath;
        }
    }
}