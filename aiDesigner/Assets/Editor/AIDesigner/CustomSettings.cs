using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace AIDesigner
{
    [CreateAssetMenu(fileName = "AICustomSettings", menuName = "ScriptableObjects/AICustomSettings", order = 1)]
    public class CustomSettings : ScriptableObject
    {
        public UsedForType UsedFor = UsedForType.Battle;
        public string LuaRootPath = "Assets/";
        public string[] DefinePath = new String[0];

        public string BattleConfigPath = "Cfgs/";
        public string BattleEditorConfigPath = "Cfgs/Editor/";
        public string[] BattleTaskPath = new string[2] {"Tasks", "Logic/AIDesigner/Task/"};

        public string SystemConfigPath = "Cfgs/";
        public string SystemEditorConfigPath = "Cfgs/Editor/";
        public string[] SystemTaskPath = new string[2] {"Tasks", "Logic/AIDesigner/Task/"};

        public string AIVarPath = "Logic/AIDesigner/Base/AIVar";
        public string AIDefinePath = "Logic/AIDesigner/Base/AIDefine";
        public string AITreeCenterPath = "Logic/AIDesigner/AITreeMgr";
        
        public string TreeReaderFilePath = "Editor/AIDesigner/Lua/TreeReader.lua";
        public string TreeWriterFilePath = "Editor/AIDesigner/Lua/TreeWriter.lua";
        public string TreeDebugFilePath = "Editor/AIDesigner/Lua/TreeDebug.lua";
        public string OptionReaderFilePath = "Editor/AIDesigner/Lua/OptionReader.lua";

        private string m_AppDataPath;

        public string AppDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_AppDataPath))
                {
                    m_AppDataPath = Path.GetFullPath(Application.dataPath + "/..").Replace('\\', '/');
                }

                return m_AppDataPath;
            }
        }

        public string BattleConfigFullPath
        {
            get => LuaRootPath + BattleConfigPath;
        }

        public string BattleEditorConfigFullPath
        {
            get => LuaRootPath + BattleEditorConfigPath;
        }

        public string SystemConfigFullPath
        {
            get => LuaRootPath + SystemConfigPath;
        }

        public string SystemEditorConfigFullPath
        {
            get => LuaRootPath + SystemEditorConfigPath;
        }
    }

    [CustomEditor(typeof(CustomSettings))]
    public class CustomSettingsEditor : Editor
    {
        public CustomSettings m_settings
        {
            get => target as CustomSettings;
        }

        private SerializedProperty rootPath;
        private SerializedProperty definePath;

        private SerializedProperty battleConfigPath;
        private SerializedProperty battleEditorConfigPath;
        private SerializedProperty battleTaskPath;

        private SerializedProperty systemConfigPath;
        private SerializedProperty systemEditorConfigPath;
        private SerializedProperty systemTaskPath;

        public void OnEnable()
        {
            rootPath = serializedObject.FindProperty("LuaRootPath");
            definePath = serializedObject.FindProperty("DefinePath");

            battleTaskPath = serializedObject.FindProperty("BattleTaskPath");
            battleConfigPath = serializedObject.FindProperty("BattleConfigPath");
            battleEditorConfigPath = serializedObject.FindProperty("BattleEditorConfigPath");

            systemTaskPath = serializedObject.FindProperty("SystemTaskPath");
            systemConfigPath = serializedObject.FindProperty("SystemConfigPath");
            systemEditorConfigPath = serializedObject.FindProperty("SystemEditorConfigPath");
        }

        public override void OnInspectorGUI()
        {
            m_settings.UsedFor = (UsedForType) EditorGUILayout.EnumPopup("Used For", m_settings.UsedFor);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(rootPath, new GUIContent("Root Path"));
            EditorGUILayout.PropertyField(definePath, new GUIContent("Define Path"));
            if (m_settings.UsedFor == UsedForType.Battle)
            {
                EditorGUILayout.PropertyField(battleTaskPath, new GUIContent("Task Path"));
                EditorGUILayout.PropertyField(battleConfigPath, new GUIContent("Config Path"));
                EditorGUILayout.PropertyField(battleEditorConfigPath, new GUIContent("Config Editor Path"));
            }
            else
            {
                EditorGUILayout.PropertyField(systemTaskPath, new GUIContent("Task Path"));
                EditorGUILayout.PropertyField(systemConfigPath, new GUIContent("Config Path"));
                EditorGUILayout.PropertyField(systemEditorConfigPath, new GUIContent("Config Editor Path"));
            }

            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
