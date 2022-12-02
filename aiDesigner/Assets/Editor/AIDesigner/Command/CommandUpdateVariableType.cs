using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandUpdateVariableType : CommandBase
    {
        private string m_name;
        private VarType m_fromType;
        private VarType m_toType;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandUpdateVariableType(string name, VarType fromType, VarType toType) : base()
        {
            m_name = name;
            m_fromType = fromType;
            m_toType = toType;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.UpdateSharedVariableType(m_name, m_fromType);
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.UpdateSharedVariableType(m_name, m_toType);
        }
    }
}