using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class InitMapMixamo : SmartbodyJointMap
{
    void Awake()
    {
        mapName = "mixamo";

        mappings.Add(new KeyValuePair<string,string>("Hips", "base"));
        mappings.Add(new KeyValuePair<string,string>("Neck", "spine4"));
        mappings.Add(new KeyValuePair<string,string>("Neck1", "spine5"));
        mappings.Add(new KeyValuePair<string,string>("Head", "skullbase"));
        mappings.Add(new KeyValuePair<string,string>("LeftShoulder", "l_sternoclavicular"));
        mappings.Add(new KeyValuePair<string,string>("LeftArm", "l_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("LeftForeArm", "l_elbow"));
        mappings.Add(new KeyValuePair<string,string>("LeftHand", "l_wrist"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandThumb1", "l_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandThumb2", "l_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandThumb3", "l_thumb3"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandThumb4", "l_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandIndex1", "l_index1"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandIndex2", "l_index2"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandIndex3", "l_index3"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandIndex4", "l_index4"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle1", "l_middle1"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle2", "l_middle2"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle3", "l_middle3"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle4", "l_middle4"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandRing1", "l_ring1"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandRing2", "l_ring2"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandRing3", "l_ring3"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandRing4", "l_ring4"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandPinky1", "l_pinky1"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandPinky2", "l_pinky2"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandPinky3", "l_pinky3"));
        mappings.Add(new KeyValuePair<string,string>("LeftHandPinky4", "l_pinky4"));
        mappings.Add(new KeyValuePair<string,string>("RightShoulder", "r_sternoclavicular"));
        mappings.Add(new KeyValuePair<string,string>("RightArm", "r_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("RightForeArm", "r_elbow"));
        mappings.Add(new KeyValuePair<string,string>("RightHand", "r_wrist"));
        mappings.Add(new KeyValuePair<string,string>("RightHandThumb1", "r_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("RightHandThumb2", "r_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("RightHandThumb3", "r_thumb3"));
        mappings.Add(new KeyValuePair<string,string>("RightHandThumb4", "r_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("RightHandIndex1", "r_index1"));
        mappings.Add(new KeyValuePair<string,string>("RightHandIndex2", "r_index2"));
        mappings.Add(new KeyValuePair<string,string>("RightHandIndex3", "r_index3"));
        mappings.Add(new KeyValuePair<string,string>("RightHandIndex4", "r_index4"));
        mappings.Add(new KeyValuePair<string,string>("RightHandMiddle1", "r_middle1"));
        mappings.Add(new KeyValuePair<string,string>("RightHandMiddle2", "r_middle2"));
        mappings.Add(new KeyValuePair<string,string>("RightHandMiddle3", "r_middle3"));
        mappings.Add(new KeyValuePair<string,string>("RightHandMiddle4", "r_middle4"));
        mappings.Add(new KeyValuePair<string,string>("RightHandRing1", "r_ring1"));
        mappings.Add(new KeyValuePair<string,string>("RightHandRing2", "r_ring2"));
        mappings.Add(new KeyValuePair<string,string>("RightHandRing3", "r_ring3"));
        mappings.Add(new KeyValuePair<string,string>("RightHandRing4", "r_ring4"));
        mappings.Add(new KeyValuePair<string,string>("RightHandPinky1", "r_pinky1"));
        mappings.Add(new KeyValuePair<string,string>("RightHandPinky2", "r_pinky2"));
        mappings.Add(new KeyValuePair<string,string>("RightHandPinky3", "r_pinky3"));
        mappings.Add(new KeyValuePair<string,string>("RightHandPinky4", "r_pinky4"));
        mappings.Add(new KeyValuePair<string,string>("LeftUpLeg", "l_hip"));
        mappings.Add(new KeyValuePair<string,string>("LeftLeg", "l_knee"));
        mappings.Add(new KeyValuePair<string,string>("LeftFoot", "l_ankle"));
        mappings.Add(new KeyValuePair<string,string>("LeftToeBase", "l_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("LeftFootToeBase_End", "l_toe"));
        mappings.Add(new KeyValuePair<string,string>("LeftToe_End", "l_toe"));
        mappings.Add(new KeyValuePair<string,string>("RightUpLeg", "r_hip"));
        mappings.Add(new KeyValuePair<string,string>("RightLeg", "r_knee"));
        mappings.Add(new KeyValuePair<string,string>("RightFoot", "r_ankle"));
        mappings.Add(new KeyValuePair<string,string>("RightToeBase", "r_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("RightFootToeBase_End", "r_toe"));
        mappings.Add(new KeyValuePair<string,string>("RightToe_End", "r_toe"));
        mappings.Add(new KeyValuePair<string,string>("Spine2", "spine3"));
        mappings.Add(new KeyValuePair<string,string>("Spine1", "spine2"));
        mappings.Add(new KeyValuePair<string,string>("Spine", "spine1"));
    }


    void Start()
    {
    }
}
