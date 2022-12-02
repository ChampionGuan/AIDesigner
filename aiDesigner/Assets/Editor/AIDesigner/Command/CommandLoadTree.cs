using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandLoadTree : CommandBase
    {
        private string m_fromName;
        private int m_fromRuntimeID;
        private string m_toName;
        private int m_toRuntimeID;
        private bool m_addHistory;

        public CommandLoadTree(string fromName, int fromRuntimeID, string toName, int toRuntimeID, bool addHistory) : base()
        {
            m_fromName = fromName;
            m_fromRuntimeID = fromRuntimeID;
            m_toName = toName;
            m_toRuntimeID = toRuntimeID;
            m_addHistory = addHistory;
        }

        protected override bool VerifyIsValid()
        {
            return TreeReader.HasTree(m_toName);
        }

        protected override bool OnUnDo()
        {
            TreeChart.Instance.LoadTree(m_fromName, m_fromRuntimeID, m_addHistory);
            return true;
        }

        protected override bool OnReDo()
        {
            TreeChart.Instance.LoadTree(m_toName, m_toRuntimeID, m_addHistory);
            return true;
        }
    }
}