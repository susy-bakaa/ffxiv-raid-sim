// From https://answers.unity.com/questions/1654475/why-doesnt-unity-property-editor-show-a-nullable-v.html
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Serializable Nullable (SN) Does the same as C# System.Nullable, except it's an ordinary
/// serializable struct, allowing unity to serialize it and show it in the inspector.
/// </summary>
[System.Serializable]
public struct SN<T> where T : struct
{
    public T Value
    {
        get
        {
            if (!HasValue)
                throw new System.InvalidOperationException("Serializable nullable object must have a value.");
            return v;
        }
    }

    public bool HasValue { get { return hasValue; } }

    [SerializeField]
    private T v;

    [SerializeField]
    private bool hasValue;

    public SN(bool hasValue, T v)
    {
        this.v = v;
        this.hasValue = hasValue;
    }

    private SN(T v)
    {
        this.v = v;
        this.hasValue = true;
    }

    public static implicit operator SN<T>(T value)
    {
        return new SN<T>(value);
    }

    public static implicit operator SN<T>(System.Nullable<T> value)
    {
        return value.HasValue ? new SN<T>(value.Value) : new SN<T>();
    }

    public static implicit operator System.Nullable<T>(SN<T> value)
    {
        return value.HasValue ? (T?)value.Value : null;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SN<>))]
internal class SNDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var setRect = new Rect(position.x, position.y, 15, position.height);
        var consumed = setRect.width + 5;
        var valueRect = new Rect(position.x + consumed, position.y, position.width - consumed, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        var hasValueProp = property.FindPropertyRelative("hasValue");
        EditorGUI.PropertyField(setRect, hasValueProp, GUIContent.none);
        bool guiEnabled = GUI.enabled;
        GUI.enabled = guiEnabled && hasValueProp.boolValue;
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("v"), GUIContent.none);
        GUI.enabled = guiEnabled;

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
#endif