using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class EventRecorder
{
    #region Variables
    SortedDictionary<int, List<RewindData>> m_TimeData = new SortedDictionary<int, List<RewindData>>();
    #endregion

    #region Functions
    /// <summary>
    /// Saves the data of all specified events at the specified time. The data that is saved
    /// is specific to each ICutsceneEventInterface 
    /// </summary>
    /// <param name="timeOfRecording"></param>
    /// <param name="events"></param>
    public void RecordEvents(float timeOfRecording, List<CutsceneEvent> events)
    {
        string lastEventRead = string.Empty;
        try
        {
            int recordedTime = ConvertTimeToKey(timeOfRecording);
            List<RewindData> rewindDatas;
            if (m_TimeData.ContainsKey(recordedTime))
            {
                rewindDatas = m_TimeData[recordedTime];
            }
            else
            {
                rewindDatas = new List<RewindData>();
                m_TimeData.Add(recordedTime, rewindDatas);
            }

            foreach (CutsceneEvent ce in events)
            {
                lastEventRead = ce.Name;
                if (ce.Enabled && ce.HasDataToRecord())
                {
                    rewindDatas.Add(new RewindData(ce, ce.SaveRewindData()));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Failed to record event {0}. Error: {1}", lastEventRead, e.Message));
        }
        //int datapoint = 10;
        //m_TimeData.Keys.Where(w => w > start && w < end);
    }

    /// <summary>
    /// Loads and sets the state of each event at the specified time
    /// </summary>
    /// <param name="timeOfRecording"></param>
    public void LoadEventStatesAtTime(float timeOfRecording)
    {
        string lastEventRead = string.Empty;
        try
        {
            // find the closest key based on the timeOfRecording
            List<RewindData> rewindDataList = null;
            int keyToTest = ConvertTimeToKey(timeOfRecording);
            if (!m_TimeData.TryGetValue(keyToTest, out rewindDataList))
            {
                return;
            }

            /*m_TimeData.Keys.Zip(m_TimeData.Keys.Skip(1), 
                  (a, b) => new { a, b })
             .Where(x => x.a <= keyToTest && x.b >= keyToTest)
             .FirstOrDefault();*/

            // this needs to be optimized
            /*int[] keys = m_TimeData.Keys.ToArray();
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (keys[i] < keyToTest && keys[i + 1] > keyToTest)
                {
                    // found the boundaries
                    if ()
                    {

                    }
                    break;
                }
            }*/

            // this needs to be optimized
            foreach (RewindData rewindData in rewindDataList)
            {
                lastEventRead = rewindData.Event.Name;
                rewindData.Event.LoadRewindData(rewindData.Data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Failed to record event {0}. Error: {1}", lastEventRead, e.Message));
        }
    }

    int ConvertTimeToKey(float time)
    {
        return (int)(time * 100);
    }

    #endregion
}
