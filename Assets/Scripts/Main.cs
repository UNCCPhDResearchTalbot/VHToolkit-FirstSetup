using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;

public class Main : VHMain
{
    public bool m_displayVhmsgLog = false;
    public FreeMouseLook m_camera;
    public SmartbodyManager m_sbm;
    public Texture2D m_whiteTexture;
    public BMLEventHandler m_BMLEventHandler;


    Vector3 m_StartingCameraPosition;  // used for camera reset
    Quaternion m_StartingCameraRotation;

    private bool m_showMenu = false;

    private bool m_showController = false;
    bool m_walkToMode = false;
    Vector3 m_walkToPoint;
    bool m_reachMode = false;
    private float m_timeSlider = 1.0f;

    private bool m_gazingAtMouse = false;

    protected int m_debugTextMode = 0;   // 0 - off, 1 - fps/camera, 2 - systeminfo

    private string [] m_controllerMenuText = { "sb-voice", "sb-motion", "sb-controller", "sb-bonebus" };
    private int m_controllerMenuSelected = 0;

    private string [] testUtteranceButtonText = { "1", "Tts", "V" };
    private string [] testUtteranceName = { "brad_fullname", "speech_womanTTS", "z_viseme_test2" };
    private string [] testUtteranceText = { "", "If the system cannot find my regular voice, it defaults back to the Windows standard voice. Depending on your version of Windows that can be a woman's voice. Don't I sound delightful?", "" };  // the TTS text
    private int testUtteranceSelected = 0;
    private string [] testTtsVoices = { "Festival_voice_rab_diphone", "Festival_voice_kal_diphone", "Festival_voice_ked_diphone", "MicrosoftAnna", "MicrosoftMary", "MicrosoftMike" };
    private int testTtsSelected = 0;
    private string [] testAnimButtonText = { "1", "2", "3" };
    private string [] testAnimName = { "ChrBrad@Idle01_ChopBoth01", "ChrBrad@Idle01_IndicateLeftLf01", "ChrBrad@Idle01_IndicateRightRt01" };
    private int testAnimSelected = 0;

    private float m_gazeOffsetValueVertical   = 0;
    private float m_gazeOffsetValueHorizontal = 0;

    private float m_nodNumber = 2;
    private float m_nodTime   = 1;

    int m_bmlId = 42;
    string m_vhmsgServer = "cedros";


    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        //Debug.Log("Main.Start()");

        Application.targetFrameRate = 60;
        base.Start();

        DisplaySubtitles = true;
        DisplayUserDialog = true;

        m_StartingCameraPosition = m_camera.transform.position;
        m_StartingCameraRotation = m_camera.transform.rotation;

        /*
        m_bUsingBoneBus = bool.Parse(m_ConfigFile.GetSetting("general", "UseBoneBus"));
        if (m_bUsingBoneBus)
        {
            Debug.Log("Using BoneBus");
            m_SBMBoneBus.gameObject.active = true;
            m_SBMBoneBus.Activate();
            m_SBMBoneBus.DisplaySubtitles = true;
            //m_SBMBoneBus.RefreshPawns();
        }
        else
        */

        m_Console = DebugConsole.Get();

        m_sbm = SmartbodyManager.Get();

        if (m_sbm)
        {
            m_sbm.AddCustomCharCreateCB(new SmartbodyManager.OnCustomCharacterCallback(OnCharacterCreate));
            m_sbm.AddCustomCharDeleteCB(new SmartbodyManager.OnCustomCharacterCallback(OnCharacterDelete));
        }

