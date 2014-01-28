using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[System.Serializable]
public class CutsceneTrackGroup
{
    #region Variables
    public string GroupName = "";

    [HideInInspector]
    public bool Expanded;

    [HideInInspector]
    public bool EditingName;

    [HideInInspector]
    public bool Hidden;

    [HideInInspector]
    public bool Muted;

    [HideInInspector]
    public Rect GroupNamePosition = new Rect();

    [HideInInspector]
    public bool IsSelected = false;

    [HideInInspector]
    public Rect TrackPosition = new Rect();

    [HideInInspector]
    public string UniqueId = "";

    public List<CutsceneTrackGroup> m_Children = new List<CutsceneTrackGroup>();

    // the GUID's of the events that are on the track
    [HideInInspector]
    public List<string> m_TrackItems = new List<string>();
    #endregion

    #region Properties
    public bool HasChildren
    {
        get { return m_Children.Count > 0; }
    }

    public int NumChildren
    {
        get { return m_Children.Count; }
    }
    #endregion

    #region Functions
    public CutsceneTrackGroup() { }
    public CutsceneTrackGroup(Rect trackPosition, string groupName, bool expanded)
    {
        GroupName = groupName;
        Expanded = expanded;
        TrackPosition = trackPosition;
        UniqueId = Guid.NewGuid().ToString();
    }

    public void AddChildGroup(CutsceneTrackGroup child)
    {
        if (!m_Children.Contains(child))
        {
            m_Children.Add(child);
            Expanded = true;
            //Debug.Log(string.Format("{0} now has child {1}", GroupName, child.GroupName));
        }
    }

    public void SetStartingPosition(Rect pos)
    {
        SetTrackWidthHeight(pos.width, pos.height);
    }

    public void SetTrackPosition(float x, float y)
    {
        TrackPosition.x = x;
        TrackPosition.y = y;
    }

    public void SetTrackWidthHeight(float w, float h)
    {
        TrackPosition.width = w;
        TrackPosition.height = h;
    }

    /// <summary>
    /// Returns true if you clicked in the track area
    /// </summary>
    /// <param name="selectionPoint"></param>
    /// <returns></returns>
    public bool TrackContainsPosition(Vector2 selectionPoint)
    {
        Rect tempTrackPosition = TrackPosition;
        // this is a hack to make sure that when we drag events off the right side of the screen,
        // the track appears to be infinite in width so that events know what track they are on.
        // Without this, quickly dragging events to the right will cause errors about the event not
        // being on any track
        tempTrackPosition.width = 1000000; 
        return tempTrackPosition.Contains(selectionPoint);
    }

    /// <summary>
    /// Returns true if you clicked in the track area
    /// </summary>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    /// <returns></returns>
    public bool TrackContainsPosition(float posX, float posY)
    {
        return TrackContainsPosition(new Vector2(posX, posY));
    }

    /// <summary>
    /// Returns true if the trackItem is part of this track group
    /// </summary>
    /// <param name="trackItem"></param>
    /// <returns></returns>
    public bool TrackContainsItem(CutsceneTrackItem trackItem)
    {
       // return m_TrackItems.Find(t => t.UniqueId == trackItem.UniqueId) != null;
        return (int)trackItem.GuiPosition.y == (int)TrackPosition.y;
    }

    /// <summary>
    /// Returns true if you click the name of the group
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool GroupNameContainsPosition(Vector2 position)
    {
        return GroupNamePosition.Contains(position);
    }

    /// <summary>
    /// Highlights the track at the specified location
    /// </summary>
    /// <param name="selectionPoint"></param>
    /// <param name="clearPreviousSelections"></param>
    public void SelectTrack(bool selected)
    {
        IsSelected = selected;
    }

    public void Draw(Rect drawPosition)
    {
        Color original = GUI.color;
        TrackPosition.y = drawPosition.y;

        if (IsSelected)
        {
            GUI.color = CutsceneEvent.SelectedColor;
        }
        
        GUI.Box(drawPosition, "");

        // restore
        GUI.color = original;
    }

    /// <summary>
    /// Returns true if the potentialChild is a child. This checks recursively through child tracks
    /// </summary>
    /// <param name="potentialChild"></param>
    /// <returns></returns>
    public bool IsAncestorOf(CutsceneTrackGroup potentialChild)
    {
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup group in m_Children)
        {
            groupStack.Clear();
            groupStack.Push(group);

            while (groupStack.Count > 0)
            {
                CutsceneTrackGroup currGroup = groupStack.Pop();
                if (currGroup == potentialChild)
                {
                    return true;
                }

                foreach (CutsceneTrackGroup child in currGroup.m_Children)
                {
                    groupStack.Push(child);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if this track has potentialAncestor in it's hierarchy
    /// </summary>
    /// <param name="potentialAncestor"></param>
    /// <returns></returns>
    public bool IsChildOf(CutsceneTrackGroup potentialAncestor)
    {
        return potentialAncestor.IsAncestorOf(this);
    }            
    #endregion
}
