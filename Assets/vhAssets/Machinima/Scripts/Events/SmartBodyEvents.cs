using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

public class SmartBodyEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.SmartBody; }
    #endregion

    #region Events
    public class SmartBodyEvent_Base : ICutsceneEventInterface
    {
        #region Functions
        public static Character FindCharacter(string gameObjectName, string eventName)
        {
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return null;
            }

            Character[] chrs = (Character[])GameObject.FindObjectsOfType(typeof(Character));
            foreach (Character chr in chrs)
            {
                if (chr.CharacterName == gameObjectName || chr.gameObject.name == gameObjectName)
                {
                    return chr;
                }
            }

            Debug.LogWarning(string.Format("Couldn't find Character {0} in the scene. Event {1} needs to be looked at", gameObjectName, eventName));
            return null;
        }

        protected string GetObjectName(CutsceneEvent ce, string objectParamName)
        {
            CutsceneEventParam param = ce.FindParameter(objectParamName);
            UnityEngine.Object o = param.objData;
            return o != null ? o.name : param.stringData;
        }

        static public UnityEngine.Object FindObject(string assetPath, Type assetType, string fileExtension)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            UnityEngine.Object retVal = null;
#if UNITY_EDITOR && !UNITY_WEBPLAYER
            retVal = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
            if (retVal == null)
            {
                // try a project search, this is slow but doesn't require a media path
                //Debug.Log(string.Format("looking for: ", ));
                string[] files = Directory.GetFiles(string.Format("{0}", Application.dataPath), assetPath + fileExtension, SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    files[0] = files[0].Replace("\\", "/"); // unity doesn't like backslashes in the asset path
                    files[0] = files[0].Replace(Application.dataPath, "");
                    files[0] = files[0].Insert(0, "Assets");
                    retVal = UnityEditor.AssetDatabase.LoadAssetAtPath(files[0], assetType);
                }
            }

            // if it's still null, it wasn't found at all
            if (retVal == null)
            {
                Debug.LogError(string.Format("Couldn't load {0} {1}", assetType, assetPath));
            }