        SubscribeVHMsg();


        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (!m_Console.DrawConsole) m_Console.ToggleConsole();
            m_showController = true;
        }


        if (Application.loadedLevelName == "vhAssetsTestSceneBrown")
        {
            testAnimName[0] = "ChrBrownRoc@Idle01_AppreciateBody01";
            testAnimName[1] = "ChrBrownRoc@Idle01_HeadNodEnthusiastic01";
            testAnimName[2] = "ChrBrownRoc@Idle01_ThumbsUp01";
        }
    }

    void SubscribeVHMsg()
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg)
        {
            vhmsg.SubscribeMessage("vrAllCall");
            vhmsg.SubscribeMessage("vrKillComponent");
            vhmsg.SubscribeMessage("vrExpress");
            vhmsg.SubscribeMessage("vrSpeak");
            vhmsg.SubscribeMessage("vrSpoke");
            vhmsg.SubscribeMessage("CommAPI");
            vhmsg.SubscribeMessage("acquireSpeech");
            vhmsg.SubscribeMessage("PlaySound");
            vhmsg.SubscribeMessage("StopSound");
            vhmsg.SubscribeMessage("renderer");

            vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));

            vhmsg.SendVHMsg("vrComponent renderer");
        }
    }

    public void Update()
    {
#if false
        if (Application.isLoadingLevel)
            return;
#endif

        if (m_sbm)
        {
            m_sbm.m_camPos = m_camera.transform.position;
            m_sbm.m_camRot = m_camera.transform.rotation;
            m_sbm.m_camFovY = m_camera.camera.fieldOfView;
            m_sbm.m_camAspect = m_camera.camera.aspect;
            m_sbm.m_camZNear = m_camera.camera.nearClipPlane;
            m_sbm.m_camZFar = m_camera.camera.farClipPlane;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            MecanimCharacter mecAnimCharacter = GameObject.Find("BradM").GetComponent<MecanimCharacter>();
            mecAnimCharacter.PlayAudio("brad_fullname");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Application.Quit();

            if (m_showMenu)
            {
                m_showMenu = false;
                Time.timeScale = m_timeSlider;
            }
            else
            {
                m_showMenu = true;
                Time.timeScale = 0;
            }
        }

        m_camera.enabled = !m_Console.DrawConsole;

        if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ||
           !m_Console.DrawConsole) // they aren't typing in a box
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //TestBMLParser();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //selectedChar.Gesture(m_AnimationNames[m_SelectedAnim]); 
                //string[] m_AnimationNames = { "ChrBrad@Idle01_ScratchHeadLf01", "ChrBrad@Idle01_PointLf01", "ChrBrad@Idle01_ChopBoth01" };
                //m_CharacterControllers.ForEach(item => item.SBPlayAnim(m_SelectedCharacterName, m_AnimationNames[m_SelectedAnim]));
                GameObject.Find("BradM").GetComponent<MecanimCharacter>().PlayAnim("ChrBrad@Idle01_ScratchHeadLf01");
            }


            if (Input.GetKeyDown(KeyCode.C))
            {
                m_showController = !m_showController;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleAxisLines();
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                // reset camera position
                m_camera.transform.position = m_StartingCameraPosition;
                m_camera.transform.rotation = m_StartingCameraRotation;
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                // toggle debug text
                m_debugTextMode++;
                m_debugTextMode = m_debugTextMode % 3;
            }


            if (m_walkToMode)
            {
                // walk to mouse position

                bool doRaycast;
                Vector3 position = Vector3.zero;
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    doRaycast = Input.touches.Length > 0;
                    if (doRaycast)
                        position = Input.touches[0].position;
                }
                else
                {
                    doRaycast = Input.GetMouseButtonDown(0);
                    position = Input.mousePosition;
                }

                if (doRaycast)
                {
                    Ray ray = m_camera.camera.ScreenPointToRay(position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log("Walk to: " + -hit.point.x + " " + hit.point.z);
                        SmartbodyManager.Get().SBWalkTo("*", string.Format("{0} {1}", -hit.point.x, hit.point.z), false);
                        m_walkToPoint = hit.point;
                    }
                }
            }


            if (m_reachMode)
            {
                bool doReach;
                Vector3 position = Vector3.zero;
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    doReach = Input.touches.Length > 0;
                    if (doReach)
                        position = Input.touches[0].position;
                }
                else
                {
                    doReach = Input.GetMouseButtonDown(0);
                    position = Input.mousePosition;
                }

                if (doReach)
                {
                    bool found = false;
                    Ray ray = m_camera.camera.ScreenPointToRay(position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.GetComponent<SmartbodyPawn>() != null)
                        {
                            string cubeName = hit.collider.gameObject.name;
                            string rightLeft = UnityEngine.Random.Range(0, 2) > 0 ? "right" : "left";

                            m_sbm.PythonCommand(string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-range=""neck eyes"" />')", "Brad", cubeName));
                            m_sbm.PythonCommand(string.Format(@"bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" sbm:action=""touch"" sbm:reach-type=""{1}"" target=""{2}"" />')", "Brad", rightLeft, cubeName));

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        m_sbm.PythonCommand(string.Format(@"bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" sbm:action=""touch"" sbm:reach-finish=""true"" />')", "Brad"));
                        m_sbm.PythonCommand(string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-range=""neck eyes"" />')", "Brad", "Camera"));
                    }
                }
            }
        }


        if (m_gazingAtMouse)
        {
            if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ||
               !m_Console.DrawConsole) // they aren't typing in a box
            {
                bool doGaze;
                Vector3 position = Vector3.zero;
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    doGaze = Input.touches.Length > 0;
                    if (doGaze)
                        position = Input.touches[0].position;
                }
                else
                {
                    doGaze = true;
                    position = Input.mousePosition;
                }

                if (doGaze)
                {
                    Ray ray = m_camera.camera.ScreenPointToRay(position);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject mousePawn = GameObject.Find("MousePawn");
                        mousePawn.transform.position = hit.point;
                    }
                    else
                    {
                        GameObject mousePawn = GameObject.Find("MousePawn");
                        mousePawn.transform.position = m_camera.camera.transform.position;
                    }
                }
            }
        }


        if (!m_showMenu)
        {
            // lock the screen cursor if they are looking around or using their mic
            Screen.lockCursor = m_camera.CameraRotationOn;
        }
    }

    int bml1Index = 0;
    int bml3Index = 0;

    public override void OnGUI()
    {
        base.OnGUI();

        if (m_showMenu)
        {
            Rect r = new Rect(0.25f, 0.2f, 0.5f, 0.6f);
            GUILayout.BeginArea(VHGUI.ScaleToRes(ref r));
            GUILayout.BeginVertical();

            if (GUILayout.Button("Main Menu"))
            {
                m_showMenu = false;
                Time.timeScale = m_timeSlider;

                m_sbm.RemoveAllSBObjects();

                Application.LoadLevel("MainMenu");
            }

            GUILayout.Space(40);

            if (GUILayout.Button("Exit"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.ExecuteMenuItem( "Edit/Play" );
#else
                Application.Quit();
#endif
            }

            GUILayout.Space(40);

            if (GUILayout.Button("Return to Game"))
            {
                m_showMenu = false;
                Time.timeScale = m_timeSlider;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        if (m_showController)
        {
            float buttonX = 0;
            float buttonY = 0;
            float buttonH;
            float buttonW = 120;
            float spaceHeight = 30;

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
                buttonH = 70;
            else
                buttonH = 20;

            GUILayout.BeginArea(new Rect (buttonX, buttonY, buttonW, Screen.height));
            GUILayout.BeginVertical();

            if (GUILayout.Button(m_controllerMenuText[m_controllerMenuSelected], GUILayout.Height(buttonH)))
            {
                m_controllerMenuSelected++;
                m_controllerMenuSelected = m_controllerMenuSelected % m_controllerMenuText.Length;
            }

            GUILayout.Space(spaceHeight);

            if (m_controllerMenuSelected == 0) // sb-voice
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(testUtteranceButtonText[testUtteranceSelected], GUILayout.Height(buttonH)))
                {
                    testUtteranceSelected++;
                    testUtteranceSelected = testUtteranceSelected % testUtteranceButtonText.Length;
                }
                if (GUILayout.Button("Test Utt", GUILayout.Height(buttonH)))    { m_sbm.SBPlayAudio("Brad", testUtteranceName[testUtteranceSelected], testUtteranceText[testUtteranceSelected]); MobilePlayAudio(testUtteranceName[testUtteranceSelected]); }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(testTtsVoices[testTtsSelected], GUILayout.Height(buttonH)))
                {
                    testTtsSelected++;
                    testTtsSelected = testTtsSelected % testTtsVoices.Length;

                    m_sbm.PythonCommand(string.Format(@"scene.command('set character {0} voicebackup remote {1}')", "Brad", testTtsVoices[testTtsSelected]));
                }

                if (GUILayout.Button("Play Cutscene", GUILayout.Height(buttonH)))
                {
                    GameObject.Find("Cutscene01").GetComponent<Cutscene>().Play();
                }
            }
            else if (m_controllerMenuSelected == 1) // sb-motion
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(testAnimButtonText[testAnimSelected], GUILayout.Height(buttonH)))
                {
                    testAnimSelected++;
                    testAnimSelected = testAnimSelected % testAnimButtonText.Length;
                }

                if (GUILayout.Button("PlayAnim", GUILayout.Height(buttonH)))
                {
                    string c = "Brad";

                    if (Application.loadedLevelName == "vhAssetsTestSceneBrown")
                    {
                        c = "ChrBrownRoc";
                    }

                    m_sbm.SBPlayAnim(c, testAnimName[testAnimSelected]);
                }
                GUILayout.EndHorizontal();
            }
            else if (m_controllerMenuSelected == 2) // sb-controller
            {
                m_walkToMode = GUILayout.Toggle(m_walkToMode, "WalkToMode");

                if (GUILayout.Button("Stop Walking"))
                {
                    string message = string.Format(@"bml.execBML('{0}', '<locomotion enable=""{1}"" />')", "*", "false");
                    SmartbodyManager.Get().PythonCommand(message);
                }

                bool reachMode = GUILayout.Toggle(m_reachMode, "ReachMode");
                if (reachMode != m_reachMode)
                {
                    m_reachMode = reachMode;
                    GameObject reachObjects = GameObject.Find("ReachObjects");

                    Transform[] allChildren = reachObjects.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in allChildren)
                    {
                        if (t == reachObjects.transform)
                            continue;

                        t.gameObject.SetActive(m_reachMode);
                    }
                }

                if (GUILayout.Button("Gaze Camera", GUILayout.Height(buttonH)))
                {
                    m_gazingAtMouse = false;
                    m_sbm.SBGaze("Brad", "Camera");
                }

                if (GUILayout.Button("Gaze Mouse", GUILayout.Height(buttonH)))
                {
                    m_gazingAtMouse = true;
                    m_sbm.SBGaze("Brad", "MousePawn");
                }

                if (GUILayout.Button("Gaze Off", GUILayout.Height(buttonH)))
                {
                    m_gazingAtMouse = false;
                    m_sbm.PythonCommand(string.Format(@"scene.command('char {0} gazefade out 1')", "Brad"));
                }

                GUILayout.Space(20);

                GUILayout.Label(string.Format("Gaze offset - {0} {1}", (int)m_gazeOffsetValueVertical, (int)m_gazeOffsetValueHorizontal));
                GUILayout.BeginHorizontal();
                //GUILayout.Space(buttonW / 2);
                //float gazeOffsetVTemp = GUILayout.VerticalSlider(m_gazeOffsetValueVertical, 90, -90);
                GUILayout.EndHorizontal();
                float gazeOffsetHTemp = GUILayout.HorizontalSlider(m_gazeOffsetValueHorizontal, -90, 90);

                if (//gazeOffsetVTemp != m_gazeOffsetValueVertical ||
                    gazeOffsetHTemp != m_gazeOffsetValueHorizontal)
                {
                    //m_gazeOffsetValueVertical   = gazeOffsetVTemp;
                    m_gazeOffsetValueHorizontal = gazeOffsetHTemp;

                    string message = string.Format(@"bml.execBML('{0}', '<gaze target=""{1}"" direction=""{2}"" angle=""{3}"" />')", "Brad", "Camera", m_gazeOffsetValueHorizontal > 0 ? "LEFT" : "RIGHT", Math.Abs(m_gazeOffsetValueHorizontal));
                    m_sbm.PythonCommand(message);
                }

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("n:{0:F1}", m_nodNumber), GUILayout.Width(40));
                m_nodNumber = GUILayout.HorizontalSlider(m_nodNumber, 0, 5);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("t:{0:F1}", m_nodTime), GUILayout.Width(40));
                m_nodTime = GUILayout.HorizontalSlider(m_nodTime, 0, 5);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Nod", GUILayout.Height(buttonH)))
                {
                    string message = string.Format(@"bml.execBML('{0}', '<head type=""{1}"" repeats=""{2}"" start=""0"" end=""{3}"" />')", "Brad", "NOD", m_nodNumber, m_nodTime);
                    m_sbm.PythonCommand(message);
                }

                if (GUILayout.Button("Shake", GUILayout.Height(buttonH)))
                {
                    string message = string.Format(@"bml.execBML('{0}', '<head type=""{1}"" repeats=""{2}"" start=""0"" end=""{3}"" />')", "Brad", "SHAKE", m_nodNumber, m_nodTime);
                    m_sbm.PythonCommand(message);
                }
            }
            else if (m_controllerMenuSelected == 3) // sb-bonebus
            {
                if (GUILayout.Button("Launch SB Bonebus"))
                {
#if !UNITY_WEBPLAYER
                    // disable internal SmartbodyManager and enable SmartbodyManagerBoneBus
                    if (!(SmartbodyManager.Get() is SmartbodyManagerBoneBus))
                        SmartbodyManager.Get().gameObject.SetActive(false);
                    if (SmartbodyManagerBoneBus.GetDisabled())
                        SmartbodyManagerBoneBus.GetDisabled().gameObject.SetActive(true);

                    // reset the static global, so that the bonebus version will be returned
                    SmartbodyManager.ResetGet();
                    m_sbm = SmartbodyManagerBoneBus.Get();

                    // start external smartbody process
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = Application.dataPath + "/../" + "runSbBonebus.bat";
                    startInfo.WorkingDirectory = Application.dataPath + "/../";
                    System.Diagnostics.Process.Start( startInfo );
#endif
                }

                if (GUILayout.Button("Init Scene"))
                {
                    if (Application.isEditor)
                    {
                        m_sbm.PythonCommand(@"scene.setMediaPath('../../../../lib/vhunity/vhAssets')");  // base path prepended to sound, seq file, and motion paths
                        m_sbm.PythonCommand(@"scene.command('path audio .')");  // this is still required because of a bug in sbm path handling with sounds
                    }
                    else
                    {
                        m_sbm.PythonCommand(@"scene.setMediaPath('../../../../bin/vhAssets')");  // base path prepended to sound, seq file, and motion paths
                        m_sbm.PythonCommand(@"scene.command('path audio .')");  // this is still required because of a bug in sbm path handling with sounds
                    }

                    // refresh all smartbody data so the bonebus version is up-to-date
                    m_sbm.RefreshInit();
                    m_sbm.RefreshPawns();
                    m_sbm.RefreshCharacters();
                }
            }

            GUILayout.Space(spaceHeight);

            if (m_sbm) m_sbm.m_displayLogMessages = GUILayout.Toggle(m_sbm.m_displayLogMessages, "SBMLog");
            m_displayVhmsgLog = GUILayout.Toggle(m_displayVhmsgLog, "VHMsgLog");
            m_timeSlider = GUILayout.HorizontalSlider(m_timeSlider, 0.01f, 3);
            GUILayout.Label(string.Format("Time: {0}", m_timeSlider));

            if (!m_showMenu)
            {
                Time.timeScale = m_timeSlider;
            }

            string character1 = "Brad";
            string character2 = "BradM";

            if (GUILayout.Button("BML1"))
            {
#if false
                vrSpeak Brad ALL sbm_test_bml_7
                <?xml version="1.0" encoding="UTF-8"?>
                <act>
                    <bml>
                        <animation name="ChrBrad@Idle01_ChopBoth01" start="1.0" />
                    </bml>
                </act>


                vrSpeak Brad ALL sbm_test_bml_9
                <?xml version="1.0" encoding="UTF-8"?>
                <act>
                    <bml>
                        <head type="NOD" repeats="2" start="0" end="2" />
                    </bml>
                </act>

                vrSpeak Brad ALL sbm_test_bml_10
                <?xml version="1.0" encoding="UTF-8"?>
                <act>
                    <participant id="Brad" role="actor"/>
                    <bml>
                        <speech id="sp1" ref="helloworld" type="application/ssml+xml">Hello world</speech>
                    </bml>
                </act>
#endif

                
                string bmlIdString = "renderer_bml_" + m_bmlId++;
                string xml =    @"<?xml version=""1.0"" encoding=""UTF-8""?>" + 
                                @"<act>" + 
                                @"<bml>" + 
                                @"<animation name=""ChrBrad@Idle01_ChopBoth01"" start=""1.0"" />" + 
                                @"</bml>" + 
                                @"</act>";

                string [] anims = new string [] {
                    "ChrBrad@Idle01_ArmStretch01", 
                    "ChrBrad@Idle01_ChopLf01", 
                    "ChrBrad@Idle01_Contemplate01", 
                    "ChrBrad@Idle01_ExampleLf01", 
                    "ChrBrad@Idle01_IndicateRightRt01", 
                    "ChrBrad@Idle01_MeLf01", 
                    "ChrBrad@Idle01_NegativeBt01", 
                    "ChrBrad@Idle01_NegativeRt01", 
                    "ChrBrad@Idle01_OfferBoth01", 
                    "ChrBrad@Idle01_PleaBt02", 
                    "ChrBrad@Idle01_ScratchChest01", 
                    "ChrBrad@Idle01_ScratchHeadLf01", 
                    "ChrBrad@Idle01_ScratchTempleLf01", 
                    "ChrBrad@Idle01_ShoulderStretch01", 
                    "ChrBrad@Idle01_TouchHands01", 
                    "ChrBrad@Idle01_WeightShift01", 
                    "ChrBrad@Idle01_WeightShift02", 
                    "ChrBrad@Idle01_YouLf01" };

                xml = xml.Replace("ChrBrad@Idle01_ChopBoth01", anims[bml1Index]);
                bml1Index = (++bml1Index) % anims.Length;


                VHMsgBase vhmsg = VHMsgBase.Get();
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character1, bmlIdString, xml));
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
            }

            if (GUILayout.Button("BML2"))
            {
                string bmlIdString = "renderer_bml_" + m_bmlId++;
                string xml =    @"<?xml version=""1.0"" encoding=""UTF-8""?>" + 
                                @"<act>" + 
                                @"<bml>" + 
                                @"<head type=""NOD"" repeats=""2"" start=""0"" end=""2"" />" + 
                                @"</bml>" + 
                                @"</act>";

                VHMsgBase vhmsg = VHMsgBase.Get();

                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character1, bmlIdString, xml));
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
            }

            if (GUILayout.Button("BML3"))
            {
                string bmlIdString = "renderer_bml_" + m_bmlId++;
                string xml =    @"<?xml version=""1.0"" encoding=""UTF-8""?>" + 
                                @"<act>" + 
                                @"<participant id=""Brad"" role=""actor""/>" + 
                                @"<bml>" + 
                                @"<speech id=""sp1"" ref=""brad_fullname"" type=""application/ssml+xml"">My full name is brad mathew smith</speech>" + 
                                @"</bml>" + 
                                @"</act>";

                string [] sounds = new string [] { "brad_fullname", "brad_age", "brad_alwayscertain", "brad_alwayssure", "brad_answeredthat" };

                xml = xml.Replace("brad_fullname", sounds[bml3Index]);
                bml3Index = (++bml3Index) % 5;
/*
brad_age.wav
brad_alwayscertain.wav
brad_alwayssure.wav
brad_answeredthat.wav
brad_anythingelse.wav
brad_anytime.wav
brad_askmeabout.wav
brad_astronaut.wav
brad_basicapplied.wav
brad_basicexamples.wav
brad_basictts.wav
brad_benice.wav
brad_boringyou.wav
brad_brains.wav
brad_byebye.wav
brad_byte.wav
brad_campus.wav
brad_cannotanswer.wav
brad_cansayagain.wav
brad_cantsee.wav
brad_checkouturl.wav
brad_classifier.wav
brad_congrats.wav
brad_couldsayagain.wav
brad_dancing.wav
brad_dontdrink.wav
brad_donteither.wav
brad_dontknow.wav
brad_dowith.wav
brad_excuseme.wav
brad_fighton.wav
brad_fromuscict.wav
brad_fullname.wav
brad_good.wav
brad_goodbadside.wav
brad_goodbye.wav
brad_goodmorning.wav
brad_goodtosee.wav
brad_goodyou.wav
brad_goonlineurl.wav
brad_greathelp.wav
brad_hanging.wav
brad_haveagoodday.wav
brad_height.wav
brad_hello.wav
brad_hey.wav
brad_hi.wav
brad_hibrad.wav
brad_highlevel.wav
brad_hungry.wav
brad_ictoverview.wav
brad_ilikeyou.wav
brad_imbrad.wav
brad_isaid.wav
brad_keyboard.wav
brad_knowtoo.wav
brad_laughoncue.wav
brad_letfinish.wav
brad_license.wav
brad_likegoingthere.wav
brad_likehouse.wav
brad_liketoknow.wav
brad_littlesleep.wav
brad_lookbasic.wav
brad_makemewalk.wav
brad_misunderstood.wav
brad_moreinfo.wav
brad_myhouse.wav
brad_nada.wav
brad_name.wav
brad_nicemeet.wav
brad_nicetosay.wav
brad_night.wav
brad_nobodyknows.wav
brad_nocomment.wav
brad_nope.wav
brad_notthirtyone.wav
brad_noworries.wav
brad_npceditor.wav
brad_nvbg.wav
brad_nvbganalyzes.wav
brad_ogre.wav
brad_okface.wav
brad_older.wav
brad_onemoretime.wav
brad_onlineurl.wav
brad_opensource.wav
brad_otherquestions.wav
brad_ourselves.wav
brad_painting.wav
brad_perez.wav
brad_pictures.wav
brad_pictureswall.wav
brad_pocketsphinx.wav
brad_prettycool.wav
brad_prettyneat.wav
brad_programmed.wav
brad_quitewell.wav
brad_rachelhere.wav
brad_randy.wav
brad_repeat.wav
brad_repeatthat.wav
brad_researchers.wav
brad_retargeting.wav
brad_rtay.wav
brad_sapi.wav
brad_sapirightway.wav
brad_sblocomotion.wav
brad_sbscheduled.wav
brad_seeya.wav
brad_sgtstar.wav
brad_shopping.wav
brad_showoff.wav
brad_shutup.wav
brad_skindeep.wav
brad_socal.wav
brad_sodefensive.wav
brad_somethingelse.wav
brad_sorrycatch.wav
brad_sorryunderstand.wav
brad_sounddelightful.wav
brad_speechmodels.wav
brad_statue.wav
brad_sunsets.wav
brad_sureido.wav
brad_takecare.wav
brad_talktoolkit.wav
brad_textbooks.wav
brad_thanks.wav
brad_thatsucks.wav
brad_thirtyone.wav
brad_thoughttold.wav
brad_threeofus.wav
brad_tml.wav
brad_trojan.wav
brad_tweaking.wav
brad_twentiethcentury.wav
brad_uarc.wav
brad_unitydefault.wav
brad_unityogre.wav
brad_uscedu.wav
brad_uscict.wav
brad_verywelcome.wav
brad_visitonline.wav
brad_weathersgreat.wav
brad_webcam.wav
brad_websiteurl.wav
brad_well.wav
brad_willsmith.wav
brad_wow.wav
brad_yourewelcome.wav
brad_youtoo.wav
*/

                VHMsgBase vhmsg = VHMsgBase.Get();

                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character1, bmlIdString, xml));
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
            }

            if (GUILayout.Button("BML4"))
            {
                string bmlIdString = "renderer_bml_" + m_bmlId++;
                string xml =    @"<?xml version=""1.0"" encoding=""UTF-8""?>" + 
                                @"<act>" + 
                                @"<participant id=""Brad"" role=""actor"" />" + 
                                @"<bml>" + 
                                @"<speech id=""sp1"" ref=""brad_fullname"" type=""application/ssml+xml"">" + 
                                @"<mark name=""T0"" />My" + 
                                @"<mark name=""T1"" /><mark name=""T2"" />full" + 
                                @"<mark name=""T3"" /><mark name=""T4"" />name" + 
                                @"<mark name=""T5"" /><mark name=""T6"" />is" + 
                                @"<mark name=""T7"" /><mark name=""T8"" />Brad" + 
                                @"<mark name=""T9"" /><mark name=""T10"" />Matthew" + 
                                @"<mark name=""T11"" /><mark name=""T12"" />Smith." + 
                                @"<mark name=""T13"" />" + 
                                @"</speech>" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T1 My "" stroke=""sp1:T1"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T3 My full "" stroke=""sp1:T3"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T5 My full name "" stroke=""sp1:T5"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T7 My full name is "" stroke=""sp1:T7"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T9 My full name is Brad "" stroke=""sp1:T9"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T11 My full name is Brad Matthew "" stroke=""sp1:T11"" />" + 
                                @"<event message=""vrAgentSpeech partial 1366842060442-9-1 T13 My full name is Brad Matthew Smith. "" stroke=""sp1:T13"" />" +
                                @"<gaze participant=""Brad"" id =""gaze"" target=""Camera"" direction=""UPLEFT"" angle=""0"" sbm:joint-range=""EYES HEAD"" xmlns:sbm=""http://ict.usc.edu"" />" + 
                                @"<event message=""vrSpoke Brad all 1366842060442-9-1 My full name is Brad Matthew Smith"" stroke=""sp1:relax"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" xmlns:sbm=""http://ict.usc.edu"" />" + 
                                @"<!--Inclusivity-->" +
                                @"<head type=""SHAKE"" amount=""0.4"" repeats=""1.0"" velocity=""1"" start=""sp1:T4"" priority=""4"" duration=""1"" />" + 
                                @"<!--Noun clause nod-->" +
                                @"<head type=""NOD"" amount=""0.20"" repeats=""1.0"" start=""sp1:T9"" priority=""5"" duration=""1"" />" +
                                @"<animation name=""ChrBrad@Idle01_Contemplate01"" start=""sp1:T8"" />" + 
                                @"</bml>" + 
                                @"</act>";

                VHMsgBase vhmsg = VHMsgBase.Get();
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character1, bmlIdString, xml));
                vhmsg.SendVHMsg(string.Format("vrSpeak {0} ALL {1} {2}", character2, bmlIdString, xml));
            }


            if (Application.loadedLevelName == "mecanimWeb")
            {
                m_vhmsgServer = GUILayout.TextField(m_vhmsgServer, 256);

                if (Network.connections.Length > 0)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        Debug.Log("Calling Network.Disconnect()");
                        Network.Disconnect();
                    }
                }
                else
                {
                    if (GUILayout.Button("Connect"))
                    {
                        Debug.Log("Calling Network.Connect()");
                        NetworkConnectionError error = Network.Connect(m_vhmsgServer, 25000);
                        Debug.Log(error.ToString());


                        VHMsgBase m_vhmsg = GameObject.Find("VHMsgEmulator").GetComponent<VHMsgEmulator>();

                        m_vhmsg.SubscribeMessage("vrAllCall");
                        m_vhmsg.SubscribeMessage("vrKillComponent");
                        //m_vhmsg.SubscribeMessage("vrExpress");
                        m_vhmsg.SubscribeMessage("vrSpeak");
                        //m_vhmsg.SubscribeMessage("vrSpoke");
                        //m_vhmsg.SubscribeMessage("CommAPI");

                        m_vhmsg.SubscribeMessage("PlaySound");
                        m_vhmsg.SubscribeMessage("StopSound");
                        //m_vhmsg.SubscribeMessage("ToggleObjectVisibility");
                        //m_vhmsg.SubscribeMessage("wsp");

                        // sbm related vhmsgs
                        //m_vhmsg.SubscribeMessage("sbm");
                        //m_vhmsg.SubscribeMessage("vrAgentBML");
                        //m_vhmsg.SubscribeMessage("vrSpeak");
                        //m_vhmsg.SubscribeMessage("RemoteSpeechReply");
                        //m_vhmsg.SubscribeMessage("StopSound");
                        //m_vhmsg.SubscribeMessage("object-data");

                        m_vhmsg.AddMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));
                        m_vhmsg.SendVHMsg("vrComponent renderer");
                    }
                }
            }


            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                if (GUILayout.Button("Assets"))
                {
                    m_sbm.PythonCommand(@"scene.command('resource path')");
                    m_sbm.PythonCommand(@"scene.command('resource motion')");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }


        if (m_walkToMode)
        {
            Vector3 screenPoint = m_camera.gameObject.camera.WorldToScreenPoint(m_walkToPoint);

            GUI.color = new Color(1, 0, 0, 1);
            float boxH = 10;
            float boxW = 10;
            Rect r = new Rect(screenPoint.x - (boxW / 2), (m_camera.gameObject.camera.pixelHeight - screenPoint.y) - (boxH / 2), boxW, boxH);
            GUI.DrawTexture(r, m_whiteTexture);
            GUI.color = Color.white;
        }


        if (m_debugTextMode == 1)
        {
            float fps = 0;
            float averageFps = 0;
            FpsCounter fpsCounter = GetComponent<FpsCounter>();
            if (fpsCounter)
            {
                fps = fpsCounter.Fps;
                averageFps = fpsCounter.AverageFps;
            }

            GUILayout.BeginArea(new Rect (0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            VHGUILayout.Label(string.Format("T: {0:f2} F: {1} AVG: {2:f0} FPS: {3:f2}", Time.time, Time.frameCount, averageFps, fps), new Color(1, Math.Min( 1.0f, averageFps / 30 ), Math.Min( 1.0f, averageFps / 30 )));
            GUILayout.Label(string.Format("{0}x{1}x{2} ({3})", Screen.width, Screen.height, Screen.currentResolution.refreshRate, Utils.GetCommonAspectText((float)Screen.width / Screen.height)));
            GUILayout.Label(string.Format("{0}", Application.loadedLevelName));

            Transform camTrans = m_camera.transform;
            GUILayout.Label(string.Format("Cam Pos ({0}): {1:f2} {2:f2} {3:f2}", m_camera.name, camTrans.position.x, camTrans.position.y, camTrans.position.z));
            GUILayout.Label(string.Format("Cam Rot (xyz): {0:f2} {1:f2} {2:f2}", camTrans.rotation.eulerAngles.x, camTrans.rotation.y, camTrans.rotation.z));

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else if (m_debugTextMode == 2)
        {
            GUILayout.BeginArea(new Rect (0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            GUILayout.Label(SystemInfo.operatingSystem);  // Operating system name with version (Read Only).
            GUILayout.Label(string.Format("{0} x {1}", SystemInfo.processorCount, SystemInfo.processorType));  // Processor name (Read Only).
            GUILayout.Label(string.Format("Mem: {0:f1}gb", SystemInfo.systemMemorySize / 1000.0f));  // Amount of system memory present (Read Only).
            GUILayout.Label(SystemInfo.graphicsDeviceName);  // The name of the graphics device (Read Only).
            GUILayout.Label(SystemInfo.graphicsDeviceVersion);  // The graphics API version supported by the graphics device (Read Only).
            GUILayout.Label(string.Format("VMem: {0}mb", SystemInfo.graphicsMemorySize));  // Amount of video memory present (Read Only).
            GUILayout.Label(string.Format("Shader Level: {0:f1}", SystemInfo.graphicsShaderLevel / 10.0f));  // Graphics device shader capability level (Read Only).
            GUILayout.Label(string.Format("Fillrate: {0}", SystemInfo.graphicsPixelFillrate.ToString()));  // Approximate pixel fill-rate of the graphics device (Read Only).
            GUILayout.Label(string.Format("Shadows:{0} RT:{1} FX:{2}", SystemInfo.supportsShadows ? "y" : "n", SystemInfo.supportsRenderTextures ? "y" : "n", SystemInfo.supportsImageEffects ? "y" : "n"));  // Are built-in shadows supported? (Read Only)
            GUILayout.Label(string.Format("deviceUniqueIdentifier: {0}", SystemInfo.deviceUniqueIdentifier));  // A unique device identifier. It is guaranteed to be unique for every
            GUILayout.Label(string.Format("deviceName: {0}", SystemInfo.deviceName));  // The user defined name of the device (Read Only).
            GUILayout.Label(string.Format("deviceModel: {0}", SystemInfo.deviceModel));  // The model of the device (Read Only).

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }

    void OnDestroy()
    {
        VHMsgBase vhmsg = VHMsgBase.Get();
        if (vhmsg)
            vhmsg.RemoveMessageEventHandler(new VHMsgBase.MessageEventHandler(VHMsg_MessageEvent));
    }

    void OnLevelWasLoaded()
    {
        //Debug.Log("GlobalsLoaded.OnLevelWasLoaded()");
    }

    void VHMsg_MessageEvent(object sender, VHMsgBase.Message message)
    {
        if (m_displayVhmsgLog)
        {
            Debug.Log("VHMsg recvd: " + message.s);
        }

        string [] splitargs = message.s.Split( " ".ToCharArray() );

        if (splitargs.Length > 0)
        {
            if (splitargs[0] == "vrAllCall")
            {
                VHMsgBase vhmsg = VHMsgBase.Get();
                vhmsg.SendVHMsg("vrComponent renderer");
            }
            else if (splitargs[0] == "vrKillComponent")
            {
                if (splitargs.Length > 1)
                {
                    if (splitargs[1] == "renderer" || splitargs[1] == "all")
                    {
                        if (Application.isEditor)
                        {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.ExecuteMenuItem( "Edit/Play" );
#endif
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }
                }
            }
            else if (splitargs[0] == "PlaySound")
            {
                string path = splitargs[1].Trim('"');   // PlaySound has double quotes around the sound file.  remove them before continuing.
                path = Path.GetFullPath(path);
                path = path.Replace("\\", "/");
                path = "file://" + path;
                WWW www = new WWW(path);
                Utils.PlayWWWSound(this, www, m_sbm.GetCharacterVoice(splitargs[2]), false);
            }
            else if (splitargs[0] == "StopSound")
            {
                m_sbm.GetCharacterVoice("Brad").Stop();
            }
            else if (splitargs[0] == "renderer")
            {
                if (splitargs.Length > 2)
                {
                    if (splitargs[1] == "function")
                    {
                        // "renderer function log testing testing"
                        // "renderer function console show_tips 1"

                        string function = splitargs[2].ToLower();
                        string[] rendererSplitArgs = new string[splitargs.Length - 3];
                        Array.Copy(splitargs, 3, rendererSplitArgs, 0, splitargs.Length - 3);

                        gameObject.SendMessage(function, rendererSplitArgs);
                    }
                }
            }
            else if (splitargs[0] == "vrSpeak" || splitargs[0] == "vrAgentBML")
            {
#if false
                vrSpeak Brad ALL sbm_test_bml_7
                <?xml version="1.0" encoding="UTF-8"?>
                <act>
                    <bml>
                        <animation name="ChrBrad@Idle01_ChopBoth01" start="1.0" />
                    </bml>
                </act>
#endif
                if (splitargs.Length > 4)
                {
                    string character = splitargs[1];
                    //string all = splitargs[2];
                    //string bmlId = splitargs[3];
                    string xml = String.Join(" ", splitargs, 4, splitargs.Length - 4);

                    if (m_BMLEventHandler != null && character == "BradM")
                    {
                        m_BMLEventHandler.LoadXMLString(character, xml);
                    }
                    //BMLParser.TestBMLParser(character, xml);
                }
            }
        }
    }

    void OnCharacterCreate(UnitySmartbodyCharacter character)
    {
        Debug.Log(string.Format("Character '{0}' created", character.SBMCharacterName));
    }

    void OnCharacterDelete(UnitySmartbodyCharacter character)
    {
        Debug.Log(string.Format("Character '{0}' deleted", character.SBMCharacterName));
    }

    protected void log( string [] args )
    {
        if (args.Length > 0)
        {
            string argsString = String.Join(" ", args);
            Debug.Log(argsString);
        }
    }

    protected void console( string [] args )
    {
        if (args.Length > 0)
        {
            string argsString = String.Join(" ", args);
            HandleConsoleMessage(argsString, m_Console);
        }
    }

    protected override void HandleConsoleMessage(string commandEntered, DebugConsole console)
    {
        base.HandleConsoleMessage(commandEntered, console);

        if (commandEntered.IndexOf("vhmsg") != -1)
        {
            string opCode = string.Empty;
            string args = string.Empty;
            if (console.ParseVHMSG(commandEntered, ref opCode, ref args))
            {
                VHMsgBase vhmsg = VHMsgBase.Get();
                vhmsg.SendVHMsg(opCode, args);
            }
            else
            {
                console.AddTextToLog(commandEntered + " requires an opcode string and can have an optional argument string");
            }
        }
        else if (commandEntered.IndexOf("set_resolution") != -1)
        {
            Vector2 vec2Data = Vector2.zero;
            if (console.ParseVector2(commandEntered, ref vec2Data))
            {
                SetResolution((int)vec2Data.x, (int)vec2Data.y, Screen.fullScreen);
            }
        }
    }

    void SetResolution(int width, int height, bool fullScreen)
    {
        Screen.SetResolution(width, height, fullScreen);
    }

    void MobilePlayAudio(string audioFile)
    {
        // Play the audio directly because VHMsg isn't enabled on mobile.  So, we can't receive the PlaySound message

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string s = "Sounds/" + audioFile + ".wav";
            var www = Utils.LoadStreamingAssetsAsync(s);
            Utils.PlayWWWSound(this, www, m_sbm.GetCharacterVoice("Brad"), false);
        }
    }

    protected IEnumerator StartReachSequence()
    {
        Debug.Log(string.Format("StartReachSequence()"));

        string cubeName = "GrabSphere1";

        VHMsgBase.Get().SendVHMsg(string.Format(@"sb bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-range=""neck eyes"" />')", "Brad", cubeName));

        yield return new WaitForSeconds(0.2f);

        VHMsgBase.Get().SendVHMsg(string.Format(@"sb bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" sbm:action=""touch"" sbm:reach-type=""right"" target=""{1}"" />')", "Brad", cubeName));

        yield return new WaitForSeconds(0.8f);

        //VHMsgBase.Get().SendVHMsg(string.Format(@"unity reach {0} attach {1} r_wrist", cubeName, "Brad"));
        //yield return new WaitForSeconds(0.5f);
        //VHMsgBase.Get().SendVHMsg(string.Format(@"sb bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" target=""Issue{1}"" sbm:reach-type=""right"" sbm:fade-in=""1.0"" />')", "Brad", other_player));

        yield return new WaitForSeconds(1.0f);

        //VHMsgBase.Get().SendVHMsg(string.Format(@"unity reach {0} remove {1}", cubeName, "Brad"));
        VHMsgBase.Get().SendVHMsg(string.Format(@"sb bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" sbm:action=""touch"" sbm:reach-finish=""true"" />')", "Brad"));
        VHMsgBase.Get().SendVHMsg(string.Format(@"sb bml.execBML('{0}', '<gaze target=""{1}"" sbm:joint-range=""neck eyes"" />')", "Brad", "Camera"));
    }

    public void ToggleAxisLines()
    {
        GameObject axisLines = GameObject.Find("AxisLines");
        if (axisLines)
        {
            if (axisLines.transform.childCount > 0)
            {
                Transform[] allChildren = axisLines.GetComponentsInChildren<Transform>(true);

                if (axisLines.transform.GetChild(0).gameObject.activeSelf)
                {
                    foreach (Transform t in allChildren)
                    {
                        if (t == axisLines.transform)
                            continue;

                        t.gameObject.SetActive(false);
                    }
                }
                else
                {
                    foreach (Transform t in allChildren)
                    {
                        if (t == axisLines.transform)
                            continue;

                        t.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
