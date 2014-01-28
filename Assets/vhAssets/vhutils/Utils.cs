using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

static public class Utils
{
    public enum AudioPlayType
    {
        Unity,              // plays a pre-loaded audio clip through the unity sound system
        UnityResourceLoad,  // calls Resouce.Load to load the clip, then plays it through the unity sound system
        VHCL,               // plays a sound through VHCL::Audio using vhwrapper.dll

        NUM_AUDIO_PLAY_TYPES
    }

    static public GameObject FindChild(GameObject pRoot, string pName)
    {
        Transform child = pRoot.transform.Find(pName);
        return child != null ? child.gameObject : null;
    }

    static public GameObject FindChildRecursive(GameObject root, string name)
    {
        if (root.name == name)
            return root;

        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject found = FindChildRecursive(root.transform.GetChild(i).gameObject, name);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Network instantiates a VHBehaviour to be managed by the VHBehaviourManager
    /// </summary>
    /// <returns>
    [Obsolete("Please report to vh-support if you are using this function")]
    static public T VHNetworkInstantiate<T>(T prefab, Vector3 position, Quaternion rotation, int group) where T : Component
    {
        T retVal = (T)Network.Instantiate(prefab, position, rotation, group);

        if (retVal == null)
        {
            Debug.Log("VHInstantiate ERROR");
        }

        if (retVal is VHBehaviour)
        {
            VHBehaviour vhBehavior = retVal as VHBehaviour;
            vhBehavior.Activate();
        }
        return retVal;
    }

    /// <summary>
    /// Instantiates the prefab that is passed in, unless it already exists in the scene (hierachy view)
    /// </summary>
    /// <typeparam name="T">The type of unity component that the prefab's gameobject contains</typeparam>
    /// <param name="prefab">The component to instantiate, if it already exists in the scene, returns itself</param>
    /// <param name="prefabTagName">The tag name of the prefab so that the scene can be searched in case prefab is null</param>
    /// <param name="prefabResoucePath">The directory location and prefab name that will be loaded if prefab is null and the tag search fails</param>
    /// <returns>The created prefab, null is failure</returns>
    [Obsolete("Avoid using this function and do some sort of Find() instead")]
    static public T VHInstantiate<T>(T prefab, string prefabResoucePath) where T : Component
    {
        return VHInstantiate<T>(prefab, prefabResoucePath, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Creates the prefab from the specified resource path
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabResoucePath"></param>
    /// <returns></returns>
    [Obsolete("Avoid using this function and do some sort of Find() instead")]
    static public T VHInstantiate<T>(string prefabResoucePath) where T : Component
    {
        return VHInstantiate<T>(null, prefabResoucePath, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Creates the prefab from the specified resource path at the specified world position and rotation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabResoucePath"></param>
    /// <param name="pos">world position</param>
    /// <param name="rot">world rotation</param>
    /// <returns></returns>
    [Obsolete("Avoid using this function and do some sort of Find() instead")]
    static public T VHInstantiate<T>(string prefabResoucePath, Vector3 pos, Quaternion rot) where T : Component
    {
        return VHInstantiate<T>(null, prefabResoucePath, pos, rot);
    }

    /// <summary>
    /// Instantiates the prefab that is passed in, unless it already exists in the scene (hierachy view)
    /// </summary>
    /// <typeparam name="T">The type of unity component that the prefab's gameobject contains</typeparam>
    /// <param name="prefab">The component to instantiate, if it already exists in the scene, returns itself</param>
    /// <param name="prefabTagName">The tag name of the prefab so that the scene can be searched in case prefab is null</param>
    /// <param name="prefabResoucePath">The directory location and prefab name that will be loaded if prefab is null and the tag search fails</param>
    /// <param name="pos">World position where the prefab will spawn</param>
    /// <param name="rot">World rotation of the instatiated prefab</param>
    /// <returns>The created prefab, null is failure</returns>
    [Obsolete("Avoid using this function and do some sort of Find() instead")]
    static public T VHInstantiate<T>(T prefab, string prefabResoucePath,
        Vector3 pos, Quaternion rot) where T : Component
    {
        T retVal = default(T);

        T[] obj = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
        if (obj != null)
        {
            for (int i = 0; i < obj.Length; i++)
            {
#if UNITY_EDITOR
                if (typeof(T) == obj[i].GetType() && UnityEditor.PrefabUtility.GetPrefabType(obj[i]) != UnityEditor.PrefabType.Prefab)
#else
                if (typeof(T) == obj[i].GetType())
#endif
                {
                    retVal = obj[i];
                    break;
                }
            }
        }

        if (retVal == null && prefab != null)
        {
            // they have correctly set the prefab, instantiate it
            retVal = (T)GameObject.Instantiate(prefab, pos, rot);
        }

        // try on last time to load if nothing else worked
        if (retVal == null && prefabResoucePath != null)
        {
            // they never set the prefab variable in the script, there is no object in the scene (hierarhy view) with
            // the tag prefabTagName, so try to load the prefab from the resources folder based on the path they passed in
            retVal = (T)GameObject.Instantiate((Resources.Load(prefabResoucePath, typeof(T))));

            if (retVal == null)
            {
                Debug.LogError("VHInstantiate ERROR: There is no prefab located under your projects Assets/Resources " +
                    "folder with path and name: " + prefabResoucePath);
            }
        }

        if (retVal == null)
        {
            Debug.Log("VHInstantiate ERROR");
        }

        if (retVal is VHBehaviour)
        {
            VHBehaviour vhBehavior = retVal as VHBehaviour;
            vhBehavior.Activate();
        }

        return retVal;
    }

    public static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
    {
#if !UNITY_WEBPLAYER
        // taken from "How to: Copy Directories" http://msdn.microsoft.com/en-us/library/bb762914.aspx
        // modified to include overwrite flag

        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
        }

        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }


        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destDirName, file.Name);

            // Copy the file.
            file.CopyTo(temppath, overwrite);
        }

        // If copySubDirs is true, copy the subdirectories.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                CopyFolder(subdir.FullName, temppath, copySubDirs, overwrite);
            }
        }
#endif
    }


    public static void CopyFolderWithFolderExclusion(string sourceDirName, string destDirName, string excludeString, bool copySubDirs, bool overwrite)
    {
#if !UNITY_WEBPLAYER
        // TODO - merge this into CopyFolder
        // TODO - give it a true search pattern for excludeString
        // TODO - allow passing in a list of exclusion patterns
        // TODO - maybe write a separate function that allows you to exclude files

        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
        }


        if (dir.FullName.Contains(excludeString))
        {
            return;
        }


        // If the destination directory does not exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }


        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach (FileInfo file in files)
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine(destDirName, file.Name);

            // Copy the file.
            file.CopyTo(temppath, overwrite);
        }

        // If copySubDirs is true, copy the subdirectories.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destDirName, subdir.Name);

                // Copy the subdirectories.
                CopyFolderWithFolderExclusion(subdir.FullName, temppath, excludeString, copySubDirs, overwrite);
            }
        }
