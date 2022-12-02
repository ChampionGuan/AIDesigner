using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandCreateTreeFromTemplate : CommandBase
    {
        private TreeStructure m_fromTree;
        private TreeStructure m_toTree;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandCreateTreeFromTemplate(string toName) : base()
        {
            m_fromTree = CurrTree;
            if (toName == m_fromTree.FullName)
            {
                return;
            }

            TreeReader.LoadTree(toName, 0, false, (tree, addHistory) =>
            {
                m_toTree = tree;
                tree.SetFullName(m_fromTree.FullName, false);
            });
        }

        protected override bool OnUnDo()
        {
            TreeChart.Instance.SetTree(m_fromTree);
            return true;
        }

        protected override bool OnReDo()
        {
            if (null == m_fromTree || null == m_toTree)
            {
                return false;
            }

            TreeChart.Instance.SetTree(m_toTree);
            return true;
        }
    }
}