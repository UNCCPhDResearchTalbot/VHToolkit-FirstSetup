using UnityEngine;

using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Replace the unity default camera gameobject "Main Camera" with this object and you
/// can move your camera and look around the world with whatever keys you like
/// </summary>
[AddComponentMenu("Camera-Control/Smooth Mouse Look")]
public class FreeMouseLook : MonoBehaviour
{
    #region Variables
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float movementSpeed = 5F;
    public float secondaryMovementSpeed = 2.5f;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public bool m_CameraRotationOn = false;

    protected float rotationX = 0F;
    protected float rotationY = 0F;

    public KeyCode[] m_MoveForwardKeys = new KeyCode[]{KeyCode.W, KeyCode.UpArrow};
    public KeyCode[] m_MoveBackwardKeys = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
    public KeyCode[] m_MoveLeftKeys = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
    public KeyCode[] m_MoveRightKeys = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
    public KeyCode[] m_MoveUpKeys = new KeyCode[] { KeyCode.E };
    public KeyCode[] m_MoveDownKeys = new KeyCode[] { KeyCode.Q };
    public KeyCode[] m_ToggleMouseLookKeys = new KeyCode[] { KeyCode.J };

    public delegate void MovementCallback();
    #endregion

    #region Properties
    public bool CameraRotationOn
    {
        get { return m_CameraRotationOn; }
        set { m_CameraRotationOn = value; }
    }
    #endregion

    #region Functions
    public virtual void Start()
    {
        if (rigidbody)
            rigidbody.freezeRotation = true;

        rotationX = transform.localRotation.eulerAngles.y;
        rotationY = transform.localRotation.eulerAngles.x;
    }

    public virtual void Update()
    {
        if (CameraRotationOn)
        {
            if (axes == RotationAxes.MouseXAndY)
            {
                rotationY += Input.GetAxis("Mouse Y") * -sensitivityY;
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;

                rotationY = ClampAngle(rotationY, minimumY, maximumY);
                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);

                transform.localRotation = xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;

                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation = xQuaternion;
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * -sensitivityY;

                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
                transform.localRotation = yQuaternion;
            }
        }

        CheckKeyPress(m_MoveForwardKeys, MoveForward);
        CheckKeyPress(m_MoveBackwardKeys, MoveBackward);
        CheckKeyPress(m_MoveLeftKeys, MoveLeft);
        CheckKeyPress(m_MoveRightKeys, MoveRight);
        CheckKeyPress(m_MoveUpKeys, MoveUp);
        CheckKeyPress(m_MoveDownKeys, MoveDown);
        CheckKeyDown(m_ToggleMouseLookKeys, ToggleMouseLook);
    }

    protected float GetMovementSpeed()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? secondaryMovementSpeed : movementSpeed;
    }

    public virtual void MoveForward()
    {
        transform.localPosition += transform.forward * GetMovementSpeed() * Time.deltaTime;
    }

    public virtual void MoveBackward()
    {
        transform.localPosition += -transform.forward * GetMovementSpeed() * Time.deltaTime;
    }

    public virtual void MoveLeft()
    {
        transform.localPosition -= transform.right * GetMovementSpeed() * Time.deltaTime;
    }

    public virtual void MoveRight()
    {
        transform.localPosition += transform.right * GetMovementSpeed() * Time.deltaTime;
    }

    public virtual void MoveUp()
    {
        transform.localPosition += transform.up * GetMovementSpeed() * Time.deltaTime;
    }

    public virtual void MoveDown()
    {
        transform.localPosition -= transform.up * GetMovementSpeed() * Time.deltaTime;
    }

    public void ToggleMouseLook()
    {
        CameraRotationOn = !CameraRotationOn;
        if (CameraRotationOn)
        {
            rotationX = transform.localRotation.eulerAngles.y;
            rotationY = transform.localRotation.eulerAngles.x;
        }
    }

    public void CheckKeyPress(KeyCode[] movementKeys, MovementCallback cb)
    {
        if (movementKeys == null)
        {
            return;
        }

        for (int i = 0; i < movementKeys.Length; i++)
        {
            if (Input.GetKey(movementKeys[i]))
            {
                cb();
                break;
            }
        }
    }

    protected void CheckKeyDown(KeyCode[] movementKeys, MovementCallback cb)
    {
        if (movementKeys == null)
        {
            return;
        }

        for (int i = 0; i < movementKeys.Length; i++)
        {
            if (Input.GetKeyDown(movementKeys[i]))
            {
                cb();
                break;
            }
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    #endregion
}
