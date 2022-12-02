using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandDisabledTask : CommandBase
    {
        private TreeTask m_task;
        private bool m_fromDisabled;
        private bool m_toDisabled;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandDisabledTask(TreeTask task, bool fromDisabled, bool toDisabled) : base()
        {
            m_task = task;
            m_fromDisabled = fromDisabled;
            m_toDisabled = toDisabled;
        }

        protected override bool OnUnDo()
        {
            var task = CurrTree?.GetTask(m_task.HashID, m_task.DebugID);
            if (null == task)
            {
                return false;
            }

            task.IsDisabled = m_fromDisabled;
            CurrTree.SetRuntimeTaskDisabled(task.DebugID, m_fromDisabled);
            return true;
        }

        protected override bool OnReDo()
        {
            var task = CurrTree?.GetTask(m_task.HashID, m_task.DebugID);
            if (null == task)
            {
                return false;
            }

            task.IsDisabled = m_toDisabled;
            CurrTree.SetRuntimeTaskDisabled(task.DebugID, m_toDisabled);
            return true;
        }
    }
}