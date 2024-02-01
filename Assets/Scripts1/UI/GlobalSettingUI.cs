using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GlobalSettingUI : MonoBehaviour
{
    const string PREFNAME_MUSICVOLUME = "MusicVolume";
    const string PREFNAME_MUSICSFX = "MusicSFX";
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] GameObject _btnSFXOn, _btnSFXOff, _hintMark;
    [SerializeField] TMP_Dropdown _ddCamera;
    float _camRefreshtime;
    List<string> _camList = new List<string>();
    

    public void LoadAudioSetting(){
        float volume = PlayerPrefs.GetFloat(PREFNAME_MUSICVOLUME, 0.5f);
        SetAudioVolume(volume);

        string sfxvolume = PlayerPrefs.GetString(PREFNAME_MUSICSFX, "true");
        SetAudioSFX(sfxvolume);
    }
    void OnEnable()
    {
         float volume = PlayerPrefs.GetFloat(PREFNAME_MUSICVOLUME, 0.5f);
        musicSlider.value = volume;

        string sfxvolume = PlayerPrefs.GetString(PREFNAME_MUSICSFX, "true");
        _btnSFXOff.SetActive(sfxvolume == "true");
        _btnSFXOn.SetActive(sfxvolume != "true");

        bool showMark = true;
        string[] gameSceneNames = SessionMgr.GetGameSceneNames();
        foreach(string gameScenename in gameSceneNames){
            string strdonthint = PlayerPrefs.GetString(TheraphHelpController.PREFNAME_DONTSHOWHINT + gameScenename, "true");
            if(strdonthint == "true"){
                showMark = false;
                break;
            }
        }
        _hintMark.SetActive(showMark);
    }

    // Update is called once per frame
    void Update()
    {
        _camRefreshtime -= Time.deltaTime;
        if(_camRefreshtime < 0){
            _camRefreshtime += 1;
            RefreshCamera();
        }
    }

    void RefreshCamera(){
        WebCamDevice[] devices = WebCamTexture.devices;
        bool changed = false;
        if(devices.Length != _camList.Count)
            changed = true;
        else{
            for(int i = 0; i < devices.Length; i++){
                if(devices[i].name != _camList[i]){
                    changed = true;
                    break;
                }
            }
        }
        if(!changed)
            return;
        _ddCamera.options.Clear();
        _camList.Clear();
        int value = -1;
        string curname = PlayerPrefs.GetString(DataKey.WBCAMERANAME);
        for(int i = 0; i < devices.Length; i++){
            string name = devices[i].name;
            _camList.Add(name);
            _ddCamera.options.Add(new TMP_Dropdown.OptionData(name));
            if(curname == name)
                value = i;
        }
        _ddCamera.value = value == -1? 0: value;
        _ddCamera.RefreshShownValue();
    }

    public static int GetCurrentCameraIndex(){
        WebCamDevice[] devices = WebCamTexture.devices;
        if(devices.Length == 0)
            return -1;
        string curname = PlayerPrefs.GetString(DataKey.WBCAMERANAME);
        for(int i = 0; i < devices.Length; i++){
            if(devices[i].name == curname)
                return i;
        }
        return 0;
    }

    public void OnCameraChanged(int value){
        if(value == -1)
            return;
        PlayerPrefs.SetString(DataKey.WBCAMERANAME, _ddCamera.options[value].text);
    }

    public void SetAudioSFX(string value){
        value = value.ToLower();
        float fvalue = (value == "true")? 1: 0.0001f;
        audioMixer.SetFloat("sfx", Mathf.Log10(fvalue) * 20);
        PlayerPrefs.SetString(PREFNAME_MUSICSFX, value);
    }

    public void ResetAllGameManual(){
        string[] gameSceneNames = SessionMgr.GetGameSceneNames();
        foreach(string gameScenename in gameSceneNames){
            PlayerPrefs.SetString(TheraphHelpController.PREFNAME_DONTSHOWHINT + gameScenename, "false");
        }
    }

    public void SetAudioVolume(float value){
        audioMixer.SetFloat("music", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(PREFNAME_MUSICVOLUME, value);
    }


}
