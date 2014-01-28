using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class FbxToSbmConverter : EditorWindow
{
    #region Constants
    const string SavedWindowPosXKey = "FbxToSbmConverterWindowX";
    const string SavedWindowPosYKey = "FbxToSbmConverterWindowY";
    const string SavedWindowWKey = "FbxToSbmConverterWindowW";
    const string SavedWindowHKey = "FbxToSbmConverterindowH";
    const string Precision = "f3";

#if false
    string[] AllowedJoints = new string[]
    {
        "JtRoot XPos",
        "JtRoot YPos",
        "JtRoot ZPos",
        "JtRoot Quat",
        "JtHipLf Quat",
        "JtKneeLf Quat",
        "JtAnkleLf Quat",
        "JtBallLf Quat",
        "JtToeLf Quat",
        "JtHipRt Quat",
        "JtKneeRt Quat",
        "JtAnkleRt Quat",
        "JtBallRt Quat",
        "JtToeRt Quat",
        "JtSpineA Quat",
        "JtSpineB Quat",
        "JtSpineC Quat",
        "JtClavicleLf Quat",
        "JtShoulderLf Quat",
        "JtUpperArmTwistALf Quat",
        "JtUpperArmTwistBLf Quat",
        "JtElbowLf Quat",
        "JtForearmTwistALf Quat",
        "JtForearmTwistBLf Quat",
        "JtWristLf Quat",
        "JtIndexALf Quat",
        "JtIndexBLf Quat",
        "JtIndexCLf Quat",
        "JtLittleALf Quat",
        "JtLittleBLf Quat",
        "JtLittleCLf Quat",
        "JtMiddleALf Quat",
        "JtMiddleBLf Quat",
        "JtMiddleCLf Quat",
        "JtRingALf Quat",
        "JtRingBLf Quat",
        "JtRingCLf Quat",
        "JtThumbALf Quat",
        "JtThumbBLf Quat",
        "JtThumbCLf Quat",
        "JtClavicleRt Quat",
        "JtShoulderRt Quat",
        "JtUpperArmTwistARt Quat",
        "JtUpperArmTwistBRt Quat",
        "JtElbowRt Quat",
        "JtForearmTwistARt Quat",
        "JtForearmTwistBRt Quat",
        "JtWristRt Quat",
        "JtIndexARt Quat",
        "JtIndexBRt Quat",
        "JtIndexCRt Quat",
        "JtLittleARt Quat",
        "JtLittleBRt Quat",
        "JtLittleCRt Quat",
        "JtMiddleARt Quat",
        "JtMiddleBRt Quat",
        "JtMiddleCRt Quat",
        "JtRingARt Quat",
        "JtRingBRt Quat",
        "JtRingCRt Quat",
        "JtThumbARt Quat",
        "JtThumbBRt Quat",
        "JtThumbCRt Quat",
        "JtNeckA Quat",
        "JtNeckB Quat",
        "JtSkullA Quat",
        "JtCheekLowerLf XPos",
        "JtCheekLowerLf YPos",
        "JtCheekLowerLf ZPos",
        "JtCheekLowerLf Quat",
        "JtCheekLowerRt XPos",
        "JtCheekLowerRt YPos",
        "JtCheekLowerRt ZPos",
        "JtCheekLowerRt Quat",
        "JtCheekUpperLf XPos",
        "JtCheekUpperLf YPos",
        "JtCheekUpperLf ZPos",
        "JtCheekUpperLf Quat",
        "JtCheekUpperRt XPos",
        "JtCheekUpperRt YPos",
        "JtCheekUpperRt ZPos",
        "JtCheekUpperRt Quat",
        "JtJaw Quat",
        "JtLipLowerLf XPos",
        "JtLipLowerLf YPos",
        "JtLipLowerLf ZPos",
        "JtLipLowerLf Quat",
        "JtLipLowerMid XPos",
        "JtLipLowerMid YPos",
        "JtLipLowerMid ZPos",
        "JtLipLowerMid Quat",
        "JtLipLowerRt XPos",
        "JtLipLowerRt YPos",
        "JtLipLowerRt ZPos",
        "JtLipLowerRt Quat",
        "JtTongueA XPos",
        "JtTongueA YPos",
        "JtTongueA ZPos",
        "JtTongueA Quat",
        "JtTongueB XPos",
        "JtTongueB YPos",
        "JtTongueB ZPos",
        "JtTongueB Quat",
        "JtTongueC XPos",
        "JtTongueC YPos",
        "JtTongueC ZPos",
        "JtTongueC Quat",
        "JtLipCornerLf XPos",
        "JtLipCornerLf YPos",
        "JtLipCornerLf ZPos",
        "JtLipCornerLf Quat",
        "JtLipCornerRt XPos",
        "JtLipCornerRt YPos",
        "JtLipCornerRt ZPos",
        "JtLipCornerRt Quat",
        "JtLipUpperLf XPos",
        "JtLipUpperLf YPos",
        "JtLipUpperLf ZPos",
        "JtLipUpperLf Quat",
        "JtLipUpperMid XPos",
        "JtLipUpperMid YPos",
        "JtLipUpperMid ZPos",
        "JtLipUpperMid Quat",
        "JtLipUpperRt XPos",
        "JtLipUpperRt YPos",
        "JtLipUpperRt ZPos",
        "JtLipUpperRt Quat",
        "JtEyebrow01Lf XPos",
        "JtEyebrow01Lf YPos",
        "JtEyebrow01Lf ZPos",
        "JtEyebrow01Lf Quat",
        "JtEyebrow02Lf XPos",
        "JtEyebrow02Lf YPos",
        "JtEyebrow02Lf ZPos",
        "JtEyebrow02Lf Quat",
        "JtEyebrow03Lf XPos",
        "JtEyebrow03Lf YPos",
        "JtEyebrow03Lf ZPos",
        "JtEyebrow03Lf Quat",
        "JtEyebrow04Lf XPos",
        "JtEyebrow04Lf YPos",
        "JtEyebrow04Lf ZPos",
        "JtEyebrow04Lf Quat",
        "JtEyebrow01Rt XPos",
        "JtEyebrow01Rt YPos",
        "JtEyebrow01Rt ZPos",
        "JtEyebrow01Rt Quat",
        "JtEyebrow02Rt XPos",
        "JtEyebrow02Rt YPos",
        "JtEyebrow02Rt ZPos",
        "JtEyebrow02Rt Quat",
        "JtEyebrow03Rt XPos",
        "JtEyebrow03Rt YPos",
        "JtEyebrow03Rt ZPos",
        "JtEyebrow03Rt Quat",
        "JtEyebrow04Rt XPos",
        "JtEyebrow04Rt YPos",
        "JtEyebrow04Rt ZPos",
        "JtEyebrow04Rt Quat",
        "JtEarLf XPos",
        "JtEarLf YPos",
        "JtEarLf ZPos",
        "JtEarLf Quat",
        "JtEarRt XPos",
        "JtEarRt YPos",
        "JtEarRt ZPos",
        "JtEarRt Quat",
        "JtEyeLf Quat",
        "JtEyeRt Quat",
        "JtEyelidLowerLf XPos",
        "JtEyelidLowerLf YPos",
        "JtEyelidLowerLf ZPos",
        "JtEyelidLowerLf Quat",
        "JtEyelidLowerRt XPos",
        "JtEyelidLowerRt YPos",
        "JtEyelidLowerRt ZPos",
        "JtEyelidLowerRt Quat",
        "JtEyelidUpperLf XPos",
        "JtEyelidUpperLf YPos",
        "JtEyelidUpperLf ZPos",
        "JtEyelidUpperLf Quat",
        "JtEyelidUpperRt XPos",
        "JtEyelidUpperRt YPos",
        "JtEyelidUpperRt ZPos",
        "JtEyelidUpperRt Quat",
        "JtUpperNoseLf XPos",
        "JtUpperNoseLf YPos",
        "JtUpperNoseLf ZPos",
        "JtUpperNoseLf Quat",
        "JtLowerNoseLf XPos",
        "JtLowerNoseLf YPos",
        "JtLowerNoseLf ZPos",
        "JtLowerNoseLf Quat",
        "JtUpperNoseRt XPos",
        "JtUpperNoseRt YPos",
        "JtUpperNoseRt ZPos",
        "JtUpperNoseRt Quat",
        "JtLowerNoseRt XPos",
        "JtLowerNoseRt YPos",
        "JtLowerNoseRt ZPos",
        "JtLowerNoseRt Quat",
    };
#endif

    class ChannelData
    {
        public float Time;
        public Quaternion Quat;
        public List<string> parents = new List<string>();
        //public Vector3 Pos;
    }

    enum OutputFormat
    {
        sk,
        skm
    }
    #endregion

    #region Variables
    string[] m_OutputFormats = new string[2] { "sk (skeleton)", "skm (animation)" };
    AnimationClip m_SelectedClip;
    Transform m_SelectedTransform;
    string m_OutputPathSk = "";
    string m_OutputPathSkm = "";
    float m_Scale = 1.0f;
    OutputFormat m_OutputFormat = OutputFormat.sk;
    int m_NumJoints = 0;
    bool m_UseRotationAsPreRot;
    public SmartbodyManager.SkmMetaData m_SkmMetaData = new SmartbodyManager.SkmMetaData();
    float m_EmphasisTime;
    float m_ReadyTime;
    float m_RelaxTime;
    float m_StrokeStartTime;
    float m_StrokeTime;
    #endregion

    #region Functions
    [MenuItem("VH/Fbx To Skm")]
    static void Init()
    {
        FbxToSbmConverter window = (FbxToSbmConverter)EditorWindow.GetWindow(typeof(FbxToSbmConverter));
        window.autoRepaintOnSceneChange = true;
        window.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(SavedWindowWKey, 215),
            PlayerPrefs.GetFloat(SavedWindowHKey, 145));
        window.title = "FbxToSbm";

        window.m_SkmMetaData.SyncPoints.Add("ready", 0);
        window.m_SkmMetaData.SyncPoints.Add("strokeStart", 0);
        window.m_SkmMetaData.SyncPoints.Add("emphasis", 0);
        window.m_SkmMetaData.SyncPoints.Add("stroke", 0);
        window.m_SkmMetaData.SyncPoints.Add("relax", 0);

        window.Show();
    }

    void OnDestroy()
    {
        SaveLocation();
    }

    void SaveLocation()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, position.y);
        PlayerPrefs.SetFloat(SavedWindowWKey, position.width);
        PlayerPrefs.SetFloat(SavedWindowHKey, position.height);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        {
            m_OutputFormat = (OutputFormat)EditorGUILayout.Popup("Output Format", (int)m_OutputFormat, m_OutputFormats);
            switch (m_OutputFormat)
            {
                case OutputFormat.sk:
                    DrawFormatSK();
                    break;

                case OutputFormat.skm:
                    DrawFormatSKM();
                    break;
            }
        }
        GUILayout.EndVertical();
    }

    void DrawFormatSK()
    {
        m_SelectedTransform = (Transform)EditorGUILayout.ObjectField("Target Skeleton", m_SelectedTransform, typeof(Transform), true);
        GUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            m_OutputPathSk = EditorGUILayout.TextField("Output Path", m_OutputPathSk);
            GUI.enabled = true;
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string output = EditorUtility.SaveFilePanelInProject("Output sk", "", "sk", "");
                if (!string.IsNullOrEmpty(output))
                {
                    m_OutputPathSk = output;
                }
                Repaint();
                Focus();
            }
        }
        GUILayout.EndHorizontal();
        m_Scale = EditorGUILayout.FloatField("Scale", m_Scale);
        if (GUILayout.Button("Convert"))
        {
            ConvertFbxToSk(m_SelectedTransform, m_OutputPathSk);
        }
    }

    void DrawFormatSKM()
    {
        m_SelectedClip = (AnimationClip)EditorGUILayout.ObjectField("Target Animation", m_SelectedClip, typeof(AnimationClip), false);
        GUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            m_OutputPathSkm = EditorGUILayout.TextField("Output Path", m_OutputPathSkm);
            GUI.enabled = true;
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string output = EditorUtility.SaveFilePanelInProject("Output skm", "", "txt", "");
                if (!string.IsNullOrEmpty(output))
                {
                    m_OutputPathSkm = output;
                }
                Repaint();
                Focus();
            }
        }
        GUILayout.EndHorizontal();
        m_Scale = EditorGUILayout.FloatField("Scale", m_Scale);

        m_EmphasisTime = EditorGUILayout.FloatField("Emphasis", m_EmphasisTime);
        m_ReadyTime = EditorGUILayout.FloatField("Ready", m_ReadyTime);
        m_RelaxTime = EditorGUILayout.FloatField("Relax", m_RelaxTime);
        m_StrokeStartTime = EditorGUILayout.FloatField("Stroke Start", m_StrokeStartTime);
        m_StrokeTime = EditorGUILayout.FloatField("Stroke", m_StrokeTime);

        if (GUILayout.Button("Convert"))
        {
            ConvertFbxToSkm(m_SelectedClip, m_OutputPathSkm);
        }
    }

    bool AllowedChannel(string channelName)
    {
        //return (channelName.IndexOf("m_LocalRotation") != -1 || channelName.IndexOf("m_LocalPosition") != -1);
        return (channelName.IndexOf("m_LocalRotation") != -1);
    }

    Quaternion CreateQuat(float x, float y, float z, float degrees)
    {
        Quaternion q = new Quaternion();
        float radians = Mathf.PI * degrees / 180.0f;
        //float radians = degrees * DEG_TO_RAD;

        // normalize axis:
        q.x = x;
        q.y = y;
        q.z = z;

        float f = x * x + y * y + z * z;

        if (f > 0)
        {
            f = Mathf.Sqrt(f);
            q.x /= f;
            q.y /= f;
            q.z /= f;
        }

        // set the quaternion:
        radians /= 2;
        f = Mathf.Sin(radians);
        q.x *= f;
        q.y *= f;
        q.z *= f;
        q.w = Mathf.Cos(radians);
        return q;
    }

    void QuatToAxisAngle(Quaternion q, out Vector4 aa, bool normalizeAxis)
    {
        aa = new Vector4();
        float ang = 2.0f * Mathf.Acos(q.w);
        //q.y *= -1;
        //q.z *= -1;
        float norm = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
        if (norm == 0 || ang == 0)
        {
            aa[0] = 0;
            aa[1] = 0;
            aa[2] = 0;
        }
        else
        {
            aa[0] = (q[0] / norm);
            aa[1] = (q[1] / norm);
            aa[2] = (q[2] / norm);

            if (!normalizeAxis)
            {
                aa[0] *= ang;
                aa[1] *= ang;
                aa[2] *= ang;
            }
        }

        aa[3] = ang * 180.0f / Mathf.PI;
    }

    void ConvertFbxToSk(Transform root, string outputPathAndName)
    {
        if (string.IsNullOrEmpty(outputPathAndName))
        {
            EditorUtility.DisplayDialog("Error", "Please select an output file path for this sk.", "Ok");
            return;
        }

        StreamWriter outfile = new StreamWriter(string.Format("{0}", outputPathAndName));
        
        outfile.WriteLine("# SK Skeleton Definition");
        outfile.WriteLine(string.Format("# File generated with Unity FbxConverter.exe v1.0    Units: m  Joints: {0}  Channels: {1}", "119", "201"));
        outfile.Write(Environment.NewLine);
        outfile.WriteLine(string.Format("set_name {0}", Path.GetFileNameWithoutExtension(outputPathAndName)));
        outfile.Write(Environment.NewLine);
        outfile.WriteLine("skeleton");

        m_NumJoints = 0;
        int tabCount = 0;
        ParseJoints(root, tabCount, outfile);

        outfile.WriteLine("end");
        outfile.Close();
    }

    void ParseJoints(Transform node, int tabCount, StreamWriter output)
    {
        StringBuilder buffer = new StringBuilder("");

        //string jointName = node.name;
        Quaternion rotData = Quaternion.identity;
        for (int i = 0; i < tabCount; i++)
        {
            buffer.Append("  ");
        }

        Vector3 pos = node.localPosition;
        if (m_Scale != 1)
        {
            pos *= m_Scale;
        }
        output.WriteLine(string.Format("{0}{1} {2} ", buffer.ToString(), m_NumJoints == 0 ? "root" : "joint", node.name));
        output.WriteLine(string.Format("{0}{{ offset {1} {2} {3}", buffer.ToString(), (-pos.x).ToString(Precision), pos.y.ToString(Precision), pos.z.ToString(Precision)));
        
        /*if (m_UseRotationAsPreRot)
        {
            rotData = node.localRotation;
        }
        else*/
        {
            rotData = node.localRotation;
        }

        //if (rotData.y != 0)
        {
            rotData.y = -rotData.y;
        }
        //if (rotData.z != 0)
        {
            rotData.z = -rotData.z;
        }
        

        Vector4 axisAngle = Vector4.zero;
        QuatToAxisAngle(rotData, out axisAngle, true);
        output.WriteLine(string.Format("  {0}prerot  axis {1} {2} {3} ang {4} ", buffer, axisAngle.x.ToString(Precision), axisAngle.y.ToString(Precision), axisAngle.z.ToString(Precision), axisAngle.w.ToString(Precision)));

        //if (node.name == "JtRoot")
        {
            output.WriteLine(string.Format("  {0}channel XPos 0 free", buffer.ToString()));
            output.WriteLine(string.Format("  {0}channel YPos 0 free", buffer.ToString()));
            output.WriteLine(string.Format("  {0}channel ZPos 0 free", buffer.ToString()));
        }
        
        output.WriteLine(string.Format("  {0}channel Quat", buffer.ToString()));

        ++m_NumJoints;
        
        if (node.childCount > 0)
        {
            output.Write(Environment.NewLine);
        }

        for (int i = 0; i < node.childCount; i++)
        {
            // iterate on the children of this node
            ParseJoints(node.GetChild(i), tabCount + 1, output);
        }

        buffer = buffer.Remove(0, buffer.Length);
        for (int i = 0; i < tabCount; i++)
        {
            buffer.Append("  ");
        }

        Transform pParent = node.parent;
        if (pParent != null && pParent.childCount > 1
           && pParent.GetChild(pParent.childCount - 1) != node) // not the last child
            output.WriteLine(string.Format("{0}}}{1}", buffer, Environment.NewLine));
        else
            output.WriteLine(string.Format("{0}}}", buffer));
    }

    void ConvertFbxToSkm(AnimationClip clip, string outputName)
    {
        if (clip == null)
        {
            EditorUtility.DisplayDialog("No Clip Selected", "Select an animation clip to convert", "Ok");
            return;
        }

        if (string.IsNullOrEmpty(outputName))
        {
            EditorUtility.DisplayDialog("Error", "Please select an output file path for this skm.", "Ok");
            return;
        }

        //Debug.Log(clip.frameRate);
        Debug.Log("clip.length: " + clip.length);
        int numFrames = (int)(clip.length / 30.0f * 1000.0f) - 1;
        //Debug.Log("numFrames: " + numFrames);
        
        AnimationClipCurveData[] curveData = AnimationUtility.GetAllCurves(clip, true);
        //Debug.Log(string.Format("{0} {1}", curveData[0].path, curveData[0].propertyName));
        StreamWriter outfile = new StreamWriter(string.Format("{0}", outputName));

        outfile.WriteLine("# SKM Motion Definition ");
        outfile.WriteLine("# FbxConverter.exe v1.0    Units: m");
        outfile.Write('\n');
        outfile.WriteLine("SkMotion");
        outfile.Write('\n');
        outfile.WriteLine(string.Format("name \"{0}\"", Path.GetFileNameWithoutExtension(outputName)));
        outfile.Write('\n');

        List<AnimationClipCurveData> holder = new List<AnimationClipCurveData>(curveData);
        Dictionary<string, List<ChannelData>> channelToQuatMap = new Dictionary<string, List<ChannelData>>();
        for (int i = 0; i < curveData.Length; i++)
        {
            //Debug.Log(curveData[i].path);
            List<AnimationClipCurveData> datas = holder.FindAll(a => Path.GetFileNameWithoutExtension(a.path) == Path.GetFileNameWithoutExtension(curveData[i].path));
            string key = "";

            for (int j = 0; j < datas.Count; j++)
            {
                if (!AllowedChannel(datas[j].propertyName))
                {
                    continue;
                }

                //if (Path.GetFileNameWithoutExtension(datas[j].path).IndexOf("IctSettings") != -1)
                //{
                //    continue;
                //}

                string channelName = "";
                if (datas[j].propertyName.Contains("m_LocalPosition.x"))
                {
                    channelName = "XPos";
                }
                else if (datas[j].propertyName.Contains("m_LocalPosition.y"))
                {
                    channelName = "YPos";
                }
                else if (datas[j].propertyName.Contains("m_LocalPosition.z"))
                {
                    channelName = "ZPos";
                }
                else if (datas[j].propertyName.Contains("m_LocalRotation"))
                {
                    channelName = "Quat";
                }

                key = string.Format("{0} {1}", Path.GetFileNameWithoutExtension(datas[j].path), channelName);

                                                            /*
                                                            if (Array.FindIndex<string>(AllowedJoints, s => s == key) == -1)
                                                            {
                                                                continue;
                                                            }
                                                            */

                //Debug.Log("property name: " + datas[j].path);

                if (!channelToQuatMap.ContainsKey(key))
                {
                    channelToQuatMap.Add(key, new List<ChannelData>());
                }

                ChannelData channelData = null;
                bool newKey = false;
                //for (int k = 0; k < datas[j].curve.keys.Length; k++)
                for (int k = 1; k < numFrames + 1; k++)
                {
                    //Keyframe keyframe = datas[j].curve.keys[0];
                    int frame = k - 1;

                    if (frame < channelToQuatMap[key].Count)
                    {
                        channelData = channelToQuatMap[key][frame];
                        newKey = false;
                    }
                    else
                    {
                        channelData = new ChannelData();
                        newKey = true;
                    }

                    //Quaternion jointTransform = GameObject.Find(key.Split(' ')[0]).transform.localRotation;

                    channelData.Time = ((float)(k)) / (float)30.0f;// keyframe.time;
                    if (datas[j].propertyName.Contains("m_LocalRotation.x"))
                    {
                        channelData.Quat.x = datas[j].curve.Evaluate(channelData.Time);
                    }
                    else if (datas[j].propertyName.Contains("m_LocalRotation.y"))
                    {
                        channelData.Quat.y = -datas[j].curve.Evaluate(channelData.Time);
                    }
                    else if (datas[j].propertyName.Contains("m_LocalRotation.z"))
                    {
                        channelData.Quat.z = -datas[j].curve.Evaluate(channelData.Time);
                    }
                    else if (datas[j].propertyName.Contains("m_LocalRotation.w"))
                    {
                        channelData.Quat.w = datas[j].curve.Evaluate(channelData.Time);
                    }
                    else if (datas[j].propertyName.Contains("m_LocalPosition.x"))
                    {
                        channelData.Quat.x = (-datas[j].curve.Evaluate(channelData.Time) - datas[j].curve.Evaluate(0)) * m_Scale;
                        //channelData.Quat.x = (-datas[j].curve.Evaluate(channelData.Time)) * m_Scale;
                    }
                    else if (datas[j].propertyName.Contains("m_LocalPosition.y"))
                    {
                        channelData.Quat.y = (datas[j].curve.Evaluate(channelData.Time) - datas[j].curve.Evaluate(0)) * m_Scale;
                        //channelData.Quat.y = (datas[j].curve.Evaluate(channelData.Time)) * m_Scale;
                    }
                    else if (datas[j].propertyName.Contains("m_LocalPosition.z"))
                    {
                        channelData.Quat.z = (datas[j].curve.Evaluate(channelData.Time) - datas[j].curve.Evaluate(0)) * m_Scale;
                        //channelData.Quat.z = (datas[j].curve.Evaluate(channelData.Time)) * m_Scale;
                    }
                    else
                    {
                        Debug.LogError(string.Format("channel {0} isn't handled", datas[j].propertyName));
                    }

                    if (newKey)
                    {
                        channelToQuatMap[key].Add(channelData);
                    }
                }

                holder.Remove(datas[j]);
            }
        }

        //Debug.Log("channelToQuatMap[JtHipLf Quat].Count" + channelToQuatMap["JtHipLf Quat"].Count);

        outfile.WriteLine(string.Format("channels {0}", channelToQuatMap.Keys.Count));
        foreach (KeyValuePair<string, List<ChannelData>> kvp in channelToQuatMap)
        {
            outfile.WriteLine(kvp.Key);
        }

        outfile.Write("\n");

        outfile.WriteLine(string.Format("frames {0}", numFrames));
        StringBuilder builder = new StringBuilder();
        int currKey = 0;
        Debug.Log("channelToQuatMap.Keys.Count " + channelToQuatMap.Keys.Count);
        //foreach (KeyValuePair<string, List<ChannelData>> kvp in channelToQuatMap)
        for (int i = 0; i < numFrames; i++)
        {
            builder = builder.Remove(0, builder.Length);
            bool lineBegin = true;

            foreach (KeyValuePair<string, List<ChannelData>> kvp2 in channelToQuatMap)
            {
                if (lineBegin)
                {
                    //Debug.Log("currKey: " + currKey + " Channel: " + kvp2.Key);
                    float time = kvp2.Value[currKey].Time - kvp2.Value[0].Time;
                    builder.Append(string.Format("kt {0} fr ", time.ToString(Precision)));
                    lineBegin = false;
                }

                if (kvp2.Key.Contains("XPos"))
                {
                    builder.Append(string.Format("{0} ", kvp2.Value[currKey].Quat.x.ToString(Precision)));
                }
                else if (kvp2.Key.Contains("YPos"))
                {
                    builder.Append(string.Format("{0} ", kvp2.Value[currKey].Quat.y.ToString(Precision)));
                }
                else if (kvp2.Key.Contains("ZPos"))
                {
                    builder.Append(string.Format("{0} ", kvp2.Value[currKey].Quat.z.ToString(Precision)));
                }
                else if (kvp2.Key.Contains("Quat"))
                {
                    //GameObject obj = GameObject.Find(kvp2.Key.Split(' ')[0]);
                    //Vector4 vec4 = new Vector4(obj.transform.localRotation.x, obj.transform.localRotation.y, obj.transform.localRotation.z, obj.transform.localRotation.w);
                    //Quaternion quat = obj.transform.localRotation;
                    //quat.y = -1;
                    //quat.z = -1;
                    //kvp2.Value[currKey].Quat.eulerAngles += quat.eulerAngles;
                    //kvp2.Value[currKey].Quat *= quat;

                    Quaternion quat = kvp2.Value[currKey].Quat;// *obj.transform.localRotation;
                    builder.Append(string.Format("{0} {1} {2} {3} ", quat.w.ToString(Precision), quat.x.ToString(Precision), quat.y.ToString(Precision), quat.z.ToString(Precision)));
/*
                    Quaternion qx, qy, qz;
                    qx = CreateQuat(1, 0, 0, quat.eulerAngles.x);
                    qy = CreateQuat(0, 1, 0, quat.eulerAngles.y);
                    qz = CreateQuat(0, 0, 1, quat.eulerAngles.z);

                    Quaternion qzyx;
                    Quaternion qyx;
                    qyx = qy * qx;
                    qzyx = qz * qyx;

                    //qzyx.y *= -1;
                    //qzyx.z *= -1;
                    //quat.y = quat.y;
                    //quat.z = quat.z;
                    //Quaternion quat = new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
                    //Vector3 preRot = new Vector3(quat.eulerAngles.x, quat.eulerAngles.y, quat.eulerAngles.z);
                    //kvp2.Value[currKey].Quat.eulerAngles += preRot;
                                //
                    //kvp2.Value[currKey].Quat.y *= -1;
                    //kvp2.Value[currKey].Quat.z *= -1;
                    kvp2.Value[currKey].Quat *= qzyx;

                    //kvp2.Value[currKey].Quat.y *= -1;
                    //kvp2.Value[currKey].Quat.z *= -1;
*/

                    //Vector4 axis;
                    //QuatToAxisAngle(kvp2.Value[currKey].Quat, out axis, false);
                    //builder.Append(string.Format("{0} {1} {2} ", axis.x.ToString("f6"), axis.y.ToString("f6"), axis.z.ToString("f6")));

                    //Vector3 axis2;
                    //float angle;
                    //kvp2.Value[currKey].Quat.ToAngleAxis(out angle, out axis2);
                    //axis2.Normalize();
                    //builder.Append(string.Format("{0} {1} {2} ", axis2.x.ToString("f6"), axis2.y.ToString("f6"), axis2.z.ToString("f6")));
                }
            }
            outfile.WriteLine(builder.ToString());
            ++currKey;
        }

        outfile.Write("\n");

        outfile.WriteLine(string.Format("emphasis time: {0}", m_EmphasisTime.ToString(Precision)));
        outfile.WriteLine(string.Format("ready time: {0}", m_ReadyTime.ToString(Precision)));
        outfile.WriteLine(string.Format("relax time: {0}", m_RelaxTime.ToString(Precision)));
        outfile.WriteLine(string.Format("strokeStart time: {0}", m_StrokeStartTime.ToString(Precision)));
        outfile.WriteLine(string.Format("stroke time: {0}", m_StrokeTime.ToString(Precision)));

        outfile.Close();

        Debug.Log("done!!!");
    }

    int GetNumChannels(AnimationClipCurveData[] curveData)
    {
        int numChannels = 0;
        for (int i = 0; i < curveData.Length; i++)
        {
            if (!AllowedChannel(curveData[i].propertyName))
            {
                continue;
            }
            ++numChannels;
        }

        return numChannels;
    }
    #endregion
}
