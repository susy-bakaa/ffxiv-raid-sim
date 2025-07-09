using UnityEngine;

namespace dev.susybaka.Shared.Editor
{
    public class SoundNameAttribute : PropertyAttribute
    {
        /// <summary>
        /// Relative path inside your Assets folder to the AudioManager prefab.
        /// </summary>
        public string prefabPath;

        public SoundNameAttribute(string prefabPath = "Assets/Resources/Prefabs/AudioManager.prefab")
        {
            this.prefabPath = prefabPath;
        }
    }
}