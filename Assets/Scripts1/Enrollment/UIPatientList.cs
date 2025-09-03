using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
public class UIPatientList : MonoBehaviour
{
	public static UIPatientList Instance;
	[SerializeField] PatientItem _itemtmpl;

	[SerializeField] TMP_InputField _textPatientName;
	[SerializeField] TMP_Dropdown _dropdownPatientKind;


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

	void OnLoadPatientDataSuccess(Dictionary<string, PatientData> plist)
	{
		PatientMgr.SetPatientList(plist);
		Toggle toggle = FillWithPatientList(plist, GameState.currentPatient != null? GameState.currentPatient.name: "");
		if (toggle)
			toggle.isOn = true;
		else{
			ShowErrorMsg(plist.Count == 0? "Please add a patient.": "Please select a patient to view details.");
		}
	}



	Toggle FillWithPatientList(Dictionary<string, PatientData> patientList, string selectname = "")
	{
		if (patientList == null)
			return null;
		UtilityFunc.DeleteAllSideTransforms(_itemtmpl.transform, false);

		int count = 0;
		RectTransform rt = _itemtmpl.GetComponent<RectTransform>();
		Toggle selection = null;
		foreach (KeyValuePair<string, PatientData> pair in patientList)
		{
			PatientItem newitem = Instantiate(_itemtmpl, rt);
			newitem.name = pair.Key;
			RectTransform newrt = newitem.GetComponent<RectTransform>();
			newrt.SetParent(rt.parent);
			newrt.localPosition = rt.localPosition + new Vector3(0, -count * 100, 0);
			newrt.localScale = rt.localScale;
			count++;
			newitem.SetPatientInfo(pair.Key, count);
			newitem.gameObject.SetActive(true);
			if (pair.Key == selectname)
				selection = newitem.GetComponent<Toggle>();
		}
		return selection;
	}

	public void DeletePatient()
	{
		if (GameState.currentPatient == null)
			return;
		PopupUI.ShowQuestionBox($"Are you sure to delete {GameState.currentPatient.name}?", delegate{
			PatientDataManager.DeletePatient(GameState.currentPatient.name, OnDeletePatientSuccess, ShowErrorMsg);
		});
		//PatientDataManager.DeletePatient(GameState.currentPatient, OnDeletePatientSuccess, ShowErrorMsg);
	}

	void OnDeletePatientSuccess(string name)
	{
		PatientMgr.RemovePatient(name);
		if(GameState.currentPatient != null && name == GameState.currentPatient.name) { 
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
		Toggle selectable = FillWithPatientList(PatientMgr.GetPatientList(), pdata.name);
		if (selectable)
			selectable.isOn = true;
		GameState.currentPatient = pdata;
		ColorCalibration.OnPatientChanged();
		PatientView.Instance.ViewPatientData();
	}

	public void OnSearchTextChanged(string value)
	{
		ApplyPatientFilter();
	}

	public void OnPatientKindChanged(int kindIndex)
	{
		ApplyPatientFilter();
	}

	void ApplyPatientFilter()
	{
		Dictionary<string, PatientData> list = new Dictionary<string, PatientData>();
		Dictionary<string, PatientData> alllist = PatientMgr.GetPatientList();
		Dictionary<string, string> nameIDList = PatientMgr.GetNameIDList();
		string searchText = _textPatientName.text;
		foreach (KeyValuePair<string, PatientData> pair in alllist)
		{
			if ((string.IsNullOrEmpty(searchText) || pair.Key.ToLower().Contains(searchText.ToLower())) &&
				(_dropdownPatientKind.value == 0 || (_dropdownPatientKind.value == 1 && nameIDList[pair.Key] == GameConst.PLAYFABID_CLINIC) || (_dropdownPatientKind.value == 2 && nameIDList[pair.Key] != GameConst.PLAYFABID_CLINIC)))
				list[pair.Key] = pair.Value;
		}
		FillWithPatientList(list, "");
	}
}
