using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used for easy control of cutscenes within the current unity scene
/// </summary>
public class CutsceneDebugPlayer : MonoBehaviour
{
    #region Constants
    [System.Serializable]
    public class KeyToCutsceneMapper
    {
        public KeyCode playButton;
        public KeyCode fastForwardButton;
        public KeyCode pauseButton;
        public KeyCode stopButton;
        public Cutscene cutscene;
    }
    #endregion

    #region Variables
    public List<KeyToCutsceneMapper> m_CutsceneMap = new List<KeyToCutsceneMapper>();
    #endregion

    #region Functions
    void Update()
    {
        foreach (KeyToCutsceneMapper ktc in m_CutsceneMap)
        {
            if (Input.GetKeyDown(ktc.playButton))
            {
                ktc.cutscene.Play();
            }
            else if (Input.GetKeyDown(ktc.fastForwardButton))
            {
                ktc.cutscene.FastForward();
            }
            else if (Input.GetKeyDown(ktc.pauseButton))
            {
                if (ktc.cutscene.IsPaused)
                    ktc.cutscene.Resume();                
                else               
                    ktc.cutscene.Pause();
            }
            else if (Input.GetKeyDown(ktc.stopButton))
            {
                ktc.cutscene.Stop();
            }
        }
    }
    #endregion
}
