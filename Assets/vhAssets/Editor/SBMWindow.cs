using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// Used for smartbody character debugging/functionality without the reliance on bonebus.
/// </summary>
public class SBMWindow : EditorWindow
{
    #region Constants
    const float MaxToggleWidth = 130;
    const float SpaceBetweenGroups = 10;
    const float MaxButtonWidth = 100;
    #endregion

    #region Variables
    const string SavedWindowPosXKey = "SBMWindowX";
    const string SavedWindowPosYKey = "SBMWindowY";
    const string SavedWindowWKey = "SBMWindowW";
    const string SavedWindowHKey = "SBMWindowH";
    const string WildCard = "*";

    static SBMWindow ThisWindow;

    // debug view toggles
    bool m_bShowBones;
    bool m_bShowAxes;
    bool m_bShowEyeBeams;
    bool m_bBoneUpdates = true;
    bool m_JustToggledBoneUpdate = false;

    // spawn types combo box
    string[] SpawnTypes = new string[2] { "Pawn", "Character" };
    int m_nSelectedSpawnIndex = 0;

    // spawn name typing field
    string m_SpawnName = "Type name here";

    // spawn position vector 3 field
    Vector3 m_Position = new Vector3();


    // character selection
    string[] m_CharacterNames;
    int m_nCharacterSelection = 0;

    // pawn selection
    string[] m_PawnNames;
    int m_nPawnSelection = 0;

    static List<string> m_AnimationNames = new List<string>();
    static List<string> m_AnimationNamesBank = new List<string>();
    string m_AnimSearchText = "";
    int m_nSelectedAnimIndex = 0;

    // Speech box
    string m_SpeechText = "Hello. I am a virtual human";

    // Gaze
    int m_nGazeTargetSelection = 0;
    string[] m_GazeTargets;

    // Joints
    Transform m_PrevSelectedTransform = null;
    Vector3 m_PrevTransformPos;
    Quaternion m_PrevTransformRot;

    // bml/xml
    string[] m_FileNames;
    int m_nSelectedFileIndex = 0;

    // focus
    int m_nFocusIndex = 0;
    List<string> m_JointNames = new List<string>();

    bool m_bWasPlayingApplication = false;
    #endregion

