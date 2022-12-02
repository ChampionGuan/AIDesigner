using System;

namespace AIDesigner
{
    public class CommandMgr : Singleton<CommandMgr>
    {
        private History<CommandBase> m_history = new History<CommandBase>(100);

        public CommandBase Curr()
        {
            return m_history.Curr();
        }

        public bool UnDo()
        {
            var command = m_history.UnDo();
            if (null == command)
            {
                return false;
            }

            command.Undo();
            return true;
        }

        public bool ReDo()
        {
            var command = m_history.ReDo();
            if (null == command)
            {
                return false;
            }

            command.Redo();
            return true;
        }

        public bool Do<T>(params object[] data) where T : CommandBase
        {
            var command = Activator.CreateInstance(typeof(T), data) as T;
            if (command.Do())
            {
                m_history.Do(command);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}