using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MecanimManager : ICharacterController
{
    #region Variables
    public List<MecanimCharacter> m_characterList = new List<MecanimCharacter>();
    #endregion

    #region Functions
    void Awake()
    {
        m_characterList.AddRange((MecanimCharacter[])GameObject.FindObjectsOfType(typeof(MecanimCharacter)));
    }
    public MecanimCharacter GetCharacterByName(string character)
    {
        return m_characterList.Find(delegate(MecanimCharacter c) { return c.CharacterName == character; });
    }
    #endregion

    #region ICharacterController Functions
    public override void SBRunPythonScript(string script)
    {
        /*string command = string.Format(@"scene.run('{0}')", script);
        PythonCommand(command);*/
    }

    public override void SBMoveCharacter(string character, string direction, float fSpeed, float fLrps, float fFadeOutTime)
    {
        /*string command = string.Format(@"scene.command('sbm test loco char {0} {1} spd {2} rps {3} time {4}')", character, direction, fSpeed, fLrps, fFadeOutTime);
        PythonCommand(command);*/
    }

    public override void SBWalkTo(string character, string waypoint, bool isRunning)
    {
        /*string run = isRunning ? @"manner=""run""" : "";
        string message = string.Format(@"bml.execBML('{0}', '<locomotion target=""{1}"" facing=""{2}"" {3} />')", character, waypoint, waypoint, run);
        PythonCommand(message);*/
    }

    public override void SBWalkImmediate(string character, string locomotionPrefix, double velocity, double turn, double strafe)
    {
        //<sbm:states mode="schedule" loop="true" name="allLocomotion" x="100" y ="0" z="0"/>
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states mode=""schedule"" loop=""true"" sbm:startnow=""true"" name=""{1}"" x=""{2}"" y =""{3}"" z=""{4}"" />')", character, locomotionPrefix, velocity, turn, strafe);
        PythonCommand(message);*/
    }

    public override void SBPlayAudio(string character, string audioId)
    {
        GetCharacterByName(character).PlayAudio(audioId);
    }

    public override void SBPlayAudio(string character, string audioId, string text)
    {
        GetCharacterByName(character).PlayAudio(audioId);
    }

    public override void SBPlayAudio(string character, AudioClip audioId)
    {
        GetCharacterByName(character).PlayAudio(audioId);
    }

    public override void SBPlayAudio(string character, AudioClip audioId, string text)
    {
        GetCharacterByName(character).PlayAudio(audioId);
    }

    public override void SBPlayXml(string character, string xml)
    {
        /*string message = string.Format(@"scene.command('bml char {0} file Assets/Sounds/{1}')", character, xml);
        PythonCommand(message);*/
    }

    public override void SBTransform(string character, Transform transform)
    {
        SBTransform(character, transform.position, transform.rotation);
    }

    public override void SBTransform(string character, Vector3 pos, Quaternion rot)
    {
        MecanimCharacter c = GetCharacterByName(character);
        c.transform.localPosition = pos;
        c.transform.localRotation = rot;
    }

    public override void SBTransform(string character, double y, double p)
    {

    }

    public override void SBTransform(string character, double x, double y, double z)
    {
        GetCharacterByName(character).transform.position = new Vector3((float)x, (float)y, (float)z);
    }

    public override void SBTransform(string character, double x, double y, double z, double h, double p, double r)
    {
        SBTransform(character, new Vector3((float)x, (float)y, (float)z), Quaternion.Euler(new Vector3((float)p, (float)h, (float)r)));
    }

    public override void SBRotate(string character, double h)
    {
        GetCharacterByName(character).transform.Rotate(0, (float)h, 0);
    }

    public override void SBPosture(string character, string posture, float startTime)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<body posture=""{1}"" start=""{2}""/>')", character, posture, startTime);
        PythonCommand(message);*/
    }

    public override void SBPlayAnim(string character, string animName)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<animation name=""{1}""/>')", character, animName);
        PythonCommand(message);*/
        GetCharacterByName(character).PlayAnim(animName);
    }

    public override void SBPlayAnim(string character, string animName, float readyTime, float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
    {
        SBPlayAnim(character, animName);
    }

    public override void SBPlayFAC(string character, int au, SmartbodyManager.FaceSide side, float weight, float time)
    {
        // TODO - add blend in/out time
        // side == "left", "right" or "both"
        /*string message = string.Format(@"bml.execBML('{0}', '<face type=""facs"" au=""{1}"" side=""{2}"" amount=""{3}"" sbm:duration=""{4}"" />')", character, au, side, weight, time);
        PythonCommand(message);*/
        //GetCharacterByName(character).Speak(au, side, weight, time);
    }

    public override IEnumerator SBPlayViseme(string character, string viseme, float weight, float totalTime, float blendTime)
    {
        // sbm char * viseme W 0 1

        // Please note this needs to be called from a StartCoroutine()!
        // sbm viseme command is an immediate command, doesn't have a total duration.  So we send one command to go to weight, then another to go to 0

        /*string message = string.Format(@"scene.command('char {0} viseme {1} {2} {3}')", character, viseme, weight, blendTime);
        PythonCommand(message);

        yield return new WaitForSeconds(totalTime - blendTime);

        message = string.Format(@"scene.command('char {0} viseme {1} {2} {3}')", character, viseme, 0, blendTime);
        PythonCommand(message);*/
        yield break;
    }

    public override void SBNod(string character, float amount, float repeats, float time)
    {
        GetCharacterByName(character).NodHead(amount, repeats, time);
    }

    public override void SBShake(string character, float amount, float repeats, float time)
    {
        GetCharacterByName(character).ShakeHead(amount, repeats, time);
    }

    public override void SBGaze(string character, string gazeAt)
    {
        GetCharacterByName(character).SetGazeTarget(GameObject.Find(gazeAt));
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed)
    {
        GetCharacterByName(character).SetGazeTarget(GameObject.Find(gazeAt));
    }

    public override void SBGaze(string character, string gazeAt, float neckSpeed, float eyeSpeed, SmartbodyManager.GazeJointRange jointRange)
    {
        GetCharacterByName(character).SetGazeTarget(GameObject.Find(gazeAt));
    }

    public override void SBGaze(string character, string gazeAt, SmartbodyManager.GazeTargetBone targetBone, SmartbodyManager.GazeDirection gazeDirection,
            SmartbodyManager.GazeJointRange jointRange, float angle, float headSpeed, float eyeSpeed, float fadeOut, string gazeHandleName, float duration)
    {
        GetCharacterByName(character).SetGazeTarget(GameObject.Find(gazeAt));
    }

    public override void SBStopGaze(string character)
    {
        GetCharacterByName(character).SetGazeTarget(null);
    }

    public override void SBStopGaze(string character, float fadoutTime)
    {
        GetCharacterByName(character).SetGazeTarget(null);
    }

    public override void SBSaccade(string character, SmartbodyManager.SaccadeType type, bool finish, float duration)
    {

    }

    public override void SBSaccade(string character, SmartbodyManager.SaccadeType type, bool finish, float duration, float angleLimit, float direction, float magnitude)
    {

    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}""/>')", character, state, mode, wrapMode, scheduleMode);
        PythonCommand(message);*/
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}""/>')", character, state, mode, wrapMode, scheduleMode, x.ToString());
        PythonCommand(message);*/
    }

    public override void SBStateChange(string character, string state, string mode, string wrapMode, string scheduleMode, float x, float y, float z)
    {
        /*string message = string.Format(@"bml.execBML('{0}', '<sbm:states name=""{1}"" mode=""{2}"" sbm:wrap-mode=""{3}"" sbm:schedule-mode=""{4}"" x=""{5}"" y=""{6}"" z=""{7}""/>')", character, state, mode, wrapMode, scheduleMode, x.ToString(), y.ToString(), z.ToString());
        PythonCommand(message);*/
    }

    public override string SBGetCurrentStateName(string character)
    {
        /*string pyCmd = string.Format(@"scene.getStateManager().getCurrentState('{0}')", character);
        return PythonCommand<string>(pyCmd);*/
        return string.Empty;
    }

    public override Vector3 SBGetCurrentStateParams(string character)
    {
        /*Vector3 ret = new Vector3();
        string pyCmd = string.Empty;

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(0)", character);
        ret.x = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(1)", character);
        ret.y = PythonCommand<float>(pyCmd);

        pyCmd = string.Format(@"scene.getStateManager().getCurrentStateParameters('{0}').getData(2)", character);
        ret.z = PythonCommand<float>(pyCmd);

        return ret;*/
        return Vector3.zero;
    }

    public override bool SBIsStateScheduled(string character, string stateName)
    {
        /*string pyCmd = string.Format(@"scene.getStateManager().isStateScheduled('{0}', '{1}')", character, stateName);
        return PythonCommand<bool>(pyCmd);*/
        return false;
    }

    public override float SBGetAuValue(string character, string auName)
    {
        /*string pyCmd = string.Format(@"scene.getCharacter('{0}').getSkeleton().getJointByName('{1}').getPosition().getData(0)", character, auName);
        return PythonCommand<float>(pyCmd);*/
        return 0;
    }

    public override void SBExpress(string character, string uttID, string uttNum, string text)
    {
        /*string message = string.Format("vrExpress {0} user 1303332588320-{2}-1 <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>"
            + "<act><participant id=\"{0}\" role=\"actor\" /><fml><turn start=\"take\" end=\"give\" /><affect type=\"neutral\" "
            + "target=\"addressee\"></affect><culture type=\"neutral\"></culture><personality type=\"neutral\"></personality></fml>"
            + "<bml><speech id=\"sp1\" ref=\"{1}\" type=\"application/ssml+xml\">{3}</speech></bml></act>", character, uttID, uttNum, text);
        PythonCommand(message);*/
    }
    #endregion
}
