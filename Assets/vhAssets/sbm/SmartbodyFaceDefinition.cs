using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections;

public abstract class SmartbodyFaceDefinition : MonoBehaviour
{
    public class SmartbodyFacialExpressionDefinition
    {
        public int au;
        public string side;
        public string name;

        public SmartbodyFacialExpressionDefinition(int au, string side, string name) { this.au = au; this.side = side; this.name = name; }
    }

    [NonSerialized] public string definitionName;
    [NonSerialized] public string neutral;
    [NonSerialized] public List<SmartbodyFacialExpressionDefinition> actionUnits = new List<SmartbodyFacialExpressionDefinition>();
    [NonSerialized] public List<KeyValuePair<string, string>> visemes = new List<KeyValuePair<string,string>>();  // "open", "ChrBrad@open"


    void Start()
    {
    }
}
