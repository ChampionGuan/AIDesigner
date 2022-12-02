using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandClearUnusedVariable : CommandBase
    {
        private List<TreeRefVariable> m_variables = new List<TreeRefVariable>();

        protected TreeStructure CurrTree => TreeChart.Instance.CurrTree;

        public CommandClearUnusedVariable() : base()
        {
            if (null == CurrTree)
            {
                return;
            }

            foreach (var variable in CurrTree.GetUnusedVariables())
            {
                m_variables.Add(variable.DeepCopy());
            }
        }

        protected override bool VerifyIsValid()
        {
            return m_variables.Count > 0;
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree || m_variables.Count < 1)
            {
                return false;
            }

            foreach (var v in m_variables)
            {
                CurrTree.AddSharedVariable(v.DeepCopy());
            }

            return true;
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree || m_variables.Count < 1)
            {
                return false;
            }

            foreach (var v in m_variables)
            {
                CurrTree.RemoveSharedVariable(v.Key);
            }

            return true;
        }
    }
}