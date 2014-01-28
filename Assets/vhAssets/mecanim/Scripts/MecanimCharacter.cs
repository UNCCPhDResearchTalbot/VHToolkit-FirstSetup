 /// <summary>
/// 
/// </summary>

using UnityEngine;
using System;
using System.Collections;
  
[RequireComponent(typeof(Animator))]

//Name of class must be name of file as well

public class MecanimCharacter : Character
{
    #region Constants
    const float DefaultNodShakeSpeed = 0.5f;
    #endregion

    #region Variables
    protected Animator animator;
    public AudioClip m_FaceFxTestAudio;
    public GameObject m_GazeTarget;
    public Transform m_Face;
    public Transform m_GrabNode;
    public Transform m_Neck;
    public float m_HeadGazeWeight = 0;
    public float m_BodyGazeWeight = 0;
    public float m_EyeGazeWeight = 0;
    public string m_RealName;
    public string m_AnimRemoveName = "._";
    AnimatorStateInfo baseCurrentState;
    AnimatorStateInfo baseNextState;
    AnimatorStateInfo otherMovesCurrentState;
    //string m_LastPlayedAnim = "";
    
    public float shakeAmplitude = 1;
    public float shakeAngularFrequency = 3.9f;
    public float nodAmplitude = 0.34f;
    public float nodAngularFrequency = 5.14f;
    public float m_NumShakes = 3;

    // noding/shaking variables
    float m_CurrentNeckRot;
    float time = 0;
    float maxTime = 0;
    bool m_RotateHead;
    bool nodMovement;
    
    public Transform grabbableObj;
    public bool gotWrench;
	
	public float DirectionDampTime = .25f;
    #endregion

    #region Properties
    public override string CharacterName
    {
        get { return m_RealName; }
    }
    #endregion

    #region Functions
    // Use this for initialization
	void Awake () 
	{
		animator = GetComponent<Animator>();
        
        //avatar.SetTarget(AvatarTarget.Body, 1.0f);
        //avatar.MatchTarget(m_GazeTarget.transform.position, m_GazeTarget.transform.rotation, AvatarTarget.Body, 0);

        if (animator.layerCount > 1)
        {
            animator.SetLayerWeight(1, 1.0f);
        }
	}

    public void MoveToPoint(Vector3 point, Quaternion rot)
    {
        animator.SetFloat("Speed", 1);
        //animator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);
        //animator.MatchTarget(point, rot, AvatarTarget.Root, 1.0f);
    }

    IEnumerator DoMoveToPoint()
    {
        yield break;
    }

    public void PlayAnim(string animName)
    {
        animator.SetBool(animName, true);
        StartCoroutine(SetAnimatorBoolDelayed(0.2f, animName, false));
        Debug.Log("play anim: " + animName);
        //m_LastPlayedAnim = animName;
    }

    public void PlayAnim(string animName, float startDelay)
    {
        StartCoroutine(SetAnimatorBoolDelayed(startDelay, animName, true));
        StartCoroutine(SetAnimatorBoolDelayed(startDelay + 0.2f, animName, false));
        //m_LastPlayedAnim = animName;
    }

    public void PlayAU(int au, string side, float weight, float time)
    {

    }

    public void Speak(string audio)
    {

    }

    public void PlayAudioAsync(MonoBehaviour behaviour, string animName, WWW www)
    {
        behaviour.StartCoroutine(PlayAudioAsync(animName, www));
    }

    IEnumerator PlayAudioAsync(string animName, WWW www)
    {
        yield return www;
        PlayAudio(animName, www.audioClip);
    }

    public void PlayAudio(string audioName)
    {
        Debug.Log(string.Format("playing {0}", audioName));
        AudioClip clip = m_FaceFxTestAudio;
        AudioBank bank = (AudioBank)FindObjectOfType(typeof(AudioBank));
        if (bank != null)
        {
            clip = bank.FindClip(audioName);
        }
        PlayAudio(audioName, clip/*(AudioClip)Resources.Load(clipName, typeof(AudioClip))*/);
    }

    public void PlayAudio(AudioClip clip)
    {
        PlayAudio(clip.name, clip);
        Debug.Log(clip.name);
    }

    public void PlayAudio(string animName, AudioClip audio)
    {
        if (string.IsNullOrEmpty(animName))
        {
            return;
        }

        animName = m_AnimRemoveName + animName;

        AnimationState animState = animation[animName];
        if (animState != null)
        {
            animState.layer = 1;
            // Make sure we have a non-addative animation playing all the time, 
            // otherwise addative poses will be added to infinity.  If we don't use ClapForever, 
            // be sure another animation is playing.
            animState.wrapMode = WrapMode.ClampForever;
            animState.blendMode = AnimationBlendMode.Blend;
        }
        else
        {
            Debug.Log("animState is NULL!");
        }

        FaceFXControllerScript ffxController = GetComponent<FaceFXControllerScript>();
        if (ffxController != null)
        {
            ffxController.PlayAnim(animName, audio);
        }
        else
        {
            Debug.Log("ffxController is NULL!");
        }
    }
    
