using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AIDesigner
{
    public class GraphTree : Singleton<GraphTree>
    {
        private readonly string[] m_topbarStrings = new string[4] {"Behavior", "Tasks", "Variables", "Inspector"};

        private Material m_gridMaterial;
        private Vector2 m_graphScrollSize = new Vector2(20000f, 20000f);

        private Rect m_auxiliaryTaskRect = new Rect(0, 0, Define.TaskWidth, Define.TaskHeight);
        private Rect m_entryTaskRect = new Rect(0, 0, Define.TaskWidth, Define.TaskHeight);
        private Rect m_boxSelectRect = new Rect(0, 0, 0, 0);

        private readonly Vector3[] m_linePoints = new Vector3[2];
        private float m_lineMinY;
        private float m_lineMiddleY;
        private Rect m_lineRectOut;
        private bool m_lineSelected;

        private Rect m_graphStateRect_1;
        private Rect m_graphStateRect_2;
        private Rect m_graphRuntimeRect;
        private Rect m_graphTopBarRect;
        private Rect m_graphDetailRect;
        private Rect m_graphScrollRect;
        private Rect m_graphScrollAreaRect;
        private Vector2 m_graphScrollPosition;
        private Vector2 m_graphScrollOffset;
        private float m_graphScrollZoom = 1f;
        private int m_topbarMenuIndex = 1;
        private float m_runtimeTick = 0;
        private Color m_taskRunningColor = new Color(0f, 0.698f, 0.4f);
        private Color m_taskDiabledColor = new Color(0.7f, 0.7f, 0.7f);
        private Color m_taskLineSelectedColor = new Color(0f, 0.698f, 0.4f);
        private Color m_taskLocateColor = new Color(0.88f, 0.35f, 0.05f);
        private List<TreeTask> m_tasksTemp = new List<TreeTask>();

        public Vector2 CurrMousePos { get; private set; }
        public TreeBehaviour Behaviour { get; private set; }
        public TreeTaskList TaskList { get; private set; }
        public TreeRefVariablesList RefVariablesList { get; private set; }
        public TreeTaskInspector TaskInspector { get; private set; }

        public Vector2 GraphScrollPosition
        {
            get => m_graphScrollPosition;
            private set
            {
                m_graphScrollPosition = value;
                StoragePrefs.SetPref(PrefsType.TreeScrollPos, value);
            }
        }

        public Vector2 GraphScrollOffset
        {
            get => m_graphScrollOffset;
            private set
            {
                m_graphScrollOffset = value;
                StoragePrefs.SetPref(PrefsType.TreeScrollOffset, value);
            }
        }

        public float GraphScrollZoom
        {
            get => m_graphScrollZoom;
            private set
            {
                m_graphScrollZoom = value;
                StoragePrefs.SetPref(PrefsType.TreeScrollZoom, value);
            }
        }

        public int TopbarMenuIndex
        {
            get => m_topbarMenuIndex;
            private set
            {
                m_topbarMenuIndex = value;
                StoragePrefs.SetPref(PrefsType.TreeMenuIndex, value);
            }
        }

        protected TreeStructure CurrTree
        {
            get => TreeChart.Instance.CurrTree;
        }

        protected override void OnInstance()
        {
            TreeReader.Read();
            TreeWriter.Read();
            EditorTaskReader.Read();

            Behaviour = new TreeBehaviour();
            TaskList = new TreeTaskList();
            RefVariablesList = new TreeRefVariablesList();
            TaskInspector = new TreeTaskInspector();

            m_topbarMenuIndex = (int) StoragePrefs.GetPref(PrefsType.TreeMenuIndex);
            m_graphScrollZoom = (float) StoragePrefs.GetPref(PrefsType.TreeScrollZoom);
            m_graphScrollPosition = (Vector2) StoragePrefs.GetPref(PrefsType.TreeScrollPos);
            m_graphScrollOffset = (Vector2) StoragePrefs.GetPref(PrefsType.TreeScrollOffset);
            m_gridMaterial = new Material(Shader.Find("Hidden/AIDesigner/Grid"));
            m_gridMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_gridMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        public void OnGUI()
        {
            if (AIDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphTopBarRect = new Rect(0f, 0f, 300f, 18f);
                m_graphDetailRect = new Rect(0f, m_graphTopBarRect.height, 300f, AIDesignerWindow.Instance.ScreenSizeHeight - m_graphTopBarRect.height - 18);
                m_graphScrollRect = new Rect(300f, 18f, AIDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, AIDesignerWindow.Instance.ScreenSizeHeight - 36f - 21f - 15f);
                m_graphRuntimeRect = new Rect(300f + 5f, 18f + 12f, 15f, 15f);
                m_graphStateRect_1 = new Rect(300f + 5f, 18f + 5f, m_graphDetailRect.width, 30);
                m_graphStateRect_2 = new Rect(300f + 25f, 18f + 5f, m_graphDetailRect.width, 30);
                m_graphScrollAreaRect = new Rect(m_graphScrollRect.xMin + 15f, m_graphScrollRect.yMin + 15f, m_graphScrollRect.width - 30f, m_graphScrollRect.height - 30f);
                if (GraphScrollPosition == Vector2.zero)
                {
                    LocateToTree();
                }
            }

            CurrMousePos = UnityEngine.Event.current.mousePosition;

            m_runtimeTick += Time.deltaTime;
            m_runtimeTick = m_runtimeTick > 2 ? 0 : m_runtimeTick;

            DrawMenuBar();
            DrawChart();
        }

        public void OnEvent()
        {
            UnityEngine.Event currentEvent = UnityEngine.Event.current;
            if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)
            {
                return;
            }

            Vector2 mousePos = Vector2.zero;
            // Debug.Log(Event.current.type + currMousePos.ToString());
            switch (currentEvent.type)
            {
                case EventType.KeyUp:
                    if (currentEvent.control)
                    {
                        if (currentEvent.keyCode == KeyCode.Z && CommandMgr.Instance.UnDo())
                        {
                            currentEvent.Use();
                        }
                        else if (currentEvent.keyCode == KeyCode.Y && CommandMgr.Instance.ReDo())
                        {
                            currentEvent.Use();
                        }
                        else if (currentEvent.keyCode == KeyCode.S)
                        {
                            TreeChart.Instance.CurrTree?.Save();
                        }
                    }
                    else if (currentEvent.alt)
                    {
                    }
                    else if (GraphQuickSearch.Instance.CheckKeyCodeUp())
                    {
                        currentEvent.Use();
                    }
                    else if (VerifyPointInScrollGraph(out mousePos))
                    {
                        if ((Event.current.keyCode == KeyCode.Delete || Event.current.commandName.Equals("Delete")) &&
                            (TreeChart.Instance.DeleteTasks() || TreeChart.Instance.BreakOffTasksRelation()))
                        {
                            currentEvent.Use();
                        }

                        if (Event.current.keyCode == GraphPreferences.Instance.QuickSearchTaskPanelShortcut)
                        {
                            var rect = new Rect(mousePos * GraphScrollZoom + new Vector2(m_graphScrollRect.x, 0f), new Vector2(200f, 200f));
                            if (rect.xMax > m_graphScrollAreaRect.xMax)
                            {
                                rect.x -= (rect.xMax - m_graphScrollAreaRect.xMax);
                            }

                            if (rect.yMax > m_graphScrollAreaRect.yMax)
                            {
                                rect.y -= (rect.yMax - m_graphScrollAreaRect.yMax);
                            }

                            if (rect.yMin < m_graphScrollAreaRect.yMin)
                            {
                                rect.y += (m_graphScrollAreaRect.yMin - rect.yMin);
                            }

                            GraphQuickSearch.Instance.Display(mousePos, rect);
                            currentEvent.Use();
                        }
                        else if (Event.current.keyCode == GraphPreferences.Instance.QuickLocateToTreeShortcut)
                        {
                            LocateToTree();
                            currentEvent.Use();
                        }
                    }

                    break;
                case EventType.MouseDown:
                    GUIUtility.keyboardControl = 0;
                    if (currentEvent.button == 0 && !currentEvent.control)
                    {
                        if (VerifyPointInScrollGraph(out mousePos))
                        {
                            if (TreeChart.Instance.CheckLeftMouseDown(mousePos, GraphScrollPosition))
                            {
                                currentEvent.Use();
                            }

                            if (GraphQuickSearch.Instance.CheckLeftMouseDown(CurrMousePos))
                            {
                                currentEvent.Use();
                            }
                        }
                        else if (TopbarMenuIndex == 2 && VerifyPointInDetailGraph(out mousePos) && RefVariablesList.CheckLeftMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                    }
                    else if (currentEvent.button == 1)
                    {
                        if (VerifyPointInScrollGraph(out mousePos) && TreeChart.Instance.CheckRightMouseDown(mousePos, GraphScrollPosition))
                        {
                            currentEvent.Use();
                        }
                        else if (TopbarMenuIndex == 2 && VerifyPointInDetailGraph(out mousePos) && RefVariablesList.CheckRightMouseDown(mousePos))
                        {
                            currentEvent.Use();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (currentEvent.button == 0 && !currentEvent.control)
                    {
                        if (TreeChart.Instance.CheckLeftMouseUp(VerifyPointInScrollGraph(out mousePos), mousePos))
                        {
                            currentEvent.Use();
                        }
                    }
                    else if (currentEvent.button == 1)
                    {
                        if (TreeChart.Instance.CheckRightMouseUp(VerifyPointInScrollGraph(out mousePos), mousePos))
                        {
                            currentEvent.Use();
                        }
                    }

                    break;
                case EventType.MouseDrag:
                    if (VerifyPointInScrollGraph(out mousePos))
                    {
                        if (currentEvent.button == 0)
                        {
                            if (TreeChart.Instance.CheckLeftMouseDrag(mousePos, currentEvent.delta / GraphScrollZoom))
                            {
                                currentEvent.Use();
                            }
                        }
                        else if (currentEvent.button == 2)
                        {
                            if (ScrollTreeGraph(currentEvent.delta))
                            {
                                currentEvent.Use();
                            }
                        }
                    }

                    break;
                case EventType.MouseMove:
                    if (VerifyPointInScrollGraph(out mousePos) && TreeChart.Instance.CheckLeftMouseMove(mousePos, GraphScrollPosition))
                    {
                        currentEvent.Use();
                    }

                    break;
                case EventType.ScrollWheel:
                    if (VerifyPointInScrollGraph(out mousePos) && ZoomTreeGraph(mousePos))
                    {
                        currentEvent.Use();
                    }

                    break;
                case EventType.Ignore:
                    if (TreeChart.Instance.CheckIgnore(mousePos))
                    {
                        currentEvent.Use();
                    }

                    break;
                case EventType.ValidateCommand:
                    if (currentEvent.commandName.Equals("Copy") || currentEvent.commandName.Equals("Paste"))
                    {
                        currentEvent.Use();
                    }

                    break;
                case EventType.ExecuteCommand:
                    if (GUIUtility.keyboardControl == 0)
                    {
                        if (Event.current.commandName.Equals("Copy") && TreeChart.Instance.CopyTasks())
                        {
                            currentEvent.Use();
                        }
                        else if (Event.current.commandName.Equals("Paste") && TreeChart.Instance.PasteTasks(false))
                        {
                            currentEvent.Use();
                        }
                    }

                    break;
                default: break;
            }

            if (currentEvent.type != EventType.KeyUp)
            {
                if (VerifyPointInScrollGraph(out _) && ScrollTreeGraph())
                {
                    currentEvent.Use();
                }
            }
        }

        public void LocateToTree()
        {
            GraphScrollZoom = 1;
            GraphScrollOffset = Vector2.zero;
            GraphScrollPosition = (m_graphScrollSize - (m_graphScrollRect.size - Vector2.one * Define.TaskWidth * 0.5f)) * 0.5f;
            CurrTree?.Entry?.SetOffset(Vector2.zero, false);
        }

        public void SetMenuIndex(int index)
        {
            GUI.FocusControl(null);
            TopbarMenuIndex = index;
        }

        private void DrawMenuBar()
        {
            GUILayout.BeginArea(m_graphTopBarRect, EditorStyles.toolbar);
            var index = GUILayout.Toolbar(TopbarMenuIndex, m_topbarStrings, EditorStyles.toolbarButton);
            if (index != TopbarMenuIndex)
            {
                CommandMgr.Instance.Do<CommandTreeMenuIndex>(TopbarMenuIndex, index);
            }

            GUILayout.EndArea();

            GUILayout.BeginArea(m_graphDetailRect, AIDesignerUIUtility.PropertyBoxGUIStyle);
            if (TopbarMenuIndex == 0)
            {
                GUILayout.Space(5f);
                Behaviour.Draw();
            }
            else if (TopbarMenuIndex == 1)
            {
                TaskList.Draw();
            }
            else if (TopbarMenuIndex == 2)
            {
                GUILayout.Space(5f);
                RefVariablesList.Draw();
            }
            else if (TopbarMenuIndex == 3)
            {
                GUILayout.Space(5f);
                TaskInspector.Draw();
            }

            GUILayout.EndArea();
        }

        private void DrawChart()
        {
            if (UnityEngine.Event.current.type != EventType.ScrollWheel)
            {
                var pos = GUI.BeginScrollView(new Rect(m_graphScrollRect.x, m_graphScrollRect.y, m_graphScrollRect.width + 15f, m_graphScrollRect.height + 15f), GraphScrollPosition, new Rect(0f, 0f, m_graphScrollSize.x, m_graphScrollSize.y), true, true);
                if (pos != GraphScrollPosition && UnityEngine.Event.current.type != EventType.DragUpdated && UnityEngine.Event.current.type != EventType.Ignore)
                {
                    GraphScrollOffset -= (pos - GraphScrollPosition) / GraphScrollZoom;
                    GraphScrollPosition = pos;
                }

                GUI.EndScrollView();
            }

            GUI.Box(m_graphScrollRect, string.Empty, AIDesignerUIUtility.GraphBackgroundGUIStyle);
            DrawGridBG();
            DrawTreeState();
            DrawTaskDesc();

            EditorZoomArea.Begin(m_graphScrollRect, GraphScrollZoom);
            DrawChart(VerifyPointInScrollGraph(out Vector2 mousePos), mousePos, GraphScrollPosition);
            EditorZoomArea.End();
        }

        private void DrawGridBG()
        {
            if (UnityEngine.Event.current.type == EventType.Repaint)
            {
                m_gridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 1 : 0);
                GL.PushMatrix();
                GL.Begin(1);
                DrawGridLines(Define.MeshSize * GraphScrollZoom, new Vector2(GraphScrollOffset.x % Define.MeshSize * GraphScrollZoom, GraphScrollOffset.y % Define.MeshSize * GraphScrollZoom));
                GL.End();
                GL.PopMatrix();

                m_gridMaterial.SetPass((!EditorGUIUtility.isProSkin) ? 3 : 2);
                GL.PushMatrix();
                GL.Begin(1);
                DrawGridLines((Define.MeshSize * 5) * GraphScrollZoom, new Vector2(GraphScrollOffset.x % (Define.MeshSize * 5) * GraphScrollZoom, GraphScrollOffset.y % (Define.MeshSize * 5) * GraphScrollZoom));
                GL.End();
                GL.PopMatrix();
            }
        }

        private void DrawGridLines(float gridSize, Vector2 offset)
        {
            var num = m_graphScrollRect.x + offset.x;
            if (offset.x < 0f)
            {
                num += gridSize;
            }

            for (var num2 = num; num2 < m_graphScrollRect.x + m_graphScrollRect.width; num2 += gridSize)
            {
                DrawGridLine(new Vector2(num2, m_graphScrollRect.y), new Vector2(num2, m_graphScrollRect.y + m_graphScrollRect.height));
            }

            var num3 = m_graphScrollRect.y + offset.y;
            if (offset.y < 0f)
            {
                num3 += gridSize;
            }

            for (var num4 = num3; num4 < m_graphScrollRect.y + m_graphScrollRect.height; num4 += gridSize)
            {
                DrawGridLine(new Vector2(m_graphScrollRect.x, num4), new Vector2(m_graphScrollRect.x + m_graphScrollRect.width, num4));
            }
        }

        private void DrawGridLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        private void DrawTreeState()
        {
            string state;
            if (null == CurrTree)
            {
                state = "Right Click, Create a Tree";
            }
            else if (null == CurrTree.Entry)
            {
                state = "Add a Task";
            }
            else
            {
                state = CurrTree.ShortName;
                if (CurrTree.IsRuntimeTree)
                {
                    if (!CurrTree.RuntimeTree.IsStart)
                    {
                        state += "  - Disabled";
                    }
                    else if (CurrTree.RuntimeTree.IsPaused)
                    {
                        state += "  - Paused";
                    }
                    else
                    {
                        state += "  - Running";
                    }
                }
            }

            if (!state.Equals(string.Empty))
            {
                var rect = m_graphStateRect_1;
                if (null != CurrTree && null != CurrTree.RuntimeTree)
                {
                    if (m_runtimeTick > 1 || EditorApplication.isPaused)
                    {
                        GUI.DrawTexture(m_graphRuntimeRect, AIDesignerUIUtility.BreakpointTexture, ScaleMode.ScaleToFit);
                    }

                    rect = m_graphStateRect_2;
                }

                GUI.Label(rect, state, AIDesignerUIUtility.GraphStatusGUIStyle);
            }
        }

        private void DrawTaskDesc()
        {
            if (null == TreeChart.Instance.CurrTaskByClick)
            {
                return;
            }

            if (string.IsNullOrEmpty(TreeChart.Instance.CurrTaskByClick.Desc))
            {
                return;
            }

            var uiContent = new GUIContent(TreeChart.Instance.CurrTaskByClick.Desc);
            AIDesignerUIUtility.TaskCommentGUIStyle.CalcMinMaxWidth(uiContent, out _, out var width);
            var num3 = Mathf.Min(400f, width + 20f);
            var num4 = Mathf.Min(300f, AIDesignerUIUtility.TaskCommentGUIStyle.CalcHeight(uiContent, num3)) + 3f;
            GUI.Box(new Rect(m_graphScrollRect.x + 5f, m_graphScrollRect.yMax - num4 - 5f, num3, num4), string.Empty, AIDesignerUIUtility.TaskDescriptionGUIStyle);
            GUI.Box(new Rect(m_graphScrollRect.x + 7f, m_graphScrollRect.yMax - num4 - 5f, num3, num4), TreeChart.Instance.CurrTaskByClick.Desc, AIDesignerUIUtility.TaskCommentGUIStyle);
        }

        public void DrawChart(bool inArea, Vector2 mousePos, Vector2 treeScrollPos)
        {
            if (null != CurrTree && null != CurrTree.Entry)
            {
                m_entryTaskRect.x = m_graphScrollRect.width * 0.5f - m_entryTaskRect.width * 0.5f;
                m_entryTaskRect.y = m_graphScrollRect.height * 0.1f;
                m_entryTaskRect.x += GraphScrollOffset.x;
                m_entryTaskRect.y += GraphScrollOffset.y;
                CurrTree.Entry.SetRect(m_entryTaskRect);

                m_auxiliaryTaskRect.x = m_entryTaskRect.x;
                m_auxiliaryTaskRect.y = m_entryTaskRect.y + Define.TaskHeight * 2;
                CurrTree.SetAuxiliaryTreeRect(m_auxiliaryTaskRect);

                DrawMousePos(mousePos);
                DrawTask(CurrTree.Entry);
                foreach (var task in CurrTree.AuxiliaryTrees)
                {
                    DrawTask(task);
                }
            }

            if (TreeChart.Instance.BoxesSelecting)
            {
                if (inArea)
                {
                    m_boxSelectRect = TreeChart.Instance.GetBoxesArea(mousePos, treeScrollPos);
                }

                GUI.Box(m_boxSelectRect, string.Empty, AIDesignerUIUtility.SelectionGUIStyle);
            }
        }

        private void DrawPolyLine(Vector2 start, Vector2 end, Color color)
        {
            Handles.color = color;
            m_linePoints[0] = start;
            m_linePoints[1] = end;
            Handles.DrawPolyLine(m_linePoints);
            Handles.color = Color.white;
        }

        private void DrawRectLine(Rect rect, Color color)
        {
            var half = 0f;
            var start = Vector2.zero;
            var end = Vector2.zero;
            if (rect.width > rect.height)
            {
                half = rect.height * 0.5f;
                start.x = rect.xMin + half;
                start.y = rect.yMin + half;
                end.x = rect.xMax - half;
                end.y = start.y;
            }
            else
            {
                half = rect.width * 0.5f;
                start.x = rect.xMin + half;
                start.y = rect.yMin + half;
                end.x = start.x;
                end.y = rect.yMax - half;
            }

            DrawPolyLine(start, end, color);
        }

        private void DrawMousePos(Vector2 pos)
        {
            if (null == TreeChart.Instance.CurrTaskByInArea && null == TreeChart.Instance.CurrTaskByOutArea)
            {
                return;
            }

            var start = null == TreeChart.Instance.CurrTaskByInArea ? TreeChart.Instance.CurrTaskByOutArea.TaskRect.center : TreeChart.Instance.CurrTaskByInArea.TaskRect.center;
            m_lineMiddleY = Mathf.Lerp(pos.y, start.y, 0.5f);
            DrawPolyLine(start, new Vector2(start.x, m_lineMiddleY), Color.white);
            DrawPolyLine(new Vector2(start.x, m_lineMiddleY), new Vector2(pos.x, m_lineMiddleY), Color.white);
            DrawPolyLine(new Vector2(pos.x, m_lineMiddleY), pos, Color.white);
        }

        private void DrawTask(TreeTask task)
        {
            DrawTaskConnections(task, task.IsDisabled);
            DrawTaskContents(task, task.IsDisabled);
            DrawTaskFinal(task);
        }

        private void DrawTaskConnections(TreeTask task, bool disabled)
        {
            if (null == task.Children || task.Children.Count <= 0 || (!task.IsFoldout && !GraphHelp.Instance.AllTasksFoldout))
            {
                return;
            }

            m_lineMinY = task.TaskRect.center.y + 99999;
            foreach (var v in task.Children)
            {
                if (v.TaskRect.center.y <= task.TaskRect.center.y + task.TaskRect.height * 1.3f)
                {
                    m_lineMinY = task.TaskRect.center.y + task.TaskRect.height * 1.6f;
                    break;
                }

                if (v.TaskRect.center.y < m_lineMinY)
                {
                    m_lineMinY = v.TaskRect.center.y + 3;
                }
            }

            m_lineMiddleY = Mathf.Lerp(task.TaskRect.center.y, m_lineMinY, 0.5f);
            foreach (var child in task.Children)
            {
                child.LinesRect[0].width = 4f;
                child.LinesRect[0].height = child.InRect.center.y > m_lineMiddleY ? Mathf.Abs(m_lineMiddleY - 2 - child.InRect.yMin) + 4f : Mathf.Abs(m_lineMiddleY - 2 - child.InRect.yMin) - 2;
                child.LinesRect[0].xMin = child.InRect.center.x - 2f;
                child.LinesRect[0].yMin = child.InRect.center.y > m_lineMiddleY ? m_lineMiddleY - 2f : child.InRect.center.y;

                child.LinesRect[1].width = 4f;
                child.LinesRect[1].height = Mathf.Abs(m_lineMiddleY - task.OutRect.yMax) + 7f;
                child.LinesRect[1].xMin = task.OutRect.center.x - 2f;
                child.LinesRect[1].yMin = task.OutRect.yMax - 5f;

                child.LinesRect[2].width = Mathf.Abs(child.InRect.center.x - task.OutRect.center.x) + 4f;
                child.LinesRect[2].height = 4f;
                child.LinesRect[2].xMin = task.OutRect.center.x > child.InRect.center.x ? child.InRect.center.x - 2f : task.OutRect.center.x - 2f;
                child.LinesRect[2].yMin = m_lineMiddleY - 2f;

                var color = Color.Lerp(disabled || child.IsDisabled ? m_taskDiabledColor : Color.white, m_taskRunningColor, child.StateTickValue);
                DrawRectLine(child.LinesRect[0], color);
                DrawRectLine(child.LinesRect[2], color);
                m_lineRectOut = child.LinesRect[1];
            }

            m_lineSelected = false;
            foreach (var child in task.Children)
            {
                if (TreeChart.Instance.CurrTasksByLines.Contains(child))
                {
                    m_lineSelected = true;
                    DrawRectLine(child.LinesRect[0], m_taskLineSelectedColor);
                    DrawRectLine(child.LinesRect[2], m_taskLineSelectedColor);
                }
            }

            var taskColor = m_lineSelected ? m_taskLineSelectedColor : Color.Lerp((disabled || task.IsDisabled) ? m_taskDiabledColor : Color.white, m_taskRunningColor, task.StateTickValue);
            DrawRectLine(m_lineRectOut, taskColor);

            foreach (var child in task.Children)
            {
                DrawTaskConnections(child, disabled || child.IsDisabled);
            }
        }

        private void DrawTaskContents(TreeTask task, bool disabled)
        {
            GUIStyle bgStyle = null;
            Texture2D state = null;
            Texture2D inTexture = null;
            Texture2D outTexture = null;
            var stateType = CurrTree.GetRuntimeTaskState(task.DebugID);
            if (stateType != TaskStateType.None)
            {
                task.StateTickValue = 1;
                task.StateType = stateType;
            }

            switch (task.StateType)
            {
                case TaskStateType.None:
                    break;
                case TaskStateType.Success:
                    state = AIDesignerUIUtility.ExecutionSuccessTexture;
                    break;
                case TaskStateType.Failure:
                    state = AIDesignerUIUtility.ExecutionFailureTexture;
                    break;
                case TaskStateType.Running:
                    state = AIDesignerUIUtility.ExecutionSuccessRepeatTexture;
                    break;
            }

            if (task.StateTickValue < 1)
            {
                if (task.InType == TaskInType.Yes)
                {
                    inTexture = AIDesignerUIUtility.GetTaskConnectionTopTexture(0);
                }

                if (task.OutType != TaskOutType.No || (null != task.Children && task.Children.Count > 0))
                {
                    outTexture = AIDesignerUIUtility.GetTaskConnectionBottomTexture(0);
                }

                if (IsLocateTask(task))
                {
                    bgStyle = TreeChart.Instance.CurrTaskByClick == task || TreeChart.Instance.CurrTasksByBoxes.Contains(task) ? AIDesignerUIUtility.TaskIdentifySelectedCompactGUIStyle : AIDesignerUIUtility.TaskIdentifyCompactGUIStyle;
                }
                else
                {
                    bgStyle = TreeChart.Instance.CurrTaskByClick == task || TreeChart.Instance.CurrTasksByBoxes.Contains(task) ? AIDesignerUIUtility.GetTaskSelectedGUIStyle(task.ColorIndex) : AIDesignerUIUtility.GetTaskGUIStyle(task.ColorIndex);
                }

                GUI.color = (disabled || task.IsDisabled) ? m_taskDiabledColor : Color.white;
                DrawTaskBg(task, inTexture, outTexture, state, bgStyle);
                GUI.color = Color.white;
            }

            if (null != CurrTree.RuntimeTree && task.StateTickValue > 0)
            {
                if (task.InType == TaskInType.Yes)
                {
                    inTexture = AIDesignerUIUtility.TaskConnectionRunningTopTexture;
                }

                if (task.OutType != TaskOutType.No || (null != task.Children && task.Children.Count > 0))
                {
                    outTexture = AIDesignerUIUtility.TaskConnectionRunningBottomTexture;
                }

                bgStyle = TreeChart.Instance.CurrTaskByClick == task || TreeChart.Instance.CurrTasksByBoxes.Contains(task) ? AIDesignerUIUtility.TaskRunningSelectedGUIStyle : AIDesignerUIUtility.TaskRunningGUIStyle;

                var color = Color.white;
                color.a = task.StateTickValue;
                GUI.color = color;
                DrawTaskBg(task, inTexture, outTexture, state, bgStyle);
                GUI.color = Color.white;
            }

            if (TreeChart.Instance.CurrTaskBySuspend == task)
            {
                GUI.DrawTexture(task.DisabledBtnRect, !task.IsDisabled ? AIDesignerUIUtility.DisableTaskTexture : AIDesignerUIUtility.EnableTaskTexture, ScaleMode.ScaleToFit);
            }

            if (task.OutType != TaskOutType.No || (null != task.Children && task.Children.Count > 0))
            {
                if (TreeChart.Instance.CurrTaskBySuspend == task)
                {
                    GUI.DrawTexture(task.ExpandedBtnRect, !task.IsFoldout ? AIDesignerUIUtility.ExpandTaskTexture : AIDesignerUIUtility.CollapseTaskTexture, ScaleMode.ScaleToFit);
                }

                if (!task.IsFoldout)
                {
                    GUI.DrawTexture(task.ExpandedIconRect, AIDesignerUIUtility.TaskConnectionCollapsedTexture);
                }
            }

            if (!string.IsNullOrEmpty(task.Comment))
            {
                GUI.Box(task.CommentRect, string.Empty, AIDesignerUIUtility.TaskDescriptionGUIStyle);
                GUI.Label(task.CommentRect, task.Comment, AIDesignerUIUtility.TaskCommentGUIStyle);
            }

            if (task.Type == TaskType.Composite)
            {
                switch (task.AbortType)
                {
                    case AbortType.Self:
                        GUI.DrawTexture(task.AbortTypeRect, AIDesignerUIUtility.ConditionalAbortSelfTexture);
                        break;
                    case AbortType.LowerPriority:
                        GUI.DrawTexture(task.AbortTypeRect, AIDesignerUIUtility.ConditionalAbortLowerPriorityTexture);
                        break;
                    case AbortType.Both:
                        GUI.DrawTexture(task.AbortTypeRect, AIDesignerUIUtility.ConditionalAbortBothTexture);
                        break;
                }
            }

            if (null != CurrTree.RuntimeTree && task.IsBreakpoint && stateType != TaskStateType.None)
            {
                EditorApplication.isPaused = true;
            }

            if (task.IsErrorTask
                || ((task.Type == TaskType.Composite || task.Type == TaskType.Decorator || task.Type == TaskType.Entry) && (null == task.Children || task.Children.Count <= 0)))
            {
                GUI.DrawTexture(task.ErrorRect, AIDesignerUIUtility.ErrorIconTexture);
            }

            if (task.IsBreakpoint)
            {
                GUI.DrawTexture(task.BreakpointRect, AIDesignerUIUtility.BreakpointTexture);
            }

            if (null != task.Icon)
            {
                GUI.DrawTexture(task.IconRect, task.Icon);
            }

            if (!EditorApplication.isPaused)
            {
                task.StateTickValue -= 0.5f * Time.deltaTime * (1 / (GraphPreferences.Instance.TaskDebugHighlightTime > 0 ? GraphPreferences.Instance.TaskDebugHighlightTime : 1f));
            }

            if (null == CurrTree.RuntimeTree || task.StateTickValue <= 0)
            {
                task.StateTickValue = 0;
                task.StateType = TaskStateType.None;
            }

            if (null == task.Children || (!task.IsFoldout && !GraphHelp.Instance.AllTasksFoldout))
            {
                return;
            }

            foreach (var child in task.Children)
            {
                DrawTaskContents(child, disabled || child.IsDisabled);
            }
        }

        private void DrawTaskBg(TreeTask task, Texture2D inTexture, Texture2D outTexture, Texture2D state, GUIStyle bg)
        {
            if (null != inTexture)
            {
                GUI.DrawTexture(task.InRect, inTexture);
            }

            if (null != outTexture)
            {
                GUI.DrawTexture(task.OutRect, outTexture);
            }

            if (null != bg)
            {
                GUI.Label(task.TaskRect, task.Name, bg);
            }

            if (null != state)
            {
                GUI.DrawTexture(task.StateRect, state);
            }
        }

        private void DrawTaskFinal(TreeTask task)
        {
            var isRunning = CurrTree.GetRuntimeTaskState(task.DebugID) != TaskStateType.None;
            if (isRunning || IsLocateTask(task))
            {
                m_tasksTemp.Clear();

                var taskN = task;
                var parent = task.Parent;
                while (null != parent && CurrTree.GetRuntimeTaskState(parent.DebugID) == TaskStateType.None)
                {
                    if (!parent.IsFoldout)
                    {
                        m_tasksTemp.Clear();
                    }
                    else
                    {
                        m_tasksTemp.Add(taskN);
                    }

                    taskN = parent;
                    parent = parent.Parent;
                }

                if (m_tasksTemp.Count > 0)
                {
                    var color = isRunning ? m_taskRunningColor : m_taskLocateColor;
                    foreach (var temp in m_tasksTemp)
                    {
                        DrawRectLine(temp.LinesRect[0], color);
                        DrawRectLine(temp.LinesRect[1], color);
                        DrawRectLine(temp.LinesRect[2], color);
                    }
                }
            }

            if (null == task.Children)
            {
                return;
            }

            foreach (var child in task.Children)
            {
                DrawTaskFinal(child);
            }
        }

        private bool IsLocateTask(TreeTask task)
        {
            return !string.IsNullOrEmpty(GraphHelp.Instance.FindTaskName) && GraphHelp.Instance.FindTaskName.Length > 2 && task.Name.ToLower().Contains(GraphHelp.Instance.FindTaskName.ToLower()) ||
                   !string.IsNullOrEmpty(GraphHelp.Instance.FindVariableName) && task.ContainSharedVariableKey(GraphHelp.Instance.FindVariableName);
        }

        private bool ZoomTreeGraph(Vector2 zoomPos)
        {
            GraphScrollZoom += (-UnityEngine.Event.current.delta.y / 150f);
            GraphScrollZoom = Mathf.Clamp(GraphScrollZoom, 0.4f, 1.4f);

            VerifyPointInScrollGraph(out var point);
            GraphScrollOffset += point - zoomPos;
            GraphScrollPosition += point - zoomPos;
            return true;
        }

        private bool ScrollTreeGraph(Vector2 delta)
        {
            GraphScrollOffset += delta / GraphScrollZoom;
            GraphScrollPosition -= delta;
            return true;
        }

        private bool ScrollTreeGraph()
        {
            var mousePos = CurrMousePos;
            if (m_graphScrollAreaRect.Contains(mousePos))
            {
                return false;
            }

            if (!TreeChart.Instance.LeftMouseDowning)
            {
                return false;
            }

            var result = false;
            var offset = Vector2.zero;
            if (mousePos.y < m_graphScrollAreaRect.yMin + 15f)
            {
                offset.y = 5f;
                result = true;
            }
            else if (mousePos.y > m_graphScrollAreaRect.yMax - 15f)
            {
                offset.y = -5f;
                result = true;
            }

            if (mousePos.x < m_graphScrollAreaRect.xMin + 15f)
            {
                offset.x = 5f;
                result = true;
            }
            else if (mousePos.x > m_graphScrollAreaRect.xMax - 15f)
            {
                offset.x = -5f;
                result = true;
            }

            if (result) ScrollTreeGraph(offset);
            return result;
        }

        private bool VerifyPointInDetailGraph(out Vector2 point)
        {
            point = CurrMousePos;
            if (!m_graphDetailRect.Contains(point))
            {
                return false;
            }

            point.x -= m_graphDetailRect.xMin;
            point.y -= m_graphDetailRect.yMin;
            return true;
        }

        private bool VerifyPointInScrollGraph(out Vector2 point)
        {
            point = CurrMousePos;
            if (!m_graphScrollRect.Contains(point))
            {
                return false;
            }

            point -= new Vector2(m_graphScrollRect.xMin, m_graphScrollRect.yMin);
            point /= GraphScrollZoom;
            return true;
        }
    }
}