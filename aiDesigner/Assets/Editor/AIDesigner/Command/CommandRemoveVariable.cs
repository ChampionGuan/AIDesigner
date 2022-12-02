using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandRemoveVariable : CommandBase
    {
        private TreeRefVariable m_refVariable;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandRemoveVariable(TreeRefVariable refVariable) : base()
        {
            m_refVariable = refVariable.DeepCopy();
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.AddSharedVariable(m_refVariable.DeepCopy());
        }


        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.RemoveSharedVariable(m_refVariable.Key);
        }
    }
}