	void Update () 
	{
#if false
        //if (!m_MovingToPoint)
		{
			baseCurrentState= animator.GetCurrentAnimatorStateInfo(0);
            baseNextState = animator.GetNextAnimatorStateInfo(0);

            if(animator.layerCount>1)
            {
				otherMovesCurrentState = animator.GetCurrentAnimatorStateInfo(1);
			}

            if (Input.GetButtonDown("Fire2"))
            {
                animator.SetBool("Scratch", true);
            }

        	if(baseCurrentState.IsName("Base Layer.RunBT"))
			{
                if (Input.GetButton("Jump"))
                    animator.SetBool("Jump", true );
			}
			else
			{
            	animator.SetBool("Jump", false);				
			}
            
      		float h = Input.GetAxis("Horizontal");
        	float v = Input.GetAxis("Vertical");

#if false
			animator.SetFloat("Speed", h*h+v*v);
            animator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);
#endif
            /*if (m_BodyGazeWeight != 0 || m_HeadGazeWeight != 0 || m_EyeGazeWeight != 0)
            {
                animator.SetLookAtPosition(m_GazeTarget.transform.position);
                animator.SetLookAtWeight(1.0f, m_BodyGazeWeight, m_HeadGazeWeight, m_EyeGazeWeight);
            }
            else
            {
                animator.SetLookAtWeight(0, 0, 0, 0);
            }*/

            if (m_GazeTarget != null)
            {
                animator.SetLookAtPosition(m_GazeTarget.transform.position);
                animator.SetLookAtWeight(1.0f, 1, 1, 0);
            }
            
		}

        // Check other moves layer for status
        /*if (otherMovesCurrentState.IsName("OtherMoves.ScratchHead"))
        {
            animator.SetBool("Scratch", false);
        }
        else if (otherMovesCurrentState.IsName("OtherMoves.Indicate"))
        {
            animator.SetBool("Point", false);
        }*/
#endif
	}

    IEnumerator SetAnimatorBoolDelayed(float delay, string boolName, bool value)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool(boolName, value);

    }

    public void SetGazeTarget(GameObject gazeTarget)
    {
        m_GazeTarget = gazeTarget;
    }

    void LateUpdate()
    {
        if (m_RotateHead)
        {
            if (nodMovement)
            {
                DoHeadMovement(nodAmplitude, nodAngularFrequency, m_NumShakes, m_Neck.transform.forward);
            }
            else
            {
                DoHeadMovement(shakeAmplitude, shakeAngularFrequency, m_NumShakes, m_Neck.transform.right);
            }
        }
    }

    public void NodHead(float amount, float nodNumber, float nodTime)
    {
        //Debug.Log(string.Format("NodHead {0} {1} {2}", amount, nodNumber, nodTime));
        amount = Mathf.Clamp(amount, -1, 1);
        if (amount == 0)
            amount = DefaultNodShakeSpeed;
        
        nodAmplitude = amount * 30.0f;
        nodMovement = true;
        m_RotateHead = true;
        nodAngularFrequency = (Mathf.PI * 2.0f) / (nodTime);
        //nodAngularFrequency = (nodTime / nodNumber);
        m_NumShakes = nodNumber;
        maxTime = nodTime;
    }

    public void ShakeHead(float amount, float nodNumber, float nodTime)
    {
        //Debug.Log(string.Format("ShakeHead {0} {1} {2}", amount, nodNumber, nodTime));
        amount = Mathf.Clamp(amount, -1, 1);
        if (amount == 0)
            amount = DefaultNodShakeSpeed;

        shakeAmplitude = (amount * 45.0f) * -1;
        nodMovement = false;
        m_RotateHead = true;
        shakeAngularFrequency = (Mathf.PI * 2.0f) / nodTime;
        m_NumShakes = nodNumber;
        maxTime = nodTime;
    }
  
    void DoHeadMovement(float amplitude, float angularFrequency, float numShakes, Vector3 rotationAxis)
    {
        //m_CurrentNeckRot = amplitude * Mathf.Sin(angularFrequency * time);
        m_CurrentNeckRot = amplitude * Mathf.Sin((time / maxTime) * 2.0f * Mathf.PI * numShakes);
        time += Time.deltaTime;

        m_Neck.transform.Rotate(rotationAxis, m_CurrentNeckRot, 0);
     
        if (time >= maxTime)
        {
            m_RotateHead = false;
            m_CurrentNeckRot = 0;
            time = 0;
        }
    }
    
    void OnAvatarIK()
    {
        if(baseCurrentState.IsName("Base Layer.GrabIdle") || baseNextState.IsName("Base Layer.GrabIdle"))
        {
            float grabCurve = animator.GetFloat ("GrabCurve");
            AvatarIKGoal lefthand = AvatarIKGoal.LeftHand;

            //Debug.Log("grabCurve: " + grabCurve);

            if(grabbableObj != null && !gotWrench) 
            {
                // Use the Curve from grabbing animation to drive smoothing of avatar hand position
                animator.SetIKPositionWeight(lefthand, grabCurve);
                animator.SetIKRotationWeight(lefthand, grabCurve);
                
                // Set IK hand position and rotation destination 
                animator.SetIKPosition(lefthand, grabbableObj.position);
                animator.SetIKRotation(lefthand, grabbableObj.rotation);
                animator.SetBool("Grab", false);
            }
            else
            {
                // Reset Hand to be controlled by animation
                animator.SetIKPositionWeight(lefthand, 0);
                animator.SetIKRotationWeight(lefthand, 0);  
            }
        }
    }
    #endregion
}
