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
    public Button Worth4DotTest;
    public Button Alignment;
    public Button Displacement;
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
                // if (!int.TryParse(result.Data["DiagnosticCount"].Value, out count))
                // {
                //     Debug.Log("Failed to parse DiagnosticCount as int.");
                //     return;
                // }

                var currentCrane2D = result.Data["Crane2D"].Value;
                var currentVAT = result.Data["VAT"].Value;
                var currentWorth4Dot = result.Data["Worth4Dot"].Value;
                JObject currentCrane2DJObject, currentVATJObject,currentWorth4DotJObject;
                try
                {
                    currentCrane2DJObject = JObject.Parse(currentCrane2D);
                    
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse Crane2D as JSON: {ex.Message}");
                    return;
                }

                try
                {
                    currentVATJObject = JObject.Parse(currentVAT);
                    
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse VAT as JSON: {ex.Message}");
                    return;
                }
                try
                {
                    currentWorth4DotJObject = JObject.Parse(currentWorth4Dot);

                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse Worth4Dot test as JSON: {ex.Message}");
                    return;
                }
                count = Int32.Parse(result.Data["DiagnosticCount"].Value);
                string sessionNeeded = "Session" + count.ToString();
                Debug.Log("Session needed is " + sessionNeeded);

                //if (currentCrane2DJObject[sessionNeeded] == null)
                //{
                //    Debug.Log($"Session {sessionNeeded} not found in Crane2D data.");
                //    return;
                //}

                JObject sessionObjectCrane2D = (JObject)currentCrane2DJObject[sessionNeeded];
                JObject sessionObjectVAT = (JObject)currentVATJObject[sessionNeeded];
                JObject sessionWorth4Dot = (JObject)currentWorth4DotJObject[sessionNeeded];
                string resultFoundCrane2D = (string)sessionObjectCrane2D?["x"];
                string resultFoundVAT = (string)sessionObjectVAT?["LeftScore"];
                string resultFoundWorth4Dot = (string)sessionWorth4Dot?["Diagnosis"];
                if (resultFoundCrane2D != null)
                {
                    Debug.Log("Result Found is " + resultFoundCrane2D);
                    Crane2D.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found for {sessionNeeded}.");
                }

                if (resultFoundVAT != null)
                {
                    Debug.Log("Result Found is " + resultFoundVAT);
                    VAT.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found for {sessionNeeded}.");
                }
                if (resultFoundWorth4Dot != null)
                {
                    Debug.Log("Result Found is " + resultFoundWorth4Dot);
                    Worth4DotTest.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found for {sessionNeeded}.");
                }
                // Add Green logic for 3 games.


            },
            error =>
            {
                Debug.Log("Can't get user data");
            });
    }

}
