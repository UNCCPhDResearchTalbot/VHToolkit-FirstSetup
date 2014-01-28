
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml; 


[CustomEditor(typeof(FaceFXControllerScript))]


public class FaceFXControllerScriptGUI : Editor {

// A class storing a bone transform.  The constructor takes the contents of a <bone> xml body (Space-seperated position rotation scale values).
public struct BoneTransform 
{
	public float[] Values;
	// Constructs a BoneTransform from a space-separated string (originating from an XML file)
	public BoneTransform ( string aValue  ){

      string[] StringValues = aValue.Split();
      Values = new float[10];
		if (StringValues.Length != 10)
		{
			Debug.Log("Error in XML.  A reference boen has only this many values:" + Values.Length ); 
			Debug.Log(aValue);
		}
		else
		{
			// Position (x, y, z)
			// @todo - Figure out why Pos.x and Rot.x values need to be negated.
			Values[0] = -float.Parse(StringValues[0]);  
			Values[1] = float.Parse(StringValues[1]);
			Values[2] = float.Parse(StringValues[2]);
			
			// Rotation (x, y, z, w but in the XML file it is stored as w,x,y,z)  
			Values[3] = -float.Parse(StringValues[4]);
			Values[4] = float.Parse(StringValues[5]);
			Values[5] = float.Parse(StringValues[6]);
			Values[6] = float.Parse(StringValues[3]);
			
			// Scale (x, y, z)
			Values[7] = float.Parse(StringValues[7]);
			Values[8] = float.Parse(StringValues[8]);
			Values[9] = float.Parse(StringValues[9]);
		}
   }
	public BoneTransform ( GameObject t  ){
		Values = new float[10];
	   Values[0] = t.transform.localPosition.x;
	   Values[1] = t.transform.localPosition.y;
	   Values[2] = t.transform.localPosition.z;
	   Values[3] = t.transform.localRotation.x;
	   Values[4] = t.transform.localRotation.y;
	   Values[5] = t.transform.localRotation.z;
	   Values[6] = t.transform.localRotation.w;
	   Values[7] = t.transform.localScale.x;
	   Values[8] = t.transform.localScale.y;
	   Values[9] = t.transform.localScale.z;  
   }
	public BoneTransform ( Vector3 pos ,   Quaternion rot ,   Vector3 scale  ){
		Values = new float[10];
	   Values[0] = pos.x;
	   Values[1] = pos.y;
	   Values[2] = pos.z;
	   Values[3] = rot.x;
	   Values[4] = rot.y;
	   Values[5] = rot.z;
	   Values[6] = rot.w;
	   Values[7] = scale.x;
	   Values[8] = scale.y;
	   Values[9] = scale.z;  
   }      
	public void Print (){
	   Debug.Log("( " + Values[0] + ", " + Values[1] + ", " + Values[2]  + ") ("  + Values[3] +", " + Values[4] +", " + Values[5] +", " + Values[6] + ") ("+ Values[7] +", " + Values[8] +", " +Values[9] + ")");
	}
	public Vector3 GetPos (){
		return new Vector3(Values[0], Values[1], Values[2]);
	}
	public Quaternion GetRot (){
		return new Quaternion( Values[3], Values[4], Values[5], Values[6]);
	}
	public Vector3 GetScale (){
		return new Vector3(Values[7], Values[8], Values[9]);
	}
}
// A class to help manage adding keys to curves and curves to clips.
struct AnimClipHelper 
{

	AnimationCurve curvePosX; 
	AnimationCurve curvePosY; 
	AnimationCurve curvePosZ;
	AnimationCurve curveRotX;
	AnimationCurve curveRotY;
	AnimationCurve curveRotZ; 		
	AnimationCurve curveRotW;
	AnimationCurve curveScaleX; 
	AnimationCurve curveScaleY; 
	AnimationCurve curveScaleZ; 
	public AnimationClip animclip;

