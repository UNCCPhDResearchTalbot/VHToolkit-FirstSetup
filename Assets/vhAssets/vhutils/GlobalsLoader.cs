using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GlobalsLoader : MonoBehaviour
{

    public GameObject worker;

    static Object instance;


    void Awake()
    {
        if (!instance)
        {
            Debug.Log("GlobalsLoader.Awake() - Creating Globals gameobject");

            instance = Object.Instantiate(worker);
            Object.DontDestroyOnLoad(instance);
        }
        else
        {
            Debug.Log("GlobalsLoader.Awake() - Globals Worker already instantiated!", worker);
        }

        DestroyImmediate(gameObject);
    }

    void Start()
    {
        //Debug.Log("GlobalsLoader.Start()");
    }
}
