using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GoogleMobileAds.Api;
using System;

public class UIEditPatient : MonoBehaviour
{
	[SerializeField] TMP_InputField _name, _licenseKey;
	[SerializeField] TMP_InputField _detail;
	[SerializeField] TMP_InputField _age, _expireDate;
	[SerializeField] TMP_Dropdown _gender, _place;
	[SerializeField] TextMeshProUGUI _msg;
	[SerializeField] TextMeshProUGUI _lblButton;
	public float _msgTime = 3;
	float _msgexpiretime;
	List<PatientData> _plist;
	PatientData _curdata;



	private void Update()
	{
		if(_msgexpiretime > 0)
		{
			_msgexpiretime -= Time.deltaTime;
			if (_msgexpiretime < 0)
				_msg.enabled = false;
		}
	}

	void ShowMessage(string txt)
	{
		Debug.Log("ShowMessage function called.");
		_msg.text = txt;
		_msg.enabled = true;
		_msgexpiretime = _msgTime;
	}

	public void OnBtnAddPatient()
	{
		//Called 2nd when click on Add
		Debug.Log("2) Button Add Patient called.");
		_curdata = null;
		_name.text = _age.text = _expireDate.text = "";
		
		_gender.value = 0;
		_lblButton.text = "Add";
		_detail.text = "";
		_place.value = (int)THERAPPYPLACE.Clinic;
		_place.enabled = true;
		_licenseKey.gameObject.SetActive(false);
		_expireDate.gameObject.SetActive(false);
		gameObject.SetActive(true);
	}

	public void OnBtnEditPatient()
	{
		Debug.Log("On Button Edit Patient Called");
		if (GameState.currentPatient == null)
			return;
		_curdata = GameState.currentPatient;
		_name.text = _curdata.name;
		_age.text = _curdata.age.ToString();
		_gender.value = (int)_curdata.gender;
		_lblButton.text = "Update";
		_detail.text = _curdata.details;
		_place.value = (int)_curdata.place;
		_place.enabled = false;
		_expireDate.text = ((THERAPPYPLACE)_gender.value == THERAPPYPLACE.Clinic)?"": _curdata.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME);
		_expireDate.gameObject.SetActive((THERAPPYPLACE)_gender.value == THERAPPYPLACE.Home);
		_licenseKey.text = _curdata.licenseKey;
		_licenseKey.gameObject.SetActive((THERAPPYPLACE)_gender.value == THERAPPYPLACE.Home);
		gameObject.SetActive(true);
	}



	public void AddOrEdit()
	{
		//AUTO GENERATED LICENSE KEY.
		//Called when we press add on the home section or clinci section 
		Debug.Log("4)AddOrEdit Function called");
		DateTime ExpDatetime = new DateTime();
		if(string.IsNullOrEmpty(_name.text))
		{
			ShowMessage("Please input name.");
			return;
		}
		else if(_name.text.Contains(',') || _name.text.Contains(':') || _name.text.Contains('\\') || _name.text.Contains('\''))
		{
			ShowMessage("Invalid name format.");
			return;
		}
		else if (string.IsNullOrEmpty(_age.text) || int.Parse(_age.text) > 99)
		{
			ShowMessage("Age is invalid.");
			return;
		}
		else if (string.IsNullOrEmpty(_detail.text))
		{
			ShowMessage("Please input details.");
			return;
		}
		else if((THERAPPYPLACE)_place.value == THERAPPYPLACE.Home && !DateTime.TryParse(_expireDate.text, out ExpDatetime)){
			ShowMessage("Invalid expiring date format");
			return;
		}
		if (_curdata == null)
		{
			PatientData pdata = new PatientData(PatientMgr.GetFreePatientID(), _name.text, byte.Parse(_age.text), (GENDER)_gender.value, _detail.text, (THERAPPYPLACE)_place.value, ExpDatetime);
			PatientDataManager.AddPatient(pdata, OnAddPatientSuccess, ShowMessage);
		}
		else
		{
			_curdata.name = _name.text;
			_curdata.age = byte.Parse(_age.text);
			_curdata.gender = (GENDER)_gender.value;
			_curdata.details = _detail.text;
			_curdata.ExpireDate = ExpDatetime;
			PatientDataManager.UpdatePatient(_curdata, OnUpdatePatientSuccess, ShowMessage);
		}

	}

	void OnAddPatientSuccess(PatientData pdata)
	{
		//This when we succesfully adding the patient
		Debug.Log("6)On Add Patient Success called");
		UIPatientList.Instance.AddPatientData(pdata);
		if((THERAPPYPLACE)_place.value == THERAPPYPLACE.Clinic){
			gameObject.SetActive(false);
			ChangeScene.LoadScene("ColorScreen");
		}
		_licenseKey.text = pdata.licenseKey;
	}


	void OnUpdatePatientSuccess(PatientData pdata)
	{
		Debug.Log("On Update Patient Success called");
		gameObject.SetActive(false);
		UIPatientList.Instance.UpdatePatientData(pdata);
	}

	public void OnPlaceChanged(int value){
		//Called when clicked on Add patient or when changed the dropdown from Clinic to home
		Debug.Log("3)On Place Changed function called");
		THERAPPYPLACE place = (THERAPPYPLACE)value;
		if(place == THERAPPYPLACE.Clinic){
			_licenseKey.text = "";
			_expireDate.gameObject.SetActive(false);
			_licenseKey.gameObject.SetActive(false);
		}
		else{
			_expireDate.gameObject.SetActive(true);
			_licenseKey.gameObject.SetActive(true);
		}
	}
}
