using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class testConnection : MonoBehaviour
{
    public static bool networkConnected = false; //bool var. that gives whether there is internet connection

    public GameObject conn_text;

    // Start is called before the first frame update
    void Awake()
    {
        /*conn_text = GameObject.Find("Connection");
        //StartCoroutine(checkInternetConnection((isConnected) =>
        //{
            WWW www = new WWW("http://google.com");
         //    System.Threading.Thread.Sleep(10);
        // yield return www;
            if (www.error != null)
            {
                networkConnected = false;
            }
            else
            {
                networkConnected = true;
            }

            if (networkConnected)
            {
                Debug.Log("connected");
                conn_text.SetActive(false);
               // networkConnected = true;
            }
            else
            {
                Debug.Log("not connected");
                conn_text.SetActive(true);
                //networkConnected = false;
            }
       // }));*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    IEnumerator checkInternetConnection(Action<bool> action) {
        WWW www = new WWW("http://google.com");
        yield return www;
        if (www.error != null)
        {
            action(false);
        }
        else {
            action(true);
        }
    }*/
}
