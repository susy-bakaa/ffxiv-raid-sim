using UnityEngine;
using UnityEditor;
using dev.susybaka.Shared.Attributes;

namespace dev.susybaka.Shared.Editor 
{
    [CustomPropertyDrawer(typeof(AssetBundleNameAttribute))]
    public class AssetBundleNameDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
            string current = property.stringValue;
            int idx = System.Array.IndexOf(allBundles, current);
            if (idx < 0)
                idx = 0;

            int sel = EditorGUI.Popup(position, label.text, idx, allBundles);
            property.stringValue = allBundles.Length > 0 ? allBundles[sel] : current;
        }
    }
}