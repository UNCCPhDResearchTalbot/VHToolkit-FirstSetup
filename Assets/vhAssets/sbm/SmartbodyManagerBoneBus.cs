using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class SmartbodyManagerBoneBus : SmartbodyManager
{
#region Variables
    // singleton
    static SmartbodyManagerBoneBus g_smartbodyManagerBoneBusDisabled;

    static int m_idCounter = 0;

    public Utils.AudioPlayType m_AudioPlayType = Utils.AudioPlayType.Unity;

    BoneBusServer m_bonebus;
#endregion


#region Functions

    public static SmartbodyManagerBoneBus GetDisabled()
    {
        // if SmartbodyManagerBoneBus is disabled at startup (see Awake()), then this function will return the disabled gameobject.
        // this is because it's hard to retrieve a gameobject pointer in Unity after it's been disabled.
        // this function is useful when switching between SmartbodyManager and SmartbodyManagerBoneBus.
        return g_smartbodyManagerBoneBusDisabled;
    }

    void Awake()
    {
        // if there are more than one SmartbodyManager object in the scene, we will assume one is SmartbodyManager and the other is SmartbodyManagerBoneBus
        // we will disable SmartbodyManagerBoneBus, until it's ready to be used.  (will have to switch gameobjects, then launch smartbody process, etc.  See vhAssets Scripts/Main.cs)
        if (UnityEngine.Object.FindObjectsOfType(typeof(SmartbodyManager)).Length > 1)
        {
            var boneBus = UnityEngine.Object.FindObjectOfType(typeof(SmartbodyManagerBoneBus)) as SmartbodyManagerBoneBus;
            if (boneBus)
            {
                g_smartbodyManagerBoneBusDisabled = boneBus;           
#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 ||UNITY_3_3 ||UNITY_3_4 || UNITY_3_5
                gameObject.active = false;
#else
                gameObject.SetActive(false);
#endif
            }
        }
    }

    public override void Start()
    {
        if (m_startCalled)
            return;

        Application.runInBackground = true;

        m_ID = new IntPtr(m_idCounter++);
        Debug.Log("m_ID = " + m_ID);
        if (m_ID == new IntPtr( -1 ))
        {
            Debug.LogError("Failed to CreateBoneBus()");
        }


        m_bonebus = new BoneBusServer();
        m_bonebus.OpenConnection();
        m_bonebus.m_OnClientConnectFunc += new BoneBusServer.OnClientConnectFunc(OnClientConnectFuncDef);
        m_bonebus.m_OnClientDisconnectFunc += new BoneBusServer.OnClientDisconnectFunc(OnClientDisconnectFuncDef);
        m_bonebus.m_OnCreateCharacterFunc += new BoneBusServer.OnCreateCharacterFunc(OnCreateCharacterFuncDef);
        m_bonebus.m_OnDeleteCharacterFunc += new BoneBusServer.OnDeleteCharacterFunc(OnDeleteCharacterFuncDef);
        m_bonebus.m_OnUpdateCharacterFunc += new BoneBusServer.OnUpdateCharacterFunc(OnUpdateCharacterFuncDef);
        m_bonebus.m_OnSetCharacterPositionFunc += new BoneBusServer.OnSetCharacterPositionFunc(OnSetCharacterPositionFuncDef);
        m_bonebus.m_OnSetCharacterRotationFunc += new BoneBusServer.OnSetCharacterRotationFunc(OnSetCharacterRotationFuncDef);
        m_bonebus.m_OnBoneRotationsFunc += new BoneBusServer.OnBoneRotationsFunc(OnBoneRotationsFuncDef);
        m_bonebus.m_OnBonePositionsFunc += new BoneBusServer.OnBonePositionsFunc(OnBonePositionsFuncDef);
        m_bonebus.m_OnSetCharacterVisemeFunc += new BoneBusServer.OnSetCharacterVisemeFunc(OnSetCharacterVisemeFuncDef);
        m_bonebus.m_OnSetBoneIdFunc += new BoneBusServer.OnSetBoneIdFunc(OnSetBoneIdFuncDef);
        m_bonebus.m_OnSetVisemeIdFunc += new BoneBusServer.OnSetVisemeIdFunc(OnSetVisemeIdFuncDef);
        m_bonebus.m_OnPlaySoundFunc += new BoneBusServer.OnPlaySoundFunc(OnPlaySoundFuncDef);
        m_bonebus.m_OnStopSoundFunc += new BoneBusServer.OnStopSoundFunc(OnStopSoundFuncDef);
        m_bonebus.m_OnExecScriptFunc += new BoneBusServer.OnExecScriptFunc(OnExecScriptFuncDef);

        m_startCalled = true;
    }

    protected override void Update()
    {
        m_bonebus.Update();
    }

    protected override void LateUpdate()
    {
    }

    protected override void OnApplicationQuit()
    {
        if (!enabled)
        {
            return;
        }

        m_ID = new IntPtr(-1);
    }

    public override void Shutdown()
    {
        if (m_ID == new IntPtr(-1))
        {
            return;
        }

        m_ID = new IntPtr(-1);

        m_startCalled = false;
    }

    public override void LoadAssetPaths(List<KeyValuePair<string, string>> assetPaths)
    {
        if (assetPaths != null)
        {
            foreach (var pair in assetPaths)
            {
                string path = Utils.GetStreamingAssetsPath() + pair.Value;
                string message = string.Format(@"scene.addAssetPath('{0}', '{1}')", "motion", path);
                PythonCommand(message);
            }
        }

        string loadMessage = string.Format(@"scene.loadAssets()");
        PythonCommand(loadMessage);
    }

    public override void CreateCharacter(UnitySmartbodyCharacter unityCharacter)
    {
        // should only be called by UnitySmartbodyCharacter

        UnitySmartbodyCharacter existingCharacter = GetCharacterBySBMName(unityCharacter.SBMCharacterName);
        if (existingCharacter != null)
        {
            Debug.LogError(string.Format("ERROR - SmartbodyManager.CreateCharacter() - character '{0}' already exists.  Smartbody character names must be unique", unityCharacter.SBMCharacterName));
            return;
        }

        m_characterList.Add(unityCharacter);

        if (m_CustomCreateCBs != null && unityCharacter != null)
        {
            m_CustomCreateCBs(unityCharacter);
        }
    }

    public override void SetMediaPath(string path)
    {
        Debug.Log("SmartbodyManagerBoneBus.SetMediaPath() - currently does nothing");
    }

    public override void SetSpeechAudiofileBasePath(string path)
    {
        Debug.Log("SmartbodyManagerBoneBus.SetSpeechAudiofileBasePath() - currently does nothing");
    }

    public override void SetProcessId(string id)
    {
        Debug.Log("SmartbodyManagerBoneBus.SetProcessId() - currently does nothing");
    }

    public override void PythonCommand(string command)
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg != null)
        {
            string s = "sb " + command;
            vhmsg.SendVHMsg(s);
        }
    }

    static bool pythonCommandError = false;
    public override T PythonCommand<T>(string command)
    {
        if (!pythonCommandError)
        {
            Debug.LogError("SmartbodyManagerBoneBus.PythonCommand<T>() - Does not work with Bonebus.");
            pythonCommandError = true;
        }
        return default(T);
    }
