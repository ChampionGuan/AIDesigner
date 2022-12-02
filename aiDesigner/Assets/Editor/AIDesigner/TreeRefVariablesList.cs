using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace AIDesigner
{
    public class TreeRefVariablesList
    {
        private Variable m_tobeAddVariable = null;
        private bool m_isArrayVariable = false;
        private Vector2 m_scrollPosition = Vector2.zero;
        private float m_minY = 0;

        private TreeRefVariable m_deletedRefVariable;
        private TreeRefVariable m_downRefVariable;
        private TreeRefVariable m_upRefVariable;
        private TreeRefVariable m_copyRefVariable;

        private Dictionary<int, string> m_varTypeOption;
        private GenericMenu m_rightMouseDownMenu;

        public Dictionary<int, string> VarTypeOption
        {
            get
            {
                if (null == m_varTypeOption)
                {
                    m_varTypeOption = new Dictionary<int, string>();
                    for (var i = -1; i < (int) VarType.MaxCount; i++)
                    {
                        var type = (VarType) i;
                        if (type == VarType.Float && AIDesignerLogicUtility.IsEditorForBattle())
                        {
                            m_varTypeOption[i] = nameof(VarType.Fix);
                        }
                        else if (type == VarType.Fix && !AIDesignerLogicUtility.IsEditorForBattle())
                        {
                            m_varTypeOption[i] = nameof(VarType.Float);
                        }
                        else
                        {
                            m_varTypeOption[i] = type.ToString();
                        }
                    }
                }

                return m_varTypeOption;
            }
        }

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public TreeRefVariablesList()
        {
            m_tobeAddVariable = new Variable(null, VarType.None, null, null, null);
        }

        public bool CheckLeftMouseDown(Vector2 mousePos)
        {
            var result = false;
            if (null == CurrTree)
            {
                return result;
            }

            foreach (var v in CurrTree.Variables)
            {
                if (v.IsSelected != false)
                {
                    result = true;
                }

                v.IsSelected = false;
            }

            foreach (var v in CurrTree.Variables)
            {
                if (mousePos.y > m_minY && mousePos.y < v.PosY - m_scrollPosition.y)
                {
                    v.IsSelected = true;
                    result = true;
                    break;
                }
            }

            return result;
        }

        public bool CheckRightMouseDown(Vector2 mousePos)
        {
            var result = false;
            if (null == CurrTree)
            {
                return result;
            }

            for (var i = 0; i < CurrTree.Variables.Count; i++)
            {
                if (mousePos.y > m_minY && mousePos.y < CurrTree.Variables[i].PosY - m_scrollPosition.y)
                {
                    m_rightMouseDownMenu = new GenericMenu();
                    if (i > 0)
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Up"), false, (var) =>
                        {
                            m_upRefVariable = var as TreeRefVariable;
                            MoveUp();
                        }, CurrTree.Variables[i]);
                    }

                    if (i < CurrTree.Variables.Count - 1)
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Move Down"), false, (var) =>
                        {
                            m_downRefVariable = var as TreeRefVariable;
                            MoveDown();
                        }, CurrTree.Variables[i]);
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Copy"), false, (var) => { m_copyRefVariable = var as TreeRefVariable; }, CurrTree.Variables[i]);
                    if (null == m_copyRefVariable)
                    {
                        m_rightMouseDownMenu.AddDisabledItem(new GUIContent("Paste"));
                    }
                    else
                    {
                        m_rightMouseDownMenu.AddItem(new GUIContent("Paste"), false, () => { Paste(); });
                    }

                    m_rightMouseDownMenu.AddSeparator(string.Empty);
                    m_rightMouseDownMenu.AddItem(new GUIContent("Delete"), false, (var) =>
                    {
                        m_deletedRefVariable = var as TreeRefVariable;
                        Delete();
                    }, CurrTree.Variables[i]);

                    m_rightMouseDownMenu.ShowAsContext();
                    result = true;
                    break;
                }
            }

            if (!result && null != m_copyRefVariable)
            {
                m_rightMouseDownMenu = new GenericMenu();
                m_rightMouseDownMenu.AddItem(new GUIContent("Paste Variable"), false, () => { Paste(); });
                m_rightMouseDownMenu.ShowAsContext();
                result = true;
            }

            return result;
        }

        public void Draw()
        {
            if (null == CurrTree)
            {
                GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", AIDesignerUIUtility.LabelWrapGUIStyle, GUILayout.Width(285f));
                return;
            }

            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            DrawHeader();
            DrawVariables();
            GUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            // name
            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);
            EditorGUILayout.LabelField("Name", GUILayout.Width(40f));
            m_tobeAddVariable.Key = EditorGUILayout.TextField(m_tobeAddVariable.Key);
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);

            // type
            GUILayout.Label("Type", GUILayout.Width(40f));
            m_tobeAddVariable.SetType((VarType) EditorGUILayout.IntPopup((int) m_tobeAddVariable.Type, VarTypeOption.Values.ToArray(), VarTypeOption.Keys.ToArray(), GUILayout.Width(120f)));
            GUILayout.Space(8f);

            // isArray
            GUILayout.Label("Array", GUILayout.Width(35f));
            m_isArrayVariable = EditorGUILayout.Toggle(m_isArrayVariable, GUILayout.Width(20f));
            GUILayout.Space(8f);

            // add
            var validVar = CheckNameIsValid(m_tobeAddVariable.Key) && m_tobeAddVariable.Type != VarType.None;
            GUI.enabled = validVar;
            if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(40f)) && validVar)
            {
                CommandMgr.Instance.Do<CommandAddVariable>(m_tobeAddVariable.Key, m_tobeAddVariable.Type, m_isArrayVariable);
                m_tobeAddVariable.Key = string.Empty;
                GUI.FocusControl(null);
            }

            GUI.enabled = true;

            GUILayout.Space(6f);
            GUILayout.EndHorizontal();

            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(4f);
            m_minY = GUILayoutUtility.GetLastRect().yMax;
        }

        private void DrawVariables()
        {
            m_deletedRefVariable = null;
            m_downRefVariable = null;
            m_upRefVariable = null;
            foreach (var v in CurrTree.Variables)
            {
                if (v.IsSelected)
                {
                    GUILayout.BeginVertical(AIDesignerUIUtility.SelectedBackgroundGUIStyle);

                    // 1
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name", GUILayout.Width(70f));
                    var name = EditorGUILayout.TextField(v.Key, GUILayout.Width(140f));
                    if (name != v.Key && CheckNameIsValid(name))
                    {
                        CommandMgr.Instance.Do<CommandUpdateVariableName>(v.Key, name);
                    }

                    GUILayout.Space(15f);

                    if (GUILayout.Button(AIDesignerUIUtility.DownArrowButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_downRefVariable = v;
                    }

                    if (GUILayout.Button(AIDesignerUIUtility.UpArrowButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_upRefVariable = v;
                    }

                    if (GUILayout.Button(AIDesignerUIUtility.VariableDeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                    {
                        m_deletedRefVariable = v;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);

                    // 2
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Type", GUILayout.Width(70f));
                    var type = (VarType) EditorGUILayout.IntPopup((int) v.Type, VarTypeOption.Values.ToArray(), VarTypeOption.Keys.ToArray(), GUILayout.Width(140f));
                    if (type != v.Type && type != VarType.None)
                    {
                        CommandMgr.Instance.Do<CommandUpdateVariableType>(v.Key, v.Type, type);
                    }

                    GUILayout.Space(10f);
                    GUI.enabled = false;
                    GUILayout.Label("Array");
                    EditorGUILayout.Toggle(v.IsArray);
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();

                    // 4
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Desc", GUILayout.Width(70f));
                    v.Desc = EditorGUILayout.TextArea(v.Desc, AIDesignerUIUtility.TreeVariableDescTextGUIStyle);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);

                    // 5
                    EditorGUI.BeginChangeCheck();
                    if (v.IsArray)
                    {
                        GUILayout.BeginHorizontal();
                        v.IsArrayExpanded = EditorGUILayout.Foldout(v.IsArrayExpanded, new GUIContent(v.Key));
                        GUILayout.EndHorizontal();
                        FieldInspector.DrawArrayElement(v);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        FieldInspector.DrawSingleField(new GUIContent(v.Key), v);
                        GUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        CurrTree.SetRuntimeTreeVariable(v);
                    }

                    AIDesignerUIUtility.DrawContentSeperator(4, 7);
                    GUILayout.EndVertical();

                    GUILayout.Space(3f);
                }
                else
                {
                    GUILayout.Space(2);
                    EditorGUI.BeginChangeCheck();
                    if (v.IsArray)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(18);
                        v.IsArrayExpanded = EditorGUILayout.Foldout(v.IsArrayExpanded, new GUIContent(v.Key));
                        if (GUILayout.Button(AIDesignerUIUtility.VariableDeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                        {
                            m_deletedRefVariable = v;
                        }

                        GUILayout.Space(12f);
                        GUILayout.EndHorizontal();
                        FieldInspector.DrawArrayElement(v);
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(18);
                        FieldInspector.DrawSingleField(new GUIContent(v.Key), v);
                        if (GUILayout.Button(AIDesignerUIUtility.VariableDeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(19f)))
                        {
                            m_deletedRefVariable = v;
                        }

                        GUILayout.Space(12f);
                        GUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        CurrTree.SetRuntimeTreeVariable(v);
                    }

                    if (!string.IsNullOrEmpty(v.Desc))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(18);
                        EditorGUILayout.LabelField(v.Desc, AIDesignerUIUtility.TreeVariableDescLabelGUIStyle);
                        GUILayout.Space(10f);
                        GUILayout.EndHorizontal();
                    }

                    AIDesignerUIUtility.DrawContentSeperator(2, 7);
                }

                v.PosY = GUILayoutUtility.GetLastRect().yMax + 7;
            }

            if (CurrTree.Variables.Count > 0)
            {
                GUILayout.Space(12f);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Unused Variables"))
                {
                    CommandMgr.Instance.Do<CommandClearUnusedVariable>();
                }

                GUILayout.Space(5f);
                if (GUILayout.Button("Sort"))
                {
                    CurrTree.SortSharedVariable();
                }

                GUILayout.EndHorizontal();
                GUILayout.Label("Select a variable to change its properties.", AIDesignerUIUtility.LabelWrapGUIStyle);
            }

            Delete();
            MoveDown();
            MoveUp();
        }

        private void MoveDown()
        {
            if (null == m_downRefVariable)
            {
                return;
            }

            for (var i = CurrTree.Variables.Count - 1; i >= 0; i--)
            {
                if (m_downRefVariable == CurrTree.Variables[i])
                {
                    if (i == CurrTree.Variables.Count - 1)
                    {
                        break;
                    }

                    var temp = CurrTree.Variables[i + 1];
                    CurrTree.Variables[i + 1] = m_downRefVariable;
                    CurrTree.Variables[i] = temp;
                    break;
                }
            }
        }

        private void MoveUp()
        {
            if (null == m_upRefVariable)
            {
                return;
            }

            for (var i = 0; i < CurrTree.Variables.Count; i++)
            {
                if (m_upRefVariable == CurrTree.Variables[i])
                {
                    if (i == 0)
                    {
                        break;
                    }

                    var temp = CurrTree.Variables[i - 1];
                    CurrTree.Variables[i - 1] = m_upRefVariable;
                    CurrTree.Variables[i] = temp;
                    break;
                }
            }
        }

        private void Delete()
        {
            if (null != m_deletedRefVariable)
            {
                CommandMgr.Instance.Do<CommandRemoveVariable>(m_deletedRefVariable);
            }
        }

        private void Paste()
        {
            if (null != m_copyRefVariable)
            {
                CommandMgr.Instance.Do<CommandPasteVariable>(m_copyRefVariable);
            }
        }

        private bool CheckNameIsValid(string name)
        {
            return !string.IsNullOrEmpty(name) && null == CurrTree.Variables.Find(x => x.Key == name);
        }
    }
}