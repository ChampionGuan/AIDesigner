using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandBase
    {
        private bool m_canUndo = false;

        public CommandBase()
        {
            m_canUndo = false;
        }

        public bool Do()
        {
            if (m_canUndo)
            {
                return false;
            }

            return VerifyIsValid() && Redo();
        }

        public bool Undo()
        {
            if (!m_canUndo)
            {
                return false;
            }

            m_canUndo = false;
            return OnUnDo();
        }

        public bool Redo()
        {
            if (m_canUndo)
            {
                return false;
            }

            m_canUndo = true;
            return OnReDo();
        }

        protected virtual bool VerifyIsValid()
        {
            return true;
        }

        protected virtual bool OnUnDo()
        {
            return true;
        }

        protected virtual bool OnReDo()
        {
            return true;
        }
    }
}