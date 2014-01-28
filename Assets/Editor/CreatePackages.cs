    using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;


public class CreatePackages
{
    [MenuItem("VH/Build/Create vhAssets Package")]
    static void MenuCreateVHAssetsPackage()
    {
        CreatePackages.CreateVHAssetsPackage();
    }

    [MenuItem("VH/Build/Create vhAssets Test Scene Package")]
    static void MenuCreateVHAssetsTestScenePackage()
    {
        CreatePackages.CreateVHAssetsTestScenePackage();
    }

    [MenuItem("VH/Build/Create vhAssets Package OSX")]
    static void MenuCreateVHAssetsPackageOSX()
    {
        CreatePackages.CreateVHAssetsPackageOSX();
    }

    [MenuItem("VH/Build/Create vhAssets Package IOS")]
    static void MenuCreateVHAssetsPackageIOS()
    {
        CreatePackages.CreateVHAssetsPackageIOS();
    }

    [MenuItem("VH/Build/Create vhMsgEmulator App")]
    static void MenuCreateVHMsgEmulatorApp()
    {
        CreatePackages.CreateVHMsgEmulatorApp();
    }


    public static void CreateVHAssetsTestScenePackage()
    {
        List<string> filesToPack = new List<string>();
        filesToPack.AddRange(Directory.GetFiles("Assets/Art", "*.*", SearchOption.AllDirectories));
        filesToPack.AddRange(Directory.GetFiles("Assets/Editor", "*.*", SearchOption.AllDirectories));
        filesToPack.AddRange(Directory.GetFiles("Assets/Mecanim", "*.*", SearchOption.AllDirectories));
        filesToPack.AddRange(Directory.GetFiles("Assets/Scenes", "*.*", SearchOption.AllDirectories));
        filesToPack.AddRange(Directory.GetFiles("Assets/Scripts", "*.*", SearchOption.AllDirectories));
        filesToPack.AddRange(Directory.GetFiles("Assets/StreamingAssets", "*.*", SearchOption.AllDirectories));

        CreateAssetPackage(filesToPack.ToArray(), "vhAssetsTestScenePackage.unityPackage", ExportPackageOptions.Recurse);
    }

    public static void CreateVHAssetsPackage()
    {
        List<string> filesToPack = new List<string>();
        filesToPack.Add("Assets/Plugins/activemq-cpp.dll");
        filesToPack.Add("Assets/Plugins/activemq-cpp.dll.meta");
        filesToPack.Add("Assets/Plugins/alut.dll");
        filesToPack.Add("Assets/Plugins/alut.dll.meta");
        filesToPack.Add("Assets/Plugins/Apache.NMS.ActiveMQ.dll");
        filesToPack.Add("Assets/Plugins/Apache.NMS.ActiveMQ.dll.meta");
        filesToPack.Add("Assets/Plugins/Apache.NMS.dll");
        filesToPack.Add("Assets/Plugins/Apache.NMS.dll.meta");
        filesToPack.Add("Assets/Plugins/blat.dll");
        filesToPack.Add("Assets/Plugins/blat.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_filesystem-vc100-mt-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_filesystem-vc100-mt-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_python-vc100-mt-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_python-vc100-mt-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_regex-vc100-mt-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_regex-vc100-mt-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_system-vc100-mt-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_system-vc100-mt-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/dbghelp.dll");
        filesToPack.Add("Assets/Plugins/dbghelp.dll.meta");
        filesToPack.Add("Assets/Plugins/glew32.dll");
        filesToPack.Add("Assets/Plugins/glew32.dll.meta");
        filesToPack.Add("Assets/Plugins/Ionic.Zlib.dll");
        filesToPack.Add("Assets/Plugins/Ionic.Zlib.dll.meta");
        filesToPack.Add("Assets/Plugins/libapr-1.dll");
        filesToPack.Add("Assets/Plugins/libapr-1.dll.meta");
        filesToPack.Add("Assets/Plugins/libapriconv-1.dll");
        filesToPack.Add("Assets/Plugins/libapriconv-1.dll.meta");
        filesToPack.Add("Assets/Plugins/libaprutil-1.dll");
        filesToPack.Add("Assets/Plugins/libaprutil-1.dll.meta");
        filesToPack.Add("Assets/Plugins/libsndfile-1.dll");
        filesToPack.Add("Assets/Plugins/libsndfile-1.dll.meta");
        filesToPack.Add("Assets/Plugins/msvcp100.dll");
        filesToPack.Add("Assets/Plugins/msvcp100.dll.meta");
        filesToPack.Add("Assets/Plugins/msvcr100.dll");
        filesToPack.Add("Assets/Plugins/msvcr100.dll.meta");
        filesToPack.Add("Assets/Plugins/OpenAL32.dll");
        filesToPack.Add("Assets/Plugins/OpenAL32.dll.meta");
        filesToPack.Add("Assets/Plugins/pprAI.dll");
        filesToPack.Add("Assets/Plugins/pprAI.dll.meta");
        filesToPack.Add("Assets/Plugins/pthreadVSE2.dll");
        filesToPack.Add("Assets/Plugins/pthreadVSE2.dll.meta");
        filesToPack.Add("Assets/Plugins/python27.dll");
        filesToPack.Add("Assets/Plugins/python27.dll.meta");
        filesToPack.Add("Assets/Plugins/SmartBody.dll");
        filesToPack.Add("Assets/Plugins/SmartBody.dll.meta");
        filesToPack.Add("Assets/Plugins/SmartBody.pdb");
        filesToPack.Add("Assets/Plugins/SmartBody.pdb.meta");
        filesToPack.Add("Assets/Plugins/steerlib.dll");
        filesToPack.Add("Assets/Plugins/steerlib.dll.meta");
        filesToPack.Add("Assets/Plugins/vhmsg-net.dll");
        filesToPack.Add("Assets/Plugins/vhmsg-net.dll.meta");
        filesToPack.Add("Assets/Plugins/vhwrapper.dll");
        filesToPack.Add("Assets/Plugins/vhwrapper.dll.meta");
        filesToPack.Add("Assets/Plugins/wrap_oal.dll");
        filesToPack.Add("Assets/Plugins/wrap_oal.dll.meta");
        filesToPack.Add("Assets/Plugins/xerces-c_3_1.dll");
        filesToPack.Add("Assets/Plugins/xerces-c_3_1.dll.meta");

        filesToPack.Add("Assets/Plugins/SmartBody_d.dll");
        filesToPack.Add("Assets/Plugins/SmartBody_d.dll.meta");
        filesToPack.Add("Assets/Plugins/SmartBody_d.pdb");
        filesToPack.Add("Assets/Plugins/SmartBody_d.pdb.meta");
        filesToPack.Add("Assets/Plugins/steerlibd.dll");
        filesToPack.Add("Assets/Plugins/steerlibd.dll.meta");
        filesToPack.Add("Assets/Plugins/pprAId.dll");
        filesToPack.Add("Assets/Plugins/pprAId.dll.meta");
        filesToPack.Add("Assets/Plugins/activemq-cppd.dll");
        filesToPack.Add("Assets/Plugins/activemq-cppd.dll.meta");
        filesToPack.Add("Assets/Plugins/xerces-c_3_1D.dll");
        filesToPack.Add("Assets/Plugins/xerces-c_3_1D.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_filesystem-vc100-mt-gd-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_filesystem-vc100-mt-gd-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_system-vc100-mt-gd-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_system-vc100-mt-gd-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_regex-vc100-mt-gd-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_regex-vc100-mt-gd-1_51.dll.meta");
        filesToPack.Add("Assets/Plugins/boost_python-vc100-mt-gd-1_51.dll");
        filesToPack.Add("Assets/Plugins/boost_python-vc100-mt-gd-1_51.dll.meta");

        filesToPack.AddRange(Directory.GetFiles("Assets/vhAssets", "*.*", SearchOption.AllDirectories));

        CreateAssetPackage(filesToPack.ToArray(), "vhAssetsPackage.unityPackage", ExportPackageOptions.Recurse);
    }