#endif
            return retVal;
        }

        protected string GetSkmPath(CutsceneEvent ce)
        {
            string assetPath = "";
#if UNITY_EDITOR
            UnityEngine.Object obj = ce.FindParameter("skm").objData;
            string skmName = ce.FindParameter("skm").stringData;
            if (obj != null)
            {
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
            }
            else if (!string.IsNullOrEmpty(skmName))
            {
                List<string> files = Utils.GetFilesRecursive(string.Format("{0}/StreamingAssets", Application.dataPath), string.Format("{0}.skm", skmName));
                if (files.Count > 0)
                {                 
                    assetPath = files[0].Replace("\\", "/");
                }
            }
#endif
            return assetPath;
        }
        #endregion
    }

    public class SmartBodyEvent_RunPythonScript : SmartBodyEvent_Base
    {
        #region Functions
        public void RunPythonScript(string script)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBRunPythonScript(script);
            else
                SmartbodyManager.Get().SBRunPythonScript(script);
        }
        #endregion
    }

    public class SmartBodyEvent_MoveCharacter : SmartBodyEvent_Base
    {
        #region Functions
        public void MoveCharacter(Character character, string direction, float fSpeed, float fLrps, float fFadeOutTime)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBMoveCharacter(character.CharacterName, direction, fSpeed, fLrps, fFadeOutTime);
            else
                SmartbodyManager.Get().SBMoveCharacter(character.CharacterName, direction, fSpeed, fLrps, fFadeOutTime);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }
        #endregion
    }

    public class SmartBodyEvent_WalkTo : SmartBodyEvent_Base
    {
        #region Functions
        public void WalkTo(Character character, Character waypoint, bool isRunning)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBWalkTo(character.CharacterName, waypoint.CharacterName, isRunning);
            else
                SmartbodyManager.Get().SBWalkTo(character.CharacterName, waypoint.CharacterName, isRunning);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }
        #endregion
    }

    public class SmartBodyEvent_WalkImmediate : SmartBodyEvent_Base
    {
        #region Functions
        public void WalkImmediate(Character character, string locomotionPrefix, float velocity, float turn, float strafe)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBWalkImmediate(character.CharacterName, locomotionPrefix, velocity, turn, strafe);
            else
                SmartbodyManager.Get().SBWalkImmediate(character.CharacterName, locomotionPrefix, velocity, turn, strafe);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }
        #endregion
    }

    public class SmartBodyEvent_PlayAudio : SmartBodyEvent_Base
    {
        #region Functions
        public void PlayAudio(Character character, AudioClip audioId)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId.name);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId);
        }

        public void PlayAudio(Character character, AudioClip audioId, string text)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId, text);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId, text);
        }

        public void PlayAudio(Character character, TextAsset audioId)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId.name);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId.name);
        }

        public void PlayAudio(Character character, TextAsset audioId, string text)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId.name, text);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId.name, text);
        }

        public void PlayAudio(Character character, string audioId)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId);
        }

        public void PlayAudio(Character character, string audioId, string text)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAudio(character.CharacterName, audioId, text);
            else
                SmartbodyManager.Get().SBPlayAudio(character.CharacterName, audioId, text);
        }

        public override string GetLengthParameterName() { return "audioId"; }

        public override float CalculateEventLength(CutsceneEvent ce)
        {
            float length = -1;
            if ((ce.FunctionOverloadIndex == 0 || ce.FunctionOverloadIndex == 1) && !IsParamNull(ce, 1))
            {
                length = Cast<AudioClip>(ce, 1).length; 
            }
            return length;
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            ce.Name = reader["ref"];
            if (ce.FunctionOverloadIndex == 0 || ce.FunctionOverloadIndex == 1)
            {
                UnityEngine.Object clip = FindObject(reader["ref"], typeof(AudioClip), ".wav");
                if (clip != null)
                {
                    ce.FindParameter("audioId").objData = clip;
                }

                if (clip == null)
                {
                    Debug.LogError("Couldn't find audio clip: " + reader["ref"]);
                }
            }
            
            ce.FindParameter("audioId").stringData = reader["ref"];
            ce.FindParameter("text").stringData = "";// reader.ReadString(); // TODO: Figure out a way to parse this
        }
        #endregion
    }

    public class SmartBodyEvent_PlayXml : SmartBodyEvent_Base
    {
        #region Functions
        public void PlayXml(Character character, string xml)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayXml(character.CharacterName, xml);
            else
                SmartbodyManager.Get().SBPlayXml(character.CharacterName, xml);
        }

        public void PlayXml(Character character, TextAsset xml)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayXml(character.CharacterName, xml.name + ".xml");
            else
                SmartbodyManager.Get().SBPlayXml(character.CharacterName, xml.name + ".xml");
        }

        //public override object SaveRewindData(CutsceneEvent ce)
        //{
        //    return Cast<Character>(ce, 0).AudioSource;
        //}

        //public override void LoadRewindData(CutsceneEvent ce, object rData)
        //{
        //    AudioSource source = Cast<Character>(ce, 0).AudioSource;
        //    if (source != null)
        //    {
        //        source.Stop();
        //    }
        //}

        public override string GetLengthParameterName() { return "xml"; }

        public override float CalculateEventLength(CutsceneEvent ce)
        {
            float length = -1;
#if UNITY_EDITOR
            // when it comes to bml, xml, txt, and .wav files associated with bml markup,
            // they all have the same name, but different file extensions
            string assetPath = "";
            if (ce.FunctionOverloadIndex == 0 && !string.IsNullOrEmpty(Param(ce, 1).stringData))
            {
                assetPath = Param(ce, 1).stringData.Replace('\\', '/');
            }
            else if (ce.FunctionOverloadIndex == 1 && Param(ce, 1).objData != null)
            {
                UnityEngine.Object obj = Param(ce, 1).objData;
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
            }
            else
            {
                // failed
                return length;
            }
            assetPath = System.IO.Path.ChangeExtension(assetPath, ".wav");   

            AudioClip relatedClip = (AudioClip)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip));
            if (relatedClip != null)
            {
                length = relatedClip.length;
            }
            else
            {
                Debug.LogWarning(string.Format("Couldn't calculate length of xml file {0} because the associated .wav file isn't located in the same folder",
                    ce.FunctionOverloadIndex == 0 ? Param(ce, 1).stringData : Param(ce, 1).objData.name));
            }  
