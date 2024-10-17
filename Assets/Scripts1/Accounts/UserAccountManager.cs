
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PlayFab;
using PlayFab.ClientModels;

using UnityEngine;
using UnityEngine.Events;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System;
using UnityEngine.TextCore.Text;
using System.Linq;
using System.Data;
using System.Net.NetworkInformation;  

public class UserAccountManager : MonoBehaviour
{
	
    const string PropName_NamePassHash = "NamePassHash";

    [SerializeField] TextMeshProUGUI errorText;
    public static UserAccountManager Instance;

    public static UnityEvent OnSignInSuccess = new UnityEvent();
    public static UnityEvent<string> OnSignInFailed = new UnityEvent<string>();
    public static UnityEvent<string> OnCreateAccountFailed = new UnityEvent<string>();
    public static UnityEvent<string, string> OnUserDataRetrieved = new UnityEvent<string, string>();
    public static UnityEvent<string, List<PlayerLeaderboardEntry>> OnLeaderboardRetrieved = new UnityEvent<string, List<PlayerLeaderboardEntry>>();
    public static UnityEvent<string, StatisticValue> OnStatisticRetrieved = new UnityEvent<string, StatisticValue>();

    public static UserAccountInfo userAccountInfo;
    public float _msgTime = 3;
    float _msgexpiretime;

    void Awake()
    {
        Instance = this;
		UtilityFunc.AppendToLog("---------------------------------Application Started------------------------------------------------");
		PatientMgr.ClearPatients();
    }

    private void Update()
    {
        if (_msgexpiretime > 0)
        {
            _msgexpiretime -= Time.deltaTime;
            if (_msgexpiretime < 0)
                errorText.text = "";
        }
    }








