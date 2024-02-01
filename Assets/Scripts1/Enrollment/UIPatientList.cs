using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIPatientList : MonoBehaviour
{
	public static UIPatientList Instance;
	[SerializeField] PatientItem _itemtmpl;


	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		if (PatientMgr.GetPatientList().Count == 0)
			PatientDataManager.LoadPatientData(OnLoadPatientDataSuccess, ShowErrorMsg);
		else
			OnLoadPatientDataSuccess(PatientMgr.GetPatientList());
	}

	void OnLoadPatientDataSuccess(Dictionary<Int32, PatientData> plist)
	{
		PatientMgr.SetPatientList(plist);
		Toggle toggle = FillWithPatientList(plist, GameState.currentPatient != null? GameState.currentPatient.ID: -1);
		if (toggle)
			toggle.isOn = true;
		else{
			ShowErrorMsg(plist.Count == 0? "Please add a patient.": "Please select a patient to view details.");
		}
	}



	Toggle FillWithPatientList(Dictionary<Int32, PatientData> patientList, int selectID = 0)
	{
		if (patientList == null)
			return null;
		UtilityFunc.DeleteAllSideTransforms(_itemtmpl.transform);

		int count = 0;
		RectTransform rt = _itemtmpl.GetComponent<RectTransform>();
		Toggle selection = null;
		foreach (KeyValuePair<Int32, PatientData> pair in patientList)
		{
			PatientData data = pair.Value;
			PatientItem newitem = Instantiate(_itemtmpl, rt);
			newitem.name = data.name;
			RectTransform newrt = newitem.GetComponent<RectTransform>();
			newrt.SetParent(rt.parent);
			newrt.localPosition = rt.localPosition + new Vector3(0, -count * 100, 0);
			newrt.localScale = rt.localScale;
			count++;
			newitem.SetPatientData(data);
			newitem.gameObject.SetActive(true);
			if (data.ID == selectID)
				selection = newitem.GetComponent<Toggle>();
		}
		return selection;
	}

	public void DeletePatient()
	{
		if (GameState.currentPatient == null)
			return;
		PopupUI.ShowQuestionBox($"Are you sure to delete {GameState.currentPatient.name}?", delegate{
			PatientDataManager.DeletePatient(GameState.currentPatient, OnDeletePatientSuccess, ShowErrorMsg);
		});
		//PatientDataManager.DeletePatient(GameState.currentPatient, OnDeletePatientSuccess, ShowErrorMsg);
	}

	void OnDeletePatientSuccess(PatientData pdata)
	{
		PatientMgr.RemovePatient(pdata.ID);
		if(pdata == GameState.currentPatient) { 
			GameState.currentPatient = null;
			PatientView.Instance.ViewPatientData();
			PatientDataMgr.ClearSessionData();
			UISessionRecordView.Instance.ShowPatientSessionData();
		}
		FillWithPatientList(PatientMgr.GetPatientList());
	}

	void ShowErrorMsg(string errstr)
	{
		EnrollmentManager.Instance.ShowMessage(errstr);
	}



	public void AddPatientData(PatientData pdata)
	{
		PatientMgr.AddPatientData(pdata);
		FillAndSelectPatient(pdata);
	}

	public void UpdatePatientData(PatientData pdata)
	{
		FillAndSelectPatient(pdata);
	}

	void FillAndSelectPatient(PatientData pdata)
	{
		Toggle selectable = FillWithPatientList(PatientMgr.GetPatientList(), pdata.ID);
		if (selectable)
			selectable.isOn = true;
		GameState.currentPatient = pdata;
		ColorCalibration.OnPatientChanged();
		PatientView.Instance.ViewPatientData();
	}

	public void OnSearchTextChanged(string value)
	{
		Dictionary<Int32, PatientData> list = new Dictionary<Int32, PatientData>();
		Dictionary<Int32, PatientData> alllist = PatientMgr.GetPatientList();
		foreach (KeyValuePair<Int32, PatientData> pair in alllist){
			if (string.IsNullOrEmpty(value) || pair.Value.name.ToLower().Contains(value.ToLower()))
				list[pair.Key] = pair.Value;
		}
		FillWithPatientList(list, -1);
	}


}
