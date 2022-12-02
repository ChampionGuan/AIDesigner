using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandPasteVariableFromTemplate : CommandBase
    {
        private List<TreeRefVariable> m_pasteVariables = new List<TreeRefVariable>();

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandPasteVariableFromTemplate(string fromTree) : base()
        {
            if (null == CurrTree || fromTree == CurrTree.FullName)
            {
                return;
            }

            TreeReader.LoadTree(fromTree, 0, false, (tree, addHistory) =>
            {
                if (null == tree)
                {
                    return;
                }

                foreach (var variable in tree.Variables)
                {
                    if (null != CurrTree.GetSharedVariable(variable.Key))
                    {
                        continue;
                    }

                    m_pasteVariables.Add(variable.DeepCopy());
                }
            });
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

            return m_pasteVariables.Count > 0;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            foreach (var variable in m_pasteVariables)
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

            return true;
        }
    }
}