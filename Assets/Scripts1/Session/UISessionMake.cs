using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;
/* using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.SceneManagement; */

public class UISessionMake : MonoBehaviour
{
	public static UISessionMake Instance;
	public UISessionGameItem _gameitemtmpl;
	[SerializeField] TextMeshProUGUI _txtTotal;
	[SerializeField] TextMeshProUGUI _txtTime;
	[SerializeField] TMP_Dropdown _gamesDropdown;
	[SerializeField] Slider _sliderTime;
	[SerializeField] TMP_InputField _inputGameName;
	[SerializeField] ListBox _matchedList;
	[SerializeField] Toggle _toggle2Min, _toggle5Min;
	int _timeMin;
	//private int count;
	//private int countLimit;

	void Awake(){
		Instance = this;
	}
	private void Start()
	{

		//t:UpdateGameSlots();
		if (GameState.currentPatient != null)
			OnTimeChanged(GameState.currentPatient.theraphyTime);
		if (GameState.userRole == USERROLE.PATIENT){
			if(PatientMgr.GetPatientList().Count == 0)
				PatientDataManager.LoadPatientData(OnLoadPatientDataSuccess, ShowErrorMsg);
			else
				OnLoadPatientDataSuccess(PatientMgr.GetPatientList());
		}
		
	}

	public void ShowTimeSetting()
	{
		if (GameState.currentPatient == null)
			return;
		if (GameState.currentPatient.theraphyTime == 2)
		{
			_timeMin = 2;
			_toggle2Min.isOn = true;
		}
		else
		{
			_timeMin = 5;
			_toggle5Min.isOn = true;
		}
	}

