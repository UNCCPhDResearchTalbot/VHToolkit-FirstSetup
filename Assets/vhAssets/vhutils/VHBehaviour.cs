using UnityEngine;
using System;
using System.Collections;


//[Obsolete("VHBehaviour is obsolete.", false)]
public class VHBehaviour : OrderedBehaviour
{
    public override void Start() { base.Start(); }


    //[Obsolete("Call OrderedDontDestroyOnLoad(). Use that instead", false)]
    public void VHDontDestroyOnLoad()
    {
        DontDestroyOnLoadOrdered();
    }
}
