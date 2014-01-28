using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


/// <summary>
/// Custom import class.  These functions get called on asset import or reimport
/// </summary>
public class VHAssetPostProcessor : AssetPostprocessor
{
    private static List<string> m_matDirs = new List<string>();
    private static List<string> m_filesToDelete = new List<string>();

    //bool m_bIsUnityMaterialsFolderEmpty = false;
    bool m_bCustomMaterialGeneration = false;
    /// <summary>
    /// The returned material will be assigned to the renderer. If you return null,
    /// Unity will use its default material finding / generation method to assign a material.
    /// The sourceMaterial is generated directly from the model before importing and will be
    /// destroyed immediately after OnAssignMaterial.
    /// </summary>
    /// <param name="material"></param>
    /// <param name="renderer"></param>
    /// <returns></returns>
    /*Material OnAssignMaterialModel(Material sourceMaterial, Renderer renderer)
    {
        return null;
        Material retVal = null;

        if (string.IsNullOrEmpty(sourceMaterial.name))
        {
            sourceMaterial.name = "No Name";
        }

        sourceMaterial.name = sourceMaterial.name.Replace(':', '_');

        // Check if a material with this nam already exists in the project
        string materialFile = FindMaterial(Path.GetDirectoryName(assetPath), sourceMaterial.name + ".mat");
        if (materialFile != null)
        {
            // The material already exists, use it
            // All asset names & paths in Unity use forward slashes, paths using backslashes will not work.
            materialFile = materialFile.Replace('\\', '/');

            // remove everything before Unity's "Assets" folder, otherwise the load fails
            materialFile = Utils.RemovePathUpTo("Assets/", materialFile);

            retVal = (Material)AssetDatabase.LoadAssetAtPath(materialFile, typeof(Material));
        }
        else
        {
            // The material doesn't exist, so let's make it
            string creationPath = Path.GetDirectoryName(assetPath);

            // Append the material name
            string folderPath = (Application.dataPath.Replace("Assets", "")) + creationPath + "/Materials";
            //Debug.Log("folderPath: " + folderPath);

            // Create the material directory as a sibling to the model file
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Give it the proper name and create it
            creationPath += "/Materials/" + sourceMaterial.name + ".mat";
            //Debug.Log("creationPath: " + creationPath);
            AssetDatabase.CreateAsset(sourceMaterial, creationPath);
            retVal = (Material)AssetDatabase.LoadAssetAtPath(creationPath, typeof(Material));
        }

        if (retVal == null)
        {
            Debug.LogError("Material generation failed for material " + sourceMaterial.name
                + " on model " + Path.GetFileNameWithoutExtension(assetPath));
        }

        return retVal;
    }*/

    /// <summary>
    /// Starts in the passed in directory directory and looks for the material named filename in a folder called "Materials".
    /// Recurses upwards if it can't find it.
    /// </summary>
    /// <param name="startPath"></param>
    /// <param name="fileName"></param>
    /// <returns>returns the file path + filename of the requested file, null if it isn't found</returns>
    public static string FindMaterial(string startPath, string fileName)
    {
        const string MaterialFolderName = "/Materials";
        string retVal = null;
        if (string.IsNullOrEmpty(startPath) || string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("Bad parameter(s) passed into FindMaterial");
            return null;
        }

        string fileNameWithPath = string.Empty;
        while (!string.IsNullOrEmpty(startPath))
        {
            fileNameWithPath = startPath + MaterialFolderName + "/" + fileName;
            if (Directory.Exists(startPath + MaterialFolderName) && File.Exists(fileNameWithPath))
            {
                // the material exists, get out
                retVal = fileNameWithPath;
                //Debug.Log(fileName + " found in: " + (startPath + MaterialFolderName));
                break;
            }

            // material still not found, move up one directory
            int lastForwardSlash = startPath.LastIndexOf("/");
            if (lastForwardSlash > -1)
            {
                startPath = startPath.Remove(lastForwardSlash);
                //Debug.Log("startPath: " + startPath);
            }
            else
            {
                break;
            }
        }

        return retVal;
    }

