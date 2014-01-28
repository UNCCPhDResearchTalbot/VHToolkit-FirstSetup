using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class MainMenu : MonoBehaviour
{
    int m_debugTextMode = 0;

    void Awake()
    {
        Application.runInBackground = true;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        Debug.Log("Unity Version: " + Application.unityVersion);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // toggle debug text
            m_debugTextMode++;
            m_debugTextMode = m_debugTextMode % 3;
        }
    }

    void OnGUI()
    {
        float buttonH;

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.Android)
            buttonH = 70;
        else
            buttonH = 20;

        Rect r = new Rect(0.25f, 0.2f, 0.5f, 0.6f);
        GUILayout.BeginArea(VHGUI.ScaleToRes(ref r));
        GUILayout.BeginVertical();

        if (GUILayout.Button("Start Level", GUILayout.Height(buttonH)))
        {
            Application.LoadLevel("vhAssetsTestScene");
        }

        if (GUILayout.Button("Mechanim Test", GUILayout.Height(buttonH)))
        {
            Application.LoadLevel("mecanim");
        }

        GUILayout.Space(40);

        if (GUILayout.Button("Exit", GUILayout.Height(buttonH)))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExecuteMenuItem( "Edit/Play" );
#else
            Application.Quit();
#endif
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        if (m_debugTextMode == 1)
        {
            float fps = 0;
            float averageFps = 0;
            FpsCounter fpsCounter = GetComponent<FpsCounter>();
            if (fpsCounter)
            {
                fps = fpsCounter.Fps;
                averageFps = fpsCounter.AverageFps;
            }

            GUILayout.BeginArea(new Rect (0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            VHGUILayout.Label(string.Format("T: {0:f2} F: {1} AVG: {2:f0} FPS: {3:f2}", Time.time, Time.frameCount, averageFps, fps), new Color(1, Math.Min( 1.0f, averageFps / 30 ), Math.Min( 1.0f, averageFps / 30 )));
            GUILayout.Label(string.Format("{0}x{1}x{2} ({3})", Screen.width, Screen.height, Screen.currentResolution.refreshRate, Utils.GetCommonAspectText((float)Screen.width / Screen.height)));
            GUILayout.Label(string.Format("{0}", Application.loadedLevelName));

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else if (m_debugTextMode == 2)
        {
            GUILayout.BeginArea(new Rect (0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();

            GUILayout.Label(SystemInfo.operatingSystem);  // Operating system name with version (Read Only).
            GUILayout.Label(string.Format("{0} x {1}", SystemInfo.processorCount, SystemInfo.processorType));  // Processor name (Read Only).
            GUILayout.Label(string.Format("Mem: {0:f1}gb", SystemInfo.systemMemorySize / 1000.0f));  // Amount of system memory present (Read Only).
            GUILayout.Label(SystemInfo.graphicsDeviceName);  // The name of the graphics device (Read Only).
            GUILayout.Label(SystemInfo.graphicsDeviceVersion);  // The graphics API version supported by the graphics device (Read Only).
            GUILayout.Label(string.Format("VMem: {0}mb", SystemInfo.graphicsMemorySize));  // Amount of video memory present (Read Only).
            GUILayout.Label(string.Format("Shader Level: {0:f1}", SystemInfo.graphicsShaderLevel / 10.0f));  // Graphics device shader capability level (Read Only).
            GUILayout.Label(string.Format("Fillrate: {0}", SystemInfo.graphicsPixelFillrate.ToString()));  // Approximate pixel fill-rate of the graphics device (Read Only).
            GUILayout.Label(string.Format("Shadows:{0} RT:{1} FX:{2}", SystemInfo.supportsShadows ? "y" : "n", SystemInfo.supportsRenderTextures ? "y" : "n", SystemInfo.supportsImageEffects ? "y" : "n"));  // Are built-in shadows supported? (Read Only)
            GUILayout.Label(string.Format("deviceUniqueIdentifier: {0}", SystemInfo.deviceUniqueIdentifier));  // A unique device identifier. It is guaranteed to be unique for every
            GUILayout.Label(string.Format("deviceName: {0}", SystemInfo.deviceName));  // The user defined name of the device (Read Only).
            GUILayout.Label(string.Format("deviceModel: {0}", SystemInfo.deviceModel));  // The model of the device (Read Only).

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
