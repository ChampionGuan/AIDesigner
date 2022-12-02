using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandConcatenateTask : CommandBase
    {
        private TreeTask m_fromTask;
        private TreeTask m_fromTaskParent;

        private TreeTask m_toTask;
        private List<TreeTask> m_toTaskChildren;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandConcatenateTask(TreeTask fromTask, TreeTask toTask) : base()
        {
            m_fromTask = fromTask;
            m_fromTaskParent = m_fromTask.Parent;

            m_toTask = toTask;
            m_toTaskChildren = null == m_toTask.Children ? new List<TreeTask>() : new List<TreeTask>(m_toTask.Children);
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            foreach (var task in m_toTaskChildren)
            {
                CurrTree.AddTask(CurrTree.GetTask(task.HashID, task.DebugID), CurrTree.GetTask(m_toTask.HashID, m_toTask.DebugID));
            }

            CurrTree.AddTask(CurrTree.GetTask(m_fromTask.HashID, m_fromTask.DebugID), null == m_fromTaskParent ? null : CurrTree.GetTask(m_fromTaskParent.HashID, m_fromTaskParent.DebugID));
            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            CurrTree.AddTask(CurrTree.GetTask(m_fromTask.HashID, m_fromTask.DebugID), CurrTree.GetTask(m_toTask.HashID, m_toTask.DebugID));
            return true;
        }
    }
}