using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CutsceneOrderPopup : EditorWindow
{
    #region Constants
    const float DropLocationHeight = 10;
    const float CutsceneListItemHeight = 20;
    public class CutsceneListItem
    {
        public Rect m_Position;
        public Cutscene m_Cutscene;

        public CutsceneListItem(Rect position, Cutscene cutscene)
        {
            m_Position = position;
            m_Cutscene = cutscene;
        }
    }
    #endregion

    #region Variables
    bool m_ShouldBeClosed;
    bool m_NeedsSorting;
    List<CutsceneListItem> m_CutsceneListItems = new List<CutsceneListItem>();
    Rect m_ScrollPosition = new Rect();
    Rect m_ScrollView = new Rect(0, 0, 1000, 1000);
    Rect m_DropLocation = new Rect();
    int m_DraggingIndex = -1;
    Vector2 m_Scoller;
    bool m_IsDragging;
    #endregion

    #region Functions
    public static void Init(Rect windowPos, EditorCutsceneManager cutsceneManager)
    {
        CutsceneOrderPopup window = (CutsceneOrderPopup)EditorWindow.GetWindow(typeof(CutsceneOrderPopup));
        window.position = windowPos;
        window.Setup(cutsceneManager, windowPos);
        window.ShowPopup();
    }

    void OnLostFocus()
    {
        m_ShouldBeClosed = true;
    }

    public void Setup(EditorCutsceneManager cutsceneManager, Rect windowPos)
    {
        Rect position = new Rect(0, 0, windowPos.width, CutsceneListItemHeight);
        for (int i = 0; i < cutsceneManager.NumCutscenes; i++)
        {
            m_CutsceneListItems.Add(new CutsceneListItem(position, cutsceneManager.GetTimelineObjectByIndex(i) as Cutscene));
            position.y += CutsceneListItemHeight;
        }

        m_CutsceneListItems.Sort((a, b) => a.m_Cutscene.Order - b.m_Cutscene.Order);

        SortList();
    }

    void Update()
    {
        if (m_NeedsSorting)
        {
            SortList();
        }

        if (m_ShouldBeClosed)
        {
            Close();
        }
    }

    int CalculateIndex(float yPos)
    {
        return (int)(yPos / CutsceneListItemHeight);
    }

    bool IndexInRange(int index)
    {
        return index >= 0 && index < m_CutsceneListItems.Count;
    }

    void OnGUI()
    {
        Event e = Event.current;
        Vector2 scrollRelativeMousePos = e.mousePosition + m_Scoller;
        if (e.isMouse)
        {
            if (e.type == EventType.MouseDrag)
            {
                HandleMouseDrag(e, scrollRelativeMousePos);
            }
            else if (e.type == EventType.MouseDown)
            {
                HandleMouseDown(e, scrollRelativeMousePos);
            }
            else if (e.type == EventType.MouseUp)
            {
                HandleMouseUp(e, scrollRelativeMousePos);
            }
        }

        // draw the draggables
        m_ScrollPosition.Set(0, 0, position.width, position.height);
        m_ScrollView.width = position.width - 15;
        m_Scoller = GUI.BeginScrollView(m_ScrollPosition, m_Scoller, m_ScrollView);
        {
            
            for (int i = 0; i < m_CutsceneListItems.Count; i++)
            {
                Cutscene cutscene = m_CutsceneListItems[i].m_Cutscene;
                m_CutsceneListItems[i].m_Position.width = position.width;

                Color prev = GUI.color;
                GUI.color = i == m_DraggingIndex ? Color.yellow : Color.white;
                GUI.Button(m_CutsceneListItems[i].m_Position, string.Format("{0}", cutscene.CutsceneName));
                GUI.color = prev;
            } 
        }

        if (m_IsDragging)
        {
            // draw drop locations
            Color prev = GUI.color;
            GUI.color = Color.green;//CutsceneEditor.SelectionBoxColor;

            GUI.Box(m_DropLocation, "");
            GUI.color = prev;
        }

        GUI.EndScrollView();
    }

    void OnDrop(int originalIndex, int newIndex)
    {
        if (!IndexInRange(originalIndex) || !IndexInRange(newIndex))
        {
            return;
        }

        // insert it into the new locations and move everything down
        CutsceneListItem temp = m_CutsceneListItems[originalIndex];
        m_CutsceneListItems.RemoveAt(originalIndex);
        m_CutsceneListItems.Insert(newIndex, temp);

        m_NeedsSorting = true;
    }

    void SortList()
    {
        for (int i = 0; i < m_CutsceneListItems.Count; i++)
        {
            m_CutsceneListItems[i].m_Cutscene.Order = i;
        }

        m_CutsceneListItems.Sort((a, b) => a.m_Cutscene.Order - b.m_Cutscene.Order);

        for (int i = 0; i < m_CutsceneListItems.Count; i++)
        {
            m_CutsceneListItems[i].m_Position.y = i * CutsceneListItemHeight;
        }

        Repaint();
        m_NeedsSorting = false;
    }

    void OnUpButtonPressed()
    {
        m_NeedsSorting = true;
        Repaint();
    }

    void HandleMouseDrag(Event e, Vector2 relativeMousePos)
    {
        if (!IndexInRange(m_DraggingIndex))
        {
            return;
        }

        int index = CalculateIndex(relativeMousePos.y);
        m_IsDragging = index >= 0 && index < m_CutsceneListItems.Count;
        m_DropLocation.Set(0, index * CutsceneListItemHeight, position.width, DropLocationHeight);
        Repaint();
    }

    void HandleMouseDown(Event e, Vector2 relativeMousePos)
    {
        m_DraggingIndex = CalculateIndex(relativeMousePos.y);
        m_IsDragging = IndexInRange(m_DraggingIndex);   
    }

    void HandleMouseUp(Event e, Vector2 relativeMousePos)
    {
        m_IsDragging = false;
        OnDrop(m_DraggingIndex, CalculateIndex(relativeMousePos.y));
        m_DraggingIndex = -1;
    }
    #endregion
}
