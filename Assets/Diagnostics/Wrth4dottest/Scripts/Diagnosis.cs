using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json.Linq;

public class Diagnosis : DiagnosticController
{
    public Questionset questionset;
    public TextMeshProUGUI Question;
    public TextMeshProUGUI diagnosis;

    public int CurrentQuestionIndex = 0;

    public GameObject buttonPrefab;
    public GameObject Quesoptions;
    [SerializeField] GameObject _image4CM, _image6M;

    int buttonnum=0;

    string finaldiagnosis, diagnosislights;
    string[] resultText;

    private void Start()
    {
        LoadQuestion();
        resultText = new string[questionset.question.Count];
    }

    private void LoadQuestion()
    {
        //Get the question from class
        Question.text = questionset.question[CurrentQuestionIndex].question;


        //Instantiate button according to class 
        foreach (var item in questionset.question[CurrentQuestionIndex].options)
        {
            GameObject button = (GameObject)Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            button.SetActive(true);
            button.name = buttonnum.ToString();
			button.GetComponent<Button>().onClick.AddListener(() => OnClick(Convert.ToInt32(button.name)));

            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item;

            buttonnum++;
        }
    }

    void Nextquestion()
    {
        UtilityFunc.DeleteAllSideTransforms(buttonPrefab.transform, false);

        buttonnum = 0;

        LoadQuestion();
    }

    void OnClick(int questionIndex)
    {
        //Debug.Log(CurrentQuestionIndex);

        string tempdiagnosis = questionset.question[CurrentQuestionIndex].Diagonis[questionIndex];
        if(tempdiagnosis != "1")
            resultText[CurrentQuestionIndex] = tempdiagnosis;
        else
            resultText[CurrentQuestionIndex] = "";
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
            finaldiagnosis = tempdiagnosis;
            CurrentQuestionIndex = 5;
            Nextquestion();
        }


    }

    public void OnTriggerPattern4CM(bool value)
    {
        _image4CM.SetActive(value);
        _image6M.SetActive(!value);
	}

    public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        string result = resultText[resultText.Length - 1];
        if(result == null)
            result = "";
        dti.AddValue(result);
        pr.AddDiagnosRecord("Worth 4 Dot Test", dti) ;
    }

    public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return !string.IsNullOrEmpty(diagnosis.text);
    }
}
