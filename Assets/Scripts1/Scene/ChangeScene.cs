using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {
	public const string SCENENAME_GAMEPANEL = "GamePanel";
	public const string SCENENAME_MODEPANEL = "ModePanel";
	//After Color Calibiration directly move to DPI scene
	private void Start()
	{

#if UNITY_EDITOR
		UISignIn.StartFromSignInDebugMode();
#endif
	}
	public void SceneChanger(int sceneName)
    {

        SceneManager.LoadScene(sceneName);
    }

	public static void LoadScene(string scenename)
	{
        SceneManager.LoadScene(scenename);
    }
	
	public static void RestartScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public static void QuitApplication()
	{
		Application.Quit();
	}
}
