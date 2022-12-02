using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public static class FieldInspector
    {
        public static void DrawSingleField(GUIContent guiContent, Variable variable)
        {
            if (null == variable)
            {
                return;
            }

            if (variable.Type == VarType.Object)
            {
                EditorGUILayout.ObjectField(guiContent, (UnityEngine.Object) variable.Value, typeof(UnityEngine.Object), false);
                return;
            }

            if (variable.Type == VarType.Float)
            {
                variable.SetValue(EditorGUILayout.FloatField(guiContent, (float) variable.Value));
                return;
            }

            if (variable.Type == VarType.Int)
            {
                if (null != variable.Options)
                {
                    variable.Options.SelectedIndex = EditorGUILayout.Popup(guiContent, variable.Options.SelectedIndex, variable.Options.Keys);
                    variable.SetValue(variable.Options.GetValue());
                }
                else
                {
                    variable.SetValue(EditorGUILayout.IntField(guiContent, (int) variable.Value));
                }

                return;
            }

            if (variable.Type == VarType.String)
            {
                if (null != variable.Options)
                {
                    variable.Options.SelectedIndex = EditorGUILayout.Popup(guiContent, variable.Options.SelectedIndex, variable.Options.Keys);
                    variable.SetValue(variable.Options.GetValue());
                }
                else
                {
                    variable.SetValue(EditorGUILayout.TextField(guiContent, (string) variable.Value));
                }

                return;
            }

            if (variable.Type == VarType.Boolean)
            {
                variable.SetValue(EditorGUILayout.Toggle(guiContent, (bool) variable.Value));
                return;
            }

            if (variable.Type == VarType.Vector2)
            {
                variable.SetValue(EditorGUILayout.Vector2Field(guiContent, (Vector2) variable.Value));
                return;
            }

            if (variable.Type == VarType.Vector2Int)
            {
                variable.SetValue(EditorGUILayout.Vector2IntField(guiContent, (Vector2Int) variable.Value));
                return;
            }

            if (variable.Type == VarType.Vector3)
            {
                variable.SetValue(EditorGUILayout.Vector3Field(guiContent, (Vector3) variable.Value));
                return;
            }

            if (variable.Type == VarType.Vector3Int)
            {
                variable.SetValue(EditorGUILayout.Vector3IntField(guiContent, (Vector3Int) variable.Value));
                return;
            }

            if (variable.Type == VarType.Vector4)
            {
                variable.SetValue(EditorGUILayout.Vector4Field(guiContent, (Vector4) variable.Value));
                return;
            }
        }

        public static void DrawArrayElement(ReferenceVariable variable)
        {
            if (!variable.IsArrayExpanded)
            {
                return;
            }

            GUILayout.BeginVertical();
            EditorGUI.indentLevel += 2;

            var size = EditorGUILayout.IntField("Size", variable.ArrayVar.Count);
            if (size != variable.ArrayVar.Count && size >= 0)
            {
                variable.SetArraySize(size);
            }

            for (var i = 0; i < variable.ArrayVar.Count; i++)
            {
                DrawSingleField(new GUIContent("Element " + i), variable.ArrayVar[i]);
                GUILayout.Space(4f);
            }

            EditorGUI.indentLevel -= 2;
            GUILayout.EndVertical();
        }
    }
}