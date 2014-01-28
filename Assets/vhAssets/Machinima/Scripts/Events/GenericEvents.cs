using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Xml;

[ExecuteInEditMode]
public class GenericEvents : MonoBehaviour
{
    #region Variables
    protected List<MethodInfo> m_EventFunctions = new List<MethodInfo>();
    #endregion

    #region Properties
    public MethodInfo[] EventMethods
    {
        get { return m_EventFunctions.ToArray(); }
    }
    #endregion

    #region Functions
    public virtual string GetEventType() { return GetType().ToString(); }
    public virtual void Update()
    {
        if (!Application.isPlaying)
        {
            CheckAvailableEvents();
        }
    }

    public GenericEvents GetGenericEventsByEventType(string eventType)
    {
        GenericEvents[] genericEvents = GetComponents<GenericEvents>();
        GenericEvents match = Array.Find<GenericEvents>(genericEvents, ge => ge.GetEventType() == eventType);

        if (match == null)
        {
            Debug.LogError(string.Format("Couldn't find GenericEvents with type {0}", eventType));
        }

        return match;
    }

    /// <summary>
    /// Refreshes the event list in the Machinima Maker
    /// </summary>
    public void CheckAvailableEvents()
    {
        m_EventFunctions.Clear();
        Type[] nestedTypes = GetType().GetNestedTypes();

        for (int i = 0; i < nestedTypes.Length; i++)
        {
            if (nestedTypes[i].IsClass)
            {
                MethodInfo[] methods = nestedTypes[i].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance); // only implemented functions
                for (int j = 0; j < methods.Length; j++)
                {
                    // ignore methods that are inherited from ICutsceneEventInterface
                    MethodInfo[] ignoredMethods = typeof(ICutsceneEventInterface).GetMethods();
                    if (Array.Exists<MethodInfo>(ignoredMethods, delegate(MethodInfo info) { return info.Name == methods[j].Name; }))
                    {
                        continue;
                    }
                    m_EventFunctions.Add(methods[j]);
                }
            }
        }
    }

    public string GetLengthDefiningParamFromMethod(string eventMethodName)
    {
        return GetReturnValueFromFunction<string>(eventMethodName, "GetLengthParameterName", null);
    }

    public bool IsEventMethodFireAndForget(string eventMethodName)
    {
        return GetReturnValueFromFunction<bool>(eventMethodName, "IsFireAndForget", null);
    }

    /// <summary>
    /// Returns the length in time of the event
    /// </summary>
    /// <param name="eventMethodName"></param>
    /// <param name="ce"></param>
    /// <returns></returns>
    public float CalculateEventLength(string eventMethodName, CutsceneEvent ce)
    {
        object[] parameters = new object[1] { ce };
        return GetReturnValueFromFunction<float>(eventMethodName, "CalculateEventLength", parameters);
    }

    public string GetXMLString(CutsceneEvent ce)
    {
        object[] parameters = new object[1] { ce };
        return GetReturnValueFromFunction<string>(ce.FunctionName, "GetXMLString", parameters);
    }

    /// <summary>
    /// Uses attribute values from the xml file in order to populate the event's parameters with data
    /// </summary>
    /// <param name="ce"></param>
    /// <param name="reader"></param>
    public void SetParameters(CutsceneEvent ce, XmlReader reader)
    {
        object[] parameters = new object[2] { ce, reader };
        InvokeMethod(ce.FunctionName, "SetParameters", parameters);
    }

    public void SetMetaData(CutsceneEvent ce, object metaData)
    {
        object[] parameters = new object[1] { metaData };
        InvokeMethod(ce.FunctionName, "SetMetaData", parameters);
    }

    public void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param)
    {
        object[] parameters = new object[2] { ce, param };
        InvokeMethod(ce.FunctionName, "UseParamDefaultValue", parameters);
    }

    /// <summary>
    /// Instatiates the event invoker based off the method name
    /// </summary>
    /// <param name="eventMethodName"></param>
    /// <returns></returns>
    public ICutsceneEventInterface CreateCutsceneEventInterfaceFromMethod(string eventMethodName)
    {
        MethodInfo method = null;
        ICutsceneEventInterface retVal = CreateInterfaceFromMethod(eventMethodName, "IsFireAndForget", 0, ref method);
        return retVal;
    }

    void InvokeMethod(string eventMethodName, string internalFunctionName, object[] parameters)
    {
        if (m_EventFunctions.Count == 0)
        {
            CheckAvailableEvents();
        }

        MethodInfo method = null;
        ICutsceneEventInterface obj = CreateInterfaceFromMethod(eventMethodName, internalFunctionName, 0, ref method);
        if (obj != null && method != null)
        {
            method.Invoke(obj, parameters);
        }
        else
        {
            Debug.LogError("Failed InvokeEventMethod on " + internalFunctionName);
        }
    }

    T GetReturnValueFromFunction<T>(string eventMethodName, string internalFunctionName, object[] parameters)
    {
        T retVal = default(T);
        if (m_EventFunctions.Count == 0)
        {
            CheckAvailableEvents();
        }

        MethodInfo method = null;
        ICutsceneEventInterface obj = CreateInterfaceFromMethod(eventMethodName, internalFunctionName, 0, ref method);
        if (obj != null && method != null)
        {
            retVal = (T)(method.Invoke(obj, parameters));
        }
        else
        {
            Debug.LogError("Failed InvokeEventMethod on " + internalFunctionName);
        }

        return retVal;
    }

    /// <summary>
    /// Invokes an event method overload with the given parameters and time
    /// </summary>
    /// <param name="eventMethodName"></param>
    /// <param name="overloadIndex"></param>
    /// <param name="parameters"></param>
    /// <param name="time"></param>
    public void InvokeEventMethod(string eventMethodName, int overloadIndex, object[] parameters, float time, object metaData, MonoBehaviour behaviour)
    {
        MethodInfo method = null;
        ICutsceneEventInterface obj = CreateInterfaceFromMethod(eventMethodName, eventMethodName, overloadIndex, ref method);
        
        if (obj != null && method != null)
        {
            obj.SetInterpolationTime(time);
            obj.SetMetaData(metaData);
            obj.SetMonoBehaviour(behaviour);
            method.Invoke(obj, parameters);
        }
        else
        {
            Debug.LogError("Failed InvokeEventMethod on " + eventMethodName);
        }
    }

    public ParameterInfo[] GetEventMethodParams(string eventMethodName, int overloadIndex)
    {
        if (m_EventFunctions.Count == 0)
        {
            CheckAvailableEvents();
        }

        List<MethodInfo> methodInfos = m_EventFunctions.FindAll(delegate(MethodInfo info) { return info.Name == eventMethodName; });
        ParameterInfo[] parameters = null;
        if (methodInfos != null && methodInfos.Count > 0)
        {
            if (overloadIndex > methodInfos.Count - 1)
            {
                Debug.Log("bad overload index for function: " + eventMethodName);
                return null;
            }
            parameters = methodInfos[overloadIndex].GetParameters();
        }
        else
        {
            Debug.LogError(string.Format("Couldn't GetEventMethodParams. Method: {0}", eventMethodName));
        }

        return parameters;
    }

    public MethodInfo[] GetEventMethodOverloads(string eventMethodName)
    {
        if (m_EventFunctions.Count == 0)
        {
            CheckAvailableEvents();
        }

        List<MethodInfo> methodInfos = m_EventFunctions.FindAll(delegate(MethodInfo info) { return info.Name == eventMethodName; });
        return methodInfos.ToArray();
    }

    /// <summary>
    /// Creates an ICutsceneEventInterface object which can be used to get and set data of a specific event.
    /// </summary>
    /// <param name="eventMethodName"></param>
    /// <param name="methodToInvokeName"></param>
    /// <param name="methodToInvokeOverloadIndex"></param>
    /// <param name="out_methodToInvoke"></param>
    /// <returns></returns>
    protected ICutsceneEventInterface CreateInterfaceFromMethod(string eventMethodName, string methodToInvokeName, int methodToInvokeOverloadIndex, ref MethodInfo out_methodToInvoke)
    {
        if (m_EventFunctions.Count == 0)
        {
            CheckAvailableEvents();
        }

        // make sure the method exists
        MethodInfo methodInfo = m_EventFunctions.Find(delegate(MethodInfo info) { return info.Name == eventMethodName; });
        if (methodInfo == null)
        {
            Debug.LogError(string.Format("Couldn't CreateInterfaceFromMethod. Method: {0}", eventMethodName));
            return null;
        }

        // search through the nested types
        Type[] nestedTypes = GetType().GetNestedTypes();
        for (int i = 0; i < nestedTypes.Length; i++)
        {
            MethodInfo[] methods = nestedTypes[i].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance); // only implemented functions
            if (Array.Exists<MethodInfo>(methods, delegate(MethodInfo info) { return info.Name == eventMethodName; }))
            {
                // the method that we want exists, get it and create and object that can invoke it
                out_methodToInvoke = Array.FindAll<MethodInfo>(nestedTypes[i].GetMethods(), delegate(MethodInfo meth) { return meth.Name == methodToInvokeName; })[methodToInvokeOverloadIndex];
                return (ICutsceneEventInterface)Activator.CreateInstance(nestedTypes[i]);
            }
        }

        Debug.LogError(string.Format("Couldn't CreateInterfaceFromMethod. Method: {0}", eventMethodName));
        return null;
    }

    
    #endregion
}
