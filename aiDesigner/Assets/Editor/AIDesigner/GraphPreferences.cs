using UnityEngine;
using System;
using UnityEditor;

namespace AIDesigner
{
    public class GraphPreferences : Singleton<GraphPreferences>
    {
        public UsedForType UsedForType { get; private set; }
        public KeyCode QuickSearchTaskPanelShortcut { get; private set; }
        public KeyCode QuickLocateToTreeShortcut { get; private set; }
        public float TaskDebugHighlightTime { get; private set; }
        public bool IsAutoSave { get; private set; }
        public bool IsTaskVariableDebug { get; private set; }
        public bool IsTaskDebugStopAtTheLastFrame { get; private set; }
        public bool IsDisplay { get; private set; }
        public bool IsSaveAsJson { get; private set; }

        private Rect m_graphRect;

        public GraphPreferences()
        {
            Refresh();
        }

        private void Refresh()
        {
            IsAutoSave = (bool) StoragePrefs.GetPref(PrefsType.AutoSave);
            IsTaskVariableDebug = (bool) StoragePrefs.GetPref(PrefsType.TaskVariableDebug);
            IsTaskDebugStopAtTheLastFrame = (bool) StoragePrefs.GetPref(PrefsType.TaskDebugStopAtTheLastFrame);
            IsSaveAsJson = (bool) StoragePrefs.GetPref(PrefsType.SaveAsJson);
            TaskDebugHighlightTime = (float) StoragePrefs.GetPref(PrefsType.TaskDebugHighlighting);

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.QuickSearchTaskPanelShortcut), out KeyCode keyCodeS))
            {
                QuickSearchTaskPanelShortcut = keyCodeS;
            }

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.QuickLocateToTreeShortcut), out KeyCode keyCodeL))
            {
                QuickLocateToTreeShortcut = keyCodeL;
            }

            if (Enum.TryParse((string) StoragePrefs.GetPref(PrefsType.UsedForType), out UsedForType forType))
            {
                UsedForType = forType;
            }
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(AIDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, (float) (18 + (EditorGUIUtility.isProSkin ? 1 : 2)), 300f, 35 + 20 * 16);
            }

            if (!IsDisplay)
            {
                return;
            }

            GUILayout.BeginArea(m_graphRect, AIDesignerUIUtility.PreferencesPaneGUIStyle);

            GUILayout.BeginHorizontal();
            GUILayout.Space(m_graphRect.width * 0.5f - 40);
            EditorGUILayout.LabelField("Prefenrences", AIDesignerUIUtility.LabelTitleGUIStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(AIDesignerUIUtility.DeleteButtonTexture, AIDesignerUIUtility.PlainButtonGUIStyle, GUILayout.Width(16)))
            {
                DisplaySwitch();
                return;
            }

            GUILayout.EndHorizontal();

            StoragePrefs.DrawPref(PrefsType.AutoSave, true, "Auto Save", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPref(PrefsType.TaskVariableDebug, true, "Task Variable Debug", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPref(PrefsType.TaskDebugStopAtTheLastFrame, true, "Task Debug Stop At The Last Frame", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPref(PrefsType.SaveAsJson, true, "Save as Json", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);
            
            EditorGUIUtility.labelWidth = 190;

            StoragePrefs.DrawPref(PrefsType.TaskDebugHighlighting, true, "Highlight Time During Debugging", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.QuickLocateToTreeShortcut, true, typeof(KeyCode), "Quick Locate To Tree Shortcut", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.QuickSearchTaskPanelShortcut, true, typeof(KeyCode), "Quick Search Task Shortcut ", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            StoragePrefs.DrawPopupPref(PrefsType.UsedForType, false, typeof(UsedForType), "Used For", PrefChangePreHandler, PrefChangeAftHandler);
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            EditorGUIUtility.labelWidth = 120;

            EditorGUILayout.TextField("Config Path", Define.ConfigFullPath, AIDesignerUIUtility.PathTitleGUIStyle, GUILayout.Height(30));
            EditorGUILayout.TextField("Config Editor Path", Define.EditorConfigFullPath, AIDesignerUIUtility.PathTitleGUIStyle, GUILayout.Height(30));
            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            if (GUILayout.Button("Restore to Defaults"))
            {
                StoragePrefs.Restore();
                Refresh();
            }

            AIDesignerUIUtility.DrawContentSeperator(2);
            GUILayout.Space(5);

            if (GUILayout.Button("Reopen"))
            {
                AIDesignerWindow.Instance.LateUpdate += (sender, e) => { AIDesignerWindow.Open(); };
            }

            GUILayout.EndArea();
        }

        public void DisplaySwitch()
        {
            IsDisplay = !IsDisplay;
            if (IsDisplay)
            {
                if (GraphHelp.Instance.IsDisplay)
                {
                    GraphHelp.Instance.DisplaySwitch();
                }

                if (GraphCreate.Instance.IsDisplay)
                {
                    GraphCreate.Instance.DisplaySwitch();
                }
            }
        }

        private void PrefChangeAftHandler(PrefsType pref, object value)
        {
            switch (pref)
            {
                case PrefsType.AutoSave:
                    IsAutoSave = (bool) value;
                    break;
                case PrefsType.TaskVariableDebug:
                    IsTaskVariableDebug = (bool) value;
                    break;
                case PrefsType.TaskDebugStopAtTheLastFrame:
                    IsTaskDebugStopAtTheLastFrame = (bool) value;
                    break;
                case PrefsType.SaveAsJson:
                    IsSaveAsJson = (bool) value;
                    break;
                case PrefsType.TaskDebugHighlighting:
                    TaskDebugHighlightTime = (float) value;
                    break;
                case PrefsType.QuickSearchTaskPanelShortcut:
                    if (Enum.TryParse((string) value, out KeyCode keyCodeS))
                    {
                        QuickSearchTaskPanelShortcut = keyCodeS;
                    }

                    break;
                case PrefsType.QuickLocateToTreeShortcut:
                    if (Enum.TryParse((string) value, out KeyCode keyCodeL))
                    {
                        QuickLocateToTreeShortcut = keyCodeL;
                    }

                    break;
                case PrefsType.UsedForType:
                    if (Enum.TryParse((string) value, out UsedForType forType))
                    {
                        AIDesignerWindow.Instance.LateUpdate += (sender, e) => { AIDesignerWindow.Open(forType); };
                    }

                    return;
            }
        }

        private void PrefChangePreHandler(PrefsType pref, object value)
        {
            switch (pref)
            {
                default: return;
            }
        }
    }
}