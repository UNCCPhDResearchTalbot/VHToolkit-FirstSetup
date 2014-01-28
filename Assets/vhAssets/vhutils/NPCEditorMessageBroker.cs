using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class NPCEditorMessageBroker : MonoBehaviour
{
    #region Constants
    public const string MsgConcatStart = "[msg:start:{0}]";
    public const string MsgConcatCont = "[msg:cont:{0}]";
    public const string MsgConcatEnd = "[msg:end:{0}]";
    const int MaxLength = 2000;
    #endregion

    #region Variables
    public VHMsgBase vhmsg;
    Dictionary<string, NetworkPlayer> m_ConnectedClients = new Dictionary<string, NetworkPlayer>();
    Queue<string> m_WaitingClients = new Queue<string>();
    StringBuilder m_CombinedMessage = new StringBuilder();
    public GameObject m_Receiver;
    #endregion 

    #region Functions
    void OnPlayerConnected(NetworkPlayer player)
    {
        m_ConnectedClients.Add(player.guid, player);
        //Network.Instantiate(m_NetworkPrefab, m_NetworkPrefab.transform.position, m_NetworkPrefab.transform.rotation, 0);
        Debug.Log("Player " + player.guid + " connected from " + player.ipAddress + ":" + player.port);
        networkView.RPC("ClientReceivesClientId", player, player.guid);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        m_ConnectedClients.Remove(player.guid);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    public void ServerSendsMessageToFIFOClient(string message)
    {
        if (m_WaitingClients.Count == 0)
        {
            Debug.LogWarning(string.Format("no clients are waiting to receive message {0}", message));
            ServerSendsMessageToAllClients(message);
        }
        else
        {
            string receivingClientId = m_WaitingClients.Dequeue();
            SendMessageToClient(receivingClientId, message);
        }
    }

    public void ServerSendsMessageToAllClients(string message)
    {
        foreach (KeyValuePair<string, NetworkPlayer> kvp in m_ConnectedClients)
        {
            SendMessageToClient(kvp.Key, message);
        }
    }

    public void ClientSendsMessageToServerFIFO(string clientId, string message)
    {
        // send message header
        networkView.RPC("ServerReceivesMessageFromClientFIFO", RPCMode.Server, clientId, string.Format(MsgConcatStart, clientId));

        string[] split = message.Split(VHMsgBase.MsgSplitterChar);
        for (int i = 0; i < split.Length; i++)
        {
            split[i] = split[i].Insert(0, string.Format(MsgConcatCont, clientId));
            networkView.RPC("ServerReceivesMessageFromClientFIFO", RPCMode.Server, clientId, split[i]);
        }

        // send message ender
        networkView.RPC("ServerReceivesMessageFromClientFIFO", RPCMode.Server, clientId, string.Format(MsgConcatEnd, clientId));
    }

    public void ClientSendsMessageToServer(string message)
    {
        string[] split = message.Split(VHMsgBase.MsgSplitterChar);
        for (int i = 0; i < split.Length; i++)
        {
            networkView.RPC("ServerReceivesMessageFromClient", RPCMode.Server, split[i]);
        }
    }

    void SendMessageToClient(string clientId, string message)
    {
        NetworkPlayer targetClient = m_ConnectedClients[clientId];
        string splitMessage = "";
        int numSplits = Mathf.CeilToInt((float)message.Length / (float)MaxLength);

        if (numSplits == 1)
        {
            networkView.RPC("ClientReceivesMessage", targetClient, clientId, message);
        }
        else
        {
            for (int i = 0; i < numSplits; i++)
            {
                splitMessage = message.Substring(0, Mathf.Min(MaxLength, message.Length));
                message = message.Remove(0, Mathf.Min(MaxLength, message.Length));
                if (i == 0)
                {
                    splitMessage = splitMessage.Insert(0, string.Format(MsgConcatStart, clientId));
                }
                else if (i == numSplits - 1)
                {
                    splitMessage = splitMessage.Insert(0, string.Format(MsgConcatEnd, clientId));
                }
                else
                {
                    splitMessage = splitMessage.Insert(0, string.Format(MsgConcatCont, clientId));
                }

                networkView.RPC("ClientReceivesMessage", targetClient, clientId, splitMessage);
            }
        }
    }

    #region RPCs
    [RPC]
    void ClientReceivesMessage(string clientId, string message)
    {
        if (message.Contains(string.Format(MsgConcatStart, clientId)))
        {
            message = message.Replace(string.Format(MsgConcatStart, clientId), "");
            m_CombinedMessage = m_CombinedMessage.Remove(0, m_CombinedMessage.Length);
            m_CombinedMessage.Append(message);
        }
        else if (message.Contains(string.Format(MsgConcatCont, clientId)))
        {
            message = message.Replace(string.Format(MsgConcatCont, clientId), "");
            m_CombinedMessage.Append(message);
        }
        else if (message.Contains(string.Format(MsgConcatEnd, clientId)))
        {
            message = message.Replace(string.Format(MsgConcatEnd, clientId), "");
            m_CombinedMessage.Append(message);
            vhmsg.ReceiveVHMsg(m_CombinedMessage.ToString());
        }
        else
        {
            vhmsg.ReceiveVHMsg(message);
        }
    }

    [RPC]
    void ClientReceivesClientId(string clientId)
    {
        m_Receiver.SendMessage("SetClientId", clientId);
    }

    [RPC]
    void ServerReceivesMessageFromClientFIFO(string clientId, string message)
    {
        string header = string.Format(MsgConcatStart, clientId);
        if (message.Contains(header))
        {
            m_WaitingClients.Enqueue(clientId);
        }

        message = message.Replace(string.Format(MsgConcatStart, clientId), "");
        message = message.Replace(string.Format(MsgConcatCont, clientId), "");
        message = message.Replace(string.Format(MsgConcatEnd, clientId), "");
        
        vhmsg.SendVHMsg(message);
    }

    [RPC]
    void ServerReceivesMessageFromClient(string message)
    {
        //Debug.Log("ServerReceivesMessageFromClient: " + message);
        vhmsg.SendVHMsg(message);
    }
    #endregion
    #endregion
}
