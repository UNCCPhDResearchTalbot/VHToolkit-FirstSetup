using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
public class TimelineWindow : EditorWindow
{
    #region Constants
    public const float VerticalTimeLineVisibleWidth = 1;
    public const float VerticalTimeLineGrabWidth = 20;

    public class EventSelectionData
    {
        public CutsceneTrackItem ItemToSelect;
        public bool UnselectPreviousItems;
        public EventSelectionData(CutsceneTrackItem itemToSelect, bool unselectPreviousItems)
        {
            ItemToSelect = itemToSelect;
            UnselectPreviousItems = unselectPreviousItems;
        }
    }

    public enum CollisionResolutionType
    {
        None,
        Snap,
        MoveDown,
        Collision_Test,
    }

    // Track groups
    public const float SpaceBetweenFoldouts = 4.5f;
    public const float SpaceBetweenLabels = 2;
    public const int TrackGroupIndent = 20;

    // tool bar
    public const float ViewTypeButtonWidth = 100;
    public const float PlayButtonWidth = 20;
    public const float PointerModeButtonWidth = 40;
    public const float ZoomSliderWidth = 200;
    public const float PlaySpeedSliderWidth = 125;
    public const float MinZoom = 0.3f;
    public const float MaxZoom = 10.0f;
    public const float ToolBarHeight = 20;
    public const float FileButtonWidth = 30;

    // scene view
    public const float CutsceneListWidth = 200;

    // cutscene view
    public const float CutsceneSelectionBoxWidth = 150;
    public const float CutsceneEventsListWidth = 0;
    public const float CutsceneTrackGroupsListWidth = 230;
    public const float EventNameColumnWidth = 100;
    public const float EventGroupColumnWidth = 100;
    public const float EventDataWidth = 300;
    public const float VerticalScrollBarWidth = 16;
    public const float HorizontalScrollBarHeight = 16;
    public const float NoteBoxHeight = 100;

    // timeline
    public const float TimeSliderHeight = 20;
    public const int NumTimeDivisions = 10;
    public const float PixelSpaceBetweenTimeIntervals = 50;
    public const float DefaultTimeDivisionInterval = 1; //seconds
    public const float TimeLineHeight = 20;
    public const float TimeLineStartingY = ToolBarHeight + TimeSliderHeight;

    // Track Groups
    public const float GroupStartingXOffset = 20;
    public const float GroupStartingX = CutsceneEventsListWidth + GroupStartingXOffset;

    // Tracks
    public const float TrackStartingX = CutsceneEventsListWidth + CutsceneTrackGroupsListWidth + 10;
    public const float TrackStartingY = ToolBarHeight + TimeSliderHeight + TimeLineHeight;
    public const float ScrollStartingY = TrackStartingY - TimeSliderHeight - TimeLineHeight;
    public const float TrackWidthSubtractor = TrackStartingX + EventDataWidth + 10;
    public const float TrackHeightSubtractor = ScrollStartingY;
    public const float TrackHeight = 20;
    public const float DefaultScrollWidth = 1000;
    public const float DefaultScrollHeight = 10000;
    public const float EventWidth = 10;
    public const float ScrollerWidth = 15;

    // Events
    public static Color AlphaWhite = new Color(1, 1, 1, 0.5f);
    public static Color SelectionBoxColor = new Color(46f / 255f, 185f / 255f, 215f / 255f);
    public static Color Transparent = new Color(1, 1, 1, 0);
    public const string EventControlName = "EventControlHeader";
    #endregion

    #region Variables
    protected GUIContent m_TooltipContent = new GUIContent();
    protected float m_Zoom = 1;
    public CutsceneTrackGroupManager m_TrackGroupManager;
    //public Sequence m_EventManager;
    //protected List<CutsceneTrackItem> m_SelectedEvents = new List<CutsceneTrackItem>();
    public string MachinimaFolderLocation = string.Empty;
    public string UnitySequencerFolderLocation = string.Empty;

    protected Vector2 m_CutsceneViewDataList;
    protected Vector2 m_CutsceneEventDataScrollPos;
    protected bool m_ShowNotes = true;

    // Textures
    public Texture FoldOutExpandedTex;
    public Texture FoldOutNotExpandedTex;
    public Texture TimeSliderVerticalLine;
    public Texture TimeSliderArrow;
    public Texture VerticalTimeLine;
    public Texture PlayButton;
    public Texture ResetButton;
    public Texture StopButton;

