using XLua;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using Test;
using XLua.LuaDLL;

namespace AIDesigner
{
    public class Launcher : MonoBehaviour
    {
        public XLuaEnv LuaEnv { get; protected set; }
        private static Launcher _instance;
        private LuaTable luaTest;
        private LuaFunction funcTick;

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        void Start()
        {
            if (null != LuaEnv)
            {
                LuaEnv.Dispose();
            }

            _instance = this;

            LuaEnv = new XLuaEnv();
            LuaEnv.AddLoader(LoadLua);

            LuaEnv.Global.Set<string, Action<object>>("g_Log", Debug.Log);
            LuaEnv.Global.Set<string, Action<object>>("g_Warn", Debug.LogWarning);
            LuaEnv.Global.Set<string, Action<object>>("g_Error", Debug.LogError);

            LuaEnv.DoString("require 'TestTree' ");
            luaTest = LuaEnv.Global.Get<LuaTable>("Test");
            funcTick = luaTest.Get<LuaFunction>("Update");
            

            Lua.xlua_getglobal(LuaEnv.L,"printT");
            Lua.lua_pushstring(LuaEnv.L,"Testtttt");
            Lua.lua_pcall(LuaEnv.L, 1, 0, -1);
        }

        private void OnDestroy()
        {
            if (LuaEnv == null)
            {
                return;
            }

            LuaEnv.Dispose();
            LuaEnv = null;
        }

        private void Update()
        {
            funcTick?.Call();
        }

        private byte[] LoadLua(ref string filePath)
        {
            // Debug.Log(filePath);

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            try
            {
                string luaPath = "";
                byte[] luaBytes = null;

                // filePath = filePath.ToLower();
                filePath = filePath.Replace(@".", "/");
                filePath = filePath.Replace(@"\", "/");
                luaPath = filePath + ".lua";
                luaBytes = ReadFileToBytes(string.Format("{0}/logic/{1}", Application.dataPath, luaPath));
                if (null == luaBytes) luaBytes = ReadFileToBytes(string.Format("{0}/cfgs/{1}", Application.dataPath, luaPath));
                if (null == luaBytes) luaBytes = ReadFileToBytes(string.Format("{0}/_lcg/{1}", Application.dataPath, luaPath));
                if (null == luaBytes) luaBytes = ReadFileToBytes(string.Format("{0}/{1}", Application.dataPath, luaPath));

                return luaBytes;
            }
            catch
            {
                return null;
            }
        }

        private byte[] ReadFileToBytes(string path)
        {
            path = path.Replace(@"//", "/");
            if (!File.Exists(path))
            {
                //Debug.Log("[warning] 文件不存在: " + path);
                return null;
            }

            byte[] bytes = null;
            try
            {
                //using (FileStream fs = File.Open(path, FileMode.Open))
                //{
                //    bytes = new byte[fs.Length];
                //    fs.Read(bytes, 0, bytes.Length);
                //}
                bytes = System.Text.Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning(path + "[error] 读取文件错误: " + e);
                return null;
            }

            return bytes;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Launcher))]
    public class BattleLauncherEditor : Editor
    {
        string testT = "Test";

        public override void OnInspectorGUI()
        {
            Launcher _target = (Launcher) target;

            //testT = GUILayout.TextField(testT);
            if (GUILayout.Button("运行"))
            {
                EditorApplication.isPlaying = true;
            }
        }
    }
#endif
}