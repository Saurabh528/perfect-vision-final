using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ModeMgr : MonoBehaviour
{
    [SerializeField] GlobalSettingUI SettingUI;
    [SerializeField] GameObject btnEnrollment, btnHomeTherapy, btnGamePlay;
    // Start is called before the first frame update
    void Start()
    {
		SettingUI.LoadAudioSetting();
		GameState.currentGamePlay = null;
        VisualFactor.LoadFactor();
        btnEnrollment.SetActive(GameState.IsDoctor());
        btnHomeTherapy.SetActive(!GameConst.MODE_DOCTORTEST && GameState.IsPatient());
        btnGamePlay.SetActive(!GameConst.MODE_DOCTORTEST && GameState.IsDoctor());
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
