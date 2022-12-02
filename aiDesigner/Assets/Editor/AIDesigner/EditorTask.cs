using System.Collections.Generic;

namespace AIDesigner
{
    public class EditorTask : Task
    {
        public string LowerName { get; private set; }
        public string IconName { get; private set; }
        public string Category { get; private set; }
        public bool VisibleFlag { get; private set; }
        public List<EditorTaskVariable> Variables { get; private set; }

        public EditorTask(string path, TaskType type, string desc, string iconName, string category, List<EditorTaskVariable> variable, AbortType abortType) : base(null, path, type, abortType)
        {
            Desc = desc;
            IconName = iconName;
            Category = category;
            Variables = variable;
            VisibleFlag = true;
            LowerName = Name.ToLower();
        }

        public void SetVisible(bool visible)
        {
            VisibleFlag = visible;
        }
    }
}