/*--------------------------------------------------------------------------------------------------
 * This script creates a prefab from selected asset(s) from the Project view.
 * It automates the steps outlined in Confluence about 'Nesting a FBX in a Prefab':
 *      1. Create a new empty gameobject in your Hierarchy (in the same directory as your FBX).
 *      2. Name it "HouseGameObject"
 *      3. Drag your imported model ("HouseFBX") from the Project panel onto the empty gameobject in the Hierarchy panel.
 *      4. Restore any material links that are broken.
 *      5. Create a new prefab in your Project panel (name this "HousePrefab").
 *      6. Drag the game object ("HouseGameObject") from the Hierarchy panel onto this new prefab ("HousePrefab") in your Project panel.
 *      7. Delete the game object in the Hiearchy.
 *      8. Drag the Prefab from the Project into the scene.
 *
 * End result: Prefab > GameObject > FBX
 *
 * 2012-May-09 - Now Smartbody ready!
 *             - A separate Sbm prefab button is available, which only adds Sbm components to the
 *               prefab if 'CharacterRoot' is found in the FBX.
 *
 * Joe Yip
 * yip@ict.usc.edu
 * 2011-Apr-04
--------------------------------------------------------------------------------------------------*/

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AGPrefabs : Editor{

    public static void Create(Object asset, bool SmartbodyCharacter){
        //Bring asset into Hierarchy
        string FBXPath = AssetDatabase.GetAssetPath(asset);
        if (FBXPath.IndexOf(".fbx") > 0){
            Debug.Log("Creating prefab: " + asset.name);
            GameObject obj = PrefabUtility.InstantiatePrefab(Resources.LoadAssetAtPath(FBXPath, typeof(Object))) as GameObject;

            string assetPrefabName = obj.name + "Prefab.prefab";
            string assetGameObjectName = obj.name + "GameObject";

            //Reset transforms
            obj.transform.localPosition = new Vector3(0, 0, 0);
            obj.transform.localRotation = new Quaternion(0, 0, 0, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);

            //Create Prefab and GameObject
            FBXPath = FBXPath.Replace((obj.name + ".fbx"), "");
            Object assetPrefab = PrefabUtility.CreateEmptyPrefab(FBXPath + assetPrefabName);
            GameObject assetGameObject = new GameObject(assetGameObjectName);

            //Parent FBX to GameObject
            obj.transform.parent = assetGameObject.transform;

            //Check if this is a character, if so, add necessary Smartbody components
            if (SmartbodyCharacter == true && obj.transform.FindChild("CharacterRoot") != null){
                SmartbodySetup(assetGameObject);
            }

            //Apply GameObject to Prefab Object (in the project)
            PrefabUtility.ReplacePrefab(assetGameObject, assetPrefab);

            //Cleanup
            Debug.Log(asset.name + "Prefab created.");
            Transform.DestroyImmediate(assetGameObject);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }

    public static void SmartbodySetup(GameObject assetGameObject){
        Debug.Log("    Setting up as Smartbody character.");

        //Add "UnitySmartbodyCharacter" script and set variables
        Debug.Log("        Added 'UnitySmartbodyCharacter' component.");
        assetGameObject.AddComponent<UnitySmartbodyCharacter>();
        //UnitySmartbodyCharacter SbmScript = assetGameObject.GetComponent<UnitySmartbodyCharacter>();
        //SbmScript.m_BoneParentName = assetGameObject.name.Replace("GameObject", "") + "/CharacterRoot";

        //Create SoundNode
        string soundNodeName = "SoundNode";
        GameObject soundNode = new GameObject(soundNodeName);
        Debug.Log("        Created 'SoundNode'.");
        soundNode.AddComponent("AudioSource");
        soundNode.transform.localPosition = new Vector3(0, 0, 0);
        soundNode.transform.localRotation = new Quaternion(0, 0, 0, 0);
        soundNode.transform.localScale = new Vector3(1, 1, 1);

        //Parent SoundNode and move it to where the mouth is (Zebra1 and Zebra2)
        soundNode.transform.parent = assetGameObject.transform;
        foreach (Transform child in assetGameObject.GetComponentsInChildren<Transform>()){
            if (child.name == "JtTongueC" || child.name == "Tongue_front"){
                soundNode.transform.localPosition = child.transform.position;
                Debug.Log("            Positioned 'SoundNode' at the character's mouth.");
                break;
            }
        }
    }

    public static void Update(Object asset, bool refreshAssets){
        //Bring asset into Hierarchy
        string PrefabPath = AssetDatabase.GetAssetPath(asset);
        string PrefabDir = PrefabPath.Replace(("/" + asset.name + ".prefab"), "");
        string FBXPath;
        string FBXNextToPrefabPath;
        string FBXOneLevelAbovePath;
        int prefabChanges = 0;

        if (PrefabPath.IndexOf(".prefab") > 0){
            Debug.Log("Updating prefab: " + asset.name);
            GameObject original = (GameObject)AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(Object));
            GameObject prefabObj = (GameObject)PrefabUtility.InstantiatePrefab(original);

            //Replace each FBX as we find them
            foreach (Transform child in prefabObj.transform){
                FBXPath = null;
                FBXNextToPrefabPath = (PrefabDir + "/" + child.name + ".fbx");
                FBXOneLevelAbovePath = PrefabDir.Substring(0, PrefabDir.LastIndexOf("/")) + "/" + child.name + ".fbx";

                //Try to find the corresponding FBX next to the prefab, or one level above the prefab
                if (System.IO.File.Exists(FBXNextToPrefabPath)){
                    FBXPath = FBXNextToPrefabPath;
                }
                else if (System.IO.File.Exists(FBXOneLevelAbovePath)){
                    FBXPath = FBXOneLevelAbovePath;
                }

                //Proceed if the FBX is found
                if (FBXPath != null){
                    //Debug.Log("    Found FBX at path: " + FBXPath);
                    GameObject newFBX = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(FBXPath, typeof(Object)));
                    Debug.Log("        Updated " + prefabObj.name + " with fbx: " + child.name + ".fbx");
                    Editor.DestroyImmediate(child.gameObject);


                    //Reset FBX transforms
                    newFBX.transform.localPosition = Vector3.zero;
                    newFBX.transform.localRotation = Quaternion.identity;
                    newFBX.transform.localScale = Vector3.one;

                    newFBX.transform.parent = prefabObj.transform;
                    prefabChanges += 1;
                }
            }

            if (prefabChanges > 0)
            {
                Debug.Log(prefabObj.name + " updated; " + prefabChanges.ToString() + " FBX(s) updated.");
                PrefabUtility.ReplacePrefab(prefabObj, original);

                if (refreshAssets)
                {
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }

            Transform.DestroyImmediate(prefabObj);
        }
    }
}

