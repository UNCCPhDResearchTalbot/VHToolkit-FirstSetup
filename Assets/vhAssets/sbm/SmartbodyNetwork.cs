using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;

public class SmartbodyNetwork : MonoBehaviour
{
    #region Data Members
    public SmartbodyManagerBoneBus m_UnitySmartBody;
    private BoneBusServer m_bonebus;
    //private Queue m_MessageQueue = new Queue();
    #endregion

    #region Functions

    void InitializeCallbacks()
    {
        Application.runInBackground = true;

        m_bonebus = new BoneBusServer();
        m_bonebus.OpenConnection();
        m_bonebus.m_OnClientConnectFunc += new BoneBusServer.OnClientConnectFunc(OnClientConnectFunc);
        m_bonebus.m_OnClientDisconnectFunc += new BoneBusServer.OnClientDisconnectFunc(OnClientDisconnectFunc);
        m_bonebus.m_OnCreateCharacterFunc += new BoneBusServer.OnCreateCharacterFunc(OnCreateCharacterFunc);
        m_bonebus.m_OnDeleteCharacterFunc += new BoneBusServer.OnDeleteCharacterFunc(OnDeleteCharacterFunc);
        m_bonebus.m_OnUpdateCharacterFunc += new BoneBusServer.OnUpdateCharacterFunc(OnUpdateCharacterFunc);
        m_bonebus.m_OnSetCharacterPositionFunc += new BoneBusServer.OnSetCharacterPositionFunc(OnSetCharacterPositionFunc);
        m_bonebus.m_OnSetCharacterRotationFunc += new BoneBusServer.OnSetCharacterRotationFunc(OnSetCharacterRotationFunc);
        m_bonebus.m_OnBoneRotationsFunc += new BoneBusServer.OnBoneRotationsFunc(OnBoneRotationsFunc);
        m_bonebus.m_OnBonePositionsFunc += new BoneBusServer.OnBonePositionsFunc(OnBonePositionsFunc);
        m_bonebus.m_OnSetCharacterVisemeFunc += new BoneBusServer.OnSetCharacterVisemeFunc(OnSetCharacterVisemeFunc);
        m_bonebus.m_OnSetBoneIdFunc += new BoneBusServer.OnSetBoneIdFunc(OnSetBoneIdFunc);
        m_bonebus.m_OnSetVisemeIdFunc += new BoneBusServer.OnSetVisemeIdFunc(OnSetVisemeIdFunc);
        m_bonebus.m_OnPlaySoundFunc += new BoneBusServer.OnPlaySoundFunc(OnPlaySoundFunc);
        m_bonebus.m_OnStopSoundFunc += new BoneBusServer.OnStopSoundFunc(OnStopSoundFunc);
        m_bonebus.m_OnExecScriptFunc += new BoneBusServer.OnExecScriptFunc(OnExecScriptFunc);
    }

    public void Update()
    {
        if (m_bonebus != null)
        {
            m_bonebus.Update();
        }
    }

    #endregion

    #region BoneBus Server Callback Definitions
    void OnClientConnectFunc(string clientName, IntPtr userData)
    {
        networkView.RPC("OnClientConnectFuncDef", RPCMode.Others, clientName);
    }

    void OnClientDisconnectFunc()
    {
        networkView.RPC("OnClientDisconnectFuncDef", RPCMode.Others);
    }

    void OnCreateCharacterFunc(int characterID, string characterType, string characterName, int skeletonType, IntPtr userData)
    {
        networkView.RPC("OnCreateCharacterFuncDef", RPCMode.Others, characterID, characterType, characterName, skeletonType);
    }

    void OnDeleteCharacterFunc(int characterID, IntPtr userData)
    {
        networkView.RPC("OnDeleteCharacterFuncDef", RPCMode.Others, characterID);
    }

    void OnUpdateCharacterFunc(int characterID, string characterType, string characterName, int skeletonType, IntPtr userData)
    {
        networkView.RPC("OnCreateCharacterFuncDef", RPCMode.Others, characterID, characterType, characterName, skeletonType);
    }

    void OnSetCharacterPositionFunc(int characterID, float x, float y, float z, IntPtr userData)
    {
        networkView.RPC("OnSetCharacterPositionFuncDef", RPCMode.Others, characterID, x, y, z);
    }

    void OnSetCharacterRotationFunc(int characterID, float w, float x, float y, float z, IntPtr userData)
    {
        networkView.RPC("OnSetCharacterRotationFuncDef", RPCMode.Others, characterID, w, x, y, z);
    }