    // time line
    public Rect m_TimeNumberPosition = new Rect();
    public Rect m_TimeSliderPosition = new Rect();
    public Rect m_CurrentTimeTextPosition = new Rect();
    public Rect m_VerticalTimeLinePosition = new Rect();
    public Rect m_VerticalTimeLineGrabArea = new Rect();
    public Rect m_ClickableTimeLineArea = new Rect();

    // Tracks
    public Vector2 m_TrackScrollPos;
    public float m_TimeSliderTime;
    public Rect m_TrackStartingPosition = new Rect(TrackStartingX, TrackStartingY, 1, TrackHeight);
    public Rect m_TrackArea = new Rect();
    public Rect m_TrackScrollArea = new Rect();
    public Rect m_AllTrackPositions = new Rect();
    public Rect m_TrackBackground = new Rect();

    // groups (left-hand side)
    public Vector2 m_TrackGroupScrollPos;
    public Rect m_TrackGroupArea = new Rect();
    public Rect m_TrackGroupScrollArea = new Rect();
    public Rect m_SelectedTrackGroupArea = new Rect(GroupStartingX, 0, CutsceneTrackGroupsListWidth - 10, TrackHeight);
    public Rect m_TrackGroupDropArea = new Rect(GroupStartingX, 0, CutsceneTrackGroupsListWidth - 10, TrackHeight);
    public float m_MoveToTime;

    // lists
    public List<CutsceneTrackItem> m_ItemsToBeAdded = new List<CutsceneTrackItem>();
    protected List<EventSelectionData> m_ItemsToBeSelected = new List<EventSelectionData>();

    // input
    protected TimelineInput m_InputController;
    protected float m_CurrentTime;
    protected float m_PreviousTime;
    protected CutsceneTrackGroup m_SelectedTrackGroup;
    protected CutsceneTrackGroup m_DropLocationTrackGroup;
    protected bool m_VerboseText = true;
    public bool m_MockPlaying;
    public Stack<CutsceneTrackItem> m_DeletedEvents = new Stack<CutsceneTrackItem>(); // used as an undo stack during runtime
    protected int m_SelectedIndex;
    protected List<string> m_TimeObjectNames = new List<string>();
    protected float m_TimeScale = 1;
    #endregion

    #region Properties
    virtual public string SavedWindowPosXKey { get { return ""; } }
    virtual public string SavedWindowPosYKey { get { return ""; } }
    virtual public string SavedWindowWKey { get { return ""; } }
    virtual public string SavedWindowHKey { get { return ""; } }

    virtual public float StartTime 
    { 
        get { return 0; }
        set { }
    }

    virtual public float EndTime 
    { 
        get { return 1; }
        set { }
    }

    virtual public float Length
    {
        get { return 1; }
        set { }
    }

    virtual public EditorTimelineObjectManager TimelineObjectManager
    {
        get { return null; }
    }

    virtual public CutsceneTrackGroupManager GroupManager { get { return null; } }
    public float Zoom 
    {  
        get { return m_Zoom; }
        set { m_Zoom = value; m_Zoom = Mathf.Clamp(m_Zoom, MinZoom, MaxZoom); }
    }
    protected TimelineInput InputController { get { return m_InputController; } }
    public float CurrentTime { get { return m_CurrentTime; } }
    virtual public CutsceneTrackItem SelectedEvent { get { return/* m_SelectedEvents.Count > 0 ? m_SelectedEvents[0] : */null; } }

    virtual public List<CutsceneTrackItem> SelectedEvents
    {
        get { return null; }// m_SelectedEvents; }
    }

    virtual public int NumSelectedEvents
    {
        get { return 0; }// m_SelectedEvents.Count; }
    }

    public List<EventSelectionData> EventsQueuedForSelection
    {
        get { return m_ItemsToBeSelected; }
    }

    public CutsceneTrackGroup SelectedTrackGroup
    {
        get { return m_SelectedTrackGroup; }
    }

    public GUIContent TooltipContent
    {
        get { return m_TooltipContent; }
    }

    public float TimeScale
    {
        get { return m_TimeScale; }
    }

    #endregion

    #region Functions
    protected virtual void OnDestroy()
    {
        SaveLocation();
    }

    void OnFocus()
    {
        Setup();
    }

