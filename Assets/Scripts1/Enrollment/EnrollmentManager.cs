using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnrollmentManager : MonoBehaviour
{

	public static EnrollmentManager Instance;
	[SerializeField] TextMeshProUGUI _msg;
	public float _msgTime = 3;
	float _msgexpiretime;
	[SerializeField] GameObject sessionMakeObj;
	void Awake()
	{
#if UNITY_EDITOR
		UISignIn.StartFromSignInDebugMode();
#endif
		Instance = this;
		if(GameConst.MODE_DOCTORTEST)
			sessionMakeObj.SetActive(false);
	}

	private void Start()
	{
		if(SessionMgr.NeedToAddSessionRecord())
		{
			if (GameState.currentPatient != null && GameState.currentGameMode == GAMEMODE.SessionGame)
			{
				SessionMgr.AddSessionRecordToData();
				PatientDataMgr.SavePatientData(SaveSessionDataSuccess, SaveSessionDataFailed);
			}
		}
		UISessionRecordView.Instance.ShowPatientSessionData();
	}

	void SaveSessionDataSuccess()
	{
		if(EnrollmentManager.Instance)
			EnrollmentManager.Instance.ShowMessage("Session record saved.");
	}

	void SaveSessionDataFailed(string error)
	{
		EnrollmentManager.Instance.ShowMessage("Saving session error: " + error);
	}

	public void ShowMessage(string txt)
	{
		_msg.text = txt;
		_msg.enabled = true;
		_msgexpiretime = _msgTime;
	}
	// Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
		if (_msgexpiretime > 0)
		{
			_msgexpiretime -= Time.deltaTime;
			if (_msgexpiretime < 0)
				_msg.enabled = false;
		}
	}


}
