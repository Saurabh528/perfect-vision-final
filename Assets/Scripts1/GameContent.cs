using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Org.BouncyCastle.Asn1.Mozilla;
using Newtonsoft.Json.Linq;

public abstract class GameConst
{
	public const int MAX_THERAPHYGAMECOUNT = 6;
	public const string PATIENTNAME_ANONYMOUS = "Anonymous";
	public const string STRFORMAT_DATETIME = "yyyy-MM-dd";
	public const string PYARG_CAMERAINDEX = "cameraindex";
	public const string PYARG_PATIENTNAME = "patientname";
	public const string PLAYFABID_CLINIC = "0000000000000000";
}

public enum ColorChannel
{
	CC_Background,
	CC_Red,
	CC_Cyan,
	CC_RedOrCyan
}

public enum IRISSIMCLASS
{
	LELD = 0,
	LERD,
	RELD,
	RERD
}

public enum USERROLE{
	DOCTOR,
	PATIENT
}
public class IrisState
{
	public float LeLd, LeRd, ReLd, ReRd;
	
}

public class ColorSet
{
	public UInt32 red, cyan, back, slider;//slider: 00, Red, Cyan, Back(%)
	
}
public abstract class GameState
{
	
	public static PatientData currentPatient;
	public static GAMEMODE currentGameMode;
	public static GamePlay currentGamePlay;
	public static Int32 currentSessionPlayIndex;
	public static string username;
	static string _playfabID;
	public static string playfabID{
		get{
			return _playfabID;
		}
		set{
			_playfabID = value;
			StringEncrypter.SetKey(value);
        }
    }
	//public string playfabID2 = ;
	public static string password = "";
	public static bool MODE_DOCTORTEST = false;
	public static USERROLE userRole;
	public static string DoctorID;
	public static DateTime ExpireDate;
	public static bool IsOnline;
	public static int CilinicLimit;//Count limit of cilinic patients enrollment
	public static int HomeLimit;//Count limit of home patients enrollment

	public static bool IsDoctor(){
		return userRole == USERROLE.DOCTOR;
	}

	public static bool IsPatient(){
		return userRole == USERROLE.PATIENT;
	}
}

/* public abstract class HomeGameState{
	public static HomePatientData homePatientData;
} */

public abstract class PatientMgr
{
	static Dictionary<string, PatientData> patientList = new Dictionary<string, PatientData>();
	static Dictionary<string, string> nameIDList = new Dictionary<string, string>();//<name, playfabID>
	static Dictionary<string, HomePatientData> homePatientDataList = new Dictionary<string, HomePatientData>();//{name, HomePatientData}--updated by doctor

	public static Dictionary<string, PatientData> GetPatientList()
	{
		return patientList;
	}

	public static void SetNameIDList(Dictionary<string, string> list){
		nameIDList = list;
	}

	public static Dictionary<string, HomePatientData> GetHomePatientDataList(){
		return homePatientDataList;
	}



	public static Dictionary<string, string> GetNameIDList(){
		return nameIDList;
	}

	public static void SetPatientList(Dictionary<string, PatientData> plist)
	{
		if (patientList == plist)
			return;
		if (patientList != null)
			patientList.Clear();
		patientList = plist;
	}

	public static void RemovePatient(string name)
	{
		if (patientList.ContainsKey(name))
			patientList.Remove(name);
		if(nameIDList.ContainsKey(name))
			nameIDList.Remove(name);
		if(homePatientDataList.ContainsKey(name))
			homePatientDataList.Remove(name);
	}

	public static void ClearPatients(){
		patientList.Clear();
		nameIDList.Clear();
		homePatientDataList.Clear();
	}


	public static void AddPatientData(PatientData data)
	{
		patientList[data.name] = data;
		nameIDList[data.name] = data.PFID;
		homePatientDataList[data.name] = new HomePatientData();
	}

	public static void UpdatePatientData(PatientData data)
	{
		if (!patientList.ContainsKey(data.name))
			return;
		patientList[data.name] = data;
	}

	public static PatientData FindPatient(string name)
	{
		if (!patientList.ContainsKey(name))
			return null;
		return patientList[name];
	}


	public static string GetPatientDataDir()
	{
		return Application.dataPath + "/../Python/PatientData/" + GetCurrentPatientName();
	}

	public static string GetCurrentPatientName()
	{
		return GameState.currentPatient == null ? GameConst.PATIENTNAME_ANONYMOUS : GameState.currentPatient.name;
	}

	public static string GetTherapyResultDir()
	{
		return Application.persistentDataPath + "/" + "TherapyReport/";
	}

