using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TimelineInput
{
    #region Constants
    const float MouseDraggingSpeed = 0.02f;
    const float KeyboardMovementSpeed = 0.1f;
    static Rect WholeScreen = new Rect(-10000f, -10000f, 20000f, 20000f);
    
    public enum PointerModes
    {
        Selection,
        Event,
        NUM_MODES
    }

    public enum DragType
    {
        None,
        TimeSlider,
        TrackGroup_Attempt,
        TrackGroup,
        TrackItem_Attempt,
        TrackItem,
        GroupSelect_Attempt,
        GroupSelect,
    }

    public enum SelectedArea
    {
        TrackGroup,
        Timeline,
        EventData,
    }

    protected delegate bool IsContextOptionEnabledCallback();
    protected struct ContextMenuOption
    {
        public string Name;
        public bool On;
        public bool FollowedBySeperator;
        public GenericMenu.MenuFunction2 OptionCallback;
        public IsContextOptionEnabledCallback OptionAvailableCallback;

        public ContextMenuOption(string name, bool on, GenericMenu.MenuFunction2 optionCallback,
            bool followedBySeperator, IsContextOptionEnabledCallback optionAvailableCallback)
        {
            Name = name;
            On = on;
            FollowedBySeperator = followedBySeperator;
            OptionCallback = optionCallback;
            OptionAvailableCallback = optionAvailableCallback;
        }
    }
    #endregion

    #region Variables
    protected TimelineWindow editor;
    CutsceneTrackItem.DragLocation m_TrackItemDragLocation;
    PointerModes m_PointerMode = PointerModes.Selection;
    List<CutsceneTrackItem> m_CopiedEvents = new List<CutsceneTrackItem>();
    List<ContextMenuOption> m_EventContextOptions = new List<ContextMenuOption>();
    List<ContextMenuOption> m_TrackContextOptions = new List<ContextMenuOption>();
    List<Vector2> m_MouseHandleOffsets = new List<Vector2>();
    DragType m_DragType = DragType.None;
    Vector2 m_MouseDownPos;
    protected Rect m_RubberBandSelectionArea = new Rect();
    bool m_CanRenameTrackGroup;
    SelectedArea m_SelectedArea = SelectedArea.Timeline;
    #endregion

    #region Properties
    public bool IsRubberBandSelecting
    {
        get { return m_DragType == DragType.GroupSelect; }
    }

    public Rect RubberBandSelectionArea
    {
        get { return m_RubberBandSelectionArea; }
    }

    float EventMouseMovementSpeed
    {
        get { return MouseDraggingSpeed / editor.Zoom; }
    }

    float EventKeyboardMovementSpeed
    {
        get { return KeyboardMovementSpeed / editor.Zoom; }
    }

    public PointerModes PointerMode
    {
        get { return m_PointerMode; }
        set { m_PointerMode = value; }
    }
    #endregion

    #region Functions
    public TimelineInput(TimelineWindow _editor)
    {
        editor = _editor;
        SetupContextMenuOptions();
    }

    void SetupContextMenuOptions()
    {
        // event context menu options
        m_EventContextOptions.Add(new ContextMenuOption("Add Event", false, AddEventContextCallback, true, null));
        m_EventContextOptions.Add(new ContextMenuOption("Duplicate", false, DuplicateEventsContextCallback, false, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Copy", false, CopyEventsContextCallback, false, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Paste", false, PasteEventsContextCallback, true, AreEventsOnClipboard));
        m_EventContextOptions.Add(new ContextMenuOption("Lock", false, LockEventsContextCallback, false, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Unlock", false, UnLockEventsContextCallback, true, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Mute", false, MuteEventsContextCallback, false, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Un-mute", false, UnmuteEventsContextCallback, true, IsEventSelected));
        m_EventContextOptions.Add(new ContextMenuOption("Delete", false, RemoveEventsContextCallback, false, IsEventSelected));

        // track context menu options
        m_TrackContextOptions.Add(new ContextMenuOption("Add Track", false, AddTrackGroupContextCallback, false, null));
        m_TrackContextOptions.Add(new ContextMenuOption("Add Child", false, AddChildTrackGroupContextCallback, false, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Rename", false, RenameGroupContextCallback, true, IsTrackSelectedAndVisible));
        //m_TrackContextOptions.Add(new ContextMenuOption("Move Up", false, MoveTrackGroupUpContextCallback, false, CanTrackMoveUp));
        //m_TrackContextOptions.Add(new ContextMenuOption("Move Down", false, MoveTrackGroupDownContextCallback, true, CanTrackMoveDown));
        m_TrackContextOptions.Add(new ContextMenuOption("Select Events", false, SelectTrackGroupEventsContextCallback, true, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Lock Events", false, LockTrackGroupContextCallback, false, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Unlock Events", false, UnlockTrackGroupContextCallback, true, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Mute Events", false, MuteTrackGroupContextCallback, false, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Un-mute Events", false, UnmuteTrackGroupContextCallback, true, IsTrackSelectedAndVisible));
        m_TrackContextOptions.Add(new ContextMenuOption("Delete Track", false, DeleteGroupContextCallback, false, IsTrackSelectedAndVisible));
    }

    bool IsEventSelected() { return editor.SelectedEvents.Count > 0; }
    bool AreEventsOnClipboard() { return m_CopiedEvents.Count > 0; }
    bool IsTrackSelectedAndVisible() { return editor.SelectedTrackGroup != null && !editor.SelectedTrackGroup.Hidden; }

    /// <summary>
    /// Main update loop. Call this once per frame
    /// </summary>
    virtual public void Input()
    {
        switch (m_PointerMode)
        {
            case PointerModes.Event:
                EventModeInput();
                break;

            case PointerModes.Selection:
                SelectionModeInput();
                break;
        }
    }

    /// <summary>
    /// Updates the mouse cursor image based on what you're hovering over. Only works in unity 4.0 or later
    /// </summary>
    public void UpdateCursorImage()
    {
#if UNITY_3_5 || UNITY_3_4
#else
        if (m_PointerMode != PointerModes.Selection)
        {
            return;
        }

        if (m_DragType == DragType.TimeSlider)
        {
            EditorGUIUtility.AddCursorRect(WholeScreen, MouseCursor.ResizeHorizontal);
        }
        else if (editor.SelectedEvent != null)
        {
            switch (m_TrackItemDragLocation)
            {
                case CutsceneTrackItem.DragLocation.Left_Handle:
                case CutsceneTrackItem.DragLocation.Right_Handle:
                    EditorGUIUtility.AddCursorRect(WholeScreen, MouseCursor.ResizeHorizontal);
                    break;

                case CutsceneTrackItem.DragLocation.Center:
                    EditorGUIUtility.AddCursorRect(WholeScreen, MouseCursor.Pan);
                    break;

                case CutsceneTrackItem.DragLocation.None:
                    EditorGUIUtility.AddCursorRect(GetScrollRelativePosition(editor.SelectedEvent.LeftHandle), MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(GetScrollRelativePosition(editor.SelectedEvent.RightHandle), MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(GetScrollRelativePosition(editor.SelectedEvent.GuiPosition), MouseCursor.Pan);
                    EditorGUIUtility.AddCursorRect(GetScrollRelativePosition(editor.m_VerticalTimeLineGrabArea), MouseCursor.ResizeHorizontal);
                    break;
            }
        }
        else
        {
            EditorGUIUtility.AddCursorRect(GetScrollRelativePosition(editor.m_VerticalTimeLineGrabArea), MouseCursor.ResizeHorizontal);
        }
#endif
    }

    Rect GetScrollRelativePosition(Rect pos)
    {
        Rect relative = pos;
        relative.x -= editor.m_TrackScrollPos.x;
        relative.y -= editor.m_TrackScrollPos.y;

        if (pos.xMax > editor.m_TrackArea.xMax + editor.m_TrackScrollPos.x + CutsceneEditor.ScrollerWidth)
        {
            relative.xMax = editor.m_TrackArea.xMax - editor.m_TrackScrollPos.x - CutsceneEditor.ScrollerWidth;
        }
        return relative;
    }

    void EventModeInput()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && editor.m_TrackArea.Contains(e.mousePosition))
        {
            editor.CreateEventAtPosition(e.mousePosition);
        }
    }

    /// <summary>
    /// Main function for handling input
    /// </summary>
    /// <param name="selectedCutscene"></param>
    void SelectionModeInput()
    {
        Event e = Event.current;
        Vector2 relativeMousePos = e.mousePosition + editor.m_TrackScrollPos;

        if (e.isMouse)
        {
            if (e.type == EventType.MouseDrag)
            {
                HandleMouseDrag(e.mousePosition, relativeMousePos, e.delta.x * EventMouseMovementSpeed, e);
            }
            else if (e.type == EventType.MouseDown)
            {
                HandleMouseDown(relativeMousePos, e);
            }
            else if (e.type == EventType.MouseUp)
            {
                HandleMouseUp(relativeMousePos, e);
            }
        }
        else if (e.isKey)
        {
            HandleKeyboardEvent(editor.GroupManager, e);
        }
        else if (e.type == EventType.DragExited)
        {
            HandleDragDropFinish(relativeMousePos, e);
        }
        else if (e.type == EventType.DragUpdated)
        {
            HandleDragDropMove(relativeMousePos, e);
        }
    }

    bool ClickedInTrackGroupArea(Vector2 position)
    {
        // I add in the offset so that clicking the scroll bar doesn't count as a click in the area
        Rect area = editor.m_TrackGroupArea;
        area.width -= CutsceneEditor.ScrollerWidth;
        area.height -= CutsceneEditor.ScrollerWidth;
        return area.Contains(position);
    }

    bool ClickedInTimeline(Vector2 position)
    {
        Rect draggableArea = editor.m_TrackArea;
        draggableArea.width -= CutsceneEditor.ScrollerWidth;
        draggableArea.height -= CutsceneEditor.ScrollerWidth;
        return draggableArea.Contains(position);
    }

    void HandleKeyboardEvent(CutsceneTrackGroupManager selectedCutscene, Event e)
    {
        switch (m_SelectedArea)
        {
            case SelectedArea.TrackGroup:
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode == KeyCode.LeftArrow)
                    {
                        if (editor.SelectedTrackGroup != null)
                        {
                            editor.SelectedTrackGroup.Expanded = false;
                        }
                    }
                    else if (e.keyCode == KeyCode.RightArrow)
                    {
                        if (editor.SelectedTrackGroup != null)
                        {
                            editor.SelectedTrackGroup.Expanded = true;
                        }
                    }
                    else if (e.keyCode == KeyCode.UpArrow)
                    {
                        ChangeTrackGroupSelection(selectedCutscene, true);
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                    {
                        ChangeTrackGroupSelection(selectedCutscene, false);
                    }
                }
                else if (e.keyCode == KeyCode.Delete)
                {
                    editor.RemoveTrackGroup(editor.SelectedTrackGroup);
                }
                else if (e.type == EventType.KeyUp)
                {
                    if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
                    {
                        EnterPressed();
                    }
                }
                break;

            case SelectedArea.Timeline:
                if (e.type == EventType.KeyDown)
                {
                    // theses commands will only happen when the event is in focus
                    if (e.keyCode == KeyCode.LeftArrow)
                    {
                        OffsetTrackItems(editor.SelectedEvents, -EventKeyboardMovementSpeed);
                    }
                    else if (e.keyCode == KeyCode.RightArrow)
                    {
                        OffsetTrackItems(editor.SelectedEvents, EventKeyboardMovementSpeed);
                    }
                    else if (e.keyCode == KeyCode.UpArrow)
                    {
                        ChangeTrackItemTrack(selectedCutscene, editor.SelectedEvents, -1);
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                    {
                        ChangeTrackItemTrack(selectedCutscene, editor.SelectedEvents, 1);
                    }
                    else if (e.keyCode == KeyCode.Delete)
                    {
                        editor.RemoveSelectedEvents();
                    }
                    else if (e.control || e.functionKey)
                    {
                        if (e.keyCode == KeyCode.D)
                        {
                            // duplicate
                            editor.SelectedEvents.ForEach(ce => editor.DuplicateEvent(ce));
                        }
                        else if (e.keyCode == KeyCode.Z)
                        {
                            // undo
                            //selectedCutscene.AttachEventsToTracks();
                            if (editor.m_DeletedEvents.Count > 0)
                            {
                                CutsceneEvent deletedEvent = editor.m_DeletedEvents.Pop() as CutsceneEvent;
                                editor.DuplicateEvent(deletedEvent);
                            }
                        }
                    }
                }
                break;
        }

        editor.Repaint();
    }

    protected virtual void HandleSequencerDragDrop(string[] dragAndDropObjectRefs)
    {
        
    }

    void HandleDragDropMove(Vector2 relativeMousePos, Event e)
    {
        if (DragAndDrop.objectReferences.Length > 0)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }   
    }

    void HandleDragDropFinish(Vector2 relativeMousePos, Event e)
    {
        if (DragAndDrop.objectReferences.Length > 0)
        {
            HandleSequencerDragDrop(DragAndDrop.paths);
        }
    }

    void HandleMouseUp(Vector2 relativeMousePos, Event e)
    {
        if (!ClickedInTrackGroupArea(m_MouseDownPos) && !ClickedInTimeline(m_MouseDownPos))
        {
            // the initial click happened in an area that we don't care about, so we don't care what happens on mouse up
            return;
        }

        switch (m_DragType)
        {
            case DragType.GroupSelect:
                // do nothing
                break;

            case DragType.TimeSlider:
                // do nothing
                break;

            case DragType.TrackGroup_Attempt:
                if (ClickedInTrackGroupArea(e.mousePosition) && editor.SelectedTrackGroup != null && m_CanRenameTrackGroup)
                {
                    editor.SelectedTrackGroup.EditingName = true;
                }
                break;

            case DragType.TrackGroup:
                if (ClickedInTrackGroupArea(e.mousePosition))
                {
                    CutsceneTrackGroupManager groupManager = editor.GroupManager;
                    CutsceneTrackGroup newParent = groupManager.GetGroupNameContainingPosition(relativeMousePos);
                    if (editor.SelectedTrackGroup != newParent && newParent != null)
                    {
                        groupManager.RepositionGroup(newParent, editor.SelectedTrackGroup, TimelineWindow.TrackStartingY);
                        editor.ChangedData();
                    }
                }
                break;

            default:
                if (m_DragType == DragType.TrackItem)
                {
                    // event movement occured, so flag the scene as dirty
                    editor.ChangedData();
                }

                CutsceneTrackItem trackItem = editor.GetTrackItemContainingPoint(relativeMousePos);
                if (trackItem == null)
                {
                    if (e.control)
                    {
                        if (e.mousePosition.y >= TimelineWindow.TrackStartingY)
                        {
                            // they are trying to create and event
                            editor.CreateEventAtPosition(e.mousePosition);
                        }
                    }
                    else if (m_DragType != DragType.TrackItem && ClickedInTimeline(e.mousePosition))
                    {
                        // they weren't dragging and they aren't clicking on an event, deselect everything
                        editor.UnSelectAll();
                    }
                }
                else
                {
                    if (ClickedInTimeline(e.mousePosition))
                    {
                        // they are trying to select an event
                        if (e.control)
                        {
                            if (editor.IsEventSelected(trackItem))
                            {
                                // they are de-selecting it
                                editor.UnSelectTrackItem(trackItem);
                            }
                            else
                            {
                                editor.SelectTrackItem(trackItem, false);
                            }
                        }
                        else
                        {
                            if (!editor.IsEventSelected(trackItem) && m_DragType != DragType.TrackItem)
                            {
                                editor.SelectTrackItem(trackItem, true);
                            }
                        }
                    }
                }
                break;
        }

        ResetDragging();
    }

    void HandleMouseDrag(Vector2 absoluteMousePos, Vector2 relativeMousePos, float amount, Event e)
    {
        if (e.button != 0) // only left click drags
        {
            return;
        }

        Rect draggableArea = editor.m_TrackArea;
        draggableArea.width -= CutsceneEditor.ScrollerWidth;
        draggableArea.height -= CutsceneEditor.ScrollerWidth;
        switch (m_DragType)
        {
            case DragType.TimeSlider:
                DragTimeSlider(absoluteMousePos, amount);
                break;

            case DragType.TrackGroup_Attempt:
                // now that a drag has occured, we can change to TrackGroup
                m_DragType = DragType.TrackGroup;
                break;

            case DragType.TrackGroup:
                DragTrackGroup(editor.GroupManager, editor.SelectedTrackGroup, relativeMousePos, amount);
                break;

            case DragType.TrackItem_Attempt:
                foreach (CutsceneTrackItem ce in editor.SelectedEvents)
                {
                    Vector2 dragOffset = relativeMousePos;
                    dragOffset.x -= ce.GuiPosition.x;
                    m_MouseHandleOffsets.Add(dragOffset);
                }
                // now that a drag has occured, we can change to TrackItem
                m_DragType = DragType.TrackItem;
                break;

            case DragType.TrackItem:
                DragTrackItem(editor.SelectedEvent, relativeMousePos, amount, e);
                break;

            case DragType.GroupSelect_Attempt:
                // now that a drag has occured, we can change to GroupSelect
                m_DragType = DragType.GroupSelect;
                break;

            case DragType.GroupSelect:
                SetRubberBand(m_MouseDownPos, absoluteMousePos - m_MouseDownPos);
                DoRubberBandSelect();
                editor.Repaint();
                break;

            case DragType.None:
            default:
                // nothing
                break;
        }
    }

    protected virtual void HandleTimeJump() { }

    void HandleMouseDown(Vector2 relativeMousePos, Event e)
    {
        m_MouseDownPos = e.mousePosition;
        
        if (editor.m_ClickableTimeLineArea.Contains(e.mousePosition))
        {
            if (editor.m_TimeSliderPosition.Contains(relativeMousePos) || editor.m_VerticalTimeLineGrabArea.Contains(relativeMousePos))
            {
                // clicked on time slider
                m_DragType = DragType.TimeSlider;
            }

            float time = editor.GetTimeFromScrollPosition(editor.StartTime, editor.EndTime, editor.m_TrackScrollArea, new Rect(relativeMousePos.x, relativeMousePos.y, 1, 1));
            editor.SetTime(time);

            HandleTimeJump();
            m_SelectedArea = SelectedArea.Timeline;
        }
        else if (editor.m_TimeSliderPosition.Contains(relativeMousePos) || editor.m_VerticalTimeLineGrabArea.Contains(relativeMousePos))
        {
            // clicked on time slider
            m_DragType = DragType.TimeSlider;
            m_SelectedArea = SelectedArea.Timeline;
        }
        else if (ClickedInTrackGroupArea(e.mousePosition))
        {
            // selected a track group   
            AttemptSelectTrackGroup(e.mousePosition + editor.m_TrackGroupScrollPos, e);
            m_SelectedArea = SelectedArea.TrackGroup;
        }
        else if (ClickedInTimeline(e.mousePosition))
        {
            m_SelectedArea = SelectedArea.Timeline;
            // check to see if an event is selected
            CutsceneTrackItem trackItem = editor.GetTrackItemContainingPoint(relativeMousePos);
            if (trackItem == null)
            {
                if (e.button == 0 && !e.control)
                {
                    //m_ItemDraggingAllowed = false;
                    m_DragType = DragType.GroupSelect_Attempt;
                    SetRubberBand(m_MouseDownPos, Vector2.one);
                }
            }
            else
            {
                AttemptDragTrackItem(relativeMousePos, e);
            }

            if (e.button == 1) // right click
            {
                if (trackItem != null)
                {
                    editor.SelectTrackItem(trackItem, false);
                }
                HandleContextClick(relativeMousePos, e);
            }
        }
        else
        {
            m_SelectedArea = SelectedArea.EventData;
        }

        if (m_SelectedArea != SelectedArea.TrackGroup)
        {
            editor.UnSelectTrackGroup();
        }

        editor.Repaint();
    }

    void EnterPressed()
    {
        if (editor.SelectedTrackGroup != null)
        {
            editor.SelectedTrackGroup.EditingName = !editor.SelectedTrackGroup.EditingName;
        }
    }

    void DragTrackItem(CutsceneTrackItem trackItem, Vector2 relativeMousePos, float amount, Event e)
    {
        if (trackItem == null)
            return;

        if (!trackItem.FireAndForget && m_TrackItemDragLocation != CutsceneTrackItem.DragLocation.Center)
        {
            ScaleTrackItem(trackItem, amount, m_TrackItemDragLocation == CutsceneTrackItem.DragLocation.Right_Handle, relativeMousePos);
        }
        else
        {
            if (editor.NumSelectedEvents == 1 && !e.shift)
            {
                // can't do vertical movement if multiple events are selected or if you're holding shift
                CheckVerticalMovement(editor.GroupManager, trackItem, relativeMousePos);
            }
            MoveTrackItems(editor.SelectedEvents, relativeMousePos, m_MouseHandleOffsets);
        }
    }

    void DragTrackGroup(CutsceneTrackGroupManager groupManager, CutsceneTrackGroup group, Vector2 relativeMousePos, float amount)
    {
        CutsceneTrackGroup dropLocation = groupManager.GetGroupNameContainingPosition(relativeMousePos);
        if (dropLocation != null && group != dropLocation)
        {
            // highlight
            editor.SetTrackGroupDropLocation(dropLocation);
        }
        else
        {
            editor.ClearDropArea();
        }
    }

    protected virtual void DragTimeSlider(Vector2 mousePos, float amount)
    {
        editor.OffsetTime(amount);
        m_DragType = DragType.TimeSlider;
        CheckHorizontalScroll(editor.m_TimeSliderPosition, amount > 0);
        editor.Repaint();
    }

    void AttemptDragTrackItem(Vector2 relativeMousePos, Event e)
    {
        if (e.button != 0 || e.control)
        {
            return;
        }

        CutsceneTrackItem draggable = editor.GetTrackItemContainingPoint(relativeMousePos);
        if (draggable != null)
        {
            // this sets m_SelectedEvent
            if (!editor.IsEventSelected(draggable))
            {
                editor.SelectTrackItem(draggable, !e.control);
            }

            m_TrackItemDragLocation = CutsceneTrackItem.DragLocation.Center;
            if (!draggable.FireAndForget)
            {
                m_TrackItemDragLocation = draggable.GetDragStateFromPosition(relativeMousePos);
            }

            m_DragType = DragType.TrackItem_Attempt;
        }

        editor.Repaint();
    }

    /// <summary>
    /// Resets variables that involve dragging and mouse clicks
    /// </summary>
    void ResetDragging()
    {
        m_TrackItemDragLocation = CutsceneTrackItem.DragLocation.None;
        m_DragType = DragType.None;
        SetRubberBand(Vector2.zero, Vector2.zero);
        editor.ClearDropArea();
        m_CanRenameTrackGroup = false;
        editor.Repaint();
        m_MouseHandleOffsets.Clear();
    }

    void ShowEventContextMenu(Event e, List<CutsceneTrackItem> ces, Vector2 relativeMousePosition)
    {
        GenericMenu contextMenu = new GenericMenu();
        foreach (ContextMenuOption option in m_EventContextOptions)
        {
            if (option.OptionAvailableCallback == null || option.OptionAvailableCallback())
            {
                contextMenu.AddItem(new GUIContent(option.Name), option.On, option.OptionCallback, ces);
            }
            else
            {
                contextMenu.AddDisabledItem(new GUIContent(option.Name));
            }

            if (option.FollowedBySeperator)
            {
                contextMenu.AddSeparator("");
            }
        }
        contextMenu.ShowAsContext();
    }

    void ShowTrackGroupContextMenu(Event e, CutsceneTrackGroup selectedTrackGroup)
    {
        GenericMenu contextMenu = new GenericMenu();
        foreach (ContextMenuOption option in m_TrackContextOptions)
        {
            if (option.OptionAvailableCallback == null || option.OptionAvailableCallback())
            {
                contextMenu.AddItem(new GUIContent(option.Name), option.On, option.OptionCallback, selectedTrackGroup);
            }
            else
            {
                contextMenu.AddDisabledItem(new GUIContent(option.Name));
            }

            if (option.FollowedBySeperator)
            {
                contextMenu.AddSeparator("");
            }
        }
        contextMenu.ShowAsContext();
    }

    /// <summary>
    /// Handles the right click of the mouse
    /// </summary>
    /// <param name="relativeMousePos"></param>
    /// <param name="e"></param>
    void HandleContextClick(Vector2 relativeMousePos, Event e)
    {
        CutsceneTrackGroup track = editor.GroupManager.GetTrackContainingPosition(relativeMousePos);
        if (track != null)
        {
            // right clicked in track area
            ShowEventContextMenu(e, editor.SelectedEvents, relativeMousePos);
        }
        else if (editor.m_TrackGroupArea.Contains(relativeMousePos))
        {
            // right clicked outside of track area
            HandleTrackGroupAreaClick(relativeMousePos, e);
        }
        e.Use();
        editor.Repaint();
    }

    /// <summary>
    /// Handles logic for when a right click in the track group area occurs
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="relativeMousePos"></param>
    /// <param name="e"></param>
    void HandleTrackGroupAreaClick(Vector2 relativeMousePos, Event e)
    {
        ShowTrackGroupContextMenu(e, editor.SelectedTrackGroup);
    }

    /// <summary>
    /// Determines if you're dragging the mouse enough distance on the y axis in order to vertically
    /// move events across tracks
    /// </summary>
    /// <param name="trackManager"></param>
    /// <param name="trackItem"></param>
    /// <param name="mousePos"></param>
    void CheckVerticalMovement(CutsceneTrackGroupManager trackManager, CutsceneTrackItem trackItem, Vector2 mousePos)
    {
        if ((mousePos.y < trackItem.GuiPosition.y || mousePos.y > trackItem.GuiPosition.y + CutsceneEditor.TrackHeight))
        {
            int spotsToMove = Mathf.RoundToInt((mousePos.y - trackItem.GuiPosition.y) / CutsceneEditor.TrackHeight);
            spotsToMove += mousePos.y < trackItem.GuiPosition.y ? -1 : 0;
            ChangeTrackItemTrack(trackManager, trackItem, spotsToMove);
        }
    }

    /// <summary>
    /// Changes the vertical positioning of the list of CutsceneEvents
    /// </summary>
    /// <param name="trackManager"></param>
    /// <param name="trackItems"></param>
    /// <param name="spotsToMove"></param>
    void ChangeTrackItemTrack(CutsceneTrackGroupManager trackManager, List<CutsceneTrackItem> trackItems, int spotsToMove)
    {
        trackItems.ForEach(ce => ChangeTrackItemTrack(trackManager, ce, spotsToMove));
    }

    /// <summary>
    /// Moves a track item vertically to a new track
    /// </summary>
    /// <param name="trackManager"></param>
    /// <param name="trackItem"></param>
    /// <param name="spotsToMove"></param>
    void ChangeTrackItemTrack(CutsceneTrackGroupManager trackManager, CutsceneTrackItem trackItem, int spotsToMove)
    {
        if (trackItem == null || trackItem.Locked)
        {
            return;
        }

        CutsceneTrackGroup currentTrack = trackManager.GetTrackContainingPosition(trackItem.GuiPosition.x, trackItem.GuiPosition.y);
        CutsceneTrackGroup newTrackLocation = trackManager.GetNearbyTrack(currentTrack, spotsToMove, false);

        if (newTrackLocation != null)
        {
            Rect projectedNewPosition = trackItem.GuiPosition;
            projectedNewPosition.y = newTrackLocation.TrackPosition.y;
            //if (newTrackLocation.IsTrackItemIntersecting(projectedNewPosition))
            if (trackManager.Cutscene.IsTrackItemIntersecting(projectedNewPosition))
            {
                // it's colliding with another event on the track above or below, so don't allow the movement
                return;
            }
            trackItem.GuiPosition.y = projectedNewPosition.y;
            trackManager.RemoveTrackItem(currentTrack, trackItem);
            trackManager.AddTrackItem(newTrackLocation, trackItem);
            editor.Repaint();
        }
    }

    /// <summary>
    /// Changes the width of an event
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="trackItem"></param>
    /// <param name="amount"></param>
    /// <param name="scaleLength"></param>
    public void ScaleTrackItem(CutsceneTrackItem trackItem, float amount, bool scaleLength, Vector2 relativePos)
    {
        if (trackItem.Locked)
        {
            return;
        }

        if (scaleLength)
        {
            float prevLength = trackItem.Length;
            trackItem.Length += amount;
            editor.Length = editor.CalculateTimelineLength();

            // find the new width then check collisions
            Rect prevPos = trackItem.GuiPosition;
            trackItem.GuiPosition.width = editor.GetWidthFromTime(editor.StartTime, editor.EndTime,
                trackItem.StartTime + trackItem.Length, trackItem.GuiPosition.x, editor.m_TrackScrollArea);

            if (IsCollidingHorizontally(editor.GroupManager, trackItem))
            {
                trackItem.Length = prevLength;
                trackItem.GuiPosition = prevPos;
            }

            CheckHorizontalScroll(trackItem.GuiPosition, true);
        }
        else
        {
            float prevStartTime = trackItem.StartTime;

            // they are scaling the start time  
            MoveTrackItem(trackItem, relativePos -m_MouseHandleOffsets[0]);

            // we need to increase the length because the start time is changing
            trackItem.Length -= (trackItem.StartTime - prevStartTime);
        }
    }

    /// <summary>
    /// Changes the currently selected track group if the move is possible
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="up"></param>
    void ChangeTrackGroupSelection(CutsceneTrackGroupManager trackManager, bool up)
    {
        if (editor.SelectedTrackGroup == null)
        {
            return;
        }

        Rect position = editor.SelectedTrackGroup.GroupNamePosition;
        // force the x to the middle
        position.x += editor.SelectedTrackGroup.GroupNamePosition.width * 0.5f;
        position.y += (up ? -CutsceneEditor.TrackHeight : CutsceneEditor.TrackHeight);
        CutsceneTrackGroup group = trackManager.GetGroupNameContainingPosition(new Vector2(position.x, position.y));
        if (group != null)
        {
            editor.SelectTrackGroup(group);
        }
    }

    /// <summary>
    /// Horizontally moves a track item by the given amount
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="trackItem"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool OffsetTrackItem(CutsceneTrackItem trackItem, float amount)
    {
        return editor.OffsetTrackItemTime(trackItem, amount, TimelineWindow.CollisionResolutionType.Snap);
    }

    public void OffsetTrackItems(List<CutsceneTrackItem> trackItems, float amount)
    {
        // project collisions first then, if none of the events are colliding, do the move. 
        // This way, all events stay the same distance apart
        for (int i = 0; i < trackItems.Count; i++)
        {
            float timeOffset = trackItems[i].StartTime + amount;
            if (timeOffset <= 0)
            {
                // we count this as a collision
                return;
            }

            CutsceneTrackItem collider = editor.TestHorizontalCollision(trackItems[i], timeOffset);
            if (collider != null && trackItems.Find(ti => ti.UniqueId == collider.UniqueId) == null)
            {
                // collision occured, don't move any of the events
                return;
            }
        }

        // we made it this far, so there were no collisions. Do the actual movement
        trackItems.ForEach(ce => OffsetTrackItem(ce, amount));

        // we made it this far, so there were no collisions with ungroup events.
        // use no collision resolution because it's safe to and we don't want collisions with grouped events
        for (int i = 0; i < trackItems.Count; i++)
        {
            editor.OffsetTrackItemTime(trackItems[i], amount, TimelineWindow.CollisionResolutionType.None);
        }
    }

    /// <summary>
    /// Horizontally moves a track item to the specified position
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="trackItem"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool MoveTrackItem(CutsceneTrackItem trackItem, Vector2 position)
    {
        return editor.SetTrackItemTime(trackItem, editor.GetTimeFromScrollPosition(position), TimelineWindow.CollisionResolutionType.Snap);
    }

    /// <summary>
    /// Horizontally moves a list of track items by the given amount
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="trackItems"></param>
    /// <param name="amount"></param>
    public void MoveTrackItems(List<CutsceneTrackItem> trackItems, Vector2 position, List<Vector2> positionOffets)
    {
        if (trackItems.Count != positionOffets.Count)
        {
            Debug.LogError(string.Format("trackItems.Count: {0} vs positionOffets.count: {1}", trackItems.Count, positionOffets.Count));
            return;
        }

        // project collisions first then, if none of the events are colliding, do the move. 
        // This way, all events stay the same distance apart
        for (int i = 0; i < trackItems.Count; i++)
        {
            float targetTime = editor.GetTimeFromScrollPosition(position - positionOffets[i]);
            if (targetTime <= 0)
            {
                // we count this as a collision
                return;
            }

            CutsceneTrackItem collider = editor.TestHorizontalCollision(trackItems[i], targetTime);
            if (collider != null && trackItems.Find(ti => ti.UniqueId == collider.UniqueId) == null)
            {
                // collision occured with an un-grouped events, don't move any of the events
                return;
            }
        }

        // we made it this far, so there were no collisions with ungroup events.
        // use no collision resolution because it's safe to and we don't want collisions with grouped events
        for (int i = 0; i < trackItems.Count; i++)
        {
            editor.SetTrackItemTime(trackItems[i], editor.GetTimeFromScrollPosition(position - positionOffets[i]), TimelineWindow.CollisionResolutionType.None);
        }
    }

    /// <summary>
    /// Scrolls the timeline to track a position so that it doesn't fall off screen
    /// </summary>
    /// <param name="positionToCheck"></param>
    /// <param name="movingRight"></param>
    public void CheckHorizontalScroll(Rect positionToCheck, bool movingRight)
    {
        Rect scrollLocation = editor.m_TrackArea;
        scrollLocation.x += editor.m_TrackScrollPos.x;
        if (movingRight)
        {
            if (positionToCheck.xMax > scrollLocation.xMax)
            {
                editor.m_TrackScrollPos.x -= (scrollLocation.xMax - positionToCheck.xMax);
            }
        }
        else
        {
            if (positionToCheck.x < scrollLocation.x)
            {
                editor.m_TrackScrollPos.x -= (scrollLocation.x - positionToCheck.x);
            }
        }
    }

    /// <summary>
    /// Returns the TrackItem that the provided trackItem is colliding with.  Returns null if there is no collision
    /// </summary>
    /// <param name="groupManager"></param>
    /// <param name="trackItem"></param>
    /// <returns></returns>
    public CutsceneTrackItem GetHorizontalCollider(CutsceneTrackGroupManager groupManager, CutsceneTrackItem trackItem)
    {
        if (trackItem.ReadOnly)
        {
            // we don't want read only track items moving
            return null;
        }

        CutsceneTrackGroup track = groupManager.GetTrackContainingPosition(trackItem.GuiPosition.x, trackItem.GuiPosition.y);
        if (track == null)
        {
            Debug.LogError(string.Format("couldn't find track. trackItem.GuiPosition: {0}", trackItem.GuiPosition));
            return null;
        }

        List<CutsceneTrackItem> trackItems = groupManager.GetTrackItems(track, false);
        for (int i = 0; i < trackItems.Count; i++)
        {
            if (trackItems[i] == trackItem)
            {
                continue;
            }

            if (!trackItems[i].Hidden && Utils.IsRectOverlapping(trackItems[i].GuiPosition, trackItem.GuiPosition))
            {
                return trackItems[i];
            }
        }

        for (int i = 0; i < editor.m_ItemsToBeAdded.Count; i++)
        {
            if (editor.m_ItemsToBeAdded[i] == trackItem)
            {
                continue;
            }

            if (!editor.m_ItemsToBeAdded[i].Hidden && !editor.m_ItemsToBeAdded[i].ReadOnly && Utils.IsRectOverlapping(editor.m_ItemsToBeAdded[i].GuiPosition, trackItem.GuiPosition))
            {
                return editor.m_ItemsToBeAdded[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Returns true if a horizontal collision is detected between 2 track items on the same track
    /// </summary>
    /// <param name="track"></param>
    /// <param name="trackItem"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsCollidingHorizontally(CutsceneTrackGroupManager groupManager, CutsceneTrackItem trackItem)
    {
        return GetHorizontalCollider(groupManager, trackItem) != null;
    }

    /// <summary>
    /// This function won't allow any track item to overlap with any other track item on the same track
    /// If there is a collision, it will be forced downward, creating a new track if need be
    /// </summary>
    /// <param name="cutscene"></param>
    /// <param name="trackItem"></param>
    public void ResolveHorizontalCollision(CutsceneTrackGroupManager groupManager, CutsceneTrackItem trackItem)
    {
        if (trackItem.ReadOnly)
        {
            // we don't want read only track items moving
            return;
        }

        CutsceneTrackGroup track = groupManager.GetTrackContainingPosition(trackItem.GuiPosition.x, trackItem.GuiPosition.y);
        if (track == null)
        {
            Debug.LogError("null track on event: " + trackItem.Name + " position: " + trackItem.GuiPosition);
            return;
        }

        List<CutsceneTrackItem> trackItems = groupManager.GetTrackItems(track, false);
        trackItems.AddRange(editor.m_ItemsToBeAdded);

        for (int i = 0; i < trackItems.Count; i++)
        {
            if (trackItems[i] == trackItem)
            {
                continue;
            }

            if (Utils.IsRectOverlapping(trackItems[i].GuiPosition, trackItem.GuiPosition))
            {
                CutsceneTrackGroup newTrackLocation = groupManager.GetNearbyTrack(track, 1, false);
                if (newTrackLocation == null)
                {
                    // they are already on the bottom track, just make a new one and stick the event there
                    editor.AddTrackGroup(editor.GroupManager);
                    trackItem.GuiPosition.y = track.TrackPosition.y + track.TrackPosition.height;
                }
                else
                {
                    // a track exists below the current one, move the event to that one, but then check 
                    // to make sure that we aren't colliding with an event on that one.
                    groupManager.RemoveTrackItem(track, trackItem);
                    groupManager.AddTrackItem(newTrackLocation, trackItem);
                    trackItem.GuiPosition.y = newTrackLocation.TrackPosition.y;
                    ResolveHorizontalCollision(groupManager, trackItem);
                }
            }
        }
    }

    /// <summary>
    /// Checks if the relativeMousePos is in a valid track group area
    /// </summary>
    /// <param name="relativeMousePos"></param>
    /// <param name="e"></param>
    void AttemptSelectTrackGroup(Vector2 relativeMousePos, Event e)
    {
        CutsceneTrackGroup group = editor.GroupManager.GetGroupNameContainingPosition(relativeMousePos);
        if (group != null)
        {
            if (editor.SelectedTrackGroup == group)
            {
                // this one is already selected, so they will have the chance to rename it if they don't drag it
                m_CanRenameTrackGroup = true;
            }
            editor.SelectTrackGroup(group);
            m_DragType = DragType.TrackGroup_Attempt;
        }

        if (e.button == 1)
        {
            HandleContextClick(relativeMousePos, e);
        }
    }

    /// <summary>
    /// Returns a list of Cutscene events that the rubberband selection area intersects
    /// </summary>
    /// <returns></returns>
    virtual protected List<CutsceneTrackItem> GetRubberBandSelections()
    {
        return null;
    }

    void DoRubberBandSelect()
    {
        editor.UnSelectAll();
        List<CutsceneTrackItem> selected = GetRubberBandSelections();
        for (int i = 0; i < selected.Count; i++)
        {
            editor.SelectTrackItem(selected[i], false);
        }
    }

    public void SetRubberBand(Vector2 pos, Vector2 dimensions)
    {
        m_RubberBandSelectionArea.Set(pos.x + editor.m_TrackScrollPos.x, pos.y + editor.m_TrackScrollPos.y, dimensions.x, dimensions.y);
        if (m_RubberBandSelectionArea.width < 0)
        {
            m_RubberBandSelectionArea.width = Mathf.Abs(m_RubberBandSelectionArea.width);
            m_RubberBandSelectionArea.x -= m_RubberBandSelectionArea.width;
        }
        if (m_RubberBandSelectionArea.height < 0)
        {
            m_RubberBandSelectionArea.height = Mathf.Abs(m_RubberBandSelectionArea.height);
            m_RubberBandSelectionArea.y -= m_RubberBandSelectionArea.height;
        }
    }

    #region Context Handlers

    #region Group
    void AddTrackGroupContextCallback(object cutscene)
    {
        editor.AddTrackGroup();
    }

    void AddChildTrackGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup parent = (CutsceneTrackGroup)trackGroup;
        Rect childPos = parent.TrackPosition;
        childPos.y += parent.NumChildren * CutsceneEditor.TrackHeight;
        parent.AddChildGroup(new CutsceneTrackGroup(childPos, string.Format("Track{0}", parent.NumChildren), false));
    }

    void RenameGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        group.EditingName = true;
    }

    void MoveTrackGroupUpContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.MoveTrackGroup(group, true);
    }

    void MoveTrackGroupDownContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.MoveTrackGroup(group, false);
    }

    void MuteTrackGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.SetTrackMuted(group, true);
    }

    void UnmuteTrackGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.SetTrackMuted(group, false);
    }

    void SelectTrackGroupEventsContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        List<CutsceneTrackItem> trackItems = editor.GroupManager.GetTrackItems(group, true);
        editor.UnSelectAll();
        trackItems.ForEach(ti => editor.SelectTrackItem(ti as CutsceneEvent, false));
    }

    void LockTrackGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.SetTrackLocked(group, true);
    }

    void UnlockTrackGroupContextCallback(object trackGroup)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)trackGroup;
        editor.GroupManager.SetTrackLocked(group, false);
    }

    void DeleteGroupContextCallback(object data)
    {
        CutsceneTrackGroup group = (CutsceneTrackGroup)data;
        editor.RemoveTrackGroup((CutsceneTrackGroup)group);
    }
    #endregion

    #region Event
    void AddEventContextCallback(object data)
    {
        editor.CreateEventAtPosition(m_MouseDownPos);
    }

    void DuplicateEventsContextCallback(object data)
    {
        List<CutsceneTrackItem> selectedEvents = (List<CutsceneTrackItem>)data;
        selectedEvents.ForEach(ce => editor.DuplicateEvent(ce));
    }

    void CopyEventsContextCallback(object data)
    {
        List<CutsceneTrackItem> selectedEvents = (List<CutsceneTrackItem>)data;
        m_CopiedEvents.Clear();
        m_CopiedEvents.AddRange(selectedEvents);
    }

    void PasteEventsContextCallback(object data)
    {
        List<CutsceneTrackItem> newEvents = new List<CutsceneTrackItem>();
        foreach (CutsceneEvent ce in m_CopiedEvents)
        {
            newEvents.Add(editor.DuplicateEvent(ce, TimelineWindow.CollisionResolutionType.None));
        }
        float time = editor.GetTimeFromScrollPosition(m_MouseDownPos + editor.m_TrackScrollPos);
        editor.MoveEventsToTime(newEvents, time);

        // unselect the old events and select the new ones
        editor.UnSelectAll();
        newEvents.ForEach(ce => editor.SelectTrackItem(ce, false));
    }

    void MuteEventsContextCallback(object data)
    {
        List<CutsceneEvent> selectedEvents = (List<CutsceneEvent>)data;
        selectedEvents.ForEach(ce => ce.SetEnabled(false));
    }

    void UnmuteEventsContextCallback(object data)
    {
        List<CutsceneEvent> selectedEvents = (List<CutsceneEvent>)data;
        selectedEvents.ForEach(ce => ce.SetEnabled(true));
    }

    void LockEventsContextCallback(object data)
    {
        List<CutsceneEvent> selectedEvents = (List<CutsceneEvent>)data;
        selectedEvents.ForEach(ce => ce.SetLocked(true));
    }

    void UnLockEventsContextCallback(object data)
    {
        List<CutsceneEvent> selectedEvents = (List<CutsceneEvent>)data;
        selectedEvents.ForEach(ce => ce.SetLocked(false));
    }

    void RemoveEventsContextCallback(object data)
    {
        editor.RemoveSelectedEvents();
    }
    #endregion
    #endregion
    #endregion
}
