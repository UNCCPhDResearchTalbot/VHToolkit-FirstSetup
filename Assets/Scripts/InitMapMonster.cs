using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class InitMapMonster : SmartbodyJointMap
{
    void Awake()
    {
        mapName = "monster";

        mappings.Add(new KeyValuePair<string,string>("hip", "base"));
        mappings.Add(new KeyValuePair<string,string>("belly", "spine1"));
        mappings.Add(new KeyValuePair<string,string>("shoulder", "spine2"));
        mappings.Add(new KeyValuePair<string,string>("shoulder", "spine3"));
        mappings.Add(new KeyValuePair<string,string>("shoulder", "spine4"));
        mappings.Add(new KeyValuePair<string,string>("shoulder", "spine5"));
        mappings.Add(new KeyValuePair<string,string>("head", "skullbase"));
        mappings.Add(new KeyValuePair<string,string>("L_arm", "l_acromioclavicular"));
        mappings.Add(new KeyValuePair<string,string>("L_arm", "l_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("L_elbow", "l_elbow"));
        mappings.Add(new KeyValuePair<string,string>("L_wrist", "l_wrist"));
        mappings.Add(new KeyValuePair<string,string>("L_thumb1", "l_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("L_thumb2", "l_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("L_thumbtip", "l_thumb3"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandThumb4", "l_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("L_finger1", "l_index1"));
        mappings.Add(new KeyValuePair<string,string>("L_finger2", "l_index2"));
        mappings.Add(new KeyValuePair<string,string>("L_fingertip", "l_index3"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandIndex4", "l_index4"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle1", "l_middle1"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle2", "l_middle2"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle3", "l_middle3"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandMiddle4", "l_middle4"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandRing1", "l_ring1"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandRing2", "l_ring2"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandRing3", "l_ring3"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandRing4", "l_ring4"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandPinky1", "l_pinky1"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandPinky2", "l_pinky2"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandPinky3", "l_pinky3"));
        //mappings.Add(new KeyValuePair<string,string>("LeftHandPinky4", "l_pinky4"));
        mappings.Add(new KeyValuePair<string,string>("R_arm", "r_acromioclavicular"));
        mappings.Add(new KeyValuePair<string,string>("R_arm", "r_shoulder"));
        mappings.Add(new KeyValuePair<string,string>("R_elbow", "r_elbow"));
        mappings.Add(new KeyValuePair<string,string>("R_wrist", "r_wrist"));
        mappings.Add(new KeyValuePair<string,string>("R_thumb1", "r_thumb1"));
        mappings.Add(new KeyValuePair<string,string>("R_thumb2", "r_thumb2"));
        mappings.Add(new KeyValuePair<string,string>("R_thumbtip", "r_thumb3"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandThumb4", "r_thumb4"));
        mappings.Add(new KeyValuePair<string,string>("R_finger1", "r_index1"));
        mappings.Add(new KeyValuePair<string,string>("R_finger2", "r_index2"));
        mappings.Add(new KeyValuePair<string,string>("R_fingertip", "r_index3"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandIndex4", "r_index4"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandMiddle1", "r_middle1"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandMiddle2", "r_middle2"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandMiddle3", "r_middle3"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandMiddle4", "r_middle4"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandRing1", "r_ring1"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandRing2", "r_ring2"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandRing3", "r_ring3"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandRing4", "r_ring4"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandPinky1", "r_pinky1"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandPinky2", "r_pinky2"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandPinky3", "r_pinky3"));
        //mappings.Add(new KeyValuePair<string,string>("RightHandPinky4", "r_pinky4"));
        mappings.Add(new KeyValuePair<string,string>("L_leg", "l_hip"));
        mappings.Add(new KeyValuePair<string,string>("L_knee", "l_knee"));
        mappings.Add(new KeyValuePair<string,string>("L_ankle", "l_ankle"));
        mappings.Add(new KeyValuePair<string,string>("L_foot", "l_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("L_toes", "l_toe"));
        //mappings.Add(new KeyValuePair<string,string>("LeftToe_End", "l_toe"));
        mappings.Add(new KeyValuePair<string,string>("R_leg", "r_hip"));
        mappings.Add(new KeyValuePair<string,string>("R_knee", "r_knee"));
        mappings.Add(new KeyValuePair<string,string>("R_ankle", "r_ankle"));
        mappings.Add(new KeyValuePair<string,string>("R_foot", "r_forefoot"));
        mappings.Add(new KeyValuePair<string,string>("R_toes", "r_toe"));
        //mappings.Add(new KeyValuePair<string,string>("RightToe_End", "r_toe"));
    }


    void Start()
    {
    }
}
