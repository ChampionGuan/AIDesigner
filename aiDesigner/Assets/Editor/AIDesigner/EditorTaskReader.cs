using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;
using System;
using UnityEditor;

namespace AIDesigner
{
    public static class EditorTaskReader
    {
        private static Action<List<string>, List<object>> m_varOptions;
        private static EditorTask m_findTask;
        private static LuaFunction m_loadOptions;
        private static string m_luaRootFullPath;
        private static string m_luaString;

        public static string EntryTaskPath { get; private set; }
        public static Dictionary<TaskType, EditorTaskTab> TaskTabs { get; private set; }

        public static string LuaString
        {
            get
            {
                if (null == m_luaString)
                {
                    m_luaString = AIDesignerLogicUtility.FileRead($"{Application.dataPath}/{Define.CustomSettings.OptionReaderFilePath}");
                }

                return m_luaString;
            }
        }

        public static void Read()
        {
            m_findTask = null;
            m_luaString = null;
            m_loadOptions = AIDesignerLuaEnv.Instance.DoString(LuaString)[0] as LuaFunction;
            m_luaRootFullPath = Path.GetFullPath($"{Define.CustomSettings.AppDataPath}/{Define.CustomSettings.LuaRootPath}").Replace("\\", "/");
            TaskTabs = new Dictionary<TaskType, EditorTaskTab>();

            var files = new List<string>();
            foreach (var path in Define.TaskPath)
            {
                files.AddRange(AIDesignerLogicUtility.GetAllFilesPathInDir($"{Define.CustomSettings.AppDataPath}/{Define.CustomSettings.LuaRootPath}{path}", "*.lua"));
            }

            foreach (var path in files)
            {
                ParseTask(path);
            }

            void sort(EditorTaskTab tab)
            {
                tab.Tasks.Sort((a, b) => a.Name.CompareTo(b.Name));
            }

            foreach (var category in TaskTabs.Values)
            {
                RecursionTaskTab(sort, category);
            }
        }

        public static void AddTask(EditorTask task)
        {
            if (!TaskTabs.ContainsKey(task.Type))
            {
                TaskTabs.Add(task.Type, new EditorTaskTab(GetTaskCategoryName(task.Type)));
            }

            if (task.Type == TaskType.Entry)
            {
                EntryTaskPath = task.Path;
            }

            TaskTabs[task.Type].Add(task, task.Category);
        }

        public static EditorTask GetTask(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (null != m_findTask && m_findTask.Path == path)
            {
                return m_findTask;
            }

            EditorTask task = null;

            void find(EditorTaskTab tab)
            {
                if (null == task)
                {
                    task = tab.Tasks.Find(x => x.Path == path);
                }
            }

            foreach (var tab in TaskTabs.Values)
            {
                RecursionTaskTab(find, tab);
            }

            if (null == task)
            {
                return null;
            }

            m_findTask = task;
            return task;
        }

        public static bool HasTask(string path)
        {
            return null != GetTask(path);
        }

        public static void OpenTask(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = $"{Define.CustomSettings.AppDataPath}/{Define.CustomSettings.LuaRootPath}{path}".Replace(".", "/") + ".lua";
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            System.Diagnostics.Process.Start(path);
            // AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(path));
        }

