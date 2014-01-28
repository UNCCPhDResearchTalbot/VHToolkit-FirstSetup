using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CutsceneTrackGroupManager))]
public class Cutscene : TimelineObject
{
    #region Constants
    public const float MaxFastForwardSpeed = 10;
    public delegate void OnCutsceneFinished(Cutscene cutscene);
    public delegate void OnEventFired(Cutscene cutscene, CutsceneEvent ce);
    #endregion

    #region Variables
    public CutsceneData m_CutsceneData = new CutsceneData();
    bool m_IsPaused;
    bool m_StartedPlaying;
    float m_LocalTime;
    int m_LoopCounter;

    [HideInInspector]
    public CutsceneTrackGroupManager m_GroupManager;
    List<CutsceneEvent> m_LoadedEvents = new List<CutsceneEvent>();

    OnCutsceneFinished m_CutsceneFinishedCallbacks;
    OnEventFired m_EventFiredCallbacks;

    // Rewind variables
    EventRecorder m_EventRecorder = new EventRecorder();
    bool m_StartStateSaved;
    #endregion

    #region Properties
    override public int NumEvents
    {
        get { return m_CutsceneData.Events.Count; }
    }

    public List<CutsceneEvent> CutsceneEvents
    {
        get { return m_CutsceneData.Events; }
    }

    public override string NameIdentifier
    {
        get { return CutsceneName; }
        set { CutsceneName = value; }
    }

    public string CutsceneName
    {
        get { return m_CutsceneData.CutsceneName; }
        set { name = m_CutsceneData.CutsceneName = value; }
    }

    override public float StartTime
    {
        get { return m_CutsceneData.StartTime; }
        set { m_CutsceneData.StartTime = value; }
    }

    override public float Length
    {
        get { return m_CutsceneData.Length; }
        set { m_CutsceneData.Length = value; }
    }

    override public float EndTime
    {
        get { return StartTime + Length; }
    }

    public bool Loop
    {
        get { return m_CutsceneData.Loop; }
        set { m_CutsceneData.Loop = value; }
    }

    public int LoopCount
    {
        get { return m_CutsceneData.LoopCount; }
        set { m_CutsceneData.LoopCount = value; }
    }

    public int Order
    {
        get { return m_CutsceneData.Order; }
        set { m_CutsceneData.Order = value; }
    }

    public CutsceneTrackGroupManager GroupManager
    {
        get { return m_GroupManager; }
        //get { return GetComponent<CutsceneTrackGroupManager>(); }
    }

    public float LocalTime
    {
        get { return m_LocalTime; }
    }

    public float CompletionPercentage
    {
        get { return LocalTime / Length; }
    }

    public bool HasStartedPlaying
    {
        get { return m_StartedPlaying; }
    }

    public bool IsPaused
    {
        get { return m_IsPaused; }
    }

    #endregion

    #region Function
    // Use this for initialization
	void Start()
    {
        CutsceneEvents.ForEach(ce => ce.Init());
        LoadEvents(0);

#if UNITY_EDITOR
        m_EventRecorder.RecordEvents(0, CutsceneEvents);
#endif
	}

    /// <summary>
    /// All events that start on or after startTime are loaded
    /// </summary>
    /// <param name="startTime"></param>
    void LoadEvents(float startTime)
    {
        m_LoadedEvents.Clear();
        m_LoadedEvents.AddRange(m_CutsceneData.Events.FindAll(ce => ce.StartTime >= startTime));
    }

    /// <summary>
    /// Start the cutscene at the beginning
    /// </summary>
    public void Play()
    {
#if UNITY_EDITOR
        if (!m_StartStateSaved)
        {
            m_StartStateSaved = true;
            m_EventRecorder.RecordEvents(0, CutsceneEvents);
        }
#endif 
        Reset();
        StartCoroutine(StartPlaying(0));
    }

    /// <summary>
    /// Start playing the cutscene at the specified starting time
    /// </summary>
    /// <param name="startingTime"></param>
    public void Play(float startingTime)
    {
        if (startingTime == 0)
        {
            Play();
        }
        else
        {
            StartCoroutine(StartPlaying(startingTime));
        }
        //Reset();
        //LoadEvents(startingTime); 
    }

