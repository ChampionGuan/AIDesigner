using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace AIDesigner
{
    public class TaskName : Attribute
    {
        private string m_name;

        public string name
        {
            get => m_name;
        }

        public TaskName(string taskName)
        {
            m_name = taskName;
        }
    }

    public class TaskInspectorBase
    {
        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        protected TreeTask CurrTask
        {
            get => TreeChart.Instance.CurrTask;
        }

        public virtual void OnInspector()
        {
            foreach (var var in CurrTask.Variables)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(4);

                if (var.IsArray)
                {
                    DrawArrayField(var);
                }
                else
                {
                    DrawSoloField(var);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    CurrTree.SetRuntimeTaskVariable(CurrTask.DebugID, var);
                }
            }
        }

        protected virtual void DrawArrayField(ReferenceVariable var)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button(AIDesignerUIUtility.VariableWatchButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(15f));

            if (!var.SharedFlag)
            {
                var.IsArrayExpanded = EditorGUILayout.Foldout(var.IsArrayExpanded, new GUIContent(AIDesignerLogicUtility.ToUpperFirst(var.Key), var.Desc));
            }
            else
            {
                DrawSharedField(var, var.IsArray);
            }

            if (var.IsShared && GUILayout.Button("S", GUILayout.Width(17f)))
            {
                var.SharedFlag = !var.SharedFlag;
                if (!var.SharedFlag)
                {
                    var.SharedKey = null;
                }
            }

            GUILayout.EndHorizontal();

            if (!var.SharedFlag && var.IsArrayExpanded)
            {
                EditorGUI.indentLevel += 2;

                var size = EditorGUILayout.IntField("Size", var.ArrayVar.Count);
                if (size != var.ArrayVar.Count && size >= 0)
                {
                    var.SetArraySize(size);
                }

                for (var i = 0; i < var.ArrayVar.Count; i++)
                {
                    DrawSoloField(var.ArrayVar[i], false, "Element " + i);
                    GUILayout.Space(4f);
                }

                EditorGUI.indentLevel -= 2;
            }

            DrawDebugField(var);
        }

        protected virtual void DrawSoloField(SharedVariable var, bool needWatch = true, string name = null)
        {
            GUILayout.BeginHorizontal();

            if (needWatch)
            {
                GUILayout.Button(AIDesignerUIUtility.VariableWatchButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(15f));
            }

            if (string.IsNullOrEmpty(name))
            {
                name = var.Key;
            }

            if (!var.IsShared || !var.SharedFlag)
            {
                FieldInspector.DrawSingleField(new GUIContent(AIDesignerLogicUtility.ToUpperFirst(name), var.Desc), var);
                var.SharedKey = null;
            }

            if (var.SharedFlag)
            {
                DrawSharedField(var, false, name);
            }

            if (var.IsShared && GUILayout.Button("S", GUILayout.Width(17f)))
            {
                var.SharedFlag = !var.SharedFlag;
                if (!var.SharedFlag)
                {
                    var.SharedKey = null;
                }
            }

            GUILayout.EndHorizontal();
            DrawDebugField(var as ReferenceVariable);
        }

        private void DrawSharedField(SharedVariable var, bool isArray, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = var.Key;
            }

            var index = 0;
            var names = new List<string> {"(None)"};
            for (var i = 0; i < CurrTree.Variables.Count; i++)
            {
                if ((CurrTree.Variables[i].Type == var.Type || var.IsAnyType) && CurrTree.Variables[i].IsArray == isArray)
                {
                    names.Add(CurrTree.Variables[i].Key);
                    if (CurrTree.Variables[i].Key == var.SharedKey)
                    {
                        index = names.Count - 1;
                    }
                }
            }

            var backgroundColor = GUI.backgroundColor;
            if (index == 0)
            {
                GUI.backgroundColor = Color.red;
                var.SharedKey = null;
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup(AIDesignerLogicUtility.ToUpperFirst(name), index, names.ToArray(), AIDesignerUIUtility.SharedVariableToolbarPopup);
            GUI.backgroundColor = backgroundColor;
            if (EditorGUI.EndChangeCheck())
            {
                if (index == 0)
                {
                    var.SharedKey = null;
                }
                else
                {
                    var.SharedKey = names[index];
                }
            }
        }

        private void DrawDebugField(ReferenceVariable var)
        {
            if (null == var || !var.SharedFlag || !GraphPreferences.Instance.IsTaskVariableDebug)
            {
                return;
            }

            GUILayout.BeginVertical();
            GUI.enabled = false;

            var treeVariable = CurrTree.GetSharedVariable(var.SharedKey);
            if (null == treeVariable)
            {
                EditorGUILayout.TextArea(var.IsArray || null == var.Value ? "None" : var.Value.ToString(), GUILayout.Height(18f));
            }
            else
            {
                if (!var.IsArray)
                {
                    DrawOptionsText(var.Options, treeVariable.Value);
                }
                else if (treeVariable.ArrayVar.Count < 1)
                {
                    EditorGUILayout.TextArea("Size:0", GUILayout.Height(18f));
                }
                else
                {
                    foreach (var variable in treeVariable.ArrayVar)
                    {
                        DrawOptionsText(var.Options, variable.Value);
                    }
                }
            }

            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        private void DrawOptionsText(Options options, object value)
        {
            if (null == value)
            {
                EditorGUILayout.TextArea("None", GUILayout.Height(18f));
                return;
            }

            if (null != options)
            {
                var index = options.GetValueSelectedIndex(value);
                if (index >= 0)
                {
                    EditorGUILayout.TextArea(options.Keys[index], GUILayout.Height(18f));
                    return;
                }
            }

            EditorGUILayout.TextArea(value.ToString(), GUILayout.Height(18f));
        }
    }
}