	public AnimClipHelper(AnimationClip clip){
		animclip = clip;
		curvePosX = new AnimationCurve();
		curvePosY = new AnimationCurve();
		curvePosZ = new AnimationCurve();
		curveRotX = new AnimationCurve();
		curveRotY = new AnimationCurve();
		curveRotZ = new AnimationCurve();
		curveRotW = new AnimationCurve();
		curveScaleX = new AnimationCurve();
		curveScaleY = new AnimationCurve();
		curveScaleZ = new AnimationCurve();		
	}

	public void PreAddKeys (){
		curvePosX = new AnimationCurve();
		curvePosY = new AnimationCurve();
		curvePosZ = new AnimationCurve();
		curveRotX = new AnimationCurve();
		curveRotY = new AnimationCurve();
		curveRotZ = new AnimationCurve();
		curveRotW = new AnimationCurve();
		curveScaleX = new AnimationCurve();
		curveScaleY = new AnimationCurve();
		curveScaleZ = new AnimationCurve();	
	}

	public void AddKeys (  float t ,   BoneTransform values   ){

		// Position x,y,z
		curvePosX.AddKey(new Keyframe(t,values.Values[0]));
		curvePosY.AddKey(new Keyframe(t,values.Values[1]));
		curvePosZ.AddKey(new Keyframe(t,values.Values[2]));
		
		// Rotation x,y,z,w
		curveRotX.AddKey(new Keyframe(t,values.Values[3]));
		curveRotY.AddKey(new Keyframe(t,values.Values[4]));
		curveRotZ.AddKey(new Keyframe(t,values.Values[5]));
		curveRotW.AddKey(new Keyframe(t,values.Values[6]));
		
		// Scale x,y,z
		curveScaleX.AddKey(new Keyframe(t,values.Values[7]));
		curveScaleY.AddKey(new Keyframe(t,values.Values[8]));
		curveScaleZ.AddKey(new Keyframe(t,values.Values[9]));		
	}
	
