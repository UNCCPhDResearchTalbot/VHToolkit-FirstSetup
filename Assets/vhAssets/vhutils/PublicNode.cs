using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PublicNode : MonoBehaviour
{
    public GameObject m_gameObject;
    public string m_nodeName;

    void Start()
    {
        // find object and parent it to node
        GameObject parent = Utils.FindChildRecursive(m_gameObject, m_nodeName);
        if (parent == null)
        {
            Debug.Log(String.Format("PublicNode - node {0} not found in object {1}", m_nodeName, m_gameObject.ToString()));
            return;
        }

        this.transform.parent = parent.transform;

        // move all components (except for this one) to the parent
        MonoBehaviour [] components = this.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            if (component == this)
                continue;

            // http://answers.unity3d.com/questions/12653/editor-wizard-copy-existing-components-to-another.html
        }
    }
}
