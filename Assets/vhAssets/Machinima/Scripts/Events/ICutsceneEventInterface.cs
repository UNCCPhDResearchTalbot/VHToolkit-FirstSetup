using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Xml;

public abstract class ICutsceneEventInterface
{
    #region Variables
    protected float m_InterpolationTime;
    protected object m_MetaData;
    protected MonoBehaviour m_Behaviour;
    #endregion

    #region Functions
    public virtual string GetLengthParameterName() { return CutsceneEvent.NoneParameter; }
    public virtual bool IsFireAndForget() { return true; }
    public void SetInterpolationTime(float time) { m_InterpolationTime = time; }
    public void SetMetaData(object metaData) { m_MetaData = metaData; }
    public void SetMonoBehaviour(MonoBehaviour behaviour) { m_Behaviour = behaviour; } 
    public virtual object SaveRewindData(CutsceneEvent ce) { return null; }
    public virtual void LoadRewindData(CutsceneEvent ce, object rData) { }
    public virtual void InterpolateRewindData(CutsceneEvent ce, object a, object b, float t) { }
    public virtual string GetXMLString(CutsceneEvent ce) { return string.Empty; }

    /// <summary>
    /// Uses attribute values from the xml file in order to populate the event's parameters with data
    /// </summary>
    /// <param name="ce"></param>
    /// <param name="reader"></param>
    public virtual void SetParameters(CutsceneEvent ce, XmlReader reader) { }
    public virtual void UseParamDefaultValue(CutsceneEvent ce, CutsceneEventParam param) { }

    /// <summary>
    /// Determines how long the event will be represented on the timeline
    /// </summary>
    /// <param name="ce"></param>
    /// <returns></returns>
    public virtual float CalculateEventLength(CutsceneEvent ce)
    {
        float length = -1;
        string paramName = GetLengthParameterName();
        if (paramName == CutsceneEvent.NoneParameter)
        {
            return -1;
        }

        CutsceneEventParam cep = ce.m_Params.Find(p => p.Name == paramName);
        if (cep == null)
        {
            Debug.LogError(string.Format("Parameter named {0} doesn't exist for function {1} using overload index {2}", paramName, ce.FunctionName, ce.FunctionOverloadIndex));
            return -1;
        }
        else
        {
            length = cep.GetLength();
        }

        return length;
    }

    /// <summary>
    /// Recurses through a transform hierarchy and saves the position, rotation, and scale and returns it
    /// </summary>
    /// <param name="root"></param>
    protected List<TransformData> SaveTransformHierarchy(Transform root)
    {
        List<TransformData> hierarchyTransformData = new List<TransformData>();
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(root);

        // depth first traversal
        while (stack.Count > 0)
        {
            Transform curr = stack.Pop();

            // act on data
            TransformData tData = new TransformData(curr.localPosition, curr.localRotation.eulerAngles, curr.localScale);
            hierarchyTransformData.Add(tData);

            for (int i = 0; i < curr.childCount; i++)
            {
                stack.Push(curr.GetChild(i));
            }
        }

        return hierarchyTransformData;
    }

    protected void LoadTransformHierarchy(List<TransformData> transformData, Transform transformToOverwrite)
    {
        Stack<Transform> stack = new Stack<Transform>();
        int counter = 0;
        stack.Push(transformToOverwrite);

        // depth first traversal
        while (stack.Count > 0)
        {
            Transform curr = stack.Pop();
            curr.localPosition = transformData[counter].Position;
            curr.localRotation = Quaternion.Euler(transformData[counter].Rotation);
            curr.localScale = transformData[counter].Scale;

            ++counter;

            for (int i = 0; i < curr.childCount; i++)
            {
                stack.Push(curr.GetChild(i));
            }
        }
    }

    // Helpers functions for dealing with cutscene params and data
    protected CutsceneEventParam Param(CutsceneEvent ce, int paramIndex) { return ce.m_Params[paramIndex]; }
    protected T Cast<T>(CutsceneEvent ce, int paramIndex) where T : UnityEngine.Object { return Param(ce, paramIndex).objData != null ? (T)(Param(ce, paramIndex).objData) : null; }
    protected T CastMetaData<T>() where T : UnityEngine.Object { return m_MetaData != null ? (T)m_MetaData : null; } 
    protected bool IsParamNull(CutsceneEvent ce, int paramIndex) { return Param(ce, paramIndex).objData == null; }

    protected float ParseFloat(string s)
    {
        float retVal = 0;
        if (string.IsNullOrEmpty(s))
        {
            return retVal;
        }

        if (!float.TryParse(s, out retVal))
        {
            retVal = 0;
        }

        return retVal;
    }
    #endregion
}
