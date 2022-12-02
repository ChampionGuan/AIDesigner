using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace AIDesigner
{
    public class TreeChart : Singleton<TreeChart>
    {
        public TreeStructure CurrTree { get; private set; }

        public TreeTask CurrTask
        {
            get
            {
                if (null != CurrTaskByClick)
                {
                    return CurrTaskByClick;
                }
                else if (CurrTasksByBoxes.Count > 0)
                {
                    return CurrTasksByBoxes[0];
                }

                return null;
            }
        }

        public TreeTask CurrTaskByClick { get; private set; }
        public TreeTask CurrTaskBySuspend { get; private set; }
        public TreeTask CurrTaskByInArea { get; private set; }
        public TreeTask CurrTaskByOutArea { get; private set; }
        public List<TreeTask> CurrTasksByBoxes { get; private set; }
        public List<TreeTask> CurrTasksByCopy { get; private set; }
        public List<TreeTask> CurrTasksByLines { get; private set; }
        public List<TreeRefVariable> CurrVariablesByCopy { get; private set; }

        public bool BoxesSelecting { get; private set; }
        public bool LeftMouseDowning { get; private set; }
        public bool DragTasking { get; private set; }

        private GenericMenu m_referenceTreeMenu;
        private GenericMenu m_allTreeNameMenu;
        private GenericMenu m_mouseRightClickMenu;
        private GenericMenu m_historyTreeMenu;

        private Vector2 m_mouseUpPos;
        private Vector2 m_mouseDownPos;
        private Vector2 m_mouseDownTaskOffset;
        private Vector2 m_mouseDownTreeScrollPos;
        private Rect m_boxesRect = new Rect(0, 0, 0, 0);

        private Action<TreeStructure, bool> m_loadTreeAction;
        private History<string> m_historyTrees = new History<string>(10);

        private List<TreeTask> m_dragMoveTasks = new List<TreeTask>();
        private List<Vector2> m_dragMoveTasksToPos = new List<Vector2>();

        protected override void OnInstance()
        {
            m_loadTreeAction = (tree, addHistory) =>
            {
                ClearTree();
                CurrTree = tree;
                StoragePrefs.SetPref(PrefsType.TreeName, tree?.FullName);

                if (null != tree && addHistory)
                {
                    if (m_historyTrees.Contains(tree.FullName))
                    {
                        m_historyTrees.Remove(tree.FullName);
                    }

                    m_historyTrees.Do(tree.FullName);
                }
            };

            CurrTasksByBoxes = new List<TreeTask>();
            CurrTasksByCopy = new List<TreeTask>();
            CurrTasksByLines = new List<TreeTask>();
            CurrVariablesByCopy = new List<TreeRefVariable>();

            if (!EditorApplication.isPlaying)
            {
                // load the last tree !!
                CommandMgr.Instance.Do<CommandLoadTree>(null, 0, (string) StoragePrefs.GetPref(PrefsType.TreeName), 0, true);
            }
        }

        protected override void OnDispose()
        {
            CurrTree?.Save();
        }

        private void ClearTree()
        {
            DragTasking = false;
            LeftMouseDowning = false;
            CurrTaskByClick = null;
            CurrTaskBySuspend = null;
            CurrTasksByBoxes.Clear();
            CurrTasksByLines.Clear();
            CurrTaskByInArea = null;
            CurrTaskByOutArea = null;
            CurrTree?.Clear();
        }

        public void SetTree(TreeStructure tree)
        {
            ClearTree();
            CurrTree = tree;
            CurrTree?.Save(true);
        }

        public void DeleteTree(string name)
        {
            if (TreeWriter.DeleteTree(name) && null != CurrTree && name == CurrTree.FullName)
            {
                ClearTree();
                CurrTree = null;
            }
        }

        public void LoadTree(string name, int runningID = 0, bool addHistory = false)
        {
            if (null != CurrTree)
            {
                if (CurrTree.FullName == name && CurrTree.RuntimeID == runningID)
                {
                    return;
                }

                CurrTree.Save();
            }

            if (string.IsNullOrEmpty(name))
            {
                ClearTree();
                CurrTree = null;
            }
            else
            {
                TreeReader.LoadTree(name, EditorApplication.isPlaying ? runningID : 0, addHistory, m_loadTreeAction);
            }
        }

        public void LoadNextTree()
        {
            var name = m_historyTrees.Next();
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, name, 0, false);
        }

        public void LoadPrevTree()
        {
            var name = m_historyTrees.Prev();
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, name, 0, false);
        }

        public bool DeleteTasks()
        {
            var deleteTasks = new List<TreeTask>(CurrTasksByBoxes);
            if (!deleteTasks.Contains(CurrTaskByClick))
            {
                deleteTasks.Add(CurrTaskByClick);
            }

            if (CommandMgr.Instance.Do<CommandDeleteTask>(deleteTasks))
            {
                CurrTasksByBoxes.Clear();
                CurrTaskByClick = null;
                return true;
            }

            return false;
        }

        public bool BreakOffTasksRelation()
        {
            if (CurrTasksByLines.Count < 1)
            {
                return false;
            }

            var breakOffTasks = new List<TreeTask>();
            foreach (var task in CurrTasksByLines)
            {
                if (null != task && null != task.Parent && task != CurrTree.Entry)
                {
                    breakOffTasks.Add(task);
                }
            }

            if (breakOffTasks.Count < 1)
            {
                return false;
            }

            if (CommandMgr.Instance.Do<CommandBreakOffTaskRelation>(breakOffTasks))
            {
                CurrTasksByLines.Clear();
                return true;
            }

            return false;
        }

        public bool CopyTasks()
        {
            if (null == CurrTree)
            {
                return false;
            }

            CurrTasksByCopy.Clear();
            if (null != CurrTaskByClick && CurrTasksByBoxes.Contains(CurrTaskByClick))
            {
                CurrTasksByBoxes.Remove(CurrTaskByClick);
            }

            if (CurrTasksByBoxes.Contains(CurrTree.Entry))
            {
                CurrTasksByBoxes.Remove(CurrTree.Entry);
            }

            if (null != CurrTaskByClick && CurrTree.Entry != CurrTaskByClick)
            {
                CurrTasksByCopy.Add(CurrTaskByClick);
            }

            CurrTasksByCopy.AddRange(CurrTasksByBoxes);

            CurrVariablesByCopy.Clear();
            foreach (var task in CurrTasksByCopy)
            {
                foreach (var key in task.GetSharedVariableKeys())
                {
                    if (null != CurrVariablesByCopy.Find(x => x.Key == key))
                    {
                        continue;
                    }

                    var variable = CurrTree.GetSharedVariable(key);
                    if (null != variable)
                    {
                        CurrVariablesByCopy.Add(variable);
                    }
                }
            }

            return CurrTasksByCopy.Count > 0;
        }

        public bool PasteTasks(bool fromMousePos = true)
        {
            if (CurrTasksByCopy.Count < 1)
            {
                return false;
            }

            var offset = CurrTasksByCopy[0].TaskRect.position - (fromMousePos ? m_mouseUpPos : CurrTasksByCopy[0].TaskRect.position + new Vector2(2, 2) * Define.MeshSize);
            var pasteTasks = new List<TreeTask>(CurrTasksByCopy.Count);
            var pasteTasksParent = new List<TreeTask>(CurrTasksByCopy.Count);
            foreach (var task in CurrTasksByCopy)
            {
                var taskN = task.DeepCopy();
                var rect = new Rect(taskN.TaskRect);
                rect.position -= offset;
                taskN.SetOffset(Vector2.zero, false);
                taskN.SetRect(rect);
                pasteTasks.Add(taskN);
            }

            for (var m = 0; m < CurrTasksByCopy.Count; m++)
            {
                int? index = null;
                for (var n = 0; n < CurrTasksByCopy.Count; n++)
                {
                    if (m != n && CurrTasksByCopy[n] == CurrTasksByCopy[m].Parent)
                    {
                        index = n;
                        break;
                    }
                }

                pasteTasksParent.Add(null == index ? null : pasteTasks[index.Value]);
            }

            if (CommandMgr.Instance.Do<CommandPasteTask>(pasteTasks, CurrVariablesByCopy, pasteTasksParent))
            {
                CurrTasksByBoxes = pasteTasks;
            }

            return true;
        }

        public void ReplaceTask(string path)
        {
            if (!EditorTaskReader.HasTask(path))
            {
                return;
            }

            var fromTask = CurrTask;
            var toTask = new TreeTask(path, fromTask.IsDisabled, fromTask.AbortType);
            toTask.SetRect(new Rect(fromTask.TaskRect.x + (fromTask.TaskRect.width - toTask.TaskRect.width) * 0.5f, fromTask.TaskRect.y, 0, 0));
            toTask.Comment = fromTask.Comment;
            if (CommandMgr.Instance.Do<CommandReplaceTask>(fromTask, toTask))
            {
                CurrTaskByClick = toTask;
            }
        }

        public void ShowAllTrees()
        {
            m_allTreeNameMenu = new GenericMenu();
            m_allTreeNameMenu.AddItem(new GUIContent("[None]"), false, (name) => { CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, null, 0, true); }, null);
            foreach (var treeName in TreeReader.AllTreesName)
            {
                m_allTreeNameMenu.AddItem(new GUIContent(treeName), null != CurrTree && CurrTree.FullName == treeName, (name) => { CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, (string) name, 0, true); }, treeName);
            }

            m_allTreeNameMenu.ShowAsContext();
        }

        public void ShowHistoryTrees()
        {
            m_historyTreeMenu = new GenericMenu();
            var treeNames = m_historyTrees.All();
            if (treeNames.Count > 0)
            {
                foreach (var treeName in treeNames)
                {
                    m_historyTreeMenu.AddItem(new GUIContent(treeName), false, (name) => { CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, (string) name, 0, false); }, treeName);
                }
            }
            else
            {
                m_historyTreeMenu.AddDisabledItem(new GUIContent("[None]"), false);
            }

            m_historyTreeMenu.ShowAsContext();
        }

        public void ShowReferenceTrees()
        {
            m_referenceTreeMenu = null;
            if (null != CurrTree)
            {
                m_referenceTreeMenu = new GenericMenu();
                var rTrees = CurrTree.GetReferenceTrees();
                if (rTrees.Count > 0)
                {
                    foreach (var treeName in rTrees)
                    {
                        m_referenceTreeMenu.AddItem(new GUIContent(treeName), false, (name) => { CommandMgr.Instance.Do<CommandLoadTree>(CurrTree?.FullName, 0, (string) name, 0, true); }, treeName);
                    }
                }
                else
                {
                    m_referenceTreeMenu.AddDisabledItem(new GUIContent("[None]"), false);
                }
            }

            m_referenceTreeMenu?.ShowAsContext();
        }

        public Rect GetBoxesArea(Vector2 mousePos, Vector2 treeScrollPos)
        {
            Vector3 downPos = m_mouseDownPos + (m_mouseDownTreeScrollPos - treeScrollPos);
            m_boxesRect.Set(
                mousePos.x < downPos.x ? mousePos.x : downPos.x,
                mousePos.y < downPos.y ? mousePos.y : downPos.y,
                Mathf.Abs(mousePos.x - downPos.x),
                Mathf.Abs(mousePos.y - downPos.y)
            );
            return m_boxesRect;
        }

        public bool CheckIgnore(Vector2 mousePos)
        {
            return CheckLeftMouseUp(false, m_mouseDownPos);
        }

        public bool CheckLeftMouseMove(Vector2 mousePos, Vector2 treeScrollPos)
        {
            var result = null != CurrTaskBySuspend;
            CurrTaskBySuspend = null;
            if (null != CurrTree)
            {
                foreach (var task in CurrTree.AuxiliaryTrees)
                {
                    CurrTree.RecursionTask(CheckSuspendTask, task, mousePos);
                }

                CurrTree.RecursionTask(CheckSuspendTask, CurrTree.Entry, mousePos);
            }

            if (null != CurrTaskBySuspend)
            {
                result = true;
            }

            return result;
        }

        public bool CheckLeftMouseUp(bool inArea, Vector2 mousePos)
        {
            var result = false;

            if (inArea)
            {
                m_mouseUpPos = mousePos;
            }

            if (BoxesSelecting)
            {
                result = true;
                BoxesSelecting = false;
            }

            if (inArea && null != CurrTaskBySuspend && CurrTaskBySuspend.ExpandedBtnRect.Contains(mousePos))
            {
                CommandMgr.Instance.Do<CommandFoldoutTask>(CurrTaskBySuspend, CurrTaskBySuspend.IsFoldout, !CurrTaskBySuspend.IsFoldout);
            }

            if (inArea && null != CurrTaskBySuspend && CurrTaskBySuspend.DisabledBtnRect.Contains(mousePos))
            {
                CommandMgr.Instance.Do<CommandDisabledTask>(CurrTaskBySuspend, CurrTaskBySuspend.IsDisabled, !CurrTaskBySuspend.IsDisabled);
            }

            if (!inArea)
            {
                CurrTaskByInArea = null;
                CurrTaskByOutArea = null;
            }

            if (null != CurrTaskByInArea || null != CurrTaskByOutArea)
            {
                if (null != CurrTree)
                {
                    CurrTree.RecursionTask(CheckClickTask, CurrTree.Entry, mousePos);
                    foreach (var entry in CurrTree.AuxiliaryTrees)
                    {
                        CurrTree.RecursionTask(CheckClickTask, entry, mousePos);
                    }

                    if (null != CurrTaskByClick)
                    {
                        if (null == CurrTaskByInArea && CurrTaskByClick.InType == TaskInType.Yes)
                        {
                            CurrTaskByInArea = CurrTaskByClick;
                        }

                        if (null == CurrTaskByOutArea && CurrTaskByClick.OutType != TaskOutType.No)
                        {
                            CurrTaskByOutArea = CurrTaskByClick;
                        }
                    }
                }

                if (null != CurrTaskByInArea && null != CurrTaskByOutArea && CurrTaskByOutArea != CurrTaskByInArea && CurrTaskByInArea != CurrTree.Entry)
                {
                    var add = true;
                    var task = CurrTaskByOutArea.Parent;
                    while (null != task)
                    {
                        if (task == CurrTaskByInArea)
                        {
                            add = false;
                            break;
                        }

                        task = task.Parent;
                    }

                    if (add)
                    {
                        CommandMgr.Instance.Do<CommandConcatenateTask>(CurrTaskByInArea, CurrTaskByOutArea);
                    }
                }

                CurrTaskByInArea = null;
                CurrTaskByOutArea = null;
                result = true;
            }

            LeftMouseDowning = false;
            DragTasking = false;
            return result;
        }

        public bool CheckRightMouseUp(bool inArea, Vector2 mousePos)
        {
            var result = false;
            if (inArea)
            {
                var separator = true;
                m_mouseUpPos = mousePos;
                m_mouseRightClickMenu = new GenericMenu();
                if (null == CurrTree)
                {
                    m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Add Task"));
                    m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                }
                else if (CurrTasksByBoxes.Count > 1)
                {
                    m_mouseRightClickMenu.AddItem(new GUIContent("Copy Tasks"), false, () => { CopyTasks(); });
                    if (CurrTasksByCopy.Count > 0)
                    {
                        m_mouseRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, () => { PasteTasks(); });
                    }
                    else
                    {
                        m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                    }

                    m_mouseRightClickMenu.AddItem(new GUIContent("Delete Tasks"), false, () => { DeleteTasks(); });
                }
                else if (CurrTasksByBoxes.Count > 0 || null != CurrTaskByClick)
                {
                    var task = CurrTaskByClick;
                    if (null == task)
                    {
                        task = CurrTasksByBoxes[0];
                    }

                    if (task == CurrTree.Entry)
                    {
                        separator = false;
                    }
                    else
                    {
                        EditorTaskList.Menu(ref m_mouseRightClickMenu, "Replace", (path) => { ReplaceTask((string) path); });
                        m_mouseRightClickMenu.AddItem(new GUIContent("Edit Script"), false, () => { EditorTaskReader.OpenTask(CurrTask.Path); });
                        m_mouseRightClickMenu.AddItem(new GUIContent("Locate Script"), false, () => { EditorTaskReader.LocateTask(CurrTask.Path); });
                        if (task.IsBreakpoint)
                        {
                            m_mouseRightClickMenu.AddItem(new GUIContent("Remove Breakpoint"), false, () => { CurrTask.IsBreakpoint = false; });
                        }
                        else
                        {
                            m_mouseRightClickMenu.AddItem(new GUIContent("Set Breakpoint"), false, () => { CurrTask.IsBreakpoint = true; });
                        }

                        m_mouseRightClickMenu.AddItem(new GUIContent("Copy Task"), false, () => { CopyTasks(); });
                        if (CurrTasksByCopy.Count > 0)
                        {
                            m_mouseRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, () => { PasteTasks(); });
                        }
                        else
                        {
                            m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                        }

                        m_mouseRightClickMenu.AddItem(new GUIContent("Delete Task"), false, () => { DeleteTasks(); });
                    }
                }
                else
                {
                    EditorTaskList.Menu(ref m_mouseRightClickMenu, "Add Task", (path) => { CommandMgr.Instance.Do<CommandCreateTask>(path, new Vector2(m_mouseUpPos.x, m_mouseUpPos.y)); });
                    if (CurrTasksByCopy.Count > 0)
                    {
                        m_mouseRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, () => { PasteTasks(); });
                    }
                    else
                    {
                        m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
                    }
                }

                if (separator)
                {
                    m_mouseRightClickMenu.AddSeparator(string.Empty);
                }

                if (null == CurrTree)
                {
                    m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Save Tree"));
                    m_mouseRightClickMenu.AddDisabledItem(new GUIContent("Delete Tree"));
                }
                else
                {
                    m_mouseRightClickMenu.AddItem(new GUIContent("Save Tree"), false, () => { CurrTree.Save(); });
                    m_mouseRightClickMenu.AddItem(new GUIContent("Delete Tree"), false, () => { CommandMgr.Instance.Do<CommandDeleteTree>(CurrTree.FullName); });
                }

                m_mouseRightClickMenu.AddItem(new GUIContent("Create New Tree"), false, () =>
                {
                    if (!GraphCreate.Instance.IsDisplay) GraphCreate.Instance.DisplaySwitch();
                });

                m_mouseRightClickMenu.ShowAsContext();
                result = true;
            }

            return result;
        }

        public bool CheckLeftMouseDown(Vector2 mousePos, Vector2 treeScrollPos)
        {
            BoxesSelecting = true;
            LeftMouseDowning = true;
            m_mouseDownPos = mousePos;
            m_mouseDownTreeScrollPos = treeScrollPos;

            CurrTaskByClick = null;
            CurrTasksByLines.Clear();

            if (null == CurrTree)
            {
                return false;
            }

            CurrTree.RecursionTask(CheckClickTask, CurrTree.Entry, mousePos);
            foreach (var entry in CurrTree.AuxiliaryTrees)
            {
                CurrTree.RecursionTask(CheckClickTask, entry, mousePos);
            }

            var result = null != CurrTaskByInArea || null != CurrTaskByOutArea || null != CurrTaskByClick || CurrTasksByLines.Count > 0;
            if (result)
            {
                BoxesSelecting = false;
            }

            if (null != CurrTaskByClick)
            {
                CurrTaskByInArea = null;
                CurrTaskByOutArea = null;
            }

            if (null == CurrTaskByClick || !CurrTasksByBoxes.Contains(CurrTaskByClick))
            {
                CurrTasksByBoxes.Clear();
            }

            if (Event.current.clickCount == 2 &&
                null == CurrTree.RuntimeTree &&
                null != CurrTaskByClick && CurrTasksByBoxes.Count <= 0 &&
                CurrTaskByClick.IsRefTask && CurrTaskByClick.Variables.Count > 0)
            {
                CommandMgr.Instance.Do<CommandLoadTree>(CurrTree.FullName, 0, (string) CurrTaskByClick.Variables[0].Value, 0, true);
            }

            return result;
        }

        public bool CheckRightMouseDown(Vector2 mousePos, Vector2 delta)
        {
            m_mouseDownPos = mousePos;

            CurrTaskByClick = null;
            CurrTasksByLines.Clear();

            if (null != CurrTree)
            {
                CurrTree.RecursionTask(CheckClickTask, CurrTree.Entry, mousePos);
                foreach (var entry in CurrTree.AuxiliaryTrees)
                {
                    CurrTree.RecursionTask(CheckClickTask, entry, mousePos);
                }
            }

            CurrTaskByInArea = null;
            CurrTaskByOutArea = null;
            CurrTaskBySuspend = null;
            return true;
        }

        public bool CheckLeftMouseDrag(Vector2 mousePos, Vector2 delta)
        {
            if (null != CurrTaskByClick && LeftMouseDowning)
            {
                var offset = mousePos - CurrTaskByClick.TaskRect.center + m_mouseDownTaskOffset;
                offset.x = (int) (offset.x / Define.MeshSize) * Define.MeshSize;
                offset.y = (int) (offset.y / Define.MeshSize) * Define.MeshSize;

                m_dragMoveTasks.Clear();
                m_dragMoveTasksToPos.Clear();

                if (CurrTasksByBoxes.Count > 0)
                {
                    foreach (var task in CurrTasksByBoxes)
                    {
                        var move = false;
                        var parent = task.Parent;
                        while (true)
                        {
                            if (null == parent)
                            {
                                move = true;
                                break;
                            }

                            if (null != parent && CurrTasksByBoxes.Contains(parent))
                            {
                                move = false;
                                break;
                            }

                            parent = parent.Parent;
                        }

                        if (move)
                        {
                            m_dragMoveTasks.Add(task);
                            m_dragMoveTasksToPos.Add(task.TaskRectOffset + offset);
                        }
                    }
                }
                else
                {
                    m_dragMoveTasks.Add(CurrTaskByClick);
                    m_dragMoveTasksToPos.Add(CurrTaskByClick.TaskRectOffset + offset);
                }

                if (m_dragMoveTasks.Count > 0)
                {
                    if (!DragTasking)
                    {
                        CommandMgr.Instance.Do<CommandMoveTask>(m_dragMoveTasks, m_dragMoveTasksToPos);
                    }
                    else
                    {
                        (CommandMgr.Instance.Curr() as CommandMoveTask).Refresh(m_dragMoveTasks, m_dragMoveTasksToPos);
                    }

                    DragTasking = true;
                }
            }

            if (BoxesSelecting)
            {
                CurrTasksByBoxes.Clear();
                if (null != CurrTree)
                {
                    foreach (var task in CurrTree.AuxiliaryTrees)
                    {
                        CurrTree.RecursionTask(CheckBoxesTask, task, m_boxesRect);
                    }

                    CurrTree.RecursionTask(CheckBoxesTask, CurrTree.Entry, m_boxesRect);
                }
            }

            return BoxesSelecting || null != CurrTaskByInArea || null != CurrTaskByOutArea || null != CurrTaskByClick;
        }

        private void CheckClickTask(TreeTask task, Vector2 mousePos)
        {
            if (null == task)
            {
                return;
            }

            var parent = task.Parent;
            while (null != parent)
            {
                if (!parent.IsFoldout)
                {
                    return;
                }

                parent = parent.Parent;
            }

            var inSelected = task.InType == TaskInType.Yes && task.InRect.Contains(mousePos);
            if (inSelected)
            {
                CurrTaskByInArea = task;
            }

            var outSelected = task.OutType != TaskOutType.No && task.OutRect.Contains(mousePos);
            if (outSelected)
            {
                CurrTaskByOutArea = task;
            }

            var taskSelected = task.TaskRect.Contains(mousePos);
            if (taskSelected)
            {
                if (task != CurrTaskByClick)
                {
                    GUI.FocusControl(null);
                }

                CurrTaskByClick = task;
                m_mouseDownTaskOffset = task.TaskRect.center - mousePos;
            }

            if (inSelected || outSelected || taskSelected)
            {
                return;
            }

            foreach (var rect in task.LinesRect)
            {
                if (CurrTasksByLines.Contains(task))
                {
                    break;
                }

                if (rect.Contains(mousePos))
                {
                    CurrTasksByLines.Add(task);
                }
            }
        }

        private void CheckBoxesTask(TreeTask task, Rect rect)
        {
            if (null == task)
            {
                return;
            }

            if (CurrTasksByBoxes.Contains(task) || (rect.xMin > task.TaskRect.xMax || rect.yMin > task.TaskRect.yMax || task.TaskRect.xMin > rect.xMax || task.TaskRect.yMin > rect.yMax))
            {
                return;
            }

            var add = true;
            var parent = task.Parent;
            while (null != parent)
            {
                if (!parent.IsFoldout)
                {
                    add = false;
                    break;
                }

                parent = parent.Parent;
            }

            if (add)
            {
                CurrTasksByBoxes.Add(task);
            }
        }

        private void CheckSuspendTask(TreeTask task, Vector2 mousePos)
        {
            if (null == task)
            {
                return;
            }

            var parent = task.Parent;
            while (null != parent)
            {
                if (!parent.IsFoldout)
                {
                    return;
                }

                parent = parent.Parent;
            }

            var selected = task.SuspendRect.Contains(mousePos);
            if (selected)
            {
                CurrTaskBySuspend = task;
            }
        }
    }
}