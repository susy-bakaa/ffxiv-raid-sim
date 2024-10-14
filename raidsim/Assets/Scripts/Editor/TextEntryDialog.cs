using UnityEditor;
using UnityEngine;

namespace susy_baka.WaveSurvivalGame.Editor
{
    public class TextEntryDialog : EditorWindow
    {
        const string k_textFieldName = "textentry";

        string m_entry;
        string m_entryFieldResult;

        bool m_firstPass = true;
        bool m_showValidation = false;

        GUIStyle m_validationGUIStyle = null;

        public static string Show(string windowName, string entry)
        {
            TextEntryDialog dialog = CreateInstance<TextEntryDialog>();
            dialog.titleContent = new GUIContent(windowName);

            dialog.maxSize = new Vector2(320, 120);
            dialog.minSize = new Vector2(320, 120);

            dialog.m_entry = entry;
            dialog.m_entryFieldResult = entry;

            dialog.ShowModal();

            return dialog.m_entryFieldResult;
        }

        void OnGUI()
        {
            if (Event.current != null && Event.current.isKey)
            {
                switch(Event.current.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        Accepted();
                        break;
                    case KeyCode.Escape:
                        Cancelled();
                        break;
                }
            }

            GUILayout.Space(20);

            GUI.SetNextControlName(k_textFieldName);
            m_entryFieldResult = EditorGUILayout.TextField(m_entryFieldResult, GUILayout.Width(310));

            if (m_validationGUIStyle == null)
            {
                m_validationGUIStyle = new GUIStyle();
                m_validationGUIStyle.normal.textColor = Color.red;
            }

            if (m_showValidation)
            {
                if (!string.IsNullOrEmpty(m_entryFieldResult))
                {
                    m_showValidation = false;
                }

                GUILayout.Label("No entry set", m_validationGUIStyle, GUILayout.Width(300));
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Cancelled();
            }

            if (GUILayout.Button("OK", GUILayout.Width(100)))
            {
                Accepted();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (m_firstPass)
            {
                m_firstPass = false;
                GUI.FocusControl(k_textFieldName);
            }
        }

        void Accepted()
        {
            if (!string.IsNullOrEmpty(m_entryFieldResult))
            {
                Close();
            }
        }

        void Cancelled()
        {
            m_entryFieldResult = m_entry;
            Close();
        }
    }
}
