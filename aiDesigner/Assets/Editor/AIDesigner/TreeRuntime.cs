using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using System.Text;

namespace AIDesigner
{
    public class TreeRuntime
    {
        public int ID { get; private set; }
        public string Path { get; private set; }
        public string FullName { get; private set; }

        public bool IsStart { get; private set; }
        public bool IsPaused { get; private set; }

        public LuaTable Tree { get; private set; }
        public GameObject Host { get; private set; }

        private int m_currFrameCount;
        private TreeStructure m_editorTree;

        private Dictionary<string, TaskStateType> m_taskStateType = new Dictionary<string, TaskStateType>();
        private Dictionary<string, LuaTable> m_treeVariables = new Dictionary<string, LuaTable>();
        private Dictionary<string, LuaTable> m_treeTasks = new Dictionary<string, LuaTable>();

        private Action<LuaTable> m_readLuaTask;
        private Action<TreeTask> m_readCSharpTask;

        public TreeRuntime(GameObject host, string path, string fullName, LuaTable tree)
        {
            TreeReader.LegalTreeName(ref fullName);
            FullName = fullName;
            Host = host;
            Path = path;
            Tree = tree;
            ID = tree.GetHashCode();
        }

        public TaskStateType GetTaskState(string debugId)
        {
            if (!GraphPreferences.Instance.IsTaskDebugStopAtTheLastFrame)
            {
                if (Time.frameCount > m_currFrameCount)
                {
                    m_taskStateType.Clear();
                }
            }

            if (m_taskStateType.TryGetValue(debugId, out var type))
            {
                return type;
            }

            return TaskStateType.None;
        }

        public void SetTreeVariable(TreeRefVariable variable)
        {
            if (null == variable || !m_treeVariables.TryGetValue(variable.Key, out var tab))
            {
                return;
            }

            TreeDebug.Instance.SetTreeVariableValue(tab, variable);
        }

        public void SetTaskVariable(string debugID, TreeTaskVariable variables)
        {
            if (null == Tree || string.IsNullOrEmpty(debugID))
            {
                return;
            }

            TreeDebug.Instance.SetTaskVariableValue(Tree, debugID, variables);
        }

        public void SetTaskAbortType(string debugID, AbortType type)
        {
            if (null == Tree)
            {
                return;
            }

            TreeDebug.Instance.SetTaskAbortType(Tree, debugID, type);
        }

        public void SetTaskDisabled(string debugID, bool disabled)
        {
            if (null == Tree)
            {
                return;
            }

            TreeDebug.Instance.SetTaskDisabled(Tree, debugID, disabled);
        }

        public void Rebind(TreeStructure editorTree)
        {
            m_editorTree = editorTree;
            m_treeVariables.Clear();
            m_treeTasks.Clear();

            if (null == Tree)
            {
                return;
            }

            IsStart = Tree.GetInPath<bool>("_start");
            IsPaused = Tree.GetInPath<bool>("_pause");

            Tree.SetInPath<Action<LuaTable, bool>>("__onAddRefTree", OnAddRefTree);
            Tree.SetInPath<Action<bool, bool>>("__onStateUpdate", OnTreeStateUpdate);

            Tree.GetInPath<LuaTable>("_vars")?.ForEach<string, LuaTable>((key, value) =>
            {
                value.SetInPath<Action<LuaTable, LuaTable>>("__onValueUpdate", OnVariableValueUpdate);
                OnVariableValueUpdate(value);
                m_treeVariables.Add(key, value);
            });

            if (null == m_readCSharpTask)
            {
                m_readCSharpTask = (task) =>
                {
                    StringBuilder debugID = new StringBuilder();
                    TreeTask parent = task;
                    while (null != parent)
                    {
                        debugID.Append(parent.HashID.ToString() + "_");
                        parent = parent.Parent;
                    }

                    task.DebugID = AIDesignerLogicUtility.BuildMD5ByString(debugID.ToString());
                };
            }

            editorTree.RecursionTask(m_readCSharpTask, editorTree.Entry);

            if (null == m_readLuaTask)
            {
                m_readLuaTask = (task) =>
                {
                    var debugID = new StringBuilder();
                    var parent = task;
                    while (null != parent)
                    {
                        debugID.Append(parent.GetInPath<int>("config.task.hashID").ToString() + "_");
                        parent = parent.GetInPath<LuaTable>("parent");
                    }

                    task.SetInPath<string>("debugID", AIDesignerLogicUtility.BuildMD5ByString(debugID.ToString()));
                    task.SetInPath<Action<LuaTable>>("__onTaskUpdate", OnTaskStateUpdate);
                    OnTaskStateUpdate(task);
                    m_treeTasks.Add(debugID.ToString(), task);
                };
            }

            Tree.GetInPath<LuaFunction>("_RecursionTask")?.Call(Tree, Tree.GetInPath<LuaTable>("_entry"), m_readLuaTask);
        }