class AGPrefab{
    const string menuCreate = "VH/Prefabs/Create Prefab(s) From Selected";
    const string menuCreateSbm = "VH/Prefabs/Create Smartbody Prefab(s) From Selected";
    const string menuUpdate = "VH/Prefabs/Update Selected Prefab(s) %#r";
    const string menuUpdateAll = "VH/Prefabs/Update All";

    //Adds a menu named "Create Prefab(s) From Selected" to the GameObject menu.
    [MenuItem(menuCreate)]
    static void CreatePrefabMenu(){
        //Creates a prefab per asset selected
        GameObject[] list = (GameObject[])Selection.gameObjects;
        foreach (GameObject asset in list){
            AGPrefabs.Create(asset, false);
        }
    }

    [MenuItem(menuCreateSbm)]
    static void CreateSbmPrefabMenu(){
        //Creates a prefab per asset selected
        GameObject[] list = (GameObject[])Selection.gameObjects;
        foreach (GameObject asset in list){
            AGPrefabs.Create(asset, true);
        }
    }

    [MenuItem(menuUpdate)]
    static void UpdatePrefabMenu(){
        //Update the selected prefab(s). This will work in hierarchy or project view.
        GameObject[] list = (GameObject[])Selection.gameObjects;
        List<Object> uniqueObjects = new List<Object>();
        List<string> uniqueObjectPaths = new List<string>();
        
        foreach (GameObject asset in list){
            //Debug.Log (asset.name + ", " + PrefabUtility.GetPrefabParent(asset).name);
            Object objectInProject = PrefabUtility.GetPrefabParent(asset);
            string objectInProjectPath = AssetDatabase.GetAssetPath(objectInProject);
            
            //Check if there are doubles in the list
            if (objectInProject == null){
                uniqueObjectPaths.Add(AssetDatabase.GetAssetPath(asset));
                uniqueObjects.Add(asset);
            }
            else if (uniqueObjectPaths.Contains(objectInProjectPath) == false){
                uniqueObjectPaths.Add(objectInProjectPath);
                uniqueObjects.Add(objectInProject);
            }
        }
        
        //Update
        foreach (Object asset in uniqueObjects){
            AGPrefabs.Update(asset, true);
        }
    }

    [MenuItem(menuUpdateAll)]
    static void UpdateAllPrefabMenu(){
        float startTime = Time.realtimeSinceStartup;
        List<string> files = Utils.GetFilesRecursive("Assets", "*.prefab");

        for (int i = 0; i < files.Count; i++)
        {
            AGPrefabs.Update(AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject)), false);
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log("Prefabs in project: " + files.Count);
        Debug.Log(string.Format("Updated all fbx project prefabs in {0} seconds", Time.realtimeSinceStartup - startTime));
    }

    //Validates the menu; the item will be disabled if no game object is selected.
    //Returns True if the menu item is valid.
    [MenuItem(menuCreate, true)]
    static bool ValidateCreatePrefabMenu(){
        return Selection.activeGameObject != null;
    }

    [MenuItem(menuCreateSbm, true)]
    static bool ValidateCreateSbmPrefabMenu(){
        return Selection.activeGameObject != null;
    }

    [MenuItem(menuUpdate, true)]
    static bool ValidateUpdatePrefabMenu(){
        return Selection.activeGameObject != null;
    }
}
