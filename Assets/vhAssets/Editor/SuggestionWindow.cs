using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class SuggestionWindow : EditorWindow
{
    #region Constants
    const string SavedWindowPosXKey = "SuggestionWindowX";
    const string SavedWindowPosYKey = "SuggestionWindowY";
    const string SavedWindowWKey = "SuggestionWindowW";
    const string SavedWindowHKey = "SuggestionWindowH";
    const string DefaultEmail = "nospam@ict.usc.edu";
    const string ReceiverEmail = "vh-support@ict.usc.edu";
    const string Smtp = "smtp.ict.usc.edu";
    const string SubjectText = "VH Suggestion";
    const string PhpUrl = "https://confluence.ict.usc.edu/contact/contact.php";
    #endregion

    #region Variables
    string m_SuggestionText = "Please enter whatever is on your mind";
    string m_Sender = DefaultEmail;
    string m_SenderName = "Anonymous";
    bool m_InvalidEmail = false;
    #endregion

    #region Functions
    [MenuItem("VH/Suggestions", false, 0)]
    static void Init()
    {
        SuggestionWindow window = (SuggestionWindow)EditorWindow.GetWindow(typeof(SuggestionWindow));
        window.autoRepaintOnSceneChange = true;
        window.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), PlayerPrefs.GetFloat(SavedWindowWKey, 435),
            PlayerPrefs.GetFloat(SavedWindowHKey, 309));

        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        m_SuggestionText = EditorGUILayout.TextArea(m_SuggestionText, GUILayout.Height(200));

        EditorGUILayout.LabelField("Please enter your name");
        m_SenderName = EditorGUILayout.TextField(m_SenderName);

        EditorGUILayout.LabelField("Please enter your email address");
        m_Sender = EditorGUILayout.TextField(m_Sender);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Send", GUILayout.Width(100)))
        {
            SendEmail();
        }

        if (GUILayout.Button("Cancel", GUILayout.Width(100)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void SendEmail()
    {
        if (string.IsNullOrEmpty(m_Sender))
        {
            m_Sender = DefaultEmail;
        }

        if (!IsValidEmail(m_Sender))
        {
            EditorUtility.DisplayDialog("Invalid Email Address", "Please enter a valid email address", "Ok");
            return;
        }

        /*
        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
        message.To.Add(ReceiverEmail);
        message.Subject = SubjectText;
        message.From = new System.Net.Mail.MailAddress(m_Sender);
        message.Body = m_SuggestionText;
        System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(Smtp);
        smtp.Send(message);
        */
        
        WWWForm form = new WWWForm();
        form.AddField("email", m_Sender);
        form.AddField("name", m_SenderName);
        form.AddField("message", m_SuggestionText);
        form.AddField("submitted", "");

        new WWW(PhpUrl, form);
    }

    bool IsValidEmail(string strIn)
    {
       m_InvalidEmail = false;
       if (String.IsNullOrEmpty(strIn))
          return false;

       // Use IdnMapping class to convert Unicode domain names.
       strIn = Regex.Replace(strIn, @"(@)(.+)$", this.DomainMapper);
       if (m_InvalidEmail)
          return false;

       // Return true if strIn is in valid e-mail format.
       return Regex.IsMatch(strIn,
              @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
              @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
              RegexOptions.IgnoreCase);
    }

     private string DomainMapper(Match match)
    {
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            m_InvalidEmail = true;
        }

        return match.Groups[1].Value + domainName;
    }

    void OnDestroy()
    {
        SaveLocation();
    }

    void SaveLocation()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, position.y);
        PlayerPrefs.SetFloat(SavedWindowWKey, position.width);
        PlayerPrefs.SetFloat(SavedWindowHKey, position.height);
    }
    #endregion
}
