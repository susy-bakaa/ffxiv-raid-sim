using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using dev.susybaka.Shared.Audio;

namespace dev.susybaka.Shared.Editor
{
    [CustomPropertyDrawer(typeof(SoundNameAttribute))]
    public class SoundNameDrawer : PropertyDrawer
    {
        string[] _cachedNames;
        double _lastRefreshTime;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            // Only on strings
            if (prop.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(pos, "[SoundName] only works on strings", MessageType.Error);
                return;
            }

            // Get the prefab-path from the attribute
            var attr = (SoundNameAttribute)attribute;
            string path = attr.prefabPath;

            // Throttle how often we re-load the asset (every 2s in editor)
            if (_cachedNames == null || EditorApplication.timeSinceStartup - _lastRefreshTime > 2.0)
            {
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                _cachedNames = LoadSoundNamesFromPrefab(path);
            }

            // If none found, draw a warning & fallback to text field
            if (_cachedNames == null || _cachedNames.Length == 0)
            {
                EditorGUI.PropertyField(pos, prop, label);
                EditorGUI.HelpBox(
                    new Rect(pos.x, pos.y + pos.height, pos.width, 30),
                    $"No sounds found in prefab at:\n{path}",
                    MessageType.Warning
                );
                return;
            }

            // Find currently selected index
            int idx = System.Array.IndexOf(_cachedNames, prop.stringValue);
            if (idx < 0)
                idx = 0;

            if (prop.stringValue == string.Empty)
                idx = 0;

            // Draw popup
            idx = EditorGUI.Popup(pos, label.text, idx, _cachedNames);

            // Assign back
            prop.stringValue = _cachedNames[idx];
        }

        private string[] LoadSoundNamesFromPrefab(string assetPath)
        {
            // Try load the prefab itself as a GameObject
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go == null)
                return new string[0];

            // Find the AudioManager component on it
            var mgr = go.GetComponent<AudioManager>();
            if (mgr == null || mgr.sounds == null)
                return new string[0];

            List<string> names = new List<string>();

            // Add a default "None" option
            names.Add("<None>");

            names.AddRange(mgr.sounds.Select(s => s.name));

            // Extract the names
            return names.ToArray();
        }
    }
}