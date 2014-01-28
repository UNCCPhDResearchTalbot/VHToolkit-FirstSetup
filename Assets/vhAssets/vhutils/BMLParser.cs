using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public class BMLParser
{
    #region Constants
    static readonly string[] EventXmlNames = 
    {
        "sbm:animation",
        "animation",
        "gaze",
        "head",
        "saccade",
        "face",
        "text",
        "event",
        "sbm:event",
        "speech",
    };

    //const string Speech = "speech";
    const string Participant = "participant";

    class CachedEvent
    {
        public CutsceneEvent ce;
        public string timing;

        public CachedEvent(CutsceneEvent _ce, string _timing)
        {
            ce = _ce;
            timing = _timing;
        }
    }

    class BMLTiming
    {
        public string id;
        public float time;
        public string text;

        public BMLTiming(string _id, float _time, string _text)
        {
            id = _id;
            time = _time;
            text = _text;
        }
    }

    public delegate void OnParsedBMLTiming(string id, float time, string text);
    public delegate void OnParsedVisemeTiming(string visemeName, float startTime, float endTime);
    public delegate void OnParsedBMLEvent(XmlTextReader reader, string eventType, CutsceneEvent ce);
    public delegate void OnFinishedReading(bool succeeded, List<CutsceneEvent> createdEvents);
    public delegate void OnReadBMLFile(string bmlFileName);
    #endregion

    #region Variables
    OnParsedBMLTiming m_ParsedBMLTimingCB;
    OnParsedVisemeTiming m_ParsedVisemeTimingCB;
    OnParsedBMLEvent m_ParsedBMLEventCB;
    OnFinishedReading m_FinishedReadingCB;
    OnReadBMLFile m_ReadBmlFileCB;
    List<CachedEvent> m_CachedEvents = new List<CachedEvent>();
    List<CutsceneEvent> m_CreatedEvents = new List<CutsceneEvent>();
    List<BMLTiming> m_BMLTimings = new List<BMLTiming>();
    string m_Character = "";
    string m_SpeechId = "";
    bool m_ReadBMLFile;
#if UNITY_WEBPLAYER && !UNITY_EDITOR
    bool m_BMLFileHasBeenRead;
#endif
    string m_BMLPath = "StreamingAssets/Sounds";
    string m_CachedXml = "";
    #endregion

    #region Properties
    public string BMLPath
    {
        get { return m_BMLPath; }
        set { m_BMLPath = value; }
    }
    #endregion

    #region Functions
    public BMLParser(OnParsedBMLTiming parsedBMLTimingCB, OnParsedVisemeTiming parsedVisemeTimingCB, OnParsedBMLEvent parsedBMLEventCB, OnFinishedReading finishedReadingCB)
    {
        m_ParsedBMLTimingCB = parsedBMLTimingCB;
        m_ParsedVisemeTimingCB = parsedVisemeTimingCB;
        m_ParsedBMLEventCB = parsedBMLEventCB;
        m_FinishedReadingCB = finishedReadingCB;
    }

    public void AddOnReadBMLFileCB(OnReadBMLFile cb)
    {
        m_ReadBmlFileCB += cb;
    }

    /// <summary>
    /// Loads and reads either a bml or xml file. Returns true if successfully read
    /// </summary>
    /// <param name="filePathAndName"></param>
    /// <returns></returns>
    public bool LoadFile(string filePathAndName)
    {
        bool success = false;
        string fileExt = Path.GetExtension(filePathAndName);
        if (fileExt.ToLower() == ".xml")
        {
            success = LoadXMLFile(filePathAndName);
        }
        else if (fileExt.ToLower() == ".bml")
        {
            success = LoadBMLFile(filePathAndName);
        }
        else
        {
            Debug.LogError(string.Format("Couldn't load {0} because it's not a supported file extension", filePathAndName));
        }

        return success;
    }

    /// <summary>
    /// Read a bml file, internal only
    /// </summary>
    /// <param name="filePathAndName"></param>
    /// <returns></returns>
    bool LoadBMLFile(string filePathAndName)
    {
        bool succeeded = true;
        FileStream xml = null;
        XmlTextReader reader = null;
        try
        {
            xml = new FileStream(filePathAndName, FileMode.Open, FileAccess.Read);
            reader = new XmlTextReader(xml);
            ReadBML(reader);
        }
        catch (Exception e)
        {
            succeeded = false;
            Debug.LogError(string.Format("Failed when loading {0}. Error: {1}", filePathAndName, e.Message));
        }
        finally
        {
            if (xml != null)
            {
                xml.Close();
            }
            if (reader != null)
            {
                reader.Close();
            }

            FinishedReadingBML(succeeded);
        }

        return succeeded;
    }

    public bool LoadBMLString(string bmlStr)
    {
        bool succeeded = true;
        XmlTextReader reader = null;
        StringReader bml = null;

        try
        {
            bml = new StringReader(bmlStr);
            bml.Read(); // skip BOM see this link for more detail: http://answers.unity3d.com/questions/10904/xmlexception-text-node-canot-appear-in-this-state.html
            reader = new XmlTextReader(bml);
            ReadBML(reader);
        }
        catch (Exception e)
        {
            succeeded = false;
            Debug.LogError(string.Format("Failed when loading. Error: {0} {1}. bmlStr {2}", e.Message, e.InnerException, bmlStr));
        }
        finally
        {
            if (bml != null)
            {
                bml.Close();
            }

            if (reader != null)
            {
                reader.Close();
            }

            FinishedReadingBML(succeeded);
        }

        return succeeded;
    }

    void ReadBML(XmlTextReader reader)
    {
        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == "sync")
                    {
                        string id = reader["id"];
                        float time = float.Parse(reader["time"]);
                        reader.ReadInnerXml(); // this is a hack, i do this so I can get to the text portion of the xml
                        if (m_ParsedBMLTimingCB != null)
                        {
                            BMLTiming bmlTiming = new BMLTiming(id, time, reader.Value.Trim());
                            m_BMLTimings.Add(bmlTiming);
                            m_ParsedBMLTimingCB(bmlTiming.id, bmlTiming.time, bmlTiming.text);
                        }
                    }
                    else if (reader.Name == "lips")
                    {
                        float start = float.Parse(reader["start"]);
                        float end = float.Parse(reader["end"]);
                        if (m_ParsedVisemeTimingCB != null)
                        {
                            m_ParsedVisemeTimingCB(reader["viseme"], start, end);
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the contents of an xml file as a string
    /// </summary>
    /// <param name="xmlStr"></param>
    /// <returns></returns>
    public bool LoadXMLString(string character, string xmlStr)
    {
        m_Character = character;
        bool succeeded = true;
        StringReader xml = null;
        XmlTextReader reader = null;
        m_CachedEvents.Clear();
        m_CreatedEvents.Clear();
        m_ReadBMLFile = true;
        m_CachedXml = xmlStr;

        try
        {
            xml = new StringReader(xmlStr);
            reader = new XmlTextReader(xml);
            ParseBMLEvents(reader);
        }
        catch (Exception e)
        {
            succeeded = false;
            Debug.LogError(string.Format("Failed when loading. Error: {0} {1}. couldn't load string {2}", e.Message, e.InnerException, xmlStr));
        }
        finally
        {
            if (xml != null)
            {
                xml.Close();
            }

            if (reader != null)
            {
                reader.Close();
            }
        }

        FinishedReadingXML(succeeded);
        return succeeded;
    }

    /// <summary>
    /// Reads an xml files. Internal use only
    /// </summary>
    /// <param name="filePathAndName"></param>
    /// <returns></returns>
    bool LoadXMLFile(string filePathAndName)
    {
        bool succeeded = true;
        m_ReadBMLFile = true;
        FileStream xml = null;
        XmlTextReader reader = null;

        try
        {
            xml = new FileStream(filePathAndName, FileMode.Open, FileAccess.Read);
            reader = new XmlTextReader(xml);
            ParseBMLEvents(reader);
        }
        catch (Exception e)
        {
            succeeded = false;
            Debug.LogError(string.Format("Failed when loading {0}. Error: {1} {2}", filePathAndName, e.Message, e.InnerException));
        }
        finally
        {
            if (xml != null)
            {
                xml.Close();
            }

            if (reader != null)
            {
                reader.Close();
            }
        }

        FinishedReadingXML(succeeded);
        return succeeded;
    }

    void FinishedReadingBML(bool succeeded)
    {
        if (!string.IsNullOrEmpty(m_CachedXml))
        {
#if UNITY_WEBPLAYER && !UNITY_EDITOR
            m_BMLFileHasBeenRead = true; // do this first
            LoadXMLString(m_Character, m_CachedXml);
#endif
        }
    }

    void FinishedReadingXML(bool succeeded)
    {
        // handled the cached events first
        m_CachedEvents.ForEach(c => HandleCachedEvent(c));
        
        // then do the callback
        if (m_FinishedReadingCB != null)
        {
            m_FinishedReadingCB(succeeded, m_CreatedEvents);
        }

        // now reset all the data
        m_BMLTimings.Clear();
        m_CachedEvents.Clear();
        m_CreatedEvents.Clear();
        m_Character = string.Empty;
        m_ReadBMLFile = false;
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        m_BMLFileHasBeenRead = false;
#endif
    }

    /// <summary>
    /// Reads the xml file line by line and creates events based off the node type listed in EventXmlNames
    /// </summary>
    /// <param name="reader"></param>
    void ParseBMLEvents(XmlTextReader reader)
    {
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        StringReader xml = null;
        // First we need to check if a BML file has to be loaded in order to find timing markers for events in the xml
        if (m_ReadBMLFile && !m_BMLFileHasBeenRead)
        {
            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "speech":
                        m_SpeechId = reader["id"];
                        WWW www = Utils.LoadStreamingAssetsAsync(string.Format("{0}/{1}.bml", m_BMLPath, reader["ref"]));
                        GameObject.Find("GenericEvents").GetComponent<MonoBehaviour>().StartCoroutine(WaitForBML(www));
                        return;
                }
            }

            // if you've gotten this far, the reader needs to be reset because it didn't find any speech
            reader.Close();
            xml = new StringReader(m_CachedXml);
            reader = new XmlTextReader(xml);
        }
#endif

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    int index = Array.FindIndex<string>(EventXmlNames, s => s == reader.Name.ToLower());
                    if (index != -1)
                    {
                        CreateEvent(reader, reader.Name);
                    }
                    else if (reader.Name.ToLower() == Participant)
                    {
                        if (string.IsNullOrEmpty(m_Character))
                        {
                            m_Character = reader["id"];
                        }
                    }
                    break;
            }
        }

