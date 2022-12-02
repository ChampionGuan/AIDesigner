using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandBreakOffTaskRelation : CommandBase
    {
        private List<DTask> m_tasks = new List<DTask>();

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandBreakOffTaskRelation(List<TreeTask> tasks) : base()
        {
            foreach (var task in tasks)
            {
                if (null == task || null == task.Parent)
                {
                    continue;
                }

                m_tasks.Add(new DTask(task));
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
                CurrTree.AddTask(CurrTree.GetTask(task.Task.HashID, task.Task.DebugID), CurrTree.GetTask(task.TaskParent.HashID, task.TaskParent.DebugID));
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
                CurrTree.AddTask(CurrTree.GetTask(task.Task.HashID, task.Task.DebugID));
            }

            return true;
        }

        public class DTask
        {
            public TreeTask Task { get; private set; }
            public TreeTask TaskParent { get; private set; }

            public DTask(TreeTask task)
            {
                Task = task;
                TaskParent = task.Parent;
            }
        }
    }
}