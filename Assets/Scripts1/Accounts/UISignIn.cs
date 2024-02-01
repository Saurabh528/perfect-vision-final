using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
public class UISignIn : MonoBehaviour {

	[SerializeField] TMP_InputField licenseKey;
	[SerializeField] TMP_InputField eMail;
	[SerializeField] TMP_InputField username;
	[SerializeField] TMP_InputField password;
	[SerializeField] GameObject canvas;
	public static bool levelstarted = false;
	private void Start()
	{
#if UNITY_EDITOR_
		levelstarted = true;
		username.text = "Akuete";
		password.text = "123456";
		SignIn();
#endif
		levelstarted = true;
	}


    void ShowErrorMsg (string error) {
		UserAccountManager.Instance.ShowMessage(error);
    }

    void OnSignInSuccess () {
		GameState.username = username.text;
		ChangeScene.LoadScene(ChangeScene.SCENENAME_MODEPANEL);
	}



    public void SignIn () {
		if (string.IsNullOrEmpty(username.text))
			ShowErrorMsg("Doctor name is invalid.");
		else if (string.IsNullOrEmpty(password.text) || password.text.Length < 6)
			ShowErrorMsg("Password is too short.");
		else
		{
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

	public void OnBtnClose()
	{
		Application.Quit();
	}

}