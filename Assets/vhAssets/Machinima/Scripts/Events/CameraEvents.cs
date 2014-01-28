using UnityEngine;
using System.Collections;

public class CameraEvents : GenericEvents
{
    #region Functions
    public override string GetEventType() { return GenericEventNames.Camera; }
    #endregion

    #region Events
    public class CameraEvent_Base : ICutsceneEventInterface { }
    public class CameraEvent_SwitchCameras : CameraEvent_Base
    {
        #region Function
        public void SwitchCameras(Camera prevCamera, Camera newCamera)
        {
            prevCamera.enabled = false;
            newCamera.enabled = true;

#if UNITY_3_5 || UNITY_3_4
            prevCamera.gameObject.SetActiveRecursively(false);
            newCamera.gameObject.SetActiveRecursively(true);
#else
            prevCamera.gameObject.SetActive(false);
            newCamera.gameObject.SetActive(true);
#endif

            AudioListener prevListener = prevCamera.GetComponent<AudioListener>();
            AudioListener newListener = newCamera.GetComponent<AudioListener>();
            if (prevListener != null && newListener != null)
            {
                // only turn on the audio listener if they both have one, otherwise
                // we'll get the no audio listener errors
                prevListener.enabled = false;
                newListener.enabled = true;
            }
        }

        /*public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Camera>(ce, 0).fov;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            Cast<Camera>(ce, 0).fov = (float)rData;
        }*/
        #endregion
    }

    public class CameraEvent_SetCameraDepth : CameraEvent_Base
    {
        #region Function
        public void SetCameraDepth(Camera camera, float depth)
        {
            camera.depth = depth;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Camera>(ce, 0).depth;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetCameraDepth(Cast<Camera>(ce, 0), (float)rData);
        }
        #endregion
    }

    public class CameraEvent_SetFieldOfView : CameraEvent_Base
    {
        #region Function
        public void SetFieldOfView(Camera camera, float fov)
        {
            camera.fieldOfView = fov;
        }

        public override object SaveRewindData(CutsceneEvent ce)
        {
            return Cast<Camera>(ce, 0).fieldOfView;
        }

        public override void LoadRewindData(CutsceneEvent ce, object rData)
        {
            SetFieldOfView(Cast<Camera>(ce, 0), (float)rData);
        }
        #endregion
    }
    #endregion
}
