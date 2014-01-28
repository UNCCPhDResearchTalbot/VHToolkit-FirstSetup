using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class SmartbodyExternals
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SmartbodyCharacterWrapper
    {
        public IntPtr m_name;
        public float x;
        public float y;
        public float z;
        public float rw;
        public float rx;
        public float ry;
        public float rz;
        public uint m_numJoints;
        public IntPtr jname;
        public IntPtr jx;
        public IntPtr jy;
        public IntPtr jz;
        public IntPtr jrw;
        public IntPtr jrx;
        public IntPtr jry;
        public IntPtr jrz;
    }

    public delegate int OnCharacterCreateCallback(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string objectClass);
    public delegate int OnCharacterDeleteCallback(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name);
    public delegate int OnCharacterChangeCallback(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name);
    public delegate int OnVisemeCallback(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string visemeName, float weight, float blendTime);
    public delegate int OnChannelCallback(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string channelName, float value);


#if UNITY_IPHONE
    public const string DLLIMPORT_NAME = "__Internal";
#else
    public const string DLLIMPORT_NAME = "vhwrapper";
#endif


    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern IntPtr WRAPPER_SBM_CreateSBM(bool releaseMode);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_SetMediaPath(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string path);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_SetSpeechAudiofileBasePath(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string basePath);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_SetProcessId(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string processId);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_Init(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string pythonLibPath, bool logToFile);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_Shutdown(IntPtr sbmID);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_LoadSkeleton(IntPtr sbmID, IntPtr data, int sizeBytes, [MarshalAs(UnmanagedType.LPStr)]string skeletonName);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_LoadMotion(IntPtr sbmID, IntPtr data, int sizeBytes, [MarshalAs(UnmanagedType.LPStr)]string motionName);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_MapSkeleton(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string mapName, [MarshalAs(UnmanagedType.LPStr)]string skeletonName);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_MapMotion(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string mapName, [MarshalAs(UnmanagedType.LPStr)]string motionName);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_SetListener(IntPtr sbmID, OnCharacterCreateCallback charCreateCB, OnCharacterDeleteCallback charDeleteCB, OnCharacterChangeCallback charChangeCB, OnVisemeCallback visemeCB, OnChannelCallback channelCB);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_Update(IntPtr sbmID, double timeInSeconds);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern void WRAPPER_SBM_SetDebuggerId( IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string id );

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern void WRAPPER_SBM_SetDebuggerCameraValues( IntPtr sbmID, double x, double y, double z, double rx, double ry, double rz, double rw, double fov, double aspect, double zNear, double zFar );

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern void WRAPPER_SBM_SetDebuggerRendererRightHanded( IntPtr sbmID, bool enabled );

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_ProcessVHMsgs(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string op, [MarshalAs(UnmanagedType.LPStr)]string args);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern int WRAPPER_SBM_GetNumberOfCharacters(IntPtr sbmID);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_InitCharacter(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name, ref SmartbodyCharacterWrapper character);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_GetCharacter(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string name, ref SmartbodyCharacterWrapper character);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_ReleaseCharacter(ref SmartbodyCharacterWrapper character);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsCharacterCreated(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder objectClass, int maxObjectClassLen);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsLogMessageWaiting(IntPtr sbmID, StringBuilder logMessage, int maxLogMessageLen, ref int logMessageType);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsCharacterDeleted(IntPtr sbmID, StringBuilder name, int maxNameLen);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsCharacterChanged(IntPtr sbmID, StringBuilder name, int maxNameLen);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsVisemeSet(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder visemeName, int maxVisemeNameLen, ref float weight, ref float blendTime);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_IsChannelSet(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder channelName, int maxChannelNameLen, ref float value);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern void WRAPPER_SBM_PythonCommandVoid(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string command);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern bool WRAPPER_SBM_PythonCommandBool(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string command);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern int WRAPPER_SBM_PythonCommandInt(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string command);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern float WRAPPER_SBM_PythonCommandFloat(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string command);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    private static extern IntPtr WRAPPER_SBM_PythonCommandString(IntPtr sbmID, [MarshalAs(UnmanagedType.LPStr)]string command, StringBuilder output, int capacity);

    /*
        messageType
        0 = Unity Normal Print Out
        1 = Unity Error Print Out
        2 = Unity Warning Print Out
    */
    public delegate void LogMessageCallback([MarshalAs(UnmanagedType.LPStr)]string message, int messageType);

    [DllImport(DLLIMPORT_NAME, SetLastError = true)]
    public static extern void WRAPPER_SBM_SetLogMessageCallback(LogMessageCallback logMessageCB);




    public static IntPtr CreateSBM(bool releaseMode)
    {
        // if (Application.platform != RuntimePlatform.OSXEditor)

        return WRAPPER_SBM_CreateSBM(releaseMode);
    }

    public static bool SetMediaPath(IntPtr sbmID, string path)
    {
        return WRAPPER_SBM_SetMediaPath(sbmID, path);
    }

    public static bool SetSpeechAudiofileBasePath(IntPtr sbmID, string basePath)
    {
        return WRAPPER_SBM_SetSpeechAudiofileBasePath(sbmID, basePath);
    }

    public static bool SetProcessId(IntPtr sbmID, string processId)
    {
        return WRAPPER_SBM_SetProcessId(sbmID, processId);
    }

    public static bool Init(IntPtr sbmID, string pythonLibPath, bool logToFile)
    {
        return WRAPPER_SBM_Init(sbmID, pythonLibPath, logToFile);
    }

    public static bool Shutdown(IntPtr sbmID)
    {
        return WRAPPER_SBM_Shutdown(sbmID);
    }

    public static bool LoadSkeleton(IntPtr sbmID, IntPtr data, int sizeBytes, string skeletonName)
    {
        return WRAPPER_SBM_LoadSkeleton(sbmID, data, sizeBytes, skeletonName);
    }

    public static bool LoadMotion(IntPtr sbmID, IntPtr data, int sizeBytes, string motionName)
    {
        return WRAPPER_SBM_LoadMotion(sbmID, data, sizeBytes, motionName);
    }

    public static bool MapSkeleton(IntPtr sbmID, string mapName, string skeletonName)
    {
        return WRAPPER_SBM_MapSkeleton(sbmID, mapName, skeletonName);
    }

    public static bool MapMotion(IntPtr sbmID, string mapName, string motionName)
    {
        return WRAPPER_SBM_MapMotion(sbmID, mapName, motionName);
    }

    public static bool SetListener(IntPtr sbmID, OnCharacterCreateCallback charCreateCB, OnCharacterDeleteCallback charDeleteCB, OnCharacterChangeCallback charChangeCB, OnVisemeCallback visemeCB, OnChannelCallback channelCB)
    {
        return WRAPPER_SBM_SetListener(sbmID, charCreateCB, charDeleteCB, charChangeCB, visemeCB, channelCB);
    }

    public static bool Update(IntPtr sbmID, double timeInSeconds)
    {
        return WRAPPER_SBM_Update(sbmID, timeInSeconds);
    }

    public static void SetDebuggerId(IntPtr sbmID, string id)
    {
        WRAPPER_SBM_SetDebuggerId(sbmID, id);
    }

    public static void SetDebuggerCameraValues(IntPtr sbmID, double x, double y, double z, double rx, double ry, double rz, double rw, double fov, double aspect, double zNear, double zFar)
    {
        WRAPPER_SBM_SetDebuggerCameraValues(sbmID, x, y, z, rx, ry, rz, rw, fov, aspect, zNear, zFar);
    }

    public static void SetDebuggerRendererRightHanded(IntPtr sbmID, bool enabled)
    {
        WRAPPER_SBM_SetDebuggerRendererRightHanded(sbmID, enabled);
    }

    public static bool ProcessVHMsgs(IntPtr sbmID, string op, string args)
    {
        return WRAPPER_SBM_ProcessVHMsgs(sbmID, op, args);
    }

    public static int GetNumberOfCharacters(IntPtr sbmID)
    {
        return WRAPPER_SBM_GetNumberOfCharacters(sbmID);
    }

    public static bool InitCharacter(IntPtr sbmID, string name, ref SmartbodyCharacterWrapper character)
    {
        return WRAPPER_SBM_InitCharacter(sbmID, name, ref character);
    }

    public static bool GetCharacter(IntPtr sbmID, string name, ref SmartbodyCharacterWrapper character)
    {
        return WRAPPER_SBM_GetCharacter(sbmID, name, ref character);
    }

    public static bool ReleaseCharacter(ref SmartbodyCharacterWrapper character)
    {
        return WRAPPER_SBM_ReleaseCharacter(ref character);
    }

    public static bool IsCharacterCreated(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder objectClass, int maxObjectClassLen)
    {
        return WRAPPER_SBM_IsCharacterCreated(sbmID, name, maxNameLen, objectClass, maxObjectClassLen);
    }

    public static bool IsLogMessageWaiting(IntPtr sbmID, StringBuilder logMessage, int maxLogMessageLen, ref int logMessageType)
    {
        return WRAPPER_SBM_IsLogMessageWaiting(sbmID, logMessage, maxLogMessageLen, ref logMessageType);
    }

    public static bool IsCharacterDeleted(IntPtr sbmID, StringBuilder name, int maxNameLen)
    {
        return WRAPPER_SBM_IsCharacterDeleted(sbmID, name, maxNameLen);
    }

    public static bool IsCharacterChanged(IntPtr sbmID, StringBuilder name, int maxNameLen)
    {
        return WRAPPER_SBM_IsCharacterChanged(sbmID, name, maxNameLen);
    }

    public static bool IsVisemeSet(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder visemeName, int maxVisemeNameLen, ref float weight, ref float blendTime)
    {
        return WRAPPER_SBM_IsVisemeSet(sbmID, name, maxNameLen, visemeName, maxVisemeNameLen, ref weight, ref blendTime);
    }

    public static bool IsChannelSet(IntPtr sbmID, StringBuilder name, int maxNameLen, StringBuilder channelName, int maxChannelNameLen, ref float value)
    {
        return WRAPPER_SBM_IsChannelSet(sbmID, name, maxNameLen, channelName, maxChannelNameLen, ref value);
    }

    public static void PythonCommandVoid(IntPtr sbmID, string command)
    {
        WRAPPER_SBM_PythonCommandVoid(sbmID, command);
    }

    public static bool PythonCommandBool(IntPtr sbmID, string command)
    {
        return WRAPPER_SBM_PythonCommandBool(sbmID, command);
    }

    public static int PythonCommandInt(IntPtr sbmID, string command)
    {
        return WRAPPER_SBM_PythonCommandInt(sbmID, command);
    }

    public static float PythonCommandFloat(IntPtr sbmID, string command)
    {
        return WRAPPER_SBM_PythonCommandFloat(sbmID, command);
    }

    public static IntPtr PythonCommandString(IntPtr sbmID, string command, StringBuilder output, int capacity)
    {
        return WRAPPER_SBM_PythonCommandString(sbmID, command, output, capacity);
    }
}
