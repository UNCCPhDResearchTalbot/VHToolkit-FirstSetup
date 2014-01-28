using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

public class SmartbodyManager : ICharacterController
{
    public delegate void OnCustomCharacterCallback(UnitySmartbodyCharacter character);

#region Constants
    const float TextHeight = 22;
    const string Attach = "-attach";
    const string PythonCmd = "python";

    public enum FaceSide
    {
        both,
        left,
        right,
    }

    public enum GazeTargetBone
    {
        NONE,
        BASE,
        EYEBALL_LEFT,
        EYEBALL_RIGHT,
        SKULL_BASE,
        SPINE1,
        SPINE2,
        SPINE3,
        SPINE4,
        SPINE5,
        r_wrist
    }

    public enum GazeDirection
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT,
        UPLEFT,
        UPRIGHT,
        DOWNLEFT,
        DOWNRIGHT,
        POLAR
    }

    public enum GazeJointRange
    {
        EYES,
        NECK,
        CHEST,
        BACK,
        HEAD_EYES,
        EYES_NECK,
        EYES_CHEST,
        EYES_BACK,
        NECK_CHEST,
        NECK_BACK,
        CHEST_BACK,
    }

    public enum SaccadeType
    {
        Listen,
        Talk,
        Think,
        End,
        Default,
    }

    static public readonly Dictionary<int, string> AUToFacialLookUp = new Dictionary<int, string>()
        {
            { 1, "InnerBrowRaiser"},
            { 2, "OuterBrowRaiser"},
            { 4, "InnerBrowLowerer"},
            { 5, "UpperLidRaiser"},
            { 6, "EyeSquint"},
            { 7, "LidTightener"},
            { 9, "NoseWrinkle"},
            { 10, "UpperLipRaiser"},
            { 12, "SmileMouth"},
            { 15, "LipCornerDepressor"},
            { 23, "LipTightener"},
            { 25, "LipParser"},
            { 38, "NostrilDilator"},
            { 39, "NostrilCompressor"},
            { 45, "Blink"}
        };

    static public readonly string[] SyncPointNames = 
    {
        "emphasis",
        "ready",
        "relax",
        "strokeStart",
        "stroke",
    };

    public class SkmMetaData
    {
        public float Length = -1;
        public Dictionary<string, float> SyncPoints = new Dictionary<string, float>();
    }

#endregion

#region DataMembers
    public bool m_UseReleaseMode = true;
    public bool m_displayLogMessages = true;
    public bool m_logToFile = true;
    public bool m_debugQuickLoadNoMotions = false;

    // these cam variables are for the debugger camera (sbmonitor)
    [NonSerialized] public Vector3 m_camPos = Vector3.zero;
    [NonSerialized] public Quaternion m_camRot = Quaternion.identity;
    [NonSerialized] public double  m_camFovY = 45;
    [NonSerialized] public double  m_camAspect = 1.5;
    [NonSerialized] public double  m_camZNear = 0.1;
    [NonSerialized] public double  m_camZFar = 1000;

    // singleton
    static SmartbodyManager g_smartbodyManager;

    // list of characters currently polling smart body
    protected List<UnitySmartbodyCharacter> m_characterList = new List<UnitySmartbodyCharacter>();
    protected IntPtr m_ID = new IntPtr( -1 );
    protected List<SmartbodyPawn> m_pawns = new List<SmartbodyPawn>();

    protected Dictionary<string, SmartbodyFaceDefinition> m_faceDefinitions = new Dictionary<string, SmartbodyFaceDefinition>();
    protected List<string> m_loadedSkeletons = new List<string>();
    protected List<string> m_loadedMotions = new List<string>();
    protected Dictionary<string, SmartbodyJointMap> m_jointMaps = new Dictionary<string, SmartbodyJointMap>();
    List<Character> m_CharactersToUpload = new List<Character>();

    protected SmartbodyExternals.OnCharacterCreateCallback m_CharacterCreateCB;
    protected SmartbodyExternals.OnCharacterDeleteCallback m_CharacterDeleteCB;
    protected SmartbodyExternals.OnCharacterChangeCallback m_CharacterChangeCB;
    protected SmartbodyExternals.OnVisemeCallback m_VisemeCB;
    protected SmartbodyExternals.OnChannelCallback m_ChannelCB;
    protected SmartbodyExternals.LogMessageCallback m_LogMessageCB;
    protected bool m_bReceiveBoneUpdates = true;

    protected OnCustomCharacterCallback m_CustomCreateCBs;
    protected OnCustomCharacterCallback m_CustomDeleteCBs;

    protected bool m_startCalled = false;

    protected float m_positionScaleHack = 1.0f;
#endregion

#region Properties
    public bool ReceiveBoneUpdates
    {
        get { return m_bReceiveBoneUpdates; }
        set { m_bReceiveBoneUpdates = value; }
    }

    public float PositionScaleHack { get { return m_positionScaleHack; } }
#endregion

