using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphTopBar : Singleton<GraphTopBar>
    {
        public Rect m_graphRect { get; private set; }

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(300f, 0f, AIDesignerWindow.Instance.ScreenSizeWidth - 300f, 18);
            }

            GUILayout.BeginArea(m_graphRect, EditorStyles.toolbar);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(AIDesignerUIUtility.HistoryBackwardTexture, EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                TreeChart.Instance.LoadPrevTree();
            }

            if (GUILayout.Button(AIDesignerUIUtility.HistoryForwardTexture, EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                TreeChart.Instance.LoadNextTree();
            }

            if (GUILayout.Button("...", EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                TreeChart.Instance.ShowHistoryTrees();
            }

            if (GUILayout.Button(null == CurrTree ? "(None Selected)" : CurrTree.FullName, EditorStyles.toolbarPopup, GUILayout.Width(140f)))
            {
                TreeChart.Instance.ShowAllTrees();
            }

            if (GUILayout.Button("Referenced Trees", EditorStyles.toolbarPopup, GUILayout.Width(140f)))
            {
                TreeChart.Instance.ShowReferenceTrees();
            }

            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(22f)) && null != CurrTree)
            {
                CommandMgr.Instance.Do<CommandDeleteTree>(CurrTree.FullName);
            }

            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22f)))
            {
                GraphCreate.Instance.DisplaySwitch();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(42f)) && null != CurrTree)
            {
                CurrTree.Save();
            }

            if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.Width(40f)))
            {
                GraphHelp.Instance.DisplaySwitch();
            }

            if (GUILayout.Button(AIDesignerUIUtility.LocationTexture, EditorStyles.toolbarButton, GUILayout.Width(20f)))
            {
                GraphTree.Instance.LocateToTree();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Preferences", EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                GraphPreferences.Instance.DisplaySwitch();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}