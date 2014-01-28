using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <remarks>
/// VHMsgEmulator uses VHMsgNetwork to emulate the functionality provided in VHMsgManager.  Use it for when you want to send/receive
/// messages over VHMsg but you don't have access to the required code (for example in the Web Player).  VHMsgNetwork uses Unity's Network
/// class for implementing this.
/// </remarks>
public class VHMsgWebRequest : VHMsgBase
{
    public string m_url = "";
    public List<string> m_urlArgs = new List<string>();
    //public string m_RemoteIP = "127.0.0.1";
    //public int m_RemotePort = 25000;
    List<string> m_RegisteredMessages = new List<string>();
    List<string> m_QueuedMessages = new List<string>();
    Dictionary<string, string> m_UrlParamValueMap = new Dictionary<string, string>();

    void Start()
    {
        for (int i = 0; i < m_urlArgs.Count; i++)
        {
            SetUrlParam(m_urlArgs[i], "0");
        }
    }

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

    public void SetUrlParam(string paramName, string paramValue)
    {
        //paramValue = paramValue.Replace(" ", "%20");
        if (!m_UrlParamValueMap.ContainsKey(paramName))
        {
            m_UrlParamValueMap.Add(paramName, paramValue);
        }
        else
        {
            m_UrlParamValueMap[paramName] = paramValue;
        }
    }

    public void Update()
    {
        Poll();
    }

    public override void SendVHMsg(string opandarg)
    {
        StartCoroutine(SendVHmsgRoutine(opandarg));
    }

    public override void SendVHMsg(string op, string args)
    {
        StartCoroutine(SendVHmsgRoutine(op + " " + args));
    }

    public override void SendVHMsg(string op, string[] args)
    {
        string combinedArgs = string.Empty;
        for (int i = 0; i < args.Length; i++)
        {
            combinedArgs += " " + args[i];
        }
        StartCoroutine(SendVHmsgRoutine(combinedArgs));
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

    string GetUrlString(string message)
    {
        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, string> kvp in m_UrlParamValueMap)
        {
            builder.Append(string.Format("{0}={1}&", kvp.Key, kvp.Value));
        }

        builder.Append(string.Format("vhmsg={0}", message.Replace(" ", "%20")));
        return string.Format("{0}?{1}", m_url, builder.ToString());
    }

    IEnumerator SendVHmsgRoutine(string message)
    {
        string fullUrl = GetUrlString(message);
        WWW www = new WWW(fullUrl);

        yield return www;
    }
}
