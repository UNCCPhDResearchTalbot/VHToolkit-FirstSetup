using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class InitBradFace : SmartbodyFaceDefinition
{
    void Awake()
    {
        definitionName = "ChrBradFace";
        neutral = "ChrBrad@face_neutral";

        //#actionUnits.Add(new SmartbodyInitFacialExpressionDefinition(1,  "both",    "ChrBrad@001_inner_brow_raiser"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(1,  "left",    "ChrBrad@001_inner_brow_raiser_lf"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(1,  "right",   "ChrBrad@001_inner_brow_raiser_rt"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(2,  "both",    "ChrBrad@002_outer_brow_raiser"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(2,  "left",    "ChrBrad@002_outer_brow_raiser_lf"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(2,  "right",   "ChrBrad@002_outer_brow_raiser_rt"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(4,  "both",    "ChrBrad@004_brow_lowerer"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(4,  "left",    "ChrBrad@004_brow_lowerer_lf"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(4,  "right",   "ChrBrad@004_brow_lowerer_rt"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(5,  "both",    "ChrBrad@005_upper_lid_raiser"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(6,  "both",    "ChrBrad@006_cheek_raiser"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(7,  "both",    "ChrBrad@007_lid_tightener"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(9,  "both",    "ChrBrad@009_nose_wrinkle"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(10, "both",    "ChrBrad@010_upper_lip_raiser"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(12, "both",    "ChrBrad@012_lip_corner_puller"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(12, "left",    "ChrBrad@012_lip_corner_puller_lf"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(12, "right",   "ChrBrad@012_lip_corner_puller_rt"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(15, "both",    "ChrBrad@015_lip_corner_depressor"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(23, "both",    "ChrBrad@023_lip_tightener"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(25, "both",    "ChrBrad@025_lips_part"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(26, "both",    "ChrBrad@026_jaw_drop"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(27, "both",    "ChrBrad@027_mouth_stretch"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(38, "both",    "ChrBrad@038_nostril_dilator"));
        //#actionUnits.Add(new SmartbodyFacialExpressionDefinition(45, "both",    "ChrBrad@blink"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(45, "left",    "ChrBrad@045_blink_lf"));
        actionUnits.Add(new SmartbodyFacialExpressionDefinition(45, "right",   "ChrBrad@045_blink_rt"));

        visemes.Add(new KeyValuePair<string,string>("open",    "ChrBrad@open"));
        visemes.Add(new KeyValuePair<string,string>("W",       "ChrBrad@W"));
        visemes.Add(new KeyValuePair<string,string>("ShCh",    "ChrBrad@ShCh"));
        visemes.Add(new KeyValuePair<string,string>("PBM",     "ChrBrad@PBM"));
        visemes.Add(new KeyValuePair<string,string>("FV",      "ChrBrad@FV"));
        visemes.Add(new KeyValuePair<string,string>("wide",    "ChrBrad@wide"));
        visemes.Add(new KeyValuePair<string,string>("tBack",   "ChrBrad@tBack"));
        visemes.Add(new KeyValuePair<string,string>("tRoof",   "ChrBrad@tRoof"));
        visemes.Add(new KeyValuePair<string,string>("tTeeth",  "ChrBrad@tTeeth"));
    }


    void Start()
    {
    }
}
