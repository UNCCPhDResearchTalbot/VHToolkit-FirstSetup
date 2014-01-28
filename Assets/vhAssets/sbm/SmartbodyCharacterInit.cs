using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public abstract class SmartbodyCharacterInit : MonoBehaviour
{
    [NonSerialized] public string unityBoneParent;
    [NonSerialized] public string skeletonName;
    [NonSerialized] public string voiceType;  // 'audiofile' or 'remote'
    [NonSerialized] public string voiceCode;  // if 'audiofile' then points to folder that has sounds, else the TTS voice to use
    [NonSerialized] public string voiceTypeBackup;  // if the main voice is not available, offer a backup voice
    [NonSerialized] public string voiceCodeBackup;

    [NonSerialized] public bool useVisemeCurves;  // true for all current characters

    [NonSerialized] public List<KeyValuePair<string, string>> assetPaths = new List<KeyValuePair<string,string>>();   // "ME", "Art/Characters"

    [NonSerialized] public string startingPosture;

    [NonSerialized] public string locomotionInitPythonSkeletonName;  // Set to the skeleton name to use for locomotion initialization, eg "ChrBrad.sk"
    [NonSerialized] public string locomotionInitPythonFile;  // which python file to run to initialize all the smartbody locomotion parameters for this character
    [NonSerialized] public string locomotionSteerPrefix;  // the steering agent looks for particular blend names like 'xxxLocomotion'. The prefix in this case is 'xxx'. Usually, it is the characgter's name.

    public delegate void CharacterInitHandler(UnitySmartbodyCharacter character);
    public event CharacterInitHandler PostLoadEvent;


    void Start()
    {
    }

    public void TriggerPostLoadEvent(UnitySmartbodyCharacter character)
    {
        if (PostLoadEvent != null)
            PostLoadEvent(character);
    }
}
