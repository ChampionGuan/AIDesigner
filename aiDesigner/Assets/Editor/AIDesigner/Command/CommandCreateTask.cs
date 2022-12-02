using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandCreateTask : CommandBase
    {
        private TreeTask m_createdTask;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandCreateTask(string path, Vector2 pos) : base()
        {
            m_createdTask = new TreeTask(path, false, AbortType.None);
            m_createdTask.SetRect(new Rect(pos.x, pos.y, 0, 0));
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            CurrTree.DeleteTask(CurrTree.GetTask(m_createdTask.HashID, m_createdTask.DebugID));
            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree || null == m_createdTask)
            {
                return false;
            }

            CurrTree.AddTask(m_createdTask);
            return true;
        }
    }
}