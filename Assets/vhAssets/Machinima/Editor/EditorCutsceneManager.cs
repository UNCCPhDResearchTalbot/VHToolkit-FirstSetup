using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorCutsceneManager : EditorTimelineObjectManager
{
    #region Variables
    List<Cutscene> m_Cutscenes = new List<Cutscene>();
    #endregion

    #region Properties
    public int NumCutscenes
    {
        get { return m_Cutscenes.Count; }
    }
    #endregion

    #region Functions
    /*public void FindAllCutscenes()
    {
        m_Cutscenes.Clear();
        m_Cutscenes.AddRange((Cutscene[])GameObject.FindObjectsOfType(typeof(Cutscene)));
        AlphabetizeCutsceneList();
    }

    public List<Cutscene> GetAllCutscenes()
    {
        return m_Cutscenes;
    }

    public Cutscene GetCutsceneByName(string cutsceneName)
    {
        return m_Cutscenes.Find(delegate(Cutscene cutscene) { return cutscene.CutsceneName == cutsceneName; });
    }

    public Cutscene GetCutsceneByIndex(int index)
    {
        if (index < 0 || index >= m_Cutscenes.Count)
        {
            Debug.LogError(string.Format("bad index {0} passed into GetCutsceneByIndex", index));
            return null;
        }

        return m_Cutscenes[index];
    }

    public int GetCutsceneIndexByName(string cutsceneName)
    {
        return m_Cutscenes.FindIndex(delegate(Cutscene cutscene) { return cutscene.CutsceneName == cutsceneName; });
    }

    public void ChangeCutsceneName(string oldName, string newName)
    {
        Cutscene cutscene = GetCutsceneByName(oldName);
        if (cutscene == null)
        {
            Debug.LogError(string.Format("Couldn't change cutscene {0} to new name {1} because it couldn't be found", oldName, newName));
            return;
        }

        cutscene.CutsceneName = newName;
        AlphabetizeCutsceneList();
    }

    void AlphabetizeCutsceneList()
    {
        m_Cutscenes.Sort(delegate(Cutscene cutscene1, Cutscene cutscene2) { return cutscene1.CutsceneName.CompareTo(cutscene2.CutsceneName); });
    }
    */
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

    List<Cutscene> GetCutscenesInOrder()
    {
        List<Cutscene> orderedCutscenes = new List<Cutscene>(m_Cutscenes);
        orderedCutscenes.Sort((a, b) => a.Order - b.Order);
        return orderedCutscenes;
    }

    //public void StopAllCutscenes()
    //{
    //    m_Cutscenes.ForEach(c => c.Stop());
    //}

    public void FastForward(string cutsceneName, float targetTime, float fastForwardSpeed)
    {
        FastForward(GetTimelineObjectByName(cutsceneName) as Cutscene, targetTime, fastForwardSpeed);
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
                secondsToWait += ((targetTime - c.StartTime) /  fastForwardSpeed);
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
    #endregion
}