    #region Function
    [MenuItem("VH/SBM Window")]
    static void Init()
    {
        ThisWindow = (SBMWindow)EditorWindow.GetWindow(typeof(SBMWindow));
        ThisWindow.autoRepaintOnSceneChange = true;
        ThisWindow.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(SavedWindowWKey, 421),
            PlayerPrefs.GetFloat(SavedWindowHKey, 676));
    }

    void Update()
    {
        if (m_bWasPlayingApplication && !Application.isPlaying)
        {
            Reset();
        }
        m_bWasPlayingApplication = Application.isPlaying;
    }

    #region Draw Function
    void OnGUI()
    {
        GUILayout.BeginVertical();
        {
            DrawAvailableSBMObjects();

            DrawSpace(SpaceBetweenGroups);
            DrawAnimationGUI();

            DrawSpace(SpaceBetweenGroups);
            DrawSpawnGUI();

            DrawSpace(SpaceBetweenGroups);
            DrawSpeechGUI();

            DrawSpace(SpaceBetweenGroups);
            DrawGazeGUI();

            DrawSpace(SpaceBetweenGroups);
            DrawBMLGUI();

            DrawSpace(SpaceBetweenGroups);
            DrawUtilsGUI();

        }
        GUILayout.EndVertical();
    }

    void DrawSpace(float space)
    {
        EditorGUILayout.Separator();
        GUILayout.Space(SpaceBetweenGroups);  
    }

    void DrawAvailableSBMObjects()
    {
        EditorGUILayout.LabelField("Selected", EditorStyles.boldLabel);
        // character selection
        m_CharacterNames = GetAllCharacterNames();
        int prevCharacterSelection = m_nCharacterSelection;
        if (m_CharacterNames != null)
        {
            // clamp the value in range of the array
            m_nCharacterSelection = Mathf.Clamp(m_nCharacterSelection, 0, m_CharacterNames.Length - 1);
            m_nCharacterSelection = EditorGUILayout.Popup("Characters", m_nCharacterSelection, m_CharacterNames);

            if (prevCharacterSelection != m_nCharacterSelection)
            {
                m_JointNames = GetCharacterJointNames(m_CharacterNames[m_nCharacterSelection]);
            }
        }

        // pawn selection
        m_PawnNames = GetAllPawnNames();
        if (m_PawnNames != null)
        {
            m_nPawnSelection = Mathf.Clamp(m_nPawnSelection, 0, m_PawnNames.Length - 1);
            m_nPawnSelection = EditorGUILayout.Popup("Pawns", m_nPawnSelection, m_PawnNames);
        }

        if (m_JointNames != null && m_JointNames.Count == 0 && m_CharacterNames != null)
        {
            m_JointNames = GetCharacterJointNames(m_CharacterNames[m_nCharacterSelection]);
        }
    }

    void DrawSpawnGUI()
    {
        EditorGUILayout.LabelField("Creation", EditorStyles.boldLabel);
        // Spawn variables
        m_nSelectedSpawnIndex = EditorGUILayout.Popup("Spawn Types", m_nSelectedSpawnIndex, SpawnTypes);
        m_SpawnName = EditorGUILayout.TextField(SpawnTypes[m_nSelectedSpawnIndex] == "Pawn" ? "Pawn Name" : "Seq File", m_SpawnName);
        m_Position = EditorGUILayout.Vector3Field("Spawn Position", m_Position);
        if (GUILayout.Button("Create", GUILayout.MaxWidth(MaxButtonWidth)))
        {
            SpawnEntity(SpawnTypes[m_nSelectedSpawnIndex], m_SpawnName, m_Position);
        }
    }

    void DrawAnimationGUI()
    {
        // animations     
        EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
        string previousAnimSearchText = m_AnimSearchText;
        m_AnimSearchText = EditorGUILayout.TextField("Filter", m_AnimSearchText);
        if (previousAnimSearchText != m_AnimSearchText)
        {
            SearchForAnim();
        }
        m_nSelectedAnimIndex = EditorGUILayout.Popup("Selected", m_nSelectedAnimIndex, m_AnimationNames.ToArray());

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Play", GUILayout.MaxWidth(MaxButtonWidth)))
            {
                PlayAnimation(m_CharacterNames[m_nCharacterSelection], m_AnimationNames[m_nSelectedAnimIndex]);
            }

            if (GUILayout.Button("Query Anims", GUILayout.MaxWidth(MaxButtonWidth)))
            {
                m_AnimationNames.Clear();
                SetupVHMsgCallback();
                VHMsgBase.Get().SubscribeMessage("sbmlog");
                SendVHMsg("sbm vhmsglog on");
                SendVHMsg("sbm resource motion");
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawSpeechGUI()
    {
        // speech
        EditorGUILayout.LabelField("Speech", EditorStyles.boldLabel);
        m_SpeechText = EditorGUILayout.TextField("Speech Text", m_SpeechText);
        if (GUILayout.Button("Send", GUILayout.MaxWidth(MaxButtonWidth)) && m_CharacterNames != null)
        {
            SendSpeech(m_CharacterNames[m_nCharacterSelection], m_SpeechText);
        }
    }

    void DrawGazeGUI()
    {
        // gaze
        EditorGUILayout.LabelField("Gaze", EditorStyles.boldLabel);
        m_GazeTargets = GetAllEntityNames();
        if (m_GazeTargets != null)
        {
            m_nGazeTargetSelection = EditorGUILayout.Popup("Gaze Target", m_nGazeTargetSelection, m_GazeTargets);
        }
        if (GUILayout.Button("Gaze At", GUILayout.MaxWidth(MaxButtonWidth)) && m_CharacterNames != null && m_GazeTargets != null)
        {
            GazeAt(m_CharacterNames[m_nCharacterSelection], m_GazeTargets[m_nGazeTargetSelection]);
        }

        if (m_PrevSelectedTransform != Selection.activeTransform)
        {
            m_PrevSelectedTransform = Selection.activeTransform;
        }
        if (!m_bBoneUpdates && m_CharacterNames != null && m_PrevSelectedTransform != null)
        {
            if (!m_JustToggledBoneUpdate)
            {
                m_JustToggledBoneUpdate = true;
                m_PrevTransformPos = m_PrevSelectedTransform.localPosition;
                m_PrevTransformRot = m_PrevSelectedTransform.rotation;
            }
            JointUpdate(m_PrevSelectedTransform, m_CharacterNames[m_nCharacterSelection]);
        }
        else
        {
            m_JustToggledBoneUpdate = false;
        }
    }

    void DrawBMLGUI()
    {
        // bml
        EditorGUILayout.LabelField("BML", EditorStyles.boldLabel);
        m_nSelectedFileIndex = EditorGUILayout.Popup("Files", m_nSelectedFileIndex, m_FileNames);
        if (GUILayout.Button("Use BML", GUILayout.MaxWidth(MaxButtonWidth)))
        {
            InvokeFile(m_CharacterNames[m_nCharacterSelection], m_FileNames[m_nSelectedFileIndex]);
        }
    }

    void DrawUtilsGUI()
    {
        EditorGUILayout.LabelField("Utils", EditorStyles.boldLabel);
        m_nFocusIndex = EditorGUILayout.Popup("", m_nFocusIndex, m_JointNames.ToArray());
        if (GUILayout.Button("Focus On", GUILayout.MaxWidth(MaxButtonWidth)) && m_JointNames.Count > 0)
        {
            FocusOn(m_CharacterNames[m_nCharacterSelection], m_JointNames[m_nFocusIndex]);
        }

        bool bPrevVal = m_bShowBones;
        GUILayout.BeginHorizontal();
        {
            m_bShowBones = GUILayout.Toggle(m_bShowBones, "Show Bones", GUILayout.Width(MaxToggleWidth));
            if (bPrevVal != m_bShowBones)
            {
                ToggleDebugFlag(UnitySmartbodyCharacter.DebugFlags.Show_Bones);
            }

            bPrevVal = m_bShowAxes;
            m_bShowAxes = GUILayout.Toggle(m_bShowAxes, "Show Axes", GUILayout.Width(MaxToggleWidth));
            if (bPrevVal != m_bShowAxes)
            {
                ToggleDebugFlag(UnitySmartbodyCharacter.DebugFlags.Show_Axes);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            bPrevVal = m_bShowEyeBeams;
            m_bShowEyeBeams = GUILayout.Toggle(m_bShowEyeBeams, "Show Eye Beams", GUILayout.Width(MaxToggleWidth));
            if (bPrevVal != m_bShowEyeBeams)
            {
                ToggleDebugFlag(UnitySmartbodyCharacter.DebugFlags.Show_Eye_Beams);
            }

            bPrevVal = m_bBoneUpdates;
            m_bBoneUpdates = GUILayout.Toggle(m_bBoneUpdates, "Bone Updates", GUILayout.Width(MaxToggleWidth));
            if (bPrevVal != m_bBoneUpdates)
            {
                SmartbodyManager smartbodyManager = GetSmartBodyManager();
                if (smartbodyManager != null)
                {
                    smartbodyManager.ReceiveBoneUpdates = m_bBoneUpdates;
                }

                SendVHMsg(string.Format("sbm receiver {0}", (m_bBoneUpdates ? "enable" : "disable")));
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        if (GUILayout.Button("Refresh", GUILayout.MaxWidth(MaxButtonWidth)))
        {
            SmartbodyManager smartbodyManager = GetSmartBodyManager();
            if (smartbodyManager)
            {
                smartbodyManager.RefreshPawns();
            }
        }
    }
    #endregion

    #region Utility Functions
    void SearchForAnim()
    {
        m_AnimationNames.Clear();
        if (string.IsNullOrEmpty(m_AnimSearchText))
        {
            // they aren't searching for anything so show all anims
            m_AnimationNames.AddRange(m_AnimationNamesBank);
            return;
        }

        // they typed new stuff
        m_nSelectedAnimIndex = 0;

        for (int i = 0; i < m_AnimationNamesBank.Count; i++)
        {
            Match m = Regex.Match(m_AnimationNamesBank[i], m_AnimSearchText, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                // matching expression so add it to the list
                m_AnimationNames.Add(m_AnimationNamesBank[i]);
            }
        }
    }

    List<string> GetCharacterJointNames(string character)
    {
        List<string> joints = new List<string>();
        if (string.IsNullOrEmpty(character))
        {
            return joints;
        }
        
        SmartbodyManager smartbodyManager = GetSmartBodyManager();

        UnitySmartbodyCharacter sbmChar = smartbodyManager.GetCharacterBySBMName(character);
        if (sbmChar == null)
        {
            Debug.LogError("No character named: " + character);
            return joints;
        }

        for (int i = 0; i < sbmChar.GetNumBones; i++)
        {
            Transform bone = sbmChar.GetBone(i);
            if (bone == null)
            {
                continue;
            }
            joints.Add(bone.name);
        }

        return joints;
    }

    void JointUpdate(Transform selectedTransform, string character)
    {
        //1) receiver echo <content>
        //2) receiver enable
        //3) receiver skeleton <skeletonName> <emitterType> position <joint-index/joint-name> <x> <y> <z>
        //4) receiver skeleton <skeletonName> <emitterType> positions <x1> <y1> <z1> <x2> <y2> <z2> ...
        //5) receiver skeleton <skeletonName> <emitterType> rotation <joint-index/joint-name> <q.w> <q.x> <q.y> <q.z>
        //6) receiver skeleton <skeletonName> <emitterType> rotations <q1.w> <q1.x> <q1.y> <q1.z> <q2.w> <q2.x> <q2.y> <q2.z>...

        if (selectedTransform == null)
        {
            return;
        }
        SmartbodyManager smartbodyManager = GetSmartBodyManager();

        UnitySmartbodyCharacter sbmChar = smartbodyManager.GetCharacterBySBMName(character);
        if (sbmChar.gameObject != selectedTransform.root.gameObject
            || selectedTransform.root.GetComponent<UnitySmartbodyCharacter>() == null)
        {
            // make sure they are selecting a gameobject that that belongs to their selected sbm character
            return;
        }

        if (m_PrevTransformPos != selectedTransform.localPosition)
        {
            Vector3 offset = selectedTransform.localPosition - GetSmartBodyManager().GetCharacterBySBMName(character).GetBaseBonePosition(selectedTransform.name);
            m_PrevTransformPos = selectedTransform.localPosition;

            Vector3 scaledPos = offset / smartbodyManager.PositionScaleHack;
            string msg = string.Format("sbm receiver skeleton {0} {1} position {2} {3} {4} {5}", character, "unity", selectedTransform.name,
                (-scaledPos.x).ToString("f4"), scaledPos.y.ToString("f4"), scaledPos.z.ToString("f4"));
            SendVHMsg(msg);
        }

        if (m_PrevTransformRot != selectedTransform.rotation)
        {
            m_PrevTransformRot = selectedTransform.rotation;
            Quaternion rot = selectedTransform.rotation;
            string msg = string.Format("sbm receiver skeleton {0} {1} rotation {2} {3} {4} {5} {6}", character, "unity", selectedTransform.name,
                rot.w, rot.x, -rot.y, -rot.z);
            SendVHMsg(msg);
        }
    }

    void ToggleDebugFlag(UnitySmartbodyCharacter.DebugFlags flag)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SmartbodyManager smartbodyManager = GetSmartBodyManager();
        if (smartbodyManager)
        {
            smartbodyManager.ToggleDebugFlag(flag);
        }
        else
        {
            Debug.LogError("You can't toggle Smartbody debug options if SmartbodyManager " +
                "or SmartbodyManagerBonebus is not in the scene");
        }
    }

    void FocusOn(string character, string jointName)
    {
        SmartbodyManager smartbodyManager = GetSmartBodyManager();
        UnitySmartbodyCharacter sbmChar = smartbodyManager.GetCharacterBySBMName(character);
        if (sbmChar == null)
        {
            Debug.Log("couldn't find " + character);
            return;
        }
        Selection.activeGameObject = sbmChar.GetBone(jointName).gameObject;
        SceneView.lastActiveSceneView.FrameSelected();
    }

    void SpawnEntity(string type, string name, Vector3 position)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        // we send different messages based on what we are trying to spawn
        string vhmsg = string.Empty;
        if ("Pawn" == type)
        {
            vhmsg = string.Format("unity pawn {0} init loc {1} {2} {3}", name, position.x.ToString("f2"),
                position.y.ToString("f2"), position.z.ToString("f2"));
        }
        else if ("Character" == type)
        {
            vhmsg = string.Format("sbm seq {0}", name);
        }

        SendVHMsg(vhmsg);
    }

    void PlayAnimation(string character, string animName)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SmartbodyManager smartbodyManager = GetSmartBodyManager();
        if (smartbodyManager)
        {
            if (WildCard == character)
            {
                // play this animation for all characters in the scene
                smartbodyManager.SBPlayAnim("*", animName);
            }
            else
            {
                smartbodyManager.SBPlayAnim(character, animName);
            }

        }
        else
        {
            Debug.LogError("You can't play animations if SmartbodyManager " +
                "or SmartbodyManagerBonebus is not in the scene");
        }
    }

    void SendSpeech(string speaker, string text)
    {
        //163152372   vrSpeech interp user0001 1 1.0 normal Hello. I am a virtual human
        string message = string.Format("sbm test bml character {0} recipient ALL speech \"{1}\"", speaker, text);
        SendVHMsg(message);
    }

    void GazeAt(string gazer, string gazeTarget)
    {
        string message = string.Format("sbm test bml character {0} gaze target {1}", gazer, gazeTarget);
        SendVHMsg(message);
    }

    void OnDestroy()
    {
        SaveLocation();
    }

    void SaveLocation()
    {
        if (ThisWindow != null)
        {
            PlayerPrefs.SetFloat(SavedWindowPosXKey, ThisWindow.position.x);
            PlayerPrefs.SetFloat(SavedWindowPosYKey, ThisWindow.position.y);
            PlayerPrefs.SetFloat(SavedWindowWKey, ThisWindow.position.width);
            PlayerPrefs.SetFloat(SavedWindowHKey, ThisWindow.position.height);
        }
    }

    void OnFocus()
    {
        SaveLocation();

        string[] animationNames = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Assets\\", "*.skm", SearchOption.AllDirectories);
        for (int i = 0; i < animationNames.Length; i++)
        {
            AddAnimationToList(Path.GetFileNameWithoutExtension(animationNames[i]));
        }

        m_FileNames = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Assets/", "*.xml", SearchOption.AllDirectories);
    }

    void Reset()
    {
        m_bShowBones = false;
        m_bShowAxes = false;
        m_bShowEyeBeams = false;
        m_bBoneUpdates = true;
    }

    string[] GetAllEntityNames()
    {
        int size = 0;
        if (m_CharacterNames != null)
            size += m_CharacterNames.Length;
        if (m_PawnNames != null)
            size += m_PawnNames.Length;

        string[] retVal = new string[size];
        if (m_CharacterNames != null)
            m_CharacterNames.CopyTo(retVal, 0);

        if (m_PawnNames != null)
            m_PawnNames.CopyTo(retVal, m_CharacterNames != null ? m_CharacterNames.Length : 0);

        return retVal;
    }

    string[] GetAllCharacterNames()
    {
        string[] retVal = new string[1] {""};
        SmartbodyManager sbm = GetSmartBodyManager();
        if (sbm)
        {
            string[] names = sbm.GetSBMCharacterNames();
            retVal = new string[names.Length + 1];
            names.CopyTo(retVal, 0);
            retVal[retVal.Length - 1] = WildCard;
        }

        return retVal;
    }

    string[] GetAllPawnNames()
    {
        string[] retVal = new string[1] { "" };
        SmartbodyManager sbm = GetSmartBodyManager();
        if (sbm)
        {
            retVal = sbm.GetPawnNames();
        }

        return retVal;
    }

    SmartbodyManager GetSmartBodyManager()
    {
        return (SmartbodyManager)Component.FindObjectOfType(typeof(SmartbodyManager));
    }

    void InvokeFile(string charname, string filename)
    {
        string vhmsg = string.Format("sbm bml char {0} file {1}", charname, filename);
        SendVHMsg(vhmsg);
    }

    void SendVHMsg(string vhmsg)
    {
        VHMsgBase.Get().SendVHMsg(vhmsg);
    }

    static void SetupVHMsgCallback()
    {
        VHMsgBase.Get().AddMessageEventHandler(new VHMsgBase.MessageEventHandler(SBMWindow.VHMsgCallback));
    }

    static void VHMsgCallback(object obj, VHMsgBase.Message message)
    {
        //Debug.Log("message.s: " + message.s);

        string[] splitargs = message.s.Split(" ".ToCharArray());

        if (splitargs[0] == "sbmlog" && splitargs[1] == "MotionFile" && splitargs.Length > 2)
        {
            AddAnimationToList(Path.GetFileNameWithoutExtension(splitargs[splitargs.Length - 1]));
        }
    }

    static void AddAnimationToList(string animName)
    {
        m_AnimationNames.Add(animName);
        m_AnimationNamesBank.Add(animName);
    }
    #endregion
    #endregion
}
