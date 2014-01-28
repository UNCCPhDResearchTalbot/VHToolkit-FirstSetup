using UnityEngine;
using System.Collections.Generic;

#if !UNITY_WEBPLAYER && !UNITY_IPHONE && !UNITY_ANDROID

/// <remarks>
/// VHMsgManager implements the C# VHMsg interface within Unity.  It is disabled in Web Player because
/// the Network namespace is not available in the Web Player.  See VHMsgEmulator.
/// </remarks>
public class VHMsgManager : VHMsgBase
{
    public bool m_UseSpecifiedHostAndPort = false;   //  NOTE:  this flag (and host/port setting) doesn't work if you are using Smartbody!  You must use VHMSG_SERVER env variable.
    public string m_Host = "localhost";
    public string m_Port = "61616";
    public string[] m_messagesToSendAtQuit;

    protected VHMsg.Client vhmsg;

    public override void AddMessageEventHandler(MessageEventHandler handler)
    {
        m_RegisteredMessageCallbacks.Add(handler);
    }

    public override void RemoveMessageEventHandler(MessageEventHandler handler)
    {
        m_RegisteredMessageCallbacks.Remove(handler);
    }

    public bool IsVHMsgNull
    {
        get { return vhmsg == null; }
    }

    public void Awake()
    {
        OpenConnection();
    }

    void MessageEventTranslatorCallBack(object sender, VHMsg.Message args)
    {
        // convert the message from VHMsg.Message to VHMsgBase.Message
        VHMsgBase.Message baseMessage = new Message(args.s, args.properties);

        for (int i = 0; i < m_RegisteredMessageCallbacks.Count; i++)
        {
            m_RegisteredMessageCallbacks[i].Invoke(m_RegisteredMessageCallbacks[i].Target, baseMessage);
        }
    }

    public void Update()
    {
        vhmsg.Poll();
    }

    public void OnApplicationQuit()
    {
        for (int i = 0; i < m_messagesToSendAtQuit.Length; i++)
        {
            SendVHMsg(m_messagesToSendAtQuit[i]);
        }

        CloseConnection();
    }

    void OpenConnection()
    {
        CloseConnection();

        vhmsg = new VHMsg.Client();
        if (m_UseSpecifiedHostAndPort)
        {
            vhmsg.OpenConnection(m_Host, m_Port);
        }
        else
        {
            vhmsg.OpenConnection();
        }

        vhmsg.EnablePollingMethod();

        vhmsg.MessageEvent += new VHMsg.Client.MessageEventHandler(MessageEventTranslatorCallBack);
    }

    public void CloseConnection()
    {
        if (vhmsg != null)
        {
            vhmsg.CloseConnection();
            //vhmsg.Dispose();
            vhmsg = null;
        }
    }

    void OnEnable()
    {
        OpenConnection();
    }

    void OnDisable()
    {
        CloseConnection();
    }

    public void OnDestroy()
    {
        CloseConnection();
    }

    public override void SubscribeMessage(string req)
    {
        vhmsg.SubscribeMessage(req);
    }

    public override void SendVHMsg(string opandarg)
    {
        vhmsg.SendMessage(opandarg);
    }

    public override void SendVHMsg(string op, string args)
    {
        vhmsg.SendMessage(op, args);
    }

    public override void SendVHMsg(string op, string[] args)
    {
        vhmsg.SendMessage(op, args);
    }

    public override void ReceiveVHMsg(string opandarg)
    {
        Debug.LogError("ReceiveVHMsg shouldn't be getting called on VHMsg Manager");
    }
}

#else

public class VHMsgManager : VHMsgBase
{
    public bool m_UseSpecifiedHostAndPort = false;   //  NOTE:  this flag (and host/port setting) doesn't work if you are using Smartbody!  You must use VHMSG_SERVER env variable.
    public string m_Host = "localhost";
    public string m_Port = "61616";
    public string[] m_messagesToSendAtQuit;

    public bool IsVHMsgNull
    {
        get { return false; }
    }
    public override void AddMessageEventHandler(MessageEventHandler handler) {}
    public override void RemoveMessageEventHandler(MessageEventHandler handler) {}
    public void Awake() {}
    public void Update() { }
    public void OnApplicationQuit() {}
    override public void SubscribeMessage(string req) {}
    override public void SendVHMsg(string opandarg) {}
    override public void SendVHMsg(string op, string args) {}
    override public void SendVHMsg(string op, string[] args) {}
    override public void ReceiveVHMsg(string opandarg){}
}

#endif
