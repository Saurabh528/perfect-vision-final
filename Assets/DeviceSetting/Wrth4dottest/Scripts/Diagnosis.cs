using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json.Linq;

public class Diagnosis : MonoBehaviour
{
    public Questionset questionset;
    public TextMeshProUGUI Question;
    public TextMeshProUGUI diagnosis;

    public int CurrentQuestionIndex = 0;

    public GameObject buttonPrefab;
    public GameObject panelToAttachButtonsTo;
    public GameObject Quesoptions;
    [SerializeField] GameObject _image4CM, _image6M;

    int buttonnum=0;

    string finaldiagnosis, diagnosislights;

    private void Start()
    {
        LoadQuestion();
    }

    private void LoadQuestion()
    {
        //Get the question from class
        Question.text = questionset.question[CurrentQuestionIndex].question;


        //Instantiate button according to class 
        foreach (var item in questionset.question[CurrentQuestionIndex].options)
        {
            GameObject button = (GameObject)Instantiate(buttonPrefab);
            button.name = buttonnum.ToString();
            //Setting button parent
            button.transform.SetParent(panelToAttachButtonsTo.transform);
			button.transform.localScale = Vector3.one;
			//Setting what button does when clicked
			button.GetComponent<Button>().onClick.AddListener(() => OnClick(Convert.ToInt32(button.name)));

            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item;

            buttonnum++;
        }
    }

    void Nextquestion()
    {
        for(int i = 0; i < panelToAttachButtonsTo.transform.childCount; i++)
        {
            Destroy(panelToAttachButtonsTo.transform.GetChild(i).gameObject);
        }

        buttonnum = 0;

        LoadQuestion();
    }

    void OnClick(int questionIndex)
    {
        //Debug.Log(CurrentQuestionIndex);

        string tempdiagnosis = questionset.question[CurrentQuestionIndex].Diagonis[questionIndex];

        if(CurrentQuestionIndex == 5)
        {
            Quesoptions.SetActive(false);
            diagnosislights = tempdiagnosis;
            diagnosis.text = finaldiagnosis + " And " + diagnosislights;
        }
       
        else if(CurrentQuestionIndex > 1 && CurrentQuestionIndex < 5)
        {
            if(questionset.question[CurrentQuestionIndex].options[questionIndex] == "None")
            {
                Debug.Log(tempdiagnosis);
                if(CurrentQuestionIndex == 4)
                {
                    //diagnosis.text = "Esotropia (ET) or Exotropia (XT) or Hypotropia or Hypertropia";
                    finaldiagnosis = "Esotropia (ET) or Exotropia (XT) or Hypotropia or Hypertropia";
                    CurrentQuestionIndex++;
                    Nextquestion();
                }
                else
                {
                    CurrentQuestionIndex = CurrentQuestionIndex + Convert.ToInt32(tempdiagnosis);
                    Nextquestion();
                }
                
            }
            else
            {
                //diagnosis.text = tempdiagnosis;
                finaldiagnosis = tempdiagnosis;
                if(tempdiagnosis != "1")
                {
                    CurrentQuestionIndex = 5;
                    Nextquestion();
                }
            }
        }

        else if ((CurrentQuestionIndex == 0 && questionIndex == 1) || CurrentQuestionIndex == 1)
        {
            //Debug.Log(tempdiagnosis);
            CurrentQuestionIndex = CurrentQuestionIndex + Convert.ToInt32(tempdiagnosis);
            Nextquestion();

        }

        else 
        {
            //diagnosis.text = tempdiagnosis;
            finaldiagnosis = tempdiagnosis;
            CurrentQuestionIndex = 5;
            Nextquestion();
        }

        //SaveData(tempdiagnosis);

    }

    public void OnTriggerPattern4CM(bool value)
    {
        _image4CM.SetActive(value);
        _image6M.SetActive(!value);
	}

    void SaveData(string finalAns)
    {
        Debug.Log("Save Data called");
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
            result =>
            {
                var prevJson = result.Data["Worth4Dot"].Value;
                int count = Int32.Parse(result.Data["DiagnosticCount"].Value);

                DateTime now = DateTime.Now;
                string dateCurrent = now.ToShortDateString();

                UnityEngine.Debug.Log("DiagnosticCount VARIABLE IS" + count);
                JObject prevJObject = JObject.Parse(prevJson);
                JObject newSessionData = new JObject();
                newSessionData["Diagnosis"] = finalAns;
                newSessionData["Date"] = dateCurrent;

                string sessions = "Session" + count.ToString();
                prevJObject[sessions] = newSessionData;
                string updatedJson = prevJObject.ToString(Newtonsoft.Json.Formatting.Indented);

                var request = new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string> { { "Worth4Dot", updatedJson } },
                    Permission = UserDataPermission.Public
                };
                PlayFabClientAPI.UpdateUserData(request,
                 result =>
                 {
                     UnityEngine.Debug.Log("Successfully added Worth4Dot data");
                 },
                 error =>
                 {
                     UnityEngine.Debug.Log("Not added Worth4Dot data");

                 });
            },// Success callback
            error =>
            {
                UnityEngine.Debug.Log("Worth4Dot data GetUserData api called error");

            });// Error callback
    }

}
