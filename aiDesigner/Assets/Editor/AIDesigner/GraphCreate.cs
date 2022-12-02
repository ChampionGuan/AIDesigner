using System.IO;
using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphCreate : Singleton<GraphCreate>
    {
        public bool IsDisplay { get; private set; }

        private Rect m_graphRect;

        private string m_name;
        private string m_directory;
        private int m_copyFromTreeIndex;

        private string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(m_directory))
                {
                    return m_name;
                }

                return $"{m_directory}{m_name}";
            }
        }

        public GraphCreate()
        {
            m_directory = (string) StoragePrefs.GetPref(PrefsType.TreeDirectory);
            if (!string.IsNullOrEmpty(m_directory) && !Directory.Exists($"{Define.CustomSettings.AppDataPath}/{Define.ConfigFullPath}{m_directory}"))
            {
                m_directory = null;
                StoragePrefs.SetPref(PrefsType.TreeDirectory, m_directory);
            }
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(AIDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, (float) (18 + (EditorGUIUtility.isProSkin ? 1 : 2)), 300f, 35 + 20 * 3);
            }

            if (!IsDisplay)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, AIDesignerUIUtility.PreferencesPaneGUIStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 40);
            EditorGUILayout.LabelField("Create Tree", AIDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(AIDesignerUIUtility.DeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                DisplaySwitch();
                return;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Directory", GUILayout.Width(90)))
            {
                AIDesignerLogicUtility.OpenConfigPathFolder(ref m_directory);
                StoragePrefs.SetPref(PrefsType.TreeDirectory, m_directory);
            }

            EditorGUILayout.TextField(m_directory);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            var enabled = TreeStructure.IsLegalName(m_name) && !TreeReader.AllTreesName.Contains(FullName);

            GUI.enabled = enabled;
            if (GUILayout.Button("Create", GUILayout.Width(90)) && CommandMgr.Instance.Do<CommandCreateTree>(FullName))
            {
                IsDisplay = false;
            }

            GUI.enabled = true;
            m_name = EditorGUILayout.TextField(m_name);
            GUILayout.EndHorizontal();

            GUI.enabled = enabled;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create From", GUILayout.Width(90)))
            {
                var from = TreeReader.AllTreesName[m_copyFromTreeIndex];
                if (CommandMgr.Instance.Do<CommandCreateTree>(FullName))
                {
                    CommandMgr.Instance.Do<CommandCreateTreeFromTemplate>(from);
                }
            }

            m_copyFromTreeIndex = m_copyFromTreeIndex > TreeReader.AllTreesName.Count - 1 ? TreeReader.AllTreesName.Count - 1 : m_copyFromTreeIndex;
            m_copyFromTreeIndex = EditorGUILayout.Popup(m_copyFromTreeIndex, TreeReader.AllTreesName.ToArray());
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            GUILayout.EndArea();
        }

        public void DisplaySwitch()
        {
            IsDisplay = !IsDisplay;
            if (IsDisplay)
            {
                m_copyFromTreeIndex = 0;

                if (GraphPreferences.Instance.IsDisplay)
                {
                    GraphPreferences.Instance.DisplaySwitch();
                }

                if (GraphHelp.Instance.IsDisplay)
                {
                    GraphHelp.Instance.DisplaySwitch();
                }
            }
        }
    }
}