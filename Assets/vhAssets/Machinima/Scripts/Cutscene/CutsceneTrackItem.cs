using UnityEngine;
using System.Collections;

[System.Serializable]
public class CutsceneTrackItem
{
    #region Constants
    public enum DragLocation
    {
        None,
        Left_Handle,
        Center,
        Right_Handle,
    }

    const float NonVerboseLength = 10;
    public static Color SelectedColor = Color.yellow;//new Color(236f / 255f, 219f / 255f, 50f / 255f);
    public static Color NotSelectedColor = Color.red;//new Color(215f / 255f, 47f / 255f, 100f / 255f);
    public static Color LockedColor = new Color(169f / 255f, 187f / 255f, 196f / 255f);
    #endregion

    #region Variables
    public string Name = "";
    public string EventType;
    public float StartTime;
    public float Length = 1;

    [HideInInspector]
    public bool FireAndForget = true;

    [HideInInspector]
    public bool LengthRepresented = false;

    [HideInInspector]
    public Rect GuiPosition = new Rect();

    [HideInInspector]
    public Rect LeftHandle = new Rect();

    [HideInInspector]
    public Rect RightHandle = new Rect();

    [HideInInspector]
    public Color GuiColor = Color.white;

    [HideInInspector]
    public bool Hidden;

    [HideInInspector]
    public bool Enabled = true;

    [HideInInspector]
    public string m_UniqueId = "";

    [HideInInspector]
    public bool Locked;

    [HideInInspector]
    public bool Selected;

    [HideInInspector]
    public bool ReadOnly;

    // allows developers to comment and document events. This text is displayed in the MM window under the notes section
    [HideInInspector]
    public string Notes = ""; 
    #endregion

    #region Properties
    public string UniqueId
    {
        get { return m_UniqueId; }
    }

    public float EndTime
    {
        get { return StartTime + Length; }
    }
    #endregion

    #region Functions
    public CutsceneTrackItem() { }
    public CutsceneTrackItem(string uniqueId)
    {
        m_UniqueId = uniqueId;
    }

    public CutsceneTrackItem(Rect guiPosition, string uniqueId) 
    {
        m_UniqueId = uniqueId;
        GuiPosition = guiPosition;
    }

    public CutsceneTrackItem(string uniqueId, string name, float startTime,
        float length, float yPos, float height, bool lengthRepresented, Color color)
    {
        m_UniqueId = uniqueId;
        Name = name;
        StartTime = startTime;
        Length = length;
        GuiPosition.y = yPos;
        GuiPosition.height = height;
        LengthRepresented = lengthRepresented;
        GuiColor = color;
    }

    /// <summary>
    /// Maintains the same relative handle positions when TrackItems are moved around
    /// </summary>
    /// <param name="distanceFromBorder"></param>
    public void UpdateHandlePositions(float distanceFromBorder)
    {
        LeftHandle.Set(GuiPosition.x + distanceFromBorder, GuiPosition.y, 10, GuiPosition.height);
        RightHandle.Set(GuiPosition.x + GuiPosition.width - distanceFromBorder - 10, GuiPosition.y, 10, GuiPosition.height);
    }

    /// <summary>
    /// Locks or unlocks the TrackItem. Locked items can't be moved
    /// </summary>
    /// <param name="locked"></param>
    public void SetLocked(bool locked)
    {
        Locked = locked;
        if (!Selected)
        {
            SetColor(Locked ? LockedColor : NotSelectedColor);
        }
    }

    /// <summary>
    /// Enables or disables the TrackItem.  Disables items aren't invoked when asked to be fired
    /// </summary>
    /// <param name="enabled"></param>
    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        GuiColor.a = Enabled ? 1 : 0.5f;
    }

    public void SetReadOnly(bool readOnly)
    {
        ReadOnly = readOnly;
        SetLocked(true);
    }

    public void SetSelected(bool selected)
    {
        Selected = selected;
        if (selected)
        {
            SetColor(SelectedColor);
        }
        else
        {
            SetColor(Locked ? LockedColor : NotSelectedColor);
        }
    }

    protected void SetColor(Color col)
    {
        GuiColor = col;
        GuiColor.a = Enabled ? 1 : 0.5f;
    }

    /// <summary>
    /// Returns the DragLocation type based on what area of the TrackItem rect
    /// contains the provided position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public DragLocation GetDragStateFromPosition(Vector2 position)
    {
        DragLocation retVal = DragLocation.None;

        if (LeftHandle.Contains(position))
        {
            retVal = DragLocation.Left_Handle;
        }
        else if (RightHandle.Contains(position))
        {
            retVal = DragLocation.Right_Handle;
        }
        else if (retVal == DragLocation.None && GuiPosition.Contains(position))
        {
            retVal = DragLocation.Center;
        }

        return retVal;
    }

    /// <summary>
    /// Returns the length in pixels of the given text. This function can only be called
    /// in onGUI
    /// </summary>
    /// <param name="content"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static float CalculateTextLength(GUIContent content, string text)
    {
        content.text = text;
        return GUI.skin.label.CalcSize(content).x + 8;
    }

    public bool Draw(bool verboseText, GUIContent content) 
    {
        if (Hidden)
        {
            return false;
        }

        bool buttonPressed = false;
        GUI.color = GuiColor;

        if (FireAndForget)
        {
            content.tooltip = string.Format("{0} {1}", Name, StartTime.ToString("f2"));
            if (!LengthRepresented)
            {
                content.text = verboseText ? Name : "";
                GuiPosition.width = verboseText ? CalculateTextLength(content, Name) : NonVerboseLength; 
            }
            else
            {
                content.text = Name;
            }
            
            buttonPressed = GUI.Button(GuiPosition, content);
        }
        else
        {
            // timed event
            content.text = Name;
            content.tooltip = string.Format("{0} {1}", Name, StartTime.ToString("f2"));
            UpdateHandlePositions(0);
            buttonPressed = GUI.Button(GuiPosition, content);

            GUI.color = Color.cyan;
            GUI.Button(LeftHandle, "");
            GUI.Button(RightHandle, "");
        }
        return buttonPressed;
    }
    #endregion
}
