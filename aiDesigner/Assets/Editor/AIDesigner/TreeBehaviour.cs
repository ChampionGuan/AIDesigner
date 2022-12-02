using System.IO;
using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class TreeBehaviour
    {
        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public void Draw()
        {
            if (null == CurrTree)
            {
                GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", AIDesignerUIUtility.LabelWrapGUIStyle, GUILayout.Width(285f));
                return;
            }

            EditorGUIUtility.labelWidth = 100;

            GUILayout.BeginHorizontal();
            var name = EditorGUILayout.TextField("Tree Name", CurrTree.ShortName);
            if (name != CurrTree.ShortName && TreeStructure.IsLegalName(name) && !TreeReader.HasTree(CurrTree.Directory + name))
            {
                CommandMgr.Instance.Do<CommandRenameTree>(CurrTree.FullName, CurrTree.Directory + name);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Tree Directory", CurrTree.Directory, AIDesignerUIUtility.TreeDirectoryGUIStyle);
            if (GUILayout.Button(AIDesignerUIUtility.LocationTexture, AIDesignerUIUtility.TransparentGUIStyle, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var directory = CurrTree.Directory;
                AIDesignerLogicUtility.OpenConfigPathFolder(ref directory);

                if (directory != CurrTree.Directory)
                {
                    CommandMgr.Instance.Do<CommandRenameTree>(CurrTree.FullName, directory + CurrTree.ShortName);
                }
            }

            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.LabelField("Tree Description");
            CurrTree.SetDesc(EditorGUILayout.TextArea(CurrTree.Desc, AIDesignerUIUtility.TaskInspectorCommentGUIStyle, GUILayout.Height(48f)));
            CurrTree.SetTickInterval(EditorGUILayout.IntField("Tick Interval", CurrTree.TickInterval));
            CurrTree.SetPauseWhenComplete(EditorGUILayout.Toggle("Pause When Complete", CurrTree.PauseWhenComplete));
            CurrTree.SetResetValuesOnRestart(EditorGUILayout.Toggle("Reset Values On Restart", CurrTree.ResetValuesOnRestart));
        }
    }
}