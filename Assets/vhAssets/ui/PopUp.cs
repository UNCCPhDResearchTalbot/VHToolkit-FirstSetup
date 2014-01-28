using UnityEngine;
using System.Collections;

public class PopUp
{
    #region Constants
    // -----> Constants
    public delegate void OnButtonPressedCallback(object sender);
    public struct ButtonData
    {
        public Rect m_Position;
        public string m_Text;
        public OnButtonPressedCallback m_PressedCB;
    }
    #endregion

    #region Variables
    // ----->  Variables
    protected ButtonData[] m_Buttons;
    protected Rect m_BackdropPosition;
    protected bool m_bIsVisible = true;
    #endregion

    #region Properties
    public bool IsVisible
    {
        get { return m_bIsVisible; }
        set { m_bIsVisible = value; }
    }
    #endregion

    #region Functions
    public PopUp(Rect normalizedScreenPosition, int numButtons)
    {
        m_BackdropPosition = normalizedScreenPosition;
        m_Buttons = new ButtonData[numButtons];
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i] = new ButtonData();
        }
    }

    virtual public void Draw()
    {
        VHGUI.Box(m_BackdropPosition, "");
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            if (VHGUI.Button(m_Buttons[i].m_Position, m_Buttons[i].m_Text))
            {
                m_Buttons[i].m_PressedCB(m_Buttons[i]);
            }
        }
    }

    public void AddButtonPressedCallback(int buttonIndex, OnButtonPressedCallback cb)
    {
        if (buttonIndex < 0 || buttonIndex >= m_Buttons.Length)
        {
            Debug.LogError("PopUpBox::SetButtonPressedCallback failed because of bad buttonIndex: " + buttonIndex);
            return;
        }

        m_Buttons[buttonIndex].m_PressedCB += cb;
    }

    protected void SetButtonData(int buttonIndex, Rect position, string text, OnButtonPressedCallback cb)
    {
        if (buttonIndex < 0 || buttonIndex >= m_Buttons.Length)
        {
            Debug.LogError("PopUpBox::SetButtonData failed because of bad buttonIndex: " + buttonIndex);
            return;
        }

        m_Buttons[buttonIndex].m_Position = position;
        m_Buttons[buttonIndex].m_Text = text;
        m_Buttons[buttonIndex].m_PressedCB += cb;

    }

    protected void SetButtonData(int buttonIndex, ButtonData buttonData)
    {
        SetButtonData(buttonIndex, buttonData.m_Position, buttonData.m_Text, buttonData.m_PressedCB);
    }

    #endregion
}
