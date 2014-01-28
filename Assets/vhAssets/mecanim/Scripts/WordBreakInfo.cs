using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;





public class WordBreakInfo : MonoBehaviour
{
    [Serializable]
    public class WordBreak
    {
        public string name;
        public float start;
        public float end;
    }

    [Serializable]
    public class WordBreakAnim
    {
        public string animName;
        public WordBreak [] wordBreaks;
        public AudioClip audioClip;
    }

    public WordBreakAnim [] m_anims;
}
