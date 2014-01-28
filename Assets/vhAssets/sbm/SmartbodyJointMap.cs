using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

public abstract class SmartbodyJointMap : MonoBehaviour
{
    public string skeletonName;   // needs to be provided by the user for each instance of this component, eg "ChrBrad.sk", "ChrRachel.sk"
    [NonSerialized] public string mapName;
    [NonSerialized] public List<KeyValuePair<string, string>> mappings = new List<KeyValuePair<string,string>>();  // 'JtSpineA', 'spine1'  or  newJoint, origSBJoint


    void Start()
    {
    }
}
