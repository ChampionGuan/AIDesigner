using System.Collections.Generic;
using UnityEditor;

namespace AIDesigner
{
    public class EditorTaskTab
    {
        public List<EditorTaskTab> Tabs { get; private set; }
        public List<EditorTask> Tasks { get; private set; }
        public string Name { get; private set; }

        public bool VisibleFlag { get; private set; }
        public bool FoldoutFlag { get; set; }

        public EditorTaskTab(string name)
        {
            Name = name;
            Tabs = new List<EditorTaskTab>();
            Tasks = new List<EditorTask>();
            FoldoutFlag = true;
            VisibleFlag = true;
        }

        public void Add(EditorTask task, string taskTab)
        {
            while (taskTab.StartsWith("/"))
            {
                taskTab = taskTab.Substring(1);
            }

            if (!string.IsNullOrEmpty(taskTab))
            {
                var name = taskTab.Split('/')[0];
                var tab = Tabs.Find(x => x.Name == name);
                if (null == tab)
                {
                    tab = new EditorTaskTab(name);
                    Tabs.Add(tab);
                }

                tab.Add(task, taskTab.Replace(name, ""));
            }
            else
            {
                var old = Tasks.Find(x => x.Name == task.Name);
                if (null != old)
                {
                    Tasks.Remove(old);
                }

                Tasks.Add(task);
            }
        }

        public void SetVisibleFlag(string name)
        {
            foreach (var tab in Tabs)
            {
                tab.SetVisibleFlag(name);
            }

            VisibleFlag = false;

            foreach (var tab in Tabs)
            {
                if (tab.VisibleFlag)
                {
                    VisibleFlag = true;
                    break;
                }
            }

            foreach (var task in Tasks)
            {
                task.SetVisible(string.IsNullOrEmpty(name) || task.LowerName.Contains(name));
                if (task.VisibleFlag)
                {
                    VisibleFlag = true;
                }
            }
        }
    }
}