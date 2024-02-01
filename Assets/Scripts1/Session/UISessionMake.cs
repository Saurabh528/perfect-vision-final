using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UISessionMake : MonoBehaviour
{
	public UISessionGameItem _gameitemtmpl;
	[SerializeField] TextMeshProUGUI _txtTotal;
	[SerializeField] TextMeshProUGUI _txtTime;
	[SerializeField] TMP_Dropdown _gamesDropdown;
	[SerializeField] Slider _sliderTime;
	[SerializeField] TMP_InputField _inputGameName;
	[SerializeField] ListBox _matchedList;
	[SerializeField] Toggle _toggle2Min, _toggle5Min;
	const string PREFNAME_THERAPY_2MIN = "2MinTherapy";
	int _timeMin;
	private void Start()
	{
		
		//t:UpdateGameSlots();
		if(PlayerPrefs.GetInt(PREFNAME_THERAPY_2MIN, 1) == 1)
		{
			_timeMin = 2;
			_toggle2Min.isOn = true;
		}
		else
		{
			_timeMin = 5;
			_toggle5Min.isOn = true;
		}
		if(GameState.userRole == USERROLE.PATIENT){
			if(PatientMgr.GetPatientList().Count == 0)
				PatientDataManager.LoadPatientData(OnLoadPatientDataSuccess, ShowErrorMsg);
			else
				OnLoadPatientDataSuccess(PatientMgr.GetPatientList());
		}
		
	}

	void OnLoadPatientDataSuccess(Dictionary<Int32, PatientData> plist){
		PatientMgr.SetPatientList(plist);
		Dictionary<int, PatientData> dic = PatientMgr.GetPatientList();
		if(dic.Count == 1){
			PatientData pd = dic.ElementAt(0).Value;
			if(GameState.IsPatient() || pd.IsHome())
				PatientDataManager.GetHomePatientCalib(pd, SetCurrentPatient);
			else
				PatientView.Instance.SetCurrentPatient(pd);
		}
		
	}

	void SetCurrentPatient(PatientData pd){
		PatientView.Instance.SetCurrentPatient(pd);
	}

	
	public void OnAddGame(Int32 index)
	{
		_gamesDropdown.SetValueWithoutNotify(-1);
		_gamesDropdown.value = -1;
		string error;
		if(!SessionMgr.AddGame((byte)index, out error))
		{
			EnrollmentManager.Instance.ShowMessage(error);
			return;
		}
		UpdateGameSlots();
	}


	public void UpdateGameSlots()
	{
		List<byte> gamelist = SessionMgr.GetGameList();
		UtilityFunc.DeleteAllSideTransforms(_gameitemtmpl.transform);
		int count = 0;
		RectTransform rt = _gameitemtmpl.GetComponent<RectTransform>();
		foreach (byte gameindex in gamelist)
		{
			UISessionGameItem newitem = Instantiate(_gameitemtmpl, rt);
			string gamename = SessionMgr.GetGameName(gameindex);
			newitem.name = gamename;
			RectTransform newrt = newitem.GetComponent<RectTransform>();
			newrt.SetParent(rt.parent);
			newrt.localPosition = rt.localPosition + new Vector3(290 * count, 0, 0);
			newrt.localScale = rt.localScale;
			count++;
			newitem.SetGaneName(gamename);
			newitem.gameObject.SetActive(true);
		}
		ShowDesiredSessionTime();
		if(GameState.IsDoctor()){
			_inputGameName.gameObject.SetActive(gamelist.Count < GameConst.MAX_THERAPHYGAMECOUNT);
			_inputGameName.text = "";
			rt = _inputGameName.GetComponent<RectTransform>();
			rt.offsetMin = new Vector2(20 + gamelist.Count * 290, rt.offsetMin.y);
			rt = _matchedList.GetComponent<RectTransform>();
			rt.offsetMin = new Vector2(20 + gamelist.Count * 290, rt.offsetMin.y);
			rt.offsetMax = new Vector2(420 + gamelist.Count * 290, rt.offsetMax.y);
			OnEditGameNameChanged("");
			_inputGameName.ActivateInputField();
		}
		
	}

	void ShowDesiredSessionTime()
	{
		int count = SessionMgr.GetGameList().Count;
		_txtTotal.text = count.ToString();
		_txtTime.text = (count * _timeMin).ToString() + " / " + (6 * _timeMin).ToString();
		_sliderTime.value = (float)count / GameConst.MAX_THERAPHYGAMECOUNT;
	}


	public void RemoveGameItem(UISessionGameItem item)
	{
		string error;
		if (SessionMgr.RemoveGame(item.GetGameName(), out error))
			UpdateGameSlots();
		else
			EnrollmentManager.Instance.ShowMessage(error);
	}

	public void OnBtnStartSession()
	{
		if (GameState.currentPatient == null)
			EnrollmentManager.Instance.ShowMessage("Please select patient.");
		else if (SessionMgr.GetGameList().Count < 6){
			if(GameState.IsDoctor())
				EnrollmentManager.Instance.ShowMessage("Please select at least 6 games.");
			else
				EnrollmentManager.Instance.ShowMessage("6 games have to be selected by doctor.");
		}
		else
		{
			
			SessionMgr.StartSession(_timeMin * 60);
		}
	}

	public void OnEditGameNameChanged(string str)
	{
		if(string.IsNullOrEmpty(str))
			_matchedList.gameObject.SetActive(false);
		else
		{
			string[] gamenames = SessionMgr.GetGameNames();
			_matchedList.Clear();
			foreach (string gamename in gamenames)
			{
				if (gamename.ToLower().Contains(str.ToLower()))
					_matchedList.Add(gamename);
			}
			if (_matchedList.GetItems().Count == 0)
				_matchedList.gameObject.SetActive(false);
			else
			{
				_matchedList.gameObject.SetActive(true);
				_matchedList.SetHighlight(0);
			}
		}
	}

	public void OnMatchGameClicked(string str)
	{
		_matchedList.gameObject.SetActive(false);
		string error;
		int index = Array.IndexOf(SessionMgr.GetGameNames(), str);
		if (index == -1)
			return;
		if (!SessionMgr.AddGame((byte)index, out error))
		{
			EnrollmentManager.Instance.ShowMessage(error);
			return;
		}
		UpdateGameSlots();
	}


	public void OnTimeToggleChange(bool value)
	{
		PlayerPrefs.SetInt(PREFNAME_THERAPY_2MIN, value ? 1 : 0);
		OnTimeChanged(value? 2: 5);
	}

	void OnTimeChanged(int timeMin)
	{
		_timeMin = timeMin;
		ShowDesiredSessionTime();
	}

	void ShowErrorMsg(string errstr)
	{
		EnrollmentManager.Instance.ShowMessage(errstr);
	}	
}