	void OnLoadPatientDataSuccess(Dictionary<string, PatientData> plist){
		PatientMgr.SetPatientList(plist);
		Dictionary<string, PatientData> dic = PatientMgr.GetPatientList();
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
		UtilityFunc.DeleteAllSideTransforms(_gameitemtmpl.transform, false);
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
		if (SessionMgr.RemoveGame(item.GetGameName(), out error)){
			UpdateGameSlots();
			Destroy(item.gameObject);
		}
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
    /*public void OnBtnStartSession()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && (result.Data.ContainsKey("CountLimit") || result.Data.ContainsKey("COUNT")))
            {
                countLimit = int.Parse(result.Data["CountLimit"].Value);
                count = int.Parse(result.Data["COUNT"].Value);
                //3
                Debug.Log("Count Limit extracted is: " + countLimit);
                Debug.Log("Count extracted is" + count);

            }
            else
            {
                Debug.LogWarning("Player data does not contain 'CountLimit' key. Setting default CountLimit to 15.");

                //           // Set default CountLimit to 15 in player data
                //           var data = new Dictionary<string, string>
                //           {
                //               { "CountLimit", countLimit.ToString() },
                //{ "COUNT",count.ToString() }
                //           };

                //           PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                //           {
                //               Data = data
                //           },
                //           updateResult =>
                //           {
                //               Debug.Log("Default CountLimit set in player data.");
                //           },
                //           updateError =>
                //           {
                //               Debug.LogError("Error setting default CountLimit: " + updateError.ErrorMessage);
                //           });

            }
			if (count <= countLimit)
			{
				count++;
			}
            Debug.Log("Count after incremented is " + count);
            var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string> { { "COUNT", count.ToString() } }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                //4
                Debug.Log("COUNT in UPDATE USER DATA is" + count);
                Debug.Log("Successfully updated");
                //Debug.Log("Count after incremented is " + count);

                if (GameState.currentPatient == null)
                {
                    Debug.Log("If1 section of the OnBtnStartSession");
                    EnrollmentManager.Instance.ShowMessage("Please select a patient.");
                }
                else if (SessionMgr.GetGameList().Count < 6)
                {
                    if (GameState.IsDoctor())
                    {
                        Debug.Log("If2 section of the OnBtnStartSession");
                        EnrollmentManager.Instance.ShowMessage("Please select at least 6 games.");
                    }
                    else
                    {
                        Debug.Log("Else1 section of the OnBtnStartSession");
                        EnrollmentManager.Instance.ShowMessage("6 games have to be selected by the doctor.");
                    }
                }
                else
                {
                    if (count == 3)
                    {
                        Debug.Log("Session Count Reached. Restarting session...");
                        count = 0;
                        var request = new UpdateUserDataRequest()
                        {
                            Data = new Dictionary<string, string> { 
						{ "COUNT","0" } },
                            //Permission = UserDataPermission.Public
                        };
                        Debug.Log("COUNT after restarting the session is" + count);
						PlayFabClientAPI.UpdateUserData(request,
							result =>
							{
								Debug.Log("Session Restarted");
                                SceneManager.LoadScene("ColorScreen");
                            },
							error =>
							{
								Debug.Log("Session failed");
							}
							);
                    }
                    else
                    {
                        //2
                        Debug.Log("Else2 section of the OnBtnStartSession");
                        SessionMgr.StartSession(_timeMin * 60);

                    }
                }
            },
            error =>
            {
                Debug.Log("No successfully updated");
            }
            );
        //SetPlayerData(count);
        //1)

    //    Debug.Log("Count after incremented is " + count);

    //    if (GameState.currentPatient == null)
    //    {
    //        Debug.Log("If1 section of the OnBtnStartSession");
    //        EnrollmentManager.Instance.ShowMessage("Please select a patient.");
    //    }
    //    else if (SessionMgr.GetGameList().Count < 6)
    //    {
    //        if (GameState.IsDoctor())
    //        {
    //            Debug.Log("If2 section of the OnBtnStartSession");
    //            EnrollmentManager.Instance.ShowMessage("Please select at least 6 games.");
    //        }
    //        else
    //        {
    //            Debug.Log("Else1 section of the OnBtnStartSession");
    //            EnrollmentManager.Instance.ShowMessage("6 games have to be selected by the doctor.");
    //        }
    //    }
    //    else
    //    {
    //        if (count == 3)
    //        {
    //            Debug.Log("Session Count Reached. Restarting session...");
				//count = 0;
				//Debug.Log("COUNT after restarting the session is" + count);
    //            RestartSession();
    //        }
    //        else
    //        {
				////2
    //            Debug.Log("Else2 section of the OnBtnStartSession");
    //            SessionMgr.StartSession(_timeMin * 60);
                
    //        }
    //    }
        }, error =>
        {
            Debug.LogError("Error fetching player data: " + error.ErrorMessage);

        });
        //int k = GetPlayerData();

		//count++;

  //      var request = new UpdateUserDataRequest()
  //      {
  //          Data = new Dictionary<string, string> { { "COUNT", count.ToString() } }
  //      };

  //      PlayFabClientAPI.UpdateUserData(request,
  //          result =>
  //          {
  //              //4
  //              Debug.Log("COUNT in UPDATE USER DATA is" + count);
  //              Debug.Log("Successfully updated");
  //          },
  //          error =>
  //          {
  //              Debug.Log("No successfully updated");
  //          }
  //          );
  //      //SetPlayerData(count);
  //      //1)

  //      Debug.Log("Count after incremented is " + count);

  //      if (GameState.currentPatient == null)
  //      {
  //          Debug.Log("If1 section of the OnBtnStartSession");
  //          EnrollmentManager.Instance.ShowMessage("Please select a patient.");
  //      }
  //      else if (SessionMgr.GetGameList().Count < 6)
  //      {
  //          if (GameState.IsDoctor())
  //          {
  //              Debug.Log("If2 section of the OnBtnStartSession");
  //              EnrollmentManager.Instance.ShowMessage("Please select at least 6 games.");
  //          }
  //          else
  //          {
  //              Debug.Log("Else1 section of the OnBtnStartSession");
  //              EnrollmentManager.Instance.ShowMessage("6 games have to be selected by the doctor.");
  //          }
  //      }
  //      else
  //      {
  //          if (count == 3)
  //          {
  //              Debug.Log("Session Count Reached. Restarting session...");
		//		count = 0;
		//		Debug.Log("COUNT after restarting the session is" + count);
  //              RestartSession();
  //          }
  //          else
  //          {
		//		//2
  //              Debug.Log("Else2 section of the OnBtnStartSession");
  //              SessionMgr.StartSession(_timeMin * 60);
                
  //          }
  //      }
    }*/

    //private void RestartSession()
    //{
    //    SceneManager.LoadScene("ColorScreen");
    //}

	//private void SetPlayerData(int count)
	//{
	//	var request = new UpdateUserDataRequest()
	//	{
	//		Data = new Dictionary<string, string> { { "COUNT", count.ToString() } }
	//	};

	//	PlayFabClientAPI.UpdateUserData(request,
	//		result =>
	//		{
	//			//4
	//			Debug.Log("COUNT in UPDATE USER DATA is" + count);
	//			Debug.Log("Successfully updated");
	//		},
	//		error =>
	//		{
	//			Debug.Log("No successfully updated");
	//		}
	//		);
	//}		
			
  //  private int GetPlayerData()
  //  {
		//int count=0,countLimit=3;
  //      PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
  //      {
  //          if (result.Data != null && (result.Data.ContainsKey("CountLimit") || result.Data.ContainsKey("COUNT")))
  //          {
  //               countLimit = int.Parse(result.Data["CountLimit"].Value);
  //               count = int.Parse(result.Data["COUNT"].Value);
		//		//3
  //              Debug.Log("Count Limit extracted is: " + countLimit);
		//		Debug.Log("Count extracted is" + count);
				
  //          }
  //          else
  //          {
  //              Debug.LogWarning("Player data does not contain 'CountLimit' key. Setting default CountLimit to 15.");

		//		//           // Set default CountLimit to 15 in player data
		//		//           var data = new Dictionary<string, string>
		//		//           {
		//		//               { "CountLimit", countLimit.ToString() },
		//		//{ "COUNT",count.ToString() }
		//		//           };

		//		//           PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
		//		//           {
		//		//               Data = data
		//		//           },
		//		//           updateResult =>
		//		//           {
		//		//               Debug.Log("Default CountLimit set in player data.");
		//		//           },
		//		//           updateError =>
		//		//           {
		//		//               Debug.LogError("Error setting default CountLimit: " + updateError.ErrorMessage);
		//		//           });
			
  //          }
  //      }, error =>
  //      {
  //          Debug.LogError("Error fetching player data: " + error.ErrorMessage);
            
  //      });
		//return count;
  //  }
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
		_timeMin = value? 2: 5;
		ShowDesiredSessionTime();
		if (GameState.currentPatient == null)
			return;
		GameState.currentPatient.theraphyTime = value ? 2 : 5;
		if (GameState.currentPatient.IsHome())
			GameState.currentPatient.PutDataToDoctorData(PatientMgr.GetHomePatientDataList()[GameState.currentPatient.name]);
		PatientDataManager.UpdatePatient(GameState.currentPatient);
	}

	public void OnTimeChanged(int timeMin)
	{
		_timeMin = timeMin;
		if (timeMin == 2)
			_toggle2Min.isOn = true;
		else
			_toggle5Min.isOn = true;
		ShowDesiredSessionTime();
	}

	void ShowErrorMsg(string errstr)
	{
		EnrollmentManager.Instance.ShowMessage(errstr);
	}	
}
