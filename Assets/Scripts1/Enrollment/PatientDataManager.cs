using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Newtonsoft.Json;
using System.Diagnostics;
using UnityEngine;
using PlayFab.GroupsModels;
using UnityEngine.PlayerLoop;

public static class PatientDataManager
{


	public static void DeletePatient(PatientData pdata, UnityAction<PatientData> successAction, UnityAction<string> failAction)
	{
        UnityEngine.Debug.Log("Delete Patient called");
		if (pdata == null)
			return;
		PatientData backuppatient = PatientMgr.FindPatient(pdata.ID);
		if (backuppatient == null)
		{
			failAction.Invoke(@"Patient {pdata.name} does not exist.");
			return;
		}
		if (GameState.IsOnline) {
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
			{
				Data = new Dictionary<string, string>() { { pdata.name, null } }
			},
			result => { }, error => { });
		}
		

		PatientMgr.RemovePatient(pdata.ID);
		Dictionary<Int32, PatientData> plist = PatientMgr.GetPatientList();
		string jsonstr = JsonConvert.SerializeObject(plist);
		DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
		DataKey.DeletePrefsKey(pdata.name);
		if (!GameState.IsOnline)
			successAction.Invoke(pdata);
		else
		{
			UpdateUserDataRequest request = new UpdateUserDataRequest();
			request.Data = new Dictionary<string, string>();
			request.Data.Add(DataKey.PATIENT, jsonstr);
			request.Permission = UserDataPermission.Public;
			PlayFabClientAPI.UpdateUserData(request,
				result =>
				{
					successAction.Invoke(pdata);
				},
				error =>
				{
					PatientMgr.AddPatientData(backuppatient);
					failAction.Invoke(error.ToString());
				});
		}
		
	}
	public static void AddPatient(PatientData pdata, UnityAction<PatientData> successAction, UnityAction<string> failAction)
	{
		//Called when we click add on the home section or clinic section called 
        UnityEngine.Debug.Log("5)Add Patient called");
        if (pdata == null)
			return;
		Dictionary<Int32, PatientData> plist = PatientMgr.GetPatientList();
		foreach(KeyValuePair<Int32, PatientData> pair in plist){
			if(pair.Value.name == pdata.name || pdata.name == DataKey.PATIENT){
				failAction.Invoke($"{pdata.name} already exists.");
				return;
			}

		}

		if(pdata.place == THERAPPYPLACE.Clinic){
			PatientMgr.AddPatientData(pdata);
			string jsonstr = JsonConvert.SerializeObject(plist);
			DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
			UpdateUserDataRequest request = new UpdateUserDataRequest();
			request.Data = new Dictionary<string, string>();
			request.Data.Add(DataKey.PATIENT, jsonstr);
			request.Permission = UserDataPermission.Public;
			PlayFabClientAPI.UpdateUserData(request,
				result => {
					successAction.Invoke(pdata);
				},
				error => {
					PatientMgr.RemovePatient(pdata.ID);
					failAction.Invoke(error.ToString());
			});
			return;
		}

		if(!GameState.IsOnline){
			failAction.Invoke($"Can not add patient in offline mode.");
			return;
		}
		string doctorID = GameState.playfabID;
		string licenseKey = UtilityFunc.ComputeSha256Hash (SystemInfo.deviceUniqueIdentifier + UnityEngine.Random.Range(100000, 1000000).ToString());
        UnityEngine.Debug.Log("THE LICENSE KEY IS " + licenseKey);
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = licenseKey,
            CreateAccount = true
			//, InfoRequestParameters = new GetPlayerCombinedInfoRequestParams(GetTitleData=True)
        }, (result) => {
			pdata.PFID = result.PlayFabId;
			UpdateUserDataRequest request = new UpdateUserDataRequest();
			request.Data = new Dictionary<string, string>();
			request.Data.Add(DataKey.DOCTORID, doctorID);
			PlayFabClientAPI.UpdateUserData(request,
			result => {
				request = new UpdateUserDataRequest();
				request.Data = new Dictionary<string, string>();
				request.Data.Add(DataKey.ROLE, USERROLE.PATIENT.ToString());
				PlayFabClientAPI.UpdateUserData(request,
				result => {
					PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
					{
						TitleId = PlayFabSettings.TitleId,
						Username = GameState.username,
						Password = GameState.passwordhash,
					}, result =>
					{
						pdata.licenseKey = licenseKey;
						PatientMgr.AddPatientData(pdata);
						string jsonstr = JsonConvert.SerializeObject(plist);
						DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
						UpdateUserDataRequest request = new UpdateUserDataRequest();
						request.Data = new Dictionary<string, string>();
						request.Data.Add(DataKey.PATIENT, jsonstr);
						request.Permission = UserDataPermission.Public;
						PlayFabClientAPI.UpdateUserData(request,
							result => {
								successAction.Invoke(pdata);
							},
							error => {
								PatientMgr.RemovePatient(pdata.ID);
								failAction.Invoke(error.ToString());
						});
						return;
					}, error =>
					{
						Application.Quit();
						return;
					});
				},
				error => {
					PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
					{
						TitleId = PlayFabSettings.TitleId,
						Username = GameState.username,
						Password = GameState.passwordhash,
					}, result =>
					{
						failAction.Invoke(error.ToString());
					},
					error =>{
						Application.Quit();
					});
					return;
				});
			},
			error => {
				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
				{
					TitleId = PlayFabSettings.TitleId,
					Username = GameState.username,
					Password = GameState.passwordhash,
				}, result =>
				{
					failAction.Invoke(error.ToString());
				},
				error =>{
					Application.Quit();
				});
				return;
			});
       	},
		error => {
            failAction.Invoke(error.ToString());
        });		
	}


	public static void UpdatePatient(PatientData pdata, UnityAction<PatientData> successAction = null, UnityAction<string> failAction = null)
	{
        UnityEngine.Debug.Log("Update Patient called");
        if (pdata == null)
			return;
		PatientData backuppatient = PatientMgr.FindPatient(pdata.ID);

		if (backuppatient == null)
		{
			if(failAction != null)
				failAction.Invoke(@"Patient {pdata.name} does not exist.");
			return;
		}
		PatientMgr.UpdatePatientData(pdata);
		Dictionary<Int32, PatientData> plist = PatientMgr.GetPatientList();
		string jsonstr = JsonConvert.SerializeObject(plist);
		DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
		if(GameState.IsPatient() && pdata.IsHome()){
			DataKey.SetPrefsString(DataKey.HOMECALIB, JsonConvert.SerializeObject(pdata.cali));
		}
		if (!GameState.IsOnline)
		{
			if(successAction != null)
				successAction.Invoke(pdata);
		}
		else
		{
			if(GameState.IsPatient() && pdata.IsHome()){
				UpdateUserDataRequest request = new UpdateUserDataRequest();
				request.Data = new Dictionary<string, string>();
				request.Data.Add(DataKey.HOMECALIB, JsonConvert.SerializeObject(pdata.cali));
				request.Permission = UserDataPermission.Public;
				PlayFabClientAPI.UpdateUserData(request,
					result =>
					{
						if (successAction != null)
							successAction.Invoke(pdata);
					},
					error =>
					{
						PatientMgr.UpdatePatientData(backuppatient);
						if (failAction != null)
							failAction.Invoke(error.ToString());
				});
			}
			else{
				UpdateUserDataRequest request = new UpdateUserDataRequest();
				request.Data = new Dictionary<string, string>();
				request.Data.Add(DataKey.PATIENT, jsonstr);
				request.Permission = UserDataPermission.Public;
				PlayFabClientAPI.UpdateUserData(request,
					result =>
					{
						if (successAction != null)
							successAction.Invoke(pdata);
					},
					error =>
					{
						PatientMgr.UpdatePatientData(backuppatient);
						if (failAction != null)
							failAction.Invoke(error.ToString());
				});
			}
		}
	}
	public static void LoadPatientData(UnityAction<Dictionary<Int32, PatientData>> successAction = null, UnityAction<string> failAction = null)
	{
		//This funcction is called first when clicked on Patient Enrollment
        UnityEngine.Debug.Log("1)LoadPatientData Patient called");
        if (GameState.IsOnline){
			//online mode
			GetUserDataRequest request = new GetUserDataRequest();
			request.Keys = new List<string>();
			request.Keys.Add(DataKey.PATIENT);
			if(GameState.IsPatient())
				request.PlayFabId = GameState.DoctorID;
			PlayFabClientAPI.GetUserData(request,
				result =>
				{
					if(result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
					{
						string str = result.Data[DataKey.PATIENT].Value;
						if(GameState.IsDoctor())
							DataKey.SetPrefsString(DataKey.PATIENT, str);
						Dictionary<Int32, PatientData> plist;
						if(string.IsNullOrEmpty(str))
							plist = new Dictionary<int, PatientData>();
						else
							plist = JsonConvert.DeserializeObject<Dictionary<Int32, PatientData>>(str);
						if(GameState.IsPatient() && plist.Count > 1){
							Dictionary<Int32, PatientData> plistMine = new Dictionary<int, PatientData>();
							foreach(KeyValuePair<Int32, PatientData> pair in plist){
								if(pair.Value.PFID == GameState.playfabID){
									plistMine.Add(pair.Key, pair.Value);
									break;
								} 
							}
							plist = plistMine;
							str = JsonConvert.SerializeObject(plist);
							DataKey.SetPrefsString(DataKey.PATIENT, str);
						}
						if (successAction != null)
							successAction.Invoke(plist);
					}
					else if (failAction != null)
						failAction.Invoke("Patient data does not exist.");
				},
				error =>
				{
					
				}
			);
		}
		else{
			//offline mode
			string str = DataKey.GetPrefsString(DataKey.PATIENT, "");
			if (!string.IsNullOrEmpty(str))
			{
				Dictionary<Int32, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<Int32, PatientData>>(str);
				if(successAction != null)
					successAction.Invoke(plist);
				return;
			}
			else if (failAction != null)
				failAction.Invoke("Failed to load local patient data.");
		}
	}

	public static void GetHomePatientCalib(PatientData pd, UnityAction<PatientData> successAction = null){
        UnityEngine.Debug.Log("GetHomePatientCalib Patient called");
        if (pd == null)
			return;
		else if(pd.IsClinic()){
			successAction.Invoke(pd);
			return;
		}
		if(GameState.IsOnline){
			GetUserDataRequest request = new GetUserDataRequest();
			request.Keys = new List<string>();
			request.Keys.Add(DataKey.HOMECALIB);
			request.PlayFabId = pd.PFID;
			PlayFabClientAPI.GetUserData(request,
			result =>
			{
				if(result.Data != null && result.Data.ContainsKey(DataKey.HOMECALIB))
				{
					string jsonstr = result.Data[DataKey.HOMECALIB].Value;
					if(GameState.IsPatient())
						DataKey.SetPrefsString(DataKey.HOMECALIB, jsonstr);
					pd.cali = JsonConvert.DeserializeObject<CalibraionDetails>(jsonstr);
					successAction.Invoke(pd);
				}
				else
					successAction.Invoke(pd);
			},
			error =>{
				successAction.Invoke(pd);
			});
		}
		else if(GameState.IsPatient()){
			string jsonstr = DataKey.GetPrefsString(DataKey.HOMECALIB);
			if(!string.IsNullOrEmpty(jsonstr))
				pd.cali = JsonConvert.DeserializeObject<CalibraionDetails>(jsonstr);
			else
				DataKey.SetPrefsString(DataKey.HOMECALIB, JsonConvert.SerializeObject(pd.cali));
			successAction.Invoke(pd);
		}
		else
			successAction.Invoke(pd);
	}

	
}
