using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof(Cutscene))]
public class CutsceneTrackGroupManager : MonoBehaviour
{
    #region Variables
    public List<CutsceneTrackGroup> m_TrackGroups = new List<CutsceneTrackGroup>();

    // so that we don't have to allocate one every time we draw, store this
    Stack<CutsceneTrackGroup> m_DrawStack = new Stack<CutsceneTrackGroup>();
    Rect m_StartingPosition = new Rect();
    CutsceneTrackGroup m_SelectedGroup;
    #endregion

    #region Properties
    public int NumGroups
    {
        get { return m_TrackGroups.Count; }
    }

    public Cutscene Cutscene
    {
        get { return GetComponent<Cutscene>(); }
    }

    public List<CutsceneEvent> CutsceneEvents
    {
        get { return Cutscene.CutsceneEvents; }
    }

    public CutsceneTrackGroup FirstGroup
    {
        get { return m_TrackGroups[0]; }
    }

    public CutsceneTrackGroup LastGroup
    {
        get { return m_TrackGroups.Count > 0 ? m_TrackGroups[m_TrackGroups.Count - 1] : null; }
    }
    #endregion

    #region Functions
    public void SetStartingPosition(Rect pos)
    {
        m_StartingPosition = pos;
        for (int i = 0; i < m_TrackGroups.Count; i++)
        {
            m_TrackGroups[i].SetTrackWidthHeight(pos.width, pos.height);
        }
    }

    public List<CutsceneTrackGroup> GetSelectedTracks()
    {
        return m_TrackGroups.FindAll(t => t.IsSelected);
    }

    public CutsceneTrackGroup GetTrackContainingPosition(float posX, float posY)
    {
        return GetTrackContainingPosition(new Vector2(posX, posY), false);
    }

    public CutsceneTrackGroup GetTrackContainingItem(CutsceneTrackItem trackItem)
    {
        return GetTrackContainingPosition(trackItem.GuiPosition.x, trackItem.GuiPosition.y, false);
    }

    public CutsceneTrackGroup GetTrackContainingPosition(float posX, float posY, bool ignoreExpandedFlag)
    {
        return GetTrackContainingPosition(new Vector2(posX, posY), ignoreExpandedFlag);
    }

    public CutsceneTrackGroup GetTrackContainingItem(CutsceneTrackItem trackItem, bool ignoreExpandedFlag)
    {
        return GetTrackContainingPosition(trackItem.GuiPosition.x, trackItem.GuiPosition.y, ignoreExpandedFlag);
    }

    public CutsceneTrackGroup GetTrackContainingPosition(Vector2 selectionPoint)
    {
        return GetTrackContainingPosition(selectionPoint, false);
    }

