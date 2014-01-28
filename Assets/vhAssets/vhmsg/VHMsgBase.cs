using UnityEngine;
using System.Collections.Generic;

/// <remarks>
/// VHMsgBase is the base class that VHMsgEmulator and VHMsgManager derive from.
/// </remarks>
abstract public class VHMsgBase : MonoBehaviour
{
    public const char MsgSplitterChar = '*';

    /// <remarks>
    /// This is the class that contains the message when received via the MessageEvent handler
    /// It is received as an argument from MessageEvent
    /// </remarks>
    public class Message : System.EventArgs
    {
        /// <summary>
        /// String containing the message
        /// </summary>
        public string s;

        /// <summary>
        /// properties containing the multikey portion
        /// </summary>
        public Dictionary<string, string> properties;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="s"></param>
        /// <param name="properties"></param>
        public Message(string s, Dictionary<string, string> properties)
        {
            this.s = s;
            this.properties = properties;
        }
    }

    protected List<MessageEventHandler> m_RegisteredMessageCallbacks = new List<MessageEventHandler>();
    static VHMsgBase _VHMsgBase;

    public static VHMsgBase Get()
    {
        if (_VHMsgBase == null)
        {
            _VHMsgBase = Object.FindObjectOfType(typeof(VHMsgBase)) as VHMsgBase;
        }

        return _VHMsgBase;
    }

    /// <summary>
    /// Delegate for the MessageEvent handler
    /// </summary>
    public delegate void MessageEventHandler(object sender, Message message);

    public virtual void AddMessageEventHandler(MessageEventHandler handler) { }
    public virtual void RemoveMessageEventHandler(MessageEventHandler handler) {}
    public virtual void SubscribeMessage(string req) { }
    public virtual void SendVHMsg(string opandarg) { }
    public virtual void SendVHMsg(string op, string args) { }
    public virtual void SendVHMsg(string op, string[] args) { }
    public virtual void ReceiveVHMsg(string opandarg) { }


    public static KeyValuePair<string, string> SplitIntoOpArg(string message)
    {
        // some functions want the first word split from the rest of the message
        // this convenience function does that

        string [] split = message.Split(" ".ToCharArray(), 2);
        if (split.Length == 1)
            return new KeyValuePair<string,string>(split[0], "");
        else
            return new KeyValuePair<string,string>(split[0], split[1]);
    }
}
