using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphHelp : Singleton<GraphHelp>
    {
        public bool IsDisplay { get; private set; }
        public string FindTaskName { get; private set; }
        public string FindVariableName { get; private set; }
        public bool AllTasksFoldout { get; private set; }

        private Rect m_graphRect;
        private int m_copyFromTreeIndex;
        private int m_findVariableIndex;
        private List<string> m_sharedVariables = new List<string>();

        public TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(AIDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, (float) (18 + (EditorGUIUtility.isProSkin ? 1 : 2)), 300f, 35 + 20 * 8);
            }

            if (!IsDisplay)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, AIDesignerUIUtility.PreferencesPaneGUIStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 20);
            EditorGUILayout.LabelField("Help", AIDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(AIDesignerUIUtility.DeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                DisplaySwitch();
                return;
            }

            GUILayout.EndHorizontal();

            var enable = null != CurrTree;
            GUI.enabled = enable;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Find Task", GUILayout.Width(60));
            FindTaskName = EditorGUILayout.TextField(FindTaskName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Find Variable", GUILayout.Width(80));
            m_sharedVariables.Clear();
            m_sharedVariables.Add("(None)");
            if (null != CurrTree)
            {
                foreach (var variable in CurrTree.Variables)
                {
                    m_sharedVariables.Add(variable.Key);
                }
            }

            m_findVariableIndex = m_findVariableIndex > m_sharedVariables.Count - 1 ? m_sharedVariables.Count - 1 : m_findVariableIndex;
            m_findVariableIndex = EditorGUILayout.Popup(m_findVariableIndex, m_sharedVariables.ToArray());
            FindVariableName = m_findVariableIndex > 0 ? m_sharedVariables[m_findVariableIndex] : null;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Tree From"))
            {
                CommandMgr.Instance.Do<CommandCreateTreeFromTemplate>(TreeReader.AllTreesName[m_copyFromTreeIndex]);
            }

            m_copyFromTreeIndex = m_copyFromTreeIndex > TreeReader.AllTreesName.Count - 1 ? TreeReader.AllTreesName.Count - 1 : m_copyFromTreeIndex;
            m_copyFromTreeIndex = EditorGUILayout.Popup(m_copyFromTreeIndex, TreeReader.AllTreesName.ToArray());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Variables From"))
            {
                CommandMgr.Instance.Do<CommandPasteVariableFromTemplate>(TreeReader.AllTreesName[m_copyFromTreeIndex]);
            }

            m_copyFromTreeIndex = m_copyFromTreeIndex > TreeReader.AllTreesName.Count - 1 ? TreeReader.AllTreesName.Count - 1 : m_copyFromTreeIndex;
            m_copyFromTreeIndex = EditorGUILayout.Popup(m_copyFromTreeIndex, TreeReader.AllTreesName.ToArray());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = !AllTasksFoldout && enable;
            if (GUILayout.Button("All Tasks Foldout"))
            {
                AllTasksFoldout = true;
            }

            if (AllTasksFoldout)
            {
                GUI.enabled = true && enable;
                GUILayout.Space(5);
                if (GUILayout.Button("Recover"))
                {
                    AllTasksFoldout = false;
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Unused Variables"))
            {
                CommandMgr.Instance.Do<CommandClearUnusedVariable>();
            }

            if (GUILayout.Button("Remove All Breakpoints"))
            {
                CommandMgr.Instance.Do<CommandRemoveTaskBreakpoint>(TreeChart.Instance.CurrTree.GetBreakpointTasks());
            }

            if (GUILayout.Button("Reset Tasks ID"))
            {
                TreeChart.Instance.CurrTree.ResetTaskID();
            }

            GUI.enabled = true;

            GUILayout.EndArea();
        }

        public void DisplaySwitch()
        {
            FindTaskName = null;
            FindVariableName = null;
            AllTasksFoldout = false;
            IsDisplay = !IsDisplay;
            if (!IsDisplay)
            {
                return;
            }

            m_findVariableIndex = 0;
            m_copyFromTreeIndex = 0;
            if (GraphPreferences.Instance.IsDisplay)
            {
                GraphPreferences.Instance.DisplaySwitch();
            }

            if (GraphCreate.Instance.IsDisplay)
            {
                GraphCreate.Instance.DisplaySwitch();
            }
        }
    }
}