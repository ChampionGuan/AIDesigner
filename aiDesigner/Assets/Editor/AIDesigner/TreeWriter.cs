using UnityEngine;
using XLua;

namespace AIDesigner
{
    public static class TreeWriter
    {
        private static string m_luaString;

        public static string LuaString
        {
            get
            {
                if (null == m_luaString)
                {
                    m_luaString = AIDesignerLogicUtility.FileRead($"{Application.dataPath}/{Define.CustomSettings.TreeWriterFilePath}");
                }

                return m_luaString;
            }
        }

        public static void Read()
        {
            m_luaString = null;
        }

        public static bool DeleteTree(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var delete = GetFunction("Delete", LuaString);
            if (null == delete)
            {
                return false;
            }

            delete.Call(name, name);
            TreeReader.Read();
            return true;
        }

        public static bool RenameTree(string fromName, string toName)
        {
            if (string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(toName))
            {
                return false;
            }

            var rename = GetFunction("Rename", LuaString);
            if (null == rename)
            {
                return false;
            }

            rename.Call(fromName, toName);
            TreeReader.Read();
            return true;
        }

        public static bool CreateTree(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var create = GetFunction("Create", LuaString);
            if (null == create)
            {
                return false;
            }

            while (name.StartsWith("/"))
            {
                name = name.Substring(1);
            }

            create.Call(name);
            TreeReader.Read();
            return true;
        }

        public static bool SaveTree(TreeStructure tree, bool saveToJson = false)
        {
            var save = GetFunction("Save", LuaString);
            if (null == save)
            {
                return false;
            }

            save.Call(tree, saveToJson);
            TreeReader.Read();
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
