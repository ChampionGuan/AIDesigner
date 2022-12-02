using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandTreeMenuIndex : CommandBase
    {
        private int m_fromIndex;
        private int m_toIndex;

        public CommandTreeMenuIndex(int fromIndex, int toIndex) : base()
        {
            m_fromIndex = fromIndex;
            m_toIndex = toIndex;
        }

        protected override bool OnUnDo()
        {
            GraphTree.Instance.SetMenuIndex(m_fromIndex);
            return true;
        }

        protected override bool OnReDo()
        {
            GraphTree.Instance.SetMenuIndex(m_toIndex);
            return true;
        }
    }
}