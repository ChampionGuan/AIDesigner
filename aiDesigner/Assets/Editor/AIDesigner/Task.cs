namespace AIDesigner
{
    public class Task
    {
        public int HashID { get; private set; }
        public string Path { get; protected set; }
        public string Name { get; protected set; }
        public TaskType Type { get; protected set; }
        public string Desc { get; protected set; }
        public AbortType AbortType { get; set; }

        public Task(int? hashID, string path, TaskType taskType, AbortType abortType)
        {
            Path = null == path ? string.Empty : path;
            Name = Path.Substring(Path.LastIndexOf(".") + 1).Trim();
            Type = taskType;
            AbortType = abortType;
            HashID = null == hashID ? GetHashCode() : hashID.Value;
        }

        public void ResetHashID()
        {
            HashID = GetHashCode();
        }
    }
}