using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StereoOverlapMode{
    EyesIn,
    EyesOut,
    EyesMixed
};

public enum StereoTestMode{
    VisualJump,
    VisualPower,
    VisualSymbol
};

public class StereogramSettingUI : MonoBehaviour
{
    
    [SerializeField] Slider sliderJumpTime;
    [SerializeField] TextMeshProUGUI textJumpTime;
    [SerializeField] Toggle[] togglesDepth, togglesOverlap, togglesplayTime, togglesTest;

    const string KeyName_Depth = "Stereo_Depth";
    const string KeyName_JumpTime = "Stereo_JumpTime";
    const string KeyName_OverlapMode = "Stereo_OverlapMode";
    const string KeyName_PlayTime = "Stereo_PlayTime";
    const string KeyName_TestMode = "Stereo_TestMode";
    
    // Start is called before the first frame update
    void Start()
    {
        LoadSetting();
    }

    void LoadSetting(){
        SetDepth(PlayerPrefs.GetInt(KeyName_Depth, 1));
        SetJumpTime(PlayerPrefs.GetFloat(KeyName_JumpTime, 5));
        SetOverlapMode((StereoOverlapMode)PlayerPrefs.GetInt(KeyName_OverlapMode, 0));
        SetTestMode((StereoTestMode)PlayerPrefs.GetInt(KeyName_TestMode, 0));
        SetPlayTime(PlayerPrefs.GetFloat(KeyName_PlayTime, 60));
    }

    public void SaveSetting(){
        PlayerPrefs.SetInt(KeyName_Depth, GetDepth());
        PlayerPrefs.SetFloat(KeyName_JumpTime, GetJumpTime());
        PlayerPrefs.SetInt(KeyName_OverlapMode, (int)GetOverlapMode());
        PlayerPrefs.SetInt(KeyName_TestMode, (int)GetTestMode());
        PlayerPrefs.SetFloat(KeyName_PlayTime, GetPlayTime());
    }

    public void OnBtnDecreaseJumpTime(){
        if(sliderJumpTime.value > sliderJumpTime.minValue){
            sliderJumpTime.value--;
        }
    }

    public void OnBtnIncreaseJumpTime(){
        if(sliderJumpTime.value < sliderJumpTime.maxValue){
            sliderJumpTime.value++;
        }
    }

    public void OnJumpTimeSliderChange(float value){
        textJumpTime.text = value.ToString();
    }

    public int GetDepth(){//1, 2, 3
        for(int i = 0; i < togglesDepth.Length; i++){
            if(togglesDepth[i].isOn)
                return i + 1;
        }
        return 1;
    }
    void SetDepth(int i){
        if(i - 1 >= togglesDepth.Length){
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesDepth[i - 1].isOn = true;
    }

    public float GetJumpTime(){
        return sliderJumpTime.value;
    }

    void SetJumpTime(float time){
        sliderJumpTime.value = time;
    }

    public float GetPlayTime(){
        for(int i = 0; i < togglesplayTime.Length; i++){
            if(togglesplayTime[i].isOn)
                return float.Parse(togglesplayTime[i].name);
        }
        return float.Parse(togglesplayTime[1].name);
    }

    void SetPlayTime(float time){
        for(int i = 0; i < togglesplayTime.Length; i++){
            if(togglesplayTime[i].name == ((int)time).ToString()){
                togglesplayTime[i].isOn = true;
                return;
            }
        }
    }
    

    public StereoOverlapMode GetOverlapMode(){
         for(int i = 0; i < togglesOverlap.Length; i++){
            if(togglesOverlap[i].isOn)
                return (StereoOverlapMode)i;
        }
        return StereoOverlapMode.EyesIn;
    }

    void SetOverlapMode(StereoOverlapMode mode){
        togglesOverlap[(int)mode].isOn = true;
    }

    public StereoTestMode GetTestMode(){
         for(int i = 0; i < togglesTest.Length; i++){
            if(togglesTest[i].isOn)
                return (StereoTestMode)i;
        }
        return StereoTestMode.VisualJump;
    }

    void SetTestMode(StereoTestMode mode){
        togglesTest[(int)mode].isOn = true;
    }
}
