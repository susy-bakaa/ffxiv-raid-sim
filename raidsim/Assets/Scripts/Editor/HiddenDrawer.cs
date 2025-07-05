using UnityEngine;
using UnityEditor;

namespace dev.susybaka.Shared.Editor
{
    [CustomPropertyDrawer(typeof(HiddenAttribute))]
    class HiddenDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
    }
}