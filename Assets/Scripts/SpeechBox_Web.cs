using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class SpeechBox_Web : MonoBehaviour
{
    #region Constants
    // ------> Constants
    public const string SpeechTextFieldName = "Speech Text Field";
    const string Splitter = "___MSG_SPLIT___";
    #endregion

    #region Variables
    // ------> Variables
    public VHMsgWebRequest vhmsg;
    public DebugConsole m_Console;
    public LoadingScreen m_LoadingScreen;
    public FreeMouseLook m_FreeMouseLook;
    public NPCEditorMessageBroker m_MessageBroker;
    public VHMsgWebRequestMain m_Main;

    string m_SpeechText = "Type your question";
    int m_SpeechUserID = 1;
    Rect m_SpeechTextFieldPos = new Rect(0, 0.95f, 0.9f, 0.05f);
    Rect m_SpeechSayButtonPos = new Rect(0.9f, 0.95f, 0.1f, 0.05f);
    List<string> m_SavedSpeech = new List<string>();
    int m_nPreviousSpeechIndex = 0;
    bool m_bShow = true;
    bool m_bGuiEnabled = true;

    #endregion

    #region Properties
    public bool IsSpeechTextBoxInFocus
    {
        get { return SpeechTextFieldName == GUI.GetNameOfFocusedControl(); }
    }

    public bool TypingEnabled
    {
        get { return m_bGuiEnabled; }
        set { m_bGuiEnabled = value; }
    }

    public bool Show { get { return m_bShow; } set { m_bShow = value; } }

    #endregion

    #region Functions
    void Start()
    {
        if (m_FreeMouseLook == null)
        {
            m_FreeMouseLook = (FreeMouseLook)Camera.main.GetComponent(typeof(FreeMouseLook));
        }

        // add in some default speech text that you can say to brad
        m_nPreviousSpeechIndex = m_SavedSpeech.Count;
    }

    void Update()
    {
        // speech box not in focus
        if (Input.GetKeyDown(KeyCode.L))
        {
            m_bShow = !m_bShow;
        }
    }

    void HandleInput()
    {
        if (Event.current == null || Event.current.type != EventType.KeyDown)
        {
            return;
        }

        // they are typing in the speech box
        if (IsSpeechTextBoxInFocus)
        {
            if (Event.current.character == '\n')
            {
                SendSpeechMessage(m_SpeechText);
            }
            else if (Event.current.keyCode == KeyCode.UpArrow)
            {
                if (m_nPreviousSpeechIndex > 0)
                {
                    m_SpeechText = m_SavedSpeech[--m_nPreviousSpeechIndex];
                }
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                if (m_nPreviousSpeechIndex < m_SavedSpeech.Count - 1)
                {
                    m_SpeechText = m_SavedSpeech[++m_nPreviousSpeechIndex];
                }
            }
        }
    }

    void OnGUI()
    {
        if (!m_bShow)
        {
            return;
        }

        bool prevEnabled = GUI.enabled;
        GUI.enabled = m_bGuiEnabled;

        HandleInput();

        // talk to brad and ask him questions
        if (!m_Console.DrawConsole)
        {
            string currentControlName = GUI.GetNameOfFocusedControl();
            GUI.SetNextControlName(SpeechTextFieldName);
            m_SpeechText = VHGUI.TextField(m_SpeechTextFieldPos, m_SpeechText);
            if (VHGUI.Button(m_SpeechSayButtonPos, "Say"))
            {
                SendSpeechMessage(m_SpeechText);
                GUI.FocusControl(SpeechTextFieldName);

                if (GUI.GetNameOfFocusedControl() == SpeechTextFieldName)
                {
                    HighlightText();
                }
            }

            if ((currentControlName != SpeechTextFieldName && GUI.GetNameOfFocusedControl() == SpeechTextFieldName) 
                || (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                // it wasn't selected before, but now it is so highlight the text
                HighlightText();
            }
            m_FreeMouseLook.enabled = !IsSpeechTextBoxInFocus;
        }

        GUI.enabled = prevEnabled;
    }

    public void SendSpeechMessage(string message)
    {
        message = message.Replace("\n", "");
        StartCoroutine(SendServerMessage(m_SpeechUserID, message));

        ++m_SpeechUserID;

        if (m_SavedSpeech.Count == 0 || string.Compare(m_SavedSpeech[m_SavedSpeech.Count - 1], message) != 0)
        {
            m_SavedSpeech.Add(message);
        }

        m_nPreviousSpeechIndex = m_SavedSpeech.Count;
    }

    void HighlightText()
    {
        TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        textEditor.SelectAll();
    }

    IEnumerator SendServerMessage(int speechUserId, string message)
    {
        TypingEnabled = false;
        string url = string.Format("http://vhtoolkitweb/VHMsgAsp/VHMsgSite.aspx?SpeechUserId={0}&UserMessage={1}&ClientNeedsResponse=true", speechUserId, message.Replace(" ", "%20"));
        WWW www = new WWW(url);
        Debug.Log(url);
        yield return www;

        while (!www.isDone) { yield return new WaitForEndOfFrame(); }

        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            int index = www.text.IndexOf(Splitter);
            if (index != -1)
            {
                string vrSpeakMsg = www.text.Substring(0, index);
                Debug.Log(vrSpeakMsg);
                vhmsg.ReceiveVHMsg(vrSpeakMsg);
            }
            else
            {
                Debug.LogError(string.Format("Expected message vrSpeak was not received"));
                Debug.Log(www.text);
            }
        }
    }
    #endregion
}
