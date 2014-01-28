using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CommonEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.Common; }
    #endregion

    #region Events
    public class CommonEvent_Base : ICutsceneEventInterface { }

    public class CommonEvent_ActivateGameObject : CommonEvent_Base
    {
        #region Functions
        public void ActivateGameObject(GameObject go, bool active)
        {
#if UNITY_3_5 || UNITY_3_4
            go.active = active;
#else
            go.SetActive(active);
#endif
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
#if UNITY_3_5 || UNITY_3_4
            return Cast<GameObject>(ce, 0).active;
#else
            return Cast<GameObject>(ce, 0).activeSelf;
#endif
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            ActivateGameObject(Cast<GameObject>(ce, 0), (bool)rData);
        }
        #endregion
    }

#if UNITY_3_5 || UNITY_3_4
    public class CommonEvent_ActivateGameObjectRecursive : CommonEvent_Base
    {
        #region Functions
        public void ActivateGameObjectRecursive(GameObject go, bool active)
        {
            go.SetActiveRecursively(active);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<GameObject>(ce, 0).active;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            ActivateGameObjectRecursive(Cast<GameObject>(ce, 0), (bool)rData);
        }
        #endregion
    }
#endif

    public class CommonEvent_CreateGameObject : CommonEvent_Base
    {
        #region Variables
        List<GameObject> m_CreatedObjects = new List<GameObject>();
        #endregion

        #region Functions
        public void CreateGameObject(GameObject go)
        {
            m_CreatedObjects.Add((GameObject)GameObject.Instantiate(go));
        }

        public void CreateGameObject(GameObject go, Vector3 position, Vector3 rotation)
        {
            m_CreatedObjects.Add((GameObject)GameObject.Instantiate(go, position, Quaternion.Euler(rotation)));
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return m_CreatedObjects;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            List<GameObject> result = m_CreatedObjects.Except((List<GameObject>)rData).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                GameObject.Destroy(result[i]);
            }
        }
        #endregion
    }

    public class CommonEvent_DestroyGameObject : CommonEvent_Base
    {
        #region Functions
        public void DestroyGameObject(GameObject go)
        {
            Destroy(go);
        }
        #endregion
    }

    public class CommonEvent_TranslateGameObject : CommonEvent_Base
    {
        #region Functions
        public void TranslateGameObject(GameObject go, Vector3 position)
        {
            go.transform.Translate(position);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<GameObject>(ce, 0).transform.position;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<GameObject>(ce, 0).transform.position = (Vector3)rData;
        }
        #endregion
    }

    public class CommonEvent_RotateGameObject : CommonEvent_Base
    {
        #region Functions
        public void RotateGameObject(GameObject go, Vector3 rotation)
        {
            go.transform.Rotate(rotation);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<GameObject>(ce, 0).transform.rotation;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<GameObject>(ce, 0).transform.rotation = (Quaternion)rData;
        }
        #endregion
    }

    public class CommonEvent_TransformGameObject : CommonEvent_Base
    {
        #region Functions
        public void TransformGameObject(GameObject go, Vector3 position)
        {
            go.transform.position = position;
        }

        public void TransformGameObject(GameObject go, Vector3 position, Vector3 rotation)
        {
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
        }

        public void TransformGameObject(GameObject go, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
            go.transform.localScale = scale;
        }

        public void TransformGameObject(GameObject go, Transform transform)
        {
            go.transform.position = transform.position;
            go.transform.rotation = transform.rotation;
            go.transform.localScale = transform.localScale;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            Transform transform = Cast<GameObject>(ce, 0).transform;
            return new TransformData(transform.position, transform.rotation.eulerAngles, transform.localScale);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            TransformData transformData = (TransformData)rData;
            TransformGameObject(Cast<GameObject>(ce, 0), transformData.Position, transformData.Rotation, transformData.Scale);
        }
        #endregion
    }

    public class CommonEvent_ApplyForce : CommonEvent_Base
    {
        #region Function
        public void ApplyForce(Rigidbody rb, Vector3 force)
        {
            rb.AddForce(force);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return SaveTransformHierarchy(Cast<Rigidbody>(ce, 0).transform);
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            LoadTransformHierarchy((List<TransformData>)rData, Cast<Rigidbody>(ce, 0).transform);
        }
        #endregion
    }

    public class CommonEvent_SetTimeScale : CommonEvent_Base
    {
        #region Function
        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Time.timeScale;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetTimeScale((float)rData);
        }
        #endregion
    }

    public class CommonEvent_Pause : CommonEvent_Base
    {
        #region Function
        public void Pause(bool pause)
        {
            Time.timeScale = pause ? 0 : 1;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Time.timeScale == 0;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Pause((bool)rData);
        }
        #endregion
    }

    public class CommonEvent_LookAt : CommonEvent_Base
    {
        #region Function
        public void LookAt(GameObject looker, GameObject target)
        {
            looker.transform.LookAt(target.transform);
        }

        public void LookAt(GameObject looker, Vector3 target)
        {
            looker.transform.LookAt(target);
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<GameObject>(ce, 0).transform.rotation;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<GameObject>(ce, 0).transform.rotation = (Quaternion)rData;
        }
        #endregion
    }

    public class CommonEvent_SendVHMsg : CommonEvent_Base
    {
        #region Function
        public void SendVHMsg(string message)
        {
            VHMsgBase.Get().SendVHMsg(message);
        }

        public override string GetXMLString(CutsceneEvent ce)
        {
            return string.Format(@"<event message=""{0}"" start=""{1}"" ypos=""{2}"" id=""{3}""/>", ce.FindParameter("message").stringData, ce.StartTime, ce.GuiPosition.y, ce.Name);
        }

        public override void SetParameters(CutsceneEvent ce, System.Xml.XmlReader reader)
        {
            float startTime;
            if (!string.IsNullOrEmpty(reader["start"]))
            {
                if (float.TryParse(reader["start"], out startTime))
                {
                    ce.StartTime = startTime;
                }
            }
            else if (!string.IsNullOrEmpty(reader["stroke"]))
            {
                if (float.TryParse(reader["stroke"], out startTime))
                {
                    ce.StartTime = startTime;
                }
            }

            ce.Name = !string.IsNullOrEmpty(reader["id"]) ? reader["id"] : "Event";
            ce.FindParameter("message").stringData = reader["message"];        
        }
        #endregion
    }

    public class CommonEvent_PlayCutscene : CommonEvent_Base
    {
        #region Function
        public void PlayCutscene(Cutscene cutscene)
        {
            cutscene.Play();
        }

        public override string GetLengthParameterName() { return "cutscene"; }
        #endregion
    }

    public class CommonEvent_PauseCutscene : CommonEvent_Base
    {
        #region Function
        public void PauseCutscene(Cutscene cutscene)
        {
            cutscene.Pause();
        }
        #endregion
    }

    public class CommonEvent_LoadLevel : CommonEvent_Base
    {
        #region Function
        public void LoadLevel(string levelName)
        {
            Application.LoadLevel(levelName);
        }

        public void LoadLevel(int level)
        {
            Application.LoadLevel(level);
        }
        #endregion
    }

    public class CommonEvent_SetParent : CommonEvent_Base
    {
        #region Function
        public void SetParent(Transform parent, Transform child)
        {
            child.parent = parent;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<GameObject>(ce, 0).transform.parent;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetParent(Cast<GameObject>(ce, 0).transform, (Transform)rData);
        }
        #endregion
    }

    public class CommonEvent_SendUnityMessage : CommonEvent_Base
    {
        #region Function
        public void SendUnityMessage(GameObject messenger, string methodName)
        {
            messenger.SendMessage(methodName);
        }

        public void SendUnityMessage(GameObject messenger, string methodName, object value)
        {
            messenger.SendMessage(methodName, value);
        }
        #endregion
    }

    public class CommonEvent_BroadcastUnityMessage : CommonEvent_Base
    {
        #region Function
        public void BroadcastUnityMessage(GameObject messenger, string methodName)
        {
            messenger.BroadcastMessage(methodName);
        }

        public void BroadcastUnityMessage(GameObject messenger, string methodName, object value)
        {
            messenger.SendMessage(methodName, value);
        }
        #endregion
    }

    public class CommonEvent_Marker : CommonEvent_Base
    {
        #region Function
        // Note - this event intentionally does nothing
        public void Marker() { }

        public void Marker(float length) { }

        public override string GetLengthParameterName() { return "length"; }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "length")
            {
                param.floatData = 1;
            }
        }
        #endregion
    }

    public class CommonEvent_DisplayObject : CommonEvent_Base
    {
        #region Function
        public void DisplayObject(GameObject obj, float displayTime)
        {
            Utils.DisplayObject(obj, m_Behaviour, displayTime, true);
        }

        public void DisplayObject(GameObject obj, float displayTime, bool startsOn)
        {
            Utils.DisplayObject(obj, m_Behaviour, displayTime, startsOn);
        }

        public override string GetLengthParameterName() { return "displayTime"; }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "displayTime")
            {
                param.floatData = 1;
            }
        }
        #endregion
    }

    public class CommonEvent_DisplayText : CommonEvent_Base
    {
        #region Function
        public void DisplayText(GUIText textObj, string text, float displayTime)
        {
            textObj.text = text;
            Utils.DisplayObject(textObj.gameObject, m_Behaviour, displayTime, true);
        }

        public void DisplayText(GUIText textObj, string text, float displayTime, bool startsOn)
        {
            textObj.text = text;
            Utils.DisplayObject(textObj.gameObject, m_Behaviour, displayTime, startsOn);
        }

        public override string GetLengthParameterName() { return "displayTime"; }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "displayTime")
            {
                param.floatData = 1;
            }
        }
        #endregion
    }

    public class CommonEvent_RecordPerformance_Start : CommonEvent_Base
    {
        #region Function
        public void RecordPerformance_Start(VHTimeDemo recorder, string projectName, string timeDemoName)
        {
            recorder.StartTimeDemo(projectName, timeDemoName, false);
        }

        public override void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
        {
            if (param.Name == "timeDemoName")
            {
                param.stringData = Application.loadedLevelName;
            }
        }
        #endregion
    }

    public class CommonEvent_RecordPerformance_Stop : CommonEvent_Base
    {
        #region Function
        public void RecordPerformance_Stop(VHTimeDemo recorder)
        {
            recorder.StopTimeDemo();
        }
        #endregion
    }
    #endregion 
}
