using UnityEngine;
using UnityEditor;

namespace susy_baka.Raidsim.Editor
{
    public class RoundAoeShaderInspector : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Find properties
            MaterialProperty outerRadiusProp = FindProperty("_OuterRadius", properties);
            MaterialProperty innerRadiusProp = FindProperty("_InnerRadius", properties);
            MaterialProperty innerRatioProp = FindProperty("_InnerRatio", properties);
            MaterialProperty maxFillProp = FindProperty("_MaxFill", properties);
            MaterialProperty minFillProp = FindProperty("_MinFill", properties);
            MaterialProperty glowProp = FindProperty("_Glow", properties);
            MaterialProperty glowOpacityProp = FindProperty("_GlowOpacity", properties);
            MaterialProperty outlineProp = FindProperty("_Outline", properties);
            MaterialProperty outlineOpacityProp = FindProperty("_OutlineOpacity", properties);
            MaterialProperty pulseSpeedProp = FindProperty("_PulseSpeed", properties);
            MaterialProperty fadeDurationProp = FindProperty("_FadeDuration", properties);
            MaterialProperty angleProp = FindProperty("_Angle", properties);
            MaterialProperty angularOutlineProp = FindProperty("_AngularOutline", properties);
            MaterialProperty tintProp = FindProperty("_TintColor", properties);
            MaterialProperty innerTintProp = FindProperty("_InnerTintColor", properties);
            MaterialProperty glowTintProp = FindProperty("_GlowTintColor", properties);
            MaterialProperty outlineTintProp = FindProperty("_OutlineTintColor", properties);
            MaterialProperty innerOpacityProp = FindProperty("_InnerOpacity", properties);
            MaterialProperty alphaProp = FindProperty("_Alpha", properties);
            MaterialProperty cullProp = FindProperty("_DoubleSided", properties);

            // Clamp the properties
            outerRadiusProp.floatValue = Mathf.Max(outerRadiusProp.floatValue, 0);
            innerRadiusProp.floatValue = Mathf.Clamp(innerRadiusProp.floatValue, 0.0f, outerRadiusProp.floatValue);
            innerRatioProp.floatValue = Mathf.Clamp01(innerRatioProp.floatValue);
            maxFillProp.floatValue = Mathf.Clamp01(maxFillProp.floatValue);
            minFillProp.floatValue = Mathf.Clamp01(minFillProp.floatValue);
            glowProp.floatValue = Mathf.Max(glowProp.floatValue, 0);
            glowOpacityProp.floatValue = Mathf.Clamp01(glowOpacityProp.floatValue);
            outlineOpacityProp.floatValue = Mathf.Clamp01(outlineOpacityProp.floatValue);
            pulseSpeedProp.floatValue = Mathf.Max(pulseSpeedProp.floatValue, 0);
            fadeDurationProp.floatValue = Mathf.Max(fadeDurationProp.floatValue, 0);
            angleProp.floatValue = Mathf.Clamp(angleProp.floatValue, 0.0f, 360.0f);
            innerOpacityProp.floatValue = Mathf.Clamp01(innerOpacityProp.floatValue);
            alphaProp.floatValue = Mathf.Clamp(alphaProp.floatValue, 0.0f, 2.0f);

            GUILayout.Label("Shape", EditorStyles.boldLabel);

            tintProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Color"), tintProp.colorValue, true, true, false);
            angleProp.floatValue = EditorGUILayout.Slider("Angle", angleProp.floatValue, angleProp.rangeLimits.x, angleProp.rangeLimits.y);
            outerRadiusProp.floatValue = EditorGUILayout.FloatField("Outer Radius", outerRadiusProp.floatValue);
            innerRadiusProp.floatValue = EditorGUILayout.Slider("Inner Radius", innerRadiusProp.floatValue, 0.0f, outerRadiusProp.floatValue);
            maxFillProp.floatValue = EditorGUILayout.Slider("Max Fill", maxFillProp.floatValue, maxFillProp.rangeLimits.x, maxFillProp.rangeLimits.y);
            minFillProp.floatValue = EditorGUILayout.Slider("Min Fill", minFillProp.floatValue, minFillProp.rangeLimits.x, minFillProp.rangeLimits.y);

            GUILayout.Space(20);
            GUILayout.Label("Inner Shape", EditorStyles.boldLabel);

            innerTintProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Color"), innerTintProp.colorValue, true, false, false);
            innerOpacityProp.floatValue = EditorGUILayout.Slider("Opacity", innerOpacityProp.floatValue, innerOpacityProp.rangeLimits.x, innerOpacityProp.rangeLimits.y);
            innerRatioProp.floatValue = EditorGUILayout.Slider("Ratio", innerRatioProp.floatValue, innerRatioProp.rangeLimits.x, innerRatioProp.rangeLimits.y);
            pulseSpeedProp.floatValue = EditorGUILayout.FloatField("Speed", pulseSpeedProp.floatValue);
            fadeDurationProp.floatValue = EditorGUILayout.FloatField("Fade Length", fadeDurationProp.floatValue);

            // Scale the properties for display
            float angularScaledValue = angularOutlineProp.floatValue * 1000.0f;
            float outlineScaledValue = outlineProp.floatValue * 1000.0f;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(20);
            GUILayout.Label("Edge", EditorStyles.boldLabel);

            outlineTintProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Outline Color"), outlineTintProp.colorValue, true, false, false);
            outlineScaledValue = EditorGUILayout.FloatField("Outline Thickness", outlineScaledValue);
            angularScaledValue = EditorGUILayout.FloatField("Angular Outline Thickness", angularScaledValue);
            outlineOpacityProp.floatValue = EditorGUILayout.Slider("Outline Opacity", outlineOpacityProp.floatValue, outlineOpacityProp.rangeLimits.x, outlineOpacityProp.rangeLimits.y);
            glowTintProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Glow Color"), glowTintProp.colorValue, true, false, false);
            glowProp.floatValue = EditorGUILayout.FloatField("Glow Strength", glowProp.floatValue);
            glowOpacityProp.floatValue = EditorGUILayout.Slider("Glow Opacity", glowOpacityProp.floatValue, glowOpacityProp.rangeLimits.x, glowOpacityProp.rangeLimits.y);

            if (EditorGUI.EndChangeCheck())
            {
                // Update the original property value
                angularOutlineProp.floatValue = angularScaledValue / 1000.0f;
                outlineProp.floatValue = outlineScaledValue / 1000.0f;

                // Ensure the value is within valid range or constraints
                angularOutlineProp.floatValue = Mathf.Max(angularOutlineProp.floatValue, 0);
                outlineProp.floatValue = Mathf.Max(outlineProp.floatValue, 0);
            }

            GUILayout.Space(20);

            GUILayout.Label("Other", EditorStyles.boldLabel);

            alphaProp.floatValue = EditorGUILayout.Slider("Opacity", alphaProp.floatValue, alphaProp.rangeLimits.x, alphaProp.rangeLimits.y);
            cullProp.floatValue = EditorGUILayout.Popup("Cull Mode", (int)cullProp.floatValue, new string[] { "Off", "Back", "Front" });

            GUILayout.Space(-10);

            // Update material properties
            materialEditor.PropertiesChanged();

            base.OnGUI(materialEditor, properties);
        }
    }
}