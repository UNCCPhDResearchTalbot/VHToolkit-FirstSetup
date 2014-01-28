using UnityEngine;
using System.Collections;

public class FpsCounter : MonoBehaviour
{
    private const int m_numFpsEntries = 50;
    private float [] m_averageFpsArray = new float[m_numFpsEntries];
    private float m_averageSum = 0;

    private float m_fps;
    private float m_smoothFps;
    private float m_averageFps;


    public float Fps { get { return m_fps; } }
    public float SmoothFps { get { return m_smoothFps; } }
    public float AverageFps { get { return m_averageFps; } }


    public void Start()
    {
    }

    public void Update()
    {
        float adjustedDeltaTime;
        float adjustedSmoothDeltaTime;

        if (Time.deltaTime == 0 || Time.timeScale == 0)
        {
            // alternate computation, not really accurate, but just here to give some number.
            adjustedDeltaTime = Time.realtimeSinceStartup / Time.frameCount;
            adjustedSmoothDeltaTime = Time.realtimeSinceStartup / Time.frameCount;
        }
        else
        {
            adjustedDeltaTime = Time.deltaTime / Time.timeScale;
            adjustedSmoothDeltaTime = Time.smoothDeltaTime / Time.timeScale;
        }

        m_fps = 1 / adjustedDeltaTime;
        m_smoothFps = 1 / adjustedSmoothDeltaTime;

        // compute average
        int i = Time.frameCount % m_numFpsEntries;
        m_averageSum -= m_averageFpsArray[ i ];
        m_averageFpsArray[ i ] = adjustedDeltaTime;
        m_averageSum += adjustedDeltaTime;
        m_averageFps = m_numFpsEntries / m_averageSum;
    }
}
