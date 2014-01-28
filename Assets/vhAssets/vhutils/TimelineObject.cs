using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimelineObject : MonoBehaviour
{
    #region Variables
    protected List<CutsceneTrackItem> m_Events = new List<CutsceneTrackItem>();
    #endregion

    #region Properties
    virtual public string NameIdentifier 
    {
        get { return ""; }
        set { }
    }

    virtual public float StartTime
    {
        get { return 0; }
        set { }
    }

    virtual public float Length
    {
        get { return 1; }
        set { }
    }

    virtual public float EndTime
    {
        get { return StartTime + Length; }
    }

    public List<CutsceneTrackItem> Events
    {
        get { return m_Events; }
    }

    virtual public int NumEvents // TODO FIX THIS
    {
        get { return m_Events.Count; }
    }
    #endregion

    #region Functions
    /// <summary>
    /// Stores the provided sequencer event if it isn't already being stored
    /// </summary>
    /// <param name="se"></param>
    //virtual public void AddEvent(SequencerEvent se)
    //{
    //    if (!m_Events.Contains(se))
    //    {
    //        m_Events.Add(se);
    //    }
    //}

    ///// <summary>
    ///// Find the event by name
    ///// </summary>
    ///// <param name="name"></param>
    ///// <returns></returns>
    //virtual public CutsceneTrackItem GetEvent(string name)
    //{
    //    return m_Events.Find(se => se.Name == name);
    //}

    ///// <summary>
    ///// Find the event that contains the specified point
    ///// </summary>
    ///// <param name="point"></param>
    ///// <returns></returns>
    //virtual public CutsceneTrackItem GetEventContainingPoint(Vector2 point)
    //{
    //    return m_Events.Find(ce => ce.GuiPosition.Contains(point));
    //}

    ///// <summary>
    ///// Finds an event by the specified unique id
    ///// </summary>
    ///// <param name="id"></param>
    ///// <returns></returns>
    //virtual public CutsceneTrackItem GetEventByUniqueId(string id)
    //{
    //    CutsceneTrackItem retVal = m_Events.Find(ce => ce.UniqueId == id);
    //    if (retVal == null)
    //    {
    //        Debug.LogError(string.Format("Couldn't find event with id {0}", id));
    //    }
    //    return retVal;
    //}

    ///// <summary>
    ///// Removes the provided event from storage based on the it's unique id.
    ///// </summary>
    ///// <param name="se"></param>
    //virtual public void RemoveEvent(CutsceneTrackItem se)
    //{
    //    int index = m_Events.FindIndex(e => e.UniqueId == se.UniqueId);
    //    if (index != -1)
    //    {
    //        m_Events.RemoveAt(index);
    //    }
    //}

    ///// <summary>
    ///// Removes all events from storage
    ///// </summary>
    //virtual public void RemoveAllEvents()
    //{
    //    m_Events.Clear();
    //}

    virtual public void Stop()
    {

    }

#if UNITY_EDITOR
    public virtual void DrawData()
    {

    }
#endif
    #endregion
}
