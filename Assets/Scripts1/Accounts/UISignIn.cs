using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
public class UISignIn : MonoBehaviour {

	[SerializeField] TMP_InputField eMail;
	[SerializeField] TMP_InputField username;
	[SerializeField] TMP_InputField password;
	[SerializeField] GameObject canvas;
	public static bool levelstarted = false;

	
	private void Start()
	{
		LoadingCanvas.Hide();
#if UNITY_EDITOR
		levelstarted = true;
		username.text = "Akuete";
		password.text = "123456";
		SignIn();
#endif
		levelstarted = true;
	}


    void ShowErrorMsg (string error) {
		LoadingCanvas.Hide();
		UserAccountManager.Instance.ShowMessage(error);
    }

    void OnSignInSuccess () {
		LoadingCanvas.Hide();
		GameState.username = username.text;
		if(GameState.IsDoctor())
		{
            ChangeScene.LoadScene(ChangeScene.SCENENAME_MODEPANEL);
        }
		else
		{
			PatientDataManager.GetPatientDataByName(GameState.username, OnLoadHomePatientDataSuccess, ShowErrorMsg);
            
        }
		
	}

	void OnLoadHomePatientDataSuccess(PatientData pdata)
    {
        PatientMgr.SetPatientList(new Dictionary<string, PatientData>(){{pdata.name, pdata}});
        GameState.currentPatient = pdata;
		ChangeScene.LoadScene(ChangeScene.SCENENAME_MODEPANEL);
    }



    public void SignIn () {
		if (string.IsNullOrEmpty(username.text))
			ShowErrorMsg("Doctor name is invalid.");
		else if (string.IsNullOrEmpty(password.text) || password.text.Length < 6)
			ShowErrorMsg("Password is too short.");
		else
		{
			LoadingCanvas.Show();
			UserAccountManager.Instance.ShowMessage("");
			UserAccountManager.Instance.SignIn(username.text, password.text, OnSignInSuccess, ShowErrorMsg);
		}
	}

	public static void StartFromSignInDebugMode()
	{
#if UNITY_EDITOR
		if (levelstarted == false)
		{
			ChangeScene.LoadScene("StartScene");
			return;
		}
#endif
	}
	public void SendEmail()
	{
		string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        if( Regex.IsMatch(eMail.text, pattern)) UserAccountManager.Instance.emailRecovery(eMail.text);
		else if(eMail.text == "") 
		{
			Debug.Log("Empty");
			UserAccountManager.Instance.emailRecovery(eMail.text);
		}
	}

	public void OnBtnClose()
	{
		Application.Quit();
	}

}