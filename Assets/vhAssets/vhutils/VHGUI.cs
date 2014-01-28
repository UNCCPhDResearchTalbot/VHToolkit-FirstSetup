using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used to wrap around the Unity GUI class and make gui objects scale to the resolution
/// For the rectangle position dimensions, values of 0 - 1.0f should be passed in order for the gui object
/// to be visible on screen.
///
/// Rect.x: 0 = screen pixel coordiante (0, y) and 1.0f = screen pixel coordiante (Screen.Width, y)
/// Rect.y: 0 = screen pixel coordiante (x, 0) and 1.0f = screen pixel coordiante (x, Screen.Height)
/// Rect.width: 0 = 0, 1.0f = Screen.Width
/// Rect.height: 0 = 0, 1.0f = Screen.Height
/// </summary>
public class VHGUI
{
    public static void Box(Rect position, string text)
    {
        GUI.Box(ScaleToRes(ref position), text);
    }

    public static void Box(Rect position, string text, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        GUI.Box(ScaleToRes(ref position), text);
        GUI.color = temp;
    }

    public static void Box(Rect position, Texture image)
    {
        GUI.Box(ScaleToRes(ref position), image);
    }

    public static void DrawTexture(Rect position, Texture image)
    {
        GUI.DrawTexture(ScaleToRes(ref position), image);
    }

    public static bool Button(Rect position, string text)
    {
        return GUI.Button(ScaleToRes(ref position), text);
    }

    public static bool Button(Rect position, string text, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        bool retVal = GUI.Button(ScaleToRes(ref position), text);
        GUI.color = temp;
        return retVal;
    }

    public static bool Button(Rect position, string text, GUIStyle style)
    {
        return GUI.Button(ScaleToRes(ref position), text, style);
    }

    public static void Label(Rect position, string text)
    {
        GUI.Label(ScaleToRes(ref position), text);
    }

    public static void Label(Rect position, string text, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        GUI.Label(ScaleToRes(ref position), text);
        GUI.color = temp;
    }

    public static void Label(Rect position, string text, GUIStyle style)
    {
        GUI.Label(ScaleToRes(ref position), text, style);
    }

    public static void Label(Rect position, string text, GUIStyle style, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        GUI.Label(ScaleToRes(ref position), text, style);
        GUI.color = temp;
    }

    public static string TextField(Rect position, string text)
    {
        return GUI.TextField(ScaleToRes(ref position), text);
    }

    public static string TextArea(Rect position, string text, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        string retVal = GUI.TextArea(ScaleToRes(ref position), text);
        GUI.color = temp;
        return retVal;
    }

    public static bool Toggle(Rect position, bool value, string text)
    {
        return GUI.Toggle(ScaleToRes(ref position), value, text);
    }

    public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect)
    {
        Vector2 retVal = GUI.BeginScrollView(ScaleToRes(ref position), ScaleToRes(ref scrollPosition), ScaleToRes(ref viewRect));
        return NormalizeCoordinates(ref retVal);
    }

    public static void EndScrollView()
    {
        GUI.EndScrollView();
    }

    public static Rect ScaleToRes(ref Rect r)
    {
        r.x *= Screen.width;
        r.y *= Screen.height;
        r.width *= Screen.width;
        r.height *= Screen.height;
        return r;
    }

    public static Vector2 ScaleToRes(ref Vector2 p)
    {
        p.x *= Screen.width;
        p.y *= Screen.height;
        return p;
    }

    public static Vector2 NormalizeCoordinates(ref Vector2 p)
    {
        p.x /= (float)Screen.width;
        p.y /= (float)Screen.height;
        return p;
    }
}


public class VHGUILayout
{
    public static void Label(string text, Color c)
    {
        Color temp = GUI.color;
        GUI.color = c;
        GUILayout.Label(text);
        GUI.color = temp;
    }
}
