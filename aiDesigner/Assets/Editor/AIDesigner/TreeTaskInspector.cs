using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace AIDesigner
{
    public class TreeTaskInspector : TaskInspectorBase
    {
        private Vector2 m_scrollPosition;

        private Dictionary<string, Type> m_customInspectorType = new Dictionary<string, Type>();
        private Dictionary<string, TaskInspectorBase> m_customInspectorIns = new Dictionary<string, TaskInspectorBase>();

        protected List<TreeTask> CurrBoxesTask
        {
            get => TreeChart.Instance.CurrTasksByBoxes;
        }

        public TreeTaskInspector()
        {
            foreach (var type in Assembly.Load("Assembly-CSharp-Editor").GetTypes())
            {
                if (type.BaseType == typeof(TaskInspectorBase))
                {
                    var taskNameAttr = type.GetCustomAttribute<TaskName>();
                    if (null != taskNameAttr)
                    {
                        m_customInspectorType.Add(taskNameAttr.name, type);
                    }
                }
            }
        }

        public void Draw()
        {
            if (CurrBoxesTask.Count > 1)
            {
                GUILayout.Label("Only one task can be selected at a time to\n view its properties.", AIDesignerUIUtility.LabelWrapGUIStyle, GUILayout.Width(285f));
            }
            else if (null == CurrTask)
            {
                GUILayout.Label("Select a task from the tree to\nview its properties.", AIDesignerUIUtility.LabelWrapGUIStyle, GUILayout.Width(285f));
            }
            else
            {
                m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();

                GUILayout.Label("Name", GUILayout.Width(90f));
                EditorGUILayout.TextField(CurrTask.Name, GUILayout.Width(170f));
                if (GUILayout.Button(AIDesignerUIUtility.GearTexture, AIDesignerUIUtility.TransparentButtonGUIStyle, GUILayout.Width(20f)))
                {
                    var menu = (GenericMenu) (object) new GenericMenu();
                    menu.AddItem(new GUIContent("Edit Script"), false, () => { EditorTaskReader.OpenTask(CurrTask.Path); });
                    menu.AddItem(new GUIContent("Locate Script"), false, () => { EditorTaskReader.LocateTask(CurrTask.Path); });
                    menu.AddItem(new GUIContent("Reset"), false, null);
                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();

                DrawCommand();
                DrawAbortType();

                if (m_customInspectorIns.ContainsKey(CurrTask.Name) || m_customInspectorType.ContainsKey(CurrTask.Name))
                {
                    if (!m_customInspectorIns.TryGetValue(CurrTask.Name, out var inspector))
                    {
                        Type type = null;
                        try
                        {
                            type = m_customInspectorType[CurrTask.Name]; // Type.GetType(m_customInspectorType[CurrTask.Name], true, true);
                        }
                        catch (Exception e)
                        {
                        }

                        if (null != type)
                        {
                            inspector = Activator.CreateInstance(type) as TaskInspectorBase;
                        }
                        else
                        {
                            inspector = this;
                        }

                        m_customInspectorIns.Add(CurrTask.Name, inspector);
                    }

                    inspector.OnInspector();
                }
                else
                {
                    OnInspector();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    CurrTree.Save(true);
                }

                GUILayout.EndScrollView();
            }
        }

        private void DrawCommand()
        {
            GUILayout.Label("Comment");
            CurrTask.Comment = EditorGUILayout.TextArea(CurrTask.Comment, AIDesignerUIUtility.TaskInspectorCommentGUIStyle, GUILayout.Height(48f));
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(4);
        }

        private void DrawAbortType()
        {
            if (CurrTask.Type != TaskType.Composite)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Button(AIDesignerUIUtility.VariableWatchButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(15f));
            EditorGUI.BeginChangeCheck();
            CurrTask.AbortType = (AbortType) EditorGUILayout.EnumPopup("Abort Type", CurrTask.AbortType);
            if (EditorGUI.EndChangeCheck())
            {
                CurrTree.SetRuntimeTaskAbortType(CurrTask.DebugID, CurrTask.AbortType);
            }

            GUILayout.EndHorizontal();
        }
    }
}