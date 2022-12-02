using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using UnityEditor;

namespace AIDesigner
{
    public class TreeDebug : Singleton<TreeDebug>
    {
        private TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        private Test.XLuaEnv LuaEnv
        {
            get => Test.XLuaEnv.Instance;
        }

        private static string m_luaString;

        public static string LuaString
        {
            get
            {
                if (null == m_luaString)
                {
                    m_luaString = AIDesignerLogicUtility.FileRead($"{Application.dataPath}/{Define.CustomSettings.TreeDebugFilePath}");
                }

                return m_luaString;
            }
        }

        private Dictionary<int, TreeRuntime> m_runtimeTrees = new Dictionary<int, TreeRuntime>();
        private Action<List<TreeRuntime>> m_findRuntimeTreeAction;
        private GenericMenu m_runtimeTreeMenu;

        protected override void OnInstance()
        {
            m_findRuntimeTreeAction = (trees) =>
            {
                m_runtimeTrees.Clear();
                if (null == trees)
                {
                    return;
                }

                foreach (var tree in trees)
                {
                    m_runtimeTrees.Add(tree.ID, tree);
                }
            };
        }

        protected override void OnDispose()
        {
            m_luaString = null;
            m_runtimeTrees?.Clear();
        }

        public void ShowRunningTrees()
        {
            m_runtimeTreeMenu = new GenericMenu();
            m_runtimeTreeMenu.AddDisabledItem(new GUIContent("[None]"), false);

            var findAllTree = GetFunction("AllTrees", LuaString);
            findAllTree?.Call(m_findRuntimeTreeAction);

            foreach (var tree in m_runtimeTrees.Values)
            {
                var on = null != CurrTree && CurrTree.RuntimeID == tree.ID;
                m_runtimeTreeMenu.AddItem(new GUIContent(tree.Path), on, (data) =>
                {
                    TreeRuntime runtimeTree = data as TreeRuntime;
                    if (null != CurrTree)
                    {
                        CommandMgr.Instance.Do<CommandLoadTree>(CurrTree.FullName, null != CurrTree.RuntimeTree ? CurrTree.RuntimeTree.ID : 0, runtimeTree.FullName, runtimeTree.ID, true);
                    }
                    else
                    {
                        CommandMgr.Instance.Do<CommandLoadTree>(null, 0, runtimeTree.FullName, runtimeTree.ID, true);
                    }
                }, tree);
            }

            m_runtimeTreeMenu.ShowAsContext();
        }

        public TreeRuntime GetTree(int runningID)
        {
            if (m_runtimeTrees.TryGetValue(runningID, out var tree))
            {
                return tree;
            }

            return null;
        }

        public void SetTreeVariableValue(LuaTable aiVar, TreeRefVariable variable)
        {
            var setValue = GetFunction("SetTreeValue", LuaString);
            setValue?.Call(aiVar, variable);
        }

        public object GetTreeVariableValue(LuaTable aiVar, int index = 1)
        {
            var getValue = GetFunction("GetTreeValue", LuaString);
            var objs = getValue?.Call(aiVar, index);
            if (null != objs && objs.Length > 0)
            {
                return objs[0];
            }

            return null;
        }

        public void SetTaskVariableValue(LuaTable tree, string debugID, TreeTaskVariable variable)
        {
            var setValue = GetFunction("SetTaskValue", LuaString);
            setValue?.Call(tree, debugID, variable);
        }

        public void SetTaskAbortType(LuaTable tree, string debugID, AbortType type)
        {
            var setTaskAbortType = GetFunction("SetAbortType", LuaString);
            setTaskAbortType?.Call(tree, debugID, type);
        }

        public void SetTaskDisabled(LuaTable tree, string debugID, bool disabled)
        {
            var setTaskDisabled = GetFunction("SetDisabled", LuaString);
            setTaskDisabled?.Call(tree, debugID, disabled);
        }

        private object[] DoString(string str)
        {
            if (string.IsNullOrEmpty(str) || null == LuaEnv || !EditorApplication.isPlaying)
            {
                return null;
            }

            return LuaEnv.DoString(str);
        }

        private LuaFunction GetFunction(string name, string luaStr)
        {
            var objs = DoString(luaStr);
            if (null == objs || objs.Length < 1)
            {
                return null;
            }

            return (objs[0] as LuaTable).Get<LuaFunction>(name);
        }

        private LuaFunction GetFunction(string luaStr)
        {
            var objs = DoString(luaStr);
            if (null != objs && objs.Length > 0)
            {
                return objs[0] as LuaFunction;
            }

            return null;
        }
    }
}