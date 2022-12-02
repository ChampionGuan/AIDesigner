using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandRenameTree : CommandBase
    {
        private string m_fromName;
        private string m_toName;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandRenameTree(string fromName, string toName) : base()
        {
            m_fromName = fromName;
            m_toName = toName;
        }

        protected override bool VerifyIsValid()
        {
            return !TreeReader.HasTree(m_toName);
        }

        protected override bool OnUnDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.SetFullName(m_fromName);
        }

        protected override bool OnReDo()
        {
            if (null == CurrTree)
            {
                return false;
            }

            return CurrTree.SetFullName(m_toName);
        }
    }
}