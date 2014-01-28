using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class CutsceneIO
{
    #region Variables
    MachinimaSaveData m_SaveData = new MachinimaSaveData();
    List<List<CutsceneTrackGroup>> TrackGroups = new List<List<CutsceneTrackGroup>>();
    #endregion

    #region Functions
    #region Saving
    /// <summary>
    /// Used for saving the provided cutscenes while unity is playing
    /// </summary>
    /// <param name="cutscenes"></param>
    /// <param name="path"></param>
    //public void SaveMachinimaData(List<Cutscene> cutscenes, string path)
    public void SaveMachinimaData(List<TimelineObject> cutscenes, string path)
    {
        m_SaveData = new MachinimaSaveData();
        TrackGroups.Clear();
        //TrackGroups.ForEach(g => g.Clear());

        for (int i = 0; i < cutscenes.Count; i++)
        {
            Cutscene c = cutscenes[i] as Cutscene;
            m_SaveData.CutsceneDatas.Add(c.m_CutsceneData);

            //m_SaveData.CutsceneDatas[i].TrackGroups.AddRange(c.GroupManager.m_TrackGroups);
            TrackGroups.Add(new List<CutsceneTrackGroup>());
            TrackGroups[i].AddRange(c.GroupManager.m_TrackGroups);
        }

        Utils.WriteXML<MachinimaSaveData>(path, m_SaveData);
    }

    /// <summary>
    /// Save a specific cutscene to the specified path. Writes out an XML file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cutscene"></param>
    public void SaveCutscene(string path, Cutscene cutscene)
    {
        Debug.Log(string.Format("Saving {0}", path));
        Utils.WriteXML<CutsceneData>(path, cutscene.m_CutsceneData);
    }

    /// <summary>
    /// Writes out each cutscene in this unity scene to a respectively named different xml file
    /// </summary>
    /// <param name="cutscenes"></param>
    /// <param name="path"></param>
    public void SaveAllCutscenes(string path, List<Cutscene> cutscenes)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // setup some save variables just in case for older cutscenes
        foreach (Cutscene c in cutscenes)
        {
            foreach (CutsceneEvent ce in c.CutsceneEvents)
            {
                ce.SetFunctionTargets(ce.TargetGameObject, ce.TargetComponent);
                foreach (CutsceneEventParam cep in ce.m_Params)
                {
                    cep.SetObjData(cep.objData);
                    if (cep.usesObjectData)
                        cep.objDataAssetPath = AssetDatabase.GetAssetPath(cep.objData.GetInstanceID());
                }
            }
        }

        cutscenes.ForEach(c => SaveCutscene(string.Format("{0}/{1}.xml", path, c.CutsceneName), c));
        Debug.Log("Finished Saving all Cutscenes");
    }
    #endregion

    #region Loading
    /// <summary>
    /// Used for saving between stopping and playing unity so that changes made while playing don't get wiped
    /// </summary>
    /// <param name="cutscenes"></param>
    public void LoadMachinimaData(List<TimelineObject> cutscenes)
    {
        if (m_SaveData.CutsceneDatas.Count != cutscenes.Count)
        {
            Debug.LogWarning("Data discrepances in the cutscene editor save data");
        }

        for (int i = 0; i < m_SaveData.CutsceneDatas.Count; i++)
        {
            Cutscene cutscene = cutscenes.Find(c => c.NameIdentifier == m_SaveData.CutsceneDatas[i].CutsceneName) as Cutscene;
            if (cutscene != null)
            {
                cutscene.m_CutsceneData = m_SaveData.CutsceneDatas[i];
                cutscene.GroupManager.RemoveAllGroups();
                cutscene.GroupManager.m_TrackGroups.AddRange(TrackGroups[i]);
                TrackGroups[i].Clear();

                // we need to manually load gameobject and component data since we can't serialize it
                for (int j = 0; j < cutscene.m_CutsceneData.Events.Count; j++)
                {
                    CutsceneEvent ce = cutscene.CutsceneEvents[j];
                    ce.SetFunctionTargets(m_SaveData.CutsceneDatas[i].Events[j].TargetGameObject, m_SaveData.CutsceneDatas[i].Events[j].TargetComponent);

                    for (int k = 0; k < ce.m_Params.Count; k++)
                    {
                        ce.m_Params[k].SetObjData(m_SaveData.CutsceneDatas[i].Events[j].m_Params[k].objData);
                    }
                }
            }
            else
            {
                Debug.Log("Error finding cutscene: " + m_SaveData.CutsceneDatas[i].CutsceneName);
            }
        }

        m_SaveData.Clear();
        m_SaveData = null;
    }

    /// <summary>
    /// Loads a specific cutscene from the xml path but only if a cutscene with the name name exists in the list provided
    /// </summary>
    /// <param name="cutscenes"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Cutscene LoadCutscene(List<Cutscene> cutscenes, string path, GameObject[] allGameObjects)
    {
        CutsceneData csData = Utils.ReadXML<CutsceneData>(path);
        Cutscene cutscene = cutscenes.Find(c => c.CutsceneName == csData.CutsceneName);
        if (cutscene == null)
        {
            return null;
        }

        cutscene.m_CutsceneData = csData;
        cutscene.CutsceneEvents.ForEach(ce => SetupEventObjects(cutscene, ce, allGameObjects));

        if (csData.Version == MachinimaSaveData.MachinimaVersionNumber)
        {
            UpgradeCutsceneVersion(cutscene);
        }
        return cutscene;
    }

    /// <summary>
    /// Loads all the specified cutscenes from xml files based on cutscene names
    /// </summary>
    /// <param name="cutscenes"></param>
    /// <param name="path"></param>
    public void LoadAllCutscenes(List<Cutscene> cutscenes, string path)
    {
        // this will find every single gameobject loaded, both active and in-active. This also prunes the prefab assets so they don't conflict with the instances of the prefabs
        GameObject[] allGameObjects = Array.FindAll<GameObject>((GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)), go => PrefabUtility.GetPrefabType(go) != PrefabType.Prefab);
        cutscenes.ForEach(c => LoadCutscene(cutscenes, string.Format("{0}/{1}.xml", path, c.CutsceneName), allGameObjects));
        Debug.Log("Finished Loading all Cutscenes");
    }
    #endregion

    void SetupEventObjects(Cutscene cutscene, CutsceneEvent ce, GameObject[] allGameObjects)
    {
        if (!GenericEventNames.IsCustomEvent(ce.EventType))
        {
            // locate the generic events child gameobject of the cutscene
            // and use it to correctly hook up the component reference
            Transform child = cutscene.transform.FindChild(ce.TargetGameObjectName);
            GenericEvents genericEventsComp = null;
            if (child == null)
            {
                Debug.LogError(string.Format("No child named {0} under gameobject {1}", ce.TargetGameObjectName, cutscene.name));
                return;
            }
            genericEventsComp = child.GetComponent<GenericEvents>();

            ce.TargetComponent = genericEventsComp.GetGenericEventsByEventType(ce.EventType);
            if (ce.TargetComponent == null)
            {
                // this is a project specific genericevents, so it needs to be added
                genericEventsComp.gameObject.AddComponent(ce.TargetComponentName);
            }
            ce.TargetComponent = genericEventsComp.GetGenericEventsByEventType(ce.EventType);

            ce.SetFunctionTargets(ce.TargetComponent.gameObject, ce.TargetComponent);
        }
        else
        {
            // custom events
            GameObject target = Array.Find<GameObject>(allGameObjects, go => (go.GetInstanceID() == ce.TargetGameObjectInstanceId && go.name == ce.TargetGameObjectName) || go.name == ce.TargetGameObjectName);
            if (target == null)
            {
                Debug.LogError(string.Format("Can't setup custom event {0} because no gameobject with name {1} exists with component name {2} attached",
                    ce.Name, ce.TargetGameObjectName, ce.TargetComponentName));   
                return;
            }

            ce.SetFunctionTargets(target, target.GetComponent(ce.TargetComponentName));
        }

        ce.m_Params.ForEach(cep => SetupParamData(ce.Name, cep, allGameObjects));
    }

    /// <summary>
    /// Sets up cutscene parameter data that cannot be serialized to an xml file. For example,
    /// game object and component references as well as unity asset types, like audio clips and textures
    /// </summary>
    /// <param name="cutsceneEventName"></param>
    /// <param name="cep"></param>
    void SetupParamData(string cutsceneEventName, CutsceneEventParam cep, GameObject[] allGameObjects)
    {
        if (!cep.usesObjectData)
        {
            // this parameter isn't using object data, so we don't
            // have to do anything because it's data was properly serialized
            return;
        }

        if (cep.objDataIsComponent)
        {
            GameObject[] goData = Array.FindAll<GameObject>(allGameObjects, go => (go.GetInstanceID() == cep.objDataInstanceId && go.name == cep.objDataName) || go.name == cep.objDataName);
            if (goData == null || goData.Length == 0)
            {
                Debug.LogError(string.Format("Couldn't find gameobject {0} for cutscene event {1}", cep.objDataName, cutsceneEventName));
                return;
            }
            else if (goData.Length > 1)
            {
                Debug.LogError(string.Format("There are {0} game objects in the scene with the name {1}. Picking the first one found to be used in event {2}. You should give each unique names.",
                    goData.Length, cep.objDataName, cutsceneEventName));
            }

            string shortenedType = Path.GetExtension(cep.DataType);
            shortenedType = string.IsNullOrEmpty(shortenedType) ? cep.DataType : shortenedType.Remove(0, 1);
            Component objData = goData[0].GetComponent(shortenedType);
            
            if (objData == null)
            {
                Debug.LogError(string.Format("Couldn't find component {0} on gameobject {1} for cutscene event {2}", shortenedType, cep.objDataName, cutsceneEventName));
            }
            cep.SetObjData(objData);
        }
        else
        {
            if (cep.DataType.IndexOf("GameObject") != -1)
            {
                GameObject[] goData = Array.FindAll<GameObject>(allGameObjects, go => (go.GetInstanceID() == cep.objDataInstanceId && go.name == cep.objDataName) || go.name == cep.objDataName);
                if (goData == null || goData.Length == 0)
                {
                    Debug.LogError(string.Format("Couldn't find gameobject {0} for cutscene event {1}", cep.objDataName, cutsceneEventName));
                    return;
                }
                else if (goData.Length > 1)
                {
                    Debug.LogError(string.Format("There are {0} game objects in the scene with the name {1}. Picking the first one found to be used in event {2}. You should give each unique names.",
                        goData.Length, cep.objDataName, cutsceneEventName));
                }
                cep.SetObjData(goData[0]);
            }
            else if (cep.DataType.IndexOf("AudioClip") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(AudioClip)));
            }
            else if (cep.DataType.IndexOf("AnimationClip") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(AnimationClip)));
            }
            else if (cep.DataType.IndexOf("TextAsset") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(TextAsset)));
            }
            else if (cep.DataType.IndexOf("Material") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(Material)));
            }
            else if (cep.DataType.IndexOf("Texture") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(Texture)));
            }
            else if (cep.DataType.IndexOf("Mesh") != -1)
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(Mesh)));
            }
            else
            {
                cep.SetObjData(LoadAsset(cep.objDataAssetPath, typeof(UnityEngine.Object)));
                //Debug.LogError(string.Format("Couldn't figure out how to load parameter type {0}", cep.DataType));
            }
        }
    }

    UnityEngine.Object LoadAsset(string path, Type assetType)
    {
        UnityEngine.Object retVal = AssetDatabase.LoadAssetAtPath(path, assetType);
        if (retVal == null)
        {
            Debug.LogError(string.Format("Couldn't load asset type {0} with path {1}", assetType.ToString(), path));
        }
        return retVal;
    }

    /// <summary>
    /// Used to handle any data irregularities between machinima maker versions
    /// </summary>
    /// <param name="cutscene"></param>
    void UpgradeCutsceneVersion(Cutscene cutscene)
    {

    }
    #endregion
}
