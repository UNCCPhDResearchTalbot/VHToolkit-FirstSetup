using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public class UnitySequencerIO
{
    #region Variables
    CutsceneEditor m_Timeline;
    BMLParser m_BMLParser;
    #endregion

    #region Properties
    CutsceneEditor Timeline
    {
        get { return m_Timeline; }
    }
    #endregion

    #region Functions
    public UnitySequencerIO(CutsceneEditor sequencer)
    {
        m_Timeline = sequencer;
        m_BMLParser = new BMLParser(Timeline.AddBmlTiming, Timeline.AddVisemeTiming, ParsedBMLEvent, OnFinishedReading);
    }

    public bool LoadXMLString(string character, string xmlStr)
    {
        return m_BMLParser.LoadXMLString(character, xmlStr);
    }

    public bool LoadFile(string filePathAndName)
    {
        return m_BMLParser.LoadFile(filePathAndName);
    }

    void OnFinishedReading(bool succeeded, List<CutsceneEvent> createdEvents)
    {
        // The sendvhmsg events from the parser have the correct timings, so copy them over
        // for the timeline event to use
        foreach (CutsceneEvent ce in createdEvents)
        {
            if (ce.FunctionName == "SendVHMsg")
            {
                CutsceneEvent timeLineEvent = Timeline.FindEventByID(ce.UniqueId);
                timeLineEvent.StartTime = ce.StartTime;
            }
        }
    }

    public void ParsedBMLEvent(XmlTextReader reader, string type, CutsceneEvent ce)
    {
        const float minPosition = TimelineWindow.TrackStartingY + TimelineWindow.TrackHeight * 2; // the first 2 tracks are reserved
        float eventYPos = minPosition; // the first 2 tracks are reserved
        if (!string.IsNullOrEmpty(reader["ypos"]))
        {
            eventYPos = float.Parse(reader["ypos"]);
        }

        if (eventYPos < minPosition)
        {
            eventYPos = minPosition;
        }

        Vector2 eventPos = new Vector2(Timeline.GetPositionFromTime(Timeline.StartTime, Timeline.EndTime, ce.StartTime, Timeline.m_TrackScrollArea), eventYPos);
        CutsceneEvent newEvent = Timeline.CreateEventAtPosition(eventPos) as CutsceneEvent;

        ce.CloneData(newEvent);
        newEvent.m_UniqueId = ce.UniqueId;

        Timeline.ChangedCutsceneEventType(ce.EventType, newEvent);
        Timeline.ChangedEventFunction(newEvent, ce.FunctionName, ce.FunctionOverloadIndex);
        Timeline.CalculateTimelineLength();
        newEvent.GuiPosition.width = Timeline.GetWidthFromTime(newEvent.EndTime, newEvent.GuiPosition.x);
        newEvent.SetParameters(reader);

        // try to setup the reference to the character on the event using xml data
        if (newEvent.EventType == GenericEventNames.SmartBody)
        {
            CutsceneEventParam characterParam = newEvent.FindParameter("character");
            characterParam.SetObjData(ce.FindParameter("character").objData);
        }

        // setup the length
        float length = ce.Length;
        CutsceneEventParam lengthParam = newEvent.GetLengthParameter();
        if (lengthParam != null)
        {
            lengthParam.SetLength(length);
            newEvent.SetEventLengthFromParameter(lengthParam.Name);
        }
    }

    public void CreateXml(string filePathAndName, List<CutsceneEvent> Events)
    {
        StreamWriter outfile = null;

        try
        {
            outfile = new StreamWriter(string.Format("{0}", filePathAndName));
            outfile.WriteLine(@"<?xml version=""1.0""?>");
            outfile.WriteLine(@"<act>");
            outfile.WriteLine(@"  <bml xmlns:sbm=""http://sourceforge.net/apps/mediawiki/smartbody/index.php?title=SmartBody_BML"">");
            outfile.WriteLine(string.Format(@"  <speech id=""visSeq_3"" ref=""{0}"" type=""application/ssml+xml"" />", Path.GetFileNameWithoutExtension(filePathAndName)));

            foreach (CutsceneEvent ce in Events)
            {
                string xmlString = ce.GetXMLString();
                if (!string.IsNullOrEmpty(xmlString))
                {
                    outfile.WriteLine(string.Format("    {0}", xmlString));
                }
            }

            outfile.WriteLine(@"  </bml>");
            outfile.WriteLine(@"</act>");
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("CreateXml failed: {0}", e.Message));
        }
        finally
        {
            if (outfile != null)
            {
                outfile.Close();
            }
        }
    }

    public void ListenToNVBG(bool listen)
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg == null)
        {
            Debug.LogError(string.Format("Machinima Maker can't listen to NVBG because there is no VHMsgManager in the scene"));
            return;
        }

        if (listen)
        {
            vhmsg.SubscribeMessage("vrSpeak");
            vhmsg.RemoveMessageEventHandler(VHMsg_MessageEvent);
            vhmsg.AddMessageEventHandler(VHMsg_MessageEvent);
        }
        else
        {
            vhmsg.RemoveMessageEventHandler(VHMsg_MessageEvent);
        }
    }

    void VHMsg_MessageEvent(object sender, VHMsgBase.Message message)
    {
        Debug.Log("msg received: " + message.s);
        string[] splitargs = message.s.Split(" ".ToCharArray());
        if (splitargs[0] == "vrSpeak")
        {
            if (splitargs.Length > 4)
            {
                Timeline.AddCutscene();
                string character = splitargs[1];
                string xml = String.Join(" ", splitargs, 4, splitargs.Length - 4);
                m_BMLParser.LoadXMLString(character, xml);
            }
        }
    }
    #endregion
}
