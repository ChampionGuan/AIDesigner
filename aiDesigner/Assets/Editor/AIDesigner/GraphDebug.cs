using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphDebug : Singleton<GraphDebug>
    {
        private Color m_runingColor = new Color(0.3207992f, 0.4932138f, 0.764151f, 1f);

        public Rect m_graphRect { get; private set; }

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        protected override void OnInstance()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected override void OnDispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphRect = new Rect(300, AIDesignerWindow.Instance.ScreenSizeHeight - 18f - 21f, AIDesignerWindow.Instance.ScreenSizeWidth - 300f, 18f);
            }

            GUILayout.BeginArea(m_graphRect, EditorStyles.toolbar);

            GUILayout.BeginHorizontal();
            if (EditorApplication.isPlaying)
            {
                GUI.color = m_runingColor;
            }

            if (GUILayout.Button(AIDesignerUIUtility.PlayTexture, !EditorApplication.isPlaying ? EditorStyles.toolbarButton : AIDesignerUIUtility.ToolbarButtonSelectionGUIStyle, GUILayout.Width(40f)))
            {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }

            GUI.color = Color.white;

            if (EditorApplication.isPaused)
            {
                GUI.color = Color.gray;
            }

            if (GUILayout.Button(AIDesignerUIUtility.PauseTexture, !EditorApplication.isPaused ? EditorStyles.toolbarButton : AIDesignerUIUtility.ToolbarButtonSelectionGUIStyle, GUILayout.Width(40f)))
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }

            GUI.color = Color.white;

            if (!EditorApplication.isPlaying)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button(AIDesignerUIUtility.StepTexture, EditorStyles.toolbarButton, GUILayout.Width(40f)))
            {
                EditorApplication.Step();
            }

            GUI.enabled = true;

            if (EditorApplication.isPlaying)
            {
                if (null != CurrTree && null != CurrTree.RuntimeTree)
                {
                    if (GUILayout.Button(CurrTree.RuntimeTree.Path, EditorStyles.toolbarPopup, GUILayout.Width(140f)))
                    {
                        TreeDebug.Instance.ShowRunningTrees();
                    }
                }
                else
                {
                    if (GUILayout.Button("Runtime Trees", EditorStyles.toolbarPopup, GUILayout.Width(140f)))
                    {
                        TreeDebug.Instance.ShowRunningTrees();
                    }
                }

                if (null != CurrTree && null != CurrTree.RuntimeHost)
                {
                    if (GUILayout.Button(CurrTree.RuntimeHost.name, EditorStyles.toolbarButton, GUILayout.Width(125f)))
                    {
                        Selection.activeGameObject = CurrTree.RuntimeHost;
                    }
                }
                else
                {
                    GUILayout.Button("(None GameObject)", EditorStyles.toolbarButton, GUILayout.Width(125f));
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.ExitingPlayMode && null != CurrTree && CurrTree.IsRuntimeTree)
            {
                TreeDebug.Dispose();
                TreeChart.Instance.LoadTree(CurrTree.FullName, 0, true);
            }
        }
    }
}