	public static string GetDiagnoseResultDir()
	{
		return Application.persistentDataPath + "/DiagnoseReport/";
	}

}

public abstract class SessionMgr
{
	static List<byte> gamelist = new List<byte>();
	static string[] gamenames = new string[] { "Ballon Burst", "Ping Pong", "Shape Change", "Color Switch", "Juggling", "Plane Game", "Crane Game", "FlapNFly"};
	static string[] gameScenenames = new string[] { "BallonBurst", "PingPong", "ShapeChange", "ColorSwitch", "Juggling", "Plane", "Crane2D", "FlapNFly"};
	static SessionRecord sessionRecord;
	public static int _timeSecond = 120;
	public static bool AddGame(byte gameid, out string error)
	{
		error = "";
		if (gamelist.Count >= 6)
		{
			error = "Can not place 6+ games.";
			return false;
		}
		foreach(byte id in gamelist){
			if(id == gameid){
				error = $"{gamenames[gameid]} is already selected.";
				return false;
			}
		}
		gamelist.Add(gameid);
		SaveTherapygameList();
		return true;
	}

	public static List<byte> GetGameList()
	{
		return gamelist;
	}

	public static string GetGameName(byte index)
	{
		if (index < gamenames.Length)
			return gamenames[index];
		else
			return "";
	}

	public static string[] GetGameNames()
	{
		return gamenames;
	}

	public static string[] GetGameSceneNames() {
		return gameScenenames;
	}


	public static bool RemoveGame(string name, out string error)
	{
		error = "";
		foreach (byte index in gamelist){
			if(gamenames[index] == name)
			{
				gamelist.Remove(index);
				SaveTherapygameList();
				return true;
			}
		}
		error = "Can not find game: " + name + ".";
		return false;
	}

	static void SaveTherapygameList(){
		if(GameState.currentPatient == null)
			return;
		GameState.currentPatient.therapygames.Clear();
		List<byte> games = SessionMgr.GetGameList();
		foreach (byte gameindex in games)
			GameState.currentPatient.therapygames.Add(gameindex);
		if(GameState.currentPatient.IsHome())
			GameState.currentPatient.PutDataToDoctorData(PatientMgr.GetHomePatientDataList()[GameState.currentPatient.name]);
		PatientDataManager.UpdatePatient(GameState.currentPatient);
	}

	public static void StartSession(int timeSecond)
	{
		_timeSecond = timeSecond;
		GameState.currentGameMode = GAMEMODE.SessionGame;
		GameState.currentSessionPlayIndex = 0;
		sessionRecord = new SessionRecord();
		sessionRecord.cali.back = UtilityFunc.Color2Int(ColorCalibration.BackColor);
		sessionRecord.cali.red = UtilityFunc.Color2Int(ColorCalibration.RedColor);
		sessionRecord.cali.cyan = UtilityFunc.Color2Int(ColorCalibration.CyanColor);
		sessionRecord.cali.slider = ((uint)((GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Red), 0.5f) : GameState.currentPatient.cali.rd) * 100) << 16) +
			((uint)((GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Cyan), 0.5f) : GameState.currentPatient.cali.cy) * 100) << 8) +
			(uint)((GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName (ColorCalibration.PrefName_Red), 0.5f) : GameState.currentPatient.cali.rd) * 100);
		StartSessionGame(GameState.currentSessionPlayIndex);
	}

	public static void StartSessionGame(Int32 gameindex)
	{
		if (gameindex >= gamelist.Count)
			return;
		GamePlay gamePlay = new GamePlay();
		gamePlay.name = gamenames[gamelist[gameindex]];
		sessionRecord.time = DateTime.Now;
		sessionRecord.games.Add(gamePlay);
		GameState.currentGamePlay = gamePlay;
		ChangeScene.LoadScene(gameScenenames[gamelist[gameindex]]);
	}

	public static void CancelCurrentSessionGame()
	{
		if (GameState.currentGamePlay == null)
			return;
		if (sessionRecord.games.Contains(GameState.currentGamePlay))
		{
			sessionRecord.games.Remove(GameState.currentGamePlay);
			GameState.currentGamePlay = null;
		}
	}

	public static void AddSessionRecordToData()
	{
		PatientDataMgr.AddSessionRecord(sessionRecord);
		sessionRecord = null;
		GameState.currentGamePlay = null;
	}

	public static bool NeedToAddSessionRecord()
	{
		return sessionRecord != null && GameState.currentPatient != null && sessionRecord.games.Count != 0;
	}
}