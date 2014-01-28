
//#define DEFINE_OBSOLETE_CLASS

using UnityEngine;
using System;
using System.Collections;

/*
    Created 2/25/2011 - Adam Reilly
    If you care about update order of your object,
    it needs to derive from this class instead of directly from MonoBehaviour.
    In the Unity Editor, set the InitialPriority integer on the gameobject to the desired value
*/



#if DEFINE_OBSOLETE_CLASS
//[Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
public class OrderedBehaviour : MonoBehaviour
{
    // lower numbers are updated before higher numbers.
    // i.e. objects with priority 0 are updated before priority 1
    public int InitialPriority;

    int m_currentPriority; // for internal use only, don't make public
    bool m_destroyOnLevelLoad = true;

    // for internal use only, don't make public
    // sometime gameobjects in the scene will be deactived by default so
    // VHAwake and VHStart won't get called on them. This fixes that and makes sure
    // those functions don't get called multiple times
    bool m_hasBeenActivated = false;

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public int CurrentPriority
    {
        get { return m_currentPriority; }
    }

#if !DEFINE_OBSOLETE_CLASS
    public virtual void Start() { }
#endif

    /// <summary>
    /// Changes the update order of the OrderedBehaviour
    /// </summary>
    /// <param name="newPriority"></param>
    public void ChangePriority(int newPriority)
    {
#if !DEFINE_OBSOLETE_CLASS
        if (OrderedBehaviourManager.Get() != null)
        {
            OrderedBehaviourManager.Manager.ResortBehaviour(this, newPriority);
            m_currentPriority = newPriority;
        }
#endif
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public bool DestroyOnLevelLoad
    {
        get { return m_destroyOnLevelLoad; }
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public void InitPriority() { m_currentPriority = InitialPriority; }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void AwakeOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void StartOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void UpdateOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void LateUpdateOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void FixedUpdateOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnApplicationQuitOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnGUIOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnDrawGizmosOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnDestroyOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnEnableOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnDisableOrdered() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void OnLevelWasLoadedOrdered() { }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHAwake() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHStart() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHUpdate() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHLateUpdate() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHFixedUpdate() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnApplicationQuit() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnGUI() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnDrawGizmos() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnDestroy() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnEnable() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnDisable() { }
#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void VHOnLevelWasLoaded() { }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public virtual void DestroyOrdered()
    {
#if !DEFINE_OBSOLETE_CLASS
        if (OrderedBehaviourManager.Get() != null)
        {
            OrderedBehaviourManager.Manager.RemoveBehaviour(this, CurrentPriority);
        }
        Destroy(gameObject);
#endif
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public void DontDestroyOnLoadOrdered()
    {
        m_destroyOnLevelLoad = false;
        DontDestroyOnLoad(gameObject);
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public static T Create<T>(T behaviour) where T : OrderedBehaviour
    {
        return Create(behaviour, Vector3.zero, Quaternion.identity);
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public static T Create<T>(string resourcePath) where T : OrderedBehaviour
    {
        return Create((T)Resources.Load(resourcePath, typeof(T)), Vector3.zero, Quaternion.identity);
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public static T Create<T>(string resourcePath, Vector3 position, Quaternion rotation) where T : OrderedBehaviour
    {
        return Create((T)Resources.Load(resourcePath, typeof(T)), Vector3.zero, Quaternion.identity);
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public static T Create<T>(T behaviour, Vector3 position, Quaternion rotation) where T : OrderedBehaviour
    {
        T vhBehavior = (T)Instantiate(behaviour, position, rotation);
        if (vhBehavior != null)
        {
            vhBehavior.Activate();
        }

        return vhBehavior;
    }

#if DEFINE_OBSOLETE_CLASS
    [Obsolete("OrderedBehaviour is obsolete.", false)]
#endif
    public void Activate()
    {
#if !DEFINE_OBSOLETE_CLASS
#if UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 ||UNITY_3_3 ||UNITY_3_4 || UNITY_3_5
        bool isActive = gameObject.active;
#else
        bool isActive = gameObject.activeSelf;
#endif
        if (OrderedBehaviourManager.Manager.AddBehaviour(this, InitialPriority)
            && isActive && !m_hasBeenActivated)
        {
            m_hasBeenActivated = true;
            //gameObject.active = true;
            InitPriority();

            switch (OrderedBehaviourManager.Manager.CurrentState)
            {
                case OrderedBehaviourManager.State.Awake:
                    AwakeOrdered();
                    VHAwake();
                    break;

                case OrderedBehaviourManager.State.Start:
                    AwakeOrdered();
                    VHAwake();
                    StartOrdered();
                    VHStart();
                    break;

                case OrderedBehaviourManager.State.Update:
                    AwakeOrdered();
                    VHAwake();
                    StartOrdered();
                    VHStart();
                    UpdateOrdered();
                    VHUpdate();
                    break;

                case OrderedBehaviourManager.State.Shutdown:
                    AwakeOrdered();
                    VHAwake();
                    StartOrdered();
                    VHStart();
                    OnApplicationQuitOrdered();
                    VHOnApplicationQuit();
                    break;
            }
        }
#endif
    }
}
