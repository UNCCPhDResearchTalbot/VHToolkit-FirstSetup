using UnityEngine;
using System.Collections;

/// <remarks>
/// Implements VHMsg style connect/send/receive functionality, but using Unity's Network classes
/// instead of VHMsg itself.  Used in VHMsgEmulator, and used in the standalone app for acting as the middleman between VHMsg and Unity.
/// </remarks>
public class VHMsgNetwork : MonoBehaviour
{
    public DebugConsole console;
    //public GameObject vhmsgGO;
    public VHMsgBase vhmsg;

    public void Start()
    {
        Application.runInBackground = true;
        console = DebugConsole.Get();

        if (vhmsg != null)
        {
            vhmsg = (VHMsgBase)vhmsg.GetComponent(typeof(VHMsgBase));

            if (vhmsg != null)
            {
                vhmsg.SubscribeMessage("*");
                vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));
                console.AddCommandCallback("vhmsg", new DebugConsole.ConsoleCallback(HandleConsoleMessage));
            }
            else
            {
                Debug.LogError("vhmsgGO needs to have a monobehaviour script that implements the IVHMsg interface");
            }
        }

        //Network.logLevel = NetworkLogLevel.Full;
    }

    void VHMsg_MessageEvent(object sender, VHMsgBase.Message message)
    {
        if (Network.isServer)
        {
            Debug.Log("VHMsg recvd: " + message.s);
            BroadcastVHMsg(message.s, RPCMode.Others);
        }
    }

    /// <summary>
    /// sends an RPC message that contains vhmsg data to all connected clients
    /// </summary>
    /// <param name="data"></param>
    virtual public void BroadcastVHMsg(string opandarg, RPCMode mode)
    {
        networkView.RPC("ReceiveVHMsg", mode, opandarg);
    }

    public void Update()
    {

    }

    /// <summary>
    /// called remotely from the client via RPC
    /// </summary>
    /// <param name="opandarg"></param>
    [RPC]
    virtual public void ReceiveVHMsg(string opandarg, NetworkMessageInfo info)
    {
        if (Network.isClient)
        {
            vhmsg.ReceiveVHMsg(opandarg);
        }
        else
        {
            vhmsg.SendVHMsg(opandarg);
        }
    }

    /// <summary>
    /// called from the console when a 'vhmsg' prefixed command is sent
    /// </summary>
    /// <param name="commandEntered"></param>
    /// <param name="console"></param>
    void HandleConsoleMessage(string commandEntered, DebugConsole console)
    {
        if (commandEntered.IndexOf("vhmsg") != -1)
        {
            string opCode = string.Empty;
            string args = string.Empty;
            if (console.ParseVHMSG(commandEntered, ref opCode, ref args))
            {
                if (Network.isServer)
                {
                    vhmsg.SendVHMsg(opCode, args);
                }
                else
                {
                    BroadcastVHMsg(opCode, RPCMode.Server);
                }
            }
            else
            {
                console.AddTextToLog(commandEntered + " requires an opcode string and can have an optional argument string");
            }
        }
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
}