    /// <summary>
    /// Plays the cutscene and records all rewind data
    /// </summary>
    public void PlayAndRecord()
    {
        Play();
        //m_IsRecording = true;
    }

    /// <summary>
    /// Fast forwards to a specific time on the cutscene at a specific speed
    /// </summary>
    /// <param name="targetTime"></param>
    /// <param name="playSpeed"></param>
    public void FastForward(float targetTime, float playSpeed, OnCutsceneFinished finishedFastForward)
    {
#if UNITY_EDITOR
        if (!m_StartStateSaved)
        {
            m_StartStateSaved = true;
            m_EventRecorder.RecordEvents(0, CutsceneEvents);
        }
#endif 
        Reset();
        StartCoroutine(DoFastForward(0, targetTime, playSpeed, finishedFastForward));
    }

    /// <summary>
    /// Fast forwards to the end of the cutscene
    /// </summary>
    public void FastForward()
    {
        FastForward(EndTime, MaxFastForwardSpeed, null);
    }

    public IEnumerator FastForwardNoReset(float targetTime, float playSpeed)
    {
        yield return StartCoroutine(DoFastForward(0, targetTime, playSpeed, null));
    }

    /// <summary>
    /// Halts the cutscene from playing and resets all the data
    /// </summary>
    override public void Stop()
    {
        FinishedCutscene();
        Reset();
    }

    /// <summary>
    /// Pauses the cutscene
    /// </summary>
    public void Pause()
    {
        if (m_StartedPlaying)
        {
            m_IsPaused = true;
        }
    }

    /// <summary>
    /// Restarts the cutscene from where it last left off
    /// </summary>
    public void Resume()
    {
        m_IsPaused = false;
    }

    /// <summary>
    /// Resets all data associated with the cutscene and restores objects
    /// to their starting states
    /// </summary>
    public void Reset()
    {
        FinishedCutscene();
        SetLocalTime(0);
        LoadEvents(0);
        LoadStartingState();

        /*if (SmartbodyManager.Get())
        {
            SmartbodyManager.Get().UploadCharacterTransforms();
        }*/
    }

    public void LoadStartingState()
    {
        m_EventRecorder.LoadEventStatesAtTime(0);
    }

    void FinishedCutscene()
    {
        m_StartedPlaying = false;
        //m_IsRecording = false;
        m_IsPaused = false;
        StopAllCoroutines();
        m_LoopCounter = 0;
    }

    float GetLocalTime(float globalTime)
    {
        return globalTime - StartTime;
    }

    IEnumerator DoFastForward(float startTime, float targetTime, float playSpeed, OnCutsceneFinished fastForwardFinished)
    {
        if (targetTime == 0)
        {
            yield break;
        }

        SmartbodyManager sbm = SmartbodyManager.Get();
          
        // load up the events
        Time.timeScale = playSpeed;
        float t = Time.time;

        //if (playFromStart)
        {
            LoadEvents(startTime);
            SetLocalTime(startTime);
            m_EventRecorder.LoadEventStatesAtTime(0);
        }

        if (sbm != null)
        {
            // pause smartbody
            sbm.enabled = false;
            sbm.UploadCharacterTransforms();
#if UNITY_3_5 || UNITY_3_4
            sbm.ForEachCharacter(c => c.gameObject.active = false);
#else
            sbm.ForEachCharacter(c => c.gameObject.SetActive(false));
#endif
        }
        
        yield return StartCoroutine(RunCutscene(targetTime));
        
        // rapidly update the smartbody simulation using 60fps intervals in order
        // to fast forward to a specific point in time while maintaining (relatively)
        // consistent results
        if (sbm != null)
        {
#if UNITY_3_5 || UNITY_3_4
            sbm.ForEachCharacter(c => c.gameObject.active = true);
#else
            sbm.ForEachCharacter(c => c.gameObject.SetActive(true));
#endif
            float someTime = 0;
            while (someTime < targetTime)
            {
                someTime += 0.016f;
                t += 0.016f;
                sbm.SetTime(t);
            }
            sbm.enabled = true;
        }
        
        Time.timeScale = 0;
        //FinishedCutscene();

        if (fastForwardFinished != null)
        {
            fastForwardFinished(this);
        }
    }

