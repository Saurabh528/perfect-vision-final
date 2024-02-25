using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class SignInManager : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField email;
    public TMP_InputField password;
    // public string email = "streetcodec@gmail.com";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
//    public void Login()
//     {
//         if(IsValidEmail(email.text))
//         {
//            LoginEmail();
//            return; 
//         }
//         PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
// 		{
// 			TitleId = "CD663",
// 			Username = username.text,
// 			Password = password.text
// 		}, result =>
// 		{
// 			Debug.Log("Successful Login with username and password");
//             SceneManager.LoadScene("ModePanel");
			
// 		}, error =>
// 		{
// 			Debug.Log($"<color=red>Unsuccessful Login with username and password</color>");
// 			Debug.Log(error.ErrorMessage);
// 		});

        
//     }
//    public void LoginEmail()
//    {
//     PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest()
// 		{
//             TitleId = "CD663",
// 			Email = email.text,
// 			Password = password.text
// 		}, result =>
// 		{
// 			Debug.Log("Successful login with email and password");
// 			 SceneManager.LoadScene("ModePanel");
// 		}, error =>
// 		{
// 			Debug.Log($"<color=red>Unsuccessful Login with email and password</color>");
// 			Debug.Log(error.ErrorMessage);
// 		});
//    }

    public void register()
    {
        if(!IsValidEmail(email.text))
        {
            Debug.Log("You entered wrong email");
            return;
        }
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
		{
            TitleId = "CD663",
            Username = username.text,
            Email = email.text,
            Password = password.text,
            RequireBothUsernameAndEmail = true
		}, result =>
		{
			Debug.Log("Registered!");
			
		}, error =>
		{
			Debug.Log($"Not Registered");
			Debug.Log(error.ErrorMessage);
		});
    }
    


    public void emailRecovery()
    {
        if(!IsValidEmail(email.text))
        {
            Debug.Log("You entered wrong email");
            return;
        }
        PlayFabClientAPI.SendAccountRecoveryEmail(new SendAccountRecoveryEmailRequest()
		{
            Email = email.text,
            TitleId = "CD663"
		}, result =>
		{
			Debug.Log("Email sent!");
			
		}, error =>
		{
			Debug.Log($"Not sent");
			Debug.Log(error.ErrorMessage);
		});
        
    }
    bool IsValidEmail(string email)
    {
        string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        return Regex.IsMatch(email, pattern);
    }
}
