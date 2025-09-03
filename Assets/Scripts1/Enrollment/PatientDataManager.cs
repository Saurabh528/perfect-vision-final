using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Newtonsoft.Json;
using UnityEngine;
using PlayFab.GroupsModels;
using UnityEngine.PlayerLoop;
using System.Linq;
//using UnityEditor.PackageManager;

public static class PatientDataManager
{
    public static void DeletePatient(string name, UnityAction<string> successAction, UnityAction<string> failAction)
	{
		
        UnityEngine.Debug.Log("Delete Patient called");
		Dictionary<string, string> nameIDList = PatientMgr.GetNameIDList();
		if (string.IsNullOrEmpty(name) || !nameIDList.ContainsKey(name)){
			failAction.Invoke("Patient does not exist");
			return;
		}
		string pfID = nameIDList[name];
		PatientMgr.RemovePatient(name);
		string jsonstr = JsonConvert.SerializeObject(nameIDList);
		if (GameState.IsOnline) {
			Dictionary<string, string> requestData = new Dictionary<string, string>() {{DataKey.PATIENT, jsonstr} };
			if(pfID == GameConst.PLAYFABID_CLINIC)
				requestData.Add(name, null);
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
			{
				Data = requestData
			},
			result => {
				DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
				if(pfID == GameConst.PLAYFABID_CLINIC)
					DataKey.DeletePrefsKey(name);
				successAction.Invoke(name);
			}, error => {
				failAction.Invoke(error.ToString());
			});
		}
		else{
			if(GameSetting.MODE_OFFLINEENABLED){
			DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
			if(pfID == GameConst.PLAYFABID_CLINIC)
				DataKey.DeletePrefsKey(name);
			}
		}
	}


	public static void AddPatient(PatientData pdata, UnityAction<PatientData> successAction, UnityAction<string> failAction)
	{
		if (pdata == null)
			return;
		Dictionary<string, string> nameIDList = PatientMgr.GetNameIDList();
		int cilinicCount = 0, homeCount = 0;
		foreach(KeyValuePair<string, string> pair in nameIDList){
			if(pair.Value == GameConst.PLAYFABID_CLINIC)
				cilinicCount++;
			else
				homeCount++;
		}
		if(pdata.place == THERAPPYPLACE.Clinic && GameState.CilinicLimit <= cilinicCount){
			if(GameState.CilinicLimit == 0)
				failAction.Invoke($"You are not allowed to add clinic patients.");
			else
				failAction.Invoke($"You are not allowed to enroll over {GameState.CilinicLimit} clinic patients.");
			return;
		}
		else if(pdata.place == THERAPPYPLACE.Home && GameState.HomeLimit <= homeCount){
			if(GameState.HomeLimit == 0)
				failAction.Invoke($"You are not allowed to add Home patients.");
			else
				failAction.Invoke($"You are not allowed to enroll over {GameState.HomeLimit} home patients.");
			return;
		}

		foreach(KeyValuePair<string, string> pair in nameIDList){
			if(pair.Key == pdata.name || pdata.name == DataKey.PATIENT){
				failAction.Invoke($"{pdata.name} already exists.");
				return;
			}

		}

		if(pdata.place == THERAPPYPLACE.Clinic){
			PatientMgr.AddPatientData(pdata);
			string jsonstr = JsonConvert.SerializeObject(PatientMgr.GetNameIDList());

			//update doctor record
			UpdateUserDataRequest request = new UpdateUserDataRequest();
			request.Data = new Dictionary<string, string>(){{DataKey.PATIENT, jsonstr}, {pdata.name, JsonConvert.SerializeObject(pdata)}};
			request.Permission = UserDataPermission.Public;
			PlayFabClientAPI.UpdateUserData(request,
				result => {
					DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
					DataKey.SetPrefsString(pdata.name, JsonConvert.SerializeObject(pdata));
					successAction.Invoke(pdata);
				},
				error => {
					PatientMgr.RemovePatient(pdata.name);
					failAction.Invoke(error.ToString());
			});

			return;
		}

		if(!GameState.IsOnline){
			failAction.Invoke($"Can not add patient in offline mode.");
			return;
		}
		string doctorID = GameState.playfabID;
		System.Random random = new System.Random();
		var bytes = new Byte[5];
		random.NextBytes(bytes);
		var hexArray = Array.ConvertAll(bytes, x => x.ToString("X2"));
		string licenseKey = String.Concat(hexArray);//UtilityFunc.ComputeSha256Hash (SystemInfo.deviceUniqueIdentifier + UnityEngine.Random.Range(100000, 1000000).ToString());
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = licenseKey,
            CreateAccount = true
			//, InfoRequestParameters = new GetPlayerCombinedInfoRequestParams(GetTitleData=True)
        }, (result) => {
			pdata.PFID = result.PlayFabId;
			UpdateUserDataRequest request = new UpdateUserDataRequest();
			request.Data = new Dictionary<string, string>(){{DataKey.DOCTORID, doctorID}
			, {DataKey.ROLE, USERROLE.PATIENT.ToString()}
			, {DataKey.HOMEPATIENT, JsonConvert.SerializeObject(pdata)}
			, {DataKey.EXPIREDATE, pdata.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME)}};
			request.Permission = UserDataPermission.Public;
			PlayFabClientAPI.UpdateUserData(request,
			result => {
				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
				{
					TitleId = PlayFabSettings.TitleId,
					Username = GameState.username,
					Password = GameState.password,
				}, result =>
				{
					pdata.licenseKey = licenseKey;
					PatientMgr.AddPatientData(pdata);
					string jsonstr = JsonConvert.SerializeObject(PatientMgr.GetNameIDList());
					string jstrHomePatientData = JsonConvert.SerializeObject(PatientMgr.GetHomePatientDataList()[pdata.name]);
					UpdateUserDataRequest request = new UpdateUserDataRequest();
					request.Data = new Dictionary<string, string>(){{DataKey.PATIENT, jsonstr}
					, {pdata.name, jstrHomePatientData}};
					request.Permission = UserDataPermission.Public;
					PlayFabClientAPI.UpdateUserData(request,
					result => {
						DataKey.SetPrefsString(DataKey.PATIENT, jsonstr);
						DataKey.SetPrefsString(pdata.name, jstrHomePatientData);
						successAction.Invoke(pdata);
					},
					error => {
						PatientMgr.RemovePatient(pdata.name);
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
					Password = GameState.password,
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
		PatientData backuppatient = PatientMgr.FindPatient(pdata.name);

		if (backuppatient == null)
		{
			if(failAction != null)
				failAction.Invoke(@"Patient {pdata.name} does not exist.");
			return;
		}
		PatientMgr.UpdatePatientData(pdata);
		if(GameState.IsPatient() && pdata.IsHome()){
			DataKey.SetPrefsString(DataKey.HOMECALIB, JsonConvert.SerializeObject(pdata.cali));
			DataKey.SetPrefsString(DataKey.HOMEPATIENT, JsonConvert.SerializeObject(pdata));
		}
		else if(GameState.IsDoctor() && pdata.IsClinic()){
			DataKey.SetPrefsString(pdata.name, JsonConvert.SerializeObject(pdata));
		}
		else if(GameState.IsDoctor() && pdata.IsHome()){
			Dictionary<string, HomePatientData> homeDataList = PatientMgr.GetHomePatientDataList();
			DataKey.SetPrefsString(pdata.name, JsonConvert.SerializeObject(homeDataList[pdata.name]));
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
				request.Data = new Dictionary<string, string>(){{DataKey.HOMECALIB, JsonConvert.SerializeObject(pdata.cali)}, {DataKey.HOMEPATIENT, JsonConvert.SerializeObject(pdata)}};
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
			else if(GameState.IsDoctor() && pdata.IsClinic()){
				UpdateUserDataRequest request = new UpdateUserDataRequest();
				request.Data = new Dictionary<string, string>(){{pdata.name, JsonConvert.SerializeObject(pdata)}};
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
			else if(GameState.IsDoctor() && pdata.IsHome()){
				UpdateUserDataRequest request = new UpdateUserDataRequest();
				request.Data = new Dictionary<string, string>(){{pdata.name, JsonConvert.SerializeObject(PatientMgr.GetHomePatientDataList()[pdata.name])}};
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
	public static void LoadPatientData(UnityAction<Dictionary<string, PatientData>> successAction = null, UnityAction<string> failAction = null)
	{
		//This funcction is called first when clicked on Patient Enrollment
        if (GameState.IsOnline){
			//online mode
			
			if(GameState.IsPatient()){
				GetUserDataRequest request = new GetUserDataRequest();
				request.Keys = new List<string>(){DataKey.HOMEPATIENT};
				PlayFabClientAPI.GetUserData(request,
				result =>
				{
					if(result.Data != null && result.Data.ContainsKey(DataKey.HOMEPATIENT)){
						DataKey.SetPrefsString(DataKey.HOMEPATIENT, result.Data[DataKey.HOMEPATIENT].Value);
						PatientData pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[DataKey.HOMEPATIENT].Value);
						PatientMgr.ClearPatients();
						PatientMgr.AddPatientData(pdata);
						successAction.Invoke(PatientMgr.GetPatientList());
					}
				},
				error =>
				{
					failAction.Invoke(error.ToString());
				});
			}
			else{
				GetUserDataRequest request = new GetUserDataRequest();
				request.Keys = new List<string>(){DataKey.PATIENT};
				PlayFabClientAPI.GetUserData(request,
				result =>
				{
					if(result.Data != null && result.Data.ContainsKey(DataKey.PATIENT)){
						DataKey.SetPrefsString(DataKey.PATIENT, result.Data[DataKey.PATIENT].Value);
						Dictionary<string, string> nameIDList = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Data[DataKey.PATIENT].Value);
						PatientMgr.SetNameIDList(nameIDList);
						Dictionary<string, PatientData> plist = new Dictionary<string, PatientData>();
						foreach(KeyValuePair<string, string> pair in nameIDList)
							plist[pair.Key] = null;
						successAction.Invoke(plist);
					}
				},
				error =>
				{
					failAction.Invoke(error.ToString());
				});
			}
		}
		else{
			if(GameSetting.MODE_OFFLINEENABLED){
			//offline mode
			if(GameState.IsPatient()){
				string str = DataKey.GetPrefsString(DataKey.HOMEPATIENT, "");
				if (!string.IsNullOrEmpty(str)){
					PatientData pdata = JsonConvert.DeserializeObject<PatientData>(str);
					Dictionary<string, PatientData> plist = new Dictionary<string, PatientData>();
					plist[pdata.name] = pdata;
					successAction.Invoke(plist);
				}
				else
					failAction.Invoke("Local data does not exist.");
			}
			else{
				string str = DataKey.GetPrefsString(DataKey.PATIENT, "");
				if (!string.IsNullOrEmpty(str)){
					Dictionary<string, string> nameIDPair = JsonConvert.DeserializeObject<Dictionary<string,string>>(str);
					PatientMgr.SetNameIDList(nameIDPair);
					Dictionary<string, PatientData> plist = new Dictionary<string, PatientData>();
					foreach(KeyValuePair<string, string> pair in nameIDPair)
						plist[pair.Key] = null;
					successAction.Invoke(plist);
				}
				else
					failAction.Invoke("Failed to load local parent data.");
			}
			}
		}
	}

	public static void GetHomePatientCalib(PatientData pd, UnityAction<PatientData> successAction = null){
        //UnityEngine.Debug.Log("GetHomePatientCalib Patient called");
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
		else if(GameSetting.MODE_OFFLINEENABLED){
			if(GameState.IsPatient()){
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

	public static void GetPatientDataByName(string name, UnityAction<PatientData> successAction, UnityAction<string> failAction){
		PatientData pdata= null;
		Dictionary<string, string> nameIDList = PatientMgr.GetNameIDList();
		if(!nameIDList.ContainsKey(name) && GameState.IsDoctor()){
			UnityEngine.Debug.LogError("Patient List is empty.");
			return;
		}
		if(GameState.IsDoctor()){
			if(!nameIDList.ContainsKey(name)){
				if(failAction != null){
					failAction.Invoke("Can not find such patient from database.");
					return;
				}
			}
			if(nameIDList[name] == GameConst.PLAYFABID_CLINIC){
				if(GameState.IsOnline){
					GetUserDataRequest request = new GetUserDataRequest();
					request.Keys = new List<string>(){name};
					PlayFabClientAPI.GetUserData(request,
						result=>{
							if(result.Data != null && result.Data.ContainsKey(name)){
								pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[name].Value);
								Dictionary<string, PatientData> plist = PatientMgr.GetPatientList();
								DataKey.SetPrefsString(name, result.Data[name].Value);
								plist[name] = pdata;
								if(successAction != null)
									successAction.Invoke(pdata);
							}
						},
						error=>{
							if(failAction != null)
								failAction.Invoke(error.ToString());
						}
					);
					return;
				}
				else if(GameSetting.MODE_OFFLINEENABLED){
					string localStr = DataKey.GetPrefsString(name);
					if(!string.IsNullOrEmpty(localStr)){
						pdata= JsonConvert.DeserializeObject<PatientData>(localStr);
						PatientMgr.GetPatientList()[name] = pdata;
						if(successAction != null)
							successAction.Invoke(pdata);
					}
					else if(failAction != null)
						failAction.Invoke("Local patient data does not exist.");
				}
			}
			else{//home patient
				if(GameState.IsOnline){
					GetUserDataRequest request = new GetUserDataRequest();
					request.Keys = new List<string>(){DataKey.HOMEPATIENT};
					request.PlayFabId = nameIDList[name];
					PlayFabClientAPI.GetUserData(request,
						result=>{
							if(result.Data != null && result.Data.ContainsKey(DataKey.HOMEPATIENT)){
								pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[DataKey.HOMEPATIENT].Value);
								Dictionary<string, PatientData> plist = PatientMgr.GetPatientList();
								DataKey.SetPrefsString(name, result.Data[DataKey.HOMEPATIENT].Value);
								plist[name] = pdata;
								//Get home patient data from doctor account
								request = new GetUserDataRequest();
								request.Keys = new List<string>(){pdata.name};
								PlayFabClientAPI.GetUserData(request,
									result=>{
										if(result.Data != null && result.Data.ContainsKey(pdata.name)){
											HomePatientData hpdata = JsonConvert.DeserializeObject<HomePatientData>(result.Data[pdata.name].Value);
											PatientMgr.GetHomePatientDataList()[pdata.name] = hpdata;
											pdata.GetDataFromDoctorData(hpdata);
											if(successAction != null)
												successAction.Invoke(pdata);
										}
										else if(failAction != null)
											failAction.Invoke("Can not download home patient data.");
									},
									error=>{
										if(failAction != null)
											failAction.Invoke(error.ToString());
									}
								);
							}
							else if(failAction != null)
								failAction.Invoke($"{name}'s data does not exist.");
						},
						error=>{
							if(failAction != null)
								failAction.Invoke(error.ToString());
						}
					);
					return;
				}
				else if(GameSetting.MODE_OFFLINEENABLED){
					string localStr = DataKey.GetPrefsString(name);
					if(!string.IsNullOrEmpty(localStr)){
						pdata= JsonConvert.DeserializeObject<PatientData>(localStr);
						PatientMgr.GetPatientList()[name] = pdata;
						if(successAction != null)
							successAction.Invoke(pdata);
					}
					else if(failAction != null)
						failAction.Invoke("Local patient data does not exist.");
				}
			}
		}
		else if(GameState.IsPatient()){//Home patient
			if(GameState.IsOnline){
				GetUserDataRequest request = new GetUserDataRequest();
				request.Keys = new List<string>(){DataKey.HOMEPATIENT};
				PlayFabClientAPI.GetUserData(request,
					result=>{
						if(result.Data != null && result.Data.ContainsKey(DataKey.HOMEPATIENT)){
							pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[DataKey.HOMEPATIENT].Value);
							Dictionary<string, PatientData> plist = PatientMgr.GetPatientList();
							plist[pdata.name] = pdata;
							request = new GetUserDataRequest();
							request.PlayFabId = GameState.DoctorID;
							request.Keys = new List<string>(){pdata.name};
							PlayFabClientAPI.GetUserData(request,
								result=>{
									if(result.Data != null && result.Data.ContainsKey(pdata.name)){
										HomePatientData hpdata = JsonConvert.DeserializeObject<HomePatientData>(result.Data[pdata.name].Value);
										PatientMgr.GetHomePatientDataList()[pdata.name] = hpdata;
										pdata.GetDataFromDoctorData(hpdata);
										if(successAction != null)
											successAction.Invoke(pdata);
									}
									else if(failAction != null)
										failAction.Invoke("Can not download local home patient data.");
								},
								error=>{
									if(failAction != null)
										failAction.Invoke("can not download local home patient data.");
								}
							);
						}
						else if(failAction != null)
							failAction.Invoke("Can not download patient data.");
					},
					error=>{
						if(failAction != null)
							failAction.Invoke(error.ToString());
					}
				);
			}
		}
	}
	
	public static void ClearPatientData(UnityAction successAction = null, UnityAction<string> failAction = null){
		if(GameState.currentPatient == null)
			return;
		string patientname = GameState.currentPatient.name;
		CalorimeterData.DeleteAllData();
		PatientData pd = GameState.currentPatient;
		pd.ResetData();
		PatientRecord patientRecord = PatientDataMgr.GetPatientRecord();
		if (patientRecord != null)
			patientRecord.Clear();
		if (GameState.IsDoctor() && pd.IsHome())
			PatientMgr.GetHomePatientDataList()[patientname] = new HomePatientData();
		Dictionary<string, string> requestData = new Dictionary<string, string>();
		if(GameState.IsDoctor()){//doctor
			requestData[patientname] = pd.IsClinic()?JsonConvert.SerializeObject(pd): JsonConvert.SerializeObject(PatientMgr.GetHomePatientDataList()[patientname]);
		}
		else{//home patient
			requestData[DataKey.HOMEPATIENT] = JsonConvert.SerializeObject(pd);
			requestData[DataKey.HOMECALIB] = JsonConvert.SerializeObject(pd.cali);
		}
		if (patientRecord != null && GameState.IsDoctor())//delete records for only clinic patients
			requestData[$"{patientname}{DataKey.SF_SESSIONRECORD}"] = JsonConvert.SerializeObject(patientRecord);

		PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
			{
				Data = requestData,
				Permission = UserDataPermission.Public
			},
			result => {
				if(successAction != null)
					successAction.Invoke();
			}, error => {
				if(failAction != null)
					failAction.Invoke(error.ToString());
			});
	}
}
