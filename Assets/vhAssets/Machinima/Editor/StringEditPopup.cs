using UnityEngine;
using UnityEditor;
using System.Collections;

public class StringEditPopup : EditorWindow
{
    #region Constants
    public delegate void OnSaveString(string label, string modifiedString);
    #endregion

    #region Variables
    public string m_OriginalString = "";
    public string m_StringToEdit = "";
    public string m_Label = "";
    public OnSaveString m_OnSaved;
    bool m_ShouldBeClosed;
    #endregion

    #region Functions
    public static void Init(Rect windowPos, string label, string stringToEdit, OnSaveString onSaved)
    {
        StringEditPopup window = (StringEditPopup)EditorWindow.GetWindow(typeof(StringEditPopup));
        window.m_OriginalString = window.m_StringToEdit = stringToEdit;
        window.m_Label = label;
        window.m_OnSaved += onSaved;
        window.position = windowPos;
        //window.minSize = new Vector2(windowPos.width, windowPos.height);
        //window.maxSize = new Vector2(windowPos.width, windowPos.height);
        window.ShowPopup();
    }

    void OnLostFocus()
    {
        m_ShouldBeClosed = true;
    }

    void Update()
    {
        if (m_ShouldBeClosed)
        {
            Close();
        }
    }

    void OnGUI()
    {
        GUILayout.Label(m_Label);
        m_StringToEdit = GUILayout.TextArea(m_StringToEdit, GUILayout.Height(200));

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Save"))
            {
                Save();
            }
            if (GUILayout.Button("Close"))
            {
                if (m_OriginalString != m_StringToEdit)
                {
                    if (EditorUtility.DisplayDialog("String Modified", string.Format("The string {0} was modified from \n\"{1}\" \nto \n\"{2}\"\nDo you want to save it?", m_Label, m_OriginalString, m_StringToEdit), "Yes", "No"))
                    {
                        Save();
                    }
                }
                m_ShouldBeClosed = true;    
            }
        }
        GUILayout.EndHorizontal(); 
    }

    void Save()
    {
        m_OriginalString = m_StringToEdit;
        if (m_OnSaved != null)
        {
            m_OnSaved(m_Label, m_StringToEdit);
        }
    }
    #endregion
}
