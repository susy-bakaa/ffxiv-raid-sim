// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using UnityEngine;
using UnityEditor;

namespace dev.susybaka.Shared.Editor
{
    public class TimeConversionToolWindow : EditorWindow
    {
        int hours;
        int minutes;
        int seconds;
        int frames;
        int frameRate = 60; // default 60fps

        float inputSeconds; // for reverse conversion

        [MenuItem("Tools/Time to Float Converter")]
        public static void ShowWindow()
        {
            TimeConversionToolWindow window = GetWindow<TimeConversionToolWindow>("Time Converter");

            // Set the icon for the window using Unity's default scene icon
            GUIContent titleContent = new GUIContent("Time Converter", EditorGUIUtility.IconContent("d_UnityEditor.AnimationWindow").image);
            window.titleContent = titleContent;
        }

        private void OnGUI()
        {
            GUILayout.Label("Video Time Input", EditorStyles.boldLabel);

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("H : M : S : F");

            float totalWidth = EditorGUIUtility.currentViewWidth - 150; // Reserve some for the label
            float fieldWidth = totalWidth / 4f - 5; // 4 fields, little padding

            hours = EditorGUILayout.IntField(hours, GUILayout.Width(fieldWidth));
            minutes = EditorGUILayout.IntField(minutes, GUILayout.Width(fieldWidth));
            seconds = EditorGUILayout.IntField(seconds, GUILayout.Width(fieldWidth));
            frames = EditorGUILayout.IntField(frames, GUILayout.Width(fieldWidth));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            frameRate = EditorGUILayout.IntSlider("Frame Rate", frameRate, 1, 120);

            GUILayout.Space(10);

            if (GUILayout.Button("Convert to Seconds"))
            {
                float totalSeconds = ConvertToSeconds(hours, minutes, seconds, frames, frameRate);
                EditorGUIUtility.systemCopyBuffer = totalSeconds.ToString("F6");
                Debug.Log($"Converted Time: {totalSeconds:F6} seconds (copied to clipboard)");
            }

            GUILayout.Space(20);

            GUILayout.Label("Float Seconds to Video Time", EditorStyles.boldLabel);

            GUILayout.Space(5);

            inputSeconds = EditorGUILayout.FloatField("Seconds Input", inputSeconds);

            GUILayout.Space(10);

            if (GUILayout.Button("Convert to H:M:S:F"))
            {
                string timecode = ConvertToTimecode(inputSeconds, frameRate);
                EditorGUIUtility.systemCopyBuffer = timecode;
                Debug.Log($"Converted Timecode: {timecode} (copied to clipboard)");
            }
        }


        float ConvertToSeconds(int h, int m, int s, int f, int fps)
        {
            return (h * 3600f) + (m * 60f) + s + (f / (float)fps);
        }

        string ConvertToTimecode(float totalSeconds, int fps)
        {
            int h = Mathf.FloorToInt(totalSeconds / 3600f);
            totalSeconds -= h * 3600f;

            int m = Mathf.FloorToInt(totalSeconds / 60f);
            totalSeconds -= m * 60f;

            int s = Mathf.FloorToInt(totalSeconds);
            totalSeconds -= s;

            int f = Mathf.FloorToInt(totalSeconds * fps + 0.5f); // round to nearest frame

            // handle case where rounding pushed frames to fps (e.g., 59.9999 rounds to 60)
            if (f >= fps)
            {
                f = 0;
                s++;
                if (s >= 60)
                {
                    s = 0;
                    m++;
                    if (m >= 60)
                    {
                        m = 0;
                        h++;
                    }
                }
            }

            return $"{h:00}:{m:00}:{s:00}:{f:00}";
        }
    }
}