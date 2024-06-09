using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;

public class Slider1 : MonoBehaviour
{


    public Slider BackSlider; // Frame SLider
    public Slider RedSlider;
    public Slider CyanSlider;
    public Renderer glassBack;
    public Renderer Bee;
    public Renderer Bird;
	public Image BackHandle, RedHandle, CyanHandle;


    public Text BackSliderText;
    public Text RedText;
    public Text CyanText;
	public Toggle toggleTransparent;
	public TMP_Dropdown _dropdownProfile;
	private void Start()
	{
        BackSlider.value = GameState.currentPatient == null? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Background), 0.5f): GameState.currentPatient.cali.bg;
		OnBackSliderChange(BackSlider.value);
		toggleTransparent.isOn = GameState.currentPatient == null ? 
			(PlayerPrefs.GetInt(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Transparet), 1) == 1)
			: GameState.currentPatient.cali.transparent;
		string profilestr = GameState.currentPatient == null ?
				PlayerPrefs.GetString(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Profile), ColorCalibration.PROFILE_DEFAULT)
				: GameState.currentPatient.cali.profileStr;
		if (string.IsNullOrEmpty(profilestr))
			profilestr = ColorCalibration.PROFILE_DEFAULT;
		int index = -1;
		int count = 0;
		foreach(TMP_Dropdown.OptionData data in _dropdownProfile.options)
		{
			if(data.text == profilestr)
			{
				index = count;
				break;
			}
			count++;
		}
		_dropdownProfile.value = index == -1? 0: index;
		RedSlider.value = GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Red), 0.5f) : GameState.currentPatient.cali.rd;
		OnRedSliderChange(RedSlider.value);
        CyanSlider.value = GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Cyan), 0.5f) : GameState.currentPatient.cali.cy;
		OnCyanSliderChange(CyanSlider.value);
    }


	public void OnRedSliderChange(float value)
	{
		Color newColor = ColorCalibration.GetRedColor(value);
		Bee.material.color = newColor;
		PlayerPrefs.SetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Red), value);
		if (GameState.currentPatient != null)
			GameState.currentPatient.cali.rd = value;
		int percentage = Mathf.RoundToInt(value * 100);
		RedText.text = $"{percentage}%";
		RedHandle.color = newColor;
		ColorCalibration.RedColor = newColor;
	}

	public void OnCyanSliderChange(float value)
	{
		Color newColor = ColorCalibration.GetCyanColor(value);
		Bird.material.color = newColor;
		PlayerPrefs.SetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Cyan), value);
		if (GameState.currentPatient != null)
			GameState.currentPatient.cali.cy = value;
		int percentage = Mathf.RoundToInt(value * 100);
		CyanText.text = $"{percentage}%";
		CyanHandle.color = newColor;
		ColorCalibration.CyanColor = newColor;
	}

	public void OnBackSliderChange(float value)
	{
		Color newColor = ColorCalibration.GetBackGroundColor(value);
		glassBack.material.color = newColor;
		PlayerPrefs.SetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Background), value);
		if (GameState.currentPatient != null)
			GameState.currentPatient.cali.bg = value;
		int percentage = Mathf.RoundToInt(value * 100);
		BackSliderText.text = $"{percentage}%";
		BackHandle.color = newColor;
		ColorCalibration.BackColor = newColor;
	}

	public void OnRedLeftTextChange(string value)
	{
		ColorCalibration.Color_RedLeft = value;
		OnRedSliderChange(RedSlider.value);
	}

	public void OnRedRightTextChange(string value)
	{
		ColorCalibration.Color_RedRight = value;
		OnRedSliderChange(RedSlider.value);
	}

	public void OnCyanLeftTextChange(string value)
	{
		ColorCalibration.Color_CyanLeft = value;
		OnCyanSliderChange(CyanSlider.value);
	}

	public void OnCyanRightTextChange(string value)
	{
		ColorCalibration.Color_CyanRight = value;
		OnCyanSliderChange(CyanSlider.value);
	}

	public void OnBackLeftTextChange(string value)
	{
		ColorCalibration.Color_BackLeft = value;
		OnBackSliderChange(BackSlider.value);
	}

	public void OnBackRightTextChange(string value)
	{
		ColorCalibration.Color_BackRight = value;
		OnBackSliderChange(BackSlider.value);
	}

	public void OnBtnComplete()
	{
        if (GameState.currentPatient != null)
			PatientDataManager.UpdatePatient(GameState.currentPatient, null, null);
		ChangeScene.LoadScene("ScreenDistance");
	}

	public void OnSliderTransparent(Boolean value)
	{
		PlayerPrefs.SetInt(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Transparet), value? 1 : 0);
		if (GameState.currentPatient != null)
			GameState.currentPatient.cali.transparent = value;
		Color newColor = ColorCalibration.GetRedColor(RedSlider.value);
		Bee.material.color = newColor;
		RedHandle.color = newColor;
		ColorCalibration.RedColor = newColor;
	}

	public void OnProfileChanged(Int32 index)
	{
		string profilestr = _dropdownProfile.options[index].text;
		PlayerPrefs.SetString(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Profile), profilestr);
		if (GameState.currentPatient != null)
			GameState.currentPatient.cali.profileStr = profilestr;
		Color newColor = ColorCalibration.GetRedColor(RedSlider.value);
		Bee.material.color = newColor;
		RedHandle.color = newColor;
		ColorCalibration.RedColor = newColor;
	}
}