    /// <summary>
    /// Returns the track that has a rectangular position containing the selectionPoint
    /// </summary>
    /// <param name="selectionPoint"></param>
    /// <returns></returns>
    public CutsceneTrackGroup GetTrackContainingPosition(Vector2 selectionPoint, bool ignoreExpandedFlag)
    {
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup group in m_TrackGroups)
        {
            groupStack.Clear();
            groupStack.Push(group);

            while (groupStack.Count > 0)
            {
                CutsceneTrackGroup currGroup = groupStack.Pop();
                if (currGroup.TrackContainsPosition(selectionPoint))
                {
                    return currGroup;
                }

                if (currGroup.Expanded || ignoreExpandedFlag)
                {
                    foreach (CutsceneTrackGroup child in currGroup.m_Children)
                    {
                        groupStack.Push(child);
                    }
                }
            }
        }
        return null;
    }

    public CutsceneTrackGroup GetGroupNameContainingPosition(Vector2 position)
    {
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup group in m_TrackGroups)
        {
            groupStack.Clear();
            groupStack.Push(group);

            while (groupStack.Count > 0)
            {
                CutsceneTrackGroup currGroup = groupStack.Pop();
                if (currGroup.GroupNameContainsPosition(position))
                {
                    return currGroup;
                }

                if (currGroup.Expanded)
                {
                    foreach (CutsceneTrackGroup child in currGroup.m_Children)
                    {
                        groupStack.Push(child);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the parent track group of the specified track. Null is if it has no parent
    /// </summary>
    /// <param name="track"></param>
    /// <returns></returns>
    public CutsceneTrackGroup GetParentGroup(CutsceneTrackGroup group)
    {
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup g in m_TrackGroups)
        {
            groupStack.Clear();
            groupStack.Push(g);

            while (groupStack.Count > 0)
            {
                CutsceneTrackGroup currGroup = groupStack.Pop();
                if (currGroup.m_Children.Contains(group))
                {
                    return currGroup;
                }

                if (currGroup.Expanded)
                {
                    foreach (CutsceneTrackGroup child in currGroup.m_Children)
                    {
                        groupStack.Push(child);
                    }
                }
            }
        }

        // this is a base track with no parent
        return null;
    }

    public void DetermineAllowedGroupMovements(CutsceneTrackGroup group, ref bool canMoveUp, ref bool canMoveDown)
    {
        canMoveUp = canMoveDown = false;
        CutsceneTrackGroup parent = GetParentGroup(group);
        List<CutsceneTrackGroup> children = parent == null ? m_TrackGroups : parent.m_Children;
        int index = children.FindIndex(g => g == group);
        if (index == -1)
        {
            Debug.LogError(string.Format("track {0} doesn't exist", group.GroupName));
            return;
        }

        canMoveUp = index > 0;
        canMoveDown = index < children.Count - 1;
    }

    /// <summary>
    /// Returns the cutscene track that is indexDistance from the startingPoint. indexDistance
    /// can be negative.  If clampIndex is true, the return value is guaranteed not to be null
    /// and will be in the range of 0 - (NumTracks - 1)
    /// </summary>
    /// <param name="startingPoint"></param>
    /// <param name="indexDistance"></param>
    /// <param name="clampIndex"></param>
    /// <returns></returns>
    public CutsceneTrackGroup GetNearbyTrack(CutsceneTrackGroup startingPoint, int indexDistance, bool clampIndex)
    {
        if (startingPoint == null)
        {
            Debug.LogError("No starting track");
            return null;
        }

        CutsceneTrackGroup nearbyTrack = GetTrackContainingPosition(startingPoint.TrackPosition.x,
            startingPoint.TrackPosition.y + (indexDistance * startingPoint.TrackPosition.height));

        if (nearbyTrack == null && clampIndex)
        {
            nearbyTrack = indexDistance < 0 ? FirstGroup : LastGroup;
        }

        return nearbyTrack;
    }

    /// <summary>
    /// Highlights the track at the specified location
    /// </summary>
    /// <param name="selectionPoint"></param>
    /// <param name="clearPreviousSelections"></param>
    public void SelectTrack(Vector2 selectionPoint, bool clearPreviousSelections)
    {
        if (m_SelectedGroup != null)
        {
            m_SelectedGroup.SelectTrack(false);
        }

        CutsceneTrackGroup trackGroup = GetTrackContainingPosition(selectionPoint);
        if (trackGroup != null)
        {
            m_SelectedGroup = trackGroup;
            trackGroup.SelectTrack(true);
        }
    }

    public void RemoveAllGroups()
    {
        m_TrackGroups.Clear();
    }

    /// <summary>
    /// Adds a new track to be rendered
    /// </summary>
    public CutsceneTrackGroup AddTrack(string trackName)
    {
        CutsceneTrackGroup track = new CutsceneTrackGroup(new Rect(m_StartingPosition.x, CalculatePosY(NumGroups), m_StartingPosition.width, m_StartingPosition.height), trackName, false);
        m_TrackGroups.Add(track);
        SortTracksByPosition();
        return track;
    }

    /// <summary>
    /// Keeps the tracks sorted by the y position
    /// </summary>
    void SortTracksByPosition()
    {
        m_TrackGroups.Sort((a, b) => a.TrackPosition.y >= b.TrackPosition.y ? 1 : -1);
    }

    float CalculatePosY(int i)
    {
        return m_StartingPosition.y + i * m_StartingPosition.height;
    }

    /// <summary>
    /// Draws the entire CutsceneTrackGroup Hierarchy
    /// </summary>
    /// <param name="trackHeight"></param>
    public void Draw(float trackHeight)
    {
        CutsceneTrackGroup currGroup = null;
        Rect drawPosition = m_StartingPosition;

        foreach (CutsceneTrackGroup group in m_TrackGroups)
        {
            m_DrawStack.Clear();
            m_DrawStack.Push(group);
            while (m_DrawStack.Count > 0)
            {
                currGroup = m_DrawStack.Pop();
                currGroup.Draw(drawPosition);

                foreach (string guid in currGroup.m_TrackItems)
                {
                    CutsceneEvent ce = CutsceneEvents.Find(c => c.UniqueId == guid);
                    if (ce != null)
                    {
                        ce.GuiPosition.y = drawPosition.y;
                        ce.Hidden = false;
                    }
                }

                drawPosition.y += trackHeight;

                // draw children only if expanded
                if (currGroup.Expanded)
                {
                    //foreach (CutsceneTrackGroup child in currGroup.m_Children)
                    for (int i = currGroup.m_Children.Count - 1; i > -1; i--)
                    {
                        m_DrawStack.Push(currGroup.m_Children[i]);
                    }
                }
            }
        }
    }

    float FindGlobalPosition(CutsceneTrackGroup target)
    {
        CutsceneTrackGroup currGroup = null;
        Rect drawPosition = m_StartingPosition;

        foreach (CutsceneTrackGroup group in m_TrackGroups)
        {
            m_DrawStack.Clear();
            m_DrawStack.Push(group);
            while (m_DrawStack.Count > 0)
            {
                currGroup = m_DrawStack.Pop();
                currGroup.Draw(drawPosition);

                if (currGroup == target)
                {
                    return drawPosition.y;
                }

                drawPosition.y += currGroup.TrackPosition.height;

                // draw children only if expanded
                if (currGroup.Expanded)
                {
                    //foreach (CutsceneTrackGroup child in currGroup.m_Children)
                    for (int i = currGroup.m_Children.Count - 1; i > -1; i--)
                    {
                        m_DrawStack.Push(currGroup.m_Children[i]);
                    }
                }
            }
        }

        Debug.Log(string.Format("Failed to find the position of track {0}", target.GroupName));
        return 0;
    }

    /// <summary>
    /// Gives a new parent to a track group. All children move as well
    /// </summary>
    /// <param name="newParent"></param>
    /// <param name="group"></param>
    public void RepositionGroup(CutsceneTrackGroup newParent, CutsceneTrackGroup group, float trackStartingY)
    {
        // first check if the group in question is a parent of the attempted new parent
        if (newParent.IsChildOf(group))
        {
            return;
        }

        // save all previous positions in order to shift the cutscene track items after the group has been repositioned
        //Dictionary<int, CutsceneTrackGroup> previousPositions = new Dictionary<int, CutsceneTrackGroup>();
        //CollectPreviousTrackPositions(group, previousPositions);

        //float prevPos = group.TrackPosition.y;
        //float newY = 0;
        CutsceneTrackGroup currentParent = GetParentGroup(group);
        DetachGroup(group);
        
        //Debug.Log("current parent group: " + currentParent == null ? "null parent" : currentParent.GroupName);
        if (currentParent == newParent)
        {
            // the group's current parent and new parent are the same, so make the group in question a sibling of it's parent
            newParent = GetParentGroup(currentParent);
        }

        // recalibrate all tracks
        for (int i = 0; i < m_TrackGroups.Count; i++)
        {
            SetGroupPosY(m_TrackGroups[i], FindGlobalPosition(m_TrackGroups[i])/*i * group.TrackPosition.height + trackStartingY*/);
        }

        AddGroup(newParent, group);
        SetGroupPosY(group, FindGlobalPosition(group));

        //ShiftItemPositions(group, previousPositions, new List<CutsceneTrackItem>());
    }

    void CollectPreviousTrackPositions(CutsceneTrackGroup group, Dictionary<int, CutsceneTrackGroup> previousPositions)
    {
        previousPositions.Add((int)FindGlobalPosition(group), group);
        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            CollectPreviousTrackPositions(child, previousPositions);
        }
    }

    //void ShiftItemPositions(CutsceneTrackGroup group, Dictionary<int, CutsceneTrackGroup> previousPositions, List<CutsceneTrackItem> movedItems)
    //{
    //    //foreach (KeyValuePair<int, CutsceneTrackGroup> kvp in previousPositions)
    //    //{
    //    //    Debug.Log(string.Format("{0} {1} {2}", kvp.Key, kvp.Value.GroupName, kvp.Value.TrackPosition.y));
    //    //}

    //    foreach (CutsceneEvent ce in CutsceneEvents)
    //    {
    //        int key = (int)ce.GuiPosition.y;
    //        if (previousPositions.ContainsKey(key) && !movedItems.Contains(ce))
    //        {
    //            Debug.Log(string.Format("{0} was {1} now {2} on track {3}", ce.Name, ce.GuiPosition.y,
    //                previousPositions[key].TrackPosition.y, previousPositions[key].GroupName));
    //            ce.GuiPosition.y = previousPositions[key].TrackPosition.y;
    //            movedItems.Add(ce);
    //            //previousPositions.Remove(key);
    //        }
    //    }

    //    foreach (CutsceneTrackGroup child in group.m_Children)
    //    {
    //        ShiftItemPositions(child, previousPositions, movedItems);
    //    }
    //}

    public void AddGroup(CutsceneTrackGroup group)
    {
        if (!m_TrackGroups.Contains(group))
        {
            m_TrackGroups.Add(group);
        }
    }

    public void AddGroup(CutsceneTrackGroup parent, CutsceneTrackGroup child)
    {
        if (parent == null)
        {
            AddGroup(child);
        }
        else
        {
            m_TrackGroups.Remove(child);
            parent.AddChildGroup(child);
        }   
    }

    public void RemoveGroup(Cutscene associatedCutscene, CutsceneTrackGroup group)
    {
        associatedCutscene.GroupManager.DeleteGroup(group);
        DetachGroup(group);
    }

    public void DetachGroup(CutsceneTrackGroup group)
    {
        CutsceneTrackGroup parent = GetParentGroup(group);
        if (parent == null)
        {
            m_TrackGroups.Remove(group);
        }
        else
        {
            parent.m_Children.Remove(group);
        }
    }

    public void RemoveAllTrackItemsFromGroups()
    {
        m_TrackGroups.ForEach(tg => RemoveAllTrackItems(tg));
    }

    public CutsceneTrackGroup FindGroupByName(string groupName)
    {
        CutsceneTrackGroup group = m_TrackGroups.Find(g => g.GroupName == groupName);
        if (group != null)
        {
            Debug.LogError(string.Format("Couldn't find CutsceneTrackGroup {0}", groupName));
        }
        return group;
    }

    public void MoveTrackGroup(CutsceneTrackGroup group, bool up)
    {
        CutsceneTrackGroup parent = GetParentGroup(group);
        List<CutsceneTrackGroup> children = parent == null ? m_TrackGroups : parent.m_Children;

        int index = children.FindIndex(g => g == group);
        if (index == -1)
        {
            Debug.LogError(string.Format("track {0} doesn't exist", group.GroupName));
            return;
        }

        SwitchTrackGroupPositions(children, index, up ? index - 1 : index + 1);
    }

    void SwitchTrackGroupPositions(List<CutsceneTrackGroup> children, int groupIndex1, int groupIndex2)
    {
        if (groupIndex1 == -1 || groupIndex2 == -1)
        {
            Debug.LogError("SwitchTrackGroupPositions failed");
            return;
        }

        CutsceneTrackGroup temp = children[groupIndex1];
        children[groupIndex1] = children[groupIndex2];
        children[groupIndex2] = temp;
    }

    #region Invdividual Track Group Functions

    public void SetGroupPosY(CutsceneTrackGroup group, float yPos)
    {
        group.TrackPosition.y = yPos;

        if (group.Expanded)
        {
            for (int i = 0; i < group.m_Children.Count; i++)
            {
                yPos += group.TrackPosition.height * (i + 1);
                SetGroupPosY(group.m_Children[i], yPos);
            }
        }
    }

    /// <summary>
    /// Removes a track group, all it's events, all child track groups, and all child track events
    /// </summary>
    /// <param name="associatedCutscene"></param>
    public void DeleteGroup(CutsceneTrackGroup group)
    {
        List<CutsceneEvent> events = CutsceneEvents;
        for (int i = 0; i < events.Count; i++)
        {
            if ((int)events[i].GuiPosition.y == (int)group.TrackPosition.y)
            {
                Cutscene.RemoveEvent(events[i]);
            }
        }
        
        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            DeleteGroup(child);
        }
    }

    /// <summary>
    /// Returns the number of track items on this track and all child tracks recursively
    /// </summary>
    /// <returns></returns>
    public int GetNumTrackItems(CutsceneTrackGroup group)
    {
        int count = 0;
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
            {
                ++count;
            }
        }
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            groupStack.Clear();
            groupStack.Push(child);

            while (groupStack.Count > 0)
            {
                CutsceneTrackGroup currGroup = groupStack.Pop();
                foreach (CutsceneEvent ce in CutsceneEvents)
                {
                    if ((int)ce.GuiPosition.y == (int)currGroup.TrackPosition.y)
                    {
                        ++count;
                    }
                }
                foreach (CutsceneTrackGroup child2 in currGroup.m_Children)
                {
                    groupStack.Push(child2);
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Hides or shows child tracks and track events
    /// </summary>
    /// <param name="expanded"></param>
    public void ChangedExpanded(CutsceneTrackGroup group, bool expanded)
    {
        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            SetTrackItemsHidden(child, !expanded);
            ChangedExpanded(child, !expanded ? expanded : group.Expanded);
        }
    }

    /// <summary>
    /// Recursively hides or shows all child tracks
    /// </summary>
    /// <param name="hidden"></param>
    public void SetTrackItemsHidden(CutsceneTrackGroup group, bool hidden)
    {
        group.Hidden = hidden;

        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
            {
                ce.Hidden = hidden;
            }
        }

        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            SetTrackItemsHidden(child, hidden);
        }
    }

    /// <summary>
    /// Disables or enables all track items on this track and all child tracks
    /// </summary>
    /// <param name="muted"></param>
    public void SetTrackMuted(CutsceneTrackGroup group, bool muted)
    {
        group.Muted = muted;
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
            {
                ce.SetEnabled(!muted);
            }
        }

        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            SetTrackMuted(child, muted);
        }
    }

    /// <summary>
    /// Locks or unlocks all track items on this track and all child tracks
    /// </summary>
    /// <param name="locked"></param>
    public void SetTrackLocked(CutsceneTrackGroup group, bool locked)
    {
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
            {
                ce.SetLocked(locked);
            }
        }

        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            SetTrackLocked(child, locked);
        }
    }

    /// <summary>
    /// Returns all the track items on this track and all child tracks
    /// </summary>
    /// <returns></returns>
    public List<CutsceneTrackItem> GetTrackItems(CutsceneTrackGroup group, bool recursive)
    {
        List<CutsceneTrackItem> retVal = new List<CutsceneTrackItem>();

        if (recursive)
        {
            GetTrackItemsRecursive(group, retVal, false);
        }
        else
        {
            foreach (CutsceneEvent ce in CutsceneEvents)
            {
                if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
                {
                    retVal.Add(ce);
                }
            }
        }
        
        return retVal;
    }

    /// <summary>
    /// Returns all the SELECTED track items on this track and all child tracks
    /// </summary>
    /// <returns></returns>
    public List<CutsceneTrackItem> GetSelectedTrackItems(CutsceneTrackGroup group)
    {
        List<CutsceneTrackItem> retVal = new List<CutsceneTrackItem>();
        GetTrackItemsRecursive(group, retVal, true);
        return retVal;
    }

    void GetTrackItemsRecursive(CutsceneTrackGroup group, List<CutsceneTrackItem> items, bool selectedOnly)
    {
        foreach (CutsceneEvent ce in CutsceneEvents)
        {
            if ((int)ce.GuiPosition.y == (int)group.TrackPosition.y)
            {
                if (selectedOnly)
                {
                    if (ce.Selected)
                    {
                        items.Add(ce);
                    }
                }
                else
                {
                    items.Add(ce);
                }
            }
        }

        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            GetTrackItemsRecursive(child, items, selectedOnly);
        }
    }

    /// <summary>
    /// returns the number of child tracks recursively. Does not include this track
    /// </summary>
    /// <returns></returns>
    public int GetNumChildren(CutsceneTrackGroup group)
    {
        int count = 0;
        Stack<CutsceneTrackGroup> groupStack = new Stack<CutsceneTrackGroup>();
        foreach (CutsceneTrackGroup child in group.m_Children)
        {
            groupStack.Clear();
            groupStack.Push(child);

            while (groupStack.Count > 0)
            {
                ++count;
                CutsceneTrackGroup currGroup = groupStack.Pop();
                foreach (CutsceneTrackGroup child2 in currGroup.m_Children)
                {
                    groupStack.Push(child2);
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Adds the specified track item to the track, unless it is on the track
    /// </summary>
    /// <param name="item"></param>
    public void AddTrackItem(CutsceneTrackGroup group, CutsceneTrackItem item)
    {
        int index = group.m_TrackItems.FindIndex(i => i == item.UniqueId);
        if (index == -1)
        {
            group.m_TrackItems.Add(item.UniqueId);
            item.GuiPosition.y = group.TrackPosition.y;
        }
        else
        {
            Debug.LogError(string.Format("Failed to add track item {0} to track {1} ", item.Name, group.GroupName));
        }
    }

    /// <summary>
    /// Removes all track items from this track
    /// </summary>
    public void RemoveAllTrackItems(CutsceneTrackGroup group)
    {
        group.m_TrackItems.Clear();
        group.m_Children.ForEach(c => RemoveAllTrackItems(c));
    }

    /// <summary>
    /// Removes the specified track item from this track, based on the track item's unique id
    /// </summary>
    /// <param name="item"></param>
    public void RemoveTrackItem(CutsceneTrackGroup group, CutsceneTrackItem item)
    {
        int index = group.m_TrackItems.FindIndex(i => i == item.UniqueId);
        if (index != -1)
        {
            group.m_TrackItems.RemoveAt(index);
        }
        else
        {
            //Debug.LogError(string.Format("Failed to remove track item {0} from track group {1}", item.Name, group.GroupName));
        }
    }
    #endregion
    #endregion
}
