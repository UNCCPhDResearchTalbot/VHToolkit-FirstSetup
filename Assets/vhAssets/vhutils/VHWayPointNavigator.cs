using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VHWayPointNavigator : MonoBehaviour
{
    #region Constants
    public enum PathLoopType
    {
        // when you reach the end of a path...
        Loop,       // return to the path start point and do it again
        PingPong,   // go back the way you came
        Stop,       // just stop
    }

    public enum PathFollowingType
    {
        // how you move along the path
        Lerp,
    }

    public delegate void OnPathCompleted(List<Vector3> pathFollowed);
    public delegate void OnWayPointReached(VHWayPointNavigator navaigator, Vector3 wp, int wpId, int totalNumWPs);
    #endregion

    #region Variables
    public float m_Speed = 10;
    public float m_AngularVelocity = 90;
    public bool m_TurnTowardsTargetPosition = true;
    public bool m_IgnoreHeight = false;
    public bool m_ImmediatelyStartPathing = true;
    public PathLoopType m_LoopType = PathLoopType.Stop;
    public PathFollowingType m_FollowingType = PathFollowingType.Lerp;
    public GameObject m_Pather;
    public GameObject[] WayPoints;

    List<Vector3> m_WayPoints = new List<Vector3>();
    int m_nPrevWP = -1; // the waypoint that you're coming from
    int m_nNextWP = 0; // the waypoint that you are moving towards
    float m_fTimeToReachTarget;
    float m_fCurrentTime = 0;
    bool m_bInReverse = false;
    bool m_bIsPathing = false;
    //Vector3 m_PathDirection = new Vector3();

    protected OnPathCompleted m_PathCompletedCallback;
    protected OnWayPointReached m_WayPointReachedCallback;
    Rect m_ScreenSpace;
    #endregion

    #region Properties
    public Vector3 PreviousPosition
    {
        get { return m_WayPoints[m_nPrevWP]; }
    }

    public Vector3 TargetPosition
    {
        get { return m_WayPoints[m_nNextWP]; }
    }

    public int PrevWP
    {
        get { return m_nPrevWP; }
    }

    public int NextWP
    {
        get { return m_nNextWP; }
    }

    public bool IsPathing
    {
        get { return m_bIsPathing; }
    }

    public int NumWayPoints
    {
        get { return m_WayPoints.Count; }
    }

    public Vector3 TurnTarget
    {
        get { return m_IgnoreHeight ? new Vector3(TargetPosition.x, m_Pather.transform.position.y, TargetPosition.z) : TargetPosition; }
    }
    #endregion

    #region Functions

    public void SetIsPathing(bool isPathing)
    {
        if (isPathing && m_LoopType == PathLoopType.Stop
            && (m_nPrevWP >= m_WayPoints.Count || m_nNextWP >= m_WayPoints.Count))
        {
            // they've already completed their path, so there's nowhere to path to
            Debug.Log("Can't start moving again. You've completed your path and your loop type is \'Stop\'");
            return;
        }

        m_bIsPathing = isPathing;
    }

    //public override void VHStart()
    public void Start()
    {
        if (m_Pather == null)
        {
            m_Pather = gameObject;
        }

        if (m_ImmediatelyStartPathing)
        {
            NavigatePath(true);
        }
    }

    List<Vector3> ConvertWayPointGOsToPositions(GameObject[] waypoints)
    {
        List<Vector3> waypointPositions = new List<Vector3>();
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypointPositions.Add(waypoints[i].transform.position);
        }
        return waypointPositions;
    }

    public void resetPathNavigator()
    {
        m_nPrevWP = -1;
        m_nNextWP = 0;
        m_WayPoints.Clear();
    }

    public void NavigatePath(bool forcePositionToFirstPoint)
    {
        NavigatePath(ConvertWayPointGOsToPositions(WayPoints), forcePositionToFirstPoint);
    }

    /// <summary>
    /// Start navigating the given waypoints
    /// </summary>
    /// <param name="wayPoints"></param>
    public void NavigatePath(List<Vector3> wayPoints, bool forcePositionToFirstPoint)
    {
        if (wayPoints == null || wayPoints.Count < 2)
        {
            Debug.LogError("Bad path passed into NavigatePath");
            return;
        }

        if (forcePositionToFirstPoint)
        {
            m_Pather.transform.position = wayPoints[0];
            m_Pather.transform.forward = (wayPoints[1] - wayPoints[0]).normalized;
        }

        SetPath(wayPoints, true);

        m_nPrevWP = -1;
        m_nNextWP = 0;

        SetIsPathing(true);
        MoveToNextWayPoint();

        Utils.TurnTowardsTarget(this, m_Pather, TargetPosition, 360);
    }

    /// <summary>
    /// This doesn't reset the current and next wp indices
    /// </summary>
    /// <param name="wayPoints"></param>
    public void SetPath(List<Vector3> wayPoints, bool bContinuePath)
    {
        m_WayPoints.Clear();
        m_WayPoints.AddRange(wayPoints);

        if (m_nPrevWP > wayPoints.Count - 1)
        {
            Debug.LogError("you're new path is shorter than what you have already traversed");
        }

        SetIsPathing(true);
        if(!bContinuePath)
            MoveToNextWayPoint();
    }

    //public override void VHUpdate()
    public void Update()
    {
        if (!IsPathing)
        {
            return;
        }

        //m_PathDirection = MovementDirection();
        m_Pather.transform.position = Vector3.Lerp(PreviousPosition, TargetPosition, m_fCurrentTime / m_fTimeToReachTarget);

        if (m_fCurrentTime >= m_fTimeToReachTarget)
        {
            // they reached the next point, clamp their position to what their target was
            MoveToNextWayPoint();
        }

        m_fCurrentTime += Time.deltaTime;
    }


    void MoveToNextWayPoint()
    {

        // clamp their position to where they were going
        m_Pather.transform.position = m_IgnoreHeight ? new Vector3(TargetPosition.x, m_Pather.transform.position.y, TargetPosition.z) : TargetPosition;
        m_fCurrentTime = 0;

        bool bPathComplete = false;
        switch (m_LoopType)
        {
            case PathLoopType.Loop:
                if (++m_nPrevWP >= m_WayPoints.Count)
                {
                    m_nPrevWP = 0;
                }
                if (++m_nNextWP >= m_WayPoints.Count)
                {
                    m_nNextWP = 0;
                    bPathComplete = true;
                }

                break;

            case PathLoopType.PingPong:
                if (m_bInReverse)
                {
                    if (--m_nPrevWP < 0)
                        m_nPrevWP = 0;
                    if (--m_nNextWP < 0)
                    {
                        m_nNextWP = 1;
                        m_bInReverse = false;
                        bPathComplete = true;
                    }
                }
                else
                {
                    if (++m_nPrevWP >= m_WayPoints.Count)
                        m_nPrevWP = m_WayPoints.Count - 1;
                    if (++m_nNextWP >= m_WayPoints.Count)
                    {
                        m_nNextWP = m_WayPoints.Count - 2;
                        m_bInReverse = true;
                        bPathComplete = true;
                    }
                }
                //m_fTimeToReachTarget = Utils.GetTimeToReachPosition(PreviousPosition, TargetPosition, m_Speed);
                break;

            case PathLoopType.Stop:
                ++m_nPrevWP;
                ++m_nNextWP;
                if (m_nPrevWP >= m_WayPoints.Count || m_nNextWP >= m_WayPoints.Count)
                {
                    // they reached the end of the path
                    SetIsPathing(false);
                    bPathComplete = true;
                }
                break;
           }

        if (IsPathing)
        {
            // calculate time to reach next wp
            m_fTimeToReachTarget = Utils.GetTimeToReachPosition(PreviousPosition, TargetPosition, m_Speed);
            //m_PathDirection = MovementDirection();
            //m_TurnTowardsTargetPosition = true;

            if (m_TurnTowardsTargetPosition)
            {
                StopCoroutine("Internal_TurnTowardsTarget");
                Utils.TurnTowardsTarget(this, m_Pather, TurnTarget, m_AngularVelocity);
            }
        }

        if (m_WayPointReachedCallback != null)
        {
            m_WayPointReachedCallback(this, PreviousPosition, PrevWP, m_WayPoints.Count);
        }

        if (bPathComplete && m_PathCompletedCallback != null)
        {
            m_PathCompletedCallback(m_WayPoints);
        }
    }

    /// <summary>
    /// Setups a callback that will be called when the path has been completely traversed
    /// </summary>
    /// <param name="cb"></param>
    public void AddPathCompletedCallback(OnPathCompleted cb)
    {
        m_PathCompletedCallback += cb;
    }

    public void AddWayPointReachedCallback(OnWayPointReached cb)
    {
        m_WayPointReachedCallback += cb;
    }

    /// <summary>
    /// Set how fast you move along your path
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        if (Mathf.Abs(m_Speed - speed) <= Mathf.Epsilon)
            return;

        m_Speed = speed;
        SetIsPathing(speed != 0);
    }

    public Vector3 MovementDirection()
    {
        Vector3 direction = (TargetPosition - m_Pather.transform.position);

        if (m_IgnoreHeight)
        {
            direction.y = 0;
        }
        direction.Normalize();
        return direction;
    }

    public float GetTotalPathLength()
    {
        if (m_WayPoints.Count == 0)
        {
            m_WayPoints = ConvertWayPointGOsToPositions(WayPoints);
        }

        float totalPathLength = 0;
        for (int i = 1; i < m_WayPoints.Count; i++)
        {
            totalPathLength += Vector3.Distance(m_WayPoints[i], m_WayPoints[i - 1]);
        }
        return totalPathLength;
    }

    #endregion
}
