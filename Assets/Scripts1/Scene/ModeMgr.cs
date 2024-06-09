using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ModeMgr : MonoBehaviour
{
    const bool  onlyDiagnose = false;
    [SerializeField] GlobalSettingUI SettingUI;
    [SerializeField] GameObject btnEnrollment, btnHomeTherapy, btnGamePlay;
    // Start is called before the first frame update
    void Start()
    {
		SettingUI.LoadAudioSetting();
		GameState.currentGamePlay = null;
        VisualFactor.LoadFactor();
        btnEnrollment.SetActive(!onlyDiagnose && GameState.IsDoctor());
        btnHomeTherapy.SetActive(!onlyDiagnose && GameState.IsPatient());
        btnGamePlay.SetActive(!onlyDiagnose && GameState.IsDoctor());
    }


    //This is the function which is used for logout
    public void OnBtnQuit()
    {
        Application.Quit();
    }

    public void StartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}
