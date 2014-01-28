using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;


public class BuildPlayer
{
    [XmlRootAttribute("BuildSettings", Namespace = "http://www.cpandl.com", IsNullable = false)]
    public class ProjectSettings
    {
        public string[] Scenes;
        public string   BuildOutputPath;
        public string[] ExternalAssetsPaths;
        public string[] ConfigFiles;
        public string   PostBuildScript;
    }


    // data for build specific settings
    const string SettingsFileName = "BuildSettings.xml";


    [MenuItem("VH/Build/Perform Windows Build")]
    static void MenuPerformWindowsBuild()
    {
        PerformWindowsBuild();
    }

    [MenuItem("VH/Build/Perform OSX Build")]
    static void MenuPerformOSXBuild()
    {
        PerformOSXBuild();
    }

    [MenuItem("VH/Build/Perform iOS Build")]
    static void MenuPerformiOSBuild()
    {
        PerformiOSBuild();
    }

    [MenuItem("VH/Build/Perform Android Build")]
    static void MenuPerformAndroidBuild()
    {
        PerformAndroidBuild();
    }

    [MenuItem("VH/Build/Perform Web Build")]
    static void MenuPerformWebBuild()
    {
        PerformAndroidBuild();
    }


    public static void PerformWindowsBuild()
    {
        PerformBuild(BuildTarget.StandaloneWindows);
    }

    public static void PerformOSXBuild()
    {
        PerformBuild(BuildTarget.StandaloneOSXIntel);
    }

    public static void PerformiOSBuild()
    {
        PerformBuild(BuildTarget.iPhone);
    }

    public static void PerformAndroidBuild()
    {
        PerformBuild(BuildTarget.Android);
    }

    public static void PerformWebBuild()
    {
        PerformBuild(BuildTarget.WebPlayer);
    }

    public static void PerformBuild(BuildTarget buildTarget)
    {
        PerformBuild(buildTarget, SettingsFileName);
    }