    /// <summary>
    /// Reads Maya User Properties. This function is called after OnAssignMaterialModel and before OnPostprocessModel
    /// The fbx gameobject hierachy has not been created and connected at this point
    /// </summary>
    /// <param name="go"></param>
    /// <param name="propNames"></param>
    /// <param name="values"></param>
    void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values)
    {
        int i = 0;
        try
        {
            // Go through the properties one by one
            for (i = 0; i < propNames.Length; i++)
            {
                if ("CustomMaterialGeneration" == propNames[i])
                {
                    // they are using the custom material search and renaming pipeline
                    m_bCustomMaterialGeneration = (bool)values[i];
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("OnPostprocessGameObjectWithUserProperties caught an error on propName: "
                + propNames[i] + ". Exception: " + e.Message);
        }
    }

    /// <summary>
    /// called after OnPostprocessGameObjectWithUserProperties.  At this point, the entire fbx gameobject hierachy has been
    /// created.  The root object of the hierachy (fbx name) is passed into this function
    /// </summary>
    /// <param name="go"></param>
    void OnPostprocessModel(GameObject go)
    {
        // they want to do a custom material search
        if (m_bCustomMaterialGeneration)
        {
            // reset
            m_bCustomMaterialGeneration = false;

            string startingPath = Path.GetDirectoryName(assetPath);

            // get the root object and then get all of its children
            Renderer[] allRenderersInFBX = (Renderer[])go.GetComponentsInChildren<Renderer>();
            for (int i = 0; allRenderersInFBX != null && i < allRenderersInFBX.Length; i++)
            {
                if (allRenderersInFBX[i].sharedMaterials == null)
                {
                    // no point in being here if there aren't any materials
                    continue;
                }

                Material[] newSharedMaterials = new Material[allRenderersInFBX[i].sharedMaterials.Length];
                for (int k = 0; k < allRenderersInFBX[i].sharedMaterials.Length; k++)
                {
                    string unityGeneratedMaterialPath = AssetDatabase.GetAssetPath(allRenderersInFBX[i].sharedMaterials[k]);

                    // strip out the fbx name and the - from the default unity material name
                    string customMaterialName = allRenderersInFBX[i].sharedMaterials[k].name.Replace(go.name + "-", "");

                    // the path to the unity generated material
                    string unityMaterialPathWithFilename = Application.dataPath.Replace("Assets", "") + unityGeneratedMaterialPath;

                    // the name of the unity generated material
                    string unityMaterialName = Path.GetFileNameWithoutExtension(unityMaterialPathWithFilename);

                    // look upwards in folders called "Materials" to see if this already exists
                    string materialFile = FindMaterial(startingPath, customMaterialName + ".mat");

                    if (materialFile != null)
                    {
                        // The material already exists, use it
                        // All asset names & paths in Unity use forward slashes, paths using backslashes will not work.
                        materialFile = materialFile.Replace('\\', '/');

                        // remove everything before Unity's "Assets" folder, otherwise the load fails
                        materialFile = Utils.RemovePathUpTo("Assets/", materialFile);

                        newSharedMaterials[k] = (Material)AssetDatabase.LoadAssetAtPath(materialFile, typeof(Material));
                    }
                    else
                    {
                        // Create the material directory as a sibling to the model file
                        if (!Directory.Exists(startingPath + "/Materials"))
                        {
                            Directory.CreateDirectory(startingPath + "/Materials");
                        }

                        string creationPath = startingPath + "/Materials/" + customMaterialName + ".mat";
                        allRenderersInFBX[i].sharedMaterials[k].name = customMaterialName;

                        // create the material with the fbx name removed as a prefix
                        AssetDatabase.CreateAsset(new Material(allRenderersInFBX[i].sharedMaterials[k]), creationPath);
                        newSharedMaterials[k] = (Material)AssetDatabase.LoadAssetAtPath(creationPath, typeof(Material));
                    }

                    // get rid of the unity generated material

                    if (File.Exists(unityMaterialPathWithFilename) && customMaterialName != unityMaterialName)
                    {
                        if (m_filesToDelete.Contains(unityMaterialPathWithFilename) == false)
                            m_filesToDelete.Add(unityMaterialPathWithFilename);
                        if (m_filesToDelete.Contains(unityMaterialPathWithFilename + ".meta") == false)
                            m_filesToDelete.Add(unityMaterialPathWithFilename + ".meta");
                    }

                    // cache the Materials directory if it is exists
                    string unityGeneratedMaterialsDirectory = Path.GetDirectoryName(unityMaterialPathWithFilename);
                    if (Directory.Exists(unityGeneratedMaterialsDirectory))
                    {
                        if (m_matDirs.Contains(unityGeneratedMaterialsDirectory) == false)
                            m_matDirs.Add(unityGeneratedMaterialsDirectory);
                    }

                } // end k loop

                // give the sharedMaterials array the new material list
                allRenderersInFBX[i].sharedMaterials = newSharedMaterials;
            } // end i loop
         }
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        AssetDatabase.Refresh();

        //AssetDatabase
        for (int i = 0; i < m_matDirs.Count; i++)
        {
            if (!Directory.Exists(m_matDirs[i]))
            {
                // if the directory isn't here, continue
                continue;
            }

            int AssetsIndex = -1;

            // get the files in the directory that is supposed to be deleted
            int fileMatchCounter = 0;
            AssetsIndex = -1;
            string[] filesInFolder = Directory.GetFiles(m_matDirs[i]);
            for (int j = 0; j < filesInFolder.Length; j++)
            {
                filesInFolder[j] = filesInFolder[j].Replace('\\', '/');
                //Debug.Log("filesInFolder[j]: " + filesInFolder[j]);
                if (m_filesToDelete.Contains(filesInFolder[j]))
                {
                    // this is one of the files that should be deleted, so delete it
                    ++fileMatchCounter;
                    m_filesToDelete.Remove(filesInFolder[j]);
                    AssetsIndex = filesInFolder[j].LastIndexOf("Assets/");

                    if (AssetsIndex != -1)
                    {
                        AssetDatabase.DeleteAsset(filesInFolder[j].Remove(0, AssetsIndex));
                    }
                }
            }

            //Debug.Log("fileMatchCounter: " + fileMatchCounter + " filesInFolder.Length: " + filesInFolder.Length);
            if (fileMatchCounter == filesInFolder.Length)
            {
                // the folder is now empty, so delete the folder too
                AssetsIndex = m_matDirs[i].LastIndexOf("Assets/");
                AssetDatabase.DeleteAsset(m_matDirs[i].Remove(0, AssetsIndex));
            }
        }

        AssetDatabase.Refresh();
    }

    private static void OnGeneratedCSProjectFiles()
    {
        // This function modifies the generated .sln files (both <project>.sln and <project>-csharp.sln)
        // This function makes the following changes so that Visual Studio 2008 can load it correctly when double-clicking the file:
        // - Adds a newline at the top of the file
        // - Converts line endings to Environment.Newline
        // - Writes the file with UTF-8 with the correct BOM at the top of the file

        // Open the solution file
        string projectDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;;
        string projectName = Path.GetFileName(projectDirectory);
        string slnFile = Path.Combine(projectDirectory, string.Format("{0}.sln", projectName));
        string slnFileCsharp = Path.Combine(projectDirectory, string.Format("{0}-csharp.sln", projectName));

        try
        {
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                using (StreamReader sr = new StreamReader(slnFile))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                        sb.AppendLine(line.Replace("\n", "").Replace("\r", ""));
                }

                // Mono specific - to hard-code the EOL marker and Tab length (VH Project specific!)
                sb = sb.Replace("$3.EolMarker = Unix", "$3.EolMarker = Windows");
                sb = sb.Replace("$3.inheritsSet = Mono", "$3.inheritsSet = VisualStudio");

                using (StreamWriter sw = new StreamWriter(slnFile, false, Encoding.UTF8))
                {
                    //sw.Write(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);
                    sw.Write(sb.ToString());
                }
            }

            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                using (StreamReader sr = new StreamReader(slnFileCsharp))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                        sb.AppendLine(line.Replace("\n", "").Replace("\r", ""));
                }
                using (StreamWriter sw = new StreamWriter(slnFileCsharp, false, Encoding.UTF8))
                {
                    sw.Write(sb.ToString());
                }
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
        }
    }
}
