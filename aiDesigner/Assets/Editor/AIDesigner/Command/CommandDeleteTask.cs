using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandDeleteTask : CommandBase
    {
        private List<DTask> m_tasks = new List<DTask>();

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandDeleteTask(List<TreeTask> tasks) : base()
        {
            foreach (var task in tasks)
            {
                if (null != task)
                {
                    m_tasks.Add(new DTask(task));
                }
            }
        }

        protected override bool VerifyIsValid()
        {
            return m_tasks.Count > 0;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            foreach (var task in m_tasks)
            {
                CurrTree.AddTask(task.Task, null == task.TaskParent ? null : CurrTree.GetTask(task.TaskParent.HashID, task.TaskParent.DebugID));
                foreach (var child in task.TaskChildren)
                {
                    CurrTree.AddTask(CurrTree.GetTask(child.HashID, child.DebugID), task.Task);
                }
            }

            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            foreach (var task in m_tasks)
            {
                CurrTree.DeleteTask(CurrTree.GetTask(task.Task.HashID, task.Task.DebugID));
            }

            return true;
        }

        public class DTask
        {
            public TreeTask Task { get; private set; }
            public TreeTask TaskParent { get; private set; }
            public List<TreeTask> TaskChildren { get; private set; }

            public DTask(TreeTask task)
            {
                Task = task;
                TaskParent = task.Parent;
                TaskChildren = null == task.Children ? new List<TreeTask>() : new List<TreeTask>(task.Children);
            }
        }
    }
}