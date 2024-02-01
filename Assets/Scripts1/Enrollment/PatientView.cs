using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PatientView : MonoBehaviour
{
	public static PatientView Instance;
	[SerializeField] TextMeshProUGUI _name;
	[SerializeField] TextMeshProUGUI _age;
	[SerializeField] TextMeshProUGUI _gender;
	[SerializeField] TextMeshProUGUI _details;
	[SerializeField] GameObject _btnDelete, _btnStart;
	[SerializeField] UISessionMake _sessionmakeview;
	[SerializeField] GameObject _btnExportPDF;
    [SerializeField] GameObject _btnSetting, _btnProAnylysis, _btnDiagnose;

	private void Awake()
	{
		Instance = this;
		Clear();
	}

	private void Start()
	{
		if (GameState.currentPatient != null)
		{
			ViewPatientData();
			PatientDataMgr.GetPatientRecord().sessionlist.Clear();
			UISessionRecordView.Instance.LoadSessionData();
			UISessionRecordView.Instance.ShowPatientSessionData();
		}
	}

	void OnUpdatedPatientDataSuccess(PatientData data)
	{
		EnrollmentManager.Instance.ShowMessage(data.name + "'s data updated.");
	}

	void OnUpdatePatientDataFailed(string errstr)
	{
		EnrollmentManager.Instance.ShowMessage(errstr);
	}

	public void Clear()
	{
		_name.text = _gender.text = _details.text = _age.text = "";
		if(_btnDelete)
			_btnDelete.SetActive(false);
		_btnStart.SetActive(false);
        _btnExportPDF.SetActive(false);
        _btnSetting.SetActive(false);
		_btnDiagnose.SetActive(false);
		_btnProAnylysis.SetActive(false);
	}

	public void ViewPatientData()
	{
		if (GameState.currentPatient == null)
		{
			Clear();
			return;
		}
		_name.text = GameState.currentPatient.name;
		_age.text = GameState.currentPatient.age.ToString();
		_gender.text = GameState.currentPatient.gender.ToString();
		_details.text = GameState.currentPatient.details;
		if(_btnDelete)
			_btnDelete.SetActive(true);
		_btnStart.SetActive(GameState.IsPatient() || GameState.currentPatient.IsClinic());
        _btnExportPDF.SetActive(true);
        _btnSetting.SetActive(GameState.IsPatient() || GameState.currentPatient.IsClinic());
		_btnDiagnose.SetActive(GameState.IsPatient() || GameState.currentPatient.IsClinic());
		_btnProAnylysis.SetActive(true);
	}

	public void OnClickPatient(PatientItem item)
	{
		Toggle toggle = item.GetComponent<Toggle>();
		if (toggle == null || !toggle.isOn)
			return;
		if(GameState.IsPatient() || item._pdata.IsHome()){
			PatientDataManager.GetHomePatientCalib(item._pdata, SetCurrentPatient);
		}
		else
			SetCurrentPatient( item._pdata);
	}

	public void SetCurrentPatient(PatientData pd){
		UISessionRecordView.Instance.Clear();
		GameState.currentPatient = pd;
		if(pd == null)
			return;
		
		ColorCalibration.OnPatientChanged();
		ViewPatientData();
		PatientDataMgr.GetPatientRecord().sessionlist.Clear();
		UISessionRecordView.Instance.LoadSessionData();
		UISessionRecordView.Instance.ShowPatientSessionData();
		List<byte> gamelist = SessionMgr.GetGameList();
		gamelist.Clear();
		if (GameState.currentPatient != null)
		{
			foreach (byte gameindex in GameState.currentPatient.therapygames)
			{
				gamelist.Add(gameindex);
			}
		}
		_sessionmakeview.UpdateGameSlots();
	}


}