#if UNITY_WEBPLAYER && !UNITY_EDITOR
        if (xml != null)
        {
            xml.Close();   
        }
#endif
    }

    float ParseEventStartTime(string startTime)
    {
        float eventStart = 0;
        if (!float.TryParse(startTime, out eventStart))
        {
            if (!string.IsNullOrEmpty(startTime))
            {
                // looks for timing markers that were read from the bml
                string[] split = startTime.Split(':');
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].IndexOf(m_SpeechId) != -1)
                    {
                        BMLTiming bmlTiming = m_BMLTimings.Find(t => t.id == split[i + 1]);
                        if (bmlTiming != null)
                        {
                            eventStart = bmlTiming.time;
                        }
                        break;
                    }
                }
            }
        }

        return eventStart;
    }

    CutsceneEvent CreateNewEvent(XmlTextReader reader)
    {
        float eventStart = 0;
        if (!string.IsNullOrEmpty(reader["start"]))
        {
            eventStart = ParseEventStartTime(reader["start"]);
        }
        else if (!string.IsNullOrEmpty(reader["stroke"]))
        {
            eventStart = ParseEventStartTime(reader["stroke"]);
        }

        CutsceneEvent ce = new CutsceneEvent(new Rect(), Guid.NewGuid().ToString());
        m_CreatedEvents.Add(ce);
        ce.Name = reader["id"];
        ce.StartTime = eventStart;

        // sets up the target gameobject and component
        ChangedCutsceneEventType(GenericEventNames.SmartBody, ce);

        return ce;
    }

    /// <summary>
    /// Creates an event and sets up it's parameters based on the xml data
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="type"></param>
    void CreateEvent(XmlTextReader reader, string type)
    {
        CutsceneEvent ce = CreateNewEvent(reader);

        switch (type)
        {
            case "sbm:animation":
            case "animation":
                ce.ChangedEventFunction("PlayAnim", 1);
                break;

            case "gaze":
                ce.ChangedEventFunction("GazeAdvanced");
                break;

            case "head":
                ce.ChangedEventFunction(reader["type"] == "NOD" ? "Nod" : "Shake");
                break;

            case "saccade":
                ce.ChangedEventFunction("Saccade");
                break;

            case "face":
                ce.ChangedEventFunction("PlayFAC");
                break;

            case "sbm:event":
            case "event":
                ParseVhmsgEvent(reader, type, ce);
                break;

            case "speech":
                m_SpeechId = reader["id"];
                ce.ChangedEventFunction("PlayAudio", 1);
#if !UNITY_WEBPLAYER || UNITY_EDITOR
                if (m_ReadBMLFile)
                {
                    LoadFile(string.Format("{0}/{1}/{2}.bml", Application.dataPath, m_BMLPath, reader["ref"]));
                } 
#endif
                break;
        }

        ce.SetParameters(reader);

        // try to setup the reference to the character on the event using xml data
        if (ce.EventType == GenericEventNames.SmartBody)
        {
            CutsceneEventParam characterParam = ce.FindParameter("character");
            if (characterParam != null)
            {
                if (characterParam.objData == null && !string.IsNullOrEmpty(m_Character))
                {
                    characterParam.SetObjData(SmartBodyEvents.SmartBodyEvent_Base.FindCharacter(m_Character, ce.Name));
                }
            }
            else
            {
                Debug.LogError(string.Format("Event {0} doesn't have a character param?", ce.Name));
            }
        }

        if (!string.IsNullOrEmpty(reader["relax"]))
        {
            float length = ParseEventStartTime(reader["relax"]);
            CutsceneEventParam lengthParam = ce.GetLengthParameter();
            if (lengthParam != null)
            {
                lengthParam.SetLength(length);
                ce.SetEventLengthFromParameter(lengthParam.Name);
            }
        }
       
        if (m_ParsedBMLEventCB != null)
        {
            m_ParsedBMLEventCB(reader, reader.Name, ce);
        }
    }

    IEnumerator WaitForBML(WWW www)
    {
        while (!www.isDone) { yield return new WaitForEndOfFrame(); Debug.Log("still waiting"); }
        //Debug.Log("www.text: " + www.text);
        LoadBMLString(www.text);
    }

    void ChangedCutsceneEventType(string newType, CutsceneEvent ce)
    {
        ce.EventType = newType;

        // TODO: THIS IS A HACK! get a reference to a generic events object!
        GenericEvents[] genericEventsGO = GameObject.Find("GenericEvents").GetComponentsInChildren<GenericEvents>();
        if (genericEventsGO == null)
        {
            Debug.LogError(string.Format("BMLParser doesn't have a GenericEvents componenent anywhere"));
            return;
        }

        MonoBehaviour targetComponent = null;
        foreach (GenericEvents ge in genericEventsGO)
        {
            if (ge.GetEventType() == newType)
            {
                targetComponent = ge;
                break;
            }
        }

        if (targetComponent != null)
        {
            ce.SetFunctionTargets(targetComponent.gameObject, targetComponent);
        }
        else
        {
            ce.SetFunctionTargets(null, null);
        }
    }

    void ParseVhmsgEvent(XmlTextReader xml, string type, CutsceneEvent ce)
    {
        string message = xml["message"];
        if (message.IndexOf("saccade") != -1)
        {
            // this is a saccade event
            ce.ChangedEventFunction("Saccade");
        }
        else
        {
            // event start times are usually based off of other events using event names. Because of this,
            // we need to cache this event, and later try to find the event that it's parented to
            if (!string.IsNullOrEmpty(xml["stroke"]))
            {
                m_CachedEvents.Add(new CachedEvent(ce, xml["stroke"]));
            }
            else if (!string.IsNullOrEmpty(xml["start"]))
            {
                m_CachedEvents.Add(new CachedEvent(ce, xml["start"]));
            }
            ChangedCutsceneEventType(GenericEventNames.Common, ce);
            ce.ChangedEventFunction("SendVHMsg");
        }
    }

    /// <summary>
    /// Called after all events have been read from the xml file. Handles timing adjustments
    /// for events that are timed based off of other events in the xml file
    /// </summary>
    /// <param name="cache"></param>
    void HandleCachedEvent(CachedEvent cache)
    {
        // typical format stroke=[event name]:start+[time offset]
        string[] plusSplit = cache.timing.Split('+');
        if (plusSplit.Length != 2)
        {
            return;
        }

        string[] colonSplit = plusSplit[0].Split(':');
        if (colonSplit.Length != 2)
        {
            return;
        }

        // the name of the event is the first half
        CutsceneEvent parentTimer = m_CreatedEvents.Find(ce => ce.Name == colonSplit[0]);
        if (parentTimer != null)
        {
            float offset;
            if (float.TryParse(plusSplit[1], out offset))
            {
                cache.ce.StartTime = parentTimer.StartTime + offset;
            }
        }
    }
    #endregion
}
