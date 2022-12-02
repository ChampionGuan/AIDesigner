using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using System.IO;

namespace AIDesigner
{
    public static class TreeReader
    {
        private static string m_luaString;

        public static string LuaString
        {
            get
            {
                if (null == m_luaString)
                {
                    m_luaString = AIDesignerLogicUtility.FileRead($"{Application.dataPath}/{Define.CustomSettings.TreeReaderFilePath}");
                }

                return m_luaString;
            }
        }

        public static List<string> AllTreesName { get; private set; }

        public static void Read()
        {
            m_luaString = null;
            AllTreesName = new List<string>();

            var configRootPath = $"{Define.CustomSettings.AppDataPath}/{Define.ConfigFullPath}";
            var editorConfigRootPath = $"{Define.CustomSettings.AppDataPath}/{Define.EditorConfigFullPath}";
            if (!Directory.Exists(configRootPath))
            {
                return;
            }

            var configPaths = new List<string>(Directory.GetFiles(configRootPath, "*.lua", SearchOption.AllDirectories));
            var editorConfigPaths = new List<string>(Directory.GetFiles(editorConfigRootPath, "*.lua", SearchOption.AllDirectories));

            for (var i = configPaths.Count - 1; i >= 0; i--)
            {
                configPaths[i] = $"{Path.GetDirectoryName(configPaths[i])}/{Path.GetFileNameWithoutExtension(configPaths[i])}".Replace("\\", "/");
            }

            for (var i = editorConfigPaths.Count - 1; i >= 0; i--)
            {
                editorConfigPaths[i] = $"{Path.GetDirectoryName(editorConfigPaths[i])}/{Path.GetFileNameWithoutExtension(editorConfigPaths[i])}".Replace("\\", "/");
            }

            if (Define.ConfigFullPath.Contains(Define.EditorConfigFullPath))
            {
                for (var i = editorConfigPaths.Count - 1; i >= 0; i--)
                {
                    if (editorConfigPaths[i].Contains(Define.ConfigFullPath))
                    {
                        editorConfigPaths.RemoveAt(i);
                    }
                }
            }
            else if (Define.EditorConfigFullPath.Contains(Define.ConfigFullPath))
            {
                for (var i = configPaths.Count - 1; i >= 0; i--)
                {
                    if (configPaths[i].Contains(Define.EditorConfigFullPath))
                    {
                        configPaths.RemoveAt(i);
                    }
                }
            }

            for (var i = configPaths.Count - 1; i >= 0; i--)
            {
                configPaths[i] = configPaths[i].Replace(configRootPath, "");
            }

            for (var i = editorConfigPaths.Count - 1; i >= 0; i--)
            {
                editorConfigPaths[i] = editorConfigPaths[i].Replace(editorConfigRootPath, "");
            }

            foreach (var name in configPaths)
            {
                if (editorConfigPaths.Contains(name) && !AllTreesName.Contains(name))
                {
                    AllTreesName.Add(name);
                }
            }
        }

        public static bool IsLegalTreeName(string str)
        {
            return !str.Contains("/") && !str.Contains(".");
        }

        public static string StorageTreeName(string name)
        {
            return name.Replace("/", ".");
        }

        public static void LegalTreeName(ref string name)
        {
            name = name.Replace(".", "/");
        }

        public static void LegalTreeName(ref List<string> names)
        {
            for (var i = 0; i < names.Count; i++)
            {
                var name = names[i];
                LegalTreeName(ref name);
                names[i] = name;
            }
        }

        public static bool HasTree(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (null == AllTreesName)
            {
                Read();
            }

            LegalTreeName(ref name);
            return AllTreesName.Contains(name);
        }

        public static bool LoadTree(string name, int runningID, bool addHistory, Action<TreeStructure, bool> callBack)
        {
            if (!HasTree(name))
            {
                return false;
            }

            var loadTree = GetFunction("LoadTree", LuaString);
            if (null == loadTree)
            {
                return false;
            }

            loadTree.Call(name, runningID, addHistory, callBack);
            return true;
        }

        public static bool LoadRefTree(TreeTask parent, string name)
        {
            if (!HasTree(name))
            {
                return false;
            }

            var loadRefTree = GetFunction("LoadRefTree", LuaString);
            if (null == loadRefTree)
            {
                return false;
            }

            loadRefTree.Call(parent, name);
            return true;
        }

        private static LuaFunction GetFunction(string name, string luaStr)
        {
            var objs = AIDesignerLuaEnv.Instance.DoString(luaStr);
            if (null == objs || objs.Length < 1)
            {
                return null;
            }

            return (objs[0] as LuaTable).Get<LuaFunction>(name);
        }

        private static LuaFunction GetFunction(string luaStr)
        {
            var objs = AIDesignerLuaEnv.Instance.DoString(luaStr);
            if (null != objs && objs.Length > 0)
            {
                return objs[0] as LuaFunction;
            }

            return null;
        }
    }
}