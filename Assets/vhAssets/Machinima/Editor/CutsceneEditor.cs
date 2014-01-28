using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class CutsceneEditor : TimelineWindow
{
    #region Constants
    //  general
    const string MachinimaDataFileName = "MachinimaData.xml";
    const string CutscenePrefabName = "CutscenePrefab";
    const string AssetsPath = "/Assets";
    const string ImagePath = "Assets/vhAssets/Machinima/Gizmos/";
    const string GenericEventsGOPath = "Prefabs/GenericEvents.prefab";
    static string[] DefaultFunctionExclusions = { "Update", "FixedUpdate", "LateUpdate", "Start", "Awake", "OnEnable", "OnDisable",
                                                    "OnDrawGizmos", "OnDrawGizmosSelected", "Main", "Update", "FixedUpdate", "LateUpdate",
                                                    "Awake", "OnEnable", "OnDisable", "SendMessage", "SendMessageUpwards", "BroadcastMessage",
                                                    "CancelInvoke", "Invoke", "InvokeRepeating", "StopCoroutine", "StopAllCoroutines", "set_useGUILayout",
                                                    "set_enabled", "set_active", "set_tag", "set_name", "set_hideFlags" };

    enum DataView
    {
        Cutscene,
        Event,
    }

    public enum ViewTypes
    {
        //Sceneline,
        Cutscene
    }

    //string[] ViewNames = { /*"Scene View",*/ "Cutscene View" };
    string[] DataViewNames = { "Cutscene", "Event" };

    class ScenelineViewData
    {
        public float m_FirstStartTime = 0;
        public float m_LongestLength = 0;
        public float m_LastEndTime = 0;
        public List<CutsceneTrackGroup> m_TrackGroups = new List<CutsceneTrackGroup>();
    }

    enum CleanRequest
    {
        None,
        New,
        OpenBml,
        OpenXml,
        OpenBoth
    }

    public class SequencerData
    {
        public string m_XmlTitle = "";
        public List<BMLData> m_BMLData = new List<BMLData>();
    }
    #endregion

    #region Variables
    bool m_JustFinishedPlaying = false;
    CutsceneIO m_IO = new CutsceneIO();
    UnitySequencerIO m_SequencerIO;
    //ScenelineViewData m_ScenelineViewData = new ScenelineViewData();

    // toolbar
    ViewTypes m_SelectedView = ViewTypes.Cutscene;

    // cutscene view
    DataView m_SelectedDataViewTab;

    // event data
    List<string> m_EventComponentNames = new List<string>();
    int m_SelectedEventComponent;
    List<string> m_EventFunctionNames = new List<string>();
    int m_SelectedEventFunction;
    List<string> m_EventSelectedFunctionOverloadNames = new List<string>();
    List<MethodInfo> m_MethodInfos = new List<MethodInfo>();
    List<string> m_EventLengthOptions = new List<string>();
    int m_SelectedEventLengthOption = 0;
    List<string> m_EventTypes = new List<string>();
    public EditorTimelineObjectManager m_TimeObjectManager = new EditorCutsceneManager();
    bool m_IsFastForwarding;
    bool m_ListenToNVBG;
    bool m_CreateBMLEvents = true;
    CleanRequest m_CleanRequestState = CleanRequest.None;
    SequencerData m_SequencerData = new SequencerData();
    #endregion

    #region Properties
    override public string SavedWindowPosXKey { get { return "CutsceneEditorWindowX"; } }
    override public string SavedWindowPosYKey { get { return "CutsceneEditorWindowY"; } }
    override public string SavedWindowWKey { get { return "CutsceneEditorWindowW"; } }
    override public string SavedWindowHKey { get { return "CutsceneEditorWindowH"; } }

    override public float StartTime
    {
        get { return GetSelectedCutscene().StartTime; }
        set { GetSelectedCutscene().StartTime = value; }
    }

    override public float EndTime
    {
        get { return StartTime + Length; }
    }

    override public float Length
    {
        get { return GetSelectedCutscene().Length; }
        set { GetSelectedCutscene().Length = value; }
    }

    override public CutsceneTrackGroupManager GroupManager { get { return GetSelectedCutscene().GroupManager; } }

    public override CutsceneTrackItem SelectedEvent
    {
        get { return GetSelectedCutscene().GetSelectedEvent(); }
    }

    public override List<CutsceneTrackItem> SelectedEvents
    {
        get { return GetSelectedCutscene().GetSelectedEvents(); }
    }

    public override int NumSelectedEvents
    {
        get { return GetSelectedCutscene().GetSelectedEvents().Count; }
    }

    int NumCutscenes
    {
        get { return m_TimeObjectManager.NumObjects; }
    }

    public ViewTypes SelectedView
    {
        get { return m_SelectedView; }
    }

    public override EditorTimelineObjectManager TimelineObjectManager
    {
        get { return m_TimeObjectManager; }
    }

    public bool IsFastForwarding
    {
        get { return m_IsFastForwarding; }
        set { m_IsFastForwarding = value; }
    }

    public UnitySequencerIO SequencerIO
    {
        get { return m_SequencerIO; }
        set { m_SequencerIO = value; }
    }
    #endregion

    #region Functions
    [MenuItem("VH/Machinima Maker")]
    static void Init()
    {
        CutsceneEditor window = (CutsceneEditor)EditorWindow.GetWindow(typeof(CutsceneEditor));
        window.position = new Rect(PlayerPrefs.GetFloat(window.SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(window.SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(window.SavedWindowWKey, 1429),
            PlayerPrefs.GetFloat(window.SavedWindowHKey, 523));
        window.autoRepaintOnSceneChange = true;
        window.wantsMouseMove = true;
        window.title = "Machinima";

        window.m_TimeObjectManager = new EditorCutsceneManager();

        string[] dirs = Directory.GetDirectories(Application.dataPath, "Machinima", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i].Contains("Machinima"))
            {
                window.MachinimaFolderLocation = dirs[i];
                window.MachinimaFolderLocation = window.MachinimaFolderLocation.Replace('\\', '/');

                int index = window.MachinimaFolderLocation.IndexOf(AssetsPath);
                if (index != -1)
                {
                    window.MachinimaFolderLocation = window.MachinimaFolderLocation.Remove(0, index + 1);
                }
                window.MachinimaFolderLocation += '/';
                break;
            }
        }

        window.SequencerIO = new UnitySequencerIO(window);

        window.minSize = new Vector2(800, 250);

        window.LoadTextures();

        window.Show();
        window.Setup();

        Cutscene selectedCutscene = window.GetSelectedCutscene();
        if (selectedCutscene != null)
        {
            window.SetTime(selectedCutscene.StartTime, selectedCutscene);
            window.GatherEventTypes(selectedCutscene);
        }
    }

    void OnFocus()
    {
        Setup();
    }

    /// <summary>
    /// Called when the window comes into focus
    /// </summary>
    override public void Setup()
    {
        LoadAllTimelineObjects<Cutscene>();
        m_InputController = new CutsceneEditorInput(this);
        m_SequencerIO = new UnitySequencerIO(this);

        EditorApplication.playmodeStateChanged = PlayModeStateChanged;
        UnSelectTrackGroup();

        // i have to do this to flush the list otherwise, old references remain in the list
        // and track items will appear highlighted when they aren't selected
        //m_SelectedEvents.Clear();

        // this fixes the bug where items look selected but they are not after making
        // code changes or starting or stopping the unity scene
        Cutscene selectedCutscene = GetSelectedCutscene();
        if (selectedCutscene != null)
        {
            foreach (CutsceneEvent ce in selectedCutscene.CutsceneEvents)
            {
                if (ce.GuiColor.r == CutsceneTrackItem.SelectedColor.r
                    && ce.GuiColor.g == CutsceneTrackItem.SelectedColor.g
                    && ce.GuiColor.b == CutsceneTrackItem.SelectedColor.b) // ignore alpha
                {
                    SelectTrackItem(ce, false);
                }
            }

            GatherEventTypes(selectedCutscene);
            AttachAllEventsToTracks(selectedCutscene);
        }
        //SetupScenelineData();
    }

    public void PlayModeStateChanged()
    {
        if (EditorApplication.isPlaying)
        {
            //Debug.Log("playing!");
        }
        else if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            //Debug.Log("EditorApplication.isPlayingOrWillChangePlaymode");
        }
        else if (EditorApplication.isPaused)
        {
            //Debug.Log("paused");
        }
        else
        {
            // the editor stopped playing
            m_IO.SaveMachinimaData(m_TimeObjectManager.GetAllTimelineObjects(), string.Format("{0}/{1}", Application.dataPath, MachinimaDataFileName));
            m_JustFinishedPlaying = true;
            Focus();
        }
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    protected override void Update()
    {
        if (m_CleanRequestState != CleanRequest.None)
        {
            // REMOVING THIS FOR NOW, MAYBE PUT IT BACK IN THE FUTURE
            //GroupManager.RemoveAllTrackItemsFromGroups();
            //RemoveEvents(GetSelectedCutscene().CutsceneEvents);
            //UnSelectAll();

            switch (m_CleanRequestState)
            {
                case CleanRequest.OpenBml:
                    FileOpenBML(m_SequencerData.m_XmlTitle);
                    break;

                case CleanRequest.OpenXml:
                    FileOpenXML(m_SequencerData.m_XmlTitle);
                    break;

                case CleanRequest.OpenBoth:
                    FileOpenBMLXMLPair(m_SequencerData.m_XmlTitle);
                    break;
            }
            m_CleanRequestState = CleanRequest.None;
        }

        // since guilayout requires 2 passes, if an event is selected or created between the 2 passes, unity
        // will log errors (harmless errors, but errors).  When events are created or selected, instead of immediately
        // performing the action, we defer the action to the update loop, which happens before either guilayout pass.
        // This prevents the ugle guilayout errors
        Cutscene selectedCutscene = GetSelectedCutscene();
        while (m_ItemsToBeAdded.Count > 0)
        {
            CutsceneTrackItem ce = m_ItemsToBeAdded[0];
            m_ItemsToBeAdded.RemoveAt(0);
            selectedCutscene.AddEvent(ce as CutsceneEvent);

            // recalulate the length of the cutscene to see if it should be longer with the addition of this new event
            selectedCutscene.Length = selectedCutscene.CalculateCutsceneLength();

            // select the new item
            INTERNAL_SelectTrackItem(ce, true);
        }

        while (m_ItemsToBeSelected.Count > 0)
        {
            EventSelectionData data = m_ItemsToBeSelected[0];
            m_ItemsToBeSelected.RemoveAt(0);
            INTERNAL_SelectTrackItem(data.ItemToSelect as CutsceneEvent, data.UnselectPreviousItems);
        }

        if (m_JustFinishedPlaying)
        {
            m_JustFinishedPlaying = false;
            m_IO.LoadMachinimaData(m_TimeObjectManager.GetAllTimelineObjects());
            Focus();
            //UnSelectAll();
            m_MockPlaying = false;
            m_ListenToNVBG = false;
        }
    }

    protected override void UpdateMockPlay()
    {
        Cutscene selectedCutscene = GetSelectedCutscene();
        OffsetTime((Time.realtimeSinceStartup - m_PreviousTime) * TimeScale, selectedCutscene);
        m_PreviousTime = Time.realtimeSinceStartup;
        Repaint();

        if (m_CurrentTime >= selectedCutscene.EndTime)
        {
            PlayCutscene(selectedCutscene, false);
        }
    }

    string MachinimaSaveDataPath()
    {
        string sceneName = Application.isPlaying ? Application.loadedLevelName : EditorApplication.currentScene;
        return string.Format("{0}/{1}/{2}", Application.dataPath, "CutsceneData",
            string.IsNullOrEmpty(sceneName) ? "un-namedScene" : Path.GetFileNameWithoutExtension(sceneName));
    }

    #region GUI Functions
    protected override void OnGUI()
    {
        if (NumCutscenes <= 0)
        {
            DrawAddCutscenes();
            return;
        }

        if (m_MockPlaying)
        {
            UpdateMockPlay();
        }

        InputController.Input();

        DrawToolbar();

        switch (m_SelectedView)
        {
            /*case ViewTypes.Sceneline:
                DrawSceneline();
                break;*/

            case ViewTypes.Cutscene:
                DrawCutscene();
                break;
        }

        InputController.UpdateCursorImage();
    }

    /// <summary>
    /// This is called when there are no cutscenes in the Unity scene
    /// </summary>
    void DrawAddCutscenes()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.Label("There are no cutscenes in this Unity scene");
            if (GUILayout.Button("Add Cutscene"))
            {
                AddCutscene();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Handles the drawing of the scene tabs, play button, options, etc
    /// </summary>
    protected override void DrawToolbar()
    {
        //Cutscene selectedCutscene = GetSelectedCutscene();
        GUILayout.BeginHorizontal();
        {
            /*ViewTypes prevView = m_SelectedView;
            m_SelectedView = (ViewTypes)GUILayout.Toolbar((int)m_SelectedView, ViewNames, EditorStyles.toolbarButton, GUILayout.Width(ViewTypeButtonWidth), GUILayout.Height(ToolBarHeight));
            m_SelectedView = ViewTypes.Cutscene;
            if (prevView != m_SelectedView)
            {
                // they switch views
                SwitchViews(m_SelectedView);
            }*/

            if (GUILayout.Button("File", EditorStyles.toolbarButton, GUILayout.Width(FileButtonWidth)))
            {
                ShowFileContextMenu();
            }

            int prevIndex = m_SelectedIndex;
            m_SelectedIndex = EditorGUILayout.Popup(m_SelectedIndex, m_TimeObjectNames.ToArray(), GUILayout.Width(CutsceneTrackGroupsListWidth - FileButtonWidth));
            if (prevIndex != m_SelectedIndex)
            {
                CutsceneSelected(prevIndex);
            }

            //GUILayout.Space(20);
            //InputController.PointerMode = (CutsceneEditorInput.PointerModes)GUILayout.Toolbar((int)InputController.PointerMode, PointerButtonTextures, EditorStyles.toolbarButton, GUILayout.Width(PointerModeButtonWidth));

            // Add/remove cutscene buttons
            //GUI.enabled = !Application.isPlaying;
            //GUILayout.Space(20);
            //GUILayout.Label("Cutscene", EditorStyles.toolbarTextField);
            //m_TooltipContent.text = "-";
            //m_TooltipContent.tooltip = "Delete this Cutscene";
            //if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton))
            //{
            //    ConfirmRemoveCutscene(GetSelectedCutscene());
            //}
            //m_TooltipContent.text = "+";
            //m_TooltipContent.tooltip = "Add Cutscene";
            //if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton))
            //{
            //    AddCutscene();
            //}

            //GUI.enabled = true;

            // add/remove track buttons
            GUILayout.Space(20);
            GUILayout.Label("Track", EditorStyles.toolbarTextField);
            m_TooltipContent.text = "-";
            m_TooltipContent.tooltip = "Remove Last Track";
            if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton))
            {
                RemoveTrackGroup(SelectedTrackGroup);
            }
            m_TooltipContent.text = "+";
            m_TooltipContent.tooltip = "Add Track";
            if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton))
            {
                AddTrackGroup(GroupManager);
            }

            m_TooltipContent.text = "";
            // reset/play/stop buttons
            GUI.enabled = !IsFastForwarding;
            GUILayout.Space(20);
            //m_TooltipContent.image = ResetButton;
            m_TooltipContent.text = "|<";
            m_TooltipContent.tooltip = "Reset Cutscene";
            if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton, GUILayout.Width(PlayButtonWidth)))
            {
                StopCutscene(GetSelectedCutscene(), true);
            }

            bool previousValue = m_MockPlaying;
            //m_TooltipContent.image = PlayButton;
            m_TooltipContent.text = ">";
            m_TooltipContent.tooltip = m_MockPlaying ? "Pause Cutscene" : "Play Cutscene";
            m_MockPlaying = GUILayout.Toggle(m_MockPlaying, m_TooltipContent, EditorStyles.toolbarButton, GUILayout.Width(PlayButtonWidth));
            if (m_MockPlaying != previousValue)
            {
                PlayCutscene(GetSelectedCutscene(), m_MockPlaying);
            }

            //m_TooltipContent.image = StopButton;
            m_TooltipContent.text = "[]";
            m_TooltipContent.tooltip = "Stop Cutscene";
            if (GUILayout.Button(m_TooltipContent, EditorStyles.toolbarButton, GUILayout.Width(PlayButtonWidth)))
            {
                PlayCutscene(GetSelectedCutscene(), false);
            }
            m_TooltipContent.image = null;

            DrawPlaySpeed();
            GUI.enabled = true;

            //GUILayout.Space(20);
            //m_VerboseText = GUILayout.Toggle(m_VerboseText, "Verbose", EditorStyles.toolbarButton);

            /*GUILayout.Space(20);
            if (GUILayout.Button("Order Cutscenes", EditorStyles.toolbarButton))
            {
                CutsceneOrderPopup.Init(new Rect(position.x + (position.width * 0.5f), position.y, EventDataWidth, position.height), m_CutsceneManager);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Simulate", EditorStyles.toolbarButton))
            {
                FastForwardPopup.Init(new Rect(position.x + (position.width * 0.5f), position.y, EventDataWidth, position.height), m_CutsceneManager, m_SelectedCutsceneIndex);
            }*/

            // save button
            /*GUILayout.Space(20);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Save", string.Format("Are you sure that you want to save cutscene {0}?", selectedCutscene.CutsceneName), "Yes", "No"))
                {
                    m_IO.SaveCutscene(string.Format("{0}/{1}.xml", MachinimaSaveDataPath(), selectedCutscene.CutsceneName), selectedCutscene);
                }
            }

            // save all button
            GUILayout.Space(10);
            if (GUILayout.Button("Save All", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Save", string.Format("Are you sure that you want to save all cutscenes?"), "Yes", "No"))
                {
                    m_IO.SaveAllCutscenes(MachinimaSaveDataPath(), m_CutsceneManager.GetAllCutscenes());
                }
            }

            // load all button
            GUILayout.Space(10);
            if (GUILayout.Button("Load All", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Load", string.Format("Are you sure that you want to load all cutscenes?"), "Yes", "No"))
                {
                    m_IO.LoadAllCutscenes(m_CutsceneManager.GetAllCutscenes(), MachinimaSaveDataPath());
                }
            }*/

            // right align the zoom slider
            GUILayout.FlexibleSpace();
            //float prevZoom = m_Zoom;
            DrawZoomer();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw all the data associated with the currently selecte cutscene
    /// </summary>
    void DrawCutscene()
    {
        Cutscene selectedCutscene = null;
        selectedCutscene = GetSelectedCutscene();
        if (selectedCutscene == null)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        {
            DrawCutsceneTrackGroups(selectedCutscene);
            SetupTrackPositions(selectedCutscene.Length);
            SetupTimeSliderData();

            //m_TrackScrollPos.y = m_TrackGroupScrollPos.y; // maintain the same y between the 2 scroll views
            float prevScrollY = m_TrackScrollPos.y;
            m_TrackScrollPos = GUI.BeginScrollView(m_TrackArea, m_TrackScrollPos, m_TrackScrollArea);
            {
                if (prevScrollY != m_TrackScrollPos.y)
                {
                    Repaint();
                }

                // draw in this order
                DrawTimeLine(selectedCutscene.StartTime);
                DrawCutsceneTracks(selectedCutscene.GroupManager);
                DrawCutsceneEvents(selectedCutscene);
                DrawTimeSlider(selectedCutscene.StartTime, selectedCutscene.EndTime);
            }
            GUI.EndScrollView();

            // right align the tabs
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            {
                m_SelectedDataViewTab = (DataView)GUILayout.Toolbar((int)m_SelectedDataViewTab, DataViewNames, GUILayout.Width(EventDataWidth));
                switch (m_SelectedDataViewTab)
                {
                    case DataView.Cutscene: // cutscene
                        DrawCutsceneData(selectedCutscene);
                        break;

                    case DataView.Event:
                        DrawEventData(SelectedEvents);
                        break;
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draw all the gui elements of the track groups
    /// </summary>
    /// <param name="selectedCutscene"></param>
    void DrawCutsceneTrackGroups(Cutscene selectedCutscene)
    {
        if (selectedCutscene == null || selectedCutscene.GroupManager == null || selectedCutscene.GroupManager.NumGroups == 0)
        {
            return;
        }

        m_TrackGroupArea.Set(GroupStartingX - GroupStartingXOffset, ToolBarHeight, CutsceneTrackGroupsListWidth - 15 + GroupStartingXOffset, position.height);
        m_TrackGroupScrollPos.y = m_TrackScrollPos.y; // maintain the same y between the 2 scroll views
        m_TrackGroupScrollArea.Set(m_TrackGroupArea.x, m_TrackGroupArea.y, 1000, m_TrackScrollArea.height);
        m_TrackGroupScrollPos = GUI.BeginScrollView(m_TrackGroupArea, m_TrackGroupScrollPos, m_TrackGroupScrollArea);
        {
            m_TrackScrollPos.y = m_TrackGroupScrollPos.y; // maintain the same y between the 2 scroll views
            Color prev = GUI.color;
            if (m_SelectedTrackGroup != null && !m_SelectedTrackGroup.Hidden)
            {
                GUI.color = CutsceneTrackItem.SelectedColor;
                m_SelectedTrackGroupArea.y = m_SelectedTrackGroup.GroupNamePosition.y;
                GUI.Box(m_SelectedTrackGroupArea, "");
                GUI.color = prev;
            }
            if (m_DropLocationTrackGroup != null)
            {
                GUI.color = SelectionBoxColor;
                m_TrackGroupDropArea.y = m_DropLocationTrackGroup.GroupNamePosition.y;
                GUI.Box(m_TrackGroupDropArea, "");
                GUI.color = prev;
            }

            int indentAmount = 0;
            float yPos = TrackStartingY;

            // depth first traversal
            Stack<CutsceneTrackGroup> stack = new Stack<CutsceneTrackGroup>();
            Stack<int> indentLevel = new Stack<int>();
            CutsceneTrackGroup currGroup = null;
            Rect foldoutPos = new Rect();
            foreach (CutsceneTrackGroup group in selectedCutscene.GroupManager.m_TrackGroups)
            {
                indentAmount = 0;

                stack.Clear();
                indentLevel.Clear();
                stack.Push(group);
                indentLevel.Push(indentAmount);

                while (stack.Count > 0)
                {
                    currGroup = stack.Pop();
                    indentAmount = indentLevel.Pop();

                    currGroup.GroupNamePosition.Set(GroupStartingX + indentAmount, yPos, CutsceneTrackGroupsListWidth, TrackHeight);
                    currGroup.TrackPosition.Set(TrackStartingX, yPos, position.width, TrackHeight);
                    currGroup.GroupNamePosition.width -= indentAmount;

                    if (currGroup.HasChildren)
                    {
                        foldoutPos = currGroup.GroupNamePosition;
                        foldoutPos.width = 15;
                        foldoutPos.x -= foldoutPos.width;

                        bool previouslyExpanded = currGroup.Expanded;

                        prev = GUI.color;
                        GUI.color = Transparent;
                        currGroup.Expanded = GUI.Toggle(foldoutPos, currGroup.Expanded, "");
                        GUI.color = prev;
                        GUI.DrawTexture(foldoutPos, currGroup.Expanded ? FoldOutExpandedTex : FoldOutNotExpandedTex);
                        if (currGroup.EditingName)
                        {
                            currGroup.GroupName = GUI.TextField(currGroup.GroupNamePosition, currGroup.GroupName);
                        }
                        else
                        {
                            GUI.Label(currGroup.GroupNamePosition, currGroup.GroupName);
                        }

                        if (previouslyExpanded != currGroup.Expanded)
                        {
                            // it changed this frame
                            GroupManager.ChangedExpanded(currGroup, currGroup.Expanded);

                            // hidden track items can't be selected, so unselect them
                            List<CutsceneTrackItem> selectedTrackItems = GroupManager.GetSelectedTrackItems(currGroup);
                            selectedTrackItems.ForEach(ti => UnSelectTrackItem(ti));
                        }
                    }
                    else
                    {
                        if (currGroup.EditingName)
                        {
                            currGroup.GroupName = GUI.TextField(currGroup.GroupNamePosition, currGroup.GroupName);
                        }
                        else
                        {
                            GUI.Label(currGroup.GroupNamePosition, currGroup.GroupName);
                        }
                    }

                    // draw children only if expanded
                    if (currGroup.Expanded)
                    {
                        for (int i = currGroup.m_Children.Count - 1; i >= 0; i--)
                        {
                            stack.Push(currGroup.m_Children[i]);
                            indentLevel.Push(indentAmount + TrackGroupIndent);
                        }
                    }
                    yPos += TrackHeight;
                }
            }
        }
        GUI.EndScrollView();
    }

    /// <summary>
    /// Draw the tracks on the timeline and the timeline interval
    /// </summary>
    /// <param name="trackGroupManager"></param>
    void DrawCutsceneTracks(CutsceneTrackGroupManager trackGroupManager)
    {
        if (trackGroupManager == null)
        {
            return;
        }

        Color col = GUI.color;
        GUI.color = AlphaWhite;
        m_TrackBackground.Set(m_TrackArea.x, m_TrackArea.y, m_TrackArea.width, m_TrackScrollArea.height);
        m_TrackBackground.width = Mathf.Max(m_TrackScrollArea.width, m_TrackArea.width);
        m_TrackBackground.height = Mathf.Max(m_TrackScrollArea.height, m_TrackArea.height);
        GUI.Box(m_TrackBackground, "");
        GUI.color = col;

        // draw tracks
        m_TrackStartingPosition.width = Mathf.Max(m_TrackScrollArea.width, m_TrackArea.width);
        trackGroupManager.SetStartingPosition(m_TrackStartingPosition);
        trackGroupManager.Draw(TrackHeight);

        if (InputController.IsRubberBandSelecting)
        {
            col = GUI.color;
            GUI.color = SelectionBoxColor;
            GUI.Box(InputController.RubberBandSelectionArea, "");
            GUI.color = col;
        }
    }

    /// <summary>
    /// Draw a specific track item on the timeline
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="timelineStartTime"></param>
    /// <param name="timelineEndTime"></param>
    /// <param name="scrollArea"></param>
    /// <returns></returns>
    bool DrawCutsceneTrackItem(CutsceneTrackItem trackItem, float timelineStartTime, float timelineEndTime, Rect scrollArea)
    {
        Color prevCol = GUI.color;
        // update position based on start time
        trackItem.GuiPosition.x = GetPositionFromTime(timelineStartTime, timelineEndTime, trackItem.StartTime, scrollArea);

        if (trackItem.LengthRepresented)
        {
            trackItem.GuiPosition.width = GetWidthFromTime(timelineStartTime, timelineEndTime,
                trackItem.StartTime + trackItem.Length, trackItem.GuiPosition.x, scrollArea);
        }

        // optimzation for not rendering offscreen events.  TODO: clean this up
        Rect testArea = m_TrackArea;
        testArea.x += m_TrackScrollPos.x;
        testArea.y += m_TrackScrollPos.y;
        Vector2 guiPos = Vector2.zero;
        guiPos.x = trackItem.GuiPosition.x;
        guiPos.y = trackItem.GuiPosition.y;

        bool pressed = false;
        //if (testArea.Contains(guiPos))
        if (Utils.IsRectOverlapping(testArea, trackItem.GuiPosition))
        {
            pressed = trackItem.Draw(m_VerboseText, m_TooltipContent);
        }

        GUI.color = prevCol;
        return pressed;
    }

    void DrawCutsceneEvents(Cutscene cutscene)
    {
        if (cutscene == null)
        {
            return;
        }

        Color prevCol = GUI.color;
        foreach (CutsceneEvent ce in cutscene.CutsceneEvents)
        {
            GUI.SetNextControlName(EventControlName + ce.UniqueId);
            if (DrawCutsceneTrackItem(ce, cutscene.StartTime, cutscene.EndTime, m_TrackScrollArea)) { }
        }

        GUI.color = prevCol;
    }

    /// <summary>
    /// Prepares the timeline playhead for drawing 
    /// </summary>
    protected override void SetupTimeSliderData()
    {
        Cutscene cutscene = GetSelectedCutscene();
        if (Application.isPlaying && cutscene.HasStartedPlaying)
        {
            SetTime(cutscene.StartTime + cutscene.LocalTime, cutscene);
        }

        float timelinePosX = GetPositionFromTime(cutscene.StartTime, cutscene.EndTime, m_CurrentTime, m_TrackScrollArea) - 7f; // TODO: fix offset

        if ((cutscene.HasStartedPlaying && !cutscene.IsPaused) || m_MockPlaying)
        {
            // move the track scoller with the time slider as the cutscene is played
            if (timelinePosX > m_TrackArea.x + m_TrackArea.width + m_TrackScrollPos.x - 100)
            {
                m_TrackScrollPos.x = timelinePosX - (m_TrackArea.x + m_TrackArea.width - 100);
            }
        }
    }

    void ChangeCutsceneName(string oldName, string newName)
    {
        m_TimeObjectManager.ChangeCutsceneName(oldName, newName);
        m_SelectedIndex = m_TimeObjectManager.GetTimeObjectIndexByName(newName);
        LoadTimelineObjectNames(m_TimeObjectManager.GetAllTimelineObjects());
        ChangedData();
    }

    /// <summary>
    /// Draws the cutscene specific data that doesn't have to do with events
    /// </summary>
    /// <param name="cutscene"></param>
    void DrawCutsceneData(Cutscene cutscene)
    {
        if (cutscene != null)
        {
            GUI.enabled = !Application.isPlaying;
            string oldName = cutscene.CutsceneName;
            string newName = EditorGUILayout.TextField("Cutscene Name", cutscene.CutsceneName);
            if (oldName != newName)
            {
                ChangeCutsceneName(oldName, newName);
            }
            GUI.enabled = true;

            float prevStartTime = cutscene.StartTime;
            cutscene.StartTime = EditorGUILayout.FloatField("Start Time", cutscene.StartTime);
            cutscene.StartTime = Mathf.Max(cutscene.StartTime, 0);
            if (prevStartTime != cutscene.StartTime)
            {
                cutscene.OffsetEventStartingTimes(prevStartTime - cutscene.StartTime);
                SetTime(m_CurrentTime, cutscene);
                ChangedData();
            }

            bool prevLoop = cutscene.Loop;
            cutscene.Loop = EditorGUILayout.Toggle("Loop", cutscene.Loop);
            if (prevLoop != cutscene.Loop)
            {
                ChangedData();
            }

            int prevLoopCount = cutscene.LoopCount;
            cutscene.LoopCount = EditorGUILayout.IntField("Loop Count", cutscene.LoopCount);
            if (prevLoopCount != cutscene.LoopCount)
            {
                ChangedData();
            }
        }
    }

    /// <summary>
    /// Draws all data associated with events
    /// </summary>
    /// <param name="selectedEvents"></param>
    protected override void DrawEventData(List<CutsceneTrackItem> selectedEvents)
    {
        if (selectedEvents.Count == 0)
        {
            EditorGUILayout.LabelField("No event selected");
            return;
        }
        else if (selectedEvents.Count > 1)
        {
            EditorGUILayout.LabelField("Selected Events:");
            m_CutsceneEventDataScrollPos = GUILayout.BeginScrollView(m_CutsceneEventDataScrollPos);
            {
                selectedEvents.ForEach(ce => EditorGUILayout.LabelField(string.Format("{0} {1}", ce.Name, ce.Locked ? "(LOCKED)" : "")));
                EditorGUILayout.BeginHorizontal();
                {
                    m_MoveToTime = EditorGUILayout.FloatField("Move to Time", m_MoveToTime);
                    if (m_MoveToTime < 0)
                    {
                        m_MoveToTime = 0;
                    }
                    if (GUILayout.Button("Set"))
                    {
                        MoveEventsToTime(SelectedEvents, m_MoveToTime);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            return;
        }

        m_CutsceneEventDataScrollPos = GUILayout.BeginScrollView(m_CutsceneEventDataScrollPos);
        {
            GUI.enabled = !SelectedEvent.ReadOnly;
            CutsceneEvent selectedEvent = GetSelectedCutscene().GetEventByUniqueId(selectedEvents[0].UniqueId) as CutsceneEvent;//selectedEvents[0] as CutsceneEvent;

            // general event data
            string prevName = selectedEvent.Name;
            selectedEvent.Name = EditorGUILayout.TextField("Event Name", selectedEvent.Name);
            if (prevName != selectedEvent.Name)
            {
                ChangedData();
            }
            float newTime = EditorGUILayout.FloatField("Start Time", selectedEvent.StartTime);
            if (newTime != selectedEvent.StartTime)
            {
                SetTrackItemTime(selectedEvent, newTime, CollisionResolutionType.MoveDown);
                ChangedData();
            }

            // only allow them to scale the length if it's a timed event, otherwise the length is static
            if (selectedEvent.FireAndForget)
            {
                bool prevState = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.FloatField("Length", selectedEvent.Length);
                GUI.enabled = prevState;
            }
            else
            {
                float prev = EditorGUILayout.FloatField("Length", selectedEvent.Length);
                if (prev != selectedEvent.Length)
                {
                    InputController.ScaleTrackItem(selectedEvent, prev - selectedEvent.Length, true, Vector2.zero);
                    ChangedData();
                }
            }

            bool prevVal = selectedEvent.Enabled;
            selectedEvent.Enabled = EditorGUILayout.Toggle("Enabled", selectedEvent.Enabled);
            if (prevVal != selectedEvent.Enabled)
            {
                selectedEvent.SetEnabled(selectedEvent.Enabled);
                ChangedData();
            }

            prevVal = selectedEvent.Locked;
            Color prevColor = GUI.color;
            GUI.color = selectedEvent.Locked ? Color.red : prevColor;
            selectedEvent.Locked = EditorGUILayout.Toggle("Locked", selectedEvent.Locked);
            if (prevVal != selectedEvent.Locked)
            {
                selectedEvent.SetLocked(selectedEvent.Locked);
                ChangedData();
            }
            GUI.color = prevColor;

            string prevEventType = selectedEvent.EventType;
            int index = EditorGUILayout.Popup("Event Type", m_EventTypes.FindIndex(e => selectedEvent.EventType == e), m_EventTypes.ToArray());
            if (index != -1)
            {
                selectedEvent.EventType = m_EventTypes[index];
            }
            if (selectedEvent.EventType != prevEventType)
            {
                ChangedCutsceneEventType(selectedEvent.EventType, selectedEvent);
                ChangedData();
            }

            if (selectedEvent.EventType == GenericEventNames.Custom)
            {
                DrawCustomEventData(selectedEvent);
            }

            // show the functions that are available
            DrawEventFunctions(selectedEvent);

            // update function params gui
            DrawEventFunctionParameters(selectedEvent);

            if (selectedEvent.EventType == GenericEventNames.Custom)
            {
                DrawAdditionalEventOptions(selectedEvent);
            }

            // note area
            GUILayout.Space(10);
            m_ShowNotes = EditorGUILayout.Foldout(m_ShowNotes, "Notes");
            if (m_ShowNotes)
            {
                selectedEvent.Notes = GUILayout.TextArea(selectedEvent.Notes, GUILayout.Height(NoteBoxHeight), GUILayout.MaxWidth(EventDataWidth));
            }

            GUI.enabled = true;
        } 
        GUILayout.EndScrollView(); 
    }

    /// <summary>
    /// Updates all lists and data sets used for drawing and changes the event data of the provided event
    /// </summary>
    /// <param name="newType"></param>
    /// <param name="ce"></param>
    public void ChangedCutsceneEventType(string newType, CutsceneEvent ce)
    {
        ce.EventType = newType;
        //ce.SetEventType(newType);
        Cutscene selectedCutscene = GetSelectedCutscene();

        GenericEvents[] genericEventsGO = selectedCutscene.GetComponentsInChildren<GenericEvents>();
        if (genericEventsGO == null)
        {
            Debug.LogError(string.Format("Cutscene {0} doesn't have a GenericEvents component on any child", selectedCutscene.name));
            return;
        }

        MonoBehaviour targetComponent = null;

        foreach (GenericEvents ge in genericEventsGO)
        {
            if (ge.GetEventType() == newType)
            {
                targetComponent = ge;
                break;
            }
        }

        if (targetComponent != null)
        {
            ce.SetFunctionTargets(targetComponent.gameObject, targetComponent);
            ChangedEventGameObjectSelection(ce.TargetGameObject, ce, ce.TargetComponent);
        }
        else
        {
            ce.SetFunctionTargets(null, null);

            m_EventComponentNames.Clear();
            m_SelectedEventComponent = 0;

            m_EventFunctionNames.Clear();
            m_SelectedEventFunction = 0;

            m_EventSelectedFunctionOverloadNames.Clear();
            m_MethodInfos.Clear();
        }

        ce.SetEventType(newType);
    }

    /// <summary>
    /// Upates the functions that are available from the newly selected gameobject's components
    /// </summary>
    /// <param name="newSelection"></param>
    /// <param name="selectedEvent"></param>
    /// <param name="selectedComponent"></param>
    void ChangedEventGameObjectSelection(GameObject newSelection, CutsceneEvent selectedEvent, Component selectedComponent)
    {
        selectedEvent.TargetGameObject = newSelection;

        // gameobject selection changed, gather the component names on the new selection
        GatherGameObjectComponentNames(selectedEvent.TargetGameObject);

        m_SelectedEventComponent = 0;
        m_SelectedEventFunction = 0;
        m_SelectedEventLengthOption = 0;
        selectedEvent.FunctionOverloadIndex = 0;
        selectedEvent.LengthDefiningParamName = "";

        if (selectedComponent == null)
        {
            selectedEvent.TargetComponent = selectedEvent.TargetGameObject.GetComponent(m_EventComponentNames[m_SelectedEventComponent]);
        }

        GatherComponentFunctionNames(selectedEvent.TargetComponent, selectedEvent, true);
        GatherEventLengthOptions(selectedEvent);

        selectedEvent.SetFunctionTargets(selectedEvent.TargetGameObject, selectedEvent.TargetComponent);
    }

    public void ChangedEventFunction(CutsceneEvent ce, string functionName)
    {
        ChangedEventFunction(ce, functionName, 0);
    }

    public void ChangedEventFunction(CutsceneEvent ce, string functionName, int overloadIndex)
    {
        int selectedFunctionIndex = m_EventFunctionNames.FindIndex(s => s == functionName);
        if (selectedFunctionIndex == -1)
        {
            Debug.Log(string.Format("functionName {0} doesn't exist", functionName));
            return;
        }

        m_SelectedEventFunction = selectedFunctionIndex;
        ce.ChangedEventFunction(functionName, overloadIndex);
        GatherComponentFunctionParams(ce.TargetComponent, m_EventFunctionNames[m_SelectedEventFunction], ce);
        ce.GatherEventParams(ce.TargetComponent, m_EventFunctionNames[m_SelectedEventFunction], ce.FunctionOverloadIndex);
        GatherEventLengthOptions(ce);
        ChangedData();
    }

    /// <summary>
    /// Draws the list of functions that are available on an event
    /// </summary>
    /// <param name="selectedEvent"></param>
    void DrawEventFunctions(CutsceneEvent selectedEvent)
    {
        // functions names on target component
        int prevSelection = m_SelectedEventFunction;
        m_SelectedEventFunction = EditorGUILayout.Popup(m_SelectedEventFunction, m_EventFunctionNames.ToArray());
        if (prevSelection != m_SelectedEventFunction)
        {
            // they chose a different function
            ChangedEventFunction(selectedEvent, m_EventFunctionNames[m_SelectedEventFunction]);
        }

        // function overloads
        if (m_EventSelectedFunctionOverloadNames.Count > 0)
        {
            prevSelection = selectedEvent.FunctionOverloadIndex;
            GUILayout.Label("Function Overload");
            selectedEvent.FunctionOverloadIndex = EditorGUILayout.Popup(selectedEvent.FunctionOverloadIndex, m_EventSelectedFunctionOverloadNames.ToArray());
            if (prevSelection != selectedEvent.FunctionOverloadIndex)
            {
                // they chose a different overload
                selectedEvent.GatherEventParams(selectedEvent.TargetComponent, m_EventFunctionNames[m_SelectedEventFunction], selectedEvent.FunctionOverloadIndex);
                GatherEventLengthOptions(selectedEvent);
                ChangedData();
            }
        }
    }

    /// <summary>
    /// Draws data associated with the 'Custom' event type
    /// </summary>
    /// <param name="selectedEvent"></param>
    void DrawCustomEventData(CutsceneEvent selectedEvent)
    {
        if (selectedEvent == null)
        {
            EditorGUILayout.LabelField("No event selected");
            return;
        }

        // Custom events need a gameobject target in order to draw their data correctly
        GameObject prevGOSelection = selectedEvent.TargetGameObject;
        selectedEvent.TargetGameObject = (GameObject)EditorGUILayout.ObjectField("Target", selectedEvent.TargetGameObject, typeof(GameObject), true);
        if (prevGOSelection != selectedEvent.TargetGameObject)
        {
            ChangedEventGameObjectSelection(selectedEvent.TargetGameObject, selectedEvent, null);
        }

        // component names on target game object
        int prevSelection = m_SelectedEventComponent;
        m_SelectedEventComponent = EditorGUILayout.Popup(m_SelectedEventComponent, m_EventComponentNames.ToArray());
        if (prevSelection != m_SelectedEventComponent)
        {
            // select it
            m_SelectedEventFunction = 0;
            selectedEvent.FunctionOverloadIndex = 0;
            selectedEvent.TargetComponent = (MonoBehaviour)selectedEvent.TargetGameObject.GetComponent(m_EventComponentNames[m_SelectedEventComponent]);
            selectedEvent.SetFunctionTargets(selectedEvent.TargetGameObject, selectedEvent.TargetComponent);
            GatherComponentFunctionNames(selectedEvent.TargetComponent, selectedEvent, true);
            ChangedData();
        }
    }

    /// <summary>
    /// Draw the popup that lets you choose a length defining param for custom events
    /// </summary>
    /// <param name="selectedEvent"></param>
    void DrawAdditionalEventOptions(CutsceneEvent selectedEvent)
    {
        GUILayout.Label("Duration");
        int prev = m_SelectedEventLengthOption;
        m_SelectedEventLengthOption = EditorGUILayout.Popup(m_SelectedEventLengthOption, m_EventLengthOptions.ToArray());
        if (prev != m_SelectedEventLengthOption)
        {
            selectedEvent.SetEventLengthFromParameter(m_EventLengthOptions[m_SelectedEventLengthOption]);
            ChangedData();
        }
    }

    /// <summary>
    /// Draws an appropriate gui element for each parameter of the given event function based on the parameter type
    /// </summary>
    /// <param name="cutsceneEvent"></param>
    void DrawEventFunctionParameters(CutsceneEvent cutsceneEvent)
    {
        if (cutsceneEvent == null)
        {
            return;
        }

        ParameterInfo[] selectedFunctionParams = cutsceneEvent.GetFunctionParams(cutsceneEvent.TargetComponent, cutsceneEvent.FunctionName, cutsceneEvent.FunctionOverloadIndex, cutsceneEvent.EventType);
        if (selectedFunctionParams == null)
        {
            return;
        }

        if (selectedFunctionParams.Length != cutsceneEvent.m_Params.Count)
        {
            // something went wrong, clear and reload the params
            cutsceneEvent.GatherEventParams(cutsceneEvent.TargetComponent, cutsceneEvent.FunctionName, cutsceneEvent.FunctionOverloadIndex);
            GatherEventLengthOptions(cutsceneEvent);
        }

        GUILayout.Label("Function Parameters");
        for (int i = 0; i < selectedFunctionParams.Length; i++)
        {
            ParameterInfo functionParam = selectedFunctionParams[i];
            CutsceneEventParam cutsceneParam = cutsceneEvent.m_Params[i];
            cutsceneParam.usesEnumData = functionParam.ParameterType.IsEnum;

            bool isLengthDefiner = cutsceneParam.Name == cutsceneEvent.LengthDefiningParamName;
            bool changedLengthDefinerData = false;

            if (functionParam.ParameterType == typeof(int))
            {
                int prevData = cutsceneParam.intData;
                cutsceneParam.intData = EditorGUILayout.IntField(cutsceneParam.Name, cutsceneParam.intData);
                changedLengthDefinerData = prevData != cutsceneParam.intData;
            }
            else if (functionParam.ParameterType == typeof(bool))
            {
                bool prevData = cutsceneParam.boolData;
                cutsceneParam.boolData = EditorGUILayout.Toggle(cutsceneParam.Name, cutsceneParam.boolData);
                changedLengthDefinerData = prevData != cutsceneParam.boolData;
            }
            else if (functionParam.ParameterType == typeof(float))
            {
                float prevData = cutsceneParam.floatData;
                cutsceneParam.floatData = EditorGUILayout.FloatField(cutsceneParam.Name, cutsceneParam.floatData);
                changedLengthDefinerData = prevData != cutsceneParam.floatData;
            }
            else if (functionParam.ParameterType == typeof(string))
            {
                string prevData = cutsceneParam.stringData;
                EditorGUILayout.BeginHorizontal();
                {
                    cutsceneParam.stringData = EditorGUILayout.TextField(cutsceneParam.Name, cutsceneParam.stringData);
                    if (GUILayout.Button("...", GUILayout.Width(25)))
                    {
                        StringEditPopup.Init(new Rect(position.x + position.width - EventDataWidth, position.y, EventDataWidth, 250), cutsceneParam.Name, cutsceneParam.stringData, OnSavedLongString);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                changedLengthDefinerData = prevData != cutsceneParam.stringData;
            }
            else if (functionParam.ParameterType == typeof(Vector2))
            {
                Vector2 prevData = cutsceneParam.vec2Data;
                cutsceneParam.vec2Data = EditorGUILayout.Vector2Field(cutsceneParam.Name, cutsceneParam.vec2Data);
                changedLengthDefinerData = prevData != cutsceneParam.vec2Data;
            }
            else if (functionParam.ParameterType == typeof(Vector3))
            {
                Vector3 prevData = cutsceneParam.vec3Data;
                cutsceneParam.vec3Data = EditorGUILayout.Vector3Field(cutsceneParam.Name, cutsceneParam.vec3Data);
                changedLengthDefinerData = prevData != cutsceneParam.vec3Data;
            }
            else if (functionParam.ParameterType == typeof(Vector4))
            {
                Vector4 prevData = cutsceneParam.vec4Data;
                cutsceneParam.vec4Data = EditorGUILayout.Vector4Field(cutsceneParam.Name, cutsceneParam.vec4Data);
                changedLengthDefinerData = prevData != cutsceneParam.vec4Data;
            }
            else if (functionParam.ParameterType == typeof(Color))
            {
                Color prevData = cutsceneParam.colorData;
                cutsceneParam.colorData = EditorGUILayout.ColorField(cutsceneParam.Name, cutsceneParam.colorData);
                changedLengthDefinerData = prevData != cutsceneParam.colorData;
            }
            else if (functionParam.ParameterType.IsEnum)
            {
                if (cutsceneParam.enumData == null)
                {
                    if (!string.IsNullOrEmpty(cutsceneParam.enumDataString))
                    {
                        cutsceneParam.enumData = Enum.Parse(functionParam.ParameterType, cutsceneParam.enumDataString) as Enum;
                    }
                    else
                    {
                        // if there isn't a value set, pick the first one in the enum
                        cutsceneParam.enumData = (Enum)Enum.GetValues(functionParam.ParameterType).GetValue(0);
                    }
                }
                Enum prevData = cutsceneParam.enumData;
                cutsceneParam.enumData = EditorGUILayout.EnumPopup(cutsceneParam.Name, Enum.Parse(functionParam.ParameterType, cutsceneParam.enumData.ToString()) as Enum);
                cutsceneParam.enumDataString = cutsceneParam.enumData.ToString();
                changedLengthDefinerData = prevData != cutsceneParam.enumData;
            }
            else if (functionParam.ParameterType.GetType().IsInstanceOfType(typeof(UnityEngine.Object))) // this needs to go last
            {
                UnityEngine.Object prevData = cutsceneParam.objData;
                cutsceneParam.SetObjData((UnityEngine.Object)EditorGUILayout.ObjectField(cutsceneParam.Name, cutsceneParam.objData, functionParam.ParameterType, true));
                changedLengthDefinerData = prevData != cutsceneParam.objData;
            }

            if (changedLengthDefinerData)
            {
                if (isLengthDefiner)
                {
                    UpdateCutsceneEventLength(cutsceneEvent);
                }
                ChangedData();
            }
        }
    }
    #endregion

    #region Data Functions
    public void AddBmlTiming(string id, float time, string text)
    {
        if (!m_CreateBMLEvents)
        {
            return;
        }

        //se.GuiPosition.width = se.Name.Length * 12 + 8; // hack way of calculating this font outside of OnGUI
        if (!string.IsNullOrEmpty(text))
        {
            Vector2 pos = new Vector2();
            pos.x = GetPositionFromTime(StartTime, EndTime, time, m_TrackScrollArea);
            pos.y = TrackStartingY;
            CutsceneEvent ce = CreateEventAtPosition(pos) as CutsceneEvent;
            ce.Name = text;
            m_SequencerData.m_BMLData.Add(new BMLData(id, time, text, ce.UniqueId));

            ChangedCutsceneEventType(GenericEventNames.Common, ce);
            ChangedEventFunction(ce, "Marker");
            ce.SetReadOnly(true);
        }
        else
        {
            // this is the end timing of the previous T
            BMLData bmlData = m_SequencerData.m_BMLData[m_SequencerData.m_BMLData.Count - 1]; // we want the previous one
            CutsceneTrackItem namedEvent = m_ItemsToBeAdded.Find(ti => ti.UniqueId == bmlData.m_SeqEventUniqueId);
            
            // determine the length and size
            namedEvent.LengthRepresented = true;
            namedEvent.Length = time - bmlData.m_Time;
            namedEvent.GuiPosition.width = GetWidthFromTime(namedEvent.EndTime, namedEvent.GuiPosition.x);
            m_SequencerData.m_BMLData.Add(new BMLData(id, time, text, namedEvent.UniqueId));
        }
    }

    public void AddVisemeTiming(string visemeName, float startTime, float endTime)
    {
        if (!m_CreateBMLEvents)
        {
            return;
        }

        float eventYPos = TrackStartingY + TrackHeight; // I want these on the second track
        Vector2 eventPos = new Vector2(GetPositionFromTime(StartTime, EndTime, startTime, m_TrackScrollArea), eventYPos);
        CutsceneEvent ce = CreateEventAtPosition(eventPos) as CutsceneEvent;
        ce.Name = visemeName;
        ce.SetReadOnly(true);
        ce.LengthRepresented = true;
        ce.Length = endTime - startTime;
        ChangedCutsceneEventType(GenericEventNames.Common, ce);
        ChangedEventFunction(ce, "Marker");
    }

    /// <summary>
    /// Callback function for the StringEditPopup
    /// </summary>
    /// <param name="paramName"></param>
    /// <param name="strData"></param>
    void OnSavedLongString(string paramName, string strData)
    {
        if (SelectedEvent != null)
        {
            CutsceneEventParam param = (SelectedEvent as CutsceneEvent).FindParameter(paramName);
            if (param != null)
            {
                param.stringData = strData;
            }
        }
        Repaint();
    }

    /// <summary>
    /// Maintains the proper cutscene length sot that the timeline remains correct
    /// </summary>
    /// <param name="cutsceneEvent"></param>
    void UpdateCutsceneEventLength(CutsceneEvent cutsceneEvent)
    {
        // update length based on tracked variable if need be
        //if (!string.IsNullOrEmpty(cutsceneEvent.LengthDefiningParamName))
        {
            cutsceneEvent.SetEventLengthFromParameter(cutsceneEvent.LengthDefiningParamName);

            // update the length of the cutscene;
            Cutscene selectedCutscene = GetSelectedCutscene();
            selectedCutscene.Length = selectedCutscene.CalculateCutsceneLength();
            Repaint();
        }
    }

    /// <summary>
    /// Collects all the event types available in a cutscene
    /// </summary>
    /// <param name="cutscene"></param>
    void GatherEventTypes(Cutscene cutscene)
    {
        m_EventTypes.Clear();
        if (cutscene == null)
        {
            return;
        }

        GenericEvents[] genericEventsGO = (GenericEvents[])cutscene.GetComponentsInChildren<GenericEvents>();
        if (genericEventsGO == null)
        {
            Debug.LogError(string.Format("Cutscene {0} doesn't have a GenericEvents component on any child", cutscene.name));
            return;
        }

        foreach (GenericEvents genericEvent in genericEventsGO)
        {
            string eventType = (string)genericEvent.GetType().GetMethod("GetEventType").Invoke(genericEvent, null);
            m_EventTypes.Add(eventType);
        }

        m_EventTypes.Add(GenericEventNames.Custom);
        m_EventTypes.Sort((e1, e2) => e1.CompareTo(e2));
    }

    /// <summary>
    /// Collects all the names of the components that are attached to the provided game object
    /// </summary>
    /// <param name="target"></param>
    void GatherGameObjectComponentNames(GameObject target)
    {
        MonoBehaviour[] attachedBehaviours = target.GetComponents<MonoBehaviour>();

        // sort components by name
        Array.Sort(attachedBehaviours, (m1, m2) => m1.ToString().CompareTo(m2.ToString()));

        m_EventComponentNames.Clear();
        for (int i = 0; i < attachedBehaviours.Length; i++)
        {
            m_EventComponentNames.Add(attachedBehaviours[i].GetType().ToString());
        }
    }

    /// <summary>
    /// Sorts and prunes the provided methods
    /// </summary>
    /// <param name="methods"></param>
    void SetupComponentMethods(MethodInfo[] methods)
    {
        // sort components by name
        Array.Sort(methods, (m1, m2) => m1.ToString().CompareTo(m2.ToString()));

        // add method names
        m_MethodInfos.Clear();
        m_EventFunctionNames.Clear();
        foreach (MethodInfo method in methods)
        {
            /*
             * Rules for event functions
             * 1) public only
             * 2) no constructors
             * 3) no unity inherited functions
             * 4) no return value
            */

            if (!method.IsPublic || method.IsConstructor
                || Array.IndexOf(DefaultFunctionExclusions, method.Name) != -1
                || method.ReturnType != typeof(void))
            {
                continue;
            }

            if (!m_EventFunctionNames.Contains(method.Name))
            {
                m_EventFunctionNames.Add(method.Name);
                m_MethodInfos.Add(method);
            }
        }
    }

    /// <summary>
    /// Loads all the function names that are available on the given component
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="ce"></param>
    /// <param name="resetData"></param>
    void GatherComponentFunctionNames(Component behaviour, CutsceneEvent ce, bool resetData)
    {
        if (behaviour == null || ce == null)
        {
            return;
        }

        MethodInfo[] methods = null;
        if (GenericEventNames.IsCustomEvent(ce.EventType))
        {
            // custom events have their methods collected in a different way, directly off the behaviour itself
            methods = behaviour.GetType().GetMethods();
        }
        else
        {
            GenericEvents ge = ((GenericEvents)behaviour);
            methods = ge.EventMethods;
            if (methods.Length == 0)
            {
                ge.CheckAvailableEvents();
                methods = ge.EventMethods;
            }
        }

        SetupComponentMethods(methods);

        // fill out the parameters
        if (m_MethodInfos != null && m_MethodInfos.Count > 0 && resetData)
        {
            ce.FunctionName = m_MethodInfos[0].Name;
            GatherComponentFunctionParams(behaviour, m_MethodInfos[0].Name, ce);
        }
    }

    /// <summary>
    /// Loads all the parameters associated with provided function name 
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="functionName"></param>
    /// <param name="ce"></param>
    void GatherComponentFunctionParams(Component behaviour, string functionName, CutsceneEvent ce)
    {
        MethodInfo[] functionOverloads = ce.GetFunctionOverloads(behaviour, functionName);
        ce.GatherEventParams(behaviour, functionName, 0);
        GatherEventLengthOptions(ce);
        GatherFunctionOverloads(functionName, functionOverloads);
    }

    /// <summary>
    /// Returns all the available overloads of the provided function
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="functionName"></param>
    /// <param name="ce"></param>
    /// <returns></returns>
    MethodInfo[] GetFunctionOverloads(Component behaviour, string functionName, CutsceneEvent ce)
    {
        MethodInfo[] functionOverloads = null;
        if (GenericEventNames.IsCustomEvent(ce.EventType))
        {
            functionOverloads = Array.FindAll<MethodInfo>(behaviour.GetType().GetMethods(), m => m.Name == functionName);
        }
        else
        {
            functionOverloads = ((GenericEvents)behaviour).GetEventMethodOverloads(functionName);
        }

        if (functionOverloads == null)
        {
            Debug.LogError(string.Format("{0} doesn't have a function named {1}", behaviour.name, functionName));
        }

        return functionOverloads;
    }

    /// <summary>
    /// Loads the available parameters that can be used as length definers for the event on the timeline
    /// </summary>
    /// <param name="ce"></param>
    void GatherEventLengthOptions(CutsceneEvent ce)
    {
        GatherEventLengthOptions(ce.TargetComponent, ce.FunctionName, ce.FunctionOverloadIndex, ce.LengthDefiningParamName, ce);
    }

    /// <summary>
    /// Loads the avilable parameters that can be used as length definers for the event on the timeline
    /// </summary>
    /// <param name="behaviour"></param>
    /// <param name="functionName"></param>
    /// <param name="overloadIndex"></param>
    /// <param name="lengthDefiningParamName"></param>
    /// <param name="ce"></param>
    void GatherEventLengthOptions(Component behaviour, string functionName, int overloadIndex, string lengthDefiningParamName, CutsceneEvent ce)
    {
        ParameterInfo[] funcParams = ce.GetFunctionParams(behaviour, functionName, overloadIndex, ce.EventType);
        m_EventLengthOptions.Clear();
        m_EventLengthOptions.Add(CutsceneEvent.NoneParameter);

        if (funcParams == null || funcParams.Length == 0)
        {
            // this functions takes no arguements
            return;
        }

        string param = CutsceneEvent.NoneParameter;
        if (GenericEventNames.IsCustomEvent(ce.EventType))
        {
            // it's a custom event so search for specific parameter types
            foreach (ParameterInfo func in funcParams)
            {
                if (func.ParameterType == typeof(int) || func.ParameterType == typeof(float)
                    || func.ParameterType == typeof(AudioClip) || func.ParameterType == typeof(AnimationClip)
                    || func.ParameterType == typeof(Cutscene))
                {
                    m_EventLengthOptions.Add(func.Name);
                }
            }

            m_SelectedEventLengthOption = m_EventLengthOptions.FindIndex(p => p == lengthDefiningParamName);
            if (m_SelectedEventLengthOption == -1)
            {
                m_SelectedEventLengthOption = 0;
            }

            param = m_EventLengthOptions[m_SelectedEventLengthOption];
        }
        else
        {
            param = ((GenericEvents)behaviour).GetLengthDefiningParamFromMethod(functionName);
        }

        ce.SetEventLengthFromParameter(param);
    }

    /// <summary>
    /// Loads all the available overloads of the specified function name
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="methods"></param>
    void GatherFunctionOverloads(string functionName, MethodInfo[] methods)
    {
        m_EventSelectedFunctionOverloadNames.Clear();

        // check for multiple overloads
        MethodInfo[] functionOverloads = Array.FindAll<MethodInfo>(methods, m => m.Name == functionName);
        if (functionOverloads.Length > 1)
        {
            // there are multiple overloads of this function
            foreach (MethodInfo overload in functionOverloads)
            {
                ParameterInfo[] overloadParams = overload.GetParameters();
                string allParams = string.Empty;

                foreach (ParameterInfo overloadParam in overloadParams)
                {
                    string shortenedType = Path.GetExtension(overloadParam.ParameterType.ToString());
                    shortenedType = string.IsNullOrEmpty(shortenedType) ? overloadParam.ParameterType.ToString() : shortenedType.Remove(0, 1);
                    allParams += string.Format("{0}({1}), ", overloadParam.Name, shortenedType);
                }

                if (!string.IsNullOrEmpty(allParams))
                {
                    allParams = allParams.Remove(allParams.Length - 2); // get rid of trailing comma and space
                }
                else
                {
                    allParams = "void";
                }
                
                m_EventSelectedFunctionOverloadNames.Add(allParams);
            }
        }
    }

    /// <summary>
    /// Returns the currently selected cutscene
    /// </summary>
    /// <returns></returns>
    public Cutscene GetSelectedCutscene()
    {
        Cutscene cutscene = null;
        if (m_SelectedIndex >= 0 && m_SelectedIndex < m_TimeObjectNames.Count)
        {
            cutscene = m_TimeObjectManager.GetTimelineObjectByIndex(m_SelectedIndex) as Cutscene;
        }
        else if (m_TimeObjectManager.NumObjects > 0)
        {
            cutscene = m_TimeObjectManager.GetTimelineObjectByIndex(0) as Cutscene;
        }
        return cutscene;
    }

    #region Button Handlers
    /// <summary>
    /// Marks the Unity scene and current cutscene as dirty so that it can be saved
    /// </summary>
    public override void ChangedData()
    {
        Cutscene selectedCutscene = GetSelectedCutscene();
        if (selectedCutscene != null)
        {
            EditorUtility.SetDirty(selectedCutscene);
        }
    }

    /// <summary>
    /// Play or pause the provided cutscenes
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="play"></param>
    void PlayCutscene(Cutscene cutscene, bool play)
    {
        if (cutscene == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            if (play)
            {
                Time.timeScale = m_TimeScale;
                if (cutscene.IsPaused)
                {
                    cutscene.Resume();
                }
                else
                {
                    SetTime(m_CurrentTime);
                    cutscene.Play(m_CurrentTime);
                }
            }
            else
            {
                Time.timeScale = 0;
                cutscene.Pause();
            }
        }

        m_MockPlaying = play;
        m_PreviousTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Stops the provided cutscene with the option of resetting the playhead to the start
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="resetPlayHead"></param>
    override protected void StopCutscene(Cutscene cutscene, bool resetPlayHead)
    {
        if (cutscene == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            cutscene.Reset();
        }

        m_MockPlaying = false;

        if (resetPlayHead)
        {
            m_PreviousTime = 0;
            SetTime(cutscene.StartTime, cutscene);
        }

        if (Application.isPlaying && SmartbodyManager.Get() != null)
        {
            string message = string.Format(@"bml.execBML('{0}', '<locomotion enable=""{1}"" />')", "*", "false");
            SmartbodyManager.Get().PythonCommand(message);
            //message = string.Format("bml.execBML(ChrBrad, '<blend mode=\"schedule\" name=\"NULL\" sbm:schedule-mode=\"Now\" sbm:wrap-mode=\"Once\"/>')");
        }
    }

    /// <summary>
    /// Adds a new cutscene game object to the scene and makes it selectable in the MM. Also handles all initialization work
    /// </summary>
    public void AddCutscene()
    {
        // generate a unique name
        int cutsceneNumber = m_TimeObjectManager.NumObjects;
        string newCutsceneName = string.Empty;
        do
        {
            ++cutsceneNumber;
            newCutsceneName = string.Format("Cutscene{0}", m_TimeObjectManager.NumObjects < 10 ? "0" + cutsceneNumber.ToString() : cutsceneNumber.ToString());
        }
        while (m_TimeObjectManager.GetTimelineObjectByName(newCutsceneName) != null);

        GameObject go = new GameObject(newCutsceneName);

        Cutscene cutscene = go.AddComponent<Cutscene>();
        cutscene.CutsceneName = go.name;
        cutscene.m_GroupManager = go.GetComponent<CutsceneTrackGroupManager>();
        cutscene.GroupManager.SetStartingPosition(m_TrackStartingPosition);
        cutscene.Order = cutsceneNumber;
        //cutscene.GroupManager.SetSelectionColor(TrackSelectionColor);
        // start with 1 track

        GameObject genericEventsObj = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + GenericEventsGOPath, typeof(GameObject)));
        genericEventsObj.transform.parent = cutscene.transform;

        LoadAllTimelineObjects<Cutscene>();

        int prevCutscene = m_SelectedIndex;
        // select the newest on
        m_SelectedIndex = m_TimeObjectManager.GetTimeObjectIndexByName(newCutsceneName);

        CutsceneSelected(prevCutscene);

        AddTrackGroup(GroupManager);
    }

    void ConfirmRemoveCutscene(Cutscene cutscene)
    {
        if (EditorUtility.DisplayDialog("Confirm Removal", string.Format("Are you sure that you want to remove cutscene {0}?", cutscene.CutsceneName), "Yes", "No"))
        {
            RemoveCutscene(cutscene);
        }
    }

    /// <summary>
    /// Deletes the specified cutscene and removes the game object from the scene
    /// </summary>
    /// <param name="cutscene"></param>
    void RemoveCutscene(Cutscene cutscene)
    {
        UnSelectAll();
        DestroyImmediate(cutscene.gameObject);
        LoadAllTimelineObjects<Cutscene>();
    }

    /// <summary>
    /// Asks for confirmation to delete a track group then removes it if the user clicks yes
    /// </summary>
    /// <param name="group"></param>
    override public void RemoveTrackGroup(CutsceneTrackGroup group)
    {
        RemoveTrackGroup(GetSelectedCutscene(), group);
    }

    /// <summary>
    /// Asks for confirmation to delete a track group then removes it if the user clicks yes
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="group"></param>
    public void RemoveTrackGroup(Cutscene cutscene, CutsceneTrackGroup group)
    {
        if (group == null)
        {
            return;
        }

        if (EditorUtility.DisplayDialog("Confirm Track Deletion", string.Format("Deleting track \"{0}\" will remove {1} track(s) and {2} event(s). Are you sure?",
            group.GroupName, cutscene.GroupManager.GetNumChildren(group) + 1, cutscene.GroupManager.GetNumTrackItems(group)), "Yes", "No"))
        {
            if (group == m_SelectedTrackGroup)
            {
                UnSelectTrackGroup();
            }
            cutscene.GroupManager.RemoveGroup(cutscene, group);
            ChangedData();
        }
    }

    override public CutsceneTrackItem DuplicateEvent(CutsceneTrackItem ce)
    {
        return DuplicateEvent(ce, CollisionResolutionType.MoveDown);
    }

    override public CutsceneTrackItem DuplicateEvent(CutsceneTrackItem ce, CollisionResolutionType resolutionType)
    {
        CutsceneEvent newEvent = CreateEventAtPosition(new Vector2(ce.GuiPosition.x, ce.GuiPosition.y), resolutionType) as CutsceneEvent;
        // pass the data to it
        (ce as CutsceneEvent).CloneData(newEvent);
        return newEvent;
    }

    override public CutsceneTrackItem CreateEventAtPosition(Vector2 position, CollisionResolutionType resolutionType)
    {
        CutsceneEvent newEvent = null;
        Cutscene seletedCutscene = GetSelectedCutscene();
        CutsceneTrackGroup containingTrack = seletedCutscene.GroupManager.GetTrackContainingPosition(position + m_TrackScrollPos);
        while (containingTrack == null)
        {
            AddTrackGroup(GroupManager);
            containingTrack = seletedCutscene.GroupManager.GetTrackContainingPosition(position + m_TrackScrollPos);
        }

        Rect eventPosition = containingTrack.TrackPosition;
        eventPosition.x = position.x + m_TrackScrollPos.x;
        eventPosition.y = eventPosition.y;// +m_TrackScrollPos.y;
        eventPosition.width = EventWidth;
        eventPosition.height = TrackHeight;

        // the event doesn't get added to the cutscene here. that happens in the update loop otherwise,
        // we get errors with the guilayout
        newEvent = new CutsceneEvent(eventPosition, Guid.NewGuid().ToString());
        string numEventsText = (seletedCutscene.NumEvents + 1).ToString();
        newEvent.Name = string.Format("Event{0}", seletedCutscene.NumEvents < 9 ? "0" + numEventsText : numEventsText);

        newEvent.GuiPosition.width = newEvent.Name.Length * 8 + 8;//CutsceneTrackItem.CalculateTextLength(m_TooltipContent, newEvent.Name);//EventWidth;

        // it gets added next frame
        m_ItemsToBeAdded.Add(newEvent);
        GroupManager.AddTrackItem(containingTrack, newEvent);

        SetTrackItemTime(newEvent, GetTimeFromScrollPosition(eventPosition), resolutionType);
        newEvent.SetSelected(false);

        // setup a default event function
        ChangedCutsceneEventType(GenericEventNames.Common, newEvent);
        //ChangedEventFunction(newEvent, "Marker");

        ChangedData();
        return newEvent;
    }
    
    /// <summary>
    /// Deletes the provided event from the track, the cutscene, and removes all references
    /// </summary>
    /// <param name="ce"></param>
    override public void RemoveEvent(CutsceneTrackItem ce)
    {
        if (ce != null)
        {
            UnSelectTrackItem(ce);
            CutsceneTrackGroup track = GroupManager.GetTrackContainingPosition(ce.GuiPosition.x, ce.GuiPosition.y);
            GetSelectedCutscene().RemoveEvent(ce as CutsceneEvent);
            GroupManager.RemoveTrackItem(track, ce);
            m_DeletedEvents.Push(ce);
            ChangedData();
        }
    }

    override public void RemoveSelectedEvents()
    {
        Cutscene cutscene = GetSelectedCutscene();
        for (int i = 0; i < cutscene.CutsceneEvents.Count; i++)
        {
            if (cutscene.CutsceneEvents[i].Selected)
            {
                RemoveEvent(cutscene.CutsceneEvents[i]);
                i--;
            }
        }
    }

    public void RemoveEvents(List<CutsceneEvent> events)
    {
        for (int i = 0; i < events.Count; i++)
        {
            RemoveEvent(events[i]);
            i--;
        }
    }

    public CutsceneEvent FindEventByName(string name)
    {
        Cutscene cutscene = GetSelectedCutscene();
        foreach (CutsceneTrackItem ce in m_ItemsToBeAdded)
        {
            if (ce.Name == name)
                return ce as CutsceneEvent;
        }

        foreach (CutsceneEvent ce in cutscene.CutsceneEvents)
        {
            if (ce.Name == name)
                return ce;
        }

        Debug.LogError(string.Format("Couldn't find event {0} in cutscene {1}", name, cutscene.CutsceneName));
        return null;
    }

    public CutsceneEvent FindEventByID(string id)
    {
        Cutscene cutscene = GetSelectedCutscene();
        foreach (CutsceneTrackItem ce in m_ItemsToBeAdded)
        {
            if (ce.UniqueId == id)
                return ce as CutsceneEvent;
        }

        foreach (CutsceneEvent ce in cutscene.CutsceneEvents)
        {
            if (ce.UniqueId == id)
                return ce;
        }

        Debug.LogError(string.Format("Couldn't find event {0} in cutscene {1}", id, cutscene.CutsceneName));
        return null;
    }
    #endregion

    #region Time Calculations
    /// <summary>
    /// Increment the current time of the current cutscene by the provided time
    /// </summary>
    /// <param name="time"></param>
    public override void OffsetTime(float time) 
    {
        OffsetTime(time, GetSelectedCutscene());
    }

    /// <summary>
    /// Increment the current time of the current cutscene by the provided time
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="cutscene"></param>
    public void OffsetTime(float offset, Cutscene cutscene)
    {
        SetTime(m_CurrentTime + offset, cutscene);
    }

    /// <summary>
    /// Sets the current time of the cutscene to the provided time
    /// </summary>
    /// <param name="time"></param>
    override public void SetTime(float time)
    {
        SetTime(time, GetSelectedCutscene());
    }

    /// <summary>
    /// Sets the current time of the cutscene to the provided time
    /// </summary>
    /// <param name="time"></param>
    /// <param name="selectedCutscene"></param>
    public void SetTime(float time, Cutscene selectedCutscene)
    {
        m_CurrentTime = time;
        m_CurrentTime = Mathf.Clamp(m_CurrentTime, selectedCutscene.StartTime, selectedCutscene.EndTime);
        selectedCutscene.SetLocalTime(m_CurrentTime - selectedCutscene.StartTime);
    }
    #endregion

    #endregion

    #region Selection Functions
    /// <summary>
    /// This function should only be used in this class and in the update loop. See comment in Update() 
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="unselectPrevious"></param>
    override protected void INTERNAL_SelectTrackItem(CutsceneTrackItem trackItem, bool unselectPrevious)
    {
        if (unselectPrevious)
        {
            UnSelectAll();
        }

        if (IsEventSelected(trackItem))
        {
            return;
        }

        CutsceneEvent ce = trackItem as CutsceneEvent;

        m_SelectedDataViewTab = DataView.Event;
        ce.SetSelected(true);

        //m_SelectedEvents.Add(trackItem);
        if (ce.TargetComponent != null)
        {
            // update the gui with the newly selected event's information
            GatherGameObjectComponentNames(ce.TargetGameObject);
            GatherComponentFunctionNames(ce.TargetComponent, ce, false);
            GatherFunctionOverloads(ce.FunctionName, GetFunctionOverloads(ce.TargetComponent, ce.FunctionName, ce));
            GatherEventLengthOptions(ce);

            m_SelectedEventFunction = Mathf.Max(m_EventFunctionNames.FindIndex(name => name == ce.FunctionName), 0);
        }

        try
        {
            GUI.FocusControl(EventControlName + trackItem.UniqueId);
        }
        catch { }
    }

    /// <summary>
    /// Setups up all the necessrary data for when a new cutscene is selected. Clears selected events,
    /// unselects track groups, etc
    /// </summary>
    /// <param name="previouslySelected"></param>
    public void CutsceneSelected(int previouslySelected)
    {
        Cutscene prevCutscene = m_TimeObjectManager.GetTimelineObjectByIndex(previouslySelected) as Cutscene;
        if (prevCutscene != null)
        {
            PlayCutscene(prevCutscene, false);

            if (Application.isPlaying)
            {
                // stop the run-time manager from starting new cutscenes
                CutsceneManager manager = (CutsceneManager)GameObject.FindObjectOfType(typeof(CutsceneManager));
                if (manager != null)
                {
                    manager.StopAllCoroutines();
                }
            }
        }
        // multiple could be playing at once, stop them all
        m_TimeObjectManager.StopAllCutscenes();

        UnSelectAll();
        UnSelectTrackGroup();

        // show the cutscene data tab
        m_SelectedDataViewTab = DataView.Cutscene;

        Cutscene selectedCutscene = GetSelectedCutscene();
        GatherEventTypes(selectedCutscene);
        AttachAllEventsToTracks(selectedCutscene);
        SetTime(selectedCutscene.StartTime);
        m_TrackScrollPos = Vector2.zero;
    }

    /// <summary>
    /// Makes sure all events are attached to a track
    /// </summary>
    /// <param name="cutscene"></param>
    void AttachAllEventsToTracks(Cutscene cutscene)
    {
        cutscene.AttachEventsToTracks();
    }

    /// <summary>
    /// Returns the length of the currently selected cutscene
    /// </summary>
    /// <returns></returns>
    public override float CalculateTimelineLength()
    {
        float length = 1;
        foreach (CutsceneTrackItem trackItem in m_ItemsToBeAdded)
        {
            if (trackItem.EndTime - StartTime > length)
            {
                length = trackItem.EndTime - StartTime;
            }
        }
        Cutscene cutscene = GetSelectedCutscene();

        cutscene.Length = Mathf.Max(length, cutscene.CalculateCutsceneLength());
        return cutscene.Length;
    }

    /// <summary>
    /// Returns the event that contains the given point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public override CutsceneTrackItem GetTrackItemContainingPoint(Vector2 point)
    {
        return GetSelectedCutscene().GetCutsceneEventContainingPoint(point);
    }
    #endregion

    #region Callbacks
    void ShowFileContextMenu()
    {
        GenericMenu contextMenu = new GenericMenu();
        if (Application.isPlaying)
        {
            contextMenu.AddDisabledItem(new GUIContent("New Cutscene"));
        }
        else
        {
            contextMenu.AddItem(new GUIContent("New Cutscene"), false, AddCutscene);
        }

        if (Application.isPlaying)
        {
            contextMenu.AddDisabledItem(new GUIContent("Delete Cutscene"));
        }
        else
        {
            contextMenu.AddItem(new GUIContent("Delete Cutscene"), false, ConfirmRemoveCutsceneCB, GetSelectedCutscene());
        }
        
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Verbose Events"), m_VerboseText, ToggledVerboseText, this);
        contextMenu.AddItem(new GUIContent("Delete Events"), false, ConfirmRemoveAllEventsCB, GetSelectedCutscene());
        contextMenu.AddItem(new GUIContent("Refresh Events"), false, RefreshEventsCB, GetSelectedCutscene());
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Sequencer/Open BML-XML Pair"), false, RequestFileOpenBMLXMLPair, this);
        contextMenu.AddItem(new GUIContent("Sequencer/Open BML"), false, RequestFileOpenBML, this);
        contextMenu.AddItem(new GUIContent("Sequencer/Open XML"), false, RequestFileOpenXML, this);
        contextMenu.AddSeparator("Sequencer/");
        contextMenu.AddItem(new GUIContent("Sequencer/Save XML As"), false, FileSaveXMLAs, GetSelectedCutscene());
        contextMenu.AddItem(new GUIContent("Sequencer/Batch Export"), false, FileBatchExport, this);
        contextMenu.AddSeparator("Sequencer/");

        if (Application.isPlaying)
        {
            contextMenu.AddItem(new GUIContent("Sequencer/Listen to NVBG"), m_ListenToNVBG, ListenToNVBG, this);
        }
        else
        {
            contextMenu.AddDisabledItem(new GUIContent("Sequencer/Listen to NVBG"));
        }
        
        contextMenu.ShowAsContext();
    }

    void ToggledVerboseText(object data)
    {
        m_VerboseText = !m_VerboseText;
    }

    void ConfirmRemoveCutsceneCB(object data)
    {
        ConfirmRemoveCutscene((Cutscene)data);
    }

    void ConfirmRemoveAllEventsCB(object data)
    {
        Cutscene cutscene = (Cutscene)data;
        if (EditorUtility.DisplayDialog("Confirm Removal", string.Format("Are you sure that you want to remove ALL EVENTS from cutscene {0}?", cutscene.CutsceneName), "Yes", "No"))
        {
            RemoveEvents(cutscene.CutsceneEvents);
        }
    }

    void RefreshEventsCB(object data)
    {
        Cutscene cutscene = (Cutscene)data;
        foreach (CutsceneEvent ce in cutscene.CutsceneEvents)
        {
            UpdateCutsceneEventLength(ce);
        }
    }

    void RequestFileNew(object data)
    {
        m_CleanRequestState = CleanRequest.New;
        Repaint();
    }

    void RequestFileOpenBMLXMLPair(object data)
    {
        m_SequencerData.m_XmlTitle = EditorUtility.OpenFilePanel("Open XML File", string.Format("{0}", Application.dataPath), "xml");
        if (string.IsNullOrEmpty(m_SequencerData.m_XmlTitle))
        {
            return;
        }

        m_CleanRequestState = CleanRequest.OpenBoth;
    }

    public void RequestFileOpenBMLXMLPair(string filename)
    {
        m_SequencerData.m_XmlTitle = filename;
        m_CleanRequestState = CleanRequest.OpenBoth;
    }

    void RequestFileOpenBML(object data)
    {
        m_SequencerData.m_XmlTitle = EditorUtility.OpenFilePanel("Open BML File", string.Format("{0}", Application.dataPath), "bml");
        if (string.IsNullOrEmpty(m_SequencerData.m_XmlTitle))
        {
            return;
        }

        m_CleanRequestState = CleanRequest.OpenBml;
    }

    public void RequestFileOpenBML(string filename)
    {
        m_SequencerData.m_XmlTitle = filename;
        m_CleanRequestState = CleanRequest.OpenBml;
    }

    void RequestFileOpenXML(object data)
    {
        m_SequencerData.m_XmlTitle = EditorUtility.OpenFilePanel("Open XML File", string.Format("{0}", Application.dataPath), "xml");
        if (string.IsNullOrEmpty(m_SequencerData.m_XmlTitle))
        {
            return;
        }

        m_CleanRequestState = CleanRequest.OpenXml;
    }

    public void RequestFileOpenXML(string filename)
    {
        m_SequencerData.m_XmlTitle = filename;
        m_CleanRequestState = CleanRequest.OpenXml;
    }

    void FileOpenBMLXMLPair(string filePathAndName)
    {
        m_CreateBMLEvents = false;
        LoadFile(filePathAndName, false);
        m_CreateBMLEvents = true;
        LoadFile(Path.ChangeExtension(filePathAndName, ".bml"), true);
    }

    void FileOpenBML(string filePathAndName)
    {
        m_CreateBMLEvents = true;
        LoadFile(filePathAndName, true);
    }

    void FileOpenXML(string filePathAndName)
    {
        m_CreateBMLEvents = false;
        LoadFile(filePathAndName, true);
    }

    void ListenToNVBG(object data)
    {
        m_ListenToNVBG = !m_ListenToNVBG;
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Play the scene", "You can't listen to NVBG if the scene isn't playing", "Ok");
            m_ListenToNVBG = false;
            return;
        }

        SequencerIO.ListenToNVBG(m_ListenToNVBG);
    }

    void LoadFile(string filePathAndName, bool setTimes)
    {
        if (SequencerIO.LoadFile(filePathAndName) && Path.GetExtension(filePathAndName) == ".xml")
        {
            m_SequencerData.m_XmlTitle = filePathAndName;
        }

        if (setTimes)
        {
            foreach (CutsceneTrackItem ti in m_ItemsToBeAdded)
            {
                ti.GuiPosition.x = GetPositionFromTime(ti.StartTime);
                ti.GuiPosition.width = GetWidthFromTime(EndTime, ti.GuiPosition.x);
                SetTrackItemTime(ti, GetTimeFromScrollPosition(ti.GuiPosition), CollisionResolutionType.MoveDown);
            }
        }

        string oldName = GetSelectedCutscene().CutsceneName;
        string newName = Path.GetFileNameWithoutExtension(filePathAndName);
        while (m_TimeObjectManager.GetTimelineObjectByName(newName) != null)
        {
            newName += '0';
        }
        ChangeCutsceneName(oldName, newName);
        
        Focus();
    }

    void FileSaveXML(object data)
    {
        //if (string.IsNullOrEmpty(m_SequencerData.m_XmlTitle))
        //{
        //    FileSaveXMLAs(null);
        //}
    }

    void FileSaveXMLAs(object data)
    {
        Cutscene cutscene = (Cutscene)data;
        string filePathAndName = EditorUtility.SaveFilePanelInProject("Save Xml", cutscene.CutsceneName, "xml", "Save Xml");
        if (!string.IsNullOrEmpty(filePathAndName))
        {
            SaveCutsceneXML(filePathAndName, cutscene);
            m_SequencerData.m_XmlTitle = filePathAndName;
        }
    }

    void FileBatchExport(object data)
    {
        string folderName = EditorUtility.SaveFolderPanel("Batch Export Xml", "", "");
        if (!string.IsNullOrEmpty(folderName))
        {
            foreach (TimelineObject tlo in TimelineObjectManager.GetAllTimelineObjects())
            {
                SaveCutsceneXML(string.Format("{0}/{1}.xml", folderName, tlo.NameIdentifier), tlo as Cutscene);
            }
        }
    }

    void SaveCutsceneXML(string filePathAndName, Cutscene cutscene)
    {
        SequencerIO.CreateXml(filePathAndName, cutscene.CutsceneEvents);
    }
    #endregion
    #endregion
}


/*void SwitchViews(ViewTypes newView)
    {
        if (newView == ViewTypes.Sceneline)
        {
            SetupScenelineData();
            UnSelectAll();
        }
    }

    void SetupScenelineData()
    {
        GatherScenelineTimes();

        SetupTrackPositions(m_ScenelineViewData.m_LastEndTime - m_ScenelineViewData.m_FirstStartTime);

        m_ScenelineViewData.m_TrackGroups.Clear();
        foreach (Cutscene cutscene in m_TimeObjectManager.GetAllTimelineObjects())
        {
            // create a track group and track item for each cutscene in this unity scene.
            Rect pos = m_TrackStartingPosition;
            pos.y = TrackStartingY + m_ScenelineViewData.m_TrackGroups.Count * TrackHeight;
            pos.width = Mathf.Max(m_TrackScrollArea.width, m_TrackArea.width); ;
            CutsceneTrackGroup group = new CutsceneTrackGroup(pos, string.Format("{0}", cutscene.CutsceneName), false);
            group.AddTrackItem(new CutsceneTrackItem(Guid.NewGuid().ToString(), cutscene.CutsceneName,
                cutscene.StartTime, cutscene.EndTime, pos.y, pos.height, true, CutsceneTrackItem.NotSelectedColor));
            m_ScenelineViewData.m_TrackGroups.Add(group);
        }
    }

    public void GatherScenelineTimes()
    {
        //// setup values and store them so that I can configure the timeline
        //m_ScenelineViewData.m_FirstStartTime = m_TimeObjectManager.GetFirstStartTime();
        //m_ScenelineViewData.m_LastEndTime = m_TimeObjectManager.GetLastCutsceneEndTime();
        //m_ScenelineViewData.m_LongestLength = m_TimeObjectManager.GetLongestCutsceneLength();
    }*/