using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeMgr : MonoBehaviour
{
    [SerializeField] GlobalSettingUI SettingUI;
    [SerializeField] GameObject btnEnrollment, btnHomeTherapy;
    // Start is called before the first frame update
    void Start()
    {
		SettingUI.LoadAudioSetting();
		GameState.currentGamePlay = null;
		GameState.currentPatient = null;
        VisualFactor.LoadFactor();
        btnEnrollment.SetActive(GameState.IsDoctor());
        btnHomeTherapy.SetActive(GameState.IsPatient());
    }

    public void OnBtnQuit(){
        Application.Quit();
    }
}
