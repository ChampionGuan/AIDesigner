using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class AIDesignerReadme : EditorWindow
    {
        public static AIDesignerReadme Instance { get; private set; }

        [MenuItem("AIDesigner/Readme", false, 10)]
        public static void Open()
        {
            Instance?.Close();
            Instance = EditorWindow.GetWindow<AIDesignerReadme>(false, "AIDesigner Readme");
            Instance.wantsMouseMove = true;
            Instance.minSize = new Vector2(500f, 400f);
        }

        public void OnGUI()
        {
            if (null == Instance)
            {
                Open();
            }

            Draw();
        }

        private int m_topBarMenuIndex = 0;
        private string[] m_topBarStrings = new string[4] {"Welcome", "编辑器使用说明", "节点类型介绍", "Lua节点书写说明"};

        public void Draw()
        {
            int index = GUILayout.Toolbar(m_topBarMenuIndex, m_topBarStrings, EditorStyles.toolbarButton);
            if (index != m_topBarMenuIndex)
            {
                m_topBarMenuIndex = index;
            }

            GUILayout.Space(5f);
            switch (m_topBarMenuIndex)
            {
                case 0:
                    DrawWelcome();
                    break;
                case 1:
                    DrawInstructionsForUsingTheEditor();
                    break;
                case 2:
                    DrawIntroductionsForTaskCategory();
                    break;
                case 3:
                    DrawIntroductionsForLuaWriting();
                    break;
            }

            GUILayout.Space(10f);
        }

        private Vector2 m_scrollPosition_Welcome = Vector2.zero;

        private void DrawWelcome()
        {
            GUILayout.Label(@"AI框架及编辑器设计说明", AIDesignerUIUtility.BigHeaderMiddleLabelGUIStyle);
            m_scrollPosition_Welcome = GUILayout.BeginScrollView(m_scrollPosition_Welcome, false, true);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label(@"
                问题描述：
                1、不依赖unity引擎的AI实现，同时服务于客户端与服务器
                2、可以方便的实现热更，快速的修复问题，以及功能实现
                3、可以便捷的实现AI编辑，以及功能调试

                设计思路： 
                1、使用一种树状结构的行为逻辑
                2、使用纯lua的底层实现
                3、树状图结构，及变量的存储使用lua配表
                4、节点参数支持默认值，引用变量及动态变量
                5、节点区分逻辑节点与表现节点，服务器不需要运行表现节点
                6、行为子树(模板树)的存在，及允许树的引用
                7、节点说明，入口节点、动作节点、条件节点、复合节点及装饰节点
                    7.1动作节点（action）:通过某种操作改变游戏或者游戏物体状态的节点，例如物体移动，旋转等
                    7.2条件节点（condition）:设定节点执行的条件
                    7.3复合节点（composite）:是一群子节点的父节点，用来决定子对象的执行顺序
                        7.3.1顺序执行节点（sequence），一个个的顺序执行子节点，当遇到子节点返回失败时，则结束，返回失败；全部成功它就返回成功
                        7.3.2选择执行节点（selector），一个个的顺序执行子节点，当遇到子节点返回成功时，则结束，返回成功；全部失败它就返回失败
                        7.3.3并发执行节点（parallel），顺序执行所有子节点，其中一个子节点返回失败，它就返回失败；全部返回成功它就返回成功
                        7.3.4并发选择执行节点（parallel Selector），顺序执行所有子节点，其中一个子节点返回成功，它就返回成功；全部返回失败它就返回失败
                    7.4装饰节点（decorator）: 用于包裹其他节点的节点，他只能有一个子节点。装饰节点可以改变子节点的行为，例如，装饰节点可以保持子节点持续运行或者改变子节点的返回值。
                8、节点状态，Failure（失败）、Success(成功)、Running(运行中)， Running状态，即阻断状态，可与Conditional Abort配合使用
                9、树结构执行顺序从左到右，且深度优先
                10、行为树的条件终止机制（Conditional Abort），只能由条件节点发起，当条件变化时，发起中断信息，终止一个正在Running的节点。

                方案实现：
                1、编辑器实现
                    1.1参照Behavior Designer 界面样式，进行布局，编辑
                    1.2编辑器使用Unity GUILayout控件实现，编辑器是一个壳的存在，用来编辑行为树，以及节点参数，不参与实际逻辑
                    1.3编辑器支持断点，和逐帧运行，方便调试
                    1.4支持一些快捷指令，比如搜索task，快速定位task，节点禁用，节点展开与自动布局等，根据策划反馈调整
                    1.5支持Task Inspector编辑，Tree Variable 的动态增删查改
                    1.6树状图支持，拖拽，点选，展开，缩放等便捷
                2、AI 实现
                UML类图。 链接：https://www.processon.com/view/link/602fa59fe401fd48f2ae1890

                参考：
                1、Behaviour Designer文档
                链接：https://opsive.com/support/documentation/behavior-designer/overview/");
            }

            GUILayout.EndScrollView();
        }

        private Vector2 m_scrollPosition_ForUsingTheEditor = Vector2.zero;

        private string[] m_title_ForUsingTheEditor =
        {
            "打开编辑器",
            "主面板介绍",
            "Behavior面板介绍",
            "Tasks面板介绍",
            "Variables面板介绍",
            "Inspector面板介绍",
            "CreateTree面板介绍",
            "Help面板介绍",
            "Preferences面板介绍",
            "QuickSearch面板介绍",
            "调试区域介绍",
            "树结构操作介绍",
        };

        private string[] m_instruction_ForUsingTheEditor =
        {
            "Instruction_OpenEditor.jpg",
            "Instruction_MainPanel.jpg",
            "Instruction_BehaviorPanel.jpg",
            "Instruction_TasksPanel.jpg",
            "Instruction_VariablesPanel.jpg",
            "Instruction_InspectorPanel.jpg",
            "Instruction_CreateTreePanel.jpg",
            "Instruction_HelpPanel.jpg",
            "Instruction_PreferencesPanel.jpg",
            "Instruction_QuickSearchPanel.jpg",
            "Instruction_DebugPanel.jpg",
            "Instruction_TreeHandle.jpg",
        };

        private bool[] m_foldout_ForUsingTheEditor =
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
        };

        private void DrawInstructionsForUsingTheEditor()
        {
            m_scrollPosition_ForUsingTheEditor = GUILayout.BeginScrollView(m_scrollPosition_ForUsingTheEditor, false, true);
            for (var i = 0; i < m_instruction_ForUsingTheEditor.Length; i++)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    m_foldout_ForUsingTheEditor[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldout_ForUsingTheEditor[i], m_title_ForUsingTheEditor[i], AIDesignerUIUtility.BigHeaderLabelGUIStyle);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (m_foldout_ForUsingTheEditor[i])
                    {
                        var icon = AIDesignerUIUtility.LoadIcon(m_instruction_ForUsingTheEditor[i]);
                        if (null != icon && GUILayout.Button(icon))
                        {
                            AIDesignerUIUtility.OpenAsset(icon);
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        private Vector2 m_scrollPosition_ForTaskCategory = Vector2.zero;
        private string m_instruction_ForTaskCategory = "Instruction_TaskCategory.jpg";
        private string m_title_ForTaskCategory = "节点类型介绍";
        private bool m_foldout_ForTaskCategory = true;

        private void DrawIntroductionsForTaskCategory()
        {
            m_scrollPosition_ForTaskCategory = GUILayout.BeginScrollView(m_scrollPosition_ForTaskCategory, false, true);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                m_foldout_ForTaskCategory = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldout_ForTaskCategory, m_title_ForTaskCategory, AIDesignerUIUtility.BigHeaderLabelGUIStyle);
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (m_foldout_ForTaskCategory)
                {
                    var icon = AIDesignerUIUtility.LoadIcon(m_instruction_ForTaskCategory);
                    if (null != icon && GUILayout.Button(icon))
                    {
                        AIDesignerUIUtility.OpenAsset(icon);
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        private Vector2 m_scrollPosition_ForTaskWriting = Vector2.zero;
        private string m_instruction_ForTaskWriting = "Instruction_TaskWriting.jpg";
        private string m_title_ForTaskWriting = "节点书写说明";
        private bool m_foldout_ForTaskWriting = true;

        private void DrawIntroductionsForLuaWriting()
        {
            m_scrollPosition_ForTaskWriting = GUILayout.BeginScrollView(m_scrollPosition_ForTaskWriting, false, true);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                m_foldout_ForTaskWriting = EditorGUILayout.BeginFoldoutHeaderGroup(m_foldout_ForTaskWriting, m_title_ForTaskWriting, AIDesignerUIUtility.BigHeaderLabelGUIStyle);
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (m_foldout_ForTaskWriting)
                {
                    var icon = AIDesignerUIUtility.LoadIcon(m_instruction_ForTaskWriting);
                    if (null != icon && GUILayout.Button(icon))
                    {
                        AIDesignerUIUtility.OpenAsset(icon);
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
}