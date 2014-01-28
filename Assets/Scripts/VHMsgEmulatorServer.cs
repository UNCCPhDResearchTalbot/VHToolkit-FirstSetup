using UnityEngine;
using System;
using System.Collections;

public class VHMsgEmulatorServer : VHMain
{
    public GameObject m_NetworkPrefab;
    public int m_MaxConnections = 8;
    public int m_ListenPort = 25000;
    public bool m_UseNat = false;

    public override void OnGUI()
    {
        base.OnGUI();

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            if (GUI.Button(new Rect(10, 10, 90, 25), "Start Server"))
            {
                Network.InitializeServer(m_MaxConnections, m_ListenPort, m_UseNat);
            }

#if false
            // code for connecting to the server

            string m_ConnectionIP = "127.0.0.1";
            int m_ConnectionPort = 25000;

            Rect m_ConnectButtonRect = new Rect(10, 55, 90, 25);
            Rect m_ConnectionIPRect = new Rect(105, 56.5f, 90, 22);
            Rect m_ConnectionPortRect = new Rect(105, 80.5f, 90, 22);
            if (GUI.Button(m_ConnectButtonRect, "Connect"))
            {
                Network.Connect(m_ConnectionIP, m_ConnectionPort);
            }

            m_ConnectionIP = GUI.TextField(m_ConnectionIPRect, m_ConnectionIP);
            m_ConnectionPort = int.Parse(GUI.TextField(m_ConnectionPortRect, m_ConnectionPort.ToString()));
#endif
        }
        else
        {
            if (GUI.Button(new Rect(10, 10, 90, 25), "Disconnect"))
            {
                Network.Disconnect(200);
            }
        }


        float fps = 0;
        float averageFps = 0;
        FpsCounter fpsCounter = GetComponent<FpsCounter>();
        if (fpsCounter)
        {
            fps = fpsCounter.Fps;
            averageFps = fpsCounter.AverageFps;
        }


        GUILayout.BeginArea(new Rect (0, 80, Screen.width, Screen.height));
        GUILayout.BeginVertical();

        VHGUILayout.Label(string.Format("T: {0:f2} F: {1} AVG: {2:f0} FPS: {3:f2}", Time.time, Time.frameCount, averageFps, fps), new Color(1, Math.Min( 1.0f, averageFps / 30 ), Math.Min( 1.0f, averageFps / 30 )));
        GUILayout.Label(string.Format("{0}x{1}x{2} ({3})", Screen.width, Screen.height, Screen.currentResolution.refreshRate, Utils.GetCommonAspectText((float)Screen.width / Screen.height)));

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

    void OnConnectedToServer()
    {
        Network.Instantiate(m_NetworkPrefab, m_NetworkPrefab.transform.position, m_NetworkPrefab.transform.rotation, 0);
    }
}
