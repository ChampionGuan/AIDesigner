using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIDesigner
{
    public class CommandMoveTask : CommandBase
    {
        private Dictionary<int, MTask> m_tasks;

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        public CommandMoveTask(List<TreeTask> tasks, List<Vector2> pos) : base()
        {
            m_tasks = new Dictionary<int, MTask>();
            for (int i = 0; i < tasks.Count; i++)
            {
                m_tasks.Add(tasks[i].HashID, new MTask(tasks[i], pos[i]));
            }
        }

        protected override bool OnUnDo()
        {
            if (null == m_tasks)
            {
                return false;
            }

            foreach (var task in m_tasks.Values)
            {
                task.SetPos(task.FromPos);
            }

            CurrTree?.Save(false);

            return true;
        }

        protected override bool OnReDo()
        {
            if (null == m_tasks)
            {
                return false;
            }

            foreach (var task in m_tasks.Values)
            {
                task.SetPos(task.ToPos);
            }

            CurrTree?.Save(false);

            return true;
        }

        public void Refresh(List<TreeTask> tasks, List<Vector2> toPos)
        {
            if (null == tasks || null == toPos || tasks.Count != toPos.Count)
            {
                return;
            }

            var result = false;
            for (var i = 0; i < tasks.Count; i++)
            {
                if (!m_tasks.ContainsKey(tasks[i].HashID))
                {
                    continue;
                }

                result = m_tasks[tasks[i].HashID].UpdateToPos(tasks[i], toPos[i]) || result;
            }

            if (result)
            {
                CurrTree?.Save(false);
            }
        }

        public class MTask
        {
            public Vector2 FromPos { get; private set; }
            public Vector2 ToPos { get; private set; }

            private int m_hashID;
            private string m_debugID;

            public MTask(TreeTask task, Vector2 toPos)
            {
                ToPos = toPos;
                FromPos = task.TaskRectOffset;
                m_hashID = task.HashID;
                m_debugID = task.DebugID;
            }

            public void SetPos(Vector2 pos)
            {
                if (null == TreeChart.Instance.CurrTree)
                {
                    return;
                }

                var task = TreeChart.Instance.CurrTree.GetTask(m_hashID, m_debugID);
                task?.SetOffset(pos);
            }

            public bool UpdateToPos(TreeTask task, Vector2 pos)
            {
                if (task.HashID != m_hashID || task.DebugID != m_debugID)
                {
                    return false;
                }

                if (task.SetOffset(pos))
                {
                    ToPos = task.TaskRectOffset;
                    return true;
                }

                return false;
            }
        }
    }
}