using System.Collections;
using System.Collections.Generic;
using System.Text;
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

public enum DepthMode{
    Depth10,
    Depth20,
    Depth30,
    DepthIncrease,
    DepthCustom
};

public enum SizeMode{
    Normal,
    Small
};

public enum LevelMode{
    Level1,
    Level2
};

public enum ZDepth{
    Depth1,
    Depth2,
    Depth3
}

public enum TimeMode{
    Timed,
    MaxDistance
}

public class StereogramSettingUI : MonoBehaviour
{
    
    [SerializeField] Slider sliderJumpTime, sliderCustomEyesin;
    [SerializeField] TextMeshProUGUI textJumpTime, textCustomEyesIn;
    [SerializeField] Toggle[] togglesDepth, togglesOverlap, togglesplayTime, togglesTest, togglesSize, togglesLevel, togglesZDepth, togglesTimeMode;
    [SerializeField] GameObject ZDepthGroup, TimeModeGroup, LevelGroup, TimeGroup;

    const string KeyName_Depth = "Stereo_Depth";
    const string KeyName_CustomEyesin = "Stereo_CustomEyesIn";
    const string KeyName_JumpTime = "Stereo_JumpTime";
    const string KeyName_OverlapMode = "Stereo_OverlapMode";
    const string KeyName_PlayTime = "Stereo_PlayTime";
    const string KeyName_TestMode = "Stereo_TestMode";
    const string KeyName_SizeMode = "Stereo_SizeMode";
    const string KeyName_LevelMode = "Stereo_LevelMode";
    const string KeyName_TimeMode = "Stereo_TimeMode";
    const string KeyName_ZDepth = "Stereo_ZDepth";
    
    // Start is called before the first frame update
    void Start()
    {
        LoadSetting();
    }

    void LoadSetting(){
        SetDepthMode((DepthMode)PlayerPrefs.GetInt(KeyName_Depth, 0));
        SetCustomEyesIn(PlayerPrefs.GetInt(KeyName_CustomEyesin, 10));
        SetJumpTime(PlayerPrefs.GetFloat(KeyName_JumpTime, 5));
        SetOverlapMode((StereoOverlapMode)PlayerPrefs.GetInt(KeyName_OverlapMode, 0));
        SetSizeMode((SizeMode)PlayerPrefs.GetInt(KeyName_SizeMode, 0));
        SetLevelMode((LevelMode)PlayerPrefs.GetInt(KeyName_LevelMode, 0));
        SetZDepthMode((ZDepth)PlayerPrefs.GetInt(KeyName_ZDepth, 0));
        SetTimeMode((TimeMode)PlayerPrefs.GetInt(KeyName_TimeMode, 0));
        SetTestMode((StereoTestMode)PlayerPrefs.GetInt(KeyName_TestMode, 0));
        SetPlayTime(PlayerPrefs.GetFloat(KeyName_PlayTime, 60));
    }

    public void SaveSetting(){
        PlayerPrefs.SetInt(KeyName_Depth, (int)GetDepthMode());
        PlayerPrefs.SetInt(KeyName_CustomEyesin, GetCustomEyesIn());
        PlayerPrefs.SetFloat(KeyName_JumpTime, GetJumpTime());
        PlayerPrefs.SetInt(KeyName_OverlapMode, (int)GetOverlapMode());
        PlayerPrefs.SetInt(KeyName_SizeMode, (int)GetSizeMode());
        PlayerPrefs.SetInt(KeyName_LevelMode, (int)GetLevelMode());
        PlayerPrefs.SetInt(KeyName_ZDepth, (int)GetZDepthMode());
        PlayerPrefs.SetInt(KeyName_TimeMode, (int)GetTimeMode());
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

    public void OnBtnDecreaseCustomEyesIn(){
        if(sliderCustomEyesin.value > sliderCustomEyesin.minValue){
            sliderCustomEyesin.value--;
        }
    }

    public void OnBtnIncreaseCustomEyesIn(){
        if(sliderCustomEyesin.value < sliderCustomEyesin.maxValue){
            sliderCustomEyesin.value++;
        }
    }

    public void OnCustomEyesInSliderChange(float value){
        textCustomEyesIn.text = value.ToString();
    }

    public DepthMode GetDepthMode(){
        for(int i = 0; i < togglesDepth.Length; i++){
            if(togglesDepth[i].isOn)
                return (DepthMode)i;
        }
        return DepthMode.Depth10;
    }
    void SetDepthMode(DepthMode mode){
        int i = (int )mode;
        if(i >= togglesDepth.Length){
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesDepth[i].isOn = true;
    }

    public ZDepth GetZDepthMode(){
        for(int i = 0; i < togglesZDepth.Length; i++){
            if(togglesZDepth[i].isOn)
                return (ZDepth)i;
        }
        return ZDepth.Depth1;
    }
    void SetZDepthMode(ZDepth mode){
        int i = (int )mode;
        if(i >= togglesZDepth.Length){
            UnityEngine.Debug.LogError("ZDepth value exceeds limit.");
            return;
        }
        togglesZDepth[i].isOn = true;
    }

    public TimeMode GetTimeMode(){
        for(int i = 0; i < togglesTimeMode.Length; i++){
            if(togglesTimeMode[i].isOn)
                return (TimeMode)i;
        }
        return TimeMode.Timed;
    }
    void SetTimeMode(TimeMode mode){
        int i = (int )mode;
        if(i >= togglesTimeMode.Length){
            UnityEngine.Debug.LogError("TimeMode value exceeds limit.");
            return;
        }
        togglesTimeMode[i].isOn = true;
    }

    void SetSizeMode(SizeMode mode){
        togglesSize[(int)mode].isOn = true;
    }

    void SetLevelMode(LevelMode mode){
        togglesLevel[(int)mode].isOn = true;
    }

    public float GetJumpTime(){
        return sliderJumpTime.value;
    }

    void SetJumpTime(float time){
        sliderJumpTime.value = time;
    }

    public int GetCustomEyesIn(){
        return (int)sliderCustomEyesin.value;
    }

    void SetCustomEyesIn(int value){
        sliderCustomEyesin.value = value;
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

    public SizeMode GetSizeMode(){
        if(togglesSize[0].isOn)
            return SizeMode.Normal;
        else
            return SizeMode.Small;
    }

    public LevelMode GetLevelMode(){
        if(togglesLevel[0].isOn)
            return LevelMode.Level1;
        else
            return LevelMode.Level2;
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

    public void OnToggleVisualJumpTest(bool value){
        if(!value)
            return;
        ZDepthGroup.SetActive(true);
        TimeGroup.SetActive(true);
        TimeModeGroup.SetActive(false);
        LevelGroup.SetActive(false);
    }

    public void OnToggleVisualPowerTest(bool value){
        if(!value)
            return;
        ZDepthGroup.SetActive(false);
        TimeGroup.SetActive(GetTimeMode() == TimeMode.Timed);
        TimeModeGroup.SetActive(true);
        LevelGroup.SetActive(false);
    }

    public void OnToggleVisualSymbolsTest(bool value){
        if(!value)
            return;
        ZDepthGroup.SetActive(false);
        TimeGroup.SetActive(true);
        TimeModeGroup.SetActive(false);
        LevelGroup.SetActive(true);
    }

    public void OnToggleTimedMode(bool value){
        if(!value)
            return;
        TimeGroup.SetActive(true);
    }
    public void OnToggleMaxDistanceMode(bool value){
        if(!value)
            return;
        TimeGroup.SetActive(false);
    }
}
