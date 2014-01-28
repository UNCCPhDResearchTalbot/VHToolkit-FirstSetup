using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*
    Created 3/8/2011 - Adam Reilly
    Unity inherently does not support a controlled creation, update, and destruction
    order for its gameobjects that use MonoBehaviours.  This class provides that functionality
    by using a SortedDictionary based on a 'Priority' value defined in OrderedBehaviour that can be set
    in the Unity Inspector window when selecting a gameobject with a VHBheaviour component.

*/

/*
 * Unity Execution order, taken from http://unity3d.com/support/documentation/Manual/Execution%20Order.html
 *
 * -=Initialization=-
 * Awake() - once
 * OnEnable() - once, only called if the gameobject is active, just after the object is enabled
 * Start() - once
 *
 * -=Update=-
 * FixedUpdate() - every frame sometimes multiple times per frame. Called more frequently than update. Do all physics calculations here
 * Update() - every frame, once per frame
 * LateUpdate() - every frame, once per frame. All calculations in Update are completed before LateUpdate is called
 *
 * -=Coroutines=-
 * Executed after all Update functions. If you start a coroutine in LateUpdate, it will be called after LateUpdate, just before rendering
 *
 * -=Combined Execution order=-
 * So in conclusion, this is the execution order for any given script:

    * All Awake calls
    * All Start Calls
    * while (stepping towards variable delta time)
          o All FixedUpdate functions
          o Physics simulation
          o OnEnter/Exit/Stay trigger functions
          o OnEnter/Exit/Stay collision functions
    * Rigidbody interpolation applies transform.position and rotation
    * OnMouseDown/OnMouseUp etc. events
    * All Update functions
    * Animations are advanced, blended and applied to transform
    * All LateUpdate functions
    * Rendering
*/

//[Obsolete("OrderedBehaviourManager class has been removed. Use Unity Defined Orders", false)]
public class OrderedBehaviourManager : MonoBehaviour
{
    #region Constants
    public enum State
    {
        Awake,
        Start,
        Update,
        Shutdown,
    }
    #endregion

    #region Variables
    // int = priority, lower numbers are updated before higher numbers.
    // i.e. OrderedBehaviour objects with priority 0 are updated before priority 1
    SortedDictionary<int, List<OrderedBehaviour>> m_behaviors = new SortedDictionary<int, List<OrderedBehaviour>>();
    List<KeyValuePair<int, OrderedBehaviour>> m_queuedBehavioursToBeAdded = new List<KeyValuePair<int, OrderedBehaviour>>();
    List<KeyValuePair<int, OrderedBehaviour>> m_queuedBehavioursToBeRemoved = new List<KeyValuePair<int, OrderedBehaviour>>();

    State m_currentState = State.Awake;
    bool m_disableGUI = false;

#if UNITY_EDITOR
    bool m_isCompiling = false;
#endif

    bool m_destroyOnLevelLoad = true;

    // there should only ever be one gameobject in the scene with a OrderedBehaviourManager component attached
    protected static OrderedBehaviourManager manager;
    #endregion

    #region Properties
    static public OrderedBehaviourManager Manager
    {
        get { return manager; }
    }

    public State CurrentState
    {
        get { return m_currentState; }
    }

    public bool DisableGUI
    {
        get { return m_disableGUI; }
        set { m_disableGUI = value; }
    }

    public bool DestroyOnLevelLoad
    {
        get { return m_destroyOnLevelLoad; }
        set { m_destroyOnLevelLoad = value; }
    }
    #endregion

    public static OrderedBehaviourManager Get()
    {
        if (manager == null)
        {
            manager = UnityEngine.Object.FindObjectOfType(typeof(OrderedBehaviourManager)) as OrderedBehaviourManager;
        }

        return manager;
    }

    #region Unity Messages
    public virtual void Awake()
    {
        if (manager != null)
        {
            return;
        }

        m_currentState = State.Awake;
        // there should only ever be one gameobject in the scene with a VHMain component attached
        manager = this;

        FindAllBehaviours();

        CallAwake();
    }

