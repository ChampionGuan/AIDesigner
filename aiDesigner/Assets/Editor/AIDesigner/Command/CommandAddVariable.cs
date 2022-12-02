using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandAddVariable : CommandBase
    {
        private string m_name;
        private VarType m_type;
        private bool m_isArray;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandAddVariable(string name, VarType type, bool isArray) : base()
        {
            m_name = name;
            m_type = type;
            m_isArray = isArray;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.RemoveSharedVariable(m_name);
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.AddSharedVariable(m_name, m_type, m_isArray);
        }
    }
}