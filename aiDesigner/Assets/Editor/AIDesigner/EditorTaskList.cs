using UnityEngine;
using System;
using UnityEditor;

namespace AIDesigner
{
    public static class EditorTaskList
    {
        private static TaskType[] m_typeShowOrder = {TaskType.Composite, TaskType.Decorator, TaskType.Action, TaskType.Condition};
        private static Action<string> m_onTaskClick;
        private static int m_selectedTaskIndex;

        public static string SearchTaskName { get; private set; }
        public static string SelectedTaskPath { get; private set; }

        public static void MoveSelectedIndex(bool down)
        {
            if (down)
            {
                var maxIndex = VisibleTaskCount();
                m_selectedTaskIndex = m_selectedTaskIndex >= maxIndex ? maxIndex : ++m_selectedTaskIndex;
            }
            else
            {
                var minIndex = 1;
                m_selectedTaskIndex = m_selectedTaskIndex <= minIndex ? minIndex : --m_selectedTaskIndex;
            }

            var index = 0;
            SelectedTaskPath = null;
            for (var i = 0; i < m_typeShowOrder.Length; i++)
            {
                if (!EditorTaskReader.TaskTabs.TryGetValue(m_typeShowOrder[i], out var taskCategory))
                {
                    continue;
                }

                EditorTaskReader.RecursionTaskTab(tab =>
                {
                    if (!tab.FoldoutFlag || !string.IsNullOrEmpty(SelectedTaskPath))
                    {
                        return;
                    }

                    for (var j = 0; j < tab.Tasks.Count; j++)
                    {
                        if (tab.Tasks[j].VisibleFlag && ++index == m_selectedTaskIndex)
                        {
                            SelectedTaskPath = tab.Tasks[j].Path;
                        }
                    }
                }, taskCategory, false);
            }
        }

        public static void ResetSelectedIndex()
        {
            m_selectedTaskIndex = 0;
            SelectedTaskPath = null;
        }

        public static void SearchTask(string name)
        {
            ResetSelectedIndex();
            SearchTaskName = name.ToLower().Trim();

            foreach (var tab in EditorTaskReader.TaskTabs.Values)
            {
                tab.SetVisibleFlag(SearchTaskName);
            }
        }

        public static void Draw(Action<string> onTaskClick)
        {
            m_onTaskClick = onTaskClick;
            for (var i = 0; i < m_typeShowOrder.Length; i++)
            {
                Draw(m_typeShowOrder[i]);
            }
        }

        public static void Menu(ref GenericMenu menu, string prefix, GenericMenu.MenuFunction2 func)
        {
            for (var i = 0; i < m_typeShowOrder.Length; i++)
            {
                Menu(ref menu, m_typeShowOrder[i], prefix, func);
            }
        }

        private static void Draw(TaskType type)
        {
            if (!EditorTaskReader.TaskTabs.TryGetValue(type, out var taskTab))
            {
                return;
            }

            Draw(taskTab);
        }

        private static void Draw(EditorTaskTab tab)
        {
            if (!tab.VisibleFlag)
            {
                return;
            }

            var foldOut = EditorGUILayout.Foldout(tab.FoldoutFlag, tab.Name, AIDesignerUIUtility.TaskFoldoutGUIStyle);
            if (foldOut != tab.FoldoutFlag)
            {
                tab.FoldoutFlag = foldOut;
                ResetSelectedIndex();
            }

            if (tab.FoldoutFlag)
            {
                EditorGUI.indentLevel += 1;
                foreach (var subTab in tab.Tabs)
                {
                    Draw(subTab);
                }

                GUILayout.BeginVertical();
                foreach (var task in tab.Tasks)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 10);

                    if (SelectedTaskPath == task.Path)
                    {
                        GUI.backgroundColor = new Color(1f, 0.64f, 0f);
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }

                    if (task.VisibleFlag && GUILayout.Button(task.Name, EditorStyles.toolbarButton))
                    {
                        m_onTaskClick?.Invoke(task.Path);
                    }

                    GUI.backgroundColor = Color.white;
                    GUILayout.Space(EditorGUI.indentLevel * 10);
                    GUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel -= 1;
                GUILayout.Space(3f);
                GUILayout.EndVertical();
            }
        }

        private static void Menu(ref GenericMenu menu, TaskType type, string prefix, GenericMenu.MenuFunction2 func)
        {
            if (!EditorTaskReader.TaskTabs.TryGetValue(type, out var taskCategory))
            {
                return;
            }

            Menu(taskCategory, ref menu, prefix, func);
        }

        private static void Menu(EditorTaskTab tab, ref GenericMenu menu, string prefix, GenericMenu.MenuFunction2 func)
        {
            prefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}/{tab.Name}";
            foreach (var subTab in tab.Tabs)
            {
                Menu(subTab, ref menu, prefix, func);
            }

            foreach (var task in tab.Tasks)
            {
                menu.AddItem(new GUIContent($"{prefix}/{task.Name}"), false, func, task.Path);
            }
        }

        private static int VisibleTaskCount()
        {
            var count = 0;
            for (var i = 0; i < m_typeShowOrder.Length; i++)
            {
                if (!EditorTaskReader.TaskTabs.TryGetValue(m_typeShowOrder[i], out var taskCategory))
                {
                    continue;
                }

                EditorTaskReader.RecursionTaskTab(tab =>
                {
                    if (!tab.FoldoutFlag)
                    {
                        return;
                    }

                    foreach (var task in tab.Tasks)
                    {
                        if (task.VisibleFlag)
                        {
                            count++;
                        }
                    }
                }, taskCategory);
            }

            return count;
        }
    }
}