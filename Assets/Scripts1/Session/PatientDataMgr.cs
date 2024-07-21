using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using Org.BouncyCastle.Asn1.Mozilla;
using System.Runtime.Serialization;

[JsonObject(MemberSerialization.Fields)]
public class SessionRecord
{
	public List<GamePlay> games = new List<GamePlay>();
	public DateTime time;
	public ColorSet cali;

	[OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (games == null)
        {
            games = new List<GamePlay>();
        }
    }
}
[JsonObject(MemberSerialization.Fields)]
public class DiagnoseTestItem{//diagnose test item for one diagnostics game
	public int version;
	public List<string> strings = new List<string>();
	public DiagnoseTestItem(){
		version = GameVersion.DIAGNOSTICS;
	}

	[OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (strings == null)
        {
            strings = new List<string>();
        }
    }

	public void Clear(){
		strings.Clear();
	}

	public void AddValue(string str){
		strings.Add(str);
	}
}
[JsonObject(MemberSerialization.Fields)]
public class DiagnoseRecord{
	Dictionary<string, DiagnoseTestItem> diagnoseTestItems = new Dictionary<string, DiagnoseTestItem>();//key: test name
	public ColorSet cali;
	public void AddTestItem(string testname, DiagnoseTestItem item){
		diagnoseTestItems[testname] = item;
	}

	[OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (diagnoseTestItems == null)
        {
            diagnoseTestItems = new Dictionary<string, DiagnoseTestItem>();
        }

    }

	public Dictionary<string, DiagnoseTestItem> GetTestItems(){return diagnoseTestItems;}
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
	public Dictionary<string, DiagnoseRecord> diagnoseRecords = new Dictionary<string, DiagnoseRecord>();//with date string

	 [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if(sessionlist == null)
			sessionlist = new List<SessionRecord>();
		if(displacementRecords == null)
			displacementRecords = new List<DisplacementRecord>();
		if(diagnoseRecords == null)
			diagnoseRecords = new Dictionary<string, DiagnoseRecord>();
    }
	public void AddSessionRecord(SessionRecord record)
	{
		sessionlist.Add(record);
	}

	public void AddDiagnosRecord(string testname, DiagnoseTestItem diagnos){
		DateTime now = DateTime.Now;
		string datestr = now.ToString(GameConst.STRFORMAT_DATETIME);
		DiagnoseRecord record = null;
		if(diagnoseRecords.ContainsKey(datestr))
			record = diagnoseRecords[datestr];
		else{
			record = new DiagnoseRecord();
			diagnoseRecords[datestr] = record;
		}
		record.cali = ColorCalibration.GetCurrentColorSet();
		record.AddTestItem(testname, diagnos);
	}

	public List<SessionRecord> GetSessionRecordList()
	{
		if(GameConst.MODE_DOCTORTEST){
			Debug.LogError("Can not request session list in diagnostic mode");
			return new List<SessionRecord>();
		}
		return sessionlist;
	}

	public Dictionary<string, DiagnoseRecord> GetDiagnoseRecords(){
		return diagnoseRecords;
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
	public static void SavePatientData(UnityAction successAction = null, UnityAction<string> failAction = null)
	{
		if(GameState.currentPatient == null)
			return;
		if(GameState.currentPatient.place == THERAPPYPLACE.Home && GameState.IsDoctor()){
			Debug.LogError("Can not save Home patient data.");
			return;
		}
		string jsonstr = JsonConvert.SerializeObject(patientRecord);
		string keystr = GameState.currentPatient.name + DataKey.SF_SESSIONRECORD;
		DataKey.SetPrefsString(keystr, jsonstr);
		if (!GameState.IsOnline){
			if(successAction != null)
				successAction.Invoke();
		}
		else
		{
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
			{
				Data = new Dictionary<string, string>() { {keystr , jsonstr}},
				Permission = UserDataPermission.Public
			},
			result =>
			{
				if(successAction != null)
					successAction.Invoke();
			},
			error =>
			{
				if(failAction != null)
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
		string keystr = patientname + DataKey.SF_SESSIONRECORD;
		//offline mode
		if(pd.place == THERAPPYPLACE.Clinic || GameState.IsPatient()){
			string str = DataKey.GetPrefsString(keystr, "");
			if (!string.IsNullOrEmpty(str))
			{
				patientRecord = JsonConvert.DeserializeObject<PatientRecord>(str);
				if(!GameState.IsOnline){
					if(successAction != null)
						successAction.Invoke();
					return;
				}
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
				Keys = new List<string>() { keystr },
				PlayFabId = pfid
			},
		   result =>
		   {
			   if (result.Data != null && result.Data.ContainsKey(keystr))
			   {
					string str = result.Data[keystr].Value;
					if(pd.place == THERAPPYPLACE.Clinic || GameState.IsPatient())
						PlayerPrefs.SetString(keystr, str);
					if(string.IsNullOrEmpty(str))
						patientRecord = new PatientRecord();
					else
						patientRecord = JsonConvert.DeserializeObject<PatientRecord>(str);
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
