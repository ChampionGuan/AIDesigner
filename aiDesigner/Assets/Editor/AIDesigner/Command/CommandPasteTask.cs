using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandPasteTask : CommandBase
    {
        private List<TreeTask> m_pasteTasks;
        private List<TreeTask> m_pasteTasksParent;
        private List<TreeRefVariable> m_pasteVariables;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandPasteTask(List<TreeTask> tasks, List<TreeRefVariable> pasteVariables, List<TreeTask> tasksParent) : base()
        {
            m_pasteTasks = new List<TreeTask>(tasks);
            m_pasteTasksParent = new List<TreeTask>(tasksParent);
            m_pasteVariables = new List<TreeRefVariable>(pasteVariables);
        }

        protected override bool VerifyIsValid()
        {
            for (var i = m_pasteVariables.Count - 1; i >= 0; i--)
            {
                if (CurrTree.HasSharedVariable(m_pasteVariables[i].Key))
                {
                    m_pasteVariables.RemoveAt(i);
                }
            }

            return true;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            for (var m = 0; m < m_pasteTasks.Count; m++)
            {
                CurrTree.DeleteTask(CurrTree.GetTask(m_pasteTasks[m].HashID, m_pasteTasks[m].DebugID));
            }

            foreach (var variable in CurrTree.GetUnusedVariables())
            {
                CurrTree.RemoveSharedVariable(variable.Key);
            }

            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            foreach (var variable in m_pasteVariables)
            {
                CurrTree.AddSharedVariable(variable.DeepCopy());
            }

            for (var m = 0; m < m_pasteTasks.Count; m++)
            {
                CurrTree.AddTask(m_pasteTasks[m], null == m_pasteTasksParent[m] ? null : CurrTree.GetTask(m_pasteTasksParent[m].HashID, m_pasteTasksParent[m].DebugID));
            }

            return true;
        }
    }
}