    void OnBoneRotationsFunc(ref BoneBusServer.BulkBoneRotations bulkBoneRotations, IntPtr userData)
    {
        //m_MessageQueue.Enqueue(bulkBoneRotations);
        //return;
        int characterID = bulkBoneRotations.charId;

        StringBuilder allBoneRotationData = new StringBuilder();
        //StringBuilder allBoneRotationData = new StringBuilder(string.Format("OnBoneRotationsFuncDef {0}", characterID));

        // go though all the bones that were given to us via bonebus and convert their data into a big long string
        // so it can be RPC'ed through the network
        for (int i = 0; i < bulkBoneRotations.numBoneRotations; i++)
        {
            BoneBusServer.BulkBoneRotation boneRotation = bulkBoneRotations.bones.ElementAt<BoneBusServer.BulkBoneRotation>(i);
            /*if ((boneRotation.boneId >= 62 && boneRotation.boneId <= 67) // left arm
                || (boneRotation.boneId >= 88 && boneRotation.boneId <= 93)// right arm
                || (boneRotation.boneId >= 44 && boneRotation.boneId <= 49))*/// jaw
            {
                // send the rotation as euler angles in order to cut down on the data. (3 floats instead of 4 for a quat)
                allBoneRotationData.Append(boneRotation.ToStringAsEuler() + ",");
            }
        }

        // get rid of the comma at the end
        if (allBoneRotationData.Length > 2)
        {
            allBoneRotationData.Remove(allBoneRotationData.Length - 1, 1);
            networkView.RPC("OnBoneRotationsFuncDef", RPCMode.Others, characterID, allBoneRotationData.ToString());
            //m_MessageQueue.Enqueue(allBoneRotationData.ToString());
        }
    }

    void OnBonePositionsFunc(ref BoneBusServer.BulkBonePositions bulkBonePositions, IntPtr userData)
    {
         int characterID = bulkBonePositions.charId;
        StringBuilder allBonePositionData = new StringBuilder();

        // go though all the bones that were given to us via bonebus and convert their data into a big long string
        // so it can be RPC'ed through the network
        for (int i = 0; i < bulkBonePositions.numBonePositions; i++)
        {
            BoneBusServer.BulkBonePosition bonePosition = bulkBonePositions.bones.ElementAt<BoneBusServer.BulkBonePosition>(i);
            allBonePositionData.Append(bonePosition.ToString() + ",");
        }

        // get rid of the comma at the end
        allBonePositionData.Remove(allBonePositionData.Length - 1, 1);

        networkView.RPC("OnBonePositionsFuncDef", RPCMode.Others, characterID, allBonePositionData.ToString());
    }

    void OnSetCharacterVisemeFunc(int characterID, int visemeId, float weight, float blendTime, IntPtr userData)
    {
        networkView.RPC("OnSetCharacterVisemeFuncDef", RPCMode.Others,  characterID, visemeId, weight, blendTime);
    }

    void OnSetBoneIdFunc(int characterID, string boneName, int boneId, IntPtr userData)
    {
        networkView.RPC("OnSetBoneIdFuncDef", RPCMode.Others, characterID, boneName, boneId);
    }

    void OnSetVisemeIdFunc(int characterID, string visemeName, int visemeId, IntPtr userData)
    {
        networkView.RPC("OnSetVisemeIdFuncDef", RPCMode.Others, characterID, visemeName, visemeId);
    }

    void OnPlaySoundFunc(string soundFile, string characterName, IntPtr userData)
    {
        networkView.RPC("OnPlaySoundFuncDef", RPCMode.Others, soundFile, characterName);
    }

    void OnStopSoundFunc(string soundFile, IntPtr userData)
    {
        networkView.RPC("OnStopSoundFuncDef", RPCMode.Others, soundFile);
    }

    void OnExecScriptFunc(string command, IntPtr userData)
    {
        networkView.RPC("OnExecScriptFuncDef", RPCMode.Others, command);
    }
    #endregion

    #region Bonebus Client Definitions
    [RPC]
    void OnClientConnectFuncDef(string clientName)
    {
        if (m_UnitySmartBody)
            m_UnitySmartBody.OnClientConnectFuncDef(clientName, new IntPtr());
    }

    [RPC]
    void OnClientDisconnectFuncDef()
    {
        m_UnitySmartBody.OnClientDisconnectFuncDef();
    }

    [RPC]
    void OnCreateCharacterFuncDef(int characterID, string characterType, string characterName, int skeletonType)
    {
        m_UnitySmartBody.OnCreateCharacterFuncDef(characterID, characterType, characterName, skeletonType, new IntPtr());
    }

