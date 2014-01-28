using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioBank : MonoBehaviour
{
    #region Variables
    public List<AudioClip> m_Clips = new List<AudioClip>();
    #endregion

    #region Functions
    public AudioClip FindClip(string name)
    {
        return m_Clips.Find(c => c.name == name);
    }
    #endregion
}
