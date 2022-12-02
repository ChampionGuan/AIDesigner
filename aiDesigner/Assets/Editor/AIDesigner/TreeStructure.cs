using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace AIDesigner
{
    public class TreeStructure
    {
        public string FullName
        {
            get => Directory + ShortName;
        }

        public string ShortName { get; private set; }
        public string Directory { get; private set; }
        public string Desc { get; private set; }
        public bool PauseWhenComplete { get; private set; }
        public bool ResetValuesOnRestart { get; private set; }
        public int TickInterval { get; private set; }
        public TreeTask Entry { get; private set; }
        public List<TreeRefVariable> Variables { get; private set; }

        public List<TreeTask> AuxiliaryTrees { get; private set; }
        public Rect AuxiliaryTreeRect { get; private set; }

        public bool IsRuntimeTree { get; private set; }

        private TreeRuntime m_runtimeTree;

        public TreeRuntime RuntimeTree
        {
            get
            {
                if (!EditorApplication.isPlaying)
                {
                    return null;
                }

                return m_runtimeTree;
            }
        }

        public int RuntimeID
        {
            get
            {
                if (null == RuntimeTree)
                {
                    return 0;
                }

                return RuntimeTree.ID;
            }
        }

        public GameObject RuntimeHost
        {
            get => RuntimeTree?.Host;
        }

        public TreeStructure(string fullName, int runningID, string desc, int tickInterval, bool pauseWhenComplete, bool resetValuesOnRestart, List<TreeRefVariable> variables, TreeTask entry, List<TreeTask> auxiliaryTrees)
        {
            // default
            Desc = desc;
            SetFullName(fullName, false);
            TickInterval = tickInterval < 0 ? 0 : tickInterval;
            PauseWhenComplete = pauseWhenComplete;
            ResetValuesOnRestart = resetValuesOnRestart;
            Entry = entry;
            Variables = variables ?? new List<TreeRefVariable>();
            AuxiliaryTrees = auxiliaryTrees ?? new List<TreeTask>();

            // running
            m_runtimeTree = TreeDebug.Instance.GetTree(runningID);
            IsRuntimeTree = null != m_runtimeTree;
            m_runtimeTree?.Rebind(this);
        }

        public bool Save(bool autoSave = false)
        {
            if (autoSave && !GraphPreferences.Instance.IsAutoSave)
            {
                return false;
            }

            if (IsRuntimeTree)
            {
                return false;
            }

            if (!VerifyTaskInsIdValid())
            {
                ResetTaskID();
            }

            return TreeWriter.SaveTree(this, GraphPreferences.Instance.IsSaveAsJson);
        }

        public void Clear()
        {
            m_runtimeTree?.Unbind();
        }

        public bool SetFullName(string fullName, bool save = true)
        {
            if (fullName == FullName)
            {
                return false;
            }

            var shortName = fullName;
            var directory = string.Empty;
            if (fullName.Contains("/"))
            {
                shortName = fullName.Substring(fullName.LastIndexOf("/") + 1);
                directory = fullName.Substring(0, fullName.LastIndexOf("/") + 1);
            }

            if (!save)
            {
                ShortName = shortName;
                Directory = directory;
                return true;
            }

            if (TreeWriter.RenameTree(FullName, fullName))
            {
                ShortName = shortName;
                Directory = directory;
                return true;
            }

            return false;
        }

        public void SetDesc(string desc)
        {
            if (desc == Desc)
            {
                return;
            }

            Desc = desc;
            Save(true);
        }

        public void SetPauseWhenComplete(bool value)
        {
            if (value == PauseWhenComplete)
            {
                return;
            }

            PauseWhenComplete = value;
            Save(true);
        }

        public void SetResetValuesOnRestart(bool value)
        {
            if (value == ResetValuesOnRestart)
            {
                return;
            }

            ResetValuesOnRestart = value;
            Save(true);
        }

        public void SetTickInterval(int value)
        {
            if (value == TickInterval || value < 0)
            {
                return;
            }

            TickInterval = value;
            Save(true);
        }

        public void SetAuxiliaryTreeRect(Rect rect)
        {
            AuxiliaryTreeRect = rect;
            for (var i = 0; i < AuxiliaryTrees.Count; i++)
            {
                AuxiliaryTrees[i].SetRect(AuxiliaryTreeRect);
            }
        }

        public void SetRuntimeTreeVariable(TreeRefVariable variable)
        {
            RuntimeTree?.SetTreeVariable(variable);
        }

        public void SetRuntimeTaskDisabled(string debugID, bool disabled)
        {
            RuntimeTree?.SetTaskDisabled(debugID, disabled);
        }

        public void SetRuntimeTaskVariable(string debugID, TreeTaskVariable variable)
        {
            RuntimeTree?.SetTaskVariable(debugID, variable);
        }

        public void SetRuntimeTaskAbortType(string debugID, AbortType type)
        {
            RuntimeTree?.SetTaskAbortType(debugID, type);
        }

        public TaskStateType GetRuntimeTaskState(string debugID)
        {
            var stateType = TaskStateType.None;
            if (null != RuntimeTree)
            {
                stateType = RuntimeTree.GetTaskState(debugID);
            }

            return stateType;
        }

        public List<string> GetReferenceTrees()
        {
            var refTrees = new List<string>();

            void toDo(TreeTask task)
            {
                if (task.Name == Define.RefTreeTaskName && task.Variables.Count > 0 && null != task.Variables[0])
                {
                    var name = (string) task.Variables[0].Value;
                    if (TreeReader.HasTree(name) && !refTrees.Contains(name))
                    {
                        refTrees.Add(name);
                    }
                }
            }

            RecursionTask(toDo, Entry);

            TreeReader.LegalTreeName(ref refTrees);
            return refTrees;
        }

        public List<TreeTask> GetBreakpointTasks()
        {
            var tasks = new List<TreeTask>();

            void toDo(TreeTask task)
            {
                if (task.IsBreakpoint)
                {
                    tasks.Add(task);
                }
            }

            RecursionTask(toDo, Entry);
            foreach (var task in AuxiliaryTrees)
            {
                RecursionTask(toDo, task);
            }

            return tasks;
        }

        public List<TreeRefVariable> GetUnusedVariables()
        {
            var refKeys = new List<string>();

            void toDo(TreeTask task)
            {
                foreach (var var in task.Variables)
                {
                    if (!var.IsShared)
                    {
                        continue;
                    }

                    if (var.IsArray)
                    {
                        foreach (var subVar in var.ArrayVar)
                        {
                            if (!string.IsNullOrEmpty(subVar.SharedKey) && !refKeys.Contains(subVar.SharedKey))
                            {
                                refKeys.Add(subVar.SharedKey);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(var.SharedKey) && !refKeys.Contains(var.SharedKey))
                        {
                            refKeys.Add(var.SharedKey);
                        }
                    }
                }
            }

            RecursionTask(toDo, Entry);
            var variables = new List<TreeRefVariable>();
            foreach (var v in Variables)
            {
                if (refKeys.Contains(v.Key))
                {
                    continue;
                }

                variables.Add(v);
            }

            return variables;
        }

        public bool UpdateSharedVariableKey(string fromName, string toName)
        {
            if (IsRuntimeTree)
            {
                return false;
            }

            Variable fromVar = Variables.Find(x => x.Key == fromName);
            if (null == fromVar)
            {
                return false;
            }

            Variable toVar = Variables.Find(x => x.Key == toName);
            if (null != toVar)
            {
                return false;
            }

            fromVar.Key = toName;

            void toDo(TreeTask task)
            {
                task.ChangeSharedVariableKey(fromName, toName);
            }

            RecursionTask(toDo, Entry);
            foreach (var task in AuxiliaryTrees)
            {
                RecursionTask(toDo, task);
            }

            Save(true);
            return true;
        }

        public bool UpdateSharedVariableType(string name, VarType type)
        {
            if (IsRuntimeTree)
            {
                return false;
            }

            var refVariable = Variables.Find(x => x.Key == name);
            if (null == refVariable || refVariable.Type == type)
            {
                return false;
            }

            refVariable.SetType(type);

            void toDo(TreeTask task)
            {
                task.ChangeSharedVariableType(name, type);
            }

            RecursionTask(toDo, Entry);
            foreach (var task in AuxiliaryTrees)
            {
                RecursionTask(toDo, task);
            }

            Save(true);
            return true;
        }

        public TreeRefVariable GetSharedVariable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return Variables.Find(x => x.Key == name);
        }

        public bool AddSharedVariable(string name, VarType type, bool isArray, string desc = null)
        {
            if (IsRuntimeTree)
            {
                return false;
            }

            var var = Variables.Find(x => x.Key == name);
            if (null != var)
            {
                return false;
            }

            var variable = new TreeRefVariable(name, type, desc, isArray);
            variable.IsArrayExpanded = true;
            Variables.Add(variable);
            Save(true);
            return true;
        }

        public bool AddSharedVariable(TreeRefVariable variable)
        {
            if (IsRuntimeTree || null == variable)
            {
                return false;
            }

            var var = Variables.Find(x => x.Key == variable.Key);
            if (null != var)
            {
                return false;
            }

            variable.IsArrayExpanded = true;
            Variables.Add(variable);
            Save(true);
            return true;
        }

        public bool RemoveSharedVariable(string name)
        {
            if (IsRuntimeTree)
            {
                return false;
            }

            var var = Variables.Find(x => x.Key == name);
            if (null == var)
            {
                return false;
            }

            UpdateSharedVariableKey(var.Key, null);
            Variables.Remove(var);
            Save(true);
            return true;
        }

        public bool HasSharedVariable(string name)
        {
            return null != Variables.Find(x => x.Key == name);
        }

        public void SortSharedVariable()
        {
            Variables.Sort((a, b) => a.Key.CompareTo(b.Key));
        }

        public List<int> GetTasksHashId(List<TreeTask> tasks)
        {
            var hashId = new List<int>();
            if (null == tasks)
            {
                return hashId;
            }

            foreach (var task in tasks)
            {
                hashId.Add(null != task ? task.HashID : 0);
            }

            return hashId;
        }

        public TreeTask GetTask(int hashId, string debugId)
        {
            TreeTask result = null;

            void toDo(TreeTask task)
            {
                if (null == result && task.HashID == hashId)
                {
                    if (IsRuntimeTree && !string.IsNullOrEmpty(debugId))
                    {
                        if (task.DebugID == debugId)
                        {
                            result = task;
                        }
                    }
                    else
                    {
                        result = task;
                    }
                }
            }

            foreach (var task in AuxiliaryTrees)
            {
                if (null != result)
                {
                    break;
                }

                RecursionTask(toDo, task);
            }

            if (null == result)
            {
                RecursionTask(toDo, Entry);
            }

            return result;
        }

        public void AddTask(TreeTask task)
        {
            BreakOffRelation(task);
            if (null == Entry)
            {
                Entry = new TreeTask(EditorTaskReader.EntryTaskPath, false, AbortType.None);
            }

            if (null == Entry.Children)
            {
                Entry.AddChild(task);
                task.SetOffset(new Vector2((task.TaskRect.width - Entry.TaskRect.width) * -0.5f, Define.TaskHeight * 2), false);
            }
            else
            {
                task.SetOffset(task.TaskRect.position - AuxiliaryTreeRect.position, false);
                AuxiliaryTrees.Add(task);
            }

            Save(true);
        }

        public void AddTasks(List<TreeTask> tasks)
        {
            if (null == tasks)
            {
                return;
            }

            for (var i = tasks.Count - 1; i >= 0; i--)
            {
                AddTask(tasks[i]);
            }

            Save(true);
        }

        public void AddTask(TreeTask task, TreeTask parent)
        {
            if (null == task)
            {
                return;
            }

            if (null == parent || parent.OutType == TaskOutType.No)
            {
                AddTask(task);
                return;
            }

            if (parent.OutType == TaskOutType.One && null != parent.Children)
            {
                while (parent.Children.Count > 0)
                {
                    AddTask(parent.Children[0]);
                }
            }

            BreakOffRelation(task);
            parent.AddChild(task);

            Save(true);
        }

        public void AddTasks(List<TreeTask> tasks, TreeTask parent)
        {
            if (null == tasks)
            {
                return;
            }

            for (var i = tasks.Count - 1; i >= 0; i--)
            {
                AddTask(tasks[i], parent);
            }

            Save(true);
        }

        public bool DeleteTask(TreeTask task)
        {
            if (null == task || task.Path == EditorTaskReader.EntryTaskPath)
            {
                return false;
            }

            BreakOffRelation(task);
            AddTasks(task.Children);

            Save(true);
            return true;
        }

        public void ResetTaskID()
        {
            void toDo(TreeTask task)
            {
                task.ResetHashID();
            }

            RecursionTask(toDo, Entry);
            foreach (var task in AuxiliaryTrees)
            {
                RecursionTask(toDo, task);
            }

            Save(true);
        }

        public void RecursionTask(Action<TreeTask> todo, TreeTask task, bool before = true)
        {
            if (null == task)
            {
                return;
            }

            if (before)
            {
                todo?.Invoke(task);
            }

            if (null != task.Children)
            {
                foreach (var child in task.Children)
                {
                    RecursionTask(todo, child, before);
                }
            }

            if (!before)
            {
                todo?.Invoke(task);
            }
        }

        public void RecursionTask<T>(Action<TreeTask, T> todo, TreeTask task, T t, bool before = true)
        {
            if (null == task)
            {
                return;
            }

            if (before)
            {
                todo?.Invoke(task, t);
            }

            if (null != task.Children)
            {
                foreach (var child in task.Children)
                {
                    RecursionTask(todo, child, t, before);
                }
            }

            if (!before)
            {
                todo?.Invoke(task, t);
            }
        }

        private void BreakOffRelation(TreeTask task)
        {
            if (null == task)
            {
                return;
            }

            task.Parent?.RemoveChild(task);
            if (AuxiliaryTrees.Contains(task))
            {
                AuxiliaryTrees.Remove(task);
            }
        }

        private bool VerifyTaskInsIdValid()
        {
            var result = true;
            var taskInsID = new List<int>();

            void toDo(TreeTask task)
            {
                if (taskInsID.Contains(task.HashID))
                {
                    result = false;
                    Debug.Log($"[Error]:the task(<color=#ff0000>{task.GetFullPath()}</color>) has the same instance id:(<color=#ff0000>{task.HashID}</color>).");
                }
                else if (result)
                {
                    taskInsID.Add(task.HashID);
                }
            }

            RecursionTask(toDo, Entry);
            if (result)
            {
                foreach (var task in AuxiliaryTrees)
                {
                    RecursionTask(toDo, task);
                }
            }

            return result;
        }

        public static bool IsLegalName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Length > 2 && TreeReader.IsLegalTreeName(name) && !AIDesignerLogicUtility.IsStartWithNumber(name);
        }
    }
}
