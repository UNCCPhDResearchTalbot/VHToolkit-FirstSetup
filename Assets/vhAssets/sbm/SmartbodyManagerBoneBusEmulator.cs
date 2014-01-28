using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class SmartbodyManagerBoneBusEmulator : SmartbodyManagerBoneBus
{
    #region Data Members
    // singleton
    static SmartbodyManagerBoneBusEmulator g_boneBusEmulator;
    #endregion

    #region Functions
    new public static SmartbodyManagerBoneBusEmulator Get()
    {
        Debug.Log("SmartbodyManagerBoneBus Get");
        if (g_boneBusEmulator == null)
        {
            g_boneBusEmulator = UnityEngine.Object.FindObjectOfType(typeof(SmartbodyManagerBoneBusEmulator)) as SmartbodyManagerBoneBusEmulator;
        }

        return g_boneBusEmulator;
    }

    public override void Start()
    {
        Application.runInBackground = true;
    }

    protected override void Update()
    {
    }
    #endregion
}
