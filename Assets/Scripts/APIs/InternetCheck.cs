using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InternetCheck : MonoBehaviour
{
    private bool isRunning = false;
    [SerializeField] public bool isConnect;
    private IEnumerator CheckConnection(string url)
    {
        isRunning = true;
            WWW www = new WWW(url); // Find A
        float elapsedTime = 0.0f;

        while (!www.isDone)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= 10.0f && www.progress <= 0.5) break;
            yield return null;
        }

        if (!www.isDone || !string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("Load Failed");
            isRunning = false;
            isConnect = false;
            yield break;
        }
        isConnect = true;
        isRunning = false;
    }
 

    void Start()
    {
        isConnect = false;
    }
    void Update()
    {
        if (!isRunning)
        {
            StartCoroutine(CheckConnection("google.com"));
        }
    }

    
}
