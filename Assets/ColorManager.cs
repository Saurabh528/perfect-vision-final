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
                // We get the recent most session : 
                count = Int32.Parse(result.Data["DiagnosticCount"].Value);
                string sessionNeeded = "Session" + count.ToString();
                Debug.Log("Session needed is " + sessionNeeded);

                var currentCrane2D = result.Data["Crane2D"].Value;
                var currentVAT = result.Data["VAT"].Value;
                var currentWorth4Dot = result.Data["Worth4Dot"].Value;
                var currentAlignment = result.Data["Worth4Dot"].Value;
                var currentDisplacement = result.Data["Worth4Dot"].Value;
                JObject currentCrane2DJObject, currentVATJObject,currentWorth4DotJObject,currentAlignmentJObject,currentDisplacementJObject;
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
                try
                {
                    currentAlignmentJObject = JObject.Parse(currentAlignment);
                    
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse Alignment as JSON: {ex.Message}");
                    return;
                }
                try
                {
                    currentDisplacementJObject = JObject.Parse(currentDisplacement);
                    
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to parse Displacement JSON: {ex.Message}");
                    return;
                }
                

                //if (currentCrane2DJObject[sessionNeeded] == null)
                //{
                //    Debug.Log($"Session {sessionNeeded} not found in Crane2D data.");
                //    return;
                //}

                JObject sessionObjectCrane2D = (JObject)currentCrane2DJObject[sessionNeeded];
                JObject sessionObjectVAT = (JObject)currentVATJObject[sessionNeeded];
                JObject sessionWorth4Dot = (JObject)currentWorth4DotJObject[sessionNeeded];
                JObject sessionAlignment = (JObject)currentAlignmentJObject[sessionNeeded];
                JObject sessionDisplacement = (JObject)currentDisplacementJObject[sessionNeeded];
                string resultFoundCrane2D = (string)sessionObjectCrane2D?["x"];
                string resultFoundVAT = (string)sessionObjectVAT?["LeftScore"];
                string resultFoundWorth4Dot = (string)sessionWorth4Dot?["Diagnosis"];
                string resultFoundAlignment = (string)sessionAlignment?["Placeholder"];
                string resultFoundDisplacement = (string)sessionDisplacement?["Placeholder"];
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
                if (resultFoundAlignment != null)
                {
                    Debug.Log("Result Found is " + resultFoundAlignment);
                    Alignment.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found for {sessionNeeded}.");
                }
                if (resultFoundDisplacement != null)
                {
                    Debug.Log("Result Found is " + resultFoundDisplacement);
                    Displacement.GetComponent<Image>().color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    Debug.Log($"Result not found for {sessionNeeded}.");
                }


            },
            error =>
            {
                Debug.Log("Can't get user data");
            });
    }

}