#endregion

#region BoneBus Callback Definitions
    public void OnClientConnectFuncDef(string clientName, IntPtr userData)
    {
        Debug.Log("BoneBusCS - Client Connected! - " + clientName);
    }

    public void OnClientDisconnectFuncDef()
    {
        Debug.Log("Client Disconnected!");
    }

    public void OnCreateCharacterFuncDef(int characterID, string characterType, string characterName, int skeletonType, IntPtr userData)
    {
        UnitySmartbodyCharacter character = GetCharacterBySBMName(characterName);
        if (character != null)
        {
            character.CharacterID = characterID;
        }
    }

    public void OnDeleteCharacterFuncDef(int characterID, IntPtr userData)
    {
    }

    public void OnUpdateCharacterFuncDef(int characterID, string characterType, string characterName, int skeletonType, IntPtr userData)
    {
    }

    public void OnSetCharacterPositionFuncDef(int characterID, float x, float y, float z, IntPtr userData)
    {
#if UNITY_EDITOR
        if (!ReceiveBoneUpdates)
            return;
#endif

        UnitySmartbodyCharacter character = GetCharacterByID(characterID);
        if (character != null)
        {
            character.transform.position = new Vector3(-x, y, z) * m_positionScaleHack;
        }
    }

    public void OnSetCharacterRotationFuncDef(int characterID, float w, float x, float y, float z, IntPtr userData)
    {
#if UNITY_EDITOR
        if (!ReceiveBoneUpdates)
            return;
#endif

        UnitySmartbodyCharacter character = GetCharacterByID(characterID);
        if (character != null)
        {
            character.transform.rotation = new Quaternion(x,-y, -z, w);
        }
    }

    public void OnBoneRotationsFuncDef(ref BoneBusServer.BulkBoneRotations bulkBoneRotations, IntPtr userData)
    {
#if UNITY_EDITOR
        if (!ReceiveBoneUpdates)
            return;
#endif

        UnitySmartbodyCharacter character = GetCharacterByID(bulkBoneRotations.charId);
        if (character == null)
        {
            return;
        }

        Quaternion tempRot = Quaternion.identity;
        for (int i = 0; i < bulkBoneRotations.numBoneRotations; i++)
        {
            BoneBusServer.BulkBoneRotation rot = bulkBoneRotations.bones.ElementAt< BoneBusServer.BulkBoneRotation >( i );
            Transform boneTransform = character.GetBoneFromCache(rot.boneId);
            if (boneTransform == null)
            {
                continue;
            }

            tempRot.x = rot.rot_x;
            tempRot.y = -rot.rot_y;
            tempRot.z = -rot.rot_z;
            tempRot.w = rot.rot_w;
            boneTransform.localRotation = tempRot;
        }
    }

    public void OnBonePositionsFuncDef(ref BoneBusServer.BulkBonePositions bulkBonePositions, IntPtr userData)
    {
#if UNITY_EDITOR
        if (!ReceiveBoneUpdates)
            return;
#endif

        UnitySmartbodyCharacter character = GetCharacterByID(bulkBonePositions.charId);
        if (character == null)
        {
            return;
        }

        Vector3 tempPos = Vector3.zero;
        for (int i = 0; i < bulkBonePositions.numBonePositions; i++)
        {
            BoneBusServer.BulkBonePosition position = bulkBonePositions.bones.ElementAt< BoneBusServer.BulkBonePosition >( i );
            Transform boneTransform = character.GetBoneFromCache(position.boneId);
            if (boneTransform == null)
            {
                continue;
            }

            tempPos.x = -position.pos_x;
            tempPos.y = position.pos_y;
            tempPos.z = position.pos_z;
            boneTransform.localPosition = character.GetBaseBonePosition(position.boneId) + tempPos * m_positionScaleHack;
        }
    }

    public void OnSetCharacterVisemeFuncDef(int characterID, int visemeId, float weight, float blendTime, IntPtr userData)
    {
        UnitySmartbodyCharacter character = GetCharacterByID(characterID);
        if (character != null)
        {
            //character.SetViseme(visemeId, weight, blendTime, !IsLowerFaceViseme(visemeName));
        }
    }

    public void OnSetBoneIdFuncDef(int characterID, string boneName, int boneId, IntPtr userData)
    {
        UnitySmartbodyCharacter character = GetCharacterByID(characterID);
        if (character == null)
        {
            Debug.LogError("OnSetBoneIdFuncDef failed because the character doesn't exist");
        }

        //Debug.Log("OnSetBoneIdFuncDef: " + boneId + " boneName: " + boneName);
        character.MapBoneIDToBoneName(boneId, boneName);
    }

    public void OnSetVisemeIdFuncDef(int characterID, string visemeName, int visemeId, IntPtr userData)
    {
        UnitySmartbodyCharacter character = GetCharacterByID(characterID);
        if (character == null)
        {
            Debug.LogError("OnSetVisemeIdFuncDef failed because the character doesn't exist");
        }

        //Debug.LogWarning("OnSetVisemeIdFuncDef is not implemented");
    }

    public void OnPlaySoundFuncDef(string soundFile, string characterName, IntPtr userData)
    {
        // will let the normal PlaySound/StopSound message handling take care of playing sounds.
        return;
    }

    public void OnStopSoundFuncDef(string soundFile, IntPtr userData)
    {
        // will let the normal PlaySound/StopSound message handling take care of playing sounds.
        return;
    }

    public void OnExecScriptFuncDef(string command, IntPtr userData)
    {
        var split = VHMsgBase.SplitIntoOpArg(command);
        ProcessVHMsgs(split.Key, split.Value);
    }
#endregion
}
