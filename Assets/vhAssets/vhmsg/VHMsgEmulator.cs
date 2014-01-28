using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <remarks>
/// VHMsgEmulator uses VHMsgNetwork to emulate the functionality provided in VHMsgManager.  Use it for when you want to send/receive
/// messages over VHMsg but you don't have access to the required code (for example in the Web Player).  VHMsgNetwork uses Unity's Network
/// class for implementing this.
/// </remarks>
public class VHMsgEmulator : VHMsgBase
{
    public string m_RemoteIP = "127.0.0.1";
    public int m_RemotePort = 25000;
    //public VHMsgNetwork network;
    public GameObject network;

    //Dictionary<string, List<VHMsg.Client.MessageEventHandler>> m_RegisteredMessages = new Dictionary<string, List<VHMsg.Client.MessageEventHandler>>();
    List<string> m_RegisteredMessages = new List<string>();
    List<string> m_QueuedMessages = new List<string>();

    public override void AddMessageEventHandler(MessageEventHandler handler)
    {
        if (!m_RegisteredMessageCallbacks.Contains(handler))
        {
            m_RegisteredMessageCallbacks.Add(handler);
        }
    }

    public override void SubscribeMessage(string req)
    {
        if (!m_RegisteredMessages.Contains(req))
        {
            m_RegisteredMessages.Add(req);
        }
    }

    public void Update()
    {
        Poll();
    }

    public override void SendVHMsg(string opandarg)
    {
        network.SendMessage("ClientSendsMessageToServer", opandarg);
        //network.BroadcastVHMsg(opandarg, RPCMode.Server);
        //networkView.RPC("ReceiveVHMsg", RPCMode.Server, opandarg);
    }

    public override void SendVHMsg(string op, string args)
    {
        network.SendMessage("ClientSendsMessageToServer", op + " " + args);
        //network.BroadcastVHMsg(op + " " + args, RPCMode.Server);
    }

    public override void SendVHMsg(string op, string[] args)
    {
        string combinedArgs = string.Empty;
        for (int i = 0; i < args.Length; i++)
        {
            combinedArgs += " " + args[i];
        }

        network.SendMessage("ClientSendsMessageToServer", combinedArgs);
        //network.BroadcastVHMsg(combinedArgs, RPCMode.Server);
    }

    void Poll()
    {
        for (int i = 0; i < m_QueuedMessages.Count; i++)
        {
            Message message = new Message(m_QueuedMessages[i], new Dictionary<string, string>());

            for (int j = 0; j < m_RegisteredMessageCallbacks.Count; j++)
            {
                m_RegisteredMessageCallbacks[j](this, message);
            }
        }

        m_QueuedMessages.Clear();
    }

    public override void ReceiveVHMsg(string opandarg)
    {
        // parse out the opcode
        int opCodeIndex = -1;
        string opCode = string.Empty;
        opCodeIndex = opandarg.IndexOf(" ");
        if (opCodeIndex > -1)
        {
            opCode = opandarg.Substring(0, opCodeIndex);
        }
        else
        {
            opCode = opandarg;
        }

        // check to see if we have this opcode registered
        if (m_RegisteredMessages.Contains(opCode) || m_RegisteredMessages.Contains("*"))
        {
            m_QueuedMessages.Add(opandarg);
        }
    }

    void OnConnectedToServer()
    {
        Debug.Log("VHMsgEmulator connected to server");
    }

    void OnDisconnectedFromServer()
    {
        Debug.Log("VHMsgEmulator disconnected from server");
    }

    void OnFailedToConnect()
    {
        Debug.LogError("VHMsgEmulator failed to connect to server");
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        Debug.Log("VHMsgEmulator OnNetworkInstantiate");
    }
}
