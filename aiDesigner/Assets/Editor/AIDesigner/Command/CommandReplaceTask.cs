using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandReplaceTask : CommandBase
    {
        private TreeTask m_toTask;
        private TreeTask m_fromTask;
        private TreeTask m_fromTaskParent;
        private List<TreeTask> m_fromTaskChildren;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandReplaceTask(TreeTask fromTask, TreeTask toTask) : base()
        {
            m_toTask = toTask;
            m_fromTask = fromTask;
            m_fromTaskParent = m_fromTask.Parent;
            m_fromTaskChildren = null == m_fromTask.Children ? new List<TreeTask>() : new List<TreeTask>(m_fromTask.Children);
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            CurrTree.DeleteTask(CurrTree.GetTask(m_toTask.HashID, m_toTask.DebugID));
            CurrTree.AddTask(m_fromTask, null == m_fromTaskParent ? null : CurrTree.GetTask(m_fromTaskParent.HashID, m_fromTaskParent.DebugID));
            foreach (var task in m_fromTaskChildren)
            {
                CurrTree.AddTask(CurrTree.GetTask(task.HashID, task.DebugID), m_fromTask);
            }

            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            CurrTree.AddTask(m_toTask, null == m_fromTaskParent ? null : CurrTree.GetTask(m_fromTaskParent.HashID, m_fromTaskParent.DebugID));
            foreach (var task in m_fromTaskChildren)
            {
                CurrTree.AddTask(CurrTree.GetTask(task.HashID, task.DebugID), m_toTask);
            }

            CurrTree.DeleteTask(CurrTree.GetTask(m_fromTask.HashID, m_fromTask.DebugID));
            return true;
        }
    }
}