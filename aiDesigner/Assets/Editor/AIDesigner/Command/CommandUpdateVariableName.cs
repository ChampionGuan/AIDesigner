using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandUpdateVariableName : CommandBase
    {
        private string m_fromName;
        private string m_toName;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandUpdateVariableName(string fromName, string toName) : base()
        {
            m_fromName = fromName;
            m_toName = toName;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.UpdateSharedVariableKey(m_toName, m_fromName);
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.UpdateSharedVariableKey(m_fromName, m_toName);
        }
    }
}