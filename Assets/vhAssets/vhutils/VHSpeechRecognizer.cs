using UnityEngine;
using System;
using System.Collections;

public class VHSpeechRecognizer : MonoBehaviour
{
    public GUIText m_SpeechRecognition;
    public GUIText m_TTS;
    public DebugConsole m_Console;
    //public override void  VHStart()
    void Start()
    {
        return;
#if false
        // TEST 2
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        // jni.FindClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        //currentActivity.Call("Launch");

        // jni.GetStaticFieldID(classID, "Ljava/lang/Object;");
        // jni.GetStaticObjectField(classID, fieldID);
        // jni.FindClass("java.lang.Object");

        //Debug.Log(currentActivity.Call<AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath"));
        // jni.GetMethodID(classID, "getCacheDir", "()Ljava/io/File;"); // or any baseclass thereof!
        // jni.CallObjectMethod(objectID, methodID);
        // jni.FindClass("java.io.File");
        // jni.GetMethodID(classID, "getCanonicalPath", "()Ljava/lang/String;");
        // jni.CallObjectMethod(objectID, methodID);
        // jni.GetStringUTFChars(javaString);

        AndroidJavaClass recognizerFactory = new AndroidJavaClass("android.speech.SpeechRecognizer");
        if (recognizerFactory == null)
        {
            Debug.LogError("recognizerFactory is null");
        }

        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.speech.action.RECOGNIZE_SPEECH");
        if (intent == null)
        {
            Debug.LogError("intent is null");
        }
        else
        {
            //string ACTION_RECOGNIZE_SPEECH = intent.GetStatic<string>("ACTION_RECOGNIZE_SPEECH");
            //intent = new AndroidJavaObject("android.content.Intent", ACTION_RECOGNIZE_SPEECH);
            //string ACTION_RECOGNIZE_SPEECH = intent.Get<string>("ACTION_RECOGNIZE_SPEECH");
            //string ACTION_RECOGNIZE_SPEECH = intent.Get<string>("ACTION_RECOGNIZE_SPEECH");
            intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE_MODEL", "free_form");
            intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.PROMPT", "Please speak slowly and enunciate clearly.");
            //startActivityForResult(intent, VOICE_RECOGNITION_REQUEST);
        }

        currentActivity.Call("startActivityForResult", intent, 65793);
        return;

        AndroidJavaObject listener = new AndroidJavaObject("android.speech.RecognitionListener");
        if (listener == null)
        {
            Debug.LogError("listener is null ");
        }

        AndroidJavaObject recognizer = recognizerFactory.CallStatic<AndroidJavaObject>("createSpeechRecognizer",
            currentActivity.Call<AndroidJavaObject>("getApplicationContext"));
        if (recognizer == null)
        {
            Debug.LogError("Factory created recognizer is null ");
        }
        else
        {
            recognizer.Call("setRecognitionListener", listener);
            recognizer.Call("startListening", intent);
        }

        Debug.Log("Finished function");
#endif
    }

    //public override void VHOnGUI()
    void OnGUI()
    {
#if false
        if (GUI.Button(new Rect(100, 100, 130, 40), "Speech Recognition"))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("LaunchSpeechRecognition");
        }
        else if (GUI.Button(new Rect(100, 175, 130, 40), "Text To Speech") && !string.IsNullOrEmpty(m_Console.CommandString))
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("UseTTS", m_Console.CommandString);
            OnTTS(m_Console.CommandString);
        }
#endif
    }

    public void OnSpeechRecognition(string message)
    {
        m_SpeechRecognition.text = message;
    }

    public void OnTTS(string message)
    {
        m_TTS.text = message;
    }


    /*public void speakToMe(View view)
    {
        Intent intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL,
            RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_PROMPT,
            "Please speak slowly and enunciate clearly.");
        startActivityForResult(intent, VOICE_RECOGNITION_REQUEST);
    }

    protected void onActivityResult(int requestCode, int resultCode, Intent data)
    {
        if (requestCode == VOICE_RECOGNITION_REQUEST && resultCode == RESULT_OK)
        {
            ArrayList matches = data
            .getStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS);
            TextView textView = (TextView) findViewById(R.id.speech_io_text);
            String firstMatch = matches.get(0);
            textView.setText(firstMatch);
        }
    }*/

    // TEST 1
        //The comments is what you would need to do if you use raw JNI
//AndroidJavaObject jo = new AndroidJavaObject("java.lang.String", "some_string");
        // jni.FindClass("java.lang.String");
        // jni.GetMethodID(classID, "<init>", "(Ljava/lang/String;)V");
        // jni.NewStringUTF("some_string");
        // jni.NewObject(classID, methodID, javaString);
//int hash = jo.Call<int>("hashCode");
//Debug.Log("String hash: " + hash);
        // jni.GetMethodID(classID, "hashCode", "()I");
        // jni.CallIntMethod(objectID, methodID);



}
