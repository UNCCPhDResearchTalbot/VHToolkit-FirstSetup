using UnityEngine;
using System;
using System.Collections;

public class MessageBrokerMain : VHMain
{
    #region Constants
    
    #endregion

    #region Variables
    public BMLEventHandler m_BMLEventHandler;
    public bool m_IsServer;
    public GameObject m_NetworkPrefab;
    public int m_MaxConnections = 8;
    public int m_ListenPort = 25000;
    public bool m_UseNat = false;
    public NPCEditorMessageBroker m_MessageBroker;
    public VHMsgBase vhmsg;
    public SpeechBox_Web m_SpeechBox;
    NetworkPlayer m_networkPlayer;
    string m_ClientId = "";
    string m_IP = "127.0.0.1";
    int m_bmlId = 42;
    //AudioClip m_MicAudio;
    #endregion

    #region Properties
    public string ClientId
    {
        get { return m_ClientId; }
    }
    #endregion

    #region Functions
    public override void Start()
    {
        //base.Start();
        Application.runInBackground = true;
        if (m_IsServer)
        {
            vhmsg.SubscribeMessage("*");
            vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent_Server));
        }
        else
        {
            vhmsg.SubscribeMessage("vrSpeak");
            vhmsg.SubscribeMessage("vrExpress");
            vhmsg.SubscribeMessage("vrAgentBML");
            vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent_Client));
        }
  
        m_Console.AddCommandCallback("vhmsg", new DebugConsole.ConsoleCallback(HandleConsoleMessage));

        if (m_SpeechBox != null)
        {
            //m_SpeechBox.Show = false;
        }

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

    public override void OnGUI()
    {
        base.OnGUI();

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            if (!m_IsServer)
            {
                m_IP = GUILayout.TextField(m_IP);
                if (GUILayout.Button("Connect"))
                {
                    Network.Connect(m_IP, m_ListenPort);
                    m_SpeechBox.Show = true;
                } 
            }
            else
            {
                if (GUILayout.Button("Start Server"))
                {
                    Network.InitializeServer(m_MaxConnections, m_ListenPort, m_UseNat);
                }
            }
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                Network.Disconnect(200);
                if (!m_IsServer)
                {
                    m_SpeechBox.Show = false;
                }
            }

            if (m_IsServer)
            {
                DrawServerGUI();
            }
        }
    }

    void DrawServerGUI()
    {
        string character2 = "Brad";

        if (GUILayout.Button("BML1"))
        {
            string bmlIdString = "renderer_bml_" + m_bmlId++;
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                            @"<act>" +
                            @"<bml>" +
                            @"<animation name=""ChrBrad@Idle01_ChopLf01"" start=""1.0"" />" +
                            @"</bml>" +
                            @"</act>";

            m_MessageBroker.ServerSendsMessageToAllClients(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
        }
        if (GUILayout.Button("BML2"))
        {
            string bmlIdString = "renderer_bml_" + m_bmlId++;
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                            @"<act>" +
                            @"<bml>" +
                            @"<head type=""NOD"" repeats=""2"" start=""0"" end=""2"" />" +
                            @"</bml>" +
                            @"</act>";

            m_MessageBroker.ServerSendsMessageToAllClients(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
        }
        if (GUILayout.Button("BML3"))
        {
            string bmlIdString = "renderer_bml_" + m_bmlId++;
            string xml =    @"<?xml version=""1.0"" encoding=""UTF-8""?>" + 
                            @"<act>" + 
                            @"<participant id=""Brad"" role=""actor""/>" + 
                            @"<bml>" + 
                            @"<speech id=""sp1"" ref=""brad_fullname"" type=""application/ssml+xml"">My full name is brad mathew smith</speech>" + 
                            @"</bml>" + 
                            @"</act>";

            m_MessageBroker.ServerSendsMessageToAllClients(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
        }
        if (GUILayout.Button("BML4"))
        {
            string bmlIdString = "renderer_bml_" + m_bmlId++;
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>" +
                            @"<act>" +
                            @"<participant id=""Brad"" role=""actor"" />" +
                            @"<bml>" +
                            @"<speech id=""sp1"" ref=""brad_fullname"" type=""application/ssml+xml"">" +
                            @"<mark name=""T0"" />My" +
                            @"<mark name=""T1"" /><mark name=""T2"" />full" +
                            @"<mark name=""T3"" /><mark name=""T4"" />name" +
                            @"<mark name=""T5"" /><mark name=""T6"" />is" +
                            @"<mark name=""T7"" /><mark name=""T8"" />Brad" +
                            @"<mark name=""T9"" /><mark name=""T10"" />Matthew" +
                            @"<mark name=""T11"" /><mark name=""T12"" />Smith." +
                            @"<mark name=""T13"" />" +
                            @"</speech>" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T1 My "" stroke=""sp1:T1"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T3 My full "" stroke=""sp1:T3"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T5 My full name "" stroke=""sp1:T5"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T7 My full name is "" stroke=""sp1:T7"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T9 My full name is Brad "" stroke=""sp1:T9"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T11 My full name is Brad Matthew "" stroke=""sp1:T11"" />" +
                            @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T13 My full name is Brad Matthew Smith. "" stroke=""sp1:T13"" />" +
                            @"<gaze participant=""Brad"" id =""gaze"" target=""Camera"" direction=""UPLEFT"" angle=""0"" sbm:joint-range=""HEAD EYES"" xmlns:sbm=""http://ict.usc.edu"" />" +
                            @"<event message=""vrSpoke Brad all 1366842060442-9-1 My full name is Brad Matthew Smith"" stroke=""sp1:relax"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" xmlns:sbm=""http://ict.usc.edu"" />" +
                            @"<!--Inclusivity-->" +
                            @"<head type=""SHAKE"" amount=""0.4"" repeats=""1.0"" velocity=""1"" start=""sp1:T4"" priority=""4"" duration=""1"" />" +
                            @"<!--Noun clause nod-->" +
                            @"<head type=""NOD"" amount=""0.20"" repeats=""1.0"" start=""sp1:T9"" priority=""5"" duration=""1"" />" +
                            @"<animation name=""ChrBrad@Idle01_Contemplate01"" start=""sp1:T8"" />" +
                            @"</bml>" +
                            @"</act>";

            m_MessageBroker.ServerSendsMessageToAllClients(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
        }
    }

    void SetClientId(string id)
    {
        m_ClientId = id;
    }

    public void VHMsg_MessageEvent_Client(object sender, VHMsgBase.Message message)
    {
        string[] splitargs = message.s.Split(" ".ToCharArray());
        Debug.Log("ClientVHMsgReceived: " + message.s);

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

    void VHMsg_MessageEvent_Server(object sender, VHMsgBase.Message message)
    {
        if (Network.isServer)
        {
            Debug.Log("Server VHMsg recvd: " + message.s);
            string[] splitargs = message.s.Split(" ".ToCharArray());

            if (splitargs[0] == "vrSpeak")
            {
                m_MessageBroker.ServerSendsMessageToFIFOClient(message.s);
            }
            else
            {
                m_MessageBroker.ServerSendsMessageToAllClients(message.s);
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
                    m_MessageBroker.ServerSendsMessageToFIFOClient(opCode);
                }
                else
                {
                    m_MessageBroker.ClientSendsMessageToServerFIFO(m_ClientId, opCode);
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
