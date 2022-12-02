using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class TreeTask : Task
    {
        private string _comment;
        private Vector2 _taskRectCenter;
        private Vector2 _taskRectOffset;
        private Rect _taskRect;
        private Rect _iconRect;
        private Rect _inRect;
        private Rect _outRect;
        private Rect _expandedBtnRect;
        private Rect _disabledBtnRect;
        private Rect _expandedIconRect;
        private Rect _suspendRect;
        private Rect _commentRect;
        private Rect _abortTypeRect;
        private Rect _errorRect;
        private Rect _breakpointRect;
        private Rect _stateRect;
        private Rect[] _linesRect = new Rect[3];

        public string DebugID { get; set; }
        public TreeTask Parent { get; private set; }
        public List<TreeTask> Children { get; private set; }
        public List<TreeTaskVariable> Variables { get; private set; }

        public bool IsFoldout { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsBreakpoint { get; set; }
        public int ColorIndex { get; set; }
        public float StateTickValue { get; set; }
        public TaskStateType StateType { get; set; }
        public bool IsErrorTask { get; private set; }
        public bool IsRefTask { get; private set; }
        public TaskInType InType { get; private set; }
        public TaskOutType OutType { get; private set; }
        public Texture Icon { get; private set; }

        public string Comment
        {
            get => _comment;
            set
            {
                var _widthMax = AIDesignerLogicUtility.CalcMaxWidth(value) * 9;
                _comment = value;
                _commentRect.width = _widthMax > Define.TaskCommentMaxWidth ? Define.TaskCommentMaxWidth : _widthMax;
            }
        }

        public Rect TaskRect
        {
            get => _taskRect;
        }

        public Vector2 TaskRectOffset
        {
            get => _taskRectOffset;
        }

        public Rect IconRect
        {
            get => _iconRect;
        }

        public Rect InRect
        {
            get => _inRect;
        }

        public Rect OutRect
        {
            get => _outRect;
        }

        public Rect ExpandedBtnRect
        {
            get => _expandedBtnRect;
        }

        public Rect DisabledBtnRect
        {
            get => _disabledBtnRect;
        }

        public Rect ExpandedIconRect
        {
            get => _expandedIconRect;
        }

        public Rect SuspendRect
        {
            get => _suspendRect;
        }

        public Rect CommentRect
        {
            get => _commentRect;
        }

        public Rect AbortTypeRect
        {
            get => _abortTypeRect;
        }

        public Rect ErrorRect
        {
            get => _errorRect;
        }

        public Rect BreakpointRect
        {
            get => _breakpointRect;
        }

        public Rect StateRect
        {
            get => _stateRect;
        }

        public Rect[] LinesRect
        {
            get => _linesRect;
        }

        public TreeTask(int? hashID, string path, bool disabled, AbortType abortType) : base(hashID, path, TaskType.None, abortType)
        {
            Init(disabled);
        }

        public TreeTask(string path, bool disabled, AbortType abortType) : base(null, path, TaskType.None, abortType)
        {
            Init(disabled);
        }

        public void SetRect(Rect rect)
        {
            CalcRect(rect);
            if (null != Children)
            {
                foreach (var v in Children)
                {
                    v.SetRect(TaskRect);
                }
            }
        }

        public bool SetOffset(Vector2? offset, bool check = true)
        {
            if (null == offset)
            {
                offset = Vector2.zero;
            }

            var value = offset.Value;
            bool result;
            if (check)
            {
                value = new Vector2((int) (offset.Value.x / Define.MeshSize), (int) (offset.Value.y / Define.MeshSize)) * Define.MeshSize;
                result = _taskRectOffset != value;
            }
            else
            {
                result = true;
            }

            _taskRectOffset = value;
            Parent?.SortChild();

            return result;
        }

        public void UpdateVariable(TreeTaskVariable csVar)
        {
            if (null == csVar)
            {
                return;
            }

            for (var i = 0; i < Variables.Count; i++)
            {
                if (Variables[i].Key == csVar.Key)
                {
                    Variables[i] = csVar;
                    break;
                }
            }
        }

        public void UpdateVariable(string key, object luaVar)
        {
            if (null == Variables)
            {
                return;
            }

            foreach (var v in Variables)
            {
                if (v.Key == key)
                {
                    v.VarFromLua(luaVar);
                    break;
                }
            }
        }

        public void ChangeSharedVariableKey(string fromName, string toName)
        {
            if (null == Variables)
            {
                return;
            }

            foreach (var v in Variables)
            {
                v.ChangeSharedKey(fromName, toName);
            }
        }

        public void ChangeSharedVariableType(string name, VarType type)
        {
            if (null == Variables)
            {
                return;
            }

            foreach (var v in Variables)
            {
                v.ChangeSharedType(name, type);
            }
        }

        public List<string> GetSharedVariableKeys()
        {
            var keys = new List<string>();
            foreach (var var in Variables)
            {
                if (!var.IsShared)
                {
                    continue;
                }

                if (var.IsArray)
                {
                    foreach (var subVar in var.ArrayVar)
                    {
                        if (string.IsNullOrEmpty(subVar.SharedKey) || keys.Contains(subVar.SharedKey))
                        {
                            continue;
                        }

                        keys.Add(subVar.SharedKey);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(var.SharedKey) || keys.Contains(var.SharedKey))
                    {
                        continue;
                    }

                    keys.Add(var.SharedKey);
                }
            }

            return keys;
        }

        public bool ContainSharedVariableKey(string key)
        {
            if (null == Variables)
            {
                return false;
            }

            var _result = false;
            foreach (var v in Variables)
            {
                if (v.ContainSharedKey(key))
                {
                    _result = true;
                    break;
                }
            }

            return _result;
        }

        public void AddChild(TreeTask task, Vector2? posOffset = null, bool check = true)
        {
            if (null == Children)
            {
                Children = new List<TreeTask>();
            }

            if (Children.Contains(task))
            {
                Children.Remove(task);
            }

            if (null == posOffset)
            {
                posOffset = task.TaskRect.position - TaskRect.position;
                check = false;
            }

            task.SetOffset(posOffset.Value, check);
            task.Parent = this;
            Children.Add(task);
            SortChild();
        }

        public void SortChild()
        {
            Children?.Sort((a, b) => a.TaskRectOffset.x.CompareTo(b.TaskRectOffset.x));
        }

        public void RemoveChild(TreeTask task)
        {
            if (null == Children)
            {
                return;
            }

            if (Children.Contains(task))
            {
                Children.Remove(task);
                task.Parent = null;
            }
        }

        public int GetSiblingIndex()
        {
            if (null == Parent)
            {
                return 0;
            }

            for (var i = 0; i < Parent.Children.Count; i++)
            {
                if (Parent.Children[i] == this)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public string GetFullPath()
        {
            var path = string.Empty;
            var parent = this;
            while (true)
            {
                if (null == parent)
                {
                    break;
                }

                var name = parent.Name.Contains(".") ? parent.Name.Substring(parent.Name.LastIndexOf(".")) : parent.Name;
                if (null == parent.Parent)
                {
                    path = name + path;
                }
                else
                {
                    path = $"[{parent.GetSiblingIndex()}]/{parent.Name}{path}";
                }

                parent = parent.Parent;
            }

            return path;
        }

        public TreeTask DeepCopy()
        {
            var task = new TreeTask(Path, IsDisabled, AbortType);
            foreach (var v in Variables)
            {
                task.UpdateVariable(v.DeepCopy());
            }

            task._comment = Comment;
            task._taskRect = new Rect(TaskRect);
            task._suspendRect = new Rect(SuspendRect);
            task._inRect = new Rect(InRect);
            task._outRect = new Rect(OutRect);
            task._expandedBtnRect = new Rect(ExpandedBtnRect);
            task._disabledBtnRect = new Rect(DisabledBtnRect);
            task._expandedIconRect = new Rect(ExpandedIconRect);
            task._commentRect = new Rect(CommentRect);
            task._iconRect = new Rect(IconRect);
            task._errorRect = new Rect(ErrorRect);
            task._breakpointRect = new Rect(_breakpointRect);
            task._stateRect = new Rect(_stateRect);
            task._taskRectOffset = TaskRectOffset;
            return task;
        }

        private void Init(bool disabled)
        {
            ColorIndex = 0;
            Variables = new List<TreeTaskVariable>();
            var editorTask = EditorTaskReader.GetTask(Path);
            if (null == editorTask)
            {
                IsErrorTask = true;
                Desc = $"[AIDesigner 异常！！][请确认Task是否存在：{Path}]";
                Name = "Error Task";
                Debug.LogError(Desc);
            }
            else
            {
                IsErrorTask = false;
                IsDisabled = disabled;
                IsFoldout = true;
                IsRefTask = editorTask.Name == Define.RefTreeTaskName;
                Desc = editorTask.Desc;
                Name = editorTask.Name;
                Icon = AIDesignerUIUtility.LoadIcon(editorTask.IconName);

                if (null != editorTask.Variables && editorTask.Variables.Count > 0)
                {
                    foreach (var v in editorTask.Variables)
                    {
                        Variables.Add(new TreeTaskVariable(v.Key, v.Type, v.Desc, v.ArrayType, v.IsShared, v.IsAnyType, v.Options?.DeepCopy()));
                    }
                }

                Type = editorTask.Type;
                switch (Type)
                {
                    case TaskType.Entry:
                        InType = TaskInType.No;
                        OutType = TaskOutType.One;
                        Icon = Icon ?? AIDesignerUIUtility.LoadIcon("LightEntryIcon.png");
                        break;
                    case TaskType.Action:
                        InType = TaskInType.Yes;
                        OutType = TaskOutType.No;
                        Icon = Icon ?? AIDesignerUIUtility.LoadIcon("LightActionIcon.png");
                        break;
                    case TaskType.Composite:
                        InType = TaskInType.Yes;
                        OutType = TaskOutType.Mulit;
                        Icon = Icon ?? AIDesignerUIUtility.LoadIcon("LightCompositeIcon.png");
                        break;
                    case TaskType.Decorator:
                        InType = TaskInType.Yes;
                        OutType = TaskOutType.One;
                        Icon = Icon ?? AIDesignerUIUtility.LoadIcon("LightDecoratorIcon.png");
                        break;
                    case TaskType.Condition:
                        InType = TaskInType.Yes;
                        OutType = TaskOutType.No;
                        Icon = Icon ?? AIDesignerUIUtility.LoadIcon("LightConditionalIcon.png");
                        break;
                }
            }

            InitRect();
        }

        private void InitRect()
        {
            // Vector2 size = AIDesignerGUIStype.TaskCommentGUIStyle.CalcSize(new GUIContent(name));
            // float width = size.x + 10 > AIDesigner.Define.TaskWidth ? size.x + 10 : AIDesigner.Define.TaskWidth;
            // float height = AIDesigner.Define.TaskHeight;

            var width = Name.Length * 8 > Define.TaskWidth ? Name.Length * 8 : Define.TaskWidth;
            var height = Define.TaskHeight;

            _taskRect.size = new Vector2(width, height);
            _suspendRect.size = new Vector2(width, height + 15 * 2);
            _inRect.size = new Vector2(15 * 2, 12);
            _outRect.size = new Vector2(15 * 2, 12);
            _expandedBtnRect.size = new Vector2(12, 12);
            _disabledBtnRect.size = new Vector2(12, 12);
            _expandedIconRect.size = new Vector2(22, 5);
            _commentRect.size = new Vector2(100, height);
            _abortTypeRect.size = new Vector2(15, 15);
            _iconRect.size = new Vector2(28, 28);
            _errorRect.size = new Vector2(15, 15);
            _breakpointRect.size = new Vector2(10, 10);
            _stateRect.size = new Vector2(20, 20);
        }

        private void CalcRect(Rect rect)
        {
            _taskRect.x = rect.x + TaskRectOffset.x;
            _taskRect.y = rect.y + TaskRectOffset.y;
            _taskRectCenter.x = (int) (_taskRect.center.x / Define.MeshSize) * Define.MeshSize;
            _taskRectCenter.y = (int) (_taskRect.center.y / Define.MeshSize) * Define.MeshSize;
            _taskRect.center = _taskRectCenter;

            _suspendRect.x = TaskRect.x;
            _suspendRect.y = TaskRect.y - 15;
            _inRect.x = TaskRect.center.x - 15;
            _inRect.y = TaskRect.y - 9;
            _outRect.x = TaskRect.center.x - 15;
            _outRect.y = TaskRect.yMax - 1;
            _disabledBtnRect.x = TaskRect.x - 3;
            _disabledBtnRect.y = TaskRect.y - 13;
            _expandedBtnRect.x = TaskRect.x + 10;
            _expandedBtnRect.y = TaskRect.y - 13;
            _expandedIconRect.x = TaskRect.center.x - 10;
            _expandedIconRect.y = TaskRect.yMax + 1;
            _commentRect.x = TaskRect.xMax + 2;
            _commentRect.y = TaskRect.y;
            _abortTypeRect.x = TaskRect.x + 2;
            _abortTypeRect.y = TaskRect.y + 2;
            _iconRect.center = TaskRect.center;
            _iconRect.y -= 7;
            _errorRect.x = TaskRect.xMax - 10;
            _errorRect.y = TaskRect.yMin - 5;
            _breakpointRect.x = TaskRect.xMax - 12;
            _breakpointRect.y = TaskRect.yMin + 2;
            _stateRect.x = TaskRect.xMax - 20;
            _stateRect.y = TaskRect.yMax - 25;
        }
    }
}