        public static void LocateTask(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = $"{Define.CustomSettings.AppDataPath}/{Define.CustomSettings.LuaRootPath}{path}".Replace(".", "/") + ".lua";
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            System.Diagnostics.Process.Start("Explorer", $"/select,{Path.GetDirectoryName(path)}\\{Path.GetFileName(path)}");
            // Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        public static void RecursionTaskTab(Action<EditorTaskTab> todo, EditorTaskTab tab, bool before = true)
        {
            if (null == tab || null == todo)
            {
                return;
            }

            if (before)
            {
                todo.Invoke(tab);
            }

            if (null != tab.Tabs)
            {
                foreach (var subTab in tab.Tabs)
                {
                    RecursionTaskTab(todo, subTab, before);
                }
            }

            if (!before)
            {
                todo.Invoke(tab);
            }
        }

        public static void RecursionTaskTab<T>(Action<EditorTaskTab, T> todo, EditorTaskTab tab, T t, bool before = true)
        {
            if (null == tab || null == todo)
            {
                return;
            }

            if (before)
            {
                todo.Invoke(tab, t);
            }

            if (null != tab.Tabs)
            {
                foreach (var subTab in tab.Tabs)
                {
                    RecursionTaskTab(todo, subTab, t, before);
                }
            }

            if (!before)
            {
                todo.Invoke(tab, t);
            }
        }

        public static string GetTaskCategoryName(TaskType type)
        {
            switch (type)
            {
                case TaskType.Condition: return "Condition";
                case TaskType.Action: return "Action";
                case TaskType.Composite: return "Composite";
                case TaskType.Decorator: return "Decorator";
                case TaskType.Entry: return "Entry";
            }

            return null;
        }

        private static bool IsArrayVariable(ref string str, ref ArrayType type)
        {
            if (type == ArrayType.None && str.Contains("[]"))
            {
                type = ArrayType.Regular;
            }

            if ((type == ArrayType.None || type == ArrayType.Regular) && str.Contains("AIArrayVar"))
            {
                type = ArrayType.Shared;
            }

            str = str.Replace("[]", "");
            str = str.Replace("AIArrayVar", "");
            return type != ArrayType.None;
        }

        private static bool IsSharedVariable(ref string str)
        {
            bool result = str.Contains("AIVar");
            str = str.Replace("AIVar", "");
            return result;
        }

        private static bool IsAnyTypeVariable(ref string str)
        {
            bool result = str.Contains("any");
            str = str.Replace("any", "");
            return result;
        }

        private static bool IsValidVariable(string str, ref string errorText)
        {
            str = str.Trim().ToLower();
            if (string.IsNullOrEmpty(str) || AIDesignerLogicUtility.IsStartWithNumber(str))
            {
                return false;
            }

            bool result = true;
            switch (str)
            {
                case "debugid":
                case "name":
                case "type":
                case "disabled":
                case "tree":
                case "view":
                case "master":
                case "context":
                case "parent":
                case "children":
                case "onawake":
                case "onreset":
                case "onenter":
                case "onexit":
                case "onpause":
                case "onupdate":
                case "onlateupdate":
                case "ondestroy":
                case "canrepeater":
                case "canrunchild":
                case "addtimer":
                case "removetimer":
                case "isnull":
                case "log":
                case "warn":
                case "error":
                    result = false;
                    break;
            }

            if (!result)
            {
                errorText = $"[the name:{str} conflicts with the internal variable names, please check!]";
            }

            return result;
        }

        private static VarType GetVariableType(ref string str)
        {
            if (str.Contains("String") || str.Contains("string"))
            {
                return VarType.String;
            }

            if (str.Contains("Boolean") || str.Contains("boolean"))
            {
                return VarType.Boolean;
            }

            if (str.Contains("Vector2Int") || str.Contains("vector2Int"))
            {
                return VarType.Vector2Int;
            }

            if (str.Contains("Vector2") || str.Contains("vector2"))
            {
                return VarType.Vector2;
            }

            if (str.Contains("Vector3Int") || str.Contains("vector3Int"))
            {
                return VarType.Vector3Int;
            }

            if (str.Contains("Vector3") || str.Contains("vector3"))
            {
                return VarType.Vector3;
            }

            if (str.Contains("Vector4") || str.Contains("vector4"))
            {
                return VarType.Vector4;
            }

            if (str.Contains("Object") || str.Contains("object") || str.Contains("any"))
            {
                return VarType.Object;
            }

            if (str.Contains("Float") || str.Contains("float") || str.Contains("Fix") || str.Contains("fix"))
            {
                return VarType.Float;
            }

            if (str.Contains("Int") || str.Contains("int"))
            {
                return VarType.Int;
            }

            return VarType.None;
        }

        private static TaskType GetTaskType(string str)
        {
            if (str.Contains("AICondition"))
            {
                return TaskType.Condition;
            }

            if (str.Contains("AIAction"))
            {
                return TaskType.Action;
            }

            if (str.Contains("AIComposite"))
            {
                return TaskType.Composite;
            }

            if (str.Contains("AIDecorator"))
            {
                return TaskType.Decorator;
            }

            if (str.Contains("AIEntry"))
            {
                return TaskType.Entry;
            }

            return TaskType.None;
        }

        private static void ParseTask(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            // local Parent = require("AIDesigner.Task.Action.Parent")
            //
            // ---时间倒计时
            // ---单位秒
            // ---IconName:Test.png
            // ---Category:Test/MyAction
            // ---@class SystemAI.Countdown:Parent
            // ---@field clock AIVar|Float
            // local Countdown = class("Countdown", Parent)

            var descMark = "---";
            var classMark = "---@class ";
            var fieldMark = "---@field ";
            var lines = File.ReadAllLines(path);
            for (var i = 0; i < lines.Length; i++)
            {
                //---向指定目标释放指定类型的技能
                //---@class CastSkill:AIAction
                if (!lines[i].Contains(classMark))
                {
                    continue;
                }

                var taskType = TaskType.None;
                var parentName = string.Empty;
                ParseType(lines[i].Replace(classMark, ""), ref taskType, ref parentName);

                var variables = new List<EditorTaskVariable>();
                if (taskType == TaskType.None)
                {
                    var parentFullPath = string.Empty;
                    var parentLuaPath = string.Empty;
                    ParseParent(lines, parentName, ref parentLuaPath, ref parentFullPath);
                    if (!string.IsNullOrEmpty(parentFullPath))
                    {
                        ParseTask(parentFullPath);
                    }

                    var parentTask = GetTask(parentLuaPath);
                    if (null != parentTask)
                    {
                        foreach (var variable in parentTask.Variables)
                        {
                            variables.Add(variable.DeepCopy());
                        }

                        taskType = parentTask.Type;
                    }
                }

                if (taskType == TaskType.None)
                {
                    continue;
                }

                var index = i;
                var taskDesc = string.Empty;
                var taskIcon = string.Empty;
                var taskCategory = string.Empty;
                var taskPath = path.Replace(m_luaRootFullPath, "").Replace("/", ".").Replace(@"\", ".").Replace(".lua", "");
                while (--index >= 0)
                {
                    if (lines[index].Contains(descMark))
                    {
                        var desc = lines[index].Replace(descMark, "");
                        if (desc.ToLower().Contains("category"))
                        {
                            ParseCategory(desc, ref taskCategory);
                        }
                        else if (desc.ToLower().Contains("iconname"))
                        {
                            ParseIcon(desc, ref taskIcon);
                        }
                        else if (!string.IsNullOrEmpty(desc))
                        {
                            if (!string.IsNullOrEmpty(taskDesc))
                            {
                                taskDesc = desc + "\n" + taskDesc;
                            }
                            else
                            {
                                taskDesc = desc + taskDesc;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                index = i;
                while (++index < lines.Length && lines[index].Contains(fieldMark))
                {
                    ParseVariable(lines[index].Replace(fieldMark, ""), out var variable, out var errorText);
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        Debug.LogError($"[task Variable parsing error] [task path:{taskPath}] {errorText}");
                    }

                    if (null != variable && null == variables.Find(x => x.Key == variable.Key))
                    {
                        variables.Add(variable);
                    }
                }

                AddTask(new EditorTask(taskPath, taskType, taskDesc, taskIcon, taskCategory, variables, AbortType.None));
            }
        }

        private static void ParseIcon(string str, ref string name)
        {
            // IconName:xxx.png
            var split = str.Split(':');
            if (split.Length < 2)
            {
                return;
            }

            name = split[1].TrimStart().Split(' ')[0];
            if (!string.IsNullOrEmpty(name))
            {
                name = "{SkinColor}" + name;
            }
        }

        private static void ParseCategory(string str, ref string category)
        {
            // Category:xxx/xxx
            var split = str.Split(':');
            if (split.Length < 2)
            {
                return;
            }

            category = split[1].Replace(@"\", "/").TrimStart().Split(' ')[0];

            while (category.StartsWith("/"))
            {
                category = category.Substring(1);
            }

            while (category.EndsWith("/"))
            {
                category = category.Substring(0, category.Length - 1);
            }

            category = category.TrimEnd();
        }

        private static void ParseType(string str, ref TaskType type, ref string parentName)
        {
            // AI.CastSkill:AIAction
            var split = str.Split(':');
            if (split.Length < 2)
            {
                return;
            }

            parentName = split[1].TrimStart().Split(' ')[0];
            type = GetTaskType(parentName);
        }

        private static void ParseParent(string[] lines, string name, ref string luaPath, ref string fullPath)
        {
            //local AIAction = require("Battle.AIDesigner.Base.AITask")
            if (name.Contains("."))
            {
                name = name.Substring(name.LastIndexOf(".") + 1);
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains(name) || !lines[i].Contains("require"))
                {
                    continue;
                }

                var splits = lines[i].Replace(@"'", @"""").Split('"');
                if (splits.Length < 2 || !splits[1].EndsWith(name))
                {
                    break;
                }

                luaPath = splits[1];
                fullPath = $"{m_luaRootFullPath}{luaPath.Replace(".", "/")}.lua";
                break;
            }
        }

        private static void ParseVariable(string str, out EditorTaskVariable variable, out string errorText)
        {
            //skillCaster AIVar|Object|Actor 描述信息
            variable = null;
            errorText = null;

            str = str.TrimStart();
            var name = str.Substring(0, str.IndexOf(" "));
            if (!IsValidVariable(name, ref errorText))
            {
                return;
            }

            str = str.Replace(name, "").TrimStart();
            var varType = str.Split('|');
            if (varType.Length < 1)
            {
                return;
            }

            for (var i = 0; i < varType.Length; i++)
            {
                varType[i] = varType[i].TrimStart();
                if (varType[i].Contains(" "))
                {
                    varType[i] = varType[i].Substring(0, varType[i].IndexOf(' '));
                }
            }

            var type = VarType.None;
            for (var i = 0; i < varType.Length; i++)
            {
                type = GetVariableType(ref varType[i]);
                if (type != VarType.None)
                {
                    break;
                }
            }

            if (type == VarType.None)
            {
                return;
            }

            Options options = null;
            var isShared = false;
            var isAnyType = false;
            var arrayType = ArrayType.None;
            for (var i = 0; i < varType.Length; i++)
            {
                if (!string.IsNullOrEmpty(varType[i]))
                {
                    str = str.Replace(varType[i], "");
                }

                if (IsSharedVariable(ref varType[i]) && !isShared)
                {
                    isShared = true;
                }

                if (IsAnyTypeVariable(ref varType[i]) && !isAnyType)
                {
                    isAnyType = true;
                }

                if (IsArrayVariable(ref varType[i], ref arrayType))
                {
                    if (arrayType == ArrayType.Shared)
                    {
                        isShared = true;
                    }
                }

                if (null == options && !string.IsNullOrEmpty(varType[i]) && (type == VarType.Int || type == VarType.String))
                {
                    m_varOptions = (key, value) =>
                    {
                        if (null == key || null == value || key.Count < 1 || value.Count < 1)
                        {
                            return;
                        }

                        var newValue = new object[value.Count];
                        for (var m = 0; m < value.Count; m++)
                        {
                            newValue[m] = Variable.ParseToCS(value[m], type);
                        }

                        options = new Options(name, key.ToArray(), newValue);
                    };
                    if (null == m_loadOptions)
                    {
                        return;
                    }

                    m_loadOptions.Call(varType[i], m_varOptions);
                }
            }

            str = str.Replace("|", " ").TrimStart();
            var desc = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                desc = str;
            }

            variable = new EditorTaskVariable(name, type, desc, arrayType, isShared, isShared && isAnyType, options);
        }
    }
}