using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandFoldoutTask : CommandBase
    {
        private TreeTask m_task;
        private bool m_fromFoldout;
        private bool m_toFoldout;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandFoldoutTask(TreeTask task, bool fromFoldout, bool toFoldout) : base()
        {
            m_task = task;
            m_fromFoldout = fromFoldout;
            m_toFoldout = toFoldout;
        }

        protected override bool OnUnDo()
        {
            var task = CurrTree?.GetTask(m_task.HashID, m_task.DebugID);
            if (null == task)
            {
                return false;
            }

            task.IsFoldout = m_fromFoldout;
            return true;
        }

        protected override bool OnReDo()
        {
            var task = CurrTree?.GetTask(m_task.HashID, m_task.DebugID);
            if (null == task)
            {
                return false;
            }

            task.IsFoldout = m_toFoldout;
            return true;
        }
    }
}