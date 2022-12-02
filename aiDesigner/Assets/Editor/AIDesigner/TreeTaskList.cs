using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class TreeTaskList
    {
        private Vector2 m_scrollPosition = Vector2.zero;
        private bool m_searchFocus;

        public void Draw()
        {
            // search
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("Search");
            var searchValue = EditorGUILayout.TextField(EditorTaskList.SearchTaskName, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (m_searchFocus)
            {
                m_searchFocus = false;
                GUI.FocusControl("Search");
            }

            if (EditorTaskList.SearchTaskName != searchValue)
            {
                EditorTaskList.SearchTask(searchValue);
            }

            if (GUILayout.Button(string.Empty, !string.IsNullOrEmpty(EditorTaskList.SearchTaskName) ? GUI.skin.FindStyle("ToolbarSeachCancelButton") : GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty")))
            {
                EditorTaskList.SearchTask(string.Empty);
                GUI.FocusControl(null);
                m_searchFocus = true;
            }

            GUILayout.EndHorizontal();

            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(4f);

            // scrollView
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true);
            EditorTaskList.Draw(OnTaskClick);
            GUILayout.EndScrollView();
        }

        private void OnTaskClick(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            CommandMgr.Instance.Do<CommandCreateTask>(path, new Vector2(100f, 80f));
        }
    }
}