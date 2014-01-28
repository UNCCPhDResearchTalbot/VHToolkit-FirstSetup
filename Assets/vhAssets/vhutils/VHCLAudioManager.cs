using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class VHCLAudioManager : MonoBehaviour
{
    #region Imported Functions

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern IntPtr WRAPPER_VHCL_AUDIO_CreateAudio();

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_Open(IntPtr handle);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_Close(IntPtr handle);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_SetListenerPos(IntPtr handle, float x, float y, float z);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_GetListenerPos(IntPtr handle, ref float x, ref float y, ref float z);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_SetListenerRot(IntPtr handle, float rx, float ry, float rz, float ux, float uy, float uz);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_GetListenerRot(IntPtr handle, ref float rx, ref float ry, ref float rz,
        ref float ux, ref float uy, ref float uz );

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern /*Sound**/IntPtr WRAPPER_VHCL_AUDIO_CreateSound(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName,
        [MarshalAs(UnmanagedType.LPStr)]string name);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern /*Sound**/IntPtr WRAPPER_VHCL_AUDIO_PlaySound(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName,
        [MarshalAs(UnmanagedType.LPStr)]string name, float posX, float posY, float posZ, bool looping);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_StopSound(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern void WRAPPER_VHCL_AUDIO_PauseAllSounds(IntPtr handle);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern void WRAPPER_VHCL_AUDIO_StopAllSounds(IntPtr handle);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern void WRAPPER_VHCL_AUDIO_UnpauseAllSounds(IntPtr handle);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern /*Sound**/IntPtr WRAPPER_VHCL_AUDIO_CreateSoundLibSndFile(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName,
        [MarshalAs(UnmanagedType.LPStr)]string name);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_DestroySound(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string name);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern /*Sound**/IntPtr WRAPPER_VHCL_AUDIO_FindSound(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string name);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_Update(IntPtr handle, float frameTime);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_AttachSoundToFreeChannel(IntPtr handle, IntPtr sound);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_ReleaseSoundFromChannel(IntPtr handle, IntPtr sound);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_SetSoundHardwareChannel(
        IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName, [MarshalAs(UnmanagedType.LPStr)]string channelName);

    [DllImport("vhwrapper", SetLastError = true)]
    private static extern bool WRAPPER_VHCL_AUDIO_SoundExists(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string fileName);
    #endregion


    Transform m_ListenerTransform;
    IntPtr m_ID;
    static VHCLAudioManager __VHCLAudioManager;


    #region Functions

    public static VHCLAudioManager Get()
    {
        if (__VHCLAudioManager == null)
        {
            __VHCLAudioManager = UnityEngine.Object.FindObjectOfType(typeof(VHCLAudioManager)) as VHCLAudioManager;
        }

        return __VHCLAudioManager;
    }

    public void Awake()
    {
        PluginsFolderRedirect.RedirectPluginsFolder();
    }

    public void Start()
    {
        m_ID = WRAPPER_VHCL_AUDIO_CreateAudio();
        //Debug.Log("Audio m_ID = " + m_ID);
        if (m_ID == new IntPtr(-1))
        {
            Debug.LogError("Failed to CreateAudio()");
        }

        if (WRAPPER_VHCL_AUDIO_Open(m_ID))
        {
            SetListenerTransform(Camera.main.transform);
        }
        else
        {
            Debug.LogError("Failed to open audio manager");
        }
    }

    public void Update()
    {
        SetListenerPos(m_ListenerTransform.position);
        SetListenerRot(-m_ListenerTransform.forward, m_ListenerTransform.up);
    }

    public void SetListenerTransform(Transform t)
    {
        m_ListenerTransform = t;
    }

    public void SetListenerPos(Vector3 pos)
    {
        WRAPPER_VHCL_AUDIO_SetListenerPos(m_ID, pos.x, pos.y, pos.z);
    }

    public void GetListenerPos(ref Vector3 pos)
    {
        WRAPPER_VHCL_AUDIO_GetListenerPos(m_ID, ref pos.x, ref pos.y, ref pos.z);
    }

    public void SetListenerRot(Vector3 atVec, Vector3 upVec)
    {
        WRAPPER_VHCL_AUDIO_SetListenerRot(m_ID, atVec.x, atVec.y, atVec.z, upVec.x, upVec.y, upVec.z);
    }

    public void GetListenerRot(ref Vector3 r, ref Vector3 u)
    {
        WRAPPER_VHCL_AUDIO_GetListenerRot(m_ID, ref r.x, ref r.y, ref r.z, ref u.x, ref u.y, ref u.z);
    }

    public void CreateSound(string filename, string name)
    {
        WRAPPER_VHCL_AUDIO_CreateSound(m_ID, filename, name);
    }

    public void CreateLibSoundFile(string filename, string name)
    {
        WRAPPER_VHCL_AUDIO_CreateSoundLibSndFile(m_ID, filename, name);
    }

    public void PlaySound(string filename, string name, Vector3 pos, bool looping)
    {
        WRAPPER_VHCL_AUDIO_PlaySound(m_ID, filename, name, pos.x, pos.y, pos.z, looping);
    }

    public void StopSound(string filename)
    {
        WRAPPER_VHCL_AUDIO_StopSound(m_ID, filename);
    }

    public void PauseAllSounds()
    {
        WRAPPER_VHCL_AUDIO_PauseAllSounds(m_ID);
    }

    public void StopAllSounds()
    {
        WRAPPER_VHCL_AUDIO_StopAllSounds(m_ID);
    }

    public void UnpauseAllSounds()
    {
        WRAPPER_VHCL_AUDIO_UnpauseAllSounds(m_ID);
    }

    public void SetSoundHardwareChannel(string filename, string channelName)
    {
        WRAPPER_VHCL_AUDIO_SetSoundHardwareChannel(m_ID, filename, channelName);
    }

    public bool SoundExists(string filename)
    {
        return WRAPPER_VHCL_AUDIO_SoundExists(m_ID, filename);
    }
    #endregion

    #region Unity Message Handlers
    public void OnApplicationQuit()
    {
        if (!enabled)
        {
            return;
        }
        if (WRAPPER_VHCL_AUDIO_Close(m_ID))
        {
            //Debug.Log("audio successfully shutdown");
        }
        else
        {
            Debug.LogError("VHCLAudioManager failed to shutdown");
        }

        m_ID = new IntPtr(-1);
    }
    #endregion
}
