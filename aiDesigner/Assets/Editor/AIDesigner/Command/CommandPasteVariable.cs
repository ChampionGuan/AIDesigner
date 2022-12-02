using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandPasteVariable : CommandBase
    {
        private TreeRefVariable m_pasteRefVariable;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandPasteVariable(TreeRefVariable refVariable) : base()
        {
            m_pasteRefVariable = refVariable.DeepCopy();
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.RemoveSharedVariable(m_pasteRefVariable.Key);
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.AddSharedVariable(m_pasteRefVariable.DeepCopy());
        }
    }
}