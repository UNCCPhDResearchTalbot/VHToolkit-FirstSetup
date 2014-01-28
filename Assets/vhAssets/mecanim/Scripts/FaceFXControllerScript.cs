//----------------------------------------------------------------------------------------------------------------------------
// A FaceFX Controller script to drive bones-based animations created with the FaceFXImportXMLActor.js file  
//
// Owner: Doug Perkowski
//----------------------------------------------------------------------------------------------------------------------------
//  License Information
//
// Copyright (c) 2002-2010 OC3 Entertainment, Inc.  
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The person or entity using the Software must first obtain a commercial license to FaceFX Studio Professional, 
// FaceFX Unlimted, or a FaceFX Plugin from OC3 Entertainment, Inc. or an authroized reseller.
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//----------------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
 


public class FaceFXControllerScript : MonoBehaviour {

	

// Import XML Variables
// If the FaceFX reference pose is identical to the default pose in the FBX file, then we can use the FBX file 
// to define the reference pose.  This allows us to detect differences between the XML file's reference pose and 
// the FBX's reference pose, and adjust the bone poses accordingly to reflect these differences.  Then 
// one XML file is sufficient for all characters.
// This option must be turned off if the referencve pose is different from the FBX pose. 
public bool  UseReferencePoseFromFBX = true;

// The ScaleFactor should match the scale of your FBX scaling.  
public float ScaleFactor = 1;


// This string will be appended to the FaceFX animation name to create a new event
// track animation, but only if there are FaceFX events with non-empty "payload" values.
// Unity events can't be inserted into the main FaceFX animation because this animation is 
// force-ticked by this script and the unity events are not fired.
public string EVENT_TRACK_NAME = "_eventtrack";


//------------------------------------------
	
// The facefx controller is an immediate child of the player GameObject.  It holds per-animation information like the audio start time 
// and curves for bone poses.  The audio start time is stored in the localPosition.x property of the facefx controller.  The controller 
// has one child per bone pose, and the bone pose curves are stored in the localPosition.x property of the children.  The bone poses 
// themselves are stored as Unity animations with the "facefx" prefix.  They drive the skeleton and are blended additively.  This 
// object is created by the FaceFXImportXMLActor Editor script.
private GameObject facefx_controller;

// The audio_clip to play associated with animation_name.  The audio start time is recorded for each animation in localPosition.x property
// of the facefx_controller for that animation.  Once playing, the audio determines the evaluation time of animation_name to prevent the 
// audio from getting out of synch with the animation.
private AudioClip audio_clip;

// The name of the animation.  This animation should not move bones directly, but rather drive the children of the facefx_controller.
private string animation_name;
	
// This is the layer that the addative bone pose animations are played on.
const int FACEFX_BONEPOSE_LAYER = 10;
// This is the layer of the actual FaceFX animation
const int FACEFX_ANIMATION_LAYER = 9;
// If you are using FaceFX events with a payload to communicate with your game
// (for playing full body animations perhaps), then you need a layer to play 
// the event track on.  .
const int FACEFX_EVENTTRACK_LAYER = 8;	
// Set the FACEFX_REFERENCEPOSE_LAYER above the layer of your skeleton animation
// to make facefx overwrite the bone transforms (if your full body animation has
// facial animation, but you want to ignore it in favor of FaceFX output.)
const int FACEFX_REFERENCEPOSE_LAYER = 7;

	
// 0 - Not playing / Ready to play.
// 1 - playing animation prior to audio.
// 2 - playing with audio.
// 3 - playing after audio ends.
// 4 - Blending out.
private int play_state;
// Cache the starting time of the audio.  This is stored in the localPosition.x property of the facefx_controller for each animation.
private float audio_start_time;

// We don't want to evaluate animations beyond their end time, so we use this to cache the animation evaluation time.
private float anim_eval_time;

// an inverse_hermite curve is computed and cached because of the way bone poses need to be driven within unity's animation system.
private static AnimationCurve inverse_hermite;

// A switch to tell anyone who cares that we have finished playing an animation and are ready for the next one.
private static bool  switch_anim;

 


public void InitializeFaceFXController ( GameObject ffxController  ){
	if( ffxController == null )
	{
		Debug.Log("Can not initialize null FaceFX controller.");
	}
	else
	{
		facefx_controller = ffxController;
		foreach(Transform child in facefx_controller.transform) 
		{
			AnimationState bonePoseAnim = animation[child.name];
			if( bonePoseAnim != null ) 
			{
			    // Keep bone pose animations in their own layer with addative blending
				// and ClampForever wrapping.  Enable them and set the weight to 1.
				// We are then prepared to manually adjust the "time" of the animation 
				// in the Update function to control the amount the bone pose is blended in.
				bonePoseAnim.layer = FACEFX_BONEPOSE_LAYER;
				bonePoseAnim.blendMode = AnimationBlendMode.Additive;
				bonePoseAnim.wrapMode = WrapMode.ClampForever;
				bonePoseAnim.enabled = true;
				bonePoseAnim.weight =1;
			}
		}
		if (animation == null)
		{
			Debug.Log("Warning.  Animation component must be attached to " + name + " character for animations to play!");
		}
		else
		{
		
			// The loop anim is created from the XML import.  We need a non-additive animation to play,
			// And it just has the reference pose.
			AnimationState loopAnim = animation["facefx_loop_anim"];
			if( loopAnim != null )
			{
				loopAnim.layer = FACEFX_REFERENCEPOSE_LAYER;
				loopAnim.wrapMode = WrapMode.ClampForever;
				animation.Play("facefx_loop_anim");
			}
			else
			{
				Debug.Log("No facefx_loop_anim animation found for " + name + ".  The facefx_controller is likely corrupt and should be reimported.");
			}			
		}
		foreach(Transform child in facefx_controller.transform) 
		{
			AnimationState anim = animation[child.name];
			if( anim != null ) 
			{ 
				// This prevents bones from shaking.
				anim.normalizedSpeed = 0;
			}	
		}
	}
}



	

void Start (){
	Debug.Log("Starting FaceFX Controller.");
	switch_anim = true;
	play_state = 0;
	Transform facefx_controller_transform = this.transform.Find("facefx_controller");
	if (facefx_controller_transform != null) 
	{
	    facefx_controller = facefx_controller_transform.gameObject;
	}
	if ( facefx_controller == null ) 
	{
		Debug.Log("Warning.  Could not find FaceFX Controller for " + name + "!  You need to use the ImportXML function and pass a FaceFX XML file.");
	}
	else
	{
		InitializeFaceFXController(	facefx_controller );
	}
	if( inverse_hermite == null )
	{
		inverse_hermite = new AnimationCurve();
		AnimationCurve hermiteCurve= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		
		// The "step" here defines how accurate the inverse_hermite curve is.
		for ( float i = 0;  i <= 1; i = i + (float).01 )
		{
			inverse_hermite.AddKey( hermiteCurve.Evaluate(i), i );
		}
	}
}

// Stops animation and audio from playing.  Resets the states so animations can be 
// played again from the start.
public void StopAnim (){	
	// If we have stopped this animation prematurely, we need to stop the aduio.
	audio.Stop();
	// Setting this to 0 means we are ready to play another animation.
	play_state = 0;	
	switch_anim = true;
}

// Facial animations  frequently start before the corresponding audio becuase the mouth needs to 
// move into the correct position to form the first sound.  The audio start time is stored in the 
// localPosition.x value of the facefx controller for the particular animation.
public void PlayAudioFunction (){
	if(audio.isPlaying)
	{
		Debug.Log("Audio is already playing!");
	}
	else
	{
		audio.clip = audio_clip;
		audio.Play();
	}
}

// An animation name and an audio clip are passed to the  function
// to start playing a facial animation.
public IEnumerator PlayAnimCoroutine ( string animName ,   AudioClip animAudio  ){
	switch_anim = false;
	play_state = 1;	
	anim_eval_time = 0;
	audio_start_time = 0;
	if( null == animAudio)
	{
		Debug.Log("Audio is null!");
	}
	animation_name = animName;
	audio_clip = animAudio;
	
	if( animName != null )
	{
			
		AnimationState animState = animation[animation_name];
		if( animState != null ) 
		{
			Debug.Log("playing anim " + animName);
			animState.speed = 0;
			animState.time = 0;
			animState.layer = FACEFX_ANIMATION_LAYER;

            if (!animation.Play(animName))
            {
                Debug.Log(string.Format("Animation {0} failed to play", animName));
            }
			
			if( facefx_controller != null )
			{
				// Set to a high value so that we don't trigger audio until audio_start_time is correctly set and we are past it.
				audio_start_time = 1000;
				// Yield for one frame so that the facefx controller x position (representing the start time of the audio) has time to update with the animation. 
				yield return null;
				audio_start_time = facefx_controller.transform.localPosition.x;
			}
		}
		else
		{
			Debug.Log("No AnimationState for animation:" + animation_name + " on player " + name);
		}	
			
		if( animation[animation_name+ EVENT_TRACK_NAME] != null )
		{
            if (!animation.Play(animation_name + EVENT_TRACK_NAME))
            {
                Debug.Log(string.Format("Animation {0} failed to play", animName));
            }
		}
	}
	else
	{
		Debug.Log("No animation passed into PlayAnim.  Playing audio.");
		PlayAudioFunction();
	}
}
public void PlayAnim(string animName,   AudioClip animAudio)
{
	StartCoroutine(PlayAnimCoroutine(animName,animAudio));
}

public void handleFaceFXPayLoadEvent( string payload )
{
	if( payload.StartsWith("game: playanim "))
	{
		string animname = payload.Substring(15);
		if( null != animation[animname] )
		{
			Debug.Log("playing body animation from payload: " + animname);
			animation.Play(animname);
		}
		else
		{
			Debug.Log("Payload animation doesn't exist: " + animname);
		}
	}
	else
	{
		Debug.Log("Unknown event payload: " + payload);
	}
}
public void Update (){	
	if ( play_state > 0 )
	{
		AnimationState animState = animation[animation_name];
		if( animState != null ) 
		{
			// We calcualte the animation evaluation time here.  It is overridden by the audio-based time if audio is playing.
			anim_eval_time = anim_eval_time + Time.deltaTime;

			if (play_state == 1)
			{	
				if(animState.time >= audio_start_time)
				{
               
					PlayAudioFunction();	
					play_state = 2;					
				}
			}		
			if (play_state == 2)
			{
				// audio.isPlaying is not a reliable test alone because audio stops when you loose focus.
				// But without it, the audio.time can reset to 0 when audio is finished.
				if(audio.isPlaying &&  audio.time < audio_clip.length)
				{		
					// While audio is playing, assume control of animation playback and force synch it to the audio.
					anim_eval_time = audio.time + audio_start_time;
				}
				else if( !audio.isPlaying )
				{	
					play_state = 3;			
				}
			}	
			if (play_state == 3)
			{
				if ( anim_eval_time >= animState.length )
				{
					switch_anim = true;
					play_state = 0;
				}					
			}
			// Only "tick" the animation if it wouldn't put us over the animation bounds.
			if( anim_eval_time <= animState.length )
			{		
				animState.time = anim_eval_time;
			}			
			if ( facefx_controller != null )
			{
				foreach(Transform child in facefx_controller.transform) 
				{
					AnimationState anim = animation[child.name];
					if( anim != null ) 
					{

						// The x axix stores this bone pose's curve.  
						//The normalized time of the animation is from 0-1.  
						//At 1, the bone pose is fully driven.  At 0, it is the reference pose.
						// Unfortunately, the interpolation from 0-1 is a hermite curve, not linear. So we use the inverse_hermite
						// curve to figure out what value we need to pass into the hermite curve evaluation to drive the bone pose by
						// child.transform.localPosition.x
						anim.normalizedTime = inverse_hermite.Evaluate( child.transform.localPosition.x) ;		
						
						// Remove shaking by setting normalized speed to 0.
						anim.normalizedSpeed = 0;
					}	
				}
			}					
		}
		// To support audio playback without animation, reset the state if the animation is null and the audio is finished playing.
		else if( !audio.isPlaying )
		{
			Debug.Log("audio with no animation case");
			switch_anim = true;
			play_state = 0;			
		}
	}
}
public int GetPlayState (){
	return play_state;
}
public static bool GetSwitchAnim (){
	return switch_anim;
}

public Transform GetFaceFXControllerGameObject(){
	return transform.Find("facefx_controller");
}
}


