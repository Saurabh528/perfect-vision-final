using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class UICreateAccount : MonoBehaviour {

    [SerializeField] TextMeshProUGUI errorText;
    [SerializeField] GameObject canvas;

	[SerializeField] TMP_InputField username;
	[SerializeField] TMP_InputField password;
	[SerializeField] TMP_InputField password_repeat;
	[SerializeField] TMP_InputField emailAddress;
	[SerializeField] TMP_InputField licenseKey;
	[SerializeField] GameObject objSignUp;
	[SerializeField] GameObject objCreateUserAccount;
	

	private void OnEnable()
	{
		objSignUp.SetActive(true);
		objCreateUserAccount.SetActive(false);
	}

	

	void ShowErrorMessage (string error) {
		UserAccountManager.Instance.ShowMessage(error);
	}

    void OnSignWithLicenseKeySuccess () {
		objSignUp.SetActive(false);
		objCreateUserAccount.SetActive(true);
	}

	void OnSetUsernamePasswordSuccess()
	{
		if(GameState.IsPatient())
			ChangeScene.LoadScene("ColorScreen");
		else
			ChangeScene.LoadScene(ChangeScene.SCENENAME_MODEPANEL);
	}

	public void OnBtnCreateAccount()
	{
		if (string.IsNullOrEmpty(emailAddress.text))
			ShowErrorMessage("Email Address is invalid.");
		else if (string.IsNullOrEmpty(username.text))
			ShowErrorMessage("Name is invalid.");
		else if (string.IsNullOrEmpty(password.text) || password.text.Length < 6)
			ShowErrorMessage("Password is too short.");
		else if(password.text != password_repeat.text)
			ShowErrorMessage("Password does not match.");
		else
		{
			ShowErrorMessage("");
			UserAccountManager.Instance.SetUserNameAndPassword(emailAddress.text, username.text, password.text, OnSetUsernamePasswordSuccess, ShowErrorMessage);
		}
	}



    public void SignWithLicenseKey () {
		if (string.IsNullOrEmpty(licenseKey.text))
			ShowErrorMessage("License key is invalid.");
		else
		{
			ShowErrorMessage("");
			UserAccountManager.Instance.SignUp(licenseKey.text, OnSignWithLicenseKeySuccess, ShowErrorMessage);
		}
		
    }

}