    void CallAwake()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].DestroyOnLevelLoad && kvp.Value[i].enabled)
                {
                    kvp.Value[i].InitPriority();
                    kvp.Value[i].AwakeOrdered();
                    kvp.Value[i].VHAwake();
                }
            }
        }
        MergeQueuedListsWithDictionary();
    }

    private void FindAllBehaviours()
    {
        // get all of the OrderedBehaviors currently in the scene and add them to our sorted dictionary
        OrderedBehaviour[] orderedBehaviors = (OrderedBehaviour[])Component.FindObjectsOfType(typeof(OrderedBehaviour));
        if (orderedBehaviors != null)
        {
            for (int i = 0; i < orderedBehaviors.Length; i++)
            {
                AddBehaviour(orderedBehaviors[i], orderedBehaviors[i].InitialPriority);
            }
        }

        MergeQueuedListsWithDictionary();
    }

    public virtual void Start()
    {
        //Debug.Log( "OrderedBehaviorManager.Start()" );

        m_currentState = State.Start;
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].DestroyOnLevelLoad && kvp.Value[i].enabled)
                {
                    kvp.Value[i].StartOrdered();
                    kvp.Value[i].VHStart();
                }
            }
        }

        MergeQueuedListsWithDictionary();

        // start is finished, move to next state
        m_currentState = State.Update;
    }

    public virtual void FixedUpdate()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].FixedUpdateOrdered();
                    kvp.Value[i].VHFixedUpdate();
                }
            }
        }
    }

    public virtual void OnLevelWasLoaded()
    {
        if (DestroyOnLevelLoad)
        {
            return;
        }

        if (manager != this)
        {
            return;
        }

        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].OnLevelWasLoadedOrdered();
                    kvp.Value[i].VHOnLevelWasLoaded();
                }
            }
        }
        ClearBehaviours();
        FindAllBehaviours();
        CallAwake();
        Start();
    }

    public static void VHDontDestroyOnLoad(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        OrderedBehaviour vhBehave = (OrderedBehaviour)obj.GetComponent<OrderedBehaviour>();
        if (vhBehave != null)
        {
            vhBehave.DontDestroyOnLoadOrdered();
        }
        else
        {
           DontDestroyOnLoad(obj);
        }

        if ((OrderedBehaviour)obj.GetComponent<OrderedBehaviour>() != null)
        {
            OrderedBehaviourManager.Get().DestroyOnLevelLoad = false;
            DontDestroyOnLoad(obj);
        }
    }

    public virtual void OnEnable()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].OnEnableOrdered();
                    kvp.Value[i].VHOnEnable();
                }
            }
        }
    }

    public virtual void OnDisable()
    {
        SortedDictionary<int, List<OrderedBehaviour>> reverseBehaviours =
            new SortedDictionary<int, List<OrderedBehaviour>>(m_behaviors, new DescendingComparer<int>());

        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in reverseBehaviours)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                kvp.Value[i].OnDisableOrdered();
                kvp.Value[i].VHOnDisable();
            }
        }
    }

    public virtual void Update()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isCompiling && !m_isCompiling)
        {
            m_isCompiling = true;
        }
        else if (!UnityEditor.EditorApplication.isCompiling && m_isCompiling)
        {
            m_isCompiling = false;
            FindAllBehaviours();
        }
