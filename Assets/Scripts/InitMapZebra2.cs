using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class InitMapZebra2 : SmartbodyJointMap
{
    void Awake()
    {
        mapName = "zebra2";

        //# Core
        mappings.Add(new KeyValuePair<string,string>("JtRoot", "base"));
        mappings.Add(new KeyValuePair<string,string>("JtSpineA", "spine1"));
        mappings.Add(new KeyValuePair<string,string>("JtSpineB", "spine2"));
        mappings.Add(new KeyValuePair<string,string>("JtSpineC", "spine3"));
        mappings.Add(new KeyValuePair<string,string>("JtNeckA", "spine4"));
        mappings.Add(new KeyValuePair<string,string>("JtNeckB", "spine5"));
        mappings.Add(new KeyValuePair<string,string>("JtSkullA", "skullbase"));

        //# Arm, left
        mappings.Add(new KeyValuePair<string,string>("JtClavicleLf", "l_sternoclavicular"));
        mappings.Add(new KeyValuePair<string,string>("JtShoulderLf", "l_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("JtUpperArmTwistALf", "l_upperarm1"));
        mappings.Add(new KeyValuePair<string,string>("JtUpperArmTwistBLf", "l_upperarm2"));
        mappings.Add(new KeyValuePair<string,string>("JtElbowLf", "l_elbow"));
        mappings.Add(new KeyValuePair<string,string>("JtForearmTwistALf", "l_forearm1"));
        mappings.Add(new KeyValuePair<string,string>("JtForearmTwistBLf", "l_forearm2"));
        mappings.Add(new KeyValuePair<string,string>("JtWristLf", "l_wrist"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbALf", "l_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbBLf", "l_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbCLf", "l_thumb3"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbDLf", "l_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexALf", "l_index1"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexBLf", "l_index2"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexCLf", "l_index3"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexDLf", "l_index4"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleALf", "l_middle1"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleBLf", "l_middle2"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleCLf", "l_middle3"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleDLf", "l_middle4"));
        mappings.Add(new KeyValuePair<string,string>("JtRingALf", "l_ring1"));
        mappings.Add(new KeyValuePair<string,string>("JtRingBLf", "l_ring2"));
        mappings.Add(new KeyValuePair<string,string>("JtRingCLf", "l_ring3"));
        mappings.Add(new KeyValuePair<string,string>("JtRingDLf", "l_ring4"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleALf", "l_pinky1"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleBLf", "l_pinky2"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleCLf", "l_pinky3"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleDLf", "l_pinky4"));

        //# Arm, right
        mappings.Add(new KeyValuePair<string,string>("JtClavicleRt", "r_sternoclavicular"));
        mappings.Add(new KeyValuePair<string,string>("JtShoulderRt", "r_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("JtUpperArmTwistARt", "r_upperarm1"));
        mappings.Add(new KeyValuePair<string,string>("JtUpperArmTwistBRt", "r_upperarm2"));
        mappings.Add(new KeyValuePair<string,string>("JtElbowRt", "r_elbow"));
        mappings.Add(new KeyValuePair<string,string>("JtForearmTwistARt", "r_forearm1"));
        mappings.Add(new KeyValuePair<string,string>("JtForearmTwistBRt", "r_forearm2"));
        mappings.Add(new KeyValuePair<string,string>("JtWristRt", "r_wrist"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbARt", "r_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbBRt", "r_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbCRt", "r_thumb3"));
        mappings.Add(new KeyValuePair<string,string>("JtThumbDRt", "r_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexARt", "r_index1"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexBRt", "r_index2"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexCRt", "r_index3"));
        mappings.Add(new KeyValuePair<string,string>("JtIndexDRt", "r_index4"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleARt", "r_middle1"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleBRt", "r_middle2"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleCRt", "r_middle3"));
        mappings.Add(new KeyValuePair<string,string>("JtMiddleDRt", "r_middle4"));
        mappings.Add(new KeyValuePair<string,string>("JtRingARt", "r_ring1"));
        mappings.Add(new KeyValuePair<string,string>("JtRingBRt", "r_ring2"));
        mappings.Add(new KeyValuePair<string,string>("JtRingCRt", "r_ring3"));
        mappings.Add(new KeyValuePair<string,string>("JtRingDRt", "r_ring4"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleARt", "r_pinky1"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleBRt", "r_pinky2"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleCRt", "r_pinky3"));
        mappings.Add(new KeyValuePair<string,string>("JtLittleDRt", "r_pinky4"));

        //# Leg, left
        mappings.Add(new KeyValuePair<string,string>("JtHipLf", "l_hip"));
        mappings.Add(new KeyValuePair<string,string>("JtKneeLf", "l_knee"));
        mappings.Add(new KeyValuePair<string,string>("JtAnkleLf", "l_ankle"));
        mappings.Add(new KeyValuePair<string,string>("JtBallLf", "l_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("JtToeLf", "l_toe"));

        //# Leg, right
        mappings.Add(new KeyValuePair<string,string>("JtHipRt", "r_hip"));
        mappings.Add(new KeyValuePair<string,string>("JtKneeRt", "r_knee"));
        mappings.Add(new KeyValuePair<string,string>("JtAnkleRt", "r_ankle"));
        mappings.Add(new KeyValuePair<string,string>("JtBallRt", "r_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("JtToeRt", "r_toe"));

        //# Head, left
        mappings.Add(new KeyValuePair<string,string>("JtEyeLf", "eyeball_left"));
        mappings.Add(new KeyValuePair<string,string>("JtEyelidUpperLf", "upper_eyelid_left"));
        mappings.Add(new KeyValuePair<string,string>("JtEyelidLowerLf", "lower_eyelid_left"));

        //# Head, right
        mappings.Add(new KeyValuePair<string,string>("JtEyeRt", "eyeball_right"));
        mappings.Add(new KeyValuePair<string,string>("JtEyelidUpperRt", "upper_eyelid_right"));
        mappings.Add(new KeyValuePair<string,string>("JtEyelidLowerRt", "lower_eyelid_right"));

        //mappings.Add(new KeyValuePair<string,string>("eyeJoint_R", "eyeball_right"));
        //mappings.Add(new KeyValuePair<string,string>("eyeJoint_L", "eyeball_left"));
    }


    void Start()
    {
    }
}
