using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class PluginsFolderRedirect
{
    // EDF - This component will redirect the Plugins folder to load C dlls from the Assets\Plugins folder
    //       On Windows, default behavior is to load C dlls from the Unity Editor installed folder, which
    //       is not what you want for your app.
    //       This calls the windows SetDllDirectory() to add Assets\Plugins to the dll search path.
    //       Note that C# dlls will load from Assets\Plugins without needing this redirect code.
    //       References:
    //       http://msdn.microsoft.com/en-us/library/windows/desktop/ms686203(v=vs.85).aspx
    //       https://confluence.ict.usc.edu/display/com/1079+-+External+.dlls
    //       https://jira.ict.usc.edu/browse/VH-164
    //       https://jira.ict.usc.edu/browse/VH-418

#if !UNITY_IPHONE && !UNITY_ANDROID && !UNITY_STANDALONE_OSX
    #region Imported Functions
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetDllDirectory(string lpPathName);
    #endregion
#endif

    static bool m_pluginsFolderRedirected = false;

    public static bool PluginsFolderRedirected { get { return m_pluginsFolderRedirected; } }

    public static bool RedirectPluginsFolder()
    {
#if !UNITY_IPHONE && !UNITY_ANDROID && !UNITY_STANDALONE_OSX
        if ( !PluginsFolderRedirected )
        {
            // EDF - Unity Support #1184, this is one way of detecting if we're running Free or Pro.
            //       Of course, this will also fail if the video card doesn't support shadows.  I guess we'll take that risk.
            if ( SystemInfo.supportsShadows )
            {
                // in unity editor = /Assets/Plugins
                // in unity player = /App_Data/Plugins
                // dataPath points to both correctly
                string path = Application.dataPath + "/Plugins";
                bool successfullySetDllDirectory = SetDllDirectory(path);
                if (!successfullySetDllDirectory)
                {
                    Debug.LogError(@"SetDllDirectory(""Assets/Plugins"") failed.  None of your dlls will work");
                    return false;
                }
            }

            m_pluginsFolderRedirected = true;
        }
#endif
        return true;
    }
}