	public void PostAddKeys (  string objectRelativePath   ){
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localPosition.x", curvePosX);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localPosition.y", curvePosY);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localPosition.z", curvePosZ);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localRotation.x", curveRotX);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localRotation.y", curveRotY);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localRotation.z", curveRotZ);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localRotation.w", curveRotW);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localScale.x", curveScaleX);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localScale.y", curveScaleY);
		animclip.SetCurve(objectRelativePath, typeof(Transform), "localScale.z", curveScaleZ);	
	}
}
	// The facefx controller is an immediate child of the player GameObject.  It holds per-animation information like the audio start time 
	// and curves for bone poses.  The audio start time is stored in the localPosition.x property of the facefx controller.  The controller 
	// has one child per bone pose, and the bone pose curves are stored in the localPosition.x property of the children.  The bone poses 
	// themselves are stored as Unity animations with the "facefx" prefix.  They drive the skeleton and are blended additively.  This 
	// object is created by the FaceFXImportXMLActor Editor script.
	private GameObject facefx_controller;



	public FaceFXControllerScript _target;
    void OnEnable()    
    {
        _target = (FaceFXControllerScript)target;
    }	
    public override void OnInspectorGUI () 
    {
		if( GUILayout.Button("Print Instructions To Log") )
		{
			string instructions = "The FaceFXControllerScript plays FaceFX animations on your Unity character.  " +
			"Attach the script to your character, then import a FaceFX XML Actor file that contains bone poses " +
			"and animation data.  Then play animations using the FaceFxControllerScript PlayAnim function. " +
			"\n\nTips:\n------------------------------------------" +
			"\n\nScaleFactor:\nChange the Scale Factor to match your FBX import settings.  Then Import the XML Actor file." +
			"\n\nFBX Ref Pose:\nCheck this box if your FBX file has the character in the reference pose.  It allows you to use " +
			"the same XML file on multiple characters even if their reference pose is slightly different (because one character " +
			"is taller for example).  The bone poses in the XML file will be transferred onto your character.  Be sure to import " +
			"your XML file after changing this setting." +
			"\n\nNo Face Graph Links:\nYour XML file's Face Graph should contain only bone poses, no links or combiner nodes because " +
			"the Unity integration can not evaluate the Face Graph.  Use the fgcollapse command from FaceFX Studio " +
			"prior to XML export to remove links and combiner nodes.  If you are analyzing audio from a plugin, use " +
			"the Assets\\PhraseLockedDemo\\Analysis-Actor-For-Unity-Demo-Plugin-Use.facefx file to get head " +
			"and eye rotations.\n\n";
			 Debug.Log(instructions);
		}
    	EditorGUILayout.BeginVertical("box");
    	GUILayout.Label("Import Actor Settings: (applied when importing actor)");
		float scaleFactor = EditorGUILayout.FloatField("Scale Factor", _target.ScaleFactor);
		bool useFBXRefPose = EditorGUILayout.Toggle("FBX Ref Pose", _target.UseReferencePoseFromFBX);
		EditorGUILayout.EndVertical();
        if (GUI.changed)
        {
        	_target.ScaleFactor = scaleFactor;
        	_target.UseReferencePoseFromFBX = useFBXRefPose;
        	Debug.Log("Import XML Actor settings have changed.  Import an XML Actor to use the new settings.");
        }
		if( GUILayout.Button("Import Actor & Animations") )
		{
			string path= EditorUtility.OpenFilePanel ("Import FaceFX XML Actor C#", "", "xml");
			if (path.Length != 0)
			{
				if(File.Exists(path))
				{
					Debug.Log("Importing XML Actor: " + path);
					string file_contents = File.OpenText(path).ReadToEnd();
					ImportXML(file_contents);
				}
			}			
		}
		// Only allow importing animations if there is a facefx_controller object.
		if( _target.GetFaceFXControllerGameObject() )
		{
			if( GUILayout.Button("Import Animations Only") )
			{
				Debug.Log("Importing animations from XML file.");
				string path= EditorUtility.OpenFilePanel ("Import FaceFX XML Animations C#", "", "xml");
				if (path.Length != 0)
				{
					if(File.Exists(path))
					{
						Debug.Log("Importing animations from XML file: " + path);
						string file_contents = File.OpenText(path).ReadToEnd();
						ImportXMLAnimations(file_contents);
					}
				}			
			}
		}
	}


	public void ImportXMLAnimations (  string xmltext  ){
		XmlDocument doc_reader= new XmlDocument();
		doc_reader.Load(new StringReader(xmltext));
		Transform facefx_controller = _target.transform.Find("facefx_controller");


        if (_target.gameObject.GetComponent<WordBreakInfo>() == null)
        {
            _target.gameObject.AddComponent<WordBreakInfo>();
        }

        _target.gameObject.GetComponent<WordBreakInfo>().m_anims = new WordBreakInfo.WordBreakAnim [0];


		int numAnimsImported = 0;
		if (facefx_controller == null )
		{
			Debug.Log("Warning: No facefx_controller object exists.  Failed to import animations.");
			return;		
		}
		else
		{
			XmlNodeList animNodeList= doc_reader.SelectNodes("/actor/animation_groups/animation_group/animation/curves");
			for ( int i = 0; i < animNodeList.Count; ++i )
			{
				float lastKeyTime = 0;
				float firstKeyTime = 0;					

				XmlNode curvesNode= animNodeList.Item(i); 
				string animName = curvesNode.ParentNode.Attributes["name"].Value;
				string animGroupName = curvesNode.ParentNode.ParentNode.Attributes["name"].Value;

				AnimClipHelper controllerAnimHelper= new AnimClipHelper(new AnimationClip());
			
				ArrayList curveArray= new ArrayList();
				ArrayList relativePathArray= new ArrayList();
				int j = 0;
				for( j = 0; j < curvesNode.ChildNodes.Count; ++j )
				{
					XmlNode curveFirstKeyNode= curvesNode.ChildNodes.Item(j); 
					int first_keytime_end = curveFirstKeyNode.InnerText.IndexOf(" ");
					if( first_keytime_end > -1 )
					{
						string first_keytime_string = curveFirstKeyNode.InnerText.Substring(0, first_keytime_end);
						float first_keytime = float.Parse(first_keytime_string);
						if( first_keytime< firstKeyTime )
						{
							firstKeyTime = first_keytime;
						}											
					}					
				}
				if( firstKeyTime > 0 )
				{
					firstKeyTime = 0;
				}

                

				for( j = 0; j < curvesNode.ChildNodes.Count; ++j )
				{
					XmlNode curveNode= curvesNode.ChildNodes.Item(j); 
					string curveName= curveNode.Attributes["name"].Value;
					int numKeys= int.Parse(curveNode.Attributes["num_keys"].Value);
					string curveNodeBodyString= curveNode.InnerText;
					string[] curveKeys = curveNodeBodyString.Split();

					if ( curveKeys.Length >= numKeys * 4 )
					{
						AnimationCurve bonePoseAnimCurve= new AnimationCurve();
						float keytime = 0;
						float keyvalue = 0;
						float keyslopeIn = 0;
						float keyslopeOut = 0;
						int k = 0;
						for( k = 0; k < numKeys; ++k )
						{
							int keyI= k*4;
							keytime = float.Parse(curveKeys[keyI + 0]);
							keyvalue = float.Parse(curveKeys[keyI + 1]);
							keyslopeIn = float.Parse(curveKeys[keyI+ 2]);
							keyslopeOut = float.Parse(curveKeys[keyI + 3]);

                            /*if (animName.IndexOf("brad_alwayssure") != -1)
                            {
                                Debug.LogWarning(string.Format("{0} {1} {2} {3} {4}", curveName, keytime.ToString("f2"), keyvalue.ToString("f2"), keyslopeIn.ToString("f2"), keyslopeOut.ToString("f2")));
                            }*/
							
							// Shift the entire animation by the firstKeyTime, which is negative or 0.
							// Then all key times are >= 0
							keytime -= firstKeyTime;
							
							bonePoseAnimCurve.AddKey(new Keyframe(keytime,keyvalue, keyslopeIn, keyslopeOut));
						}
						if( keytime > lastKeyTime )
						{
							lastKeyTime = keytime;
						}
						Transform bonePoseObject = _target.transform.Find ("facefx_controller/facefx " + curveName);
                        if (bonePoseObject != null)
                        {
                            string controller_relative_path = GetRelativePath(bonePoseObject);
                            curveArray.Add(bonePoseAnimCurve);
                            relativePathArray.Add(controller_relative_path);
                        }
                        else
                        {
                            //Debug.LogWarning("couldn't find " + ("facefx_controller/facefx " + curveName));
                        }
					}
					else
					{
						Debug.Log("There is an error in the XML file.  There are insufficient keys.");
					}
				}
			
				for ( j = 0; j < curveArray.Count; ++j )
				{
					AnimationCurve bonePoseCurve = curveArray[j] as AnimationCurve;
					// Unity doesn't like evaluating curves before or after the first/last key, so make sure each curve has
					// keys at the boundaries of the animations. 	
					int keyCount = bonePoseCurve.keys.Length;
					if ( keyCount > 0 )
					{
						if(bonePoseCurve.keys[0].time > 0)
						{
							bonePoseCurve.AddKey(new Keyframe(0, bonePoseCurve.Evaluate(0)));
						}
						if(bonePoseCurve.keys[keyCount-1].time < lastKeyTime)
						{
							bonePoseCurve.AddKey(new Keyframe(lastKeyTime, bonePoseCurve.Evaluate(lastKeyTime)));
						}										
					}
					controllerAnimHelper.animclip.SetCurve(relativePathArray[j] as string, typeof(Transform), "localPosition.x", bonePoseCurve);
				}
											
				// Using Unity Animation events was not a reliable way to trigger audio.													
				//AnimationEvent audioEvent = AnimationEvent();
				//audioEvent.functionName  =  "PlayAudio";
				//audioEvent.time = -firstKeyTime;
				//AnimationUtility.SetAnimationEvents(controllerAnimHelper.animclip, [audioEvent] );




				// Store the audio start time in the localPosition.x value of the facefx controller.
				string objectRelativePath = GetRelativePath(facefx_controller);
				AnimationCurve audioStartTimeCurve =new AnimationCurve();
				audioStartTimeCurve.AddKey(new Keyframe(0,-firstKeyTime));
				audioStartTimeCurve.AddKey(new Keyframe(lastKeyTime,-firstKeyTime));
				controllerAnimHelper.animclip.SetCurve(objectRelativePath, typeof(Transform), "localPosition.x", audioStartTimeCurve);
				
				if( null != _target.animation[animGroupName + "_" + animName] )
				{
						Debug.Log("Replacing existing FaceFX animation: " + animGroupName + "_" + animName);
				}



                /*
				AnimationEvent animEvent = new AnimationEvent();
				animEvent.functionName  =  "TestEd2";
				//animEvent.stringParameter = "Test 1 2 3";
				animEvent.time = 0.2f;
				animEvent.messageOptions = SendMessageOptions.RequireReceiver;  

				List<AnimationEvent> events = new List<AnimationEvent>();
				events.Add(animEvent);

				AnimationUtility.SetAnimationEvents(controllerAnimHelper.animclip, events.ToArray());
				Debug.Log("Adding animEvent");
                */


                List<WordBreakInfo.WordBreak> wordBreaks = new List<WordBreakInfo.WordBreak>();


                {
                    XmlNodeList wordList = doc_reader.SelectNodes("/actor/animation_groups/animation_group/animation/words");

                    for (int s = 0; s < wordList.Count; s++)
                    {
                        XmlNode wordsNode = wordList.Item(s);

                        string wordListAnimName = wordsNode.ParentNode.Attributes["name"].Value;
                        if (wordListAnimName != animName)
                            continue;

                        for (int t = 0; t < wordsNode.ChildNodes.Count; t++ )
                        {
                            WordBreakInfo.WordBreak wordBreak = new WordBreakInfo.WordBreak();

                            XmlNode wordNode = wordsNode.ChildNodes.Item(t);
                            if (wordNode.Attributes["start"] != null)
                            {
                                wordBreak.start = float.Parse(wordNode.Attributes["start"].Value);
                            }

                            if (wordNode.Attributes["end"] != null)
                            {
                                wordBreak.end = float.Parse(wordNode.Attributes["end"].Value);
                            }

                            wordBreak.name = wordNode.InnerText;

                            wordBreaks.Add(wordBreak);
                        }
                    }
                }


                WordBreakInfo.WordBreakAnim wordBreakAnim = new WordBreakInfo.WordBreakAnim();
                wordBreakAnim.animName = animName;
                string assetPath = "Assets/StreamingAssets/Sounds/" + animName + ".wav";
                wordBreakAnim.audioClip = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip)) as AudioClip;
                wordBreakAnim.wordBreaks = wordBreaks.ToArray();


                WordBreakInfo wordBreakInfo = _target.gameObject.GetComponent<WordBreakInfo>();
                List<WordBreakInfo.WordBreakAnim> anims = new List<WordBreakInfo.WordBreakAnim>(wordBreakInfo.m_anims);
                anims.Add(wordBreakAnim);
                wordBreakInfo.m_anims = anims.ToArray();



				_target.animation.AddClip(controllerAnimHelper.animclip, animGroupName + "_" + animName);
				numAnimsImported++;
			}
			
			XmlNodeList animEventTakeList= doc_reader.SelectNodes("/actor/animation_groups/animation_group/animation/event_take");
			for ( int i = 0; i < animEventTakeList.Count; ++i )
			{
				XmlNode eventsNode = animEventTakeList.Item(i); 
				
				string animName = eventsNode.ParentNode.Attributes["name"].Value;
				string animGroupName = eventsNode.ParentNode.ParentNode.Attributes["name"].Value;
					
				int j = 0;
				int numEvents = 0;
				for( j = 0; j < eventsNode.ChildNodes.Count; ++j )
				{
					XmlNode eventNode= eventsNode.ChildNodes.Item(j); 
					if( eventNode.Attributes["payload"] != null )
					{
						numEvents++;			
					}				
				}
				string eventTrackAnimName = animGroupName + "_" + animName + _target.EVENT_TRACK_NAME;
				if( null != _target.animation[eventTrackAnimName] )
				{
					if( numEvents > 0 )
					{		
						Debug.Log("Replacing existing event track animation: " +  eventTrackAnimName); 
					}
					else
					{
						Debug.Log("Deleting existing event track animation.  It isn't needed: " +  eventTrackAnimName);
					}
					_target.animation.RemoveClip(eventTrackAnimName);
				}
				if( numEvents > 0 )
				{	
					_target.animation.AddClip(new AnimationClip(), eventTrackAnimName);
					
							
					AnimationState animState = _target.animation[eventTrackAnimName];

					int eventIndex = 0;
					AnimationEvent [] evts = new AnimationEvent[numEvents];
					for( j = 0; j < eventsNode.ChildNodes.Count; ++j )
					{
						XmlNode eventNode= eventsNode.ChildNodes.Item(j); 
						if( eventNode.Attributes["payload"] != null )
						{

							string payload = eventNode.Attributes["payload"].Value;
							float start_time =  float.Parse(eventNode.Attributes["start_time"].Value);
							AnimationEvent ev = new AnimationEvent();
							ev.time = start_time;
							ev.stringParameter = payload;
							ev.functionName = "handleFaceFXPayLoadEvent";
							evts[eventIndex] = ev;	
							eventIndex++;							
						}				
					}
					AnimationUtility.SetAnimationEvents (animState.clip, evts);
				}
			}
		}
		if( numAnimsImported == 0 )
		{
			Debug.Log("Warning: No Animations imported. Check the XML file.");
		}
		else 
		{
			Debug.Log("Imported "+ numAnimsImported + " animations." );
		}	
	}
	// Searches the object this script is attached to recursively to find a match.  We can't use GameObject.Find because that searches the whole scene.  Transform.Find searches one level.
	Transform RecursiveFind ( Transform trans ,   string searchName  ){
		foreach(Transform child in trans) 
		{
			if(child.name == searchName)
			{
				return child;
			}	
			Transform returnTransform = RecursiveFind(child, searchName) ;
			if( returnTransform != null )
			{
				return returnTransform;
			}
		}
		return null;
	}
	public void ImportXML (  string xmltext  ){
		XmlDocument doc_reader= new XmlDocument();
		doc_reader.Load(new StringReader(xmltext));
			
		// Test to see if this is a FaceFX XML file
		XmlNodeList faceGraphNodes = doc_reader.SelectNodes("/actor/face_graph");
		if( faceGraphNodes.Count > 0 )
		{
			// Use the scale factor from the XML file if it exists.
			if( faceGraphNodes.Item(0).ParentNode.Attributes["scalefactor"] != null)
			{
				_target.ScaleFactor = float.Parse(faceGraphNodes.Item(0).ParentNode.Attributes["scalefactor"] .Value);
				Debug.Log("Using scale factor from XML file:" + _target.ScaleFactor);
			}
			else
			{
				Debug.Log("Using scale factor from Unity Settings:" + _target.ScaleFactor);
			}
			
			Transform existing_controller= _target.transform.Find("facefx_controller");
			if( existing_controller != null ) 
			{
				Debug.Log( "Warning: There was an existing facefx controller.  Deleting.");
				DestroyImmediate(existing_controller.gameObject);
				
			}
			facefx_controller  = new GameObject("facefx_controller");
			
	        facefx_controller.transform.parent = _target.transform;
	        
	        // Initialize the transforms of the FaceFX controller so that they are identical to the parent.
	        // Otherwise the facefx_controller and sub objects can get junk in their localPosition.x values
	        // which are used to drive the system.
	        facefx_controller.transform.localPosition = new Vector3(0, 0, 0);
	        facefx_controller.transform.localRotation = Quaternion.identity;
			facefx_controller.transform.localScale = new Vector3(1, 1, 1);
			
	        
			XmlNodeList refBoneList = doc_reader.SelectNodes("/actor/face_graph/bones/bone");
			
			Hashtable myRefBoneIndexTable= new Hashtable();
			
			
			ArrayList myRefBoneFileTransforms= new ArrayList();
			ArrayList myRefBoneNames= new ArrayList();
			ArrayList myRefBoneGameObjectTransforms= new ArrayList();
			ArrayList myRefBoneGameObjectBoneTransforms= new ArrayList();
			
			ArrayList myRefBoneFilePositions= new ArrayList();
			ArrayList myRefBoneFileRotations= new ArrayList();
			ArrayList myRefBoneFileScales= new ArrayList();
			int i = 0;
			for ( i = 0; i < refBoneList.Count; ++i )
			{
				XmlNode refBone= refBoneList.Item(i); 
				string refBoneName = refBone.Attributes["name"].Value;			
				myRefBoneNames.Add(refBoneName);
				
				myRefBoneIndexTable[refBoneName] = i;
				
				Transform refBoneObjectTransform = RecursiveFind (_target.transform, refBoneName);
				myRefBoneGameObjectTransforms.Add(refBoneObjectTransform);	
				if( refBoneObjectTransform == null )
				{
					Debug.Log("Warning: Couldn't find refbone: " + refBoneName);
					refBoneObjectTransform = _target.transform;
				}

				
				BoneTransform trans= new BoneTransform( refBone.InnerText);
				Vector3 myRefBonePos= trans.GetPos();
				Quaternion myRefBoneQuat= trans.GetRot();
				Vector3 myRefBoneScale= trans.GetScale();
				
				// Scale position by ScaleFactor
				Vector3 myScaledRefBonePos = Vector3.Scale(myRefBonePos, new Vector3( _target.ScaleFactor, _target.ScaleFactor, _target.ScaleFactor));
				
				myRefBoneGameObjectBoneTransforms.Add( new BoneTransform(refBoneObjectTransform.localPosition, refBoneObjectTransform.localRotation, refBoneObjectTransform.localScale));
						
				myRefBoneFileTransforms.Add(new BoneTransform( myScaledRefBonePos, myRefBoneQuat, myRefBoneScale));
				myRefBoneFilePositions.Add(myScaledRefBonePos);
				myRefBoneFileRotations.Add(myRefBoneQuat);
				myRefBoneFileScales.Add(myRefBoneScale);
				
			}
			XmlNodeList linkList = doc_reader.SelectNodes("/actor/face_graph/links/link");
			if( linkList.Count > 0 )
			{
				Debug.LogWarning("This actor contains link functions that can not be evaluated in Unity.  Be sure to collapse your actor with the fgcollapse command before exporting the XML!");
			}
			ArrayList myBonePoses= new ArrayList();
			XmlNodeList nodeList = doc_reader.SelectNodes("/actor/face_graph/nodes/node/bones");
			for ( i = 0; i < nodeList.Count; ++i )
			{
				XmlNode bonesNode= nodeList.Item(i); 
				string bonePoseName = bonesNode.ParentNode.Attributes["name"].Value;						
				myBonePoses.Add(bonePoseName);
				
				GameObject facefx_controller_subobject= new GameObject("facefx " + bonePoseName);
				facefx_controller_subobject.transform.parent = facefx_controller.transform;
				
				facefx_controller_subobject.transform.localPosition = new Vector3(0, 0, 0);
				facefx_controller_subobject.transform.localRotation = Quaternion.identity;
				facefx_controller_subobject.transform.localScale = new Vector3(1, 1, 1);			

				AnimClipHelper bonePoseHelper= new AnimClipHelper(new AnimationClip());
				int j = 0;
				for( j = 0; j < bonesNode.ChildNodes.Count; ++j )
				{
					bonePoseHelper.PreAddKeys();
					
					XmlNode boneNode= bonesNode.ChildNodes.Item(j);
					string boneName= boneNode.Attributes["name"].Value;	
					if( ! myRefBoneIndexTable.ContainsKey(boneName) )
					{
						Debug.Log("Warning! Bone not in reference pose! " + boneName );
					}
					else
					{
						int refboneIndex = (int)myRefBoneIndexTable[boneName];
						Transform boneObject = myRefBoneGameObjectTransforms[refboneIndex] as Transform;			
						if( boneObject )
						{
							string bodyString  =  boneNode.InnerText;
							BoneTransform boneTrans = new BoneTransform(bodyString);		

							// Scale bone poses by ScaleFactor
							Vector3 boneTransValues = new Vector3(boneTrans.Values[0], boneTrans.Values[1], boneTrans.Values[2]);
							Vector3 boneTransPos = Vector3.Scale( boneTransValues, new Vector3( _target.ScaleFactor, _target.ScaleFactor, _target.ScaleFactor));
							boneTrans.Values[0] = boneTransPos.x;
							boneTrans.Values[1] = boneTransPos.y;
							boneTrans.Values[2] = boneTransPos.z;
							
							if( _target.UseReferencePoseFromFBX)
							{
								// Calculate the difference between the reference pose in the xml file and the bone pose, then apply the difference to what's in the FBX
								Vector3 pos  = boneTrans.GetPos() - (Vector3)myRefBoneFilePositions[refboneIndex] + boneObject.transform.localPosition;

								Quaternion quat = Quaternion.Inverse((Quaternion)myRefBoneFileRotations[refboneIndex]) *boneTrans.GetRot();
								quat = boneObject.transform.localRotation * quat;
								
								// Probably overkill...I'm not sure if non-uniform scale is even supported in Unity.
								Vector3 fp = boneTrans.GetScale() ;
								Vector3 fr = (Vector3)myRefBoneFileScales[refboneIndex];
								Vector3 gr = boneObject.transform.localScale;
								Vector3 scale = new Vector3( fp.x * gr.x / fr.x,  fp.y * gr.y / fr.y, fp.z * gr.z / fr.z) ;

								//Vector3 scale = boneObject.transform.localScale;
								bonePoseHelper.AddKeys(0 , (BoneTransform)myRefBoneGameObjectBoneTransforms[refboneIndex]);
								bonePoseHelper.AddKeys(1 , new BoneTransform(pos, quat, scale));
								
							}		
							else
							{					
								bonePoseHelper.AddKeys(0 , (BoneTransform)myRefBoneFileTransforms[refboneIndex]);
								bonePoseHelper.AddKeys(1 , (BoneTransform)boneTrans);
							}
							if (boneObject != null)
							{
								string objectRelativePath = GetRelativePath(boneObject);	
								bonePoseHelper.PostAddKeys(objectRelativePath);
							}
							_target.animation.AddClip(bonePoseHelper.animclip, "facefx " + bonePoseName);							
						}
					}
				}
			}
			ImportXMLAnimations(xmltext);
			
			// Create an animation with only the reference pose to play in the background.
			AnimClipHelper loopAnim= new AnimClipHelper(new AnimationClip());
			for ( i = 0; i < refBoneList.Count; ++i )
			{
				if( myRefBoneGameObjectTransforms[i] != null )
				{
					loopAnim.PreAddKeys();
					if( _target.UseReferencePoseFromFBX)
					{
						loopAnim.AddKeys(0, (BoneTransform)myRefBoneGameObjectBoneTransforms[i]);
						loopAnim.AddKeys(1, (BoneTransform)myRefBoneGameObjectBoneTransforms[i]);	
					}
					else
					{
						loopAnim.AddKeys(0, (BoneTransform)myRefBoneFileTransforms[i]);
						loopAnim.AddKeys(1, (BoneTransform)myRefBoneFileTransforms[i]);	
					}
					string objectRelativePath = GetRelativePath((Transform)myRefBoneGameObjectTransforms[i]);
					loopAnim.PostAddKeys(objectRelativePath);
				}
			}
			_target.animation.AddClip(loopAnim.animclip, "facefx_loop_anim");
		
			Debug.Log("Completed importing FaceFX XML file.");		
			_target.InitializeFaceFXController(facefx_controller);
		}
		else
		{
			Debug.Log("Failed to post process XML file!");
		}
	}

	public string GetRelativePath (  Transform obj  ){	
		if ( obj != null )
		{
			string objectRelativePath = obj.name;
			Transform curObject = obj.transform;
			while ( curObject.parent && curObject.parent.gameObject != _target.gameObject )
			{
				objectRelativePath = curObject.parent.name + "/" + objectRelativePath;
				curObject = curObject.parent;
			}    
			if( curObject == null )
			{
				Debug.LogWarning("No relative path exists for unrelated object!");
				return "";
			}	
			return objectRelativePath;
		}
		else
		{
			Debug.LogWarning("No relative path exists for NULL object!");
		}
		return "";
	}
}
