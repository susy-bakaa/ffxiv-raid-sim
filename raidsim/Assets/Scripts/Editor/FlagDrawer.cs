using UnityEditor;
using UnityEngine;
using static GlobalData;

[CustomPropertyDrawer(typeof(Flag))]
public class FlagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the Flag instance to access its value
        Flag flagInstance = fieldInfo.GetValue(property.serializedObject.targetObject) as Flag;

        // Ensure the Flag's runtime dictionary is up-to-date
        if (flagInstance != null)
        {
            flagInstance.ForceUpdate();
        }

        // Get the current state of the Flag
        string flagState = flagInstance != null ? flagInstance.value.ToString() : "N/A";

        // Add the Flag Value to the label when collapsed
        string foldoutLabel = property.isExpanded ? label.text : $"{label.text} - {flagState}";

        // Repaint the Inspector to keep the value updated
        if (Event.current.type == EventType.Repaint)
        {
            HandleUtility.Repaint();
        }

        // Calculate the foldout rectangle
        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Check if the mouse is hovering over the foldout row
        if (foldoutRect.Contains(Event.current.mousePosition))
        {
            // Draw a highlight when hovered
            EditorGUI.DrawRect(foldoutRect, new Color(0.5f, 0.5f, 0.5f, 0.2f)); // Light gray highlight

            // Request a repaint for instant responsiveness
            //HandleUtility.Repaint();
        }

        // Enable folding using the default foldout behavior
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true); // `true` makes the label clickable

        if (!property.isExpanded)
            return; // If not expanded, don't draw further

        EditorGUI.BeginProperty(position, label, property);

        // Increase indentation level
        EditorGUI.indentLevel++;

        // Calculate heights and spacing
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        float yOffset = position.y + lineHeight; // Start below the foldout

        // Draw Flag Name
        SerializedProperty nameProp = property.FindPropertyRelative("name");
        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, lineHeight), nameProp, new GUIContent("Name"));
        yOffset += lineHeight;

        // Draw Aggregate Logic
        SerializedProperty logicProp = property.FindPropertyRelative("logic");
        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, lineHeight), logicProp, new GUIContent("Logic"));
        yOffset += lineHeight;

        // Draw Threshold Percentage (only if Threshold logic is selected)
        if ((Flag.AggregateLogic)logicProp.enumValueIndex == Flag.AggregateLogic.Threshold)
        {
            SerializedProperty thresholdProp = property.FindPropertyRelative("thresholdPercentage");
            EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, lineHeight), thresholdProp, new GUIContent("Threshold %"));
            yOffset += lineHeight;
        }

        // Draw Flag Values List
        SerializedProperty valuesProp = property.FindPropertyRelative("values");
        EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, lineHeight), valuesProp, new GUIContent("Flag Values"), true);

        // Adjust yOffset dynamically based on whether the list is expanded
        if (valuesProp.isExpanded)
        {
            yOffset += EditorGUI.GetPropertyHeight(valuesProp, true) + 2; // Add height for the expanded list
        }
        else
        {
            yOffset += lineHeight; // Add height for the collapsed list
        }

        // Draw Flag Value (readonly display)
        //Flag flagInstance = fieldInfo.GetValue(property.serializedObject.targetObject) as Flag;

        // Use a darkened background for readability
        var valueRect = new Rect(position.x, yOffset, position.width, lineHeight);
        EditorGUI.DrawRect(valueRect, new Color(0.1f, 0.1f, 0.1f, 0.3f)); // Darker background

        if (flagInstance != null)
        {
            // Trigger a runtime dictionary update
            flagInstance.ForceUpdate();

            EditorGUI.LabelField(valueRect, "Flag Value", flagInstance.value.ToString());
        }
        else
        {
            EditorGUI.LabelField(valueRect, "Flag Value", "N/A");
        }

        // Reset indentation level
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        float height = lineHeight; // Foldout height

        if (!property.isExpanded)
            return height; // Return only the foldout height if collapsed

        // Add heights for all visible fields
        height += lineHeight * 2; // Name and Logic fields
        SerializedProperty logicProp = property.FindPropertyRelative("logic");
        if ((Flag.AggregateLogic)logicProp.enumValueIndex == Flag.AggregateLogic.Threshold)
        {
            height += lineHeight; // Add for Threshold field if logic is Threshold
        }

        SerializedProperty valuesProp = property.FindPropertyRelative("values");
        height += EditorGUI.GetPropertyHeight(valuesProp, true) + 2; // Add height for Flag Values list

        height += lineHeight; // Add height for Flag Value (readonly display)
        return height;
    }
}
