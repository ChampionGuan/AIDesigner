using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandDeleteTree : CommandBase
    {
        private string m_deleteTreeName;
        private TreeStructure m_deleteTreeStructure;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandDeleteTree(string deleteTreeName) : base()
        {
            m_deleteTreeName = deleteTreeName;
            m_deleteTreeStructure = (null != CurrTree && CurrTree.FullName == m_deleteTreeName) ? CurrTree : null;
        }

        protected override bool VerifyIsValid()
        {
            return null != m_deleteTreeStructure && TreeReader.HasTree(m_deleteTreeName);
        }

        protected override bool OnUnDo()
        {
            m_deleteTreeStructure.Save();
            TreeChart.Instance.LoadTree(m_deleteTreeStructure.FullName);
            return true;
        }

        protected override bool OnReDo()
        {
            TreeChart.Instance.DeleteTree(m_deleteTreeName);
            return true;
        }
    }
}