    string GetHashString(string str)
    {
        // Create a SHA256   
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    string GetNamePassHash(string username, string password)
    {
        return GetHashString(username + ":" + password + ":" + SystemInfo.deviceUniqueIdentifier);
    }

	void SignInWithUserRole(UnityAction successAction, UnityAction<string> failedAction){
		if(GameState.IsPatient()){//Home patient
			PlayFabClientAPI.GetUserData(new GetUserDataRequest(){Keys = new List<string>() {DataKey.DOCTORID, DataKey.HOMEPATIENT}},
			result =>{
				if(result.Data != null && result.Data.ContainsKey(DataKey.DOCTORID) && result.Data.ContainsKey(DataKey.HOMEPATIENT)){
					GameState.DoctorID = result.Data[DataKey.DOCTORID].Value;
					DataKey.SetPrefsString(DataKey.DOCTORID, GameState.DoctorID);
					PatientData pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[DataKey.HOMEPATIENT].Value);
					PatientMgr.AddPatientData(pdata);
					PlayFabClientAPI.GetUserData(new GetUserDataRequest(){Keys = new List<string>() { pdata.name}, PlayFabId = GameState.DoctorID},
					result =>{
						if(result.Data != null && result.Data.ContainsKey(pdata.name)){
							HomePatientData hdata = JsonConvert.DeserializeObject<HomePatientData>(result.Data[pdata.name].Value);
							pdata.GetDataFromDoctorData(hdata);
							PatientMgr.GetNameIDList()[pdata.name] = GameState.playfabID;
							successAction.Invoke();
						}
						else
							failedAction.Invoke("Can not download data from doctor account.");
					},
					error=>{
						failedAction.Invoke("Can not download data from doctor account.");
					});
				}
				else
					failedAction.Invoke("Can not download data.");
			},
			error=>{
				failedAction.Invoke(error.ToString());
			});
		}
		else{
			PlayFabClientAPI.GetUserData(new GetUserDataRequest()
			{
				Keys = new List<string>() { DataKey.CLINICLIMIT, DataKey.HOMELIMIT, DataKey.PATIENT }
			},
			result =>
			{
				if(result.Data != null && result.Data.ContainsKey(DataKey.CLINICLIMIT) && result.Data.ContainsKey(DataKey.HOMELIMIT) && result.Data.ContainsKey(DataKey.PATIENT)){
					try{
						GameState.CilinicLimit = int.Parse(result.Data[DataKey.CLINICLIMIT].Value);
						GameState.HomeLimit = int.Parse(result.Data[DataKey.HOMELIMIT].Value);
						DataKey.SetPrefsString(DataKey.CLINICLIMIT, GameState.CilinicLimit.ToString());
						DataKey.SetPrefsString(DataKey.HOMELIMIT, GameState.HomeLimit.ToString());
						DataKey.SetPrefsString(DataKey.PATIENT, result.Data[DataKey.PATIENT].Value);
						Dictionary<string, string> nameIDList = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Data[DataKey.PATIENT].Value);
						PatientMgr.SetNameIDList(nameIDList);
						Dictionary<string, PatientData> plist = new Dictionary<string, PatientData>();
						foreach(KeyValuePair<string, string> pair in nameIDList)
							plist[pair.Key] = null;
						//Upload local data
						UtilityFunc.AppendToLog("LocalKey.LASTONLINE: " + PlayerPrefs.GetString(LocalKey.LASTONLINE, "True"));
						/*if(PlayerPrefs.GetString(LocalKey.LASTONLINE, "True").ToLower() == "false"){
							UtilityFunc.AppendToLog("Uploading local data.");
							string jsonstr = DataKey.GetPrefsString(DataKey.PATIENT);
							if(string.IsNullOrEmpty(jsonstr))
								successAction.Invoke();
							else{
								UpdateUserDataRequest request = new UpdateUserDataRequest();
								request.Data = new Dictionary<string, string>();
								request.Permission = UserDataPermission.Public;
								request.Data.Add(DataKey.PATIENT, jsonstr);
								Dictionary<string, string> nameIDHash = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonstr);
								foreach(KeyValuePair<string, string>pair in nameIDHash){
									if(pair.Value == GameConst.PLAYFABID_CLINIC){
										string localPatientString = DataKey.GetPrefsString(pair.Key);
										if(string.IsNullOrEmpty(localPatientString))
											request.Data.Add(pair.Key, "{}");
										else
											request.Data.Add(pair.Key, localPatientString);
									}
								}
								PlayFabClientAPI.UpdateUserData(request,
									result => {
										
										successAction.Invoke();
									},
									error =>{
										successAction.Invoke();
									}
								);
							}
						}
						else*/
							successAction.Invoke();
					}
					catch(Exception ex){
						failedAction.Invoke($"Cilinic enrollment limit error.\n{ex.ToString()}");
						return;
					}
				}
				else{
					failedAction.Invoke("Cilinic enrollment limit error.");
					return;
				}
			},
			error =>{
				failedAction.Invoke("Cilinic enrollment limit error.");
				return;
			});
		}
	}

	void SignInOnline(UnityAction successAction, UnityAction<string> failedAction){
		
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
		Keys = new List<string>(){DataKey.ROLE, DataKey.EXPIREDATE}
		},
		result =>{
			if (result.Data != null && result.Data.ContainsKey(DataKey.ROLE) && result.Data.ContainsKey(DataKey.EXPIREDATE))
			{
				GameState.userRole = result.Data[DataKey.ROLE].Value == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
				Debug.Log($"UserRole:{GameState.userRole}");
				BackgroundTask.DebugString($"UserRole:{GameState.userRole}");
				DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
				DataKey.SetPrefsString(DataKey.EXPIREDATE, result.Data[DataKey.EXPIREDATE].Value);
				if(DateTime.TryParse(result.Data[DataKey.EXPIREDATE].Value, out GameState.ExpireDate)){
					PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
						if(result.Time > GameState.ExpireDate){
							failedAction.Invoke("License Expired.");
						}
						else{
							DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
							SignInWithUserRole(successAction, failedAction);
						}
						return;
					},
					error =>
					{
						failedAction.Invoke("Can not get server time.");
						return;
					});
				}
				else{
					failedAction.Invoke("Invalid date expiring format from server.");
					return;
				}
				
			}
			else{
				failedAction.Invoke("Can not get user role.");
			}
			return;
		},
		error=>{
			failedAction.Invoke("Can not get user role.");
			return;
		});
							
	}

	public void SignIn(string username, string password, UnityAction successAction, UnityAction<string> failedAction)
	{


		Debug.Log("Using name and password: " + username);
		
		string namepassHash = GetNamePassHash(username, password);
		
		
		GameState.username = username;
		GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));
		UtilityFunc.AppendToLog($"Trying sign in as {username}");
		//offline mode
	if(GameSetting.MODE_OFFLINEENABLED){
		if(PlayerPrefs.GetString(DataKey.GetPrefKeyName (PropName_NamePassHash), "") == namepassHash &&
		!string.IsNullOrEmpty(GameState.playfabID))
		{
			BackgroundTask.DebugString("Try Offline signin");
			GameState.password = password;
			GameState.IsOnline = false;
			string expdatestr = DataKey.GetPrefsString(DataKey.EXPIREDATE);
			BackgroundTask.DebugString($"expdatestr: {expdatestr}");
			if(!string.IsNullOrEmpty(expdatestr)){
				DateTime expdate;
				if(DateTime.TryParse(expdatestr, out expdate)){
					DateTime curdate = DateTime.Now;
					if(expdate >= curdate){
						//try connect
						UtilityFunc.AppendToLog("Trying connect to playfab.com");
						
						BackgroundTask.DebugString($"Trying connect to playfab.com");
						string host = "playfab.com";  
						System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();  
						try  
						{  
							PingReply reply = p.Send(host, 30000);  
							PlayerPrefs.SetString(LocalKey.LASTONLINE, reply.Status == IPStatus.Success?"True": "False");
							Debug.Log("Saved LastOnLine key: " + (reply.Status == IPStatus.Success?"True": "False"));
							UtilityFunc.AppendToLog("Saved LastOnLine key: " + (reply.Status == IPStatus.Success?"True": "False"));
							BackgroundTask.DebugString("Saved LastOnLine key: " + reply.Status);
							if (reply.Status == IPStatus.Success){
								Debug.Log("Internet is available.");
								BackgroundTask.DebugString("Internet is available.");
								//connected to internet
								PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
								{
									TitleId = PlayFabSettings.TitleId,
									Username = username,
									Password = password,
								}, result =>
								{
									GameState.IsOnline = true;
									GameState.playfabID = result.PlayFabId;
									PlayerPrefs.SetString(DataKey.GetPrefKeyName (DataKey.PLAYFABID), GameState.playfabID);
									SignInOnline(successAction, failedAction);
								}, error =>
								{
									Debug.Log($"<color=red>Unsuccessful Login with username and password</color>: " + error.ErrorMessage);
									failedAction.Invoke(error.ErrorMessage);
								});
							}
							else{
								Debug.Log("Internet is unavailable");
								successAction.Invoke();
							}
							return;
						}  
						catch (Exception e){
							UtilityFunc.AppendToLog(e.ToString());
							Debug.Log(e.ToString());
							GameState.IsOnline = false;
							GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));
							if(string.IsNullOrEmpty(GameState.playfabID)){
								failedAction.Invoke("Can not get User ID.");
								return;
							}
							GameState.username = username;
							string role = DataKey.GetPrefsString(DataKey.ROLE);
							if(string.IsNullOrEmpty(role)){
								failedAction.Invoke("Can not get User Role.");
								return;
							}
							GameState.userRole = role == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
							if(GameState.IsPatient()){
								GameState.DoctorID = DataKey.GetPrefsString(DataKey.DOCTORID);
								if(string.IsNullOrEmpty(GameState.DoctorID)){
									failedAction.Invoke("Can not get doctor ID.");
									return;
								}
							}
							else{
								string CILINICLIMITText = DataKey.GetPrefsString(DataKey.CLINICLIMIT, "0");
								string HOMELIMITText = DataKey.GetPrefsString(DataKey.HOMELIMIT, "0");
								try{
									GameState.CilinicLimit = int.Parse(CILINICLIMITText);
									GameState.HomeLimit = int.Parse(HOMELIMITText);
									successAction.Invoke();
								}
								catch(Exception ex){
									failedAction.Invoke($"Can not get Enrollment Limit Count:\n{ex.ToString()}");
								}
							}
							return;
						}  
					}
				}
			}
		}
	}
		//online mode
		GameState.username = GameState.playfabID = GameState.DoctorID = GameState.password = "";
		
		PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
		{
			TitleId = PlayFabSettings.TitleId,
			Username = username,
			Password = password,
		}, result =>
		{
			PlayerPrefs.SetString(LocalKey.LASTONLINE, "True");
			GameState.IsOnline = true;
			GameState.password = password;
			GameState.username = username;
			GameState.playfabID = result.PlayFabId;
			string namepassHash = GetNamePassHash(username, password);
			PlayerPrefs.SetString(DataKey.GetPrefKeyName (PropName_NamePassHash), namepassHash);
			SignInOnline(successAction, failedAction);
			
		}, error =>
		{
			Debug.Log($"<color=red>Unsuccessful Login with username and password</color>");
			failedAction.Invoke(error.ErrorMessage);
		});
	}
	void SignUpForDoctor(string fabID, UnityAction successAction, UnityAction<string> failedAction){
		string key = DataKey.EXPIREDATE;
		PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
			GameState.playfabID = fabID;
			DateTime servertime = result.Time;
			PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(){Keys = new List<string>() { DataKey.FreeLicenseDay }},
				result=>{
					if(result.Data != null && result.Data.ContainsKey(DataKey.FreeLicenseDay)){
						GameState.ExpireDate = servertime.Add(new TimeSpan(30, 0, 0, 0, 0));
						string timestr = GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME);
						DataKey.SetPrefsString(key, timestr);
						PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
							Data = new Dictionary<string, string>() { {key, timestr }, { DataKey.ROLE, USERROLE.DOCTOR.ToString()}}
						},
						result =>{
								successAction.Invoke();
							},
							error =>{
								failedAction.Invoke("Can not set doctor role.");
								return;
							});
					}
					else{
						failedAction.Invoke("Can not get title data.");
						return;
					}
				},
				error=>{
					failedAction.Invoke("Can not get title data.");
					return;
				});
			
		},
		error =>
		{
			failedAction.Invoke("Can not get server time.");
			return;
		});
	}

	public void SignUp(string licensekey, UnityAction successAction, UnityAction<string> failedAction)
	{
		
		GameState.IsOnline = false;
		Debug.Log("Using custom licenseKey: " + licensekey);

		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
		{
			CustomId = licensekey,
			TitleId = PlayFabSettings.TitleId,
			CreateAccount = false
		}, result =>
		{
			GameState.IsOnline = true;
			string fabID = result.PlayFabId;
			PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
			Keys = new List<string>(){DataKey.ROLE}
			},
			result =>{
				USERROLE role = USERROLE.DOCTOR;
				if (result.Data != null && result.Data.ContainsKey(DataKey.ROLE))
				{
					if(result.Data[DataKey.ROLE].Value == USERROLE.PATIENT.ToString())
						role = USERROLE.PATIENT;
					else
						role = USERROLE.DOCTOR;
					GameState.userRole = role;
					if(role == USERROLE.PATIENT){
						PlayFabClientAPI.GetUserData(new GetUserDataRequest()
						{
							Keys = new List<string>() { DataKey.DOCTORID, DataKey.HOMEPATIENT }
						},
						result =>
						{
							if(result.Data != null && result.Data.ContainsKey(DataKey.DOCTORID) && result.Data.ContainsKey(DataKey.HOMEPATIENT)){
								GameState.DoctorID = result.Data[DataKey.DOCTORID].Value;
								string str = result.Data[DataKey.HOMEPATIENT].Value;
								PatientData pdata = JsonConvert.DeserializeObject<PatientData>(str);
								PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
									if(result.Time > pdata.ExpireDate){
										failedAction.Invoke("License Expired.");
									}
									else{
										Debug.Log($"Accepted License Key");
										GameState.ExpireDate = pdata.ExpireDate;
										GameState.playfabID = fabID;
										successAction.Invoke();
									}
								},
								error =>
								{
									failedAction.Invoke("Can not get server time.");
								});
								return;
									
							}
							else{
								failedAction.Invoke("Can not get detail data.");
								return;
							}
						}, error =>{
							failedAction.Invoke("Can not get detail data.");
							return;
						});
					}
					else
						SignUpForDoctor(fabID, successAction, failedAction);
					return;
				}
				else{
					SignUpForDoctor(fabID, successAction, failedAction);
					return;
				}
			},
			error =>{
				failedAction.Invoke(error.ToString());
				return;
			});
		}, error =>
		{
			Debug.Log($"<color=red>{error.ToString()}</color>");
			failedAction.Invoke(error.ErrorMessage);
		});
	}

    public void ShowMessage(string msg)
    {
        errorText.text = msg;
        _msgexpiretime = _msgTime;
    }
    public void emailRecovery(string email)
    {
        PlayFabClientAPI.SendAccountRecoveryEmail(new SendAccountRecoveryEmailRequest()
        {
            Email = email,
            TitleId = PlayFabSettings.TitleId
        }, result =>
        {
            Debug.Log("Email sent!");

        }, error =>
        {
            Debug.Log($"Not sent");
            Debug.Log(error.ErrorMessage);
        });

    }

    //This is for registering the sign up user.
	public void SetUserNameAndPassword(string eMail, string username, string password, UnityAction successAction, UnityAction<string> failedAction)
	{

		AddUsernamePasswordRequest req = new AddUsernamePasswordRequest();
		req.Username = username;
		req.Password = password;
		req.Email = eMail;
		PlayFabClientAPI.AddUsernamePassword(req,
		result =>
		{
			GameState.username = username;
			RemoveLocalRecords();
			string namepassHash = GetNamePassHash(username, password);
			GameState.password = password;
			DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
			PlayerPrefs.SetString(DataKey.GetPrefKeyName (PropName_NamePassHash), namepassHash);
			SetDisplayName(username);
			PlayerPrefs.SetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID), GameState.playfabID);
			
			if(GameState.IsPatient()){//homepatient
				DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
				PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
					Keys = new List<string>() { DataKey.DOCTORID, DataKey.HOMEPATIENT }
				},
				result =>{
					if (result.Data != null && result.Data.ContainsKey(DataKey.DOCTORID) && result.Data.ContainsKey(DataKey.HOMEPATIENT))
					{
						PatientData pdata = JsonConvert.DeserializeObject<PatientData>(result.Data[DataKey.HOMEPATIENT].Value);	
						if(username != pdata.name){
							failedAction.Invoke($"Name is not matching with registered name.");
							return;
						}
						PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
						GameState.DoctorID = result.Data[DataKey.DOCTORID].Value;
						DataKey.SetPrefsString(DataKey.DOCTORID, GameState.DoctorID);
						DataKey.SetPrefsString(DataKey.HOMEPATIENT, result.Data[DataKey.HOMEPATIENT].Value);
						successAction.Invoke();						
						return;
					}
				},
				error =>{
					failedAction.Invoke(error.ToString());
					return;
				});
			}
			else{
				PatientMgr.ClearPatients();
				string jsonPatients = JsonConvert.SerializeObject(PatientMgr.GetNameIDList());
				GameState.CilinicLimit = GameState.HomeLimit = 5;
				PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
					Data = new Dictionary<string, string>() { {DataKey.CLINICLIMIT, GameState.CilinicLimit.ToString()}
					, {DataKey.HOMELIMIT, GameState.HomeLimit.ToString()}
					, {DataKey.PATIENT, jsonPatients}}
				},
				result =>{
					PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
					successAction.Invoke();
					DataKey.SetPrefsString(DataKey.CLINICLIMIT, GameState.CilinicLimit.ToString());
					DataKey.SetPrefsString(DataKey.HOMELIMIT, GameState.HomeLimit.ToString());
					DataKey.SetPrefsString(DataKey.PATIENT, jsonPatients);
					return;
				},
				error =>{
					failedAction.Invoke(error.ToString());
					return;
				});
			}
		},
		error =>
		{
			failedAction.Invoke(error.ToString());
		});
	}

    

    /* void etUserRole(UnityAction<USERROLE> resultAction)
    {
        string key = DataKey.ROLE;
        string regValue = DataKey.GetPrefsString(key);
        if (regValue == USERROLE.PATIENT.ToString())
        {
            GameState.userRole = USERROLE.PATIENT;
            Debug.Log($"UserRole: " + USERROLE.PATIENT);
            return;
        }
        else if (regValue == USERROLE.DOCTOR.ToString())
        {
            GameState.userRole = USERROLE.DOCTOR;
            Debug.Log($"UserRole: " + USERROLE.DOCTOR);
            return;
        }
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            Keys = new List<string>() { key }
        },
        result => {
            if (result.Data != null && result.Data.ContainsKey(key))
            {
                if (result.Data[key].Value == USERROLE.PATIENT.ToString())
                {
                    resultAction.Invoke(USERROLE.PATIENT);
                }
                else
                {
                    resultAction.Invoke(USERROLE.DOCTOR);
                }
            }
            else
            {
                resultAction.Invoke(USERROLE.DOCTOR);
                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string>() { { key, USERROLE.DOCTOR.ToString() } }
                },
                result => { },
                error => { });
            }
        },
        error => {
            resultAction.Invoke(USERROLE.DOCTOR);
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string>() { { key, USERROLE.DOCTOR.ToString() } }
            },
            result => { },
            error => { });
        });
    } */

    void OnGetUserRole(USERROLE role)
    {
        GameState.userRole = role;
        DataKey.SetPrefsString(DataKey.ROLE, role.ToString());
        Debug.Log($"UserRole: " + role);
    }



    bool GetDeviceId(out string android_id, out string ios_id, out string custom_id)
    {
        android_id = string.Empty;
        ios_id = string.Empty;
        custom_id = string.Empty;

        if (CheckForSupportedMobilePlatform())
        {
#if UNITY_ANDROID
            //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
            AndroidJavaClass clsUnity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
            AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject> ("currentActivity");
            AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject> ("getContentResolver");
            AndroidJavaClass clsSecure = new AndroidJavaClass ("android.provider.Settings$Secure");
            android_id = clsSecure.CallStatic<string> ("getString", objResolver, "android_id");
#endif

#if UNITY_IPHONE
            ios_id = UnityEngine.iOS.Device.vendorIdentifier;
#endif
            return true;
        }
        else
        {
            custom_id = SystemInfo.deviceUniqueIdentifier;
            return false;
        }
    }

    bool CheckForSupportedMobilePlatform()
    {
        return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }

    /*
        DISPLAYNAME
    */

    void CheckDisplayName(string username, UnityAction completeAction)
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
        {
            //PlayFabId = playfabID;
        }, result => {
            userAccountInfo = result.AccountInfo;

            if (result.AccountInfo.TitleInfo.DisplayName == null || result.AccountInfo.TitleInfo.DisplayName.Length == 0)
            {
                PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
                {
                    DisplayName = username
                }, result => {
                    Debug.Log($"Display name set to username");
                    completeAction.Invoke();
                }, error => {
                    Debug.Log($"Display name could not be set to username | {error.ErrorMessage}");
                });
            }
            else
            {
                completeAction.Invoke();
            }
        }, error => {
            Debug.Log($"Could not retrieve AccountInfo | {error.ErrorMessage}");
        });
    }

    public void SetDisplayName(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = displayName
        }, result => {
            Debug.Log($"Display name set to username");
        }, error => {
            Debug.Log($"Display name could not be set to username | {error.ErrorMessage}");
        });
    }

    /*
        USERDATA
    */

    public void GetUserData(string key)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            //PlayFabId = playfabID,;
            Keys = new List<string>() {
                    key
                }
        }, result => {
            Debug.Log($"User data retrieved successfully");
            if (result.Data.ContainsKey(key)) OnUserDataRetrieved.Invoke(key, result.Data[key].Value);
            else OnUserDataRetrieved.Invoke(key, null);
        }, error => {
            Debug.Log($"Could not retrieve user data | {error.ErrorMessage}");
        });
    }

    public void SetUserData(string key, string userData)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() { { key, userData }
            }
        }, result => {
            Debug.Log($"{key} successfully updated");
        }, error => {
            Debug.Log($"{key} update unsuccessful | {error.ErrorMessage}");
        });
    }

    /*
        STATISTICS & LEADERBOARDS
    */

    public void GetStatistic(string statistic)
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest()
        {
            StatisticNames = new List<string>() {
                statistic
            }
        }, result => {
            if (result.Statistics.Count > 0)
            {
                Debug.Log($"Successfully got {statistic} | {result.Statistics[0]}");
                if (result.Statistics != null) OnStatisticRetrieved.Invoke(statistic, result.Statistics[0]);
            }
            else
            {
                Debug.Log($"No existing statistic [{statistic}] for user");
            }
        }, error => {
            Debug.Log($"Could not retrieve {statistic} | {error.ErrorMessage}");
        });
    }

    public void SetStatistic(string statistic, int value)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>() {
                new StatisticUpdate () {
                    StatisticName = statistic,
                        Value = value
                }
            }
        }, result => {
            Debug.Log($"{statistic} successfully updated");
            GetLeaderboard(statistic);
        }, error => {
            Debug.Log($"{statistic} update unsuccessful | {error.ErrorMessage}");
        });
    }

    public void GetLeaderboard(string statistic)
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest()
        {
            StatisticName = statistic
        }, result => {
            Debug.Log($"Successfully got {statistic} leaderboard | 0.{result.Leaderboard[0].DisplayName} {result.Leaderboard[0].StatValue}");
            OnLeaderboardRetrieved.Invoke(statistic, result.Leaderboard);
        }, error => {
            Debug.Log($"Could not retrieve {statistic} leaderboard | {error.ErrorMessage}");
        });
    }

    /*public void GetLeaderboardDelayed(string statistic)
    {
        StartCoroutine(CheckLeaderboardDelay(statistic));
    }


    IEnumerator CheckLeaderboardDelay(string statistic)
    {
        yield return new WaitForSeconds(3);
        GetLeaderboard(statistic);
    }

    void DownloadUserData()
    {
		if(GameState.IsDoctor()){
			string datastr = DataKey.GetPrefsString(DataKey.PATIENT);
			if (!string.IsNullOrEmpty(datastr))
				return;
			GetUserDataRequest request = new GetUserDataRequest();
			request.Keys = new List<string>(){DataKey.PATIENT};
			PlayFabClientAPI.GetUserData(request,
				result =>
				{
					if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
					{
						string str = result.Data[DataKey.PATIENT].Value;
						DataKey.SetPrefsString(DataKey.PATIENT, str);

						Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
						foreach (KeyValuePair<string, PatientData> pair in plist)
						{
							PatientDataMgr.LoadPatientData(pair.Key);
						}
					}
				},
				error =>
				{
					//failAction.Invoke(error.ToString());
				}
			);
		}
        
    }

     void UploadUserData()
    {
        // if(!GameState.IsOnline || GameState.IsPatient())
        // 	return;
        string jsonstr = DataKey.GetPrefsString(DataKey.PATIENT);
        if (string.IsNullOrEmpty(jsonstr))
            return;
        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = new Dictionary<string, string>();
        request.Data.Add(DataKey.PATIENT, jsonstr);
        request.Permission = UserDataPermission.Public;
        PlayFabClientAPI.UpdateUserData(request,
            result => {
                Dictionary<Int32, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<Int32, PatientData>>(jsonstr);
                foreach (KeyValuePair<Int32, PatientData> pair in plist)
                {
                    UpdateUserDataRequest patienrrequest = new UpdateUserDataRequest();
                    patienrrequest.Data = new Dictionary<string, string>();
                    patienrrequest.Data.Add(pair.Value.name, DataKey.GetPrefsString(pair.Value.name));
                    PlayFabClientAPI.UpdateUserData(patienrrequest,
                        result => { },
                        error => { }
                    );
                }
            },
            error => {
            }
        );
    } */

    void RemoveLocalRecords()
    {

        DataKey.DeletePrefsKey(DataKey.PATIENT);
        DataKey.DeletePrefsKey(DataKey.ROLE);
        DataKey.DeletePrefsKey(DataKey.EXPIREDATE);
        DataKey.DeletePrefsKey(DataKey.HOMECALIB);
        DataKey.DeletePrefsKey(DataKey.DOCTORID);
   		DataKey.DeletePrefsKey(DataKey.CLINICLIMIT);
		PatientMgr.ClearPatients();
    }
}