#endif
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].UpdateOrdered();
                    kvp.Value[i].VHUpdate();
                }
            }
        }
    }

    public virtual void LateUpdate()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].LateUpdateOrdered();
                    kvp.Value[i].VHLateUpdate();
                }
            }
        }
        MergeQueuedListsWithDictionary();
    }

    public virtual void OnGUI()
    {
        if (DisableGUI)
        {
            return;
        }

        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].OnGUIOrdered();
                    kvp.Value[i].VHOnGUI();
                }
            }
        }
    }

    public virtual void OnDrawGizmos()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i] != null && kvp.Value[i].enabled)
                {
                    kvp.Value[i].OnDrawGizmosOrdered();
                    kvp.Value[i].VHOnDrawGizmos();
                }
            }
        }
    }

    public virtual void OnApplicationQuit()
    {
        m_currentState = State.Shutdown;

        SortedDictionary<int, List<OrderedBehaviour>> reverseBehaviours =
            new SortedDictionary<int, List<OrderedBehaviour>>(m_behaviors, new DescendingComparer<int>());

        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in reverseBehaviours)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                kvp.Value[i].OnApplicationQuitOrdered();
                kvp.Value[i].VHOnApplicationQuit();
            }
        }
    }

    public virtual void OnDestroy()
    {
        m_currentState = State.Shutdown;

        // destroy in reverse creation order
        SortedDictionary<int, List<OrderedBehaviour>> reverseBehaviours =
            new SortedDictionary<int, List<OrderedBehaviour>>(m_behaviors, new DescendingComparer<int>());

        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in reverseBehaviours)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                kvp.Value[i].OnDestroyOrdered();
                kvp.Value[i].VHOnDestroy();
            }
        }
    }
    #endregion

    #region Main Functionality

    void MergeRemoveListWithDictionary()
    {
        // first do removals
        for (int i = 0; i < m_queuedBehavioursToBeRemoved.Count; i++)
        {
            RemoveBehaviourInternal(m_queuedBehavioursToBeRemoved[i].Value);
        }
        m_queuedBehavioursToBeRemoved.Clear();
    }

    void MergedAddListWithDictionary()
    {
        // now additions
        for (int i = 0; i < m_queuedBehavioursToBeAdded.Count; i++)
        {
            AddBehaviourInternal(m_queuedBehavioursToBeAdded[i].Value, m_queuedBehavioursToBeAdded[i].Key);
        }

        m_queuedBehavioursToBeAdded.Clear();
    }

    void MergeQueuedListsWithDictionary()
    {
        MergeRemoveListWithDictionary();
        MergedAddListWithDictionary();
    }

    /// <summary>
    /// Allows control over the update priority of an object
    /// </summary>
    /// <param name="orderedBehaviour">The OrderedBehaviour you want prioritized</param>
    /// <returns>true if the object was successfully added to the List in the SortedDictionary</returns>
    bool AddBehaviourInternal(OrderedBehaviour orderedBehaviour, int priority)
    {
        bool retVal = false;
        if (!m_behaviors.ContainsKey(priority))
        {
            // a new list has to be created
            m_behaviors.Add(priority, new List<OrderedBehaviour>());
        }

        if (!m_behaviors[priority].Contains(orderedBehaviour))
        {
            m_behaviors[priority].Add(orderedBehaviour);
            retVal = true;
        }

        return retVal;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="orderedBehaviour"></param>
    /// <returns>true if the object was successfully removed from the List in the SortedDictionary</returns>
    bool RemoveBehaviourInternal(OrderedBehaviour orderedBehaviour)
    {
        if (!m_behaviors.ContainsKey(orderedBehaviour.CurrentPriority))
        {
            return false;
        }

        m_behaviors[orderedBehaviour.CurrentPriority].Remove(orderedBehaviour);
        return true;
    }

    /// <summary>
    /// Adds a behaviour to be updated by the manager
    /// </summary>
    /// <param name="orderedBehaviour"></param>
    /// <param name="priority"></param>
    /// <returns>true if the behaviour will successfully be added, false if the behaviour already exists in the manager</returns>
    public bool AddBehaviour(OrderedBehaviour orderedBehaviour, int priority)
    {
        if (!m_behaviors.ContainsKey(priority) || !m_behaviors[priority].Contains(orderedBehaviour))
        {
            m_queuedBehavioursToBeAdded.Add(new KeyValuePair<int, OrderedBehaviour>(priority, orderedBehaviour));
            return true;
        }

        return false;
    }

    /// <summary>
    /// removes a behaviour from the update list
    /// </summary>
    /// <param name="orderedBehaviour">the behaviour to remove</param>
    /// <param name="priority"></param>
    /// <returns>true if the behaviour was successfully removed, false if it couldn't be found</returns>
    public bool RemoveBehaviour(OrderedBehaviour orderedBehaviour, int priority)
    {
        if (m_behaviors.ContainsKey(priority) && m_behaviors[priority].Contains(orderedBehaviour))
        {
            m_queuedBehavioursToBeRemoved.Add(new KeyValuePair<int, OrderedBehaviour>(priority, orderedBehaviour));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Resorts the behaviour to the new priority
    /// </summary>
    /// <param name="orderedBehaviour">the behaviour to be sorted</param>
    /// <param name="newPriority">the intended update priority</param>
    /// <returns></returns>
    public bool ResortBehaviour(OrderedBehaviour orderedBehaviour, int newPriority)
    {
        if (!RemoveBehaviour(orderedBehaviour, orderedBehaviour.CurrentPriority))
        {
            RemoveBehaviour(orderedBehaviour, orderedBehaviour.InitialPriority);
        }
        return AddBehaviour(orderedBehaviour, newPriority);
    }

    public void ClearBehaviours()
    {
        foreach (KeyValuePair<int, List<OrderedBehaviour>> kvp in m_behaviors)
        {
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                kvp.Value.Clear();
            }
        }

        m_behaviors.Clear();
    }

    #endregion
}


class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
{
    public int Compare(T x, T y)
    {
        return y.CompareTo(x);
    }
}