    public static void CreateVHAssetsPackageOSX()
    {
        List<string> filesToPack = new List<string>();

        filesToPack.AddRange(Directory.GetFiles("Assets/Plugins/vhwrapper.bundle", "*.*", SearchOption.AllDirectories));

        filesToPack.Add("Assets/Plugins/Apache.NMS.ActiveMQ.dll");
        filesToPack.Add("Assets/Plugins/Apache.NMS.ActiveMQ.dll.meta");
        filesToPack.Add("Assets/Plugins/Apache.NMS.dll");
        filesToPack.Add("Assets/Plugins/Apache.NMS.dll.meta");
        filesToPack.Add("Assets/Plugins/Ionic.Zlib.dll");
        filesToPack.Add("Assets/Plugins/Ionic.Zlib.dll.meta");
        filesToPack.Add("Assets/Plugins/vhmsg-net.dll");
        filesToPack.Add("Assets/Plugins/vhmsg-net.dll.meta");

        filesToPack.AddRange(Directory.GetFiles("Assets/vhAssets", "*.*", SearchOption.AllDirectories));

        CreateAssetPackage(filesToPack.ToArray(), "vhAssetsPackageOSX.unityPackage", ExportPackageOptions.Recurse);
    }

    public static void CreateVHAssetsPackageIOS()
    {
        List<string> filesToPack = new List<string>();

        filesToPack.AddRange(Directory.GetFiles("Assets/Plugins/iOS", "*.*", SearchOption.AllDirectories));

        filesToPack.AddRange(Directory.GetFiles("Assets/vhAssets", "*.*", SearchOption.AllDirectories));

        CreateAssetPackage(filesToPack.ToArray(), "vhAssetsPackageIOS.unityPackage", ExportPackageOptions.Recurse);
    }

    public static void CreateVHAssetsPackageAndroid()
    {
        List<string> filesToPack = new List<string>();

        filesToPack.AddRange(Directory.GetFiles("Assets/Plugins/Android", "*.*", SearchOption.AllDirectories));

        filesToPack.AddRange(Directory.GetFiles("Assets/vhAssets", "*.*", SearchOption.AllDirectories));

        CreateAssetPackage(filesToPack.ToArray(), "vhAssetsPackageAndroid.unityPackage", ExportPackageOptions.Recurse);
    }

    public static void CreateAssetPackage(string[] filesToPack, string createdPackageName, ExportPackageOptions options)
    {
        try
        {
            AssetDatabase.ExportPackage(filesToPack, createdPackageName, options);
            UnityEngine.Debug.Log("CreateAssetPackage() success: " + createdPackageName);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("CreateAssetPackage failed: " + e.Message);
        }
    }

    public static void CreateVHMsgEmulatorApp()
    {
        BuildPlayer.PerformBuild(BuildTarget.StandaloneWindows, "BuildSettingsVHMsgEmulator.xml");
    }
}
