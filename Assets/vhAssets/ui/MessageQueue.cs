using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageQueue : MonoBehaviour
{
    #region Constants
    class MessageData
    {
        public Rect position;
        public string message;
        public Color color;
        public float timeSpentRendered;
    }
    #endregion

    #region Variables
    public int m_MaxVisible = 3;
    public float m_DisplayLengthBeforeFade = 1.0f;
    public float m_FadeLength = 1.0f;
    public Color m_MessageColor = Color.white;
    public Rect m_MessageDisplayArea = new Rect(10, 10, 300, 66);

    // private
    List<MessageData> m_DisplayList = new List<MessageData>();
    Queue<string> m_MessageQueue = new Queue<string>();
    bool m_ShowMessages = true;
    #endregion

    #region Properties
    public bool ShowMessages
    {
        get { return m_ShowMessages; }
        set { m_ShowMessages = value; }
    }

    float LineHeight
    {
        get { return m_MessageDisplayArea.height / m_MaxVisible; }
    }
    #endregion

    #region Functions
    public void Start()
    {
        AdjustSettings();
        //System.GC.RegisterForFullGCNotification(
    }

    public void Update()
    {
#if UNITY_EDITOR
        AdjustSettings();
#endif
        // check if the messages have expired
        for (int i = 0; i < m_DisplayList.Count; i++)
        {
            m_DisplayList[i].timeSpentRendered += Time.deltaTime;

            if (m_DisplayList[i].timeSpentRendered > m_DisplayLengthBeforeFade)
            {
                // do the fade
                m_DisplayList[i].color.a = 1.0f - (m_DisplayList[i].timeSpentRendered - m_DisplayLengthBeforeFade) / m_FadeLength;
                m_DisplayList[i].color.a = Mathf.Clamp01(m_DisplayList[i].color.a);

                if (m_DisplayList[i].timeSpentRendered >= m_DisplayLengthBeforeFade + m_DisplayLengthBeforeFade)
                {
                    // the message has displayed long enough, remove it.
                    m_DisplayList.RemoveAt(i--);
                    CalculateMessagePositions();
                }
            }
        }

        // check to display new messages
        if (m_DisplayList.Count < m_MaxVisible
            && m_MessageQueue.Count > 0
            && ShowMessages)
        {
            DisplayMessage(m_MessageQueue.Dequeue());
        }
    }

    public void OnGUI()
    {
        Color originalColor = GUI.color;

        for (int i = 0; i < m_DisplayList.Count; i++)
        {
            GUI.color = m_DisplayList[i].color;
            GUI.Label(m_DisplayList[i].position, m_DisplayList[i].message);
        }

        GUI.color = originalColor;
    }

    public void EnqueueMessage(string message)
    {
        m_MessageQueue.Enqueue(message);
    }

    private void DisplayMessage(string message)
    {
        // create new message to display
        MessageData newMessage = new MessageData();
        newMessage.message = message;
        newMessage.color = m_MessageColor;
        newMessage.position = new Rect(m_MessageDisplayArea.x, m_MessageDisplayArea.y + m_DisplayList.Count * LineHeight,
            m_MessageDisplayArea.width, LineHeight);
        m_DisplayList.Add(newMessage);
    }

    private void CalculateMessagePositions()
    {
        for (int i = 0; i < m_DisplayList.Count; i++)
        {
            m_DisplayList[i].position.y = m_MessageDisplayArea.y + i * LineHeight;
        }
    }

    private void AdjustSettings()
    {
        m_MaxVisible = Mathf.Max(1, m_MaxVisible);
        m_MessageDisplayArea.height = 22 * m_MaxVisible; // default font height is 22
    }
    #endregion
}
