using UnityEngine;
using System.Collections.Generic;

public class PopUpManager : MonoBehaviour
{
    #region Variables
    List<PopUp> m_PopUps = new List<PopUp>();
    #endregion

    #region Functions
    public void Start()
    {
    }

    public void Update()
    {
    }

    public void OnGUI()
    {
        for (int i = 0; i < m_PopUps.Count; i++)
        {
            if (m_PopUps[i].IsVisible)
            {
                m_PopUps[i].Draw();
            }
        }
    }

    public void AddPopUp(PopUp popup)
    {
        if (popup != null && !m_PopUps.Contains(popup))
        {
            m_PopUps.Add(popup);
        }
    }

    public void RemovePopUp(PopUp popup)
    {
        m_PopUps.Remove(popup);
    }
    #endregion
}