    [RPC]
    void OnDeleteCharacterFuncDef(int characterID)
    {
        m_UnitySmartBody.OnDeleteCharacterFuncDef(characterID, new IntPtr());
    }

    [RPC]
    void OnSetCharacterPositionFuncDef(int characterID, float x, float y, float z)
    {
        m_UnitySmartBody.OnSetCharacterPositionFuncDef(characterID, x, y, z, new IntPtr());
    }

    [RPC]
    void OnSetCharacterRotationFuncDef(int characterID, float w, float x, float y, float z)
    {
        m_UnitySmartBody.OnSetCharacterRotationFuncDef(characterID, w, x, y, z, new IntPtr());
    }

    [RPC]
    void OnBoneRotationsFuncDef(int characterID, string boneRotations)
    {
        BoneBusServer.BulkBoneRotations bulkBoneRotations = new BoneBusServer.BulkBoneRotations();
        bulkBoneRotations.charId = characterID;

        // splitting by ',' seperates the bones from one another
        string[] bones = boneRotations.Split(',');
        bulkBoneRotations.numBoneRotations = bones.Length;

        string[] currBoneData;
        BoneBusServer.BulkBoneRotation[] rotationData = new BoneBusServer.BulkBoneRotation[bones.Length];

        bulkBoneRotations.bones = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BoneBusServer.BulkBoneRotation)) * bones.Length);
        IntPtr c = new IntPtr(bulkBoneRotations.bones.ToInt32());

        Vector3 asEuler = new Vector3();
        Quaternion asQuat = new Quaternion();

        for (int i = 0; i < bones.Length; i++)
        {
            rotationData[i] = new BoneBusServer.BulkBoneRotation();

            // splitting by ' ' seperates the individual bone data
            currBoneData = bones[i].Split(' ');
            // parse our the bone id and euler data
            rotationData[i].boneId = int.Parse(currBoneData[0]); // boneId
            asEuler.x = float.Parse(currBoneData[1]);
            asEuler.y = float.Parse(currBoneData[2]);
            asEuler.z = float.Parse(currBoneData[3]);

            // convert to quat
            asQuat.eulerAngles = asEuler;

            rotationData[i].rot_w = asQuat.w; // quat w
            rotationData[i].rot_x = asQuat.x; // quat x
            rotationData[i].rot_y = asQuat.y; // quat y
            rotationData[i].rot_z = asQuat.z; // quat z

            Marshal.StructureToPtr(rotationData[i], c, false);
            c = new IntPtr(c.ToInt32() + Marshal.SizeOf(typeof(BoneBusServer.BulkBoneRotation)));
        }

        m_UnitySmartBody.OnBoneRotationsFuncDef(ref bulkBoneRotations, IntPtr.Zero);
    }

    [RPC]
    void OnBonePositionsFuncDef(int characterID, string bonePositions)
    {
        //return;
        BoneBusServer.BulkBonePositions bulkBonePositions = new BoneBusServer.BulkBonePositions();
        bulkBonePositions.charId = characterID;

        // splitting by ',' seperates the bones from one another
        string[] bones = bonePositions.Split(',');
        bulkBonePositions.numBonePositions = bones.Length;

        string[] currBoneData;
        BoneBusServer.BulkBonePosition[] positionData = new BoneBusServer.BulkBonePosition[bones.Length];

        bulkBonePositions.bones = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BoneBusServer.BulkBonePosition)) * bones.Length);
        IntPtr c = new IntPtr(bulkBonePositions.bones.ToInt32());

        for (int i = 0; i < bones.Length; i++)
        {
            positionData[i] = new BoneBusServer.BulkBonePosition();

            // splitting by ' ' seperates the individual bone data
            currBoneData = bones[i].Split(' ');
            positionData[i].boneId = int.Parse(currBoneData[0]); // boneId
            positionData[i].pos_x = float.Parse(currBoneData[1]); // pos x
            positionData[i].pos_y = float.Parse(currBoneData[2]); // pos y
            positionData[i].pos_z = float.Parse(currBoneData[3]); // pos z

            Marshal.StructureToPtr(positionData[i], c, false);
            c = new IntPtr(c.ToInt32() + Marshal.SizeOf(typeof(BoneBusServer.BulkBonePosition)));
        }

        m_UnitySmartBody.OnBonePositionsFuncDef(ref bulkBonePositions, IntPtr.Zero);
    }

    [RPC]
    void OnSetCharacterVisemeFuncDef(int characterID, int visemeId, float weight, float blendTime)
    {
        m_UnitySmartBody.OnSetCharacterVisemeFuncDef(characterID, visemeId, weight, blendTime, new IntPtr());
    }

    [RPC]
    void OnSetBoneIdFuncDef(int characterID, string boneName, int boneId)
    {
        m_UnitySmartBody.OnSetBoneIdFuncDef(characterID, boneName, boneId, new IntPtr());
    }

    [RPC]
    void OnSetVisemeIdFuncDef(int characterID, string visemeName, int visemeId)
    {
        m_UnitySmartBody.OnSetVisemeIdFuncDef(characterID, visemeName, visemeId, new IntPtr());
    }

    [RPC]
    void OnPlaySoundFuncDef(string soundFile, string characterName)
    {
        m_UnitySmartBody.OnPlaySoundFuncDef(soundFile, characterName, new IntPtr());
    }

    [RPC]
    void OnStopSoundFuncDef(string soundFile)
    {
        m_UnitySmartBody.OnStopSoundFuncDef(soundFile, new IntPtr());
    }

    [RPC]
    void OnExecScriptFuncDef(string command, IntPtr userData)
    {
        m_UnitySmartBody.OnExecScriptFuncDef(command, new IntPtr());
    }
    #endregion

    #region Unity Messages
    void OnServerInitialized()
    {
        Debug.Log("OnServerInitialized");

        // Now that we know we are the server, setup the bonebus callbacks
        InitializeCallbacks();
    }

    void OnConnectedToServer()
    {
        m_UnitySmartBody = SmartbodyManagerBoneBusEmulator.Get();
    }

    // Called on the client when the connection was lost or you disconnected from the server.
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
        {
            m_bonebus.CloseConnection();
            m_bonebus = null;
        }
    }

    //void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    //{
    //    BoneBusServer.BulkBoneRotations bulkBoneRotations;
    //    Quaternion quat = new Quaternion();

    //    if (stream.isWriting)
    //    {
    //        if (m_MessageQueue.Count <= 0)
    //        {
    //            // no messages to send
    //            return;
    //        }

    //        bulkBoneRotations = (BoneBusServer.BulkBoneRotations)m_MessageQueue.Dequeue();
    //        stream.Serialize(ref bulkBoneRotations.charId);
    //        stream.Serialize(ref bulkBoneRotations.numBoneRotations);

    //        for (int i = 0; i < Mathf.Min(1, bulkBoneRotations.numBoneRotations); i++)
    //        {
    //            BoneBusServer.BulkBoneRotation rot = bulkBoneRotations.bones.ElementAt<BoneBusServer.BulkBoneRotation>(i);
    //            quat.w = rot.rot_w;
    //            quat.x = rot.rot_x;
    //            quat.y = rot.rot_y;
    //            quat.z = rot.rot_z;
    //            stream.Serialize(ref rot.boneId);
    //            stream.Serialize(ref quat);
    //        }

    //        Debug.Log("quat: " + quat);
    //    }
    //    else
    //    {
    //        //int charId = 0, numRotations = 0;
    //        bulkBoneRotations = new BoneBusServer.BulkBoneRotations();
    //        stream.Serialize(ref bulkBoneRotations.charId);
    //        stream.Serialize(ref bulkBoneRotations.numBoneRotations);

    //        BoneBusServer.BulkBoneRotation[] rotationData = new BoneBusServer.BulkBoneRotation[bulkBoneRotations.numBoneRotations];
    //        bulkBoneRotations.bones = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BoneBusServer.BulkBoneRotation)) * bulkBoneRotations.numBoneRotations);
    //        IntPtr c = new IntPtr(bulkBoneRotations.bones.ToInt32());

    //        //Debug.Log("bulkBoneRotations.numBoneRotations: " + bulkBoneRotations.numBoneRotations);

    //        for (int i = 0; i < Mathf.Min(1, bulkBoneRotations.numBoneRotations); i++)
    //        {
    //            rotationData[i] = new BoneBusServer.BulkBoneRotation();
    //            stream.Serialize(ref rotationData[i].boneId);
    //            stream.Serialize(ref quat);
    //            rotationData[i].rot_w = quat.w;
    //            rotationData[i].rot_x = quat.x;
    //            rotationData[i].rot_y = quat.y;
    //            rotationData[i].rot_z = quat.z;

    //            Marshal.StructureToPtr(rotationData[i], c, false);
    //            c = new IntPtr(c.ToInt32() + Marshal.SizeOf(typeof(BoneBusServer.BulkBoneRotation)));
    //        }

    //        Debug.Log("quat: " + quat);

    //        m_UnitySmartBody.OnBoneRotationsFuncDef(ref bulkBoneRotations, IntPtr.Zero);
    //    }
    //}
    #endregion
}
