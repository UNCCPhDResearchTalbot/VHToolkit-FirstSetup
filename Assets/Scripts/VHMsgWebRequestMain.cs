using UnityEngine;
using System;
using System.Collections;

public class VHMsgWebRequestMain : VHMain
{
    #region Constants
    enum Logos
    {
        Facebook,
        Twitter,
        GooglePlus,
        Reddit,
    }
    #endregion

    #region Variables
    public BMLEventHandler_Web m_BMLEventHandler;
    public VHMsgWebRequest vhmsg;
    public SpeechBox_Web m_SpeechBox;
    public Texture[] m_Logos;
    public string m_WebPage = "http://uperf/vhweb/vhweb.html";
    public float m_SocialMediaWidth = 500;
    public float m_SocialMediaHeight = 300;
    //int m_bmlId = 42;
    //AudioClip m_MicAudio;
    #endregion

    #region Properties
    #endregion

    #region Functions
    public override void Start()
    {
        //base.Start();
        Application.runInBackground = true;
        
        vhmsg.SubscribeMessage("vrSpeak");
        vhmsg.SubscribeMessage("vrExpress");
        vhmsg.SubscribeMessage("vrAgentBML");
        vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));
        
        m_Console.AddCommandCallback("vhmsg", new DebugConsole.ConsoleCallback(HandleConsoleMessage));

        Array.ForEach<string>(Microphone.devices, mic => Debug.Log("Device: " + mic));
        
#if UNITY_WEBPLAYER
        StartCoroutine(WaitForMicConfirmation());
#endif
    }

    IEnumerator WaitForMicConfirmation()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
        //if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone))
        //{

        //}
        //else
        //{

        //}
    }

    void Update()
    {
        if (Network.isClient)
        {
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                DoMicInput();
            }
        }
    }

    public override void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button(m_Logos[(int)Logos.Facebook]))
            {
                Application.ExternalEval(string.Format("window.open('https://www.facebook.com/sharer/sharer.php?u={0}','_blank','width={1},height={2}')", m_WebPage, m_SocialMediaWidth, m_SocialMediaHeight));
            }
            if (GUILayout.Button(m_Logos[(int)Logos.Twitter]))
            {
                Application.ExternalEval(string.Format("window.open('https://twitter.com/intent/tweet?original_referer={0}&text={1}&url={0}','_blank','width={2},height={3}')", m_WebPage, "Virtual Humans Web App", m_SocialMediaWidth, m_SocialMediaHeight));
            }
            if (GUILayout.Button(m_Logos[(int)Logos.GooglePlus]))
            {
                Application.ExternalEval(string.Format("window.open('https://plus.google.com/share?url={0}','_blank','width={1},height={2}')", m_WebPage, m_SocialMediaWidth, m_SocialMediaHeight));
            }
            if (GUILayout.Button(m_Logos[(int)Logos.Reddit]))
            {
                Application.ExternalEval(string.Format("window.open('https://www.reddit.com/submit?url={0}','_blank','width={1},height={2}')", m_WebPage, m_SocialMediaWidth, m_SocialMediaHeight));
            }
        }
        GUILayout.EndHorizontal();  
        if (GUILayout.Button("Virtual Humans Website"))
        {
            Application.ExternalEval("window.open('https://vhtoolkit.ict.usc.edu/','_blank')");
        }
    }

    void DoMicInput()
    {
        if (Microphone.devices.Length < 0)
        {
            return;
        }

        string recordingDeviceName = Microphone.devices[0];
        if (Input.GetMouseButton(0))
        {
            if (!Microphone.IsRecording(recordingDeviceName))
            {
                /*m_MicAudio = */Microphone.Start(recordingDeviceName, true, 10, 44100);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Microphone.IsRecording(recordingDeviceName))
            {
                Microphone.End(recordingDeviceName);
            }
        }
    }

    public void VHMsg_MessageEvent(object sender, VHMsgBase.Message message)
    {
        string[] splitargs = message.s.Split(" ".ToCharArray());
        Debug.Log("VHMsg_MessageEvent: " + message.s);

        if (splitargs[0] == "vrSpeak" || splitargs[0] == "vrAgentBML")
        {
            if (splitargs.Length > 4)
            {
                if (splitargs[3] == "start" || splitargs[3] == "end")
                {
                    return;
                }

                string character = splitargs[1];
                string xml = string.Join(" ", splitargs, 4, splitargs.Length - 4);

                //if (character == "Brad")
                {
                    m_BMLEventHandler.LoadXMLString(character, xml);
                }
            }
        }
    }

    /// <summary>
    /// called from the console when a 'vhmsg' prefixed command is sent
    /// </summary>
    /// <param name="commandEntered"></param>
    /// <param name="console"></param>
    protected override void HandleConsoleMessage(string commandEntered, DebugConsole console)
    {
        base.HandleConsoleMessage(commandEntered, console);
        if (commandEntered.IndexOf("vhmsg") != -1)
        {
            string opCode = string.Empty;
            string args = string.Empty;
            if (console.ParseVHMSG(commandEntered, ref opCode, ref args))
            {
                if (Network.isServer)
                {
                    //m_MessageBroker.ServerSendsMessageToFIFOClient(opCode);
                }
                else
                {
                    //m_MessageBroker.ClientSendsMessageToServerFIFO(m_ClientId, opCode);
                }
            }
            else
            {
                console.AddTextToLog(commandEntered + " requires an opcode string and can have an optional argument string");
            }
        }
    }
    #endregion
}
