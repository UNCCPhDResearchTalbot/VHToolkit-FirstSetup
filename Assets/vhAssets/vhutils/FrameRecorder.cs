using UnityEngine;
using System.Collections;
using System;

public class FrameRecorder : MonoBehaviour
{
    #region Variables
    public KeyCode m_ToggleCaptureKey = KeyCode.R;
    public int m_CaptureFrameRate = 30;
    string m_OutputFolderName;

    bool m_Capturing;
    #endregion

    #region Functions
    void Update()
    {
        if (Input.GetKeyDown(m_ToggleCaptureKey))
        {
            if (!m_Capturing)
            {
                MovieStartRecording();
            }
            else
            {
                MovieEndRecording();
            }
        }

        if (m_Capturing)
        {
            string name = string.Format("{0}/Frame_{1:D05}.png", m_OutputFolderName, Time.frameCount);

            if (Application.isEditor)
                name =  "../" + name;

            Application.CaptureScreenshot(name);
        }
    }

    private void MovieStartRecording()
    {
        m_Capturing = true;

        // Set the playback framerate!   http://unity3d.com/support/documentation/ScriptReference/Time-captureFramerate.html
        // (real time doesn't influence time anymore)
        Time.captureFramerate = m_CaptureFrameRate;

        //"movie_2012_04_04_1800_21"
        m_OutputFolderName = string.Format("movie_{0}", DateTime.Now.ToString("yyyy_MM_dd_HHmm_ss"));
        System.IO.Directory.CreateDirectory(m_OutputFolderName);

        Debug.Log(m_OutputFolderName); 
    }

    private void MovieEndRecording()
    {
        Time.captureFramerate = 0;
        m_Capturing = false;
    }
    #endregion
}