#endif
            return length;
        }
        #endregion
    }

    public class SmartBodyEvent_Transform : SmartBodyEvent_Base
    {
        #region Functions
        public void Transform(Character character, Transform transform)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBTransform(character.CharacterName, transform.position, transform.rotation);
            else
                SmartbodyManager.Get().SBTransform(character.CharacterName, transform.position, transform.rotation);
        }

        public void Transform(Character character, float x, float y, float z)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBTransform(character.CharacterName, x, y, z);
            else
                SmartbodyManager.Get().SBTransform(character.CharacterName, x, y, z);
        }

        public void Transform(Character character, float x, float y, float z, float h, float p, float r)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBTransform(character.CharacterName, x, y, z, h, p, r);
            else
                SmartbodyManager.Get().SBTransform(character.CharacterName, x, y, z, h, p, r);
        }

        public void Transform(Character character, float y, float p)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBTransform(character.CharacterName, y, p);
            else
                SmartbodyManager.Get().SBTransform(character.CharacterName, y, p);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Character>(ce, 0).transform;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Transform rewindData = (Transform)rData;
            Transform characterData = Cast<Character>(ce, 0).transform;
            characterData.position = rewindData.position;
            characterData.rotation = rewindData.rotation;
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }
        #endregion
    }

    public class SmartBodyEvent_Rotate : SmartBodyEvent_Base
    {
        #region Functions
        public void Rotate(Character character, float h)
        {
            SmartbodyManager.Get().SBRotate(character.CharacterName, h);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Character>(ce, 0).transform.rotation.eulerAngles.y;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Rotate(Cast<Character>(ce, 0), (float)rData);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }
        #endregion
    }

    public class SmartBodyEvent_Posture : SmartBodyEvent_Base
    {
        #region Functions
        public void Posture(Character character, UnityEngine.Object skm)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPosture(character.CharacterName, skm.name, 0);
            else
                SmartbodyManager.Get().SBPosture(character.CharacterName, skm.name, 0);
        }

        public void Posture(Character character, UnityEngine.Object skm, float startTime)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPosture(character.CharacterName, skm.name, startTime);
            else
                SmartbodyManager.Get().SBPosture(character.CharacterName, skm.name, startTime);
        }

        public void Posture(Character character, string skm)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPosture(character.CharacterName, skm, 0);
            else
                SmartbodyManager.Get().SBPosture(character.CharacterName, skm, 0);
        }

        public override string GetLengthParameterName() { return "skm"; }

        public override float CalculateEventLength(CutsceneEvent ce)
        {
            float length = 1;
#if UNITY_EDITOR
               length = SmartbodyManager.FindSkmLength(GetSkmPath(ce));        
#endif
            return length;
        }
        #endregion
    }

    public class SmartBodyEvent_PlayAnim : SmartBodyEvent_Base
    {
        #region Functions
        public void PlayAnim(Character character, UnityEngine.Object skm)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAnim(character.CharacterName, skm.name);
            else
                SmartbodyManager.Get().SBPlayAnim(character.CharacterName, skm.name);
        }

        public void PlayAnim(Character character, UnityEngine.Object skm, float readyTime,
            float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAnim(character.CharacterName, skm.name, readyTime, strokeStartTime, emphasisTime, strokeTime, relaxTime);
            else
                SmartbodyManager.Get().SBPlayAnim(character.CharacterName, skm.name, readyTime, strokeStartTime, emphasisTime, strokeTime, relaxTime);
        }

        public void PlayAnim(Character character, string skm)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayAnim(character.CharacterName, skm);
            else
                SmartbodyManager.Get().SBPlayAnim(character.CharacterName, skm);
        }

        public void PlayAnim(Character character, string skm, float readyTime,
            float strokeStartTime, float emphasisTime, float strokeTime, float relaxTime)
        {
            if (m_MetaData != null)
            {
                CastMetaData<ICharacterController>().SBPlayAnim(character.CharacterName, skm, readyTime, strokeStartTime, emphasisTime, strokeTime, relaxTime);
            }
            else
                SmartbodyManager.Get().SBPlayAnim(character.CharacterName, skm, readyTime, strokeStartTime, emphasisTime, strokeTime, relaxTime);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }

        public override string GetLengthParameterName() { return "skm"; }

        public override float CalculateEventLength(CutsceneEvent ce)
        {
            float length = -1;
#if UNITY_EDITOR
            length = SmartbodyManager.FindSkmLength(GetSkmPath(ce));
#endif
            return length;
        }

        public override string GetXMLString(CutsceneEvent ce)
        {
#if UNITY_EDITOR
            float readyTime = 0, strokeStartTime = 0, emphasisTime = 0, strokeTime = 0, relaxTime = 0;
            //if (ce.FunctionOverloadIndex == 0)
            {
                SmartbodyManager.SkmMetaData metaData = SmartbodyManager.FindSkmMetaData(GetSkmPath(ce));
                readyTime = metaData.SyncPoints["ready"];
                strokeStartTime = metaData.SyncPoints["strokeStart"];
                emphasisTime = metaData.SyncPoints["emphasis"];
                strokeTime = metaData.SyncPoints["stroke"];
                relaxTime = metaData.SyncPoints["relax"];
            }
            //else
            //{
            //    readyTime = ce.FindParameter("readyTime").floatData;
            //    strokeStartTime = ce.FindParameter("strokeStartTime").floatData;
            //    emphasisTime = ce.FindParameter("emphasisTime").floatData;
            //    strokeTime = ce.FindParameter("strokeTime").floatData;
            //    relaxTime = ce.FindParameter("relaxTime").floatData;
            //}

            string animationName = string.Empty;
            string animationPath = string.Empty;
            UnityEngine.Object skm = ce.FindParameter("skm").objData;
            if (skm != null)
            {
                animationName = skm.name;
                animationPath = UnityEditor.AssetDatabase.GetAssetPath(skm);
            }
            
            return string.Format(@"<sbm:animation character=""{0}"" name=""{1}"" start=""{2}"" type=""{3}"" duration=""{4}"" weight=""{5}"" ready_time=""{6}"" strokestart_time=""{7}""
                                    emphasis_time=""{8}"" stroke_time=""{9}"" relax_time=""{10}"" track=""{11}"" ypos=""{12}"" id=""{13}"" assetPath=""{14}""/>",
                    GetObjectName(ce, "character"), animationName, ce.StartTime, "body", ce.Length, 1, readyTime, strokeStartTime, emphasisTime,
                    strokeTime, relaxTime, "BODY", ce.GuiPosition.y, ce.Name, animationPath);
#else
            return "";
#endif
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            // this is used for reading in older xml formats
            ce.Name = reader["name"];

            // I took this code out when I removed the object overloads of play anim. Add this back in when unity fixes the missing skm bug in builds
            if (ce.FunctionOverloadIndex == 0 || ce.FunctionOverloadIndex == 1)
            {
                // try the easy search first. Old xml will not have this attribute
#if UNITY_EDITOR
                ce.FindParameter("skm").SetObjData(FindObject(reader["assetPath"], typeof(UnityEngine.Object), ".skm"));
#endif
                // if it's still null, it wasn't found at all
                if (ce.FindParameter("skm").objData == null)
                {
                    // asset path wasn't found, just try the name
                    ce.FindParameter("skm").SetObjData(FindObject(reader["name"], typeof(UnityEngine.Object), ".skm"));
                    //Debug.LogError(string.Format("Couldn't load animation {0}", reader["name"]));
                }
                else
                {
#if UNITY_EDITOR
                    ce.Length = SmartbodyManager.FindSkmLength(UnityEditor.AssetDatabase.GetAssetPath(ce.FindParameter("skm").objData));
#endif
                }

#if UNITY_EDITOR
                if (ce.FunctionOverloadIndex == 1)
                {
                    SmartbodyManager.SkmMetaData metaData = SmartbodyManager.FindSkmMetaData(UnityEditor.AssetDatabase.GetAssetPath(ce.FindParameter("skm").objData));
                    ce.FindParameter("readyTime").floatData = metaData.SyncPoints["ready"];
                    ce.FindParameter("strokeStartTime").floatData = metaData.SyncPoints["strokeStart"];
                    ce.FindParameter("emphasisTime").floatData = metaData.SyncPoints["emphasis"];
                    ce.FindParameter("strokeTime").floatData = metaData.SyncPoints["stroke"];
                    ce.FindParameter("relaxTime").floatData = metaData.SyncPoints["relax"];
                    ce.Length = metaData.Length;
                }
#endif
            } 

            ce.FindParameter("skm").stringData = reader["name"];
            ce.FindParameter("character").stringData = reader["character"];
            Character sbChar = FindCharacter(reader["character"], ce.Name);
            if (sbChar != null)
            {
                ce.FindParameter("character").SetObjData(sbChar);
            }
        }
        #endregion
    }

    public class SmartBodyEvent_PlayFAC : SmartBodyEvent_Base
    {
        #region Functions
        public void PlayFAC(Character character, int au, SmartbodyManager.FaceSide side, float weight, float duration)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayFAC(character.CharacterName, au, side, weight, duration);
            else
                SmartbodyManager.Get().SBPlayFAC(character.CharacterName, au, side, weight, duration);
        }

        public void PlayFAC(Character character, int au, SmartbodyManager.FaceSide side, float weight, float duration, float readyTime, float relaxTime)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBPlayFAC(character.CharacterName, au, side, weight, duration);
            else
                SmartbodyManager.Get().SBPlayFAC(character.CharacterName, au, side, weight, duration);
        }

        public override string GetLengthParameterName() { return "duration"; }

        public override string GetXMLString(CutsceneEvent ce)
        {
            float readyTime = 0, relaxTime = 0;
            if (ce.FunctionOverloadIndex == 1)
            {
                readyTime = ce.FindParameter("readyTime").floatData;
                relaxTime = ce.FindParameter("relaxTime").floatData;
            }
            return string.Format(@"<face type=""FACS"" au=""{0}"" side=""{1}"" start=""{2}"" ready=""{3}"" relax=""{4}"" end=""{5}"" amount=""{6}"" ypos=""{7}"" character=""{8}"" />",
                    ce.FindParameter("au").intData, ce.FindParameter("side").enumDataString, ce.StartTime, readyTime, relaxTime, ce.EndTime, ce.FindParameter("weight").floatData,
                    ce.GuiPosition.y, GetObjectName(ce, "character"));
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            ce.FindParameter("au").intData = int.Parse(reader["au"]);
            int au = ce.FindParameter("au").intData;
            ce.StartTime = ParseFloat(reader["start"]);
            ce.FindParameter("character").SetObjData(FindCharacter(reader["character"], ce.Name));
            //ce.EventData.Ready = ParseFloat(reader["ready"]);
            //ce.EventData.Relax = ParseFloat(reader["relax"]);

            ce.FindParameter("weight").floatData = ParseFloat(reader["amount"]);

            if (!string.IsNullOrEmpty(reader["duration"]))
            {
                ce.FindParameter("duration").floatData = ce.Length = ParseFloat(reader["duration"]);
            }
            else
            {
                float endTime = ParseFloat(reader["end"]);
                ce.FindParameter("duration").floatData = ce.Length = endTime - ce.StartTime;
            }

            ce.Name = "FACS " + au;

            if (SmartbodyManager.AUToFacialLookUp.ContainsKey(au))
            {
                ce.Name += string.Format(" {0}", SmartbodyManager.AUToFacialLookUp[au]);
            }
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "au")
            {
                param.intData = 26; // jaw
            }
            else if (param.Name == "weight")
            {
                param.floatData = 1.0f;
            }
        }
        #endregion
    }

    public class SmartBodyEvent_Nod : SmartBodyEvent_Base
    {
        #region Functions
        public void Nod(Character character, float amount, float repeats, float time)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBNod(character.CharacterName, amount, repeats, time);
            else
                SmartbodyManager.Get().SBNod(character.CharacterName, amount, repeats, time);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }

        public override string GetLengthParameterName() { return "time"; }

        public override string GetXMLString(CutsceneEvent ce)
        {
            return string.Format(@"<head start=""{0}"" type=""{1}"" repeats=""{2}"" amount=""{3}"" track=""{4}"" ypos=""{5}"" id=""{6}"" end=""{7}"" character=""{8}""/>",
                    ce.StartTime, "NOD", ce.FindParameter("repeats").floatData, ce.FindParameter("amount").floatData, "NOD", ce.GuiPosition.y, ce.Name,
                    ce.FindParameter("time").floatData, GetObjectName(ce, "character"));
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            ce.FindParameter("repeats").floatData = ParseFloat(reader["repeats"]);
            ce.FindParameter("amount").floatData = ParseFloat(reader["amount"]);
            //ce.EventData.NodVelocity = ParseFloat(reader["velocity"]);
            ce.StartTime = ParseFloat(reader["start"]);
            ce.FindParameter("character").SetObjData(FindCharacter(reader["character"], ce.Name));
            if (ce.FindParameter("character") == null)
            {
                ce.FindParameter("character").SetObjData(FindCharacter(reader["participant"], ce.Name));
            }

            if (!string.IsNullOrEmpty(reader["duration"]))
            {
                ce.FindParameter("time").floatData = ce.Length = ParseFloat(reader["duration"]);
            }
            else
            {
                ce.FindParameter("time").floatData = ce.Length = ParseFloat(reader["end"]);
            }

            if (string.IsNullOrEmpty(ce.Name))
            {
                ce.Name = string.Format("Head Nod");
            }
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "amount")
            {
                param.floatData = 1;
            }
            else if (param.Name == "repeats")
            {
                param.floatData = 2.0f;
            }
            else if (param.Name == "time")
            {
                param.floatData = 1.0f;
            }
        }
        #endregion
    }

    public class SmartBodyEvent_Shake : SmartBodyEvent_Base
    {
        #region Functions
        public void Shake(Character character, float amount, float repeats, float time)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBShake(character.CharacterName, amount, repeats, time);
            else
                SmartbodyManager.Get().SBShake(character.CharacterName, amount, repeats, time);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Character>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Character>(ce, 0).transform);
            SmartbodyManager.Get().QueueCharacterToUpload(Cast<Character>(ce, 0));
        }

        public override string GetLengthParameterName() { return "time"; }

        public override string GetXMLString(CutsceneEvent ce)
        {
            return string.Format(@"<head start=""{0}"" type=""{1}"" repeats=""{2}"" amount=""{3}"" track=""{4}"" ypos=""{5}"" id =""{6}"" end=""{7}"" character=""{8}""/>",
                    ce.StartTime, "SHAKE", ce.FindParameter("repeats").intData, ce.FindParameter("amount").floatData, "NOD", ce.GuiPosition.y, ce.Name,
                    ce.FindParameter("time").floatData, GetObjectName(ce, "character"));
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            ce.FindParameter("repeats").floatData = ParseFloat(reader["repeats"]);
            ce.FindParameter("amount").floatData = ParseFloat(reader["amount"]);
            //ce.EventData.NodVelocity = ParseFloat(reader["velocity"]);
            ce.StartTime = ParseFloat(reader["start"]);
            ce.FindParameter("character").SetObjData(FindCharacter(reader["character"], ce.Name));
            if (ce.FindParameter("character") == null)
            {
                ce.FindParameter("character").SetObjData(FindCharacter(reader["participant"], ce.Name));
            }

            if (!string.IsNullOrEmpty("duration"))
            {
                ce.FindParameter("time").floatData = ce.Length = ParseFloat(reader["duration"]);
            }
            else
            {
                ce.FindParameter("time").floatData = ce.Length = ParseFloat(reader["end"]);
            }

            if (string.IsNullOrEmpty(ce.Name))
            {
                ce.Name = string.Format("Head Shake");
            }
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "amount")
            {
                param.floatData = 1;
            }
            else if (param.Name == "repeats")
            {
                param.floatData = 2.0f;
            }
            else if (param.Name == "time")
            {
                param.floatData = 1.0f;
            }
        }
        #endregion
    }

    public class SmartBodyEvent_Gaze : SmartBodyEvent_Base
    {
        #region Functions
        public void Gaze(Character character, Character gazeAt)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBGaze(character.CharacterName, gazeAt.CharacterName);
            else
                SmartbodyManager.Get().SBGaze(character.CharacterName, gazeAt.CharacterName);
        }

        public void Gaze(Character character, Character gazeAt, float neckSpeed)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBGaze(character.CharacterName, gazeAt.CharacterName, neckSpeed);
            else
                SmartbodyManager.Get().SBGaze(character.CharacterName, gazeAt.CharacterName, neckSpeed);
        }

        public void Gaze(Character character, Character gazeAt, float neckSpeed, float eyeSpeed, SmartbodyManager.GazeJointRange jointRange)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBGaze(character.CharacterName, gazeAt.CharacterName, neckSpeed, eyeSpeed, jointRange);
            else
                SmartbodyManager.Get().SBGaze(character.CharacterName, gazeAt.CharacterName, neckSpeed, eyeSpeed, jointRange);
        }

        public void Gaze(Character character, string gazeAt)
        {
            SmartbodyManager.Get().SBGaze(character.CharacterName, gazeAt);
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "neckSpeed")
            {
                param.floatData = 400;
            }
            else if (param.Name == "eyeSpeed")
            {
                param.floatData = 400;
            }
            else if (param.Name == "jointRange")
            {
                param.SetEnumData(SmartbodyManager.GazeJointRange.EYES_NECK);
            }
        }
        #endregion
    }

    public class SmartBodyEvent_GazeAdvanced : SmartBodyEvent_Base
    {
        #region Functions
        public void GazeAdvanced(Character character, Character gazeTarget, SmartbodyManager.GazeTargetBone targetBone, SmartbodyManager.GazeDirection gazeDirection,
            SmartbodyManager.GazeJointRange jointRange, float angle, float duration, float headSpeed, float eyeSpeed, float fadeOut, string gazeHandleName)
        {
            if (character == null || gazeTarget == null)
            {
                return;
            }

            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBGaze(character.CharacterName, gazeTarget.CharacterName, targetBone, gazeDirection, jointRange, angle, headSpeed, eyeSpeed, fadeOut, gazeHandleName, duration);
            else
                SmartbodyManager.Get().SBGaze(character.CharacterName, gazeTarget.CharacterName, targetBone, gazeDirection, jointRange, angle, headSpeed, eyeSpeed, fadeOut, gazeHandleName, duration);
        }

        string GazeTargetName(CutsceneEvent ce)
        {
            string gazeTargetName = "NO_GAZE_TARGET";
            if (ce.FunctionOverloadIndex == 0)
            {
                Character sbChar = Cast<Character>(ce, 1);
                if (sbChar != null && sbChar.gameObject != null)
                {
                    gazeTargetName = Cast<Character>(ce, 1).gameObject.name;
                }
            }
            return gazeTargetName;
        }

        public override string GetLengthParameterName() { return "duration"; }

        public override string GetXMLString(CutsceneEvent ce)
        {
            string jointRangeString = ce.FindParameter("jointRange").enumDataString;
            if (!string.IsNullOrEmpty(jointRangeString))
            {
                jointRangeString = jointRangeString.Replace("_", " ");
            } 

            return string.Format(@"<gaze character=""{0}"" id=""{1}"" target=""{2}"" angle=""{3}"" start=""{4}"" duration=""{5}"" headspeed=""{6}"" eyespeed=""{7}"" fadeout=""{8}"" sbm:joint-range=""{9}"" sbm:joint-speed=""{6} {7}"" track=""{10}"" ypos=""{11}"" direction=""{12}"" sbm:handle=""{13}""/>",
                    GetObjectName(ce, "character"), ce.Name, GazeTargetName(ce), ce.FindParameter("angle").floatData,
                    ce.StartTime, ce.Length, ce.FindParameter("headSpeed").floatData, ce.FindParameter("eyeSpeed").floatData, ce.FindParameter("fadeOut").floatData,
                    jointRangeString, "GAZE", ce.GuiPosition.y, ce.FindParameter("gazeDirection").enumDataString, ce.FindParameter("gazeHandleName").stringData);
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            if (!string.IsNullOrEmpty(reader["sbm:joint-range"]))
            {
                //ce.FindParameter("jointRange").SetEnumData((SmartbodyManager.GazeJointRange)Enum.Parse(typeof(SmartbodyManager.GazeJointRange), reader["sbm:joint-range"].ToString().Replace(" ", "_"), true));
                ce.FindParameter("jointRange").SetEnumData(SmartbodyManager.ParseGazeJointRange(reader["sbm:joint-range"]));
            }
           
            if (!string.IsNullOrEmpty(reader["direction"]))
            {
                string direction = reader["direction"];
                if (reader["direction"].IndexOf(' ') != -1)
                {
                    string[] split = direction.Split(' ');
                    direction = split[0];
                }
                ce.FindParameter("gazeDirection").SetEnumData((SmartbodyManager.GazeDirection)Enum.Parse(typeof(SmartbodyManager.GazeDirection), direction, true));
            }
            ce.FindParameter("character").SetObjData(FindCharacter(reader["character"], ce.Name));
            if (ce.FindParameter("character") == null)
            {
                ce.FindParameter("character").SetObjData(FindCharacter(reader["participant"], ce.Name));
            }
            ce.FindParameter("angle").floatData = ParseFloat(reader["angle"]);
            ce.FindParameter("headSpeed").floatData = ParseFloat(reader["headspeed"]);
            ce.FindParameter("eyeSpeed").floatData = ParseFloat(reader["eyespeed"]);
            ce.FindParameter("fadeOut").floatData = ParseFloat(reader["fadeout"]);
            ce.FindParameter("gazeHandleName").stringData = reader["sbm:handle"];
            ce.FindParameter("targetBone").stringData = reader["targetBone"];
            ce.StartTime = ParseFloat(reader["start"]);
            ce.FindParameter("duration").floatData = ce.Length = ParseFloat(reader["duration"]);
            if (ce.Length == 0)
            {
                ce.Length = 1;
            }
            string targetName = reader["target"];

            // we have the target name, so now let's search through the scene looking for the reference
            if (ce.FindParameter("gazeTarget").objData == null)
            {
                // there aren't any pawns in the scene with this name, let's do a character search instead.
                Character targetChr = FindCharacter(targetName, ce.Name);
                ce.FindParameter("gazeTarget").SetObjData(targetChr);
                ce.FunctionOverloadIndex = 0;
            }

            if (ce.FindParameter("gazeTarget").objData == null)
            {
                Debug.LogWarning(string.Format("{0} event {1} has a target named {2} but that target was not found in this scene", "Gaze", ce.Name, targetName));
            }
        }


        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "headSpeed")
            {
                param.floatData = 400;
            }
            else if (param.Name == "eyeSpeed")
            {
                param.floatData = 400;
            }
            else if (param.Name == "jointRange")
            {
                param.SetEnumData(SmartbodyManager.GazeJointRange.EYES_NECK);
            }
            else if (param.Name == "targetBone")
            {
                param.SetEnumData(SmartbodyManager.GazeTargetBone.NONE);
            }
            else if (param.Name == "gazeDirection")
            {
                param.SetEnumData(SmartbodyManager.GazeDirection.NONE);
            }
            else if (param.Name == "duration")
            {
                param.floatData = 1;
            }
            else if (param.Name == "fadeOut")
            {
                param.floatData = 0.25f;
            }
        }
        #endregion
    }

    public class SmartBodyEvent_StopGaze : SmartBodyEvent_Base
    {
        #region Constants
        const float DefaultStopGazeTime = 1;
        #endregion

        #region Functions
        public void StopGaze(Character character)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBStopGaze(character.CharacterName, DefaultStopGazeTime);
            else
                SmartbodyManager.Get().SBStopGaze(character.CharacterName, DefaultStopGazeTime);
        }

        public void StopGaze(Character character, float fadeOut)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBStopGaze(character.CharacterName, fadeOut);
            else
                SmartbodyManager.Get().SBStopGaze(character.CharacterName, fadeOut);
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "fadeOut")
            {
                param.floatData = 1;
            }
        }

        #endregion
    }

    public class SmartBodyEvent_Saccade : SmartBodyEvent_Base
    {
        #region Functions
        public void Saccade(Character character, SmartbodyManager.SaccadeType type, bool finish, float duration)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBSaccade(character.CharacterName, type, finish, duration);
            else
                SmartbodyManager.Get().SBSaccade(character.CharacterName, type, finish, duration);
        }

        public void Saccade(Character character, SmartbodyManager.SaccadeType type, bool finish, float duration, float angleLimit, float direction, float magnitude)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBSaccade(character.CharacterName, type, finish, duration, angleLimit, direction, magnitude);
            else
                SmartbodyManager.Get().SBSaccade(character.CharacterName, type, finish, duration, angleLimit, direction, magnitude);
        }

        public override string GetLengthParameterName() { return "duration"; }

        public override string GetXMLString(CutsceneEvent ce)
        {
            return string.Format(@"<event message=""sbm bml char {0} &lt;saccade mode=&quot;{1}&quot; finish=&quot;{6}&quot; sbm:duration=&quot;{7}&quot; /&gt;"" stroke=""{3}"" start=""{3}"" type=""{1}"" track=""{4}"" ypos=""{5}"" duration=""{7}"" character=""{8}"" />",
                    GetObjectName(ce, "character"), ce.FindParameter("type").enumDataString, 0, ce.StartTime, "Saccade", ce.GuiPosition.y,
                    ce.FindParameter("finish").boolData, ce.FindParameter("duration").floatData, GetObjectName(ce, "character"));
        }

        public override void SetParameters(CutsceneEvent ce, XmlReader reader)
        {
            if (!string.IsNullOrEmpty(reader["start"]))
            {
                float.TryParse(reader["start"], out ce.StartTime);
            }
            else if (!string.IsNullOrEmpty(reader["stroke"]))
            {
                float.TryParse(reader["stroke"], out ce.StartTime);
            }

            ce.FindParameter("character").SetObjData(FindCharacter(reader["character"], ce.Name));

            ce.FindParameter("type").SetEnumData((SmartbodyManager.SaccadeType)Enum.Parse(typeof(SmartbodyManager.SaccadeType), reader["type"], true));
            if (!string.IsNullOrEmpty(reader["duration"]))
            {
                ce.FindParameter("duration").floatData = ParseFloat(reader["duration"]);
            }

            ce.Name = ce.FindParameter("type").enumData.ToString();
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "duration")
            {
                param.floatData = 1;
            }
        }
        #endregion
    }

    public class SmartBodyEvent_StateChange : SmartBodyEvent_Base
    {
        #region Functions
        public void StateChange(Character character, string state, string mode, string wrapMode, string scheduleMode)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode);
            else
                SmartbodyManager.Get().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode);
        }

        public void StateChange(Character character, string state, string mode, string wrapMode, string scheduleMode, float x)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode, x);
            else
                SmartbodyManager.Get().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode, x);
        }

        public void StateChange(Character character, string state, string mode, string wrapMode, string scheduleMode, float x, float y, float z)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode, x, y, z);
            else
                SmartbodyManager.Get().SBStateChange(character.CharacterName, state, mode, wrapMode, scheduleMode, x, y, z);
        }
        #endregion
    }

    public class SmartBodyEvent_Express : SmartBodyEvent_Base
    {
        #region Functions
        public void Express(Character character, AudioClip uttID, string uttNum, string text)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBExpress(character.CharacterName, uttID.name, uttNum, text);
            else
                SmartbodyManager.Get().SBExpress(character.CharacterName, uttID.name, uttNum, text);
        }

        public void Express(Character character, string uttID, string uttNum, string text)
        {
            if (m_MetaData != null)
                CastMetaData<ICharacterController>().SBExpress(character.CharacterName, uttID, uttNum, text);
            else
                SmartbodyManager.Get().SBExpress(character.CharacterName, uttID, uttNum, text);
        }

        public override string GetLengthParameterName() { return "uttID"; }

        public override float CalculateEventLength(CutsceneEvent ce)
        {
            float length = -1;
            if ((ce.FunctionOverloadIndex == 0) && !IsParamNull(ce, 1))
            {
                length = Cast<AudioClip>(ce, 1).length;
            }
            return length;
        }
        #endregion
    }

    public class SmartBodyEvent_PythonCommand : ICutsceneEventInterface
    {
        #region Functions
        public void PythonCommand(string command)
        {
            SmartbodyManager.Get().PythonCommand(command);
        }
        #endregion
    }

    #endregion
}
