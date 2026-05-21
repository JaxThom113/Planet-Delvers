using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
    public static T Instance { get; private set; } 

    protected virtual void Awake() 
    {  
        // check if an instance already exists, if so, destroy the new instance
        if (Instance != null)
        {
            Destroy(gameObject); 
            return;
        }
        
        // if no instance exists, set the instance to the current instance
        Instance = this as T;
    } 

    protected virtual void OnApplicationQuit()
    {
        Instance = null; 
        Destroy(gameObject);
    }
} 
