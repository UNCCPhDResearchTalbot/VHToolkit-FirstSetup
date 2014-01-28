using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericEventNames
{
    public const string Animation = "Animation";
    public const string Audio = "Audio";
    public const string Camera = "Camera";
    public const string SmartBody = "SmartBody";
    public const string Timed = "Timed";
    public const string Custom = "Custom";
    public const string Common = "Common";
    public const string Renderer = "Renderer";

    public static bool IsCustomEvent(string eventType)
    {
        return eventType == Custom;
    }
}

public struct RewindData
{
    public CutsceneEvent Event;
    public object Data;

    public RewindData(CutsceneEvent _Event, object _Data)
    {
        Event = _Event;
        Data = _Data;
    }
}

public struct TransformData
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public TransformData(Vector3 _Position, Vector3 _Rotation, Vector3 _Scale)
    {
        Position = _Position;
        Rotation = _Rotation;
        Scale = _Scale;
    }
}

public class BMLData
{
    public string m_TimeId = "";
    public float m_Time;
    public string m_Text = "";
    public string m_SeqEventUniqueId = "";

    public BMLData(string timeId, float time, string text, string uniqueId)
    {
        m_TimeId = timeId;
        m_Time = time;
        m_Text = text;
        m_SeqEventUniqueId = uniqueId;
    }
}

#region CutsceneData
[System.Serializable]
public class CutsceneData
{
    [HideInInspector]
    public float Version = MachinimaSaveData.MachinimaVersionNumber;

    [HideInInspector]
    public string CutsceneName;

    [HideInInspector]
    public float StartTime;

    [HideInInspector]
    public float Length = 1;

    [HideInInspector]
    public bool Loop;

    [HideInInspector]
    public int LoopCount;
    public List<CutsceneEvent> Events = new List<CutsceneEvent>();

    [HideInInspector]
    public int Order;

    //[HideInInspector]
    //public List<CutsceneTrackGroup> TrackGroups = new List<CutsceneTrackGroup>();

    public CutsceneData() { }

    public CutsceneData(Cutscene cutscene)
    {
        CutsceneName = cutscene.CutsceneName;
        StartTime = cutscene.StartTime;
        Length = cutscene.Length;
        Loop = cutscene.Loop;
        LoopCount = cutscene.LoopCount;
        Order = cutscene.Order;
        Events = cutscene.CutsceneEvents;
    }
}

[System.Serializable]
public class MachinimaSaveData
{
    public const float MachinimaVersionNumber = 1.0f;
    public float Version = MachinimaVersionNumber;
    public List<CutsceneData> CutsceneDatas = new List<CutsceneData>();
    public MachinimaSaveData() { }

    public void Clear()
    {
        CutsceneDatas.Clear();
    }
}
#endregion

#region Sequencer Data
[System.Serializable]
public class SequenceData
{
    [HideInInspector]
    public string m_XmlTitle = "";

    [HideInInspector]
    public UnitySmartbodyCharacter m_TargetCharacter;

    [HideInInspector]
    public List<BMLData> m_BMLData = new List<BMLData>();

    public SequenceData() { }
}

[System.Serializable]
public class SequenceSaveData
{
    public const float CurrentVersion = 1.0f;
    public float Version = SequenceSaveData.CurrentVersion;
    public List<SequenceSaveData> SequenceDatas = new List<SequenceSaveData>();

    public SequenceSaveData() { }

    public void Clear()
    {
        SequenceDatas.Clear();
    }
}
#endregion