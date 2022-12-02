using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphQuickSearch : Singleton<GraphQuickSearch>
    {
        private bool m_isDisplay;
        private bool m_searchFocus;
        private Rect m_taskListRect;
        private Vector2 m_taskPosition;
        private Vector2 m_scrollPosition;

        public bool IsDisplay
        {
            get => m_isDisplay;
            set
            {
                m_isDisplay = value;
                EditorTaskList.SearchTask(string.Empty);
            }
        }

        public void OnGUI()
        {
            if (!IsDisplay)
            {
                return;
            }

            GUILayout.BeginArea(m_taskListRect, AIDesignerUIUtility.PreferencesPaneGUIStyle);

            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("QuickSearch");
            string searchValue = EditorGUILayout.TextField(EditorTaskList.SearchTaskName, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (m_searchFocus)
            {
                m_searchFocus = false;
                GUI.FocusControl("QuickSearch");
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

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true);
            EditorTaskList.Draw(OnTaskClick);
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        public bool CheckLeftMouseDown(Vector2 mousePos)
        {
            if (!IsDisplay || m_taskListRect.Contains(mousePos))
            {
                return false;
            }

            IsDisplay = false;
            return true;
        }

        public bool CheckKeyCodeUp()
        {
            if (!IsDisplay)
            {
                return false;
            }

            if (Event.current.keyCode == KeyCode.DownArrow)
            {
                EditorTaskList.MoveSelectedIndex(true);
                return true;
            }

            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                EditorTaskList.MoveSelectedIndex(false);
                return true;
            }

            if (Event.current.keyCode == KeyCode.Return)
            {
                if (string.IsNullOrEmpty(EditorTaskList.SelectedTaskPath))
                {
                    return false;
                }

                OnTaskClick(EditorTaskList.SelectedTaskPath);
                return true;
            }

            return false;
        }

        public void Display(Vector2 taskPos, Rect rect)
        {
            if (IsDisplay && m_taskListRect.Contains(rect.position))
            {
                return;
            }

            IsDisplay = true;
            m_searchFocus = true;
            m_taskListRect = rect;
            m_taskPosition = taskPos;
            m_scrollPosition = Vector2.zero;
        }

        private void OnTaskClick(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            IsDisplay = false;
            CommandMgr.Instance.Do<CommandCreateTask>(path, m_taskPosition);
        }
    }
}