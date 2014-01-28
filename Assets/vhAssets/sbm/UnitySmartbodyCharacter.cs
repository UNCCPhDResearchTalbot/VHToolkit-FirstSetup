using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class UnitySmartbodyCharacter : Character
{
    #region Constants
    public enum DebugFlags
    {
        Show_Bones = 1,
        Show_Axes = 1 << 1,
        Show_Eye_Beams = 1 << 2,
    }

    public delegate void ChannelCallback(UnitySmartbodyCharacter character, string channelName, float value);
    #endregion

    #region DataMembers

    // used for GetBoneAndBaseBonePosition().  Instead of passing back a Vector3, which is a struct, and requires a copy, we wrapped the Vector3 with a class, so that it can be passed back by reference.  This function is called so many times, it's worth this optimization
    public class Vector3class
    {
        public Vector3 vector3;
    }

    // this struct represents the data that is passed to the smartbody dll wrapper and gets filled out.
    // Upon returning from smartbody, the data in this structure is assigned to this character's bones
    public struct UnityCharacterData
    {
        public SmartbodyExternals.SmartbodyCharacterWrapper m_Character;
        public IntPtr [] jnames;
        public float [] jx;
        public float [] jy;
        public float [] jz;
        public float [] jrw;
        public float [] jrx;
        public float [] jry;
        public float [] jrz;

        public float [] jprevx;
        public float [] jprevy;
        public float [] jprevz;
        public float [] jprevrw;
        public float [] jprevrx;
        public float [] jprevry;
        public float [] jprevrz;
        public float [] jNextUpdateTime;
    }
    public UnityCharacterData m_CharacterData = new UnityCharacterData();

    public const string SoundNodeName = "SoundNode";

    protected int m_characterID;
    protected string m_characterType;
    protected string m_sbmCharacterName;
    protected uint m_DebugFlags;
    protected ChannelCallback m_ChannelCB;

    const int NumBones = 120;
    Transform[] m_Bones;// = new Transform[NumBones];
    Vector3class[] m_BaseBonePositions;
    Dictionary<string, int> m_BoneLookupTable = new Dictionary<string, int>(NumBones);
    Dictionary<int, string> m_BonebusIDToBoneNameTable = new Dictionary<int,string>(NumBones);

    // these are fudge factors for legacy projects for adjusting scale.
    // for example, if your incoming smartbody data is in cm, and your level is in feet, you'd call these functions with a parameter of ( 1 / 30.48 )
    // These should be considered hacks because changes in scale will cause issues with smartbody
    float m_characterPositionScaleModifier;
    float m_bonePositionScaleModifier;

    AudioSource m_AudioSource;

    #endregion

    #region Properties

    public bool ShowBones
    {
        get { return Utils.IsFlagOn(m_DebugFlags, (uint)DebugFlags.Show_Bones); }
    }

    public bool ShowEyeBeams
    {
        get { return Utils.IsFlagOn(m_DebugFlags, (uint)DebugFlags.Show_Eye_Beams); }
    }

    public bool ShowAxes
    {
        get { return Utils.IsFlagOn(m_DebugFlags, (uint)DebugFlags.Show_Axes); }
    }

    public int GetNumBones
    {
        get { return m_Bones.Length; }
    }

    public void ToggleDebugFlag(DebugFlags flag)
    {
        Utils.ToggleFlag(ref m_DebugFlags, (uint)flag);

        if (flag == DebugFlags.Show_Bones)
        {
            // stop showing geometry, show the bones
            ShowGeometry(!Utils.IsFlagOn(m_DebugFlags, (uint)DebugFlags.Show_Bones));
        }
    }

    public float CharacterPositionScaleModifier
    {
        get { return m_characterPositionScaleModifier; }
        set { m_characterPositionScaleModifier = value; }
    }

    public float BonePositionScaleModifier
    {
        get { return m_bonePositionScaleModifier; }
        set { m_bonePositionScaleModifier = value; }
    }

    public int CharacterID
    {
        get { return m_characterID; }
        set { m_characterID = value; }
    }

    public string CharacterType
    {
        get { return m_characterType; }
        set { m_characterType = value; }
    }

    public override string CharacterName
    {
        get { return SBMCharacterName; }
    }

    public string SBMCharacterName
    {
        get { return m_sbmCharacterName; }
        set { m_sbmCharacterName = value; }
    }

    public AudioSource AudioSource
    {
        get { return m_AudioSource; }
    }

    #endregion

    public void Awake()
    {
    }

    public void Start()
    {
        //Debug.Log("UnitySmartbodyCharacter.Start()");

        // SmartbodyManager is a dependency of this component.  Make sure Start() has been called.
        SmartbodyManager sbm = SmartbodyManager.Get();
        sbm.Start();

        m_CharacterData.m_Character = new SmartbodyExternals.SmartbodyCharacterWrapper();
        m_CharacterData.m_Character.m_name = IntPtr.Zero;

        SmartbodyCharacterInit init = GetComponent<SmartbodyCharacterInit>();
        if (init != null)
        {
            SmartbodyFaceDefinition face = GetComponent<SmartbodyFaceDefinition>();   // ok if it's null
            SmartbodyJointMap [] jointMaps = GetComponents<SmartbodyJointMap>();   // ok if it's null
            CreateCharacter(init, face, jointMaps);
        }
        else
        {
            Debug.LogWarning("UnitySmartbodyCharacter.Start() - " + name + " - No SmartbodyCharacterInit script attached.  You need to attach a SmartbodyCharacterInit script to this gameobject so that it will initialize properly");
        }
    }

    public void Update()
    {
    }

    void OnDestroy()
    {
        //Debug.Log("UnitySmartbodyCharacter.OnDestroy()");

        SmartbodyManager sbm = SmartbodyManager.Get();
        if (sbm != null)
        {
            sbm.PythonCommand(string.Format(@"scene.command('sbm char {0} remove')", SBMCharacterName));

            sbm.RemoveCharacter(this);
        }
    }

    public void OnDrawGizmos()
    {
        if (ShowEyeBeams)
        {
            Transform t;
            t = GetBone("eyeball_left");
            if (t == null ) t = GetBone("JtEyeLf");
            Debug.DrawRay(t.position, t.forward, Color.red);
            t = GetBone("eyeball_right");
            if (t == null ) t = GetBone("JtEyeRt");
            Debug.DrawRay(t.position, t.forward, Color.red);
        }

        if (ShowAxes)
        {
            for (int i = 0; i < m_Bones.Length; i++)
            {
                Utils.DrawTransformLines(m_Bones[i].transform, 0.025f);
            }
        }

        if (ShowBones)
        {
            for (int i = 0; i < m_Bones.Length; i++)
            {
                Gizmos.DrawSphere(m_Bones[i].transform.position, 0.0125f);

                if (m_Bones[i].parent != null)
                {
                    Debug.DrawLine(m_Bones[i].transform.position, m_Bones[i].parent.transform.position);
                }
            }
        }
    }


    #region Functions

    public void CreateCharacter(SmartbodyCharacterInit character, SmartbodyFaceDefinition face, SmartbodyJointMap [] jointMaps)
    {
        /*
            brad = scene.createCharacter("brad", "brad-attach")
            brad.setSkeleton(scene.createSkeleton("common.sk"))
            brad.setFaceDefinition(defaultFace)
            brad.createStandardControllers()

            brad.setVoice("audiofile")
            brad.setVoiceCode("Sounds")
            brad.setVoiceBackup("remote")
            brad.setVoiceBackupCode("Festival_voice_rab_diphone")
            brad.setUseVisemeCurves(True)
        */

        {
            GameObject boneParent = Utils.FindChild(gameObject, character.unityBoneParent);
            m_Bones = boneParent.GetComponentsInChildren<Transform>();
            m_BaseBonePositions = new Vector3class[m_Bones.Length];
            m_BoneLookupTable = new Dictionary<string, int>(NumBones);

            for (int i = 0; i < m_Bones.Length; i++)
            {
                m_BoneLookupTable.Add(m_Bones[i].gameObject.name, i);
                m_BaseBonePositions[i] = new Vector3class();
                m_BaseBonePositions[i].vector3 = m_Bones[i].localPosition;
            }

            GameObject soundNode = Utils.FindChild(gameObject, SoundNodeName);
            if (soundNode != null)
            {
                m_AudioSource = soundNode.audio;
            }
            else
            {
                Debug.LogWarning("No SoundNode found for " + name + ". You need to create a gameobject called '" +
                    SoundNodeName + "' ,attach it as a child to this character's prefab, and give it an audiosource component. " +
                    "Until you do this, sound cannot be played from this character");
            }
        }

        //CharacterName = character.name;
        SBMCharacterName = character.name;

        SmartbodyManager sbm = SmartbodyManager.Get();


        sbm.LoadAssetPaths(character.assetPaths);

        if (jointMaps != null)
        {
            foreach (SmartbodyJointMap jointMap in jointMaps)
            {
                sbm.AddJointMap(jointMap);

                // TODO: Check to make sure it hasn't already been mapped
                sbm.MapSkeletonAndAssetPaths(jointMap, jointMap.skeletonName, character.assetPaths);

                sbm.PythonCommand(string.Format(@"scene.createSkeleton('{0}')", jointMap.skeletonName));
            }
        }

        foreach (var pair in character.assetPaths)
        {
            string skeletonName = pair.Key;

            // set up online retargeting
            if (skeletonName != character.skeletonName)
            {
                sbm.PythonCommand(string.Format(@"endJoints = StringVec()"));
                sbm.PythonCommand(string.Format(@"endJoints.append('l_ankle')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('l_forefoot')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('l_toe')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('l_wrist')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('r_ankle')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('r_forefoot')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('r_toe')"));
                sbm.PythonCommand(string.Format(@"endJoints.append('r_wrist')"));

                sbm.PythonCommand(string.Format(@"relativeJoints = StringVec()"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('spine1')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('spine2')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('spine3')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('spine4')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('spine5')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('r_sternoclavicular')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('l_sternoclavicular')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('r_acromioclavicular')"));
                sbm.PythonCommand(string.Format(@"relativeJoints.append('l_acromioclavicular')"));

                // TODO: Check to make sure it hasn't already been created
                sbm.PythonCommand(string.Format(@"scene.getRetargetManager().createRetarget('{0}', '{1}')", skeletonName, character.skeletonName));
                sbm.PythonCommand(string.Format(@"scene.getRetargetManager().getRetarget('{0}', '{1}').initRetarget(endJoints, relativeJoints)", skeletonName, character.skeletonName));
            }
        }

        sbm.PythonCommand(string.Format(@"scene.createCharacter('{0}', '{1}')", SBMCharacterName, CharacterName));
        sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setSkeleton(scene.createSkeleton('{1}'))", SBMCharacterName, character.skeletonName));

        if (face != null)
        {
            sbm.AddFaceDefinition(face);
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setFaceDefinition(scene.getFaceDefinition('{1}'))", SBMCharacterName, face.definitionName));
        }

        sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').createStandardControllers()", SBMCharacterName));

        if (!string.IsNullOrEmpty(character.voiceType) &&
            !string.IsNullOrEmpty(character.voiceCode))
        {
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setVoice('{1}')", SBMCharacterName, character.voiceType));
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setVoiceCode('{1}')", SBMCharacterName, character.voiceCode));
        }

        if (!string.IsNullOrEmpty(character.voiceTypeBackup) &&
            !string.IsNullOrEmpty(character.voiceCodeBackup))
        {
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setVoiceBackup('{1}')", SBMCharacterName, character.voiceTypeBackup));
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setVoiceBackupCode('{1}')", SBMCharacterName, character.voiceCodeBackup));
        }

        if (character.useVisemeCurves)
        {
            sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setUseVisemeCurves(True)", SBMCharacterName));
        }

        if (!string.IsNullOrEmpty(character.startingPosture))
        {
            sbm.SBPosture(SBMCharacterName, character.startingPosture, UnityEngine.Random.Range(0, 4.0f));
        }


        // locomotion/steering currently only working under certain platforms  (can't find pprAI lib)
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (!string.IsNullOrEmpty(character.locomotionInitPythonFile))
            {
                if (!string.IsNullOrEmpty(character.locomotionInitPythonSkeletonName))
                {
                    sbm.PythonCommand(string.Format(@"locomotionInitSkeleton = '{0}'", character.locomotionInitPythonSkeletonName));
                }

                sbm.SBRunPythonScript(character.locomotionInitPythonFile);

                sbm.PythonCommand(string.Format(@"scene.getSteerManager().removeSteerAgent('{0}')", SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getSteerManager().createSteerAgent('{0}')", SBMCharacterName));

                if (!string.IsNullOrEmpty(character.locomotionSteerPrefix))
                {
                    sbm.PythonCommand(string.Format(@"scene.getSteerManager().getSteerAgent('{0}').setSteerStateNamePrefix('{1}')", SBMCharacterName, character.locomotionSteerPrefix));
                }
                else
                {
                    Debug.LogWarning("UnitySmartbodyCharacter.CreateCharacter() - locomotionInitPython file specified, but no locomotionSteerPrefix specified.  This must be specified for locomotion to work");
                }

                sbm.PythonCommand(string.Format(@"scene.getSteerManager().getSteerAgent('{0}').setSteerType('{1}')", SBMCharacterName, "example"));
                sbm.PythonCommand(string.Format(@"scene.getCharacter('{0}').setSteerAgent(scene.getSteerManager().getSteerAgent('{1}'))", SBMCharacterName, SBMCharacterName));


                //# Toggle the steering manager
                sbm.PythonCommand(string.Format(@"scene.getSteerManager().setEnable(False)"));
                sbm.PythonCommand(string.Format(@"scene.getSteerManager().setEnable(True)"));
            }
        }


        sbm.CreateCharacter(this);


        sbm.SBTransform(SBMCharacterName, transform);


        character.TriggerPostLoadEvent(this);
    }

    void ShowGeometry(bool show)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = show;
        }
    }

    public bool IsVisible()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].isVisible)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void OnBoneTransformations(float positionScale)
    {
        Transform currentBoneTransform = null;
        Quaternion tempQ = Quaternion.identity;
        Vector3 tempVec = Vector3.zero;
        UnitySmartbodyCharacter.Vector3class baseBonePosition = null;
        string jointName = String.Empty;
        float curTime = Time.time;
        SmartbodyFaceDefinition face = GetComponent<SmartbodyFaceDefinition>();
        for (int i = 0; i < m_CharacterData.m_Character.m_numJoints; i++)
        {
            bool posCacheHit = false;
            bool rotCacheHit = false;

            if (m_CharacterData.jNextUpdateTime[i] > curTime)
            {
                if (m_CharacterData.jx[i] == m_CharacterData.jprevx[i] &&
                    m_CharacterData.jy[i] == m_CharacterData.jprevy[i] &&
                    m_CharacterData.jz[i] == m_CharacterData.jprevz[i])
                {
                    posCacheHit = true;
                }

                if (m_CharacterData.jrw[i] == m_CharacterData.jprevrw[i] &&
                    m_CharacterData.jrx[i] == m_CharacterData.jprevrx[i] &&
                    m_CharacterData.jry[i] == m_CharacterData.jprevry[i] &&
                    m_CharacterData.jrz[i] == m_CharacterData.jprevrz[i])
                {
                    rotCacheHit = true;
                }
            }


            if (posCacheHit && rotCacheHit)
                continue;


            // update when next to force an update no matter what the cache status
            const float cacheRefreshTimeMin = 5.0f;  // seconds between refreshes
            const float cacheRefreshTimeMax = 6.0f;
            float refreshTime = Mathf.Lerp(cacheRefreshTimeMin, cacheRefreshTimeMax, UnityEngine.Random.value);   // return number between min/max

            m_CharacterData.jNextUpdateTime[i] = curTime + refreshTime;

            jointName = Marshal.PtrToStringAnsi(m_CharacterData.jnames[i]);
            if (String.IsNullOrEmpty(jointName))
            {
                continue;
            }

            if (face != null && (face.visemes.FindIndex(s => s.Key == jointName) != -1 || jointName.IndexOf("au_") != -1))
            {
                SetChannel(jointName, m_CharacterData.jx[i] * positionScale);
            }

            bool ret = GetBoneAndBaseBonePosition(jointName, out currentBoneTransform, out baseBonePosition);
            if (ret == false)
            {
                continue;
            }

            if (!rotCacheHit)
            {
                // set rotation
                tempQ.Set( m_CharacterData.jrx[i],
                          -m_CharacterData.jry[i],
                          -m_CharacterData.jrz[i],
                           m_CharacterData.jrw[i]);

                currentBoneTransform.localRotation = tempQ;
            }

            if (!posCacheHit)
            {
                // set position
                tempVec.Set(baseBonePosition.vector3.x + (-m_CharacterData.jx[i] * positionScale),
                            baseBonePosition.vector3.y + ( m_CharacterData.jy[i] * positionScale),
                            baseBonePosition.vector3.z + ( m_CharacterData.jz[i] * positionScale));

                currentBoneTransform.localPosition = tempVec;
            }
        }
    }

    public virtual void SetChannel(string channelName, float value)
    {
        //Debug.Log("SetChannel() - " + channelName + " " + value);

        if (m_ChannelCB != null)
        {
            m_ChannelCB(this, channelName, value);
        }
        else
        {
            //Debug.LogError("UnitySmartbodyCharacter::SetChannel was called but no callback is set. Call SetChannelCallback to set up a callback function.");
        }
    }

    public void SetChannelCallback(ChannelCallback channelCB)
    {
        m_ChannelCB = channelCB;
    }

    public Transform GetBone(string boneName)
    {
        int index = -1;
        if (m_BoneLookupTable.TryGetValue(boneName, out index))
        {
            return m_Bones[index];
        }
        else
        {
            //Debug.LogError("there's no bone named: " + boneName);
        }
        return null;
    }

    public Transform GetBone(int index)
    {
        if (index < 0 || index >= m_Bones.Length)
        {
            return null;
        }

        return m_Bones[index];
    }


    public Vector3 GetBaseBonePosition(string boneName)
    {
        int index = -1;
        if (m_BoneLookupTable.TryGetValue(boneName, out index))
        {
            return m_BaseBonePositions[index].vector3;
        }
        else
        {
            //Debug.LogError("there's no bone named: " + boneName);
        }
        return Vector3.zero;
    }

    public Vector3 GetBaseBonePosition(int boneId)
    {
        if (m_BonebusIDToBoneNameTable == null || boneId < 0 || boneId >= m_BonebusIDToBoneNameTable.Count)
        {
            //Debug.LogError("GetBoneFromCache failed because index " + i + " is out of range.");
            return Vector3.zero;
        }

        return GetBaseBonePosition(m_BonebusIDToBoneNameTable[boneId]);
    }

    public bool GetBoneAndBaseBonePosition(string boneName, out Transform bone, out Vector3class baseBonePosition)
    {
        // this is a combination of GetBone() and GetBaseBonePosition() to reduce dictionary lookups

        int index;
        if (m_BoneLookupTable.TryGetValue(boneName, out index))
        {
            bone = m_Bones[index];
            baseBonePosition = m_BaseBonePositions[index];
            return true;
        }
        else
        {
            //Debug.LogError("there's no bone named: " + boneName);
            bone = null;
            baseBonePosition = null;
        }

        return false;
    }

    public virtual Transform GetBoneFromCache(int i)
    {
        if (m_BonebusIDToBoneNameTable == null || i < 0 || i >= m_BonebusIDToBoneNameTable.Count)
        {
            //Debug.LogError("GetBoneFromCache failed because index " + i + " is out of range.");
            return null;
        }

        return GetBone(m_BonebusIDToBoneNameTable[i]);
    }

    public void MapBoneIDToBoneName(int boneId, string boneName)
    {
        m_BonebusIDToBoneNameTable[boneId] = boneName;
    }

    #endregion
}
