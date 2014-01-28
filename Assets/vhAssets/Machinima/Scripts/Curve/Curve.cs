using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[RequireComponent(typeof())]
[ExecuteInEditMode]
public class Curve : MonoBehaviour
{
    /*#region Constants
    static Color LineColor = new Color(1, 1, 0, 1);
    #endregion*/

    #region Variables
    public Color m_LineColor = new Color(1, 1, 0, 1);
    [HideInInspector]
    public List<Transform> points = new List<Transform>();
    #endregion

    #region Functions

	// Update is called once per frame
	void Update ()
    {
        if (!Application.isPlaying)
        {
            UpdatePoints();
        } 
	}

    void UpdatePoints()
    {
        if (transform.childCount <= 0) 
        {
		    GameObject newObject = new GameObject("Point0");
		    newObject.AddComponent(typeof(CurvePoint));
            newObject.AddComponent(typeof(Camera));
            newObject.GetComponent<Camera>().enabled = false;
		    newObject.transform.parent = transform;
		    newObject.transform.localPosition = Vector3.zero;
	    }

        List<Transform> tmpArray = new List<Transform>();
	    foreach (Transform t in transform) 
        {
		    tmpArray.Add(t);
		    break;
	    }

	    foreach (Transform t in transform) 
        {
		    tmpArray.Add(t);
	    }

	    tmpArray.Add(tmpArray[tmpArray.Count - 1]);
	    int i = 0;
	    foreach (Transform t in tmpArray)
        {
		    if (i == 0) 
            {
			    i++;
			    continue;
		    }
		    if (i == tmpArray.Count - 1)
            {
			    i++;
			    continue;
		    }
		    if (t == gameObject.transform)
            {
			    i++;
			    continue;
		    }
		    string newName = "Point"+i;
		    string currentName = t.gameObject.name;
		    if (currentName != newName)
            {
			    t.gameObject.name = newName;
		    }
		    i++;
	    }

        points = tmpArray;
    }

    void OnDrawGizmos()
    {
        if (points == null) 
            return;

	    if (points.Count < 3)
            return;

	    Vector3 lastPos = transform.position;
	    lastPos = points[0].position;
        Gizmos.color = m_LineColor;

	    for (int i = 1; i < points.Count * 8; i++) 
        {
		    float ratio =  (float)(i) / (float)(points.Count * 8);
		    Vector3 pos = GetPosition(ratio);
			Gizmos.DrawLine(lastPos,pos);		    
		    lastPos = pos;
	    }
    }

    public Vector3 GetForwardNormal(float p, float sampleDist)
    {
	    float curveLength = GetLength();
	    Vector3 pos = GetPosition(p);
	    Vector3 frontPos = GetPosition(p+(sampleDist/curveLength));
	    Vector3 backPos = GetPosition(p-(sampleDist/curveLength));
	    Vector3 frontNormal = (frontPos-pos).normalized;
	    Vector3 backNormal = (backPos-pos).normalized;
	    Vector3 normal = Vector3.Slerp(frontNormal, -backNormal, 0.5f);
	    normal.Normalize();
	    return normal;
    }

    public Vector3 GetPosition(float time)
    {
        return GetPosition(time, true);
    }

    public void FollowCurveAtSpeed(GameObject follower, float speed)
    {
        FollowCurve(follower, GetLength() / speed);
    }

    public void FollowCurveAtSpeed(GameObject follower, float speed, Transform lookAtTarget)
    {
        FollowCurve(follower, GetLength() / speed, lookAtTarget);
    }

    public void FollowCurve(GameObject follower, float secondsToReachEnd)
    {
        FollowCurve(follower, secondsToReachEnd, null);
    }

    public void FollowCurve(GameObject follower, float secondsToReachEnd, Transform lookAtTarget)
    {
        StartCoroutine(FollowCurveCoroutine(follower, secondsToReachEnd, lookAtTarget));
    }

    IEnumerator FollowCurveCoroutine(GameObject follower, float secondsToReachEnd, Transform lookAtTarget)
    {
        float timePassed = 0;
        while (timePassed < secondsToReachEnd)
        {
            float interpolation = timePassed / secondsToReachEnd;
            follower.transform.localPosition = GetPosition(interpolation, true);
            if (lookAtTarget != null)
            {
                follower.transform.LookAt(lookAtTarget);
            }
            else
            {
                follower.transform.forward = GetForwardDirection(interpolation, true);
            }
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        follower.transform.localPosition = GetPosition(1.0f);
        follower.transform.forward = GetForwardDirection(1.0f, true);
    }

    int GetCurrentPointIndex(float time)
    {
        int numSections = points.Count - 3;
        return Mathf.Min(Mathf.FloorToInt(Mathf.Clamp01(time) * numSections), numSections - 1);
    }

    bool GetCurveParameters(float time, ref Transform a, ref Transform b, ref Transform c, ref Transform d, ref float u)
    {
        try
        {
            int numSections = points.Count - 3;

            if (numSections <= 0)
                return false;

            int currPt = GetCurrentPointIndex(time);
            u = time * numSections - currPt;
            a = points[currPt];
            b = points[currPt + 1];
            c = points[currPt + 2];
            d = points[currPt + 3];

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Vector3 GetPosition(float time, bool clamp)
    {
	    if (clamp) 
        {
            time = Mathf.Clamp01(time);
	    }
	    try 
        {
            Transform a = null, b = null, c = null, d = null;
            float u = 0;
            GetCurveParameters(time, ref a, ref b, ref c, ref d, ref u);
            return InterpolateCurve(a.position, b.position, c.position, d.position, u);
	    }
	    catch 
        {
		    return Vector3.zero;
	    }
    }

    public Vector3 GetForwardDirection(float time)
    {
        return GetPosition(time, true);
    }

    public Vector3 GetForwardDirection(float time, bool clamp)
    {
        if (clamp)
        {
            time = Mathf.Clamp01(time);
        }
        try
        {
            Transform a = null, b = null, c = null, d = null;
            float u = 0;
            GetCurveParameters(time, ref a, ref b, ref c, ref d, ref u);
            return InterpolateCurve(a.forward, b.forward, c.forward, d.forward, u);
        }
        catch
        {
            return Vector3.zero;
        }
    }

    float GetLength()
    {
	    if (points.Count < 3) 
            return 0;

	    float l = 0;
	    for (int i = 1; i < points.Count - 2; i++)
        {
		    if (!points[i] || !points[i+1])
                return 0;
		    l += Vector3.Distance(points[i].position,points[i+1].position);
	    }
	    return l;
    }

    //public float GetCurveLength()
    //{
    //    return 0;
    //}

    public static Vector3 InterpolateCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u)
    {
        // cubic berzier curve
        return 0.5f * ((-a + 3.0f * b - 3.0f * c + d) * (u * u * u) + (2.0f * a - 5.0f * b + 4.0f * c - d) * (u * u) + (-a + c) * u + 2.0f * b);
    }

    #endregion
}