#endif
    }


    public static void CopyFiles(string[] sourceLocations, string destinationLocation)
    {
        for (int i = 0; i < sourceLocations.Length; i++)
        {
            if (File.Exists(sourceLocations[i]))
            {
                File.Copy(sourceLocations[i], destinationLocation + Path.GetFileName(sourceLocations[i]), true);
            }
        }
    }

    public static void CopyFiles(string sourceLocation, string destinationLocation, string searchPattern, SearchOption option)
    {
        string[] files = Directory.GetFiles(sourceLocation, searchPattern/*, option*/);
        CopyFiles(files, destinationLocation);
    }

    public static void DeleteFiles(string path)
    {
        string[] files = Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i]);
        }
    }

    public static void MoveFiles(string sourceLocation, string destinationLocation, string searchPattern, SearchOption option)
    {
        string[] files = Directory.GetFiles(sourceLocation, searchPattern/*, option*/);

        string destinationPath = string.Empty;
        for (int i = 0; i < files.Length; i++)
        {
            destinationPath = destinationLocation + Path.GetFileName(files[i]);
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            //File.Move(files[i], destinationPath); // move isn't implemented in unitys integration with mono
            File.Copy(files[i], destinationPath);
        }
    }

    public static List<string> GetFilesRecursive(string startDirectory)
    {
        return GetFilesRecursive(startDirectory, "*.*");
    }

    public static List<string> GetFilesRecursive(string startDirectory, string searchPattern)
    {
        // TODO: replace with GetFiles() using the SearchOption AllDirectories

        // 1.
        // Store results in the file results list.
        List<string> result = new List<string>();

        // 2.
        // Store a stack of our directories.
        Stack<string> stack = new Stack<string>();

        // 3.
        // Add initial directory.
        stack.Push(startDirectory);

        // 4.
        // Continue while there are directories to process
        while (stack.Count > 0)
        {
            // A.
            // Get top directory
            string dir = stack.Pop();

            try
            {
                // B
                // Add all files at this directory to the result List.
                result.AddRange(Directory.GetFiles(dir, searchPattern));

                // C
                // Add all directories at this directory.
                foreach (string dn in Directory.GetDirectories(dir))
                {
                    stack.Push(dn);
                }
            }
            catch
            {
                // D
                // Could not open the directory
            }
        }
        return result;
    }

    public static string [] GetStreamingAssetsFiles(string path, string extension)
    {
#if !UNITY_WEBPLAYER
        // this function works on all platforms, special cased for Android.  See StreamingAssetsExtract class.

        // path is relative to StreamingAssets (see LoadStreamingAssets())
        // extension includes the dot, eg  ".wav"
        // wildcards not accepted in this function

        if (Application.platform == RuntimePlatform.Android)
        {
            return StreamingAssetsExtract.GetFiles(path, extension);
        }
        else
        {
            string [] files = Directory.GetFiles(Utils.GetStreamingAssetsPath() + path, "*" + extension, SearchOption.AllDirectories);
            List<string> fileList = new List<string>(files);
            for (int i = 0; i < fileList.Count; i++)
            {
                fileList[i] = fileList[i].Replace(Utils.GetStreamingAssetsPath(), "");
            }
            return fileList.ToArray();
        }
#else
        return new string [0];
#endif
    }

    public static string GetStreamingAssetsPath()
    {
        string path;

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            path = Application.streamingAssetsPath + "/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            path = "";
            Debug.Log("GetStreamingAssetsPath() - This function is invalid on Android.  Cannot access streaming assets path directly.  Need to use WWW url format (see GetStreamingAssetsURL())");
        }
        else if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            path = "";
            Debug.Log("GetStreamingAssetsPath() error - This does not work in the webplayer");
        }
        else
        {
            path = "";
            Debug.Log("GetStreamingAssetsPath() error - Platform not supported");
        }

        return path;
    }

    public static string GetStreamingAssetsURL()
    {
        string path;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            path = "file://" + Application.streamingAssetsPath + "/";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            path = "file://" + Application.streamingAssetsPath + "/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            path = "jar:file://" + Application.dataPath + "!/assets/";
        }
        else if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            path = Application.dataPath + "/";
            Debug.Log("GetStreamingAssetsURL() error - This does not work in the webplayer");
        }
        else
        {
            path = "";
            Debug.Log("GetStreamingAssetsURL() error - Platform not supported");
        }

        return path;
    }

    public static string GetExternalAssetsPath()
    {
        // this is the same for all platforms except for Android, where persistantDataPath is used

        string path;

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            path = Application.streamingAssetsPath + "/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            path = Application.persistentDataPath + "/";
        }
        else if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            path = "";
            Debug.Log("GetExternalAssetsPath() error - This does not work in the webplayer");
        }
        else
        {
            path = "";
            Debug.Log("GetExternalAssetsPath() error - Platform not supported");
        }

        return path;
    }

    public static WWW LoadStreamingAssets(string filename)
    {
        // This function will load data from the StreamingAssets folder in a cross-platform manner.
        // filename is relative to the StreamingAssets folder.
        // ie:  \Assets\StreamingAssets\data\file.dat
        // filename should be:  data\file.dat

        // data can be accessed through the WWW accessors:  .bytes, .texture, .audioClip, etc.

        // this function doesn't return until the data has been completely loaded.
        // see LoadStreamingAssetsAsync() for an asynchronous version.

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.LogWarning("LoadStreamingAssets() - doesn't work on iOS since it needs to wait a frame before it will start loading, and will get stuck in an endless loop.  Use LoadStreamingAssetsBytes() instead.  (See unity support #3167)");
            return null;
        }

        string fullPath = GetStreamingAssetsURL() + filename;

        //Debug.Log("LoadStreamingAssets() - fullPath = " + fullPath);

        var www = new WWW(fullPath);
        while (!www.isDone) { }

        if (www.error != null)
        {
            Debug.Log("LoadStreamingAssets() error - " + www.error);
            return null;
        }

        //Debug.Log("length: " + www.bytes.Length);

        return www;
    }

    public static WWW LoadStreamingAssetsAsync(string filename)
    {
        // This function will load data from the StreamingAssets folder in a cross-platform manner.
        // filename is relative to the StreamingAssets folder.
        // ie:  \Assets\StreamingAssets\data\file.dat
        // filename should be:  data\file.dat

        // data can be accessed through the WWW accessors:  .bytes, .texture, .audioClip, etc.

        // returned WWW needs to wait until it is finished loading, via isDone or yield
        // do not while loop on isDone, because on certain platforms, this will cause an endless loop, see unity support #3167

        string fullPath = GetStreamingAssetsURL() + filename;
        Debug.Log("fullPath: " + fullPath);
        //Debug.Log("LoadStreamingAssetsAsync() - fullPath = " + fullPath);

        var www = new WWW(fullPath);
        return www;
    }

    public static byte [] LoadStreamingAssetsBytes(string filename)
    {
#if !UNITY_WEBPLAYER
        // This function will load data from the StreamingAssets folder in a cross-platform manner.
        // filename is relative to the StreamingAssets folder.
        // ie:  \Assets\StreamingAssets\data\file.dat
        // filename should be:  data\file.dat

        // this function doesn't return until the data has been completely loaded.

        //Debug.Log("LoadStreamingAssetsBytes() - " + filename);

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android needs to use the WWW class
            return LoadStreamingAssets(filename).bytes;
        }
        else
        {
            string fullPath = GetStreamingAssetsPath() + filename;
            byte [] bytes = System.IO.File.ReadAllBytes(fullPath);
            return bytes;
        }
