using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class BMLEventHandler : MonoBehaviour
{
    #region Variables
    public ICharacterController m_CharacterController;
    public Cutscene m_CutscenePrefab;
    BMLParser m_BMLParser;
    #endregion

    #region Functions
    void Start()
    {
        m_BMLParser = new BMLParser(OnParsedBMLTiming, OnParsedVisemeTiming, OnParsedBMLEvent, OnFinishedReading);
    }

    public bool LoadXMLString(string character, string xmlStr)
    {
        return m_BMLParser.LoadXMLString(character, xmlStr);
    }

    void OnParsedBMLTiming(string id, float time, string text) { }
    void OnParsedVisemeTiming(string visemeName, float startTime, float endTime) { }
    void OnParsedBMLEvent(XmlTextReader reader, string eventType, CutsceneEvent ce)
    {
        if (eventType == "animation" || eventType == "sbm:animation")
        {
            ce.ChangedEventFunction("PlayAnim", 2);
            ce.SetParameters(reader);
        }
        else if (eventType == "speech")
        {
            ce.ChangedEventFunction("PlayAudio", 5);
            ce.SetParameters(reader);
        }
    }

    void OnFinishedReading(bool succeeded, List<CutsceneEvent> createdEvents)
    {
        Cutscene cs = (Cutscene)Instantiate(m_CutscenePrefab);

        foreach (CutsceneEvent ce in createdEvents)
        {
            ce.SetMetaData(m_CharacterController);
            cs.AddEvent(ce);
        }
        
        cs.Play();
        cs.AddOnFinishedCutsceneCallback(OnFinishedCutscene);
    }

    void OnFinishedCutscene(Cutscene cs)
    {
        Destroy(cs.gameObject);
    }
    #endregion
}
