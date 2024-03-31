using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
public class ColorManager : MonoBehaviour
{
    public Button VAT;
    public GameObject Worth4DotTest;
    public GameObject Alignment;
    public GameObject Displacement;
    public Button Crane2D;
    // Start is called before the first frame update
    void Start()
    {
        FetchData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //void FetchData()
    //{
    //    PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
    //    result =>
    //    {
    //        int count = Int32.Parse(result.Data["DiagnosticCount"].Value);
    //        var currentCrane2D = result.Data["Crane2D"].Value;
    //        JObject currentCrane2DJObject = JObject.Parse(currentCrane2D);
    //        string sessionNeeded = "Session" + count.ToString();
    //        Debug.Log("Session needed is "+sessionNeeded);
    //        JObject sessionObject = (JObject)currentCrane2DJObject[sessionNeeded];

    //        string resultFound= (string)sessionObject["result"];
    //        //string resultFound = (string)currentCrane2DJObject[sessionNeeded]["result"];
    //        Debug.Log("Result Found is"+resultFound);
    //    },
    //    error => 
    //    {
    //        Debug.Log("Cant get user Data");
    //    });
    //}
    void FetchData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
            result =>
            {
                
                int count;
                if (!int.TryParse(result.Data["DiagnosticCount"].Value, out count))
                {
                    Debug.Log("Failed to parse DiagnosticCount as int.");
                    return;
                }

                var currentCrane2D = result.Data["Crane2D"].Value;
                JObject currentCrane2DJObject;
                try
                {
                    currentCrane2DJObject = JObject.Parse(currentCrane2D);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse Crane2D as JSON: {ex.Message}");
                    return;
                }
                count = Int32.Parse(result.Data["DiagnosticCount"].Value);
                string sessionNeeded = "Session" + count.ToString();
                Debug.Log("Session needed is " + sessionNeeded);

                if (currentCrane2DJObject[sessionNeeded] == null)
                {
                    Debug.Log($"Session {sessionNeeded} not found in Crane2D data.");
                    return;
                }

                JObject sessionObject = (JObject)currentCrane2DJObject[sessionNeeded];
                string resultFoundX = (string)sessionObject?["x"];
                if (resultFoundX != null)
                {
                    Debug.Log("Result Found is " + resultFoundX);
                    Crane2D.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found in session {sessionNeeded}.");
                }
            },
            error =>
            {
                Debug.Log("Can't get user data");
            });
    }

}