    public static void PerformBuild(BuildTarget buildTarget, string buildSettingsFile)
    {
        FileStream fs = null;
        ProjectSettings configFile = null;
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectSettings));
            fs = new FileStream(Application.dataPath + "/" + buildSettingsFile, FileMode.Open);
            configFile = (ProjectSettings)serializer.Deserialize(fs);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to open: " + buildSettingsFile + " " + e.Message);
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
        }


        string dataPathNoAssets = Application.dataPath;
        if (dataPathNoAssets.EndsWith("Assets"))
        {
            dataPathNoAssets = dataPathNoAssets.Remove(dataPathNoAssets.Length - "Assets".Length);
        }


        // the path where the build will reside
        string locationPathName = string.Empty;
        string executableName = string.Empty;
        if (configFile != null && !string.IsNullOrEmpty(configFile.BuildOutputPath))
        {
            locationPathName = configFile.BuildOutputPath;
            switch (buildTarget)
            {
                case BuildTarget.Android:            locationPathName += ".apk"; break;
                case BuildTarget.iPhone:             locationPathName += ".ios.app"; break;
                case BuildTarget.StandaloneOSXIntel: locationPathName += ".app"; break;
                case BuildTarget.WebPlayer:          locationPathName += ".web"; break;
                case BuildTarget.WebPlayerStreamed:  locationPathName += ".web"; break;
                default:                             locationPathName += ".exe"; break;
            }
            executableName = Path.GetFileName(locationPathName);
        }
        else
        {
            // it's not specified in the Settings file, default it
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    executableName = PlayerSettings.productName + ".apk";
                    break;
                case BuildTarget.iPhone:
                case BuildTarget.StandaloneOSXIntel:
                    executableName = PlayerSettings.productName + ".app";
                    break;
                default:
                    executableName = PlayerSettings.productName + ".exe";
                    break;
            }
            locationPathName = dataPathNoAssets + PlayerSettings.productName + "_Build/" + executableName;
        }


        // check which scenes are being used and build them
        List<string> levels = new List<string>();
        if (configFile.Scenes == null || configFile.Scenes.Length == 0)
        {
            // use the scenes that have been specified in the Unity Build Settings dialog
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    levels.Add(EditorBuildSettings.scenes[i].path);
                    UnityEngine.Debug.Log(EditorBuildSettings.scenes[i].path);
                }
            }
        }
        else
        {
            // use the scenes specified in the settings xml file
            levels.AddRange(configFile.Scenes);
        }


        // On XP, BuildPlayer() will fail if the dest folder doesn't exist VH-218
        //    Error is: Cancelling DisplayDialogComplex: Moving file failed Moving Temp/StagingArea/Data to ../../bin/GSUnity\GSUnity_Data
        Directory.CreateDirectory(Path.GetDirectoryName(locationPathName));


        UnityEngine.Debug.Log("PerformBuild() - BuildPipeline.BuildPlayer() - " + locationPathName);

        string errorMessage = BuildPipeline.BuildPlayer(levels.ToArray(), locationPathName, buildTarget, BuildOptions.None);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            UnityEngine.Debug.Log(errorMessage);
        }


        string destinationLocation = locationPathName.Replace(executableName, "");


        // if Unity is in svn, then BuildPlayer copies over .svn hidden folders. things like:
        // main\main_Data\Managed\.svn
        // main\main_Data\Mono\etc\.svn
        // so go through the data folder and remove these.
        List<DirectoryInfo> dirlist = new List<DirectoryInfo>();
        dirlist.Add(new DirectoryInfo(destinationLocation));
        while (dirlist.Count > 0)
        {
            if (dirlist[0].Exists)
            {
                dirlist.AddRange(dirlist[0].GetDirectories());

                if (dirlist[0].Name == ".svn")
                {
                    Utils.ClearAttributesRecursive(dirlist[0].FullName);
                    dirlist[0].Delete(true);
                }
            }

            dirlist.RemoveAt(0);
        }


        // copy any folders that need to stay unbundled
        if (configFile != null && configFile.ExternalAssetsPaths != null)
        {
            for (int i = 0; i < configFile.ExternalAssetsPaths.Length; i++)
            {
                if (!string.IsNullOrEmpty(configFile.ExternalAssetsPaths[i]))
                {
                    string sourcePath = dataPathNoAssets + configFile.ExternalAssetsPaths[i];

                    UnityEngine.Debug.Log("PerformBuild() - Copying: '" + sourcePath + "' to '" + destinationLocation + configFile.ExternalAssetsPaths[i] + "'");

                    Utils.CopyFolderWithFolderExclusion(sourcePath, destinationLocation + configFile.ExternalAssetsPaths[i], ".svn", true, true);
                }
            }
        }

        // copy over any config files that they have
        if (configFile != null && configFile.ConfigFiles != null)
        {
            for (int i = 0; i < configFile.ConfigFiles.Length; i++)
            {
                if (!string.IsNullOrEmpty(configFile.ConfigFiles[i]))
                {
                    string sourcePath = dataPathNoAssets + configFile.ConfigFiles[i];
                    string destinationDirectoryName = Path.GetDirectoryName(destinationLocation + configFile.ConfigFiles[i]);

                    UnityEngine.Debug.Log("PerformBuild() - destinationDirectoryName: " + destinationDirectoryName);
                    if (!Directory.Exists(destinationDirectoryName))
                    {
                        Directory.CreateDirectory((destinationDirectoryName));
                    }

                    UnityEngine.Debug.Log("PerformBuild() - Copying: '" + sourcePath + "' to '" + destinationLocation + configFile.ConfigFiles[i] + "'");

                    File.Copy(sourcePath, destinationLocation + configFile.ConfigFiles[i], true);
                }
            }
        }


        // trigger any post-build scripts
        if (configFile != null && !string.IsNullOrEmpty(configFile.PostBuildScript))
        {
            string buildType = "Release";
            //string buildType = Debug.isDebugBuild or EditorUserBuildSettings.development or EditorUserBuildSettings.allowDebugging?
            string projectRoot = EditorApplication.applicationPath.Replace("Unity.exe", "") + @"..\..\..";
            projectRoot = projectRoot.Replace(@"/", @"\");
            string destinationLocationModified = destinationLocation.Replace(@"/", @"\") + @"Assets\Plugins";
            UnityEngine.Debug.Log("PerformBuild() - calling " + configFile.PostBuildScript + " " + buildType + " " + projectRoot + " " + destinationLocationModified);
            Process p = Process.Start(configFile.PostBuildScript, buildType + " " + projectRoot + " " + destinationLocationModified);
            p.WaitForExit();
        }
    }
}