#else
        return new byte [0];
#endif
    }

    public static uint TurnFlagOn(ref uint data, uint flag)
    {
        return data |= flag;
    }

    public static uint TurnFlagOff(ref uint data, uint flag)
    {
        return data &= ~flag;
    }

    public static uint ToggleFlag(ref uint data, uint flag)
    {
        return data ^= flag;
    }

    public static bool IsFlagOn(uint data, uint flag)
    {
        return (data & flag) == flag;
    }

    public static void DrawTransformLines(Transform t, float length)
    {
        Debug.DrawRay(t.position, t.right * length, Color.red);
        Debug.DrawRay(t.position, t.up * length, Color.green);
        Debug.DrawRay(t.position, t.forward * length, Color.blue);
    }

    public static string RemovePathUpTo(string upToHere, string path)
    {
        int index = path.LastIndexOf(upToHere);
        if (index > -1)
        {
            return path.Substring(index);
        }

        return path;
    }

    public static Vector3 ConvertStringsToVector(string x, string y, string z)
    {
        Vector3 retVal = new Vector3();
        if (!float.TryParse(x, out retVal.x))
        {
            Debug.LogError("ConvertStringsToVector failed on x string: " + x);
        }

        if (!float.TryParse(y, out retVal.y))
        {
            Debug.LogError("ConvertStringsToVector failed on y string: " + y);
        }

        if (!float.TryParse(z, out retVal.z))
        {
            Debug.LogError("ConvertStringsToVector failed on z string: " + z);
        }
        return retVal;
    }

    public static void ClearAttributesRecursive(string currentDir)
    {
        // recursively clear the attributes on all files under the given folder

        if (Directory.Exists(currentDir))
        {
            string [] subDirs = Directory.GetDirectories(currentDir);
            foreach (string dir in subDirs)
            {
                ClearAttributesRecursive(dir);
            }

            string [] files = Directory.GetFiles(currentDir);
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
        }
    }

    public static void PlayWWWSound(MonoBehaviour behaviour, WWW www, AudioSource source, bool loop)
    {
        behaviour.StartCoroutine(PlayWWWSound(www, source, loop));
    }

    static IEnumerator PlayWWWSound(WWW www, AudioSource source, bool loop)
    {
        yield return www;

        source.clip = www.audioClip;
        while (!source.clip.isReadyToPlay)
        {
            yield return new WaitForEndOfFrame();
        }

        source.clip.name = www.url;
        source.loop = loop;
        source.Play();
    }

    /// <summary>
    /// plays a sound using the VHCL audio wrapper in wrapper-dll
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="position"></param>
    /// <param name="loop"></param>
    public static void PlayVHCLSound(string soundName, string niceName, Vector3 position, bool loop)
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        soundName = soundName.Trim('"');
        niceName = niceName.Trim('"');
        VHCLAudioManager.Get().PlaySound(soundName, niceName, position, loop);
    }

    /// <summary>
    /// Stops playing a VHCL sound
    /// </summary>
    /// <param name="soundName"></param>
    public static void StopSound(string soundName)
    {
        soundName = soundName.Trim('"');
        VHCLAudioManager.Get().StopSound(soundName);
    }


    /// <summary>
    /// Pauses all currently playing sounds
    /// </summary>
    /// <param name="soundName"></param>
    public static void PauseAllSounds()
    {
        VHCLAudioManager.Get().PauseAllSounds();
    }


    /// <summary>
    /// Stops all currently playing sounds
    /// </summary>
    /// <param name="soundName"></param>
    public static void StopAllSounds()
    {
        VHCLAudioManager.Get().StopAllSounds();
    }


    /// <summary>
    /// unpauses all currently playing sounds
    /// </summary>
    /// <param name="soundName"></param>
    public static void UnpauseAllSounds()
    {
        VHCLAudioManager.Get().UnpauseAllSounds();
    }









    public static void CreateAxisLines()
    {
        // this is a one-time function for generating the axis lines.  You can run this and copy-paste the objects into the scene
        float width = 0.01f;
        CreateCylinder(new Vector3(-10,0,0), new Vector3(0,0,0),  width, Color.red - new Color(0.5f,0,0));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(10,0,0), width, Color.red);
        CreateCylinder(new Vector3(0,-10,0), new Vector3(0,0,0),  width, Color.green - new Color(0,0.5f,0));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(0,10,0), width, Color.green);
        CreateCylinder(new Vector3(0,0,-10), new Vector3(0,0,0),  width, Color.blue - new Color(0,0,0.5f));
        CreateCylinder(new Vector3(0,0,0),   new Vector3(0,0,10), width, Color.blue);
    }

    public static void CreateCylinder( Vector3 start, Vector3 end, float width, Color color )
    {
        Vector3 offset = end - start;
        Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
        Vector3 position = start + (offset / 2.0f);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);  //)Instantiate(cylinderPrefab, position, Quaternion.identity);
        cylinder.transform.position = position;
        cylinder.transform.rotation = Quaternion.identity;
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
        cylinder.renderer.material.color = color;
    }

    public static string GetCommonAspectText(float aspectRatio)
    {
       // http://en.wikipedia.org/wiki/List_of_common_resolutions
       const float check = 0.04f;
       if      (Math.Abs(aspectRatio - 1.00f) < check) return "1:1";
       else if (Math.Abs(aspectRatio - 1.25f) < check) return "5:4";
       else if (Math.Abs(aspectRatio - 1.33f) < check) return "4:3";
       else if (Math.Abs(aspectRatio - 1.50f) < check) return "3:2";
       else if (Math.Abs(aspectRatio - 1.60f) < check) return "16:10";
       else if (Math.Abs(aspectRatio - 1.66f) < check) return "5:3";
       else if (Math.Abs(aspectRatio - 1.77f) < check) return "16:9";
       else return "";
    }

    /// <summary>
    /// Rotate an object towards the target position
    /// </summary>
    /// <param name="turner"></param>
    /// <param name="targetPosition"></param>
    /// <param name="turnRateInDegrees"></param>
    public static void TurnTowardsTarget(MonoBehaviour turnController, GameObject turner, Vector3 targetPosition, float turnRateInDegrees)
    {
        turnController.StartCoroutine(Internal_TurnTowardsTarget(turner, targetPosition, turnRateInDegrees));
    }

    static IEnumerator Internal_TurnTowardsTarget(GameObject turner, Vector3 targetPosition, float turnRateInDegrees)
    {
        Vector3 initialOrientation = turner.transform.forward;
        Vector3 targetRotation = (targetPosition - turner.transform.position).normalized;
        float t = 0;
        float secondsToComplete = Vector3.Angle(turner.transform.forward, targetRotation) / turnRateInDegrees;

        while (t < secondsToComplete)
        {
            turner.transform.forward = Vector3.Slerp(initialOrientation, targetRotation, t / secondsToComplete);
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }
    }

    /// <summary>
    /// Returns the time in seconds that it will take to get from startPos to endPos
    /// given a constant speed
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    public static float GetTimeToReachPosition(Vector3 startPos, Vector3 endPos, float speed)
    {
        if (speed == 0)
        {
            Debug.LogError("You'll never reach your position because you aren't moving");
            return 0;
        }
        return (endPos - startPos).magnitude / Math.Abs(speed);
    }

    public static float GetMax(float[] data)
    {
        float retVal = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            if (retVal < data[i])
                retVal = data[i];
        }

        return retVal;
    }

    public static float GetMin(float[] data)
    {
        float retVal = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            if (retVal > data[i])
                retVal = data[i];
        }

        return retVal;
    }


    static public void WriteXML<T>(string filename, T data)
    {
        XmlSerializer serializer = null;
        TextWriter writer = null;
        try
        {
            serializer = new XmlSerializer(typeof(T));
            writer = new StreamWriter(filename);
            serializer.Serialize(writer, data);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write xml file " + filename + " Message: " + e.Message + ". Inner Exception: " + e.InnerException);
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
            }
        }
    }

    static public T ReadXML<T>(string filename)
    {
        T retval = default(T);
        XmlSerializer serializer = null;
        FileStream fs = null;

        try
        {
            serializer = new XmlSerializer(typeof(T));
            fs = new FileStream(filename, FileMode.Open);
            retval = (T)serializer.Deserialize(fs);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read xml file " + filename + " Message: " + e.Message + ". Inner Exception: " + e.InnerException);
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();
            }
        }
        return retval;
    }

    static public string FindPathToFolder(string folderName)
    {
        string folderLocation = string.Empty;
#if !UNITY_WEBPLAYER
        string[] dirs = Directory.GetDirectories(Application.dataPath, folderName, SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i].Contains(folderName))
            {
                folderLocation = dirs[i];
                folderLocation = folderLocation.Replace('\\', '/');

                int index = folderLocation.IndexOf("/Assets");
                if (index != -1)
                {
                    folderLocation = folderLocation.Remove(0, index + 1);
                }
                folderLocation += '/';
                break;
            }
        }
#endif
        return folderLocation;
    }

    /// <summary>
    /// Returns true if the rects are overlapping
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    static public bool IsRectOverlapping(Rect a, Rect b)
    {
        return a.x < b.xMax && a.y < b.yMax && a.xMax > b.x && a.yMax > b.y;
    }

    /// <summary>
    /// returns the amount of overlap between the 2 rectangles on the x axis
    /// 0 if they aren't overlapping
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    static public float GetRectangleOverlapX(Rect a, Rect b)
    {
        if (!Utils.IsRectOverlapping(a, b))
        {
            return 0;
        }

        return a.x > b.x ? b.xMax - a.x : b.x - a.xMax;
    }

    static public void DisplayObject(GameObject obj, MonoBehaviour coroutineRunner, float displayTime, bool startsOn)
    {
        if (coroutineRunner != null)
        {
            obj.SetActive(startsOn);
            coroutineRunner.StartCoroutine(DisplayObjectCoroutine(obj, displayTime, startsOn));
        }
    }

    static IEnumerator DisplayObjectCoroutine(GameObject obj, float displayTime, bool startsOn)
    {
        yield return new WaitForSeconds(displayTime);
        obj.SetActive(!startsOn);
    }
}
