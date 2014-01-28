using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorCameraWindow : EditorWindow
{
    // ----> Constants
    const float TextSpacing = 10;
    const int MaxCharacters = 10;
    const string SavedWindowPosXKey = "EditorCameraWindowX";
    const string SavedWindowPosYKey = "EditorCameraWindowY";

    Vector3 m_CameraPosition;
    Vector3 m_CameraRotation;
    static EditorCameraWindow ThisWindow;

    Rect m_PosRect = new Rect(10, 10, 250, 25);
    Rect m_RotRect = new Rect(10, 45, 250, 25);

    [MenuItem("VH/Editor Camera Window")]
    static void Init()
    {
        ThisWindow = (EditorCameraWindow)EditorWindow.GetWindow(typeof(EditorCameraWindow));
        ThisWindow.autoRepaintOnSceneChange = true;
        ThisWindow.position = new Rect(PlayerPrefs.GetFloat(SavedWindowPosXKey, 0),
            PlayerPrefs.GetFloat(SavedWindowPosYKey, 0), 300, 100);
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (SceneView.lastActiveSceneView == null)
        {
            return;
        }

        m_CameraPosition = SceneView.lastActiveSceneView.pivot;
        m_CameraRotation = SceneView.lastActiveSceneView.rotation.eulerAngles;

        m_CameraPosition = EditorGUI.Vector3Field(m_PosRect, "Position", m_CameraPosition);
        m_CameraRotation = EditorGUI.Vector3Field(m_RotRect, "Rotation", m_CameraRotation);

        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = m_CameraRotation;

        if (m_CameraRotation != SceneView.lastActiveSceneView.rotation.eulerAngles)
        {
            SceneView.lastActiveSceneView.rotation = rot;
        }

        if (m_CameraPosition != SceneView.lastActiveSceneView.pivot)
        {
            SceneView.lastActiveSceneView.pivot = m_CameraPosition;
        }

        SceneView.lastActiveSceneView.Repaint();
    }

    void OnFocus()
    {
        PlayerPrefs.SetFloat(SavedWindowPosXKey, ThisWindow.position.x);
        PlayerPrefs.SetFloat(SavedWindowPosYKey, ThisWindow.position.y);
    }
}
