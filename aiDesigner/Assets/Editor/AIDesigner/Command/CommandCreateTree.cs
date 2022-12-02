using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandCreateTree : CommandBase
    {
        private string m_fromName;
        private string m_toName;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandCreateTree(string toName) : base()
        {
            m_toName = toName;
            m_fromName = null != CurrTree ? CurrTree.FullName : null;
        }

        protected override bool VerifyIsValid()
        {
            return !TreeReader.HasTree(m_toName);
        }

        protected override bool OnUnDo()
        {
            TreeChart.Instance.DeleteTree(m_toName);
            TreeChart.Instance.LoadTree(m_fromName);
            return true;
        }

        protected override bool OnReDo()
        {
            if (TreeWriter.CreateTree(m_toName))
            {
                TreeChart.Instance.LoadTree(m_toName);
            }

            return true;
        }
    }
}