        public void Unbind()
        {
            m_treeVariables.Clear();
            m_editorTree = null;
            if (null == Tree)
            {
                return;
            }

            foreach (var tab in m_treeVariables.Values)
            {
                tab.SetInPath<Action<string, object>>("__onValueUpdate", null);
            }

            foreach (var tab in m_treeTasks.Values)
            {
                tab.SetInPath<Action<LuaTable>>("__onTaskUpdate", null);
            }

            Tree.SetInPath<Action<LuaTable>>("__onAddRefTree", null);
            Tree.SetInPath<Action<LuaTable>>("__onTaskUpdate", null);
            Tree.SetInPath<Action<bool, bool>>("__onStateUpdate", null);
        }

        private void OnTreeStateUpdate(bool start, bool paused)
        {
            IsStart = start;
            IsPaused = paused;
        }

        private void OnTaskStateUpdate(LuaTable task)
        {
            if (null == task)
            {
                return;
            }

            if (m_currFrameCount != Time.frameCount)
            {
                m_currFrameCount = Time.frameCount;
                m_taskStateType.Clear();
            }

            if (task.ContainsKey("___refTreeName"))
            {
                if (!task.ContainsKey("debugID"))
                {
                    OnAddRefTree(task, true);
                }
                else if (null == m_editorTree.GetTask(task.GetInPath<int>("config.task.hashID"), task.GetInPath<string>("debugID")))
                {
                    OnAddRefTree(task, true);
                }
            }

            var stateType = TaskStateType.None;
            try
            {
                stateType = task.GetInPath<TaskStateType>("state");
            }
            catch (Exception e)
            {
                stateType = TaskStateType.None;
            }

            var debugID = task.GetInPath<string>("debugID");
            if (m_taskStateType.ContainsKey(debugID))
            {
                m_taskStateType[debugID] = stateType;
            }
            else
            {
                m_taskStateType.Add(debugID, stateType);
            }
        }

        private void OnVariableValueUpdate(LuaTable tab, LuaTable subTab = null)
        {
            if (null == m_editorTree || null == tab)
            {
                return;
            }

            var key = tab.GetInPath<string>("_key");
            var variable = m_editorTree.GetSharedVariable(key);
            if (null == variable)
            {
                m_editorTree.AddSharedVariable(key, tab.GetInPath<VarType>("_type"), tab.GetInPath<bool>("_isArray"));
            }

            variable = m_editorTree.GetSharedVariable(key);
            if (null == variable)
            {
                return;
            }

            if (variable.IsArray)
            {
                var count = (int) (long) tab.GetInPath<LuaFunction>("Count").Call(tab)[0];
                if (null == subTab)
                {
                    variable.SetArraySize(count);
                    for (var index = 0; index < count; index++)
                    {
                        variable.ArrayVar[index].SetValue(TreeDebug.Instance.GetTreeVariableValue(tab, index + 1));
                    }
                }
                else
                {
                    var func = tab.GetInPath<LuaFunction>("GetChild");
                    for (var index = 0; index < count; index++)
                    {
                        if (func.Call(tab, index + 1)[0] as LuaTable == subTab)
                        {
                            variable.ArrayVar[index].SetValue(TreeDebug.Instance.GetTreeVariableValue(tab, index + 1));
                            break;
                        }
                    }
                }
            }
            else
            {
                variable.SetValue(TreeDebug.Instance.GetTreeVariableValue(tab));
            }
        }

        private void OnAddRefTree(LuaTable task, bool add)
        {
            if (null == task)
            {
                return;
            }

            if (add)
            {
                var parent = task.GetInPath<LuaTable>("parent");
                if (null == parent)
                {
                    return;
                }

                var refTreeParent = m_editorTree.GetTask(parent.GetInPath<int>("config.task.hashID"), parent.GetInPath<string>("debugID"));
                if (null == refTreeParent)
                {
                    return;
                }

                TreeReader.LoadRefTree(refTreeParent, task.GetInPath<string>("___refTreeName"));
                Rebind(m_editorTree);
            }
            else
            {
                var refTree = m_editorTree.GetTask(task.GetInPath<int>("config.task.hashID"), task.GetInPath<string>("debugID"));
                if (null == refTree?.Parent)
                {
                    return;
                }

                refTree.Parent.RemoveChild(refTree);
            }
        }
    }
}