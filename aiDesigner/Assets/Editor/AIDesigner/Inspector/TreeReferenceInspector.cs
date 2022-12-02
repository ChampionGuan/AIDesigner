using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    // ---@class AI.TreeReference:AIAction
    // ---@field treeName String

    [TaskName("TreeReference")]
    public class TreeReferenceInspector : TaskInspectorBase
    {
        protected override void DrawSoloField(SharedVariable var, bool needWatch = true, string name = null)
        {
            if (var.Key != "treeName")
            {
                base.DrawSoloField(var, needWatch, name);
            }
            else
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

                var allTrees = new List<string> {"(None)"};
                foreach (var treeName in TreeReader.AllTreesName)
                {
                    if (treeName == CurrTree.FullName)
                    {
                        continue;
                    }

                    allTrees.Add(treeName);
                }

                var oldIndex = 0;
                var refTreeName = (string) var.Value;
                TreeReader.LegalTreeName(ref refTreeName);

                for (var i = 0; i < allTrees.Count; i++)
                {
                    if (allTrees[i] == refTreeName)
                    {
                        oldIndex = i;
                        break;
                    }
                }

                var newIndex = EditorGUILayout.Popup(new GUIContent(AIDesignerLogicUtility.ToUpperFirst(name), var.Desc), oldIndex, allTrees.ToArray());
                if (oldIndex != newIndex)
                {
                    var.SetValue(newIndex == 0 ? null : TreeReader.StorageTreeName(allTrees[newIndex]));
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}