    IEnumerator StartPlaying(float startTime)
    { 
        m_StartedPlaying = true;

        while (true)
        {
            m_StartedPlaying = true;
            LoadEvents(startTime);
            SetLocalTime(startTime);
            yield return StartCoroutine(RunCutscene(Length));

            if (Loop && (m_LoopCounter < LoopCount - 1 || LoopCount == 0))
            {
                //SetLocalTime(0);
                ++m_LoopCounter;
                LoadEvents(startTime);
            }
            else
            {
                // we're done
                break;
            }
        }

        if (m_CutsceneFinishedCallbacks != null)
        {
            m_CutsceneFinishedCallbacks(this);
        }
        FinishedCutscene();
    }

    IEnumerator RunCutscene(float targetTime)
    {
        while (m_LocalTime < targetTime)
        {
            if (m_IsPaused)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            for (int i = 0; i < m_LoadedEvents.Count; i++)
            {
                if (m_LoadedEvents[i].FireAndForget && m_LocalTime >= GetLocalTime(m_LoadedEvents[i].StartTime))
                {
                    // fire this event off and unload it
                    m_LoadedEvents[i].Fire(CompletionPercentage);
                    if (m_EventFiredCallbacks != null && m_LoadedEvents[i].Enabled)
                    {
                        m_EventFiredCallbacks(this, m_LoadedEvents[i]);
                    }

                    m_LoadedEvents.RemoveAt(i--); 
                }
            }

            yield return new WaitForEndOfFrame();
            SetLocalTime(m_LocalTime + Time.deltaTime);
        }
    }

    public void SetLocalTime(float localTime)
    {
        m_LocalTime = localTime;
        SetEventTimes(Application.isPlaying ? m_LoadedEvents : m_CutsceneData.Events);

        /*
        if (Application.isPlaying && (IsPaused || !HasStartedPlaying))
        {
            //Debug.Log("LoadEventStatesAtTime!");
            m_EventRecorder.LoadEventStatesAtTime(localTime);
        }
        */
    }

    /// <summary>
    /// Adds offset to all event starting times in this cutscene
    /// </summary>
    /// <param name="offset"></param>
    public void OffsetEventStartingTimes(float offset)
    {
        CutsceneEvents.ForEach(ce => 
            {
                ce.StartTime -= offset;
                ce.StartTime = Mathf.Max(0, ce.StartTime);
            });
    }

    /// <summary>
    /// Invokes non-fire and forget events with the localized time 
    /// </summary>
    /// <param name="events"></param>
    void SetEventTimes(List<CutsceneEvent> events)
    {
        float normalizedTime = 0;
        for (int i = 0; i < events.Count; i++)
        {
            normalizedTime = (m_LocalTime - GetLocalTime(events[i].StartTime)) / events[i].Length;
            if (!events[i].FireAndForget && m_LocalTime >= GetLocalTime(events[i].StartTime) && m_LocalTime <= GetLocalTime(events[i].EndTime))
            {
                events[i].Fire(Mathf.Clamp01(normalizedTime));
                if (m_EventFiredCallbacks != null && m_LoadedEvents[i].Enabled)
                {
                    m_EventFiredCallbacks(this, events[i]);
                }
            }
        }
    }

    #region Cutscene Editor Function
    public void AddEvent(CutsceneEvent e)
    {
        m_CutsceneData.Events.Add(e);
    }

    public void RemoveEvent(CutsceneEvent e)
    {
        m_CutsceneData.Events.Remove(e);
    }

    public CutsceneTrackItem GetEventByUniqueId(string id)
    {
        CutsceneTrackItem retVal = m_CutsceneData.Events.Find(ce => ce.UniqueId == id);
        if (retVal == null)
        {
            Debug.LogError(string.Format("Couldn't find event with id {0}", id));
        }
        return retVal;
    }

