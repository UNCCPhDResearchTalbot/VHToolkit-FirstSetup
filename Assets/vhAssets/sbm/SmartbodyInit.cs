using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

public abstract class SmartbodyInit : MonoBehaviour
{
    [NonSerialized] public float scale = 1.0f;  // Unity should always be 1.0 (meter)
    [NonSerialized] public float positionScaleHack = 1.0f;   // this is used when the renderer has a different scale than smartbody, because you are mixing and matching assets.
    [NonSerialized] public string speechAudioFileBasePath = ".";   // // (probably deprecated and no longer needed) eg, mediapath + audiofilepath + "sounds"  (overriden by 'path audio ...' in the .seq file)
    [NonSerialized] public string mediaPath = ".";   // base path prepended to sound, seq file, and motion paths.  default is "."
    [NonSerialized] public string initialPySeqPath = "";  // initial path to find .py/.seq files.  More paths can be added later, this is just the initial path.  If left as null or empty, it will default to (Utils.GetExternalAssetsPath() + "SB").  It's done this way because GetExternalAssetsPath() can't be called in a default assignment.  Can only be called in a Unity function like Awake() or Start()
    [NonSerialized] public List<KeyValuePair<string, string>> assetPaths = new List<KeyValuePair<string,string>>();   // "ME", "Art/Characters"
    [NonSerialized] public float steerGridSizeX = 200;  // "Size of grid in x dimension."
    [NonSerialized] public float steerGridSizeZ = 200;  // "Size of grid in z dimension."
    [NonSerialized] public int steerMaxItemsPerGridCell = 20;  // "Max units per grid cell. If agent density is high, make sure increase this value."
    [NonSerialized] public List<string> initialCommands = new List<string>();  // list of .py commands to run at startup after initialization is done, eg:  scene.command(...), bml.execBML(...), etc

    public delegate void SmartbodyInitHandler();
    public event SmartbodyInitHandler PostLoadEvent;

    void Start()
    {
    }

    public void TriggerPostLoadEvent()
    {
        if (PostLoadEvent != null)
            PostLoadEvent();
    }
}
