using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AIDesigner
{
    public class AIDesignerWindow : EditorWindow
    {
        public static AIDesignerWindow Instance { get; private set; }

        public float ScreenSizeWidth { get; private set; }
        public float ScreenSizeHeight { get; private set; }
        public bool ScreenSizeChange { get; private set; }

        public event EventHandler LateUpdate;

        [MenuItem("AIDesigner/Editor  &1", false, 0)]
        public static void Open()
        {
            if (null == Define.CustomSettings)
            {
                return;
            }

            Instance?.Close();
            Instance = EditorWindow.GetWindow<AIDesignerWindow>(false, "AIDesigner");
            Instance.wantsMouseMove = true;
            Instance.minSize = new Vector2(700f, 100f);
            Instance.ScreenSizeWidth = -1;
            Instance.ScreenSizeHeight = -1;

            AIDesignerLuaEnv.Instance.Init();
        }

        public static void Open(string fullName)
        {
            Instance?.Close();
            StoragePrefs.SetPref(PrefsType.TreeName, fullName);
            Open();
        }

        public static void Open(UsedForType type)
        {
            Instance?.Close();
            StoragePrefs.SetPref(PrefsType.UsedForType, type.ToString());
            Open();
        }

        public static void Open(UsedForType type, string fullName)
        {
            Instance?.Close();
            StoragePrefs.SetPref(PrefsType.UsedForType, type.ToString());
            Open(fullName);
        }

        public static bool OpenWithPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".lua"))
            {
                return false;
            }

            //Assets/LuaSourceCode/AIDesigner/Config/xxx.lua
            if (assetPath.StartsWith("Assets/"))
            {
                assetPath = assetPath.Substring(7);
            }

            if (assetPath.Contains(Define.CustomSettings.BattleConfigFullPath) || assetPath.Contains(Define.CustomSettings.BattleEditorConfigFullPath))
            {
                var fullName = assetPath.Replace(Define.CustomSettings.BattleEditorConfigFullPath, "").Replace(Define.CustomSettings.BattleConfigFullPath, "");
                if (fullName.Contains("/"))
                {
                    fullName = $"{Path.GetDirectoryName(fullName)}/{Path.GetFileNameWithoutExtension(fullName)}".Replace("\\", "/");
                }
                else
                {
                    fullName = Path.GetFileNameWithoutExtension(fullName);
                }

                Open(UsedForType.Battle, fullName);
                return true;
            }

            if (assetPath.Contains(Define.CustomSettings.SystemConfigFullPath) || assetPath.Contains(Define.CustomSettings.SystemEditorConfigFullPath))
            {
                var fullName = assetPath.Replace(Define.CustomSettings.SystemEditorConfigFullPath, "").Replace(Define.CustomSettings.SystemConfigFullPath, "");
                if (fullName.Contains("/"))
                {
                    fullName = $"{Path.GetDirectoryName(fullName)}/{Path.GetFileNameWithoutExtension(fullName)}".Replace("\\", "/");
                }
                else
                {
                    fullName = Path.GetFileNameWithoutExtension(fullName);
                }

                Open(UsedForType.System, fullName);
                return true;
            }

            return false;
        }

        public void OnDestroy()
        {
            GraphTree.Dispose();
            GraphHelp.Dispose();
            GraphDebug.Dispose();
            GraphTopBar.Dispose();
            GraphCreate.Dispose();
            GraphQuickSearch.Dispose();
            GraphPreferences.Dispose();
            TreeDebug.Dispose();
            TreeChart.Dispose();
            Define.Dispose();
            AIDesignerLuaEnv.Dispose();
        }

        public void OnEnable()
        {
        }

        public void OnFocus()
        {
        }

        public void Update()
        {
            if (!EditorApplication.isPlaying || null == Instance || null == TreeChart.Instance.CurrTree || null == TreeChart.Instance.CurrTree.RuntimeTree)
            {
                return;
            }

            Repaint();
        }

        public void OnGUI()
        {
            if (null == Instance)
            {
                Open();
            }

            var width = position.width;
            var height = position.height + 22f;
            if (ScreenSizeWidth != width || ScreenSizeHeight != height)
            {
                ScreenSizeWidth = width;
                ScreenSizeHeight = height;
                ScreenSizeChange = true;
            }
            else
            {
                ScreenSizeChange = false;
            }

            GraphTree.Instance.OnGUI();
            GraphHelp.Instance.OnGUI();
            GraphDebug.Instance.OnGUI();
            GraphTopBar.Instance.OnGUI();
            GraphCreate.Instance.OnGUI();
            GraphQuickSearch.Instance.OnGUI();
            GraphPreferences.Instance.OnGUI();
            GraphTree.Instance.OnEvent();

            LateUpdate?.Invoke(null, null);
            LateUpdate = null;
        }

        [OnOpenAsset(1)]
        private static bool OpenToEdit(int insID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(insID);
            if (null == obj)
            {
                return false;
            }

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return OpenWithPath(path);
        }
    }
}