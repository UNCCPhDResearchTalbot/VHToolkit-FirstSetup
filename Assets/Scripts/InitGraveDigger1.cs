using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class InitGraveDigger1 : SmartbodyCharacterInit
{
    void Awake()
    {
        unityBoneParent = "mixamorig:Hips";
        assetPaths.Add(new KeyValuePair<string, string>("ChrBrad.sk", "Art/Characters/SB"));
        skeletonName = "GraveDigger1.sk";
        voiceType = "remote";
        voiceCode = "Festival_voice_rab_diphon";
        voiceTypeBackup = "audiofile";
        voiceCodeBackup = Utils.GetExternalAssetsPath() + "Sounds";
        //voiceType = "audiofile";
        //voiceCode = Utils.GetExternalAssetsPath() + "Sounds";
        //voiceTypeBackup = "remote";
        //voiceCodeBackup = "Festival_voice_rab_diphone";
        useVisemeCurves = true;
        startingPosture = "ChrBrad@Idle01";

        locomotionInitPythonFile = "locomotion-ChrBrad-init.py";
        locomotionSteerPrefix = "ChrMarine";

        PostLoadEvent += delegate(UnitySmartbodyCharacter character)
            {
                SmartbodyManager.Get().PythonCommand(string.Format(@"bml.execBML('{0}', '<gaze target=""Camera"" sbm:joint-range=""NECK EYES""/>')", character.SBMCharacterName));
                SmartbodyManager.Get().PythonCommand(string.Format(@"bml.execBML('{0}', '<saccade mode=""talk""/>')", character.SBMCharacterName));
                SmartbodyManager.Get().PythonCommand(string.Format(@"scene.getCharacter('{0}').setStringAttribute('saccadePolicy', 'alwayson')", character.SBMCharacterName));


                // set up reach
                SmartbodyManager sbm = SmartbodyManager.Get();

                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachCntr01').mirror('ChrBrad_ChrBillFord_Idle01_LReachCntr01', '')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachFarCornerLf01').mirror('ChrBrad_ChrBillFord_Idle01_LReachFarCornerLf01', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachFarCornerRt01').mirror('ChrBrad_ChrBillFord_Idle01_LReachFarCornerRt01', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCornerLf01').mirror('ChrBrad_ChrBillFord_Idle01_LReachNearCornerLf01', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCornerRt01').mirror('ChrBrad_ChrBillFord_Idle01_LReachNearCornerRt01', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCntr01').mirror('ChrBrad_ChrBillFord_Idle01_LReachNearCntr01', 'ChrBrad.sk')");

                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Grasp').mirror('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Grasp', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Reach').mirror('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Reach', 'ChrBrad.sk')");
                sbm.PythonCommand(@"scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Release').mirror('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Release', 'ChrBrad.sk')");

                sbm.PythonCommand(string.Format(@"scene.getReachManager().createReach('{0}')", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setInterpolatorType('KNN')", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachCntr01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachFarCornerLf01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachFarCornerRt01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachNearCornerLf01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachNearCornerRt01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('left', scene.getMotion('ChrBrad_ChrBillFord_Idle01_LReachNearCntr01'))", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachCntr01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachFarCornerLf01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachFarCornerRt01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCntr01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCornerLf01'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').addMotion('right', scene.getMotion('ChrBrad_ChrBillFord_Idle01_ReachNearCornerRt01'))", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setGrabHandMotion('right', scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Grasp'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setGrabHandMotion('left', scene.getMotion('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Grasp'))", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setReachHandMotion('right', scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Reach'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setReachHandMotion('left', scene.getMotion('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Reach'))", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setReleaseHandMotion('right', scene.getMotion('ChrBrad_ChrHarmony_Relax001_HandGraspSmSphere_Release'))", character.SBMCharacterName));
                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').setReleaseHandMotion('left', scene.getMotion('ChrBrad_ChrHarmony_Relax001_LHandGraspSmSphere_Release'))", character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"scene.getReachManager().getReach('{0}').build(scene.getCharacter('{1}'))", character.SBMCharacterName, character.SBMCharacterName));

                sbm.PythonCommand(string.Format(@"bml.execBML('{0}', '<sbm:reach sbm:handle=""rdoctor"" effector=""r_index3"" sbm:fade-in=""1.0""/>')", character.SBMCharacterName));


                sbm.SBRunPythonScript("init-reach-grasp.py");
            };
    }


    void Start()
    {
    }
}
