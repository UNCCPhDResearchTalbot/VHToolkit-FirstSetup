using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


//[Obsolete("VHBehaviourManager class has been renamed to OrderedBehaviorManager. Use that instead", false)]
public class VHBehaviourManager : OrderedBehaviourManager
{
    public override void Start() { base.Start(); }
}
