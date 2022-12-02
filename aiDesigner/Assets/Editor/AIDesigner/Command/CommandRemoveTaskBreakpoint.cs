using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandRemoveTaskBreakpoint : CommandBase
    {
        private List<TreeTask> m_tasks = new List<TreeTask>();

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandRemoveTaskBreakpoint(List<TreeTask> tasks) : base()
        {
            if (null == CurrTree)
            {
                return;
            }

            m_tasks = new List<TreeTask>(tasks);
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

            foreach (var v in m_tasks)
            {
                var task = CurrTree.GetTask(v.HashID, v.DebugID);
                if (null != task)
                {
                    task.IsBreakpoint = true;
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

            foreach (var v in m_tasks)
            {
                var task = CurrTree.GetTask(v.HashID, v.DebugID);
                if (null != task)
                {
                    task.IsBreakpoint = false;
                }
            }

            return true;
        }
    }
}