    public void RemoveEvents(List<CutsceneTrackItem> eventsToRemove)
    {
        for (int i = 0; i < eventsToRemove.Count; i++)
        {
            // TODO: fix this
            if (eventsToRemove[i] is CutsceneEvent)
            {
                RemoveEvent(eventsToRemove[i] as CutsceneEvent);
            }
        }
    }

    public void SortEventsByName(bool ascending)
    {
        if (ascending)
            m_CutsceneData.Events.Sort((e1, e2) => e1.Name.CompareTo(e2.Name));      
        else
            m_CutsceneData.Events.Sort((e1, e2) => e2.Name.CompareTo(e1.Name));     
    }

    public CutsceneEvent GetCutsceneEventContainingPoint(Vector2 point)
    {
        return m_CutsceneData.Events.Find(ce => ce.GuiPosition.Contains(point));
    }

    /// <summary>
    /// Returns the total length of the cutscene based off the event that happens last
    /// </summary>
    /// <returns></returns>
    public float CalculateCutsceneLength()
    {
        float length = 1;
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if (ce.EndTime - StartTime > length)
            {
                length = ce.EndTime - StartTime;
            }
        }
        return length;
    }

    /// <summary>
    /// Forces all track items to be linked to a specific track based on position
    /// </summary>
    public void AttachEventsToTracks()
    {
        GroupManager.RemoveAllTrackItemsFromGroups();
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            CutsceneTrackGroup group = GroupManager.GetTrackContainingPosition(ce.GuiPosition.x, ce.GuiPosition.y, true);
            if (group != null)
            {
                GroupManager.AddTrackItem(group, ce);
            }
            else
            {
                Debug.LogError(string.Format("CutsceneEvent {0} doesn't exist on any track", ce.Name));
            }
        }
    }

    /// <summary>
    /// Returns the first event found with name matching eventName
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public CutsceneEvent GetEventByName(string eventName)
    {
        CutsceneEvent retVal = CutsceneEvents.Find(ce => ce.Name == eventName);
        if (retVal == null)
        {
            Debug.LogError(string.Format("Couldn't find event with eventName {0} in cutscene {1}", eventName, CutsceneName));
        }
        return retVal;
    }

    /// <summary>
    /// All callbacks will be invoked when the cutscene reaches its end time
    /// </summary>
    /// <param name="cb"></param>
    public void AddOnFinishedCutsceneCallback(OnCutsceneFinished cb)
    {
        m_CutsceneFinishedCallbacks += cb;
    }

    public void RemoveOnFinishedCutsceneCallback(OnCutsceneFinished cb)
    {
        m_CutsceneFinishedCallbacks -= cb;
    }

    public void AddOnEventFiredCallback(OnEventFired cb)
    {
        m_EventFiredCallbacks += cb;
    }

    public void RemoveOnEventFiredCallback(OnEventFired cb)
    {
        m_EventFiredCallbacks -= cb;
    }

    public CutsceneEvent GetSelectedEvent()
    {
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if (ce.Selected)
            {
                return ce;
            }
        }
        return null;
    }

    public List<CutsceneTrackItem> GetSelectedEvents()
    {
        List<CutsceneTrackItem> selected = new List<CutsceneTrackItem>();
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if (ce.Selected)
            {
                selected.Add(ce);
            }
        }
        return selected;
    }

    #region CutsceneTrackGroupFunctions
    /// <summary>
    /// Checks all trackItems on this track to see if it is intersecting
    /// with the given itemPosition. Returns true for the first found intersection
    /// </summary>
    /// <param name="itemPosition"></param>
    /// <returns></returns>
    public bool IsTrackItemIntersecting(Rect position)
    {
        for (int i = 0; i < CutsceneEvents.Count; i++)
        {
            if (!CutsceneEvents[i].Hidden && Utils.IsRectOverlapping(CutsceneEvents[i].GuiPosition, position))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
    #endregion
    #endregion
}