    public virtual void LoadTextures()
    {
        TimeSliderVerticalLine = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/VerticalTimeSlider.png", typeof(Texture));
        TimeSliderArrow = (Texture2D)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/TimeSliderArrow.png", typeof(Texture2D));
        VerticalTimeLine = (Texture2D)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/VerticalLine.png", typeof(Texture2D));
        FoldOutNotExpandedTex = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/FoldOutArrow_NotExpanded.png", typeof(Texture));
        FoldOutExpandedTex = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/FoldOutArrow_Expanded.png", typeof(Texture));
        PlayButton = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/Button_Play.png", typeof(Texture));
        ResetButton = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/Button_Reset.png", typeof(Texture));
        StopButton = (Texture)AssetDatabase.LoadAssetAtPath(MachinimaFolderLocation + "Gizmos/Button_Stop.png", typeof(Texture));
    }

    protected void SaveLocation()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, position.y);
        PlayerPrefs.SetFloat(SavedWindowWKey, position.width);
        PlayerPrefs.SetFloat(SavedWindowHKey, position.height);
    }

    public virtual void Setup() { }
    protected virtual void Update() { }
    protected virtual void UpdateMockPlay() { }

    #region GUI Functions
    protected virtual void OnGUI() { }

    /// <summary>
    /// Handles the drawing of the scene tabs, play button, options, etc
    /// </summary>
    protected virtual void DrawToolbar() { }

    protected virtual void DrawZoomer()
    {
        if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(PlayButtonWidth)))
        {
            m_Zoom -= 0.1f;
        }
        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(PlayButtonWidth)))
        {
            m_Zoom += 0.1f;
        }
        m_Zoom = EditorGUILayout.Slider(m_Zoom, MinZoom, MaxZoom, GUILayout.Width(ZoomSliderWidth));
    }

    protected virtual void DrawPlaySpeed()
    {
        m_TimeScale = EditorGUILayout.Slider(m_TimeScale, 1, 10, GUILayout.Width(ZoomSliderWidth));
    }
    
    /// <summary>
    /// Draws all track groups and their nested children if they are expanded. Does not use recursion, uses depth first traversal
    /// </summary>
    protected void DrawTrackGroups()
    {
        if (GroupManager == null || GroupManager.NumGroups == 0)
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
            foreach (CutsceneTrackGroup group in GroupManager.m_TrackGroups)
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

    protected void SetupTrackPositions(float cutsceneLength)
    {
        // setup rectangles to be the correct sizes based on the window width/height
        m_TrackArea.Set(TrackStartingX, ScrollStartingY, position.width - TrackWidthSubtractor, position.height - TrackHeightSubtractor);
        m_TrackScrollArea.Set(TrackStartingX, ScrollStartingY, CalculateTrackScrollAreaWidth(cutsceneLength), DefaultScrollHeight);
        m_AllTrackPositions.Set(m_TrackScrollArea.x, TrackStartingY, m_TrackScrollArea.width, m_TrackScrollArea.height);
    }

    /// <summary>
    /// Searchs for all timeline objects of type T in the scene and loads them into the list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void LoadAllTimelineObjects<T>() where T : TimelineObject
    {
        TimelineObjectManager.FindTimelineObjects<T>();
        LoadTimelineObjectNames(TimelineObjectManager.GetAllTimelineObjects());

        if (m_SelectedIndex >= TimelineObjectManager.NumObjects)
        {
            m_SelectedIndex = Mathf.Max(0, TimelineObjectManager.NumObjects - 1);
        }
    }

    /// <summary>
    /// Setups up the drop down list that has all the timeline object names in it
    /// </summary>
    /// <param name="timelineObjects"></param>
    public void LoadTimelineObjectNames(List<TimelineObject> timelineObjects)
    {
        m_TimeObjectNames.Clear();

        for (int i = 0; i < timelineObjects.Count; i++)
        {
            m_TimeObjectNames.Add(timelineObjects[i].NameIdentifier);
        }
    }

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

        bool pressed = trackItem.Draw(m_VerboseText, m_TooltipContent);
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

    protected virtual void SetupTimeSliderData() { }

    protected void DrawTimeSlider(float startTime, float endTime)
    {
        float timelinePosX = GetPositionFromTime(startTime, endTime, m_CurrentTime, m_TrackScrollArea) - 7f; // TODO: fix offset

        m_TimeSliderPosition.Set(timelinePosX, ToolBarHeight + m_TrackScrollPos.y, 15, TrackHeight);

        m_CurrentTimeTextPosition.Set(m_TimeSliderPosition.x + 20, m_TimeSliderPosition.y, 50, TrackHeight);
        m_ClickableTimeLineArea.Set(TrackStartingX, m_TimeSliderPosition.y, m_TrackArea.width - ScrollerWidth, m_TimeSliderPosition.height + TrackHeight);
        GUI.Label(m_CurrentTimeTextPosition, m_CurrentTime.ToString("f2"));
        GUI.DrawTexture(m_TimeSliderPosition, TimeSliderArrow);

        m_VerticalTimeLinePosition.Set(timelinePosX + 7.5f, m_TimeSliderPosition.y, VerticalTimeLineVisibleWidth, m_TrackArea.height - 18);

        // same as the vertical position, but wider so that it's easier to grab with the mouse
        m_VerticalTimeLineGrabArea.Set(m_VerticalTimeLinePosition.x - VerticalTimeLineGrabWidth * 0.5f, m_VerticalTimeLinePosition.y, VerticalTimeLineGrabWidth, m_VerticalTimeLinePosition.height);

        GUI.DrawTexture(m_VerticalTimeLinePosition, TimeSliderVerticalLine);
    }

    protected void DrawTimeLine(float startTime)
    {
        // TODO: on large cutscenes this causes a huge slow down. CLEAN THIS UP
        // always draw the startime time
        DrawTimeInterval(startTime, 0, 0);

        int numIntervals = CalculateNumIntervalsShown(Mathf.Max(m_TrackArea.width, m_TrackScrollArea.width));
        for (int i = 0; i < numIntervals; i++)
        {
            DrawTimeInterval(startTime, i + 1, i + 1);
        }
    }

    protected void DrawTimeInterval(float startTime, int offset, int interval)
    {
        // draw the text
        float posX = TrackStartingX + (float)(offset) * PixelSpaceBetweenTimeIntervals * m_Zoom;
        m_TimeNumberPosition.Set(posX, TimeLineStartingY + m_TrackScrollPos.y, 60, TrackHeight);
        GUI.Label(m_TimeNumberPosition, CalculateScaledTimeDivisionInterval(startTime, interval).ToString("f0"));

        // draw the vertical line
        m_TimeNumberPosition.Set(posX, TrackStartingY + m_TrackScrollPos.y, 1, m_TrackBackground.height);
        GUI.DrawTexture(m_TimeNumberPosition, VerticalTimeLine);
    }

    protected virtual void DrawEventData(List<CutsceneTrackItem> selectedEvents) { }

    #endregion

    #region Data Functions
    #region Button Handlers
    public virtual void ChangedData() { }
    protected virtual void SimulatePlay(Cutscene cutscene, bool play) { }
    protected virtual void StopCutscene(Cutscene cutscene, bool resetPlayHead) { }

    void PauseCutscene(Cutscene cutscene)
    {
        EditorApplication.ExecuteMenuItem("Edit/Pause");
    }

    public void AddTrackGroup()
    {
        GroupManager.AddTrack("Track");
    }

    public void AddTrackGroup(CutsceneTrackGroupManager groupManager)
    {
        groupManager.AddTrack("Track");
        ChangedData();
    }

    virtual public void RemoveTrackGroup(CutsceneTrackGroup group)
    {
        RemoveTrackGroup(GroupManager, group);
    }

    virtual public void RemoveTrackGroup(CutsceneTrackGroupManager groupManager, CutsceneTrackGroup group) { }

    virtual public CutsceneTrackItem DuplicateEvent(CutsceneTrackItem ce)
    {
        return DuplicateEvent(ce, CollisionResolutionType.MoveDown);
    }

    virtual public CutsceneTrackItem DuplicateEvent(CutsceneTrackItem ce, CollisionResolutionType resolutionType)
    {
        CutsceneTrackItem newEvent = CreateEventAtPosition(new Vector2(ce.GuiPosition.x, ce.GuiPosition.y), resolutionType);
        return newEvent;
    }

    public CutsceneTrackItem CreateEventAtPosition(Vector2 position)
    {
        return CreateEventAtPosition(position, CollisionResolutionType.MoveDown);
    }

    public virtual CutsceneTrackItem CreateEventAtPosition(Vector2 position, CollisionResolutionType resolutionType)
    {
        return null;
    }

    virtual public void RemoveSelectedEvents()
    {
        List<CutsceneTrackItem> selected = SelectedEvents;
        for (int i = 0; i < selected.Count; i++)
        {
            selected[i].SetSelected(false);
        }
    }

    virtual public void RemoveEvent(CutsceneTrackItem ce) { }
    #endregion

    #region Time Calculations
    /// <summary>
    /// Shift the list of given events to the specific time while maintaining their time offsets.
    /// The event with the earliest start time will be placed exactly at the provided time
    /// </summary>
    /// <param name="events"></param>
    /// <param name="time"></param>
    public void MoveEventsToTime(List<CutsceneTrackItem> events, float time)
    {
        events.Sort((a, b) => a.StartTime < b.StartTime ? -1 : 1);
        float earliest = events[0].StartTime;
        foreach (CutsceneTrackItem ce in events)
        {
            float currStartTime = ce.StartTime;
            SetTrackItemTime(ce, time + (currStartTime - earliest), CollisionResolutionType.MoveDown);
        }

        ChangedData();
    }

    /// <summary>
    /// Tests to see if one track item is collidiing horizontally with another. If there is a collision
    /// the collider is returned. Null if there is no collision
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="testTime"></param>
    /// <returns></returns>
    public CutsceneTrackItem TestHorizontalCollision(CutsceneTrackItem trackItem, float testTime)
    {
        float prevStartTime = trackItem.StartTime;
        trackItem.StartTime = testTime;
        trackItem.StartTime = Mathf.Max(0, trackItem.StartTime); // can't be less than 0
        Rect prevPos = trackItem.GuiPosition;
        trackItem.GuiPosition.x = GetPositionFromTime(StartTime, EndTime, trackItem.StartTime, m_TrackScrollArea);

        CutsceneTrackItem collider = InputController.GetHorizontalCollider(GroupManager, trackItem);

        // reset the time and position
        trackItem.StartTime = prevStartTime;
        trackItem.GuiPosition = prevPos;

        return collider;
    }

    /// <summary>
    /// Offsets the time of the specified trackitem by the given amount. Returns true if a collision occured
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="offset"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public bool OffsetTrackItemTime(CutsceneTrackItem trackItem, float offset, CollisionResolutionType resolution)
    {
        return SetTrackItemTime(trackItem, trackItem.StartTime + offset, resolution);
    }

    /// <summary>
    /// Sets the time of the specified track item. Returns true if a collision occured
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="time"></param>
    /// <param name="resolution"></param>
    public bool SetTrackItemTime(CutsceneTrackItem trackItem, float time, CollisionResolutionType resolution)
    {
        if (trackItem.Locked)
        {
            return false;
        }

        float prevStartTime = trackItem.StartTime;
        trackItem.StartTime = time;
        trackItem.StartTime = Mathf.Max(0, trackItem.StartTime); // can't be less than 0
        Rect prevPos = trackItem.GuiPosition;
        trackItem.GuiPosition.x = GetPositionFromTime(StartTime, EndTime, trackItem.StartTime, m_TrackScrollArea);

        // update the cutscene length and starttime based on the event's new time
        StartTime = Mathf.Min(StartTime, trackItem.StartTime);
        Length = CalculateTimelineLength();

        bool collision = time < 0 || InputController.IsCollidingHorizontally(GroupManager, trackItem);

        switch (resolution)
        {
            case CollisionResolutionType.Collision_Test:
                if (collision)
                {
                    // we don't want to actually move the track item, we're only checking to see if it's new position
                    // will cause a collision.
                    trackItem.StartTime = prevStartTime;
                    trackItem.GuiPosition = prevPos;
                    return collision;
                }
                break;

            case CollisionResolutionType.Snap:
                if (collision)
                {
                    trackItem.StartTime = prevStartTime;
                    trackItem.GuiPosition = prevPos;
                }
                break;

            case CollisionResolutionType.MoveDown:
                if (collision)
                {
                    InputController.ResolveHorizontalCollision(GroupManager, trackItem);
                }
                break;
        }

        if (trackItem == SelectedEvent)
        {
            // only scroll based off the event that is selected.  This prevents weird jumping when moving an event group
            InputController.CheckHorizontalScroll(trackItem.GuiPosition, prevStartTime < trackItem.StartTime);
        }

        return collision;
    }

    /// <summary>
    /// Helper for drawing vertical time line intervals on the time line
    /// </summary>
    /// <param name="cutsceneLength"></param>
    /// <returns></returns>
    public float CalculateTrackScrollAreaWidth(float cutsceneLength)
    {
        return cutsceneLength * PixelSpaceBetweenTimeIntervals * m_Zoom;
    }

    /// <summary>
    /// Helper for drawing vertical time line intervals on the time line
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public float CalculateScaledTimeDivisionInterval(float startTime, int interval)
    {
        return startTime + DefaultTimeDivisionInterval * (float)interval;
    }

    /// <summary>
    /// Returns the number of vertical time line intervals that should be shown based on zoom and pixel space between them
    /// </summary>
    /// <param name="trackAreaWidth"></param>
    /// <returns></returns>
    public int CalculateNumIntervalsShown(float trackAreaWidth)
    {
        return (int)((trackAreaWidth - VerticalScrollBarWidth) / (PixelSpaceBetweenTimeIntervals * m_Zoom));
    }

    /// <summary>
    /// Returns the timeline time based on a gui position in the timeline
    /// </summary>
    /// <param name="itemPosition"></param>
    /// <returns></returns>
    public float GetTimeFromScrollPosition(Vector2 itemPosition)
    {
        return GetTimeFromScrollPosition(new Rect(itemPosition.x, itemPosition.y, 1, 1));
    }

    /// <summary>
    /// Returns the timeline time based on a gui position in the timeline
    /// </summary>
    /// <param name="itemPosition"></param>
    /// <returns></returns>
    public float GetTimeFromScrollPosition(Rect itemPosition)
    {
        return GetTimeFromScrollPosition(StartTime, EndTime, m_TrackScrollArea, itemPosition);
    }

    /// <summary>
    /// Returns the timeline time based on a gui position in the timeline
    /// </summary>
    /// <param name="cutsceneStartTime"></param>
    /// <param name="cutsceneEndTime"></param>
    /// <param name="scollerPosition"></param>
    /// <param name="itemPosition"></param>
    /// <returns></returns>
    public float GetTimeFromScrollPosition(float cutsceneStartTime, float cutsceneEndTime, Rect scollerPosition, Rect itemPosition)
    {
        // I don't use unity's lerp because it clamps time between 0 and 1
        return cutsceneStartTime + (cutsceneEndTime - cutsceneStartTime) * (itemPosition.x - scollerPosition.x) / (scollerPosition.width);
    }

    /// <summary>
    /// Get the x pixel coordinate of a track item base on it start time on the timeline
    /// </summary>
    /// <param name="trackItemStartTime"></param>
    /// <returns></returns>
    public float GetPositionFromTime(float trackItemStartTime)
    {
        return GetPositionFromTime(StartTime, EndTime, trackItemStartTime, m_TrackScrollArea);
    }

    /// <summary>
    /// Get the x pixel coordinate of a track item base on it start time on the timeline
    /// </summary>
    /// <param name="cutsceneStartTime"></param>
    /// <param name="cutsceneEndTime"></param>
    /// <param name="trackItemStartTime"></param>
    /// <param name="scollerPosition"></param>
    /// <returns></returns>
    public float GetPositionFromTime(float cutsceneStartTime, float cutsceneEndTime, float trackItemStartTime, Rect scollerPosition)
    {
        return Mathf.Lerp(scollerPosition.x, scollerPosition.x + scollerPosition.width, (trackItemStartTime - cutsceneStartTime) / (Mathf.Max(cutsceneEndTime, 1) - cutsceneStartTime));
    }

    /// <summary>
    /// Get the gui width in pixels of a track item based on it's duration on the timeline
    /// </summary>
    /// <param name="trackItemEndTime"></param>
    /// <param name="trackItemPosX"></param>
    /// <returns></returns>
    public float GetWidthFromTime(float trackItemEndTime, float trackItemPosX)
    {
        return GetWidthFromTime(StartTime, EndTime, trackItemEndTime, trackItemPosX, m_TrackScrollArea);
    }

    /// <summary>
    /// Get the gui width in pixels of a track item based on it's duration on the timeline
    /// </summary>
    /// <param name="cutsceneStartTime"></param>
    /// <param name="cutsceneEndTime"></param>
    /// <param name="trackItemEndTime"></param>
    /// <param name="trackItemPosX"></param>
    /// <param name="scrollerPosition"></param>
    /// <returns></returns>
    public float GetWidthFromTime(float cutsceneStartTime, float cutsceneEndTime, float trackItemEndTime, float trackItemPosX, Rect scrollerPosition)
    {
        return GetPositionFromTime(cutsceneStartTime, cutsceneEndTime,
                    trackItemEndTime, scrollerPosition) - trackItemPosX;
    }

    public virtual void OffsetTime(float time) { }
    public virtual void SetTime(float time) { }

    #endregion

    #endregion

    #region Selection Functions
    /// <summary>
    /// Returns true if the given event is selected
    /// </summary>
    /// <param name="trackItem"></param>
    /// <returns></returns>
    public bool IsEventSelected(CutsceneTrackItem trackItem)
    {
        return trackItem.Selected;
        //return m_SelectedEvents.FindIndex(ce => ce.UniqueId == trackItem.UniqueId) != -1;
    }

    /// <summary>
    /// Selects/highlights a track item and allows you to see the data associated with it
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="unselectPrevious"></param>
    public void SelectTrackItem(CutsceneTrackItem trackItem, bool unselectPrevious)
    {
        m_ItemsToBeSelected.Add(new EventSelectionData(trackItem, unselectPrevious));
    }

    /// <summary>
    /// This function should only be used in this class and in the update loop. See comment in Update() 
    /// </summary>
    /// <param name="trackItem"></param>
    /// <param name="unselectPrevious"></param>
    protected virtual void INTERNAL_SelectTrackItem(CutsceneTrackItem trackItem, bool unselectPrevious) { }

    public void SelectTrackGroup(CutsceneTrackGroup group)
    {
        m_SelectedTrackGroupArea.y = group.GroupNamePosition.y;
        group.SelectTrack(true);

        if (m_SelectedTrackGroup != group)
        {
            // new group selected, unselect the previous
            UnSelectTrackGroup();
            m_SelectedTrackGroup = group;
        }
    }

    public void SetTrackGroupDropLocation(CutsceneTrackGroup group)
    {
        m_TrackGroupDropArea.y = group.GroupNamePosition.y;
        m_DropLocationTrackGroup = group;
        Repaint();
    }

    /// <summary>
    /// Unselects all track items and clears the selected event list
    /// </summary>
    public void UnSelectAll()
    {
        List<CutsceneTrackItem> selected = SelectedEvents;
        foreach (CutsceneTrackItem item in selected)
        {
            UnSelectTrackItem(item);
        }
        //for (int i = 0; i < m_SelectedEvents.Count; i++)
        //{
        //    UnSelectTrackItem(m_SelectedEvents[i]);

        //    // unselect track item removes from this list
        //    i--;
        //}
        //m_SelectedEvents.Clear();

        try
        {
            // for catching errors in unity 3.5
            GUI.FocusControl("");
        }
        catch { }
    }

    /// <summary>
    /// Unselects the specific track item if it is selected
    /// </summary>
    /// <param name="trackItem"></param>
    virtual public void UnSelectTrackItem(CutsceneTrackItem trackItem)
    {
        if (trackItem != null)
        {
            trackItem.SetSelected(false);
        }

        // search for it based on its unique id
        /*int index = m_SelectedEvents.FindIndex(ce => ce.UniqueId == trackItem.UniqueId);
        if (index != -1)
        {
            m_SelectedEvents.RemoveAt(index);
        }*/

        // I MIGHT NEED THIS!!
        /*if (m_SelectedEvents.Count > 0)
        {
            try
            {
                GUI.FocusControl(EventControlName + m_SelectedEvents[0].UniqueId);
            }
            catch { }
        }*/
    }

    /// <summary>
    /// Unsselects the currently selected track group
    /// </summary>
    public void UnSelectTrackGroup()
    {
        if (m_SelectedTrackGroup != null)
        {
            m_SelectedTrackGroup.SelectTrack(false);
            m_SelectedTrackGroup.EditingName = false;
        }
        m_SelectedTrackGroup = null;
    }

    /// <summary>
    /// Clears gui drawing associated the track group drop area
    /// </summary>
    public void ClearDropArea()
    {
        m_DropLocationTrackGroup = null;
        Repaint();
    }

    public virtual void AttachAllEventsToTracks() { }
    public virtual float CalculateTimelineLength() {  return 10; }
    public virtual CutsceneTrackItem GetTrackItemContainingPoint(Vector2 point) { return null; }
    #endregion
    #endregion
}