#region Functions
    public static SmartbodyManager Get()
    {
        //Debug.Log("SmartbodyManager.Get()");
        if (g_smartbodyManager == null)
        {
            g_smartbodyManager = UnityEngine.Object.FindObjectOfType(typeof(SmartbodyManager)) as SmartbodyManager;
        }

        return g_smartbodyManager;
    }

    public static void ResetGet()
    {
        // this function will reset the global singleton to null, so that when Get() is called again, the scene is searched again for the gameobject
        // this is helpful when switching between SmartbodyManager and SmartbodyManagerBoneBus.
        g_smartbodyManager = null;
    }

    void Awake()
    {
        PluginsFolderRedirect.RedirectPluginsFolder();
        StreamingAssetsExtract.ExtractStreamingAssets();
    }

    public virtual void Start()
    {
        if (m_startCalled)
            return;

        //Debug.Log("SmartbodyManager.Start()");

        m_ID = SmartbodyExternals.CreateSBM(m_UseReleaseMode);

        //Debug.Log("m_sbmID = " + m_ID);

        if (m_ID == new IntPtr( -1 ))
        {
            Debug.LogError("Failed to CreateSBM()");
        }

        InitConsole();

        string pythonLibPath = "../smartbody/Python27/Lib";
        if (SmartbodyExternals.Init(m_ID, pythonLibPath, m_logToFile))
        {
            Debug.Log("Smartbody successfully init");

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                SmartbodyExternals.SetListener(m_ID, null, null, null, null, null);
                SmartbodyExternals.WRAPPER_SBM_SetLogMessageCallback(null);
                //PythonCommand("scene.command('vhmsglog on')");
            }
            else
            {
                m_LogMessageCB = new SmartbodyExternals.LogMessageCallback(OnLogMessage);
                SmartbodyExternals.WRAPPER_SBM_SetLogMessageCallback(m_LogMessageCB);

                // you have to create new instances of these otherwise they won't be called correctly from unmanaged code
                m_CharacterCreateCB = new SmartbodyExternals.OnCharacterCreateCallback(OnCharacterCreate);
                m_CharacterDeleteCB = new SmartbodyExternals.OnCharacterDeleteCallback(OnCharacterDelete);
                m_CharacterChangeCB = new SmartbodyExternals.OnCharacterChangeCallback(OnCharacterChange);
                m_VisemeCB = new SmartbodyExternals.OnVisemeCallback(OnViseme);
                m_ChannelCB = new SmartbodyExternals.OnChannelCallback(OnChannel);
                SmartbodyExternals.SetListener(m_ID, m_CharacterCreateCB, m_CharacterDeleteCB, m_CharacterChangeCB, m_VisemeCB, m_ChannelCB);
                SubscribeVHMsg();
            }

            SmartbodyExternals.SetDebuggerId(m_ID, "unity");
            SmartbodyExternals.SetDebuggerRendererRightHanded(m_ID, false);
        }
        else
        {
            Debug.LogError("Smartbody failed to init");
        }


        SmartbodyInit initSettings = GetComponent<SmartbodyInit>();
        if (initSettings != null)
        {
            Init(initSettings);
        }
        else
        {
            Debug.LogWarning("SmartbodyManager.Start() - No SmartbodyInit script attached.  You need to attach a SmartbodyInit script to this gameobject so that it will initialize properly");
        }


        m_startCalled = true;
    }

    public void SetTime(float time)
    {
        SmartbodyExternals.Update(m_ID, time);
    }

    protected virtual void Update()
    {
        //Debug.Log("SmartbodyManager.Update() - " + Time.time);

        SmartbodyExternals.SetDebuggerCameraValues(m_ID, m_camPos.x / m_positionScaleHack, m_camPos.y / m_positionScaleHack,
                                            m_camPos.z / m_positionScaleHack, m_camRot.x, m_camRot.y, m_camRot.z,
                                            m_camRot.w, m_camFovY, m_camAspect, m_camZNear, m_camZFar);

        SmartbodyExternals.Update(m_ID, Time.time);


#if UNITY_IPHONE || UNITY_ANDROID
        StringBuilder name = new StringBuilder(256);
        StringBuilder objectClass = new StringBuilder(256);
        float weight = 0;
        float blendTime = 0;
        int logMessageType = 3; // Other message type

        name.Length = 0;
        objectClass.Length = 0;
        while (SmartbodyExternals.IsCharacterCreated(m_ID, name, name.Capacity, objectClass, objectClass.Capacity))
        {
            OnCharacterCreate(m_ID, name.ToString(), objectClass.ToString());
            name.Length = 0;
            objectClass.Length = 0;
        }

        name.Length = 0;
        while (SmartbodyExternals.IsLogMessageWaiting(m_ID, name, name.Capacity, ref logMessageType))
        {
            OnLogMessage(name.ToString(), logMessageType);
            name.Length = 0;
            logMessageType = 3;
        }

        name.Length = 0;
        while (SmartbodyExternals.IsCharacterDeleted(m_ID, name, name.Capacity))
        {
            OnCharacterDelete(m_ID, name.ToString());
            name.Length = 0;
        }

        name.Length = 0;
        while (SmartbodyExternals.IsCharacterChanged(m_ID, name, name.Capacity))
        {
            OnCharacterChange(m_ID, name.ToString());
            name.Length = 0;
        }

        name.Length = 0;
        objectClass.Length = 0;
        while (SmartbodyExternals.IsVisemeSet(m_ID, name, name.Capacity, objectClass, objectClass.Capacity, ref weight, ref blendTime))
        {
            OnViseme(m_ID, name.ToString(), objectClass.ToString(), weight, blendTime);
            name.Length = 0;
            objectClass.Length = 0;
        }

        name.Length = 0;
        objectClass.Length = 0;
        weight = 0;
        while (SmartbodyExternals.IsChannelSet(m_ID, name, name.Capacity, objectClass, objectClass.Capacity, ref weight))
        {
            OnChannel(m_ID, name.ToString(), objectClass.ToString(), weight);
            name.Length = 0;
            objectClass.Length = 0;
            weight = 0;
        }
#endif
    }

    protected virtual void LateUpdate()
    {
#if UNITY_EDITOR
        if (!ReceiveBoneUpdates)
            return;
#endif

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        for (int i = 0; i < m_characterList.Count; i++)
        {
            if (!GetUnityCharacterData(m_characterList[i].SBMCharacterName, ref m_characterList[i].m_CharacterData))
            {
                //continue;
            }

            // character position
            pos.x = -m_characterList[i].m_CharacterData.m_Character.x;
            pos.y = m_characterList[i].m_CharacterData.m_Character.y;
            pos.z = m_characterList[i].m_CharacterData.m_Character.z;

            // character orientation
            rot.x = m_characterList[i].m_CharacterData.m_Character.rx;
            rot.y = -m_characterList[i].m_CharacterData.m_Character.ry;
            rot.z = -m_characterList[i].m_CharacterData.m_Character.rz;
            rot.w = m_characterList[i].m_CharacterData.m_Character.rw;

            OnSetCharacterPosition(m_characterList[i], pos);
            OnSetCharacterRotation(m_characterList[i], rot);

            m_characterList[i].OnBoneTransformations(m_positionScaleHack);

            //SmartbodyCharacterWrapper
            //SmartbodyExternals.ReleaseCharacter(ref m_characterList[i].m_CharacterData.m_Character);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        //Debug.Log("SmartbodyManager.OnApplicationQuit()");
    }

    protected void OnDestroy()
    {
        //Debug.Log("SmartbodyManager.OnDestroy()");

        Shutdown();
    }

    public void Init(SmartbodyInit initSettings)
    {
        /*
            scene.addAssetPath("ME", "Art/brad/SB")
            scene.loadAssets()

            defaultFace = scene.getFaceDefinition("_default_")
            defaultFace.setFaceNeutral("face_neutral")
            defaultFace.setAU(45, "left",  "fac_45L_blink")
            defaultFace.setAU(45, "right", "fac_45R_blink")
            defaultFace.setViseme("Ao",  "viseme_ao")
            defaultFace.setViseme("BMP", "viseme_bmp")
            defaultFace.setViseme("blink", "fac_45_blink")
        */

        string message;

        m_faceDefinitions.Clear();
        m_loadedSkeletons.Clear();
        m_loadedMotions.Clear();
        m_jointMaps.Clear();

        // If initialPySeqPath is null, we set a default.  We don't set the default in the 
        // SmartbodyInit class because we can't call GetExternalAssetsPath() in a default assignment.
        // It can only be called in Awake() or Start()
        string pySeqPath;
        if (string.IsNullOrEmpty(initSettings.initialPySeqPath))
            pySeqPath = Utils.GetExternalAssetsPath() + "SB";
        else
            pySeqPath = initSettings.initialPySeqPath;

        message = string.Format(@"scene.addAssetPath('seq', '{0}')", pySeqPath);
        PythonCommand(message);

        PythonCommand(string.Format(@"scene.setDoubleAttribute('scale', {0})", initSettings.scale));

        m_positionScaleHack = initSettings.positionScaleHack;

        if (!string.IsNullOrEmpty(initSettings.speechAudioFileBasePath))
        {
            SetSpeechAudiofileBasePath(initSettings.speechAudioFileBasePath);   // mediapath + audiofilepath + "sounds"  (overriden by 'path audio ...' in the .seq file)
        }

        if (!string.IsNullOrEmpty(initSettings.mediaPath))
        {
            SetMediaPath(initSettings.mediaPath);  // base path prepended to sound, seq file, and motion paths
        }


        LoadAssetPaths(initSettings.assetPaths);


        // locomotion/steering currently only working on some platforms  (can't find pprAI lib)
        if (Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            PythonCommand(string.Format(@"scene.getSteerManager().setDoubleAttribute('gridDatabaseOptions.gridSizeX', {0})", initSettings.steerGridSizeX));
            PythonCommand(string.Format(@"scene.getSteerManager().setDoubleAttribute('gridDatabaseOptions.gridSizeZ', {0})", initSettings.steerGridSizeZ));
            PythonCommand(string.Format(@"scene.getSteerManager().setIntAttribute('gridDatabaseOptions.maxItemsPerGridCell', {0})", initSettings.steerMaxItemsPerGridCell));

            // Toggle the steering manager
            PythonCommand(string.Format(@"scene.getSteerManager().setEnable(False)"));
            PythonCommand(string.Format(@"scene.getSteerManager().setEnable(True)"));
        }


        for (int i = 0; i < initSettings.initialCommands.Count; i++)
        {
            if (!String.IsNullOrEmpty(initSettings.initialCommands[i]))
            {
                PythonCommand(initSettings.initialCommands[i]);
            }
        }

        initSettings.TriggerPostLoadEvent();
    }

    public virtual void AddFaceDefinition(SmartbodyFaceDefinition face)
    {
        // don't add the face definition if it's already added.
        // TODO - remove m_faceDefinition once we're able to ask smartbody directly
        if (m_faceDefinitions.ContainsKey(face.definitionName))
        {
            return;
        }

        string message;

        // _default_ is already created by smartbody at startup
        if (face.definitionName != "_default_")
        {
            message = string.Format(@"scene.createFaceDefinition('{0}')", face.definitionName);
            PythonCommand(message);
        }

        message = string.Format(@"scene.getFaceDefinition('{0}').setFaceNeutral('{1}')", face.definitionName, face.neutral);
        PythonCommand(message);

        foreach (var au in face.actionUnits)
        {
            message = string.Format(@"scene.getFaceDefinition('{0}').setAU({1}, '{2}', '{3}')", face.definitionName, au.au, au.side, au.name);
            PythonCommand(message);
        }

        foreach (var viseme in face.visemes)
        {
            message = string.Format(@"scene.getFaceDefinition('{0}').setViseme('{1}', '{2}')", face.definitionName, viseme.Key, viseme.Value);
            PythonCommand(message);
        }

        m_faceDefinitions.Add(face.definitionName, face);
    }

    public void AddJointMap(SmartbodyJointMap jointMap)
    {
        // don't add the joint map if it's already added.
        // TODO - remove once we're able to ask smartbody directly
        if (m_jointMaps.ContainsKey(jointMap.mapName))
        {
            return;
        }

        PythonCommand(string.Format("scene.getJointMapManager().createJointMap('{0}')", jointMap.mapName));

        foreach (var joint in jointMap.mappings)
        {
            PythonCommand(string.Format("scene.getJointMapManager().getJointMap('{0}').setMapping('{1}', '{2}')", jointMap.mapName, joint.Key, joint.Value));
        }

        m_jointMaps.Add(jointMap.mapName, jointMap);
    }

    public void CreateMotion(TextAsset motionData)
    {
        PythonCommandUnformatted(string.Format(@"scene.createMotion(""{0}"")", motionData.name));

        string[] fileLines = motionData.text.Split('\n');
        string line = "";
        bool readingChannels = false;
        bool readingFrames = false;

        Debug.Log(fileLines.Length);

        for (int i = 0; i < fileLines.Length; i++)
        {
            line = fileLines[i];
            line = line.Trim();

            if (!readingChannels && !readingFrames && line.Contains("channels"))
            {
                readingChannels = true;
            }
            else if (!readingChannels && !readingFrames && line.Contains("frames"))
            {
                readingChannels = false;
                readingFrames = true;
            }
            else if (readingChannels)
            {
                if (string.IsNullOrEmpty(line))
                {
                    readingChannels = false;
                }
                else
                {
                    string[] jointAndChannel = line.Split(' ');
                    PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").addChannel(""{1}"", ""{2}"")", motionData.name, /*jointAndChannel[0]*/"base", jointAndChannel[1]));
                }
            }
            else if (readingFrames)
            {
                int index = line.IndexOf("fr");
                if (index == -1 || string.IsNullOrEmpty(line))
                {
                    readingFrames = false;
                }
                else
                {
                    string frameHeaderInfo = line.Substring(0, index + 1); // kt [time] fr
                    string keyTimeStr = frameHeaderInfo.Split(' ')[1]; //[time]

                    line = line.Remove(0, index + 3);
                    string[] channelData = line.Split(' ');

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("data = FloatVec()");
                    // parse key frame info
                    foreach (string data in channelData)
                    {
                        builder.AppendLine(string.Format("data.append({0})", data));
                    }

                    PythonCommandUnformatted(string.Format(@"{0}scene.getMotion(""{1}"").addFrame({2}, data)", builder.ToString(), motionData.name, keyTimeStr));
                }
            }
            //else if (!readingChannels && !readingFrames && line.Contains("time"))
            //{
            //    string[] syncNameAndTime = line.Split(':');
            //    PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, syncNameAndTime[0].Trim(), syncNameAndTime[1].Trim()));
            //}
        }

        // HACK!
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "start", 0));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "stroke", 2.0f));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "ready", 0.9f));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "relax", 2.5f));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "stroke_start", 1.5f));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "stroke_stop", 2.2f));
        SmartbodyManager.Get().PythonCommandUnformatted(string.Format(@"scene.getMotion(""{0}"").setSyncPoint(""{1}"", {2})", motionData.name, "stop", 3.0f));
    }

    public void MapSkeletonAndAssetPaths(SmartbodyJointMap jointMap, string skeletonName, List<KeyValuePair<string, string>> assetPaths)
    {
        PythonCommand(string.Format("scene.getJointMapManager().getJointMap('{0}').applySkeleton(scene.getSkeleton('{1}'))", jointMap.mapName, skeletonName));

        foreach (var path in assetPaths)
        {
            string [] files = Utils.GetStreamingAssetsFiles(path.Value, ".skm");
            foreach (var file in files)
            {
                string motionName = Path.GetFileNameWithoutExtension(file);
                PythonCommand(string.Format("scene.getJointMapManager().getJointMap('{0}').applyMotion(scene.getMotion('{1}'))", jointMap.mapName, motionName));
            }
        }
    }

    public virtual void Shutdown()
    {
        if (m_ID == new IntPtr(-1))
        {
            return;
        }

        if (SmartbodyExternals.Shutdown(m_ID))
        {
            Debug.Log("SmartbodyManager successfully shutdown");
        }
        else
        {
            Debug.LogError("SmartbodyManager failed to shutdown");
        }

        m_ID = new IntPtr(-1);

        m_startCalled = false;
    }

    public void AddCustomCharCreateCB(OnCustomCharacterCallback cb) { m_CustomCreateCBs += cb; }
    public void RemoveCustomCharCreateCB(OnCustomCharacterCallback cb) { m_CustomCreateCBs -= cb; }
    public void AddCustomCharDeleteCB(OnCustomCharacterCallback cb) { m_CustomDeleteCBs += cb; }
    public void RemoveCustomCharDeleteCB(OnCustomCharacterCallback cb) { m_CustomDeleteCBs -= cb; }
    public void RemoveCustomCallbacks()
    {
        m_CustomCreateCBs = null;
        m_CustomDeleteCBs = null;
    }

    public virtual void LoadAssetPaths(List<KeyValuePair<string, string>> assetPaths)
    {
        // TODO - need this code path for bonebus
#if false
        if (initSettings.assetPaths != null)
        {
            foreach (var pair in initSettings.assetPaths)
            {
                message = string.Format(@"scene.addAssetPath('{0}', '{1}')", pair.Key, pair.Value);
                PythonCommand(message);
            }
        }

        message = string.Format(@"scene.loadAssets()");
        PythonCommand(message);
#endif
        if (m_debugQuickLoadNoMotions)
            return;

        if (assetPaths != null)
        {
            foreach (var pair in assetPaths)
            {
                string skeletonName = pair.Key;
                string pathName = pair.Value;

                // load all .sk's
                {
                    string [] files = Utils.GetStreamingAssetsFiles(pathName, ".sk");
                    foreach (var file in files)
                    {
                        // don't load the skeleton if it's already added.
                        // TODO - remove once we're able to ask smartbody directly
                        if (m_loadedSkeletons.Contains(file))
                        {
                            continue;
                        }

                        //Debug.Log("LoadAssetPaths() - Loading '" + file + "'");

                        byte [] skeletonByte = Utils.LoadStreamingAssetsBytes(file);
                        if (skeletonByte != null)
                        {
                            IntPtr unmanagedPointer = Marshal.AllocHGlobal(skeletonByte.Length);
                            Marshal.Copy(skeletonByte, 0, unmanagedPointer, skeletonByte.Length);

                            SmartbodyExternals.LoadSkeleton(m_ID, unmanagedPointer, skeletonByte.Length, Path.GetFileName(file));

                            Marshal.FreeHGlobal(unmanagedPointer);

                            m_loadedSkeletons.Add(file);
                        }
                        else
                        {
                            Debug.Log("LoadSkeleton() fail - " + file);
                        }
                    }
                }

                // load all .skm's
                {
                    string [] files = Utils.GetStreamingAssetsFiles(pathName, ".skm");
                    foreach (var file in files)
                    {
                        string motionName = Path.GetFileNameWithoutExtension(file);

                        // don't load the motion if it's already added.
                        // TODO - remove once we're able to ask smartbody directly
                        if (m_loadedMotions.Contains(motionName))
                        {
                            continue;
                        }

                        //Debug.Log("LoadAssetPaths() - Loading '" + file + "'");

                        byte [] motionByte = Utils.LoadStreamingAssetsBytes(file);
                        if (motionByte != null)
                        {
                            IntPtr unmanagedPointer = Marshal.AllocHGlobal(motionByte.Length);
                            Marshal.Copy(motionByte, 0, unmanagedPointer, motionByte.Length);

                            SmartbodyExternals.LoadMotion(m_ID, unmanagedPointer, motionByte.Length, Path.GetFileName(file));

                            Marshal.FreeHGlobal(unmanagedPointer);

                            PythonCommand(string.Format(@"scene.getMotion('{0}').setMotionSkeletonName('{1}')", motionName, skeletonName));

                            m_loadedMotions.Add(motionName);
 
                        }
                        else
                        {
                            Debug.Log("LoadMotion() fail - " + file);
                        }
                    }
                }
            }
        }
    }

    public virtual void CreateCharacter(UnitySmartbodyCharacter unityCharacter)
    {
        // should only be called by UnitySmartbodyCharacter

        UnitySmartbodyCharacter existingCharacter = GetCharacterBySBMName(unityCharacter.SBMCharacterName);
        if (existingCharacter != null)
        {
            Debug.LogError(string.Format("ERROR - SmartbodyManager.CreateCharacter() - character '{0}' already exists.  Smartbody character names must be unique", unityCharacter.SBMCharacterName));
            return;
        }

        SmartbodyExternals.InitCharacter(m_ID, unityCharacter.SBMCharacterName, ref unityCharacter.m_CharacterData.m_Character);
        m_characterList.Add(unityCharacter);

        if (m_CustomCreateCBs != null && unityCharacter != null)
        {
            m_CustomCreateCBs(unityCharacter);
        }
    }

    public void RemoveCharacter(UnitySmartbodyCharacter character)
    {
        // should only be called by UnitySmartbodyCharacter

        //Debug.Log("SmartbodyManager.RemoveCharacter()");

        if (m_CustomDeleteCBs != null)
        {
            m_CustomDeleteCBs(character);
        }

        m_characterList.Remove(character);
    }

    public void AddPawn(SmartbodyPawn pawn)
    {
        // should only be called by SmartbodyPawn

        m_pawns.Add(pawn);
    }

    public void RemovePawn(SmartbodyPawn pawn)
    {
        m_pawns.Remove(pawn);
    }

    public void RefreshInit()
    {
        // this function should only be called if smartbody needs to be re-initialized.
        // this happens if you're using bonebus, because bonebus connects after the world is started up.
        // but might happen in other cases

        SmartbodyInit initSettings = GetComponent<SmartbodyInit>();
        if (initSettings != null)
        {
            Init(initSettings);
        }
    }

    public void RefreshPawns()
    {
        // this function should only be called if smartbody's representation of the number of pawns is different from Unity's.
        // this happens if you're using bonebus, because bonebus connects after the world is started up.
        // but might happen in other cases

        // remove all pawns first
        PythonCommand(@"scene.command('sbm pawn * remove')");
        m_pawns.Clear();

        SmartbodyPawn[] sbmPawns = (SmartbodyPawn[])Component.FindObjectsOfType(typeof(SmartbodyPawn));
        if (sbmPawns != null)
        {
            for (int i = 0; i < sbmPawns.Length; i++)
            {
                sbmPawns[i].AddToSmartbody();
            }
        }
    }

    public void RefreshCharacters()
    {
        // this function should only be called if smartbody's representation of the number of characters is different from Unity's.
        // this happens if you're using bonebus, because bonebus connects after the world is started up.
        // but might happen in other cases

        // remove all characters first
        PythonCommand(@"scene.command('sbm char * remove')");
        m_characterList.Clear();

        UnitySmartbodyCharacter[] sbmCharacters = (UnitySmartbodyCharacter[])Component.FindObjectsOfType(typeof(UnitySmartbodyCharacter));
        if (sbmCharacters != null)
        {
            for (int i = 0; i < sbmCharacters.Length; i++)
            {
                SmartbodyCharacterInit init = sbmCharacters[i].GetComponent<SmartbodyCharacterInit>();
                if (init != null)
                {
                    SmartbodyFaceDefinition face = sbmCharacters[i].GetComponent<SmartbodyFaceDefinition>();   // ok if it's null
                    SmartbodyJointMap [] jointMaps = sbmCharacters[i].GetComponents<SmartbodyJointMap>();   // ok if it's null
                    sbmCharacters[i].CreateCharacter(init, face, jointMaps);
                }
            }
        }
    }

    public void RemoveAllSBObjects()
    {
        PythonCommand(@"scene.command('sbm char * remove')");
        PythonCommand(@"scene.command('sbm pawn * remove')");
        m_characterList.Clear();
        m_pawns.Clear();
    }

    public virtual void SetMediaPath(string path)
    {
        SmartbodyExternals.SetMediaPath(m_ID, path);
    }

    public virtual void SetSpeechAudiofileBasePath(string path)
    {
        SmartbodyExternals.SetSpeechAudiofileBasePath(m_ID, path);
    }

    public virtual void SetProcessId(string id)
    {
        SmartbodyExternals.SetProcessId(m_ID, id);
        SmartbodyExternals.SetDebuggerId(m_ID, id);
    }

    public virtual void ProcessVHMsgs(string opCode, string parsedArgs)
    {
        if (!gameObject.activeSelf)
            return;

        //Debug.Log("SmartbodyManager.ProcessVHMsgs() - " + opCode + " " + parsedArgs);

        SmartbodyExternals.ProcessVHMsgs(m_ID, opCode, parsedArgs);
    }

    protected void InitConsole()
    {
        DebugConsole console = DebugConsole.Get();
        if (console == null)
        {
            Debug.LogWarning("There is no DebugConsole in the scene and SmartbodyManager is trying to use one");
            return;
        }
        console.AddCommandCallback("python", PythonCallback);
    }

    public void PythonCallback(string commandEntered, DebugConsole console)
    {
        if (commandEntered.IndexOf(PythonCmd) != -1)
        {
            string returnType = string.Empty;
            string pythonCommand = string.Empty;
            if (console.ParseVHMSG(commandEntered, ref returnType, ref pythonCommand))
            {
                // so I don't have to call tolower every check
                string ret = returnType.ToLower();

                // if the command has no return type, the number of arguements varies since you don't have to type 'void'
                if (string.IsNullOrEmpty(pythonCommand))
                    PythonCommand(returnType);
                else if (ret == "bool")
                    Debug.Log(PythonCommand<bool>(pythonCommand));
                else if (ret == "int")
                    Debug.Log(PythonCommand<int>(pythonCommand));
                else if (ret == "float")
                    Debug.Log(PythonCommand<float>(pythonCommand));
                else if (ret == "string")
                    Debug.Log(PythonCommand<string>(pythonCommand));
            }
        }
    }

    protected void SubscribeVHMsg()
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg == null)
        {
            Debug.LogWarning("There is no VHMsgBase in the scene and SmartbodyManager is trying to use one");
            return;
        }

        vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(MessageAction));

        // sbm related vhmsgs
        vhmsg.SubscribeMessage("vrExpress");
        vhmsg.SubscribeMessage("vrSpeak");
        vhmsg.SubscribeMessage("vrSpeech");
        vhmsg.SubscribeMessage("vrSpoke");
        vhmsg.SubscribeMessage("RemoteSpeechReply");
        vhmsg.SubscribeMessage("PlaySound");
        vhmsg.SubscribeMessage("StopSound");
        vhmsg.SubscribeMessage("unity");
        vhmsg.SubscribeMessage("sb");
        vhmsg.SubscribeMessage("sbm");
        vhmsg.SubscribeMessage("sbmdebugger");
        vhmsg.SubscribeMessage("vrPerception");
        vhmsg.SubscribeMessage("vrAgentBML");
    }

    protected void MessageAction(object sender, VHMsgBase.Message args)
    {
        var split = VHMsgBase.SplitIntoOpArg(args.s);
        ProcessVHMsgs(split.Key, split.Value);
    }

    bool GetUnityCharacterData(string characterName, ref UnitySmartbodyCharacter.UnityCharacterData characterData)
    {
        try
        {
            // pass the character struct to native code and have it filled out
            if (!SmartbodyExternals.GetCharacter(m_ID, characterName, ref characterData.m_Character))
            {
                // something went bad in native code
                Debug.Log("Couldn't update character " + characterName);
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        if (characterData.jx == null || characterData.jx.Length != characterData.m_Character.m_numJoints)
        {
            characterData.jnames = new IntPtr[characterData.m_Character.m_numJoints];
            characterData.jx = new float[characterData.m_Character.m_numJoints];
            characterData.jy = new float[characterData.m_Character.m_numJoints];
            characterData.jz = new float[characterData.m_Character.m_numJoints];
            characterData.jrw = new float[characterData.m_Character.m_numJoints];
            characterData.jrx = new float[characterData.m_Character.m_numJoints];
            characterData.jry = new float[characterData.m_Character.m_numJoints];
            characterData.jrz = new float[characterData.m_Character.m_numJoints];

            characterData.jprevx = new float[characterData.m_Character.m_numJoints];
            characterData.jprevy = new float[characterData.m_Character.m_numJoints];
            characterData.jprevz = new float[characterData.m_Character.m_numJoints];
            characterData.jprevrw = new float[characterData.m_Character.m_numJoints];
            characterData.jprevrx = new float[characterData.m_Character.m_numJoints];
            characterData.jprevry = new float[characterData.m_Character.m_numJoints];
            characterData.jprevrz = new float[characterData.m_Character.m_numJoints];

            characterData.jNextUpdateTime = new float[characterData.m_Character.m_numJoints];
        }

        characterData.jx.CopyTo(characterData.jprevx, 0);
        characterData.jy.CopyTo(characterData.jprevy, 0);
        characterData.jz.CopyTo(characterData.jprevz, 0);
        characterData.jrw.CopyTo(characterData.jprevrw, 0);
        characterData.jrx.CopyTo(characterData.jprevrx, 0);
        characterData.jry.CopyTo(characterData.jprevry, 0);
        characterData.jrz.CopyTo(characterData.jprevrz, 0);

        Marshal.Copy(characterData.m_Character.jname, characterData.jnames, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jx, characterData.jx, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jy, characterData.jy, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jz, characterData.jz, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jrw, characterData.jrw, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jrx, characterData.jrx, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jry, characterData.jry, 0, (int)characterData.m_Character.m_numJoints);
        Marshal.Copy(characterData.m_Character.jrz, characterData.jrz, 0, (int)characterData.m_Character.m_numJoints);

        return true;
    }

    public int OnCharacterCreate(IntPtr sbmID, string name, string objectClass)
    {
        //Debug.Log("OnCharacterCreate() - name: " + name + " objectClass: " + objectClass);

        if (objectClass == "pawn")
        {
            return 0;
        }
        else
        {
            return 0;
        }
    }

    public int OnCharacterDelete(IntPtr sbmID, string name)
    {
        //Debug.Log(string.Format("OnCharacterDelete() - {0}", name));

        return 0;
    }

    public int OnCharacterChange(IntPtr sbmID, string name)
    {
        //Debug.Log(string.Format("OnCharacterChange() - {0}", name));

        return 0;
    }

    public int OnViseme(IntPtr sbmID, string name, string visemeName, float weight, float blendTime)
    {
        return 0;
    }

    protected int OnChannel(IntPtr sbmID, string name, string channelName, float value)
    {
        return 0;
    }

#region Helper Functions
    public void OnLogMessage(string message, int messageType)
    {
        if (m_displayLogMessages)
        {
            string m = "sbm: " + message;
            switch (messageType)
            {
                case 2:
                    Debug.LogWarning(m);
                    break;

                case 1:
                    Debug.LogError(m);
                    break;

                default:
                    Debug.Log(m);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets a character by the name that it is known by in unity
    /// In the seq file command, "char brad1 init common.sk data/prefabs/brad", data/prefabs/brad would be the character name
    /// </summary>
    /// <param name="name">The smartbody character's prefab name</param>
    /// <returns></returns>
    public UnitySmartbodyCharacter GetCharacterByName(string name)
    {
        for (int i = 0; i < m_characterList.Count; i++)
        {
            if (String.Compare(m_characterList[i].CharacterName, name, true) == 0)
            {
                return m_characterList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a character by the name that it is known by in smartbodydll or bonebus
    /// In the seq file command, "char brad1 init common.sk data/prefabs/brad", brad1 would be the smartbody name
    /// </summary>
    /// <param name="name">What the character is know by in smartbody</param>
    /// <returns></returns>
    public UnitySmartbodyCharacter GetCharacterBySBMName(string name)
    {
        for (int i = 0; i < m_characterList.Count; i++)
        {
            if (String.Compare(m_characterList[i].SBMCharacterName, name, true) == 0)
            {
                return m_characterList[i];
            }
        }

        return null;
    }

    public UnitySmartbodyCharacter GetCharacterByID(int id)
    {
        for (int i = 0; i < m_characterList.Count; i++)
        {
            if (m_characterList[i].CharacterID == id)
            {
                return m_characterList[i];
            }
        }

        return null;
    }

    public void ToggleDebugFlag(UnitySmartbodyCharacter.DebugFlags flag)
    {
        for (int i = 0; i < m_characterList.Count; i++)
        {
            m_characterList[i].ToggleDebugFlag(flag);
        }
    }

    public static float FindSkmLength(string fullSkmPath)
    {
        SkmMetaData metaData = FindSkmMetaData(fullSkmPath);
        return metaData != null ? metaData.Length : -1;
    }

    public static SkmMetaData FindSkmMetaData(string fullSkmPath)
    {
        SkmMetaData metaData = new SkmMetaData();

        if (string.IsNullOrEmpty(fullSkmPath) || Path.GetExtension(fullSkmPath) != ".skm")
        {
            //Debug.LogError(string.Format("Couldn't located skm file {0}", skmName));
            return metaData;
        }

        string prevLine = "";
        string line = "";
        StreamReader reader = new StreamReader(fullSkmPath);
        try
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "" && prevLine.IndexOf("kt") != -1)
                {
                    prevLine = prevLine.Remove(0, 3);
                    int firstSpace = prevLine.IndexOf(" ");
                    prevLine = prevLine.Substring(0, firstSpace);
                    metaData.Length = float.Parse(prevLine);
                    //break;
                }
                else
                {
                    foreach (string s in SyncPointNames)
                    {
                        if (line.IndexOf(s) != -1)
                        {
                            string[] split = line.Split(' ');
                            if (split.Length >= 2)
                            {
                                float time;
                                if (float.TryParse(split[split.Length - 1], out time))
                                {
                                    metaData.SyncPoints.Add(s, time);
                                }
                                else
                                {
                                    Debug.LogError(string.Format("Error when parsing sync point {0} on skm {1} line {2}", s, fullSkmPath, line));
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Error when parsing sync point {0} on skm {1} line {2}", s, fullSkmPath, line));
                            }
                            break;
                        }
                    }
                }
                prevLine = line;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("FindSkmLength exeception on file {0}. Exception: {1}", fullSkmPath, e.Message));
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
            }
        }

        return metaData;
    }
#endregion

    protected virtual void OnSetCharacterPosition(UnitySmartbodyCharacter c, Vector3 pos)
    {
        c.transform.localPosition = pos * m_positionScaleHack;
    }

    protected virtual void OnSetCharacterRotation(UnitySmartbodyCharacter c, Quaternion rot)
    {
        c.transform.localRotation = rot;
    }

    /// <summary>
    /// Returns the world position of the specified sbm character's AudioSource componenet
    /// </summary>
    /// <param name="sbmCharName">What the chracter is called inside of sbm</param>
    /// <returns></returns>
    public Vector3 GetCharacterVoicePosition(string sbmCharName)
    {
        AudioSource voice = GetCharacterVoice(sbmCharName);
        return voice != null ? voice.transform.position : Vector3.zero;
    }

    /// <summary>
    /// Returns the characters AudioSource that is placed at their mouth
    /// </summary>
    /// <param name="sbmCharName"></param>
    /// <returns></returns>
    public AudioSource GetCharacterVoice(string sbmCharName)
    {
        UnitySmartbodyCharacter character = GetCharacterBySBMName(sbmCharName);
        if (character == null)
        {
            return null;
        }

        if (character.AudioSource == null)
        {
            Debug.LogError("GetCharacterVoice failed. " + sbmCharName + " doesn't have an AudioSource componenet");
            return null;
        }

        return character.AudioSource;
    }

    public string[] GetSBMCharacterNames()
    {
        string[] retval = new string[m_characterList.Count];
        for (int i = 0; i < retval.Length; i++)
        {
            retval[i] = m_characterList[i].SBMCharacterName;
        }
        return retval;
    }

    public string[] GetPawnNames()
    {
        string[] retval = new string[m_pawns.Count];
        for (int i = 0; i < retval.Length; i++)
        {
            retval[i] = m_pawns[i].PawnName;
        }
        return retval;
    }

    private string FormatPythonCommand(string command)
    {
        return command.Insert(0, "ret = ");
    }

    public virtual void PythonCommand(string command)
    {
        SmartbodyExternals.PythonCommandVoid(m_ID, FormatPythonCommand(command));
    }

    public virtual void PythonCommandUnformatted(string command)
    {
        SmartbodyExternals.PythonCommandVoid(m_ID, command);
    }

    public virtual T PythonCommand<T>(string command)
    {
        T retVal = default(T);
        Type type = typeof(T);

        if (type == typeof(bool))
        {
            retVal = (T)(object)SmartbodyExternals.PythonCommandBool(m_ID, FormatPythonCommand(command));
        }
        else if (type == typeof(int))
        {
            retVal = (T)(object)SmartbodyExternals.PythonCommandInt(m_ID, FormatPythonCommand(command));
        }
        else if (type == typeof(float))
        {
            retVal = (T)(object)SmartbodyExternals.PythonCommandFloat(m_ID, FormatPythonCommand(command));
        }
        else if (type == typeof(string))
        {
            StringBuilder output = new StringBuilder(256);
            SmartbodyExternals.PythonCommandString(m_ID, FormatPythonCommand(command), output, output.Capacity);
            retVal = (T)(object)output.ToString();
        }
        else
        {
            Debug.LogError("PythonCommand failed. Only types bool, int, float, and string are currently supported."
                + " For void return types, use the other PythonCommand overload");
        }

        return retVal;
    }

    public void QueueCharacterToUpload(Character character)
    {
        if (m_CharactersToUpload.Contains(character))
        {
            return;
        }

        m_CharactersToUpload.Add(character);
    }

    public void UploadCharacterTransforms()
    {
        m_CharactersToUpload.ForEach(sbmChar => UploadCharacterTransform(sbmChar));
        m_CharactersToUpload.Clear();
    }

    void UploadCharacterTransform(Character sbmChar)
    {
        SBTransform(sbmChar.CharacterName, sbmChar.transform);
    }

    public void ForEachCharacter(Action<UnitySmartbodyCharacter> action)
    {
        m_characterList.ForEach(action);
    }

    static public GazeJointRange ParseGazeJointRange(string value)
    {
        string temp = value.Replace(" ", "_");
        string[] enumValues = Enum.GetNames(typeof(GazeJointRange));
        
        // first attemp to match the string to the enum
        for (int i = 0; i < enumValues.Length; i++)
        {
            if (temp.ToLower() == enumValues[i].ToLower())
            {
                return (GazeJointRange)Enum.Parse(typeof(GazeJointRange), temp, true);
            }
            else
            {
                string[] words = enumValues[i].Split('_');
	            Array.Reverse(words);
	            string reverse = string.Join("_", words);
                // there is no match, so reverse the enum string and try that
                if (temp.ToLower() == reverse.ToLower())
                {
                    return (GazeJointRange)Enum.Parse(typeof(GazeJointRange), enumValues[i], true);
                }
            }
        }

        // if you got this far, an exception will be thrown
        return (GazeJointRange)Enum.Parse(typeof(GazeJointRange), temp, true);
    }

#endregion


    public override void SBRunPythonScript(string script)
    {
        // when accessing the .py from the file system isn't possible (android), can try something like the below
        //WWW www = Utils.LoadStreamingAssets(script);
        //sbm.PythonCommand(string.Format(@"scene.command('sbm python {0}')", www.text));

        string command = string.Format(@"scene.run('{0}')", script);
        PythonCommand(command);
    }

    public override void SBMoveCharacter(string character, string direction, float fSpeed, float fLrps, float fFadeOutTime)
    {
        string command = string.Format(@"scene.command('sbm test loco char {0} {1} spd {2} rps {3} time {4}')", character, direction, fSpeed, fLrps, fFadeOutTime);
        PythonCommand(command);
    }

    public override void SBWalkTo(string character, string waypoint, bool isRunning)
    {
        string run = isRunning ? @"manner=""run""" : "";
        string message = string.Format(@"bml.execBML('{0}', '<locomotion target=""{1}"" facing=""{2}"" {3} />')", character, waypoint, waypoint, run);
        PythonCommand(message);
    }

    public override void SBWalkImmediate(string character, string locomotionPrefix, double velocity, double turn, double strafe)
    {
        //<sbm:states mode="schedule" loop="true" name="allLocomotion" x="100" y ="0" z="0"/>
        string message = string.Format(@"bml.execBML('{0}', '<sbm:states mode=""schedule"" loop=""true"" sbm:startnow=""true"" name=""{1}"" x=""{2}"" y =""{3}"" z=""{4}"" />')", character, locomotionPrefix, velocity, turn, strafe);
        PythonCommand(message);
    }

    public override void SBPlayAudio(string character, string audioId)
    {
        string message = string.Format(@"bml.execXML('{0}', '<act><participant id=""{1}"" role=""actor""/><bml><speech id=""sp1"" ref=""{2}"" type=""application/ssml+xml""></speech></bml></act>')", character, character, audioId);
        PythonCommand(message);
    }

    public override void SBPlayAudio(string character, string audioId, string text)
    {
        string message = string.Format(@"bml.execXML('{0}', '<act><participant id=""{1}"" role=""actor""/><bml><speech id=""sp1"" ref=""{2}"" type=""application/ssml+xml"">{3}</speech></bml></act>')", character, character, audioId, text);
        PythonCommand(message);
    }

    public override void SBPlayAudio(string character, AudioClip audioId)
    {
        SBPlayAudio(character, audioId.name);
    }

    public override void SBPlayAudio(string character, AudioClip audioId, string text)
    {
        SBPlayAudio(character, audioId.name, text);
    }

    public override void SBPlayXml(string character, string xml)
    {
        string message = string.Format(@"scene.command('bml char {0} file {1}')", character, xml);
        PythonCommand(message);
    }

    public override void SBTransform(string character, Transform transform)
    {
        SBTransform(character, transform.position, transform.rotation);
    }

    public override void SBTransform(string character, Vector3 pos, Quaternion rot)
    {
        Vector3 position = pos / m_positionScaleHack;
        Vector3 eulerAngles = rot.eulerAngles;

        SBTransform(character, -position.x, position.y, position.z, -eulerAngles.y, eulerAngles.x, -eulerAngles.z);
    }

    public override void SBTransform(string character, double x, double y, double z)
    {
        string message = string.Format(@"scene.command('set character {0} world_offset x {1} y {2} z {3}')", character, x, y, z);
        PythonCommand(message);
    }

    public override void SBTransform(string character, double y, double p)
    {
        
        string message = string.Format(@"scene.command('set character {0} world_offset y {1} p {2}')", character, y, p);
        PythonCommand(message);
    }

    public override void SBTransform(string character, double x, double y, double z, double h, double p, double r)
    {
        string message = string.Format(@"scene.command('set character {0} world_offset x {1} y {2} z {3} h {4} p {5} r {6}')", character, x, y, z, h, p, r);
        PythonCommand(message);
    }

    public override void SBRotate(string character, double h)
    {
        string message = string.Format(@"scene.command('set character {0} world_offset h {1}')", character, h);
        PythonCommand(message);
    }

    public override void SBPosture(string character, string posture, float startTime)
    {
        string message = string.Format(@"bml.execBML('{0}', '<body posture=""{1}"" start=""{2}""/>')", character, posture, startTime);
        PythonCommand(message);
    }

    public override void SBPlayAnim(string character, string animName)
    {
        string message = string.Format(@"bml.execBML('{0}', '<animation name=""{1}""/>')", character, animName);
        PythonCommand(message);
    }

    public override void SBPlayAnim(string character, string animName, float readyTime, float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
    {
        string message = string.Format(@"bml.execBML('{0}', '<animation name=""{1}"" start=""0"" ready=""{2}"" stroke=""{3}"" relax=""{4}""/>')",
            character, animName, readyTime.ToString("f6"), strokeTime.ToString("f6"), relaxTime.ToString("f6"));
        PythonCommand(message);
    }

    public override void SBPlayFAC(string character, int au, FaceSide side, float weight, float duration)
    {
        // TODO - add blend in/out time
        // side == "left", "right" or "both"
        string message = string.Format(@"bml.execBML('{0}', '<face type=""facs"" au=""{1}"" side=""{2}"" amount=""{3}"" sbm:duration=""{4}"" />')", character, au, side.ToString(), weight, duration);
        PythonCommand(message);
    }

    public override IEnumerator SBPlayViseme(string character, string viseme, float weight, float totalTime, float blendTime)
    {
        // sbm char * viseme W 0 1

        // Please note this needs to be called from a StartCoroutine()!
        // sbm viseme command is an immediate command, doesn't have a total duration.  So we send one command to go to weight, then another to go to 0

        string message = string.Format(@"scene.command('char {0} viseme {1} {2} {3}')", character, viseme, weight, blendTime);
        PythonCommand(message);

        yield return new WaitForSeconds(totalTime - blendTime);

        message = string.Format(@"scene.command('char {0} viseme {1} {2} {3}')", character, viseme, 0, blendTime);
        PythonCommand(message);
    }

    public override void SBNod(string character, float amount, float repeats, float time)
    {
        string message = string.Format(@"bml.execBML('{0}', '<head amount=""{1}"" repeats=""{2}"" type=""{3}"" start=""{4}"" end=""{5}""/>')", character, amount, repeats, "NOD", 0, time);
        PythonCommand(message);
    }

    public override void SBShake(string character, float amount, float repeats, float time)
    {
        string message = string.Format(@"bml.execBML('{0}', '<head amount=""{1}"" repeats=""{2}"" type=""{3}"" start=""{4}"" end=""{5}""/>')", character, amount, repeats, "SHAKE", 0, time);
        PythonCommand(message);
    }

    public override void SBGaze(string character, string gazeAt)
    {
        string message = string.Format(@"bml.execBML('{0}', '<gaze target=""{1}""/>')", character, gazeAt);
        PythonCommand(message);
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed)
    {
        string message = string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-speed=""{2}""/>')", character, gazeAt, neckSpeed.ToString("f2"));
        PythonCommand(message);
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed, float eyeSpeed, SmartbodyManager.GazeJointRange jointRange)
    {
        string message = string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-speed=""{2} {3}"" sbm:joint-range=""{4}""/>')",
            character, gazeAt, neckSpeed.ToString("f2"), eyeSpeed.ToString("f2"), jointRange.ToString().Replace("_", " "));
        PythonCommand(message);
    }

    public override void SBGaze(string character, string gazeAt, SmartbodyManager.GazeTargetBone targetBone, SmartbodyManager.GazeDirection gazeDirection,
            SmartbodyManager.GazeJointRange jointRange, float angle, float headSpeed, float eyeSpeed, float fadeOut, string gazeHandleName, float duration)
    {
        string gazeTargetString = string.Format("{0}", gazeAt);
        if (targetBone != GazeTargetBone.NONE)
        {
            gazeTargetString += string.Format(":{0}", targetBone);
        }
        string message = string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" direction=""{2}"" sbm:joint-range=""{3}"" angle=""{4}"" sbm:joint-speed=""{5} {6}"" id=""{7}"" sbm:handle=""{7}"" start=""0""/>')",
            character, gazeTargetString, gazeDirection, jointRange.ToString().Replace("_", " "), angle, headSpeed.ToString("f2"), eyeSpeed.ToString("f2"), gazeHandleName, fadeOut);
        PythonCommand(message);

        //<event message="sbm char ChrRachelPrefab gazefade out 0.8" stroke="mygaze1:start+5.255221" />
        if (duration > 0) // duration of 0 means gaze infinitely
        {
            message = string.Format(@"bml.execBML('{0}', '<event message=""sbm char {0} gazefade out {1}"" stroke=""{2}:start+{3}""/>')",
            character, fadeOut, gazeHandleName, duration);
            PythonCommand(message);
        }
    }

    public override void SBStopGaze(string character)
    {
        SBStopGaze(character, 1);
    }

    public override void SBStopGaze(string character, float fadoutTime)
    {
        string message = string.Format(@"scene.command('char {0}  gazefade out {1}')", character, fadoutTime);
        PythonCommand(message);
    }

    public override void SBSaccade(string character, SaccadeType type, bool finish, float duration)
    {
        //<event message="sbm bml char ChrRachelPrefab &lt;saccade finish=&quot;true&quot;/&gt;" stroke="0" type="end" track="Saccade" />
        string message = string.Format(@"bml.execBML('{0}', '<event message=""sbm bml char {0} &lt;saccade finish=&quot;{1}&quot; mode=&quot;{2}&quot; sbm:duration=&quot;{3}&quot; /&gt;"" type=""{2}"" />')",
            character, finish.ToString().ToLower(), type, duration);
        Debug.Log(message);
        PythonCommand(message);
    }

    public override void SBSaccade(string character, SaccadeType type, bool finish, float duration, float angleLimit, float direction, float magnitude)
    { 
        string message = string.Format(@"bml.execBML('{0}', '<event message=""sbm bml char {0}"" &lt;saccade finish=&quot;{1}&quot; mode=&quot;{2}&quot; sbm:duration=&quot;{3}&quot; angle-limit=&quot;{4}&quot; direction=&quot;{5}&quot; magnitude=&quot;{6}&quot; /&gt;"" type=""{1}"" />')",
            character, finish, type, duration, angleLimit, direction, magnitude);
        PythonCommand(message);
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode)
    {
        string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}""/>')", character, state, mode, wrapMode, scheduleMode);
        PythonCommand(message);
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x)
    {
        string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}""/>')", character, state, mode, wrapMode, scheduleMode,x.ToString());
        PythonCommand(message);
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x, float y, float z)
    {
        string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}"" y=""{6}"" z=""{7}""/>')", character, state, mode, wrapMode, scheduleMode, x.ToString(), y.ToString(), z.ToString());
        PythonCommand(message);
    }

    public override string SBGetCurrentStateName(string character)
    {
        string pyCmd = string.Format(@"scene.getStateManager().getCurrentState('{0}')", character);
        return PythonCommand<string>(pyCmd);
    }

    public override Vector3 SBGetCurrentStateParams(string character)
    {
        Vector3 ret = new Vector3();
        string pyCmd = string.Empty;

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(0)", character);
        ret.x = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(1)", character);
        ret.y = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(2)", character);
        ret.z = PythonCommand<float>(pyCmd);

        return ret;
    }

    public override bool SBIsStateScheduled(string character, string stateName)
    {
        string pyCmd = string.Format(@"scene.getStateManager().isStateScheduled('{0}', '{1}')", character, stateName);
        return PythonCommand<bool>(pyCmd);
    }

    public override float SBGetAuValue(string character, string auName)
    {
        string pyCmd = string.Format(@"scene.getCharacter('{0}').getSkeleton().getJointByName('{1}').getPosition().getData(0)", character, auName);
        return PythonCommand<float>(pyCmd);
    }

    public override void SBExpress(string character, string uttID, string uttNum, string text)
    {
        string message = string.Format("vrExpress {0} user 1303332588320-{2}-1 <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>"
            + "<act><participant id=\"{0}\" role=\"actor\" /><fml><turn start=\"take\" end=\"give\" /><affect type=\"neutral\" "
            + "target=\"addressee\"></affect><culture type=\"neutral\"></culture><personality type=\"neutral\"></personality></fml>"
            + "<bml><speech id=\"sp1\" ref=\"{1}\" type=\"application/ssml+xml\">{3}</speech></bml></act>", character, uttID, uttNum, text);
        VHMsgBase.Get().SendVHMsg(message);
    }
}
