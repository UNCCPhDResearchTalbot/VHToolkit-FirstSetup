using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TipPopUp : PopUp
{
    #region Constants
    public const int BackButtonIndex = 0;
    public const int NextButtonIndex = 1;
    public const int CloseButtonIndex = 2;
    static Vector2 ButtonSize = new Vector2(0.08f, 0.06f);
    #endregion

    #region Variables
    List<string> m_TipText = new List<string>();
    int m_nCurrentTipIndex = 0;
    Rect m_TipTextPosition;

    // toggle
    bool m_bShowAtStart = true;
    Rect m_ShowAtStartTogglePos = new Rect();
    string m_ShowTipsAtStartKeyName;

    // for the scroll box
    Rect m_TipRectPosition;
    Vector2 m_ScrollPosition = new Vector2();
    Rect m_TipViewRect;
    #endregion

    #region Functions

    public TipPopUp(Rect normalizedScreenPosition, List<string> tipText, string showAtStartKeyName)
        : base(normalizedScreenPosition, 3)
    {
        m_TipText.AddRange(tipText);

        // setup buttons
        SetButtonData(BackButtonIndex, new Rect(normalizedScreenPosition.x + 0.01f, normalizedScreenPosition.yMax - 0.15f, ButtonSize.x + 0.025f, ButtonSize.y),
            "Previous", OnBackButtonPressed);
        SetButtonData(NextButtonIndex, new Rect(normalizedScreenPosition.x + 0.01f, normalizedScreenPosition.yMax - 0.075f, ButtonSize.x, ButtonSize.y),
            "Next", OnNextButtonPressed);
        SetButtonData(CloseButtonIndex, new Rect(normalizedScreenPosition.xMax - ButtonSize.x - 0.01f, normalizedScreenPosition.yMax - 0.075f, ButtonSize.x, ButtonSize.y),
            "Close", OnCloseButtonPressed);

        m_ShowAtStartTogglePos = new Rect(normalizedScreenPosition.xMax / 2, normalizedScreenPosition.yMax - 0.15f, 0.25f, 0.1f);
        m_TipViewRect = new Rect(0,0, normalizedScreenPosition.width - 0.03f, 0.33f);
        m_TipRectPosition = new Rect(normalizedScreenPosition.x, normalizedScreenPosition.y, normalizedScreenPosition.width, 0.33f);
        m_TipTextPosition = new Rect(0.015f, 0.015f, m_TipViewRect.width - 0.01f, m_TipRectPosition.height);

        if (!string.IsNullOrEmpty(showAtStartKeyName))
        {
            m_ShowTipsAtStartKeyName = showAtStartKeyName;
            m_bShowAtStart = ShowTips(m_ShowTipsAtStartKeyName);
        }
        else
        {
            Debug.LogWarning("Showing tips at startup will not be saved for next time because you didn't" +
                " enter a valid KeyName. Example: \"vhToolkitShowTips\"");
        }
    }

    public override void Draw()
    {
        base.Draw();

        // draw the tip text
        m_ScrollPosition = VHGUI.BeginScrollView(m_TipRectPosition, m_ScrollPosition, m_TipViewRect);
            VHGUI.Label(m_TipTextPosition, m_TipText[m_nCurrentTipIndex]);
        VHGUI.EndScrollView();

        m_bShowAtStart = VHGUI.Toggle(m_ShowAtStartTogglePos, m_bShowAtStart, "Show Tips at Start");
    }

    public void AddTip(string tipText)
    {
        m_TipText.Add(tipText);
    }

    public void SetTipIndex(int index)
    {
        m_nCurrentTipIndex = Mathf.Clamp(index, 0, m_TipText.Count - 1);
    }

    public void SetRandomTipIndex()
    {
        // Returns a random integer number between min [inclusive] and max [exclusive]
        m_nCurrentTipIndex = Random.Range(0, m_TipText.Count);
    }

    void OnBackButtonPressed(object sender)
    {
        if (--m_nCurrentTipIndex < 0)
        {
            m_nCurrentTipIndex = m_TipText.Count - 1;
        }
    }

    void OnNextButtonPressed(object sender)
    {
        if (++m_nCurrentTipIndex >= m_TipText.Count)
        {
            m_nCurrentTipIndex = 0;
        }
    }

    void OnCloseButtonPressed(object sender)
    {
        // save their setting
        SetShowTips(m_ShowTipsAtStartKeyName, m_bShowAtStart);
    }

    public static bool ShowTips(string showAtStartKeyName)
    {
        return PlayerPrefs.GetInt(showAtStartKeyName, 1) == 1 ? true : false;
    }

    public static void SetShowTips(string showAtStartKeyName, bool tf)
    {
        if (!string.IsNullOrEmpty(showAtStartKeyName))
        {
            PlayerPrefs.SetInt(showAtStartKeyName, tf ? 1 : 0);
        }
    }

    #endregion
}
