using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;


public class SessionRecord
{
	public List<GamePlay> games = new List<GamePlay>();
	public DateTime time;
	public ColorSet cali = new ColorSet();
}
public class DisplacementRecord
{
	public float aver_displace_left, aver_displace_right;
	public DateTime datetime;

	public DisplacementRecord(float aver_displace_left, float aver_displace_right)
	{
		this.aver_displace_left=aver_displace_left;
		this.aver_displace_right=aver_displace_right;
		this.datetime=DateTime.Now;
	}
}
[JsonObject(MemberSerialization.Fields)]
public class PatientRecord
{
	public List<SessionRecord> sessionlist = new List<SessionRecord>();
	public List<DisplacementRecord> displacementRecords = new List<DisplacementRecord>();
	public void AddSessionRecord(SessionRecord record)
	{
		sessionlist.Add(record);
	}

	public List<SessionRecord> GetSessionRecordList()
	{
		return sessionlist;
	}

	public void AddDisplacementRecord(DisplacementRecord record)
	{
		displacementRecords.Add(record);
	}

	public List<DisplacementRecord> GetDisplacementRecordList()
	{
		return displacementRecords;
	}
}
public abstract class PatientDataMgr
{
	static PatientRecord patientRecord = new PatientRecord();

	public static PatientRecord GetPatientRecord()
	{
		return patientRecord;
	}
	public static void AddSessionRecord(SessionRecord record)
	{
		patientRecord.AddSessionRecord(record);
	}
	public static void AddDisplacementRecord(DisplacementRecord record)
	{
		patientRecord.AddDisplacementRecord(record);
	}
	public static void SavePatientData(UnityAction successAction, UnityAction<string> failAction)
	{
		if (GameState.currentPatient == null || GameState.currentGameMode == GAMEMODE.SingleGame)
		{
			failAction.Invoke("There is no session data to record.");
			return;
		}
		if(GameState.currentPatient.place == THERAPPYPLACE.Home && GameState.IsDoctor()){
			Debug.LogError("Can not save Home patient data.");
			return;
		}
		string jsonstr = JsonConvert.SerializeObject(patientRecord);
		string keystr = GameState.currentPatient.name;
		DataKey.SetPrefsString(keystr, jsonstr);
		if (!GameState.IsOnline)
			successAction.Invoke();
		else
		{
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
			{
				Data = new Dictionary<string, string>() { {keystr , jsonstr}},
				Permission = UserDataPermission.Public
			},
			result =>
			{
				successAction.Invoke();
			},
			error =>
			{
				failAction.Invoke(error.ToString());
			});
		}
	}

	public static void LoadPatientData(string patientname, UnityAction successAction = null, UnityAction<string> failAction = null)
	{
		PatientData pd = PatientMgr.FindPatient(patientname);
		if(pd == null){
			failAction.Invoke($"Can not find {patientname}'s data.");
			return;
		}
		//offline mode
		if(pd.place == THERAPPYPLACE.Clinic || GameState.IsPatient()){
			string str = DataKey.GetPrefsString(patientname, "");
			if (!string.IsNullOrEmpty(str))
			{
				patientRecord = JsonConvert.DeserializeObject<PatientRecord>(str);
				if (patientRecord.displacementRecords == null)
					patientRecord.displacementRecords = new List<DisplacementRecord>();
				if (patientRecord.sessionlist == null)
					patientRecord.sessionlist = new List<SessionRecord>();
				if(successAction != null)
					successAction.Invoke();
				return;
			}
		}
		
		if (!GameState.IsOnline)
		{
			if(failAction != null)
				failAction.Invoke("Session data does not exist.");
		}
		else
		{
			string pfid = null;
			if(pd.place == THERAPPYPLACE.Home && GameState.IsDoctor())
				pfid = pd.PFID;
			PlayFabClientAPI.GetUserData(new GetUserDataRequest()
			{
				Keys = new List<string>() { patientname },
				PlayFabId = pfid
			},
		   result =>
		   {
			   if (result.Data != null && result.Data.ContainsKey(patientname))
			   {
					string str = result.Data[patientname].Value;
					if(pd.place == THERAPPYPLACE.Clinic || GameState.IsPatient())
						PlayerPrefs.SetString(patientname, str);
					if(string.IsNullOrEmpty(str))
						patientRecord = new PatientRecord();
					else
						patientRecord = JsonConvert.DeserializeObject<PatientRecord>(str);
					if (patientRecord.displacementRecords == null)
						patientRecord.displacementRecords = new List<DisplacementRecord>();
					if (patientRecord.sessionlist == null)
						patientRecord.sessionlist = new List<SessionRecord>();
					if (successAction != null)
						successAction.Invoke();
			   }
			   else if (failAction != null)
				   failAction.Invoke("Session data does not exist.");
		   },
		   error =>
		   {
			   if (failAction != null)
				   failAction.Invoke(error.ToString());
		   });
		}
	}

	

	public static void ClearSessionData()
	{
		patientRecord = new PatientRecord();
	}
}
