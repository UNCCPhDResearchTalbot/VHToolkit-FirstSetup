using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorTimelineObjectManager
{
    #region Variables
    List<TimelineObject> m_TimelineObjects = new List<TimelineObject>();
    #endregion

    #region Properties
    public int NumObjects
    {
        get { return m_TimelineObjects.Count; }
    }
    #endregion

    #region Functions
    public void FindTimelineObjects<T>() where T : TimelineObject
    {
        m_TimelineObjects.Clear();
        m_TimelineObjects.AddRange((T[])GameObject.FindObjectsOfType(typeof(T)));
        AlphabetizeTimeObjectList();
    }

    public List<TimelineObject> GetAllTimelineObjects()
    {
        return m_TimelineObjects;
    }

    public TimelineObject GetTimelineObjectByName(string name)
    {
        return m_TimelineObjects.Find(delegate(TimelineObject timelineObj) { return timelineObj.NameIdentifier == name; });
    }

    public TimelineObject GetTimelineObjectByIndex(int index)
    {
        if (index < 0 || index >= NumObjects)
        {
            Debug.LogError(string.Format("bad index {0} passed into GetCutsceneByIndex", index));
            return null;
        }

        return m_TimelineObjects[index];
    }

    public int GetTimeObjectIndexByName(string name)
    {
        return m_TimelineObjects.FindIndex(delegate(TimelineObject timelineObj) { return timelineObj.NameIdentifier == name; });
    }

    public void ChangeCutsceneName(string oldName, string newName)
    {
        TimelineObject timelineObj = GetTimelineObjectByName(oldName);
        if (timelineObj == null)
        {
            Debug.LogError(string.Format("Couldn't change cutscene {0} to new name {1} because it couldn't be found", oldName, newName));
            return;
        }

        timelineObj.NameIdentifier = newName;
        AlphabetizeTimeObjectList();
    }

    void AlphabetizeTimeObjectList()
    {
        m_TimelineObjects.Sort(delegate(TimelineObject cutscene1, TimelineObject cutscene2) { return cutscene1.NameIdentifier.CompareTo(cutscene2.NameIdentifier); });
    }

    public void StopAllCutscenes()
    {
        m_TimelineObjects.ForEach(c => c.Stop());
    }

    /*
    public float GetFirstStartTime()
    {
        float startTime = 0;
        foreach (Cutscene cutscene in m_Cutscenes)
        {
            if (startTime > cutscene.StartTime)
            {
                startTime = cutscene.StartTime;
            }
        }

        return startTime;
    }

    public float GetLongestCutsceneLength()
    {
        float length = 1;
        foreach (Cutscene cutscene in m_Cutscenes)
        {
            if (length < cutscene.Length)
            {
                length = cutscene.Length;
            }
        }
        return length;
    }

    public float GetLastCutsceneEndTime()
    {
        float greatestEndTime = 1;
        foreach (Cutscene cutscene in m_Cutscenes)
        {
            if (greatestEndTime < cutscene.EndTime)
            {
                greatestEndTime = cutscene.EndTime;
            }
        }
        return greatestEndTime;
    }
     
    public void FastForward(string cutsceneName, float targetTime, float fastForwardSpeed)
    {
        FastForward(GetCutsceneByName(cutsceneName), targetTime, fastForwardSpeed);
    }

    public void FastForward(Cutscene cutscene, float targetTime, float fastForwardSpeed)
    {
        cutscene.StartCoroutine(DoFastForward(cutscene, targetTime, fastForwardSpeed));
    }

    public float CalculateTimeRequiredToFastForward(Cutscene cutscene, float targetTime, float fastForwardSpeed)
    {
        List<Cutscene> orderedCutscenes = GetCutscenesInOrder();
        bool isTargetCutscene = false;
        float secondsToWait = 0;
        foreach (Cutscene c in orderedCutscenes)
        {
            if (c == cutscene)
            {
                isTargetCutscene = true;
                secondsToWait += ((targetTime - c.StartTime) / fastForwardSpeed);
            }
            else
            {
                secondsToWait += (c.Length / fastForwardSpeed);
            }

            if (isTargetCutscene)
            {
                break;
            }
        }

        return secondsToWait;
    }

    IEnumerator DoFastForward(Cutscene cutscene, float targetTime, float fastForwardSpeed)
    {
        if (cutscene == null)
        {
            Debug.LogError("Failed to FastForward because cutscene is null");
            yield break;
        }

        List<Cutscene> orderedCutscenes = GetCutscenesInOrder();
        bool isTargetCutscene = false;
        float secondsToWait = 0;
        orderedCutscenes.ForEach(c => c.LoadStartingState());
        foreach (Cutscene c in orderedCutscenes)
        {
            if (c == cutscene)
            {
                isTargetCutscene = true;
                secondsToWait = targetTime - c.StartTime;
            }
            else
            {
                secondsToWait = c.Length;
            }

            yield return c.StartCoroutine(c.FastForwardNoReset(secondsToWait, fastForwardSpeed));
            if (isTargetCutscene)
            {
                break;
            }
        }
    }
    */
   
    #endregion
}
