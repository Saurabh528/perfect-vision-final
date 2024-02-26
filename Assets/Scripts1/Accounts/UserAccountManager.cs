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

	string GetNamePassHash(string username, string password) {
		return GetHashString(username + ":" + password + ":" + SystemInfo.deviceUniqueIdentifier);
	}

	void SignInWithUserRole(UnityAction successAction, UnityAction<string> failedAction){
	    UploadUserData();
			successAction.Invoke();
		// if(GameState.IsPatient()){
		// 	successAction.Invoke();
		// }
		// else{
		// 	UploadUserData();
		// 	successAction.Invoke();
		// 	return;
		// }
	}

	void SignInOnline(UnityAction successAction, UnityAction<string> failedAction){
		SignInWithUserRole(successAction, failedAction);
		// string key = DataKey.ROLE;
		// PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
		// Keys = new List<string>(){key}
		// },
		// result =>{
		// 	if (result.Data != null && result.Data.ContainsKey(key))
		// 	{
		// 		GameState.userRole = result.Data[key].Value == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
		// 		DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
		// 		if(GameState.IsDoctor()){
		// 			key = DataKey.EXPIREDATE;
		// 			PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest()
		// 			{
		// 				Keys = new List<string>() { key }
		// 			},
		// 			result =>
		// 			{
		// 				if (result.Data != null && result.Data.ContainsKey(key))
		// 				{
		// 					string str = result.Data[key].Value;
		// 					DateTime expiredate;
		// 					if(DateTime.TryParse(str, out expiredate)){
		// 						DataKey.SetPrefsString(DataKey.EXPIREDATE, str);
		// 						PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
		// 							if(result.Time > expiredate){
		// 								failedAction.Invoke("License Expired.");
		// 							}
		// 							else{
		// 								SignInWithUserRole(successAction, failedAction);
		// 								return;
		// 							}
		// 							return;
		// 						},
		// 						error =>
		// 						{
		// 							failedAction.Invoke("Can not get server time.");
		// 							return;
		// 						});
		// 						return;
		// 					}
		// 					else{
		// 						failedAction.Invoke("Invalid date expiring format.");
		// 						return;
		// 					}
		// 				}
		// 				else{
		// 					failedAction.Invoke("Missing License lifetime data.");
		// 					return;
		// 				}
		// 			},
		// 			error =>
		// 			{
		// 				failedAction.Invoke("Missing License lifetime data.");
		// 				return;
		// 			});
		// 		}
		// 		else{
		// 			key = DataKey.DOCTORID;
		// 			PlayFabClientAPI.GetUserData(new GetUserDataRequest()
		// 			{
		// 				Keys = new List<string>() { key }
		// 			},
		// 			result =>
		// 			{
		// 				if(result.Data != null && result.Data.ContainsKey(key)){
		// 					GameState.DoctorID = result.Data[key].Value;
		// 					DataKey.SetPrefsString(key, GameState.DoctorID);
		// 					GetUserDataRequest request = new GetUserDataRequest();
		// 					request.Keys = new List<string>();
		// 					request.Keys.Add(DataKey.PATIENT);
		// 					request.PlayFabId = GameState.DoctorID;
		// 					PlayFabClientAPI.GetUserData(request,
		// 						result =>
		// 						{
		// 							if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
		// 							{
		// 								string str = result.Data[DataKey.PATIENT].Value;
		// 								Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
		// 								foreach(KeyValuePair<string, PatientData> pair in plist)
		// 								{
		// 									if(pair.Value.PFID == GameState.playfabID){
		// 										PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
		// 											if(result.Time > pair.Value.ExpireDate){
		// 												failedAction.Invoke("License Expired.");
		// 											}
		// 											else{
		// 												Debug.Log($"Accepted License Key");
		// 												GameState.ExpireDate = pair.Value.ExpireDate;
		// 												DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
		// 												SignInWithUserRole(successAction, failedAction);
		// 											}
		// 										},
		// 										error =>
		// 										{
		// 											failedAction.Invoke("Can not get server time.");
		// 										});
		// 										return;
		// 									}
		// 								}
										
		// 								RemoveLocalRecords();
		// 								failedAction.Invoke("Can not find patient data.");
		// 								return;
		// 							}
		// 						},
		// 						error =>
		// 						{
		// 							failedAction.Invoke(error.ToString());
		// 							return;
		// 						}
		// 					);
		// 					return;
		// 				}
		// 				else{
		// 					failedAction.Invoke("Can not get doctor ID.");
		// 					return;
		// 				}
		// 			}, error =>{
		// 				failedAction.Invoke("Can not get doctor ID.");
		// 				return;
		// 			});
		// 		}
		// 	}
		// 	else{
		// 		failedAction.Invoke("Can not get user role.");
		// 	}
		// 	return;
		// },
		// error=>{
		// 	failedAction.Invoke("Can not get user role.");
		// 	return;
		// });
							
	}

	public void SignIn(string username, string password, UnityAction successAction, UnityAction<string> failedAction)
	{


		Debug.Log("Using name and password: " + username);
		
		//string namepassHash = GetNamePassHash(username, password);
		// string pwdhash = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
		
		
		//GameState.username = username;
		//GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));

		//offline mode
		// if(PlayerPrefs.GetString(DataKey.GetPrefKeyName (PropName_NamePassHash), "") == namepassHash &&
		// !string.IsNullOrEmpty(GameState.playfabID))
		// {
		// 	GameState.passwordhash = pwdhash;
		// 	GameState.IsOnline = false;
		// 	string expdatestr = DataKey.GetPrefsString(DataKey.EXPIREDATE);
		// 	if(!string.IsNullOrEmpty(expdatestr)){
		// 		DateTime expdate;
		// 		if(DateTime.TryParse(expdatestr, out expdate)){
		// 			DateTime curdate = DateTime.Now;
		// 			if(expdate >= curdate){
		// 				//try connect
		// 				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
		// 				{
		// 					TitleId = PlayFabSettings.TitleId,
		// 					Username = username,
		// 					Password = GetHashString(password + SystemInfo.deviceUniqueIdentifier),
		// 				}, result =>
		// 				{
		// 					GameState.IsOnline = true;
		// 					GameState.playfabID = result.PlayFabId;
		// 					PlayerPrefs.SetString(DataKey.GetPrefKeyName (DataKey.PLAYFABID), GameState.playfabID);
		// 					SignInOnline(successAction, failedAction);
		// 					return;
		// 				}, error =>
		// 				{
		// 					GameState.IsOnline = false;
		// 					GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));
		// 					if(string.IsNullOrEmpty(GameState.playfabID)){
		// 						failedAction.Invoke("Can not get User ID.");
		// 						return;
		// 					}
		// 					GameState.username = username;
		// 					string role = DataKey.GetPrefsString(DataKey.ROLE);
		// 					if(string.IsNullOrEmpty(role)){
		// 						failedAction.Invoke("Can not get User Role.");
		// 						return;
		// 					}
		// 					GameState.userRole = role == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
		// 					if(GameState.IsPatient()){
		// 						GameState.DoctorID = DataKey.GetPrefsString(DataKey.DOCTORID);
		// 						if(string.IsNullOrEmpty(GameState.DoctorID)){
		// 							failedAction.Invoke("Can not get doctor ID.");
		// 							return;
		// 						}
		// 					}
		// 					successAction.Invoke();
		// 					Debug.Log($"<color=red>Unsuccessful connect to Playfab</color>");
		// 				});
		// 				return;
		// 			}
		// 		}
		// 	}
		// }

		
		//online mode
		GameState.username = GameState.playfabID = GameState.DoctorID = GameState.passwordhash = "";
		Debug.Log(username);
		Debug.Log(password);
		string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
		if(Regex.IsMatch(username, pattern))
		{
			PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest()
		{
			TitleId = PlayFabSettings.TitleId,
		    Email = username,
			Password = password,
		}, result =>
		{
			// GameState.IsOnline = true;
			// GameState.passwordhash = pwdhash;
			// GameState.username = username;
			// GameState.playfabID = result.PlayFabId;
			SignInOnline(successAction, failedAction);
			
		}, error =>
		{
			Debug.Log($"<color=yellow>Unsuccessful Login with email and password</color>");
			failedAction.Invoke(error.ErrorMessage);
			
		});


		}
		else
		{
				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
				{
					TitleId = PlayFabSettings.TitleId,
					Username = username,
					Password = password,
				}, result =>
				{
					// GameState.IsOnline = true;
					// GameState.passwordhash = pwdhash;
					// GameState.username = username;
					// GameState.playfabID = result.PlayFabId;
					SignInOnline(successAction, failedAction);
					
				}, error =>
				{
					Debug.Log($"<color=green>Unsuccessful Login with username and password</color>");
					failedAction.Invoke(error.ErrorMessage);
				});
		}
	}
	void SignUpForDoctor(string fabID, UnityAction successAction, UnityAction<string> failedAction)
	{
		string key = DataKey.EXPIREDATE;
		PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest()
		{
			Keys = new List<string>() { key }
		},
		result =>
		{
			successAction.Invoke();
			// if (result.Data != null && result.Data.ContainsKey(key))
			// {
			// 	string str = result.Data[key].Value;
			// 	DateTime expiredate;
			// 	// if(DateTime.TryParse(str, out expiredate)){
			// 		PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
			// 			// if(result.Time > expiredate){
			// 			// 	failedAction.Invoke("License Expired.");
			// 			// }
			// 			// else{
			// 				Debug.Log($"Accepted License Key");
			// 				GameState.playfabID = fabID;
			// 				GameState.ExpireDate = expiredate;
			// 				key = DataKey.ROLE;
			// 				PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
			// 					Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
			// 				},
			// 				result =>{
			// 					successAction.Invoke();
			// 				},
			// 				error =>{
			// 					failedAction.Invoke("Can not set doctor role.");
			// 					return;
			// 				});
							
			// 			// }
			// 			return;
			// 		},
			// 		error =>
			// 		{
			// 			failedAction.Invoke("Can not get server time.");
			// 			return;
			// 		});
			// 		return;
			// 	}
			// 	// else{
			// 	// 	failedAction.Invoke("Invalid date expiring format.");
			// 	// 	return;
			// 	// }
			// }
			// else{
			// 	failedAction.Invoke("Missing License lifetime data1.");
			// 	return;
			// }
		},
		error =>
		{
			failedAction.Invoke("Missing License lifetime data2.");
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
			string key = DataKey.ROLE;
            //PlayFabServerAPI.UpdateUserReadOnlyData(new UpdateUserDataRequest()
            //{
            //	Data = new Dictionary<string, string>() {
            //	 {"ExpiryDate", "2024-12-30"}
            //	 },
            //	Permission = UserDataPermission.Public

            //},
            // result => Debug.Log("Successfully updated user data"),
            // error =>
            // {
            //	 Debug.Log("Got error setting expiry date");
            //	 Debug.Log(error.GenerateErrorReport());
            // });
            PlayFabServerAPI.UpdateUserReadOnlyData(new PlayFab.ServerModels.UpdateUserDataRequest
            {
                PlayFabId = fabID, // Ensure this is the correct PlayFab ID for the user
                Data = new Dictionary<string, string> {
                { "ExpiryDate", "2024-12-30" }
                },
                Permission = PlayFab.ServerModels.UserDataPermission.Public
            },
           result => Debug.Log("Successfully updated user data"),
           error =>
           {
               Debug.Log("Got error setting expiry date");
               Debug.Log(error.GenerateErrorReport());
           });

            GetUserExpiryDate(fabID);

            PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
			Keys = new List<string>(){key}
			},
			result =>{
				USERROLE role = USERROLE.DOCTOR;
				if (result.Data != null && result.Data.ContainsKey(key))
				{
					if(result.Data[key].Value == USERROLE.PATIENT.ToString())
						role = USERROLE.PATIENT;
					else
						role = USERROLE.DOCTOR;
					GameState.userRole = role;
					if(role == USERROLE.PATIENT){
						key = DataKey.DOCTORID;
						PlayFabClientAPI.GetUserData(new GetUserDataRequest()
						{
							Keys = new List<string>() { key }
						},
						result =>
						{
							if(result.Data != null && result.Data.ContainsKey(key)){
								GameState.DoctorID = result.Data[key].Value;
								GetUserDataRequest request = new GetUserDataRequest();
								request.Keys = new List<string>();
								request.Keys.Add(DataKey.PATIENT);
								request.PlayFabId = GameState.DoctorID;
								PlayFabClientAPI.GetUserData(request,
									result =>
									{
										if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
										{
											string str = result.Data[DataKey.PATIENT].Value;
											Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
											foreach(KeyValuePair<string, PatientData> pair in plist)
											{
												if(pair.Value.PFID == fabID){
													PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
														if(result.Time > pair.Value.ExpireDate){
															failedAction.Invoke("License Expired.");
														}
														else{
															Debug.Log($"Accepted License Key");
															GameState.ExpireDate = pair.Value.ExpireDate;
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
											}
											failedAction.Invoke("Can not find patient data.");
											return;
										}
									},
									error =>
									{
										failedAction.Invoke(error.ToString());
										return;
									}
								);
								successAction.Invoke();
								return;
							}
							else{
								failedAction.Invoke("Can not get doctor ID.");
								return;
							}
						}, error =>{
							failedAction.Invoke("Can not get doctor ID.");
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
    public void GetUserExpiryDate(string playFabId)
    {
		Debug.Log("GetUserExpiryDate function called");
		Debug.Log("PlayFab ID is" + playFabId);
        PlayFabClientAPI.GetUserData(new GetUserDataRequest
        {
            PlayFabId = playFabId, // Optional for the current user
            Keys = null // Passing null fetches all user data; specify keys to fetch specific data
        },

        result => {
            if (result.Data == null || !result.Data.ContainsKey("ExpiryDate"))
                Debug.Log("No Expiry Date set for user.");
            else
                Debug.Log("Expiry Date: " + result.Data["ExpiryDate"].Value);
        },
        error => {
            Debug.Log("Got error getting user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void ShowMessage(string msg)
	{
		errorText.text = msg;
		_msgexpiretime = _msgTime;
	}
public void emailRecovery(string email)
    {
        if(email == "")
		{
			errorText.text = "Empty Email";
			return;
		}
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
		req.Password = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
		req.Email = eMail;
		PlayFabClientAPI.AddUsernamePassword(req,
			result =>
			{
				GameState.username = username;
				RemoveLocalRecords();
				string namepassHash = GetNamePassHash(username, password);
				GameState.passwordhash = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
				DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
				PlayerPrefs.SetString(DataKey.GetPrefKeyName(PropName_NamePassHash), namepassHash);
				SetDisplayName(username);
				PlayerPrefs.SetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID), GameState.playfabID);
				if (GameState.IsPatient())
				{
					DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
					string key = DataKey.DOCTORID;
					PlayFabClientAPI.GetUserData(new GetUserDataRequest()
					{
						Keys = new List<string>() { key }
					},
					result =>
					{
						if (result.Data != null && result.Data.ContainsKey(key))
						{
							GameState.DoctorID = result.Data[key].Value;
							DataKey.SetPrefsString(key, GameState.DoctorID);
							PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
							PatientDataManager.LoadPatientData(OnLoadHomePatientDataSuccess, null);

							return;
						}
					},
					error =>
					{
						failedAction.Invoke(error.ToString());
						return;
					});
				}
				else
				{
					PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
					successAction.Invoke();
					return;
				}

			},
			error =>
			{
				failedAction.Invoke(error.ToString());
			}
		);
		PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
		{
            TitleId = "CD663",
            Username = username,
            Email = eMail,
            Password = password,
            RequireBothUsernameAndEmail = true
		}, result =>
		{
			Debug.Log("Registered!");
			
		}, error =>
		{
			Debug.Log($"Not Registered");
			Debug.Log(error.ErrorMessage);
		});
	}

	void OnLoadHomePatientDataSuccess(Dictionary<Int32, PatientData> plist)
	{
		PatientMgr.SetPatientList(plist);
		GameState.currentPatient = plist.ElementAt(0).Value;
		ChangeScene.LoadScene("ColorScreen");
	}

	void etUserRole(UnityAction<USERROLE> resultAction){
		string key = DataKey.ROLE;
		string regValue = DataKey.GetPrefsString(key);
		if(regValue == USERROLE.PATIENT.ToString()){
			GameState.userRole = USERROLE.PATIENT;
			Debug.Log($"UserRole: " + USERROLE.PATIENT);
			return;
		}
		else if(regValue == USERROLE.DOCTOR.ToString()){
			GameState.userRole = USERROLE.DOCTOR;
			Debug.Log($"UserRole: " + USERROLE.DOCTOR);
			return;
		}
		PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
			Keys = new List<string>(){key}
		},
		result =>{
			if (result.Data != null && result.Data.ContainsKey(key))
			{
				if(result.Data[key].Value == USERROLE.PATIENT.ToString()){
					resultAction.Invoke(USERROLE.PATIENT);
				}
				else{
					resultAction.Invoke(USERROLE.DOCTOR);
				}
			}
			else{
				resultAction.Invoke(USERROLE.DOCTOR);
				PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
					Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
				},
				result =>{},
				error =>{});
			}
		},
		error =>{
			resultAction.Invoke(USERROLE.DOCTOR);
			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
				Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
			},
			result =>{},
			error =>{});
		});
	}

	void OnGetUserRole(USERROLE role){
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

	public void GetLeaderboardDelayed(string statistic)
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
		string datastr = DataKey.GetPrefsString(DataKey.PATIENT);
		if(!string.IsNullOrEmpty(datastr))
			return;
		GetUserDataRequest request = new GetUserDataRequest();
		request.Keys = new List<string>();
		request.Keys.Add(DataKey.PATIENT);
		PlayFabClientAPI.GetUserData(request,
			result =>
			{
				if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
				{
					string str = result.Data[DataKey.PATIENT].Value;
					DataKey.SetPrefsString(DataKey.PATIENT, str);
					Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
					foreach(KeyValuePair<string, PatientData> pair in plist)
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

	void UploadUserData()
	{
		// if(!GameState.IsOnline || GameState.IsPatient())
		// 	return;
		string jsonstr = DataKey.GetPrefsString(DataKey.PATIENT);
		if(string.IsNullOrEmpty(jsonstr))
			return;
		UpdateUserDataRequest request = new UpdateUserDataRequest();
		request.Data = new Dictionary<string, string>();
		request.Data.Add(DataKey.PATIENT, jsonstr);
		request.Permission = UserDataPermission.Public;
		PlayFabClientAPI.UpdateUserData(request,
			result => {
				Dictionary<Int32, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<Int32, PatientData>>(jsonstr);
				foreach(KeyValuePair<Int32, PatientData> pair in plist)
				{
					UpdateUserDataRequest patienrrequest = new UpdateUserDataRequest();
					patienrrequest.Data = new Dictionary<string, string>();
					patienrrequest.Data.Add(pair.Value.name, DataKey.GetPrefsString(pair.Value.name));
					PlayFabClientAPI.UpdateUserData(patienrrequest,
						result => {},
						error =>{}
					);
				}
			},
			error => {
			}
		);
	}

	void RemoveLocalRecords(){
		
		DataKey.DeletePrefsKey(DataKey.PATIENT);
		DataKey.DeletePrefsKey(DataKey.ROLE);
		DataKey.DeletePrefsKey(DataKey.EXPIREDATE);
		DataKey.DeletePrefsKey(DataKey.HOMECALIB);
		DataKey.DeletePrefsKey(DataKey.DOCTORID);
	}
}
// using System.Collections;
// using System.Collections.Generic;
// using System.Text.RegularExpressions;
// using PlayFab;
// using PlayFab.ClientModels;
// using UnityEngine;
// using UnityEngine.Events;
// using System.Text;
// using System.Security.Cryptography;
// using UnityEngine.SceneManagement;
// using TMPro;
// using Newtonsoft.Json;
// using System;
// using UnityEngine.TextCore.Text;
// using System.Linq;

// public class UserAccountManager : MonoBehaviour
// {
// 	const string PropName_NamePassHash = "NamePassHash";

// 	[SerializeField] TextMeshProUGUI errorText;
// 	public static UserAccountManager Instance;

// 	public static UnityEvent OnSignInSuccess = new UnityEvent();
// 	public static UnityEvent<string> OnSignInFailed = new UnityEvent<string>();
// 	public static UnityEvent<string> OnCreateAccountFailed = new UnityEvent<string>();
// 	public static UnityEvent<string, string> OnUserDataRetrieved = new UnityEvent<string, string>();
// 	public static UnityEvent<string, List<PlayerLeaderboardEntry>> OnLeaderboardRetrieved = new UnityEvent<string, List<PlayerLeaderboardEntry>>();
// 	public static UnityEvent<string, StatisticValue> OnStatisticRetrieved = new UnityEvent<string, StatisticValue>();

// 	public static UserAccountInfo userAccountInfo;
// 	public float _msgTime = 3;
// 	float _msgexpiretime;
	
// 	void Awake()
// 	{
// 		Instance = this;
// 	}

// 	private void Update()
// 	{
// 		if (_msgexpiretime > 0)
// 		{
// 			_msgexpiretime -= Time.deltaTime;
// 			if (_msgexpiretime < 0)
// 				errorText.text = "";
// 		}
// 	}

	






// 	string GetHashString(string str)
// 	{
// 		// Create a SHA256   
// 		using (SHA256 sha256Hash = SHA256.Create())
// 		{
// 			// ComputeHash - returns byte array  
// 			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

// 			// Convert byte array to a string   
// 			StringBuilder builder = new StringBuilder();
// 			for (int i = 0; i < bytes.Length; i++)
// 			{
// 				builder.Append(bytes[i].ToString("x2"));
// 			}
// 			return builder.ToString();
// 		}
// 	}

// 	string GetNamePassHash(string username, string password) {
// 		return GetHashString(username + ":" + password + ":" + SystemInfo.deviceUniqueIdentifier);
// 	}

// 	void SignInWithUserRole(UnityAction successAction, UnityAction<string> failedAction){
// 	    UploadUserData();
// 			successAction.Invoke();
// 		// if(GameState.IsPatient()){
// 		// 	successAction.Invoke();
// 		// }
// 		// else{
// 		// 	UploadUserData();
// 		// 	successAction.Invoke();
// 		// 	return;
// 		// }
// 	}

// 	void SignInOnline(UnityAction successAction, UnityAction<string> failedAction){
// 		SignInWithUserRole(successAction, failedAction);
// 		// string key = DataKey.ROLE;
// 		// PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
// 		// Keys = new List<string>(){key}
// 		// },
// 		// result =>{
// 		// 	if (result.Data != null && result.Data.ContainsKey(key))
// 		// 	{
// 		// 		GameState.userRole = result.Data[key].Value == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
// 		// 		DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
// 		// 		if(GameState.IsDoctor()){
// 		// 			key = DataKey.EXPIREDATE;
// 		// 			PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest()
// 		// 			{
// 		// 				Keys = new List<string>() { key }
// 		// 			},
// 		// 			result =>
// 		// 			{
// 		// 				if (result.Data != null && result.Data.ContainsKey(key))
// 		// 				{
// 		// 					string str = result.Data[key].Value;
// 		// 					DateTime expiredate;
// 		// 					if(DateTime.TryParse(str, out expiredate)){
// 		// 						DataKey.SetPrefsString(DataKey.EXPIREDATE, str);
// 		// 						PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
// 		// 							if(result.Time > expiredate){
// 		// 								failedAction.Invoke("License Expired.");
// 		// 							}
// 		// 							else{
// 		// 								SignInWithUserRole(successAction, failedAction);
// 		// 								return;
// 		// 							}
// 		// 							return;
// 		// 						},
// 		// 						error =>
// 		// 						{
// 		// 							failedAction.Invoke("Can not get server time.");
// 		// 							return;
// 		// 						});
// 		// 						return;
// 		// 					}
// 		// 					else{
// 		// 						failedAction.Invoke("Invalid date expiring format.");
// 		// 						return;
// 		// 					}
// 		// 				}
// 		// 				else{
// 		// 					failedAction.Invoke("Missing License lifetime data.");
// 		// 					return;
// 		// 				}
// 		// 			},
// 		// 			error =>
// 		// 			{
// 		// 				failedAction.Invoke("Missing License lifetime data.");
// 		// 				return;
// 		// 			});
// 		// 		}
// 		// 		else{
// 		// 			key = DataKey.DOCTORID;
// 		// 			PlayFabClientAPI.GetUserData(new GetUserDataRequest()
// 		// 			{
// 		// 				Keys = new List<string>() { key }
// 		// 			},
// 		// 			result =>
// 		// 			{
// 		// 				if(result.Data != null && result.Data.ContainsKey(key)){
// 		// 					GameState.DoctorID = result.Data[key].Value;
// 		// 					DataKey.SetPrefsString(key, GameState.DoctorID);
// 		// 					GetUserDataRequest request = new GetUserDataRequest();
// 		// 					request.Keys = new List<string>();
// 		// 					request.Keys.Add(DataKey.PATIENT);
// 		// 					request.PlayFabId = GameState.DoctorID;
// 		// 					PlayFabClientAPI.GetUserData(request,
// 		// 						result =>
// 		// 						{
// 		// 							if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
// 		// 							{
// 		// 								string str = result.Data[DataKey.PATIENT].Value;
// 		// 								Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
// 		// 								foreach(KeyValuePair<string, PatientData> pair in plist)
// 		// 								{
// 		// 									if(pair.Value.PFID == GameState.playfabID){
// 		// 										PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
// 		// 											if(result.Time > pair.Value.ExpireDate){
// 		// 												failedAction.Invoke("License Expired.");
// 		// 											}
// 		// 											else{
// 		// 												Debug.Log($"Accepted License Key");
// 		// 												GameState.ExpireDate = pair.Value.ExpireDate;
// 		// 												DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
// 		// 												SignInWithUserRole(successAction, failedAction);
// 		// 											}
// 		// 										},
// 		// 										error =>
// 		// 										{
// 		// 											failedAction.Invoke("Can not get server time.");
// 		// 										});
// 		// 										return;
// 		// 									}
// 		// 								}
										
// 		// 								RemoveLocalRecords();
// 		// 								failedAction.Invoke("Can not find patient data.");
// 		// 								return;
// 		// 							}
// 		// 						},
// 		// 						error =>
// 		// 						{
// 		// 							failedAction.Invoke(error.ToString());
// 		// 							return;
// 		// 						}
// 		// 					);
// 		// 					return;
// 		// 				}
// 		// 				else{
// 		// 					failedAction.Invoke("Can not get doctor ID.");
// 		// 					return;
// 		// 				}
// 		// 			}, error =>{
// 		// 				failedAction.Invoke("Can not get doctor ID.");
// 		// 				return;
// 		// 			});
// 		// 		}
// 		// 	}
// 		// 	else{
// 		// 		failedAction.Invoke("Can not get user role.");
// 		// 	}
// 		// 	return;
// 		// },
// 		// error=>{
// 		// 	failedAction.Invoke("Can not get user role.");
// 		// 	return;
// 		// });
							
// 	}

// 	public void SignIn(string username, string password, UnityAction successAction, UnityAction<string> failedAction)
// 	{


// 		Debug.Log("Using name and password: " + username);
		
// 		//string namepassHash = GetNamePassHash(username, password);
// 		// string pwdhash = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
		
		
// 		//GameState.username = username;
// 		//GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));

// 		//offline mode
// 		// if(PlayerPrefs.GetString(DataKey.GetPrefKeyName (PropName_NamePassHash), "") == namepassHash &&
// 		// !string.IsNullOrEmpty(GameState.playfabID))
// 		// {
// 		// 	GameState.passwordhash = pwdhash;
// 		// 	GameState.IsOnline = false;
// 		// 	string expdatestr = DataKey.GetPrefsString(DataKey.EXPIREDATE);
// 		// 	if(!string.IsNullOrEmpty(expdatestr)){
// 		// 		DateTime expdate;
// 		// 		if(DateTime.TryParse(expdatestr, out expdate)){
// 		// 			DateTime curdate = DateTime.Now;
// 		// 			if(expdate >= curdate){
// 		// 				//try connect
// 		// 				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
// 		// 				{
// 		// 					TitleId = PlayFabSettings.TitleId,
// 		// 					Username = username,
// 		// 					Password = GetHashString(password + SystemInfo.deviceUniqueIdentifier),
// 		// 				}, result =>
// 		// 				{
// 		// 					GameState.IsOnline = true;
// 		// 					GameState.playfabID = result.PlayFabId;
// 		// 					PlayerPrefs.SetString(DataKey.GetPrefKeyName (DataKey.PLAYFABID), GameState.playfabID);
// 		// 					SignInOnline(successAction, failedAction);
// 		// 					return;
// 		// 				}, error =>
// 		// 				{
// 		// 					GameState.IsOnline = false;
// 		// 					GameState.playfabID = PlayerPrefs.GetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID));
// 		// 					if(string.IsNullOrEmpty(GameState.playfabID)){
// 		// 						failedAction.Invoke("Can not get User ID.");
// 		// 						return;
// 		// 					}
// 		// 					GameState.username = username;
// 		// 					string role = DataKey.GetPrefsString(DataKey.ROLE);
// 		// 					if(string.IsNullOrEmpty(role)){
// 		// 						failedAction.Invoke("Can not get User Role.");
// 		// 						return;
// 		// 					}
// 		// 					GameState.userRole = role == USERROLE.PATIENT.ToString()? USERROLE.PATIENT: USERROLE.DOCTOR;
// 		// 					if(GameState.IsPatient()){
// 		// 						GameState.DoctorID = DataKey.GetPrefsString(DataKey.DOCTORID);
// 		// 						if(string.IsNullOrEmpty(GameState.DoctorID)){
// 		// 							failedAction.Invoke("Can not get doctor ID.");
// 		// 							return;
// 		// 						}
// 		// 					}
// 		// 					successAction.Invoke();
// 		// 					Debug.Log($"<color=red>Unsuccessful connect to Playfab</color>");
// 		// 				});
// 		// 				return;
// 		// 			}
// 		// 		}
// 		// 	}
// 		// }

		
// 		//online mode
// 		GameState.username = GameState.playfabID = GameState.DoctorID = GameState.passwordhash = "";
// 		Debug.Log(username);
// 		Debug.Log(password);
// 		string pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
// 		if(Regex.IsMatch(username, pattern))
// 		{
// 			PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest()
// 		{
// 			TitleId = PlayFabSettings.TitleId,
// 		    Email = username,
// 			Password = password,
// 		}, result =>
// 		{
// 			// GameState.IsOnline = true;
// 			// GameState.passwordhash = pwdhash;
// 			// GameState.username = username;
// 			// GameState.playfabID = result.PlayFabId;
// 			SignInOnline(successAction, failedAction);
			
// 		}, error =>
// 		{
// 			Debug.Log($"<color=yellow>Unsuccessful Login with email and password</color>");
// 			failedAction.Invoke(error.ErrorMessage);
			
// 		});


// 		}
// 		else
// 		{
// 				PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
// 				{
// 					TitleId = PlayFabSettings.TitleId,
// 					Username = username,
// 					Password = password,
// 				}, result =>
// 				{
// 					// GameState.IsOnline = true;
// 					// GameState.passwordhash = pwdhash;
// 					// GameState.username = username;
// 					// GameState.playfabID = result.PlayFabId;
// 					SignInOnline(successAction, failedAction);
					
// 				}, error =>
// 				{
// 					Debug.Log($"<color=green>Unsuccessful Login with username and password</color>");
// 					failedAction.Invoke(error.ErrorMessage);
// 				});
// 		}
// 	}
// 	void SignUpForDoctor(string fabID, UnityAction successAction, UnityAction<string> failedAction)
// 	{
// 		string key = DataKey.EXPIREDATE;
// 		PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest()
// 		{
// 			Keys = new List<string>() { key }
// 		},
// 		result =>
// 		{
// 			successAction.Invoke();
// 			// if (result.Data != null && result.Data.ContainsKey(key))
// 			// {
// 			// 	string str = result.Data[key].Value;
// 			// 	DateTime expiredate;
// 			// 	// if(DateTime.TryParse(str, out expiredate)){
// 			// 		PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
// 			// 			// if(result.Time > expiredate){
// 			// 			// 	failedAction.Invoke("License Expired.");
// 			// 			// }
// 			// 			// else{
// 			// 				Debug.Log($"Accepted License Key");
// 			// 				GameState.playfabID = fabID;
// 			// 				GameState.ExpireDate = expiredate;
// 			// 				key = DataKey.ROLE;
// 			// 				PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
// 			// 					Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
// 			// 				},
// 			// 				result =>{
// 			// 					successAction.Invoke();
// 			// 				},
// 			// 				error =>{
// 			// 					failedAction.Invoke("Can not set doctor role.");
// 			// 					return;
// 			// 				});
							
// 			// 			// }
// 			// 			return;
// 			// 		},
// 			// 		error =>
// 			// 		{
// 			// 			failedAction.Invoke("Can not get server time.");
// 			// 			return;
// 			// 		});
// 			// 		return;
// 			// 	}
// 			// 	// else{
// 			// 	// 	failedAction.Invoke("Invalid date expiring format.");
// 			// 	// 	return;
// 			// 	// }
// 			// }
// 			// else{
// 			// 	failedAction.Invoke("Missing License lifetime data1.");
// 			// 	return;
// 			// }
// 		},
// 		error =>
// 		{
// 			failedAction.Invoke("Missing License lifetime data2.");
// 			return;
// 		});
// 	}

// 	public void SignUp(string licensekey, UnityAction successAction, UnityAction<string> failedAction)
// 	{
		
// 		GameState.IsOnline = false;
// 		Debug.Log("Using custom licenseKey: " + licensekey);

// 		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
// 		{
// 			CustomId = licensekey,
// 			TitleId = PlayFabSettings.TitleId,
// 			CreateAccount = false
// 		}, result =>
// 		{
// 			GameState.IsOnline = true;
// 			string fabID = result.PlayFabId;
// 			string key = DataKey.ROLE;
// 			// PlayFabServerAPI.UpdateUserReadOnlyData(new UpdateUserDataRequest()
// 			//  {
//         	// 	Data = new Dictionary<string, string>() {
//             // 	{"ExpiryDate", "2024-12-30"}
//         	// 	},
// 			// 	Permission = UserDataPermission.Public

//     		// },
//     		// result => Debug.Log("Successfully updated user data"),
//     		// error => {
//         	// Debug.Log("Got error setting expiry date");
//         	// Debug.Log(error.GenerateErrorReport());
//     		// });
// 			PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
// 			Keys = new List<string>(){key}
// 			},
// 			result =>{
// 				USERROLE role = USERROLE.DOCTOR;
// 				if (result.Data != null && result.Data.ContainsKey(key))
// 				{
// 					if(result.Data[key].Value == USERROLE.PATIENT.ToString())
// 						role = USERROLE.PATIENT;
// 					else
// 						role = USERROLE.DOCTOR;
// 					GameState.userRole = role;
// 					if(role == USERROLE.PATIENT){
// 						key = DataKey.DOCTORID;
// 						PlayFabClientAPI.GetUserData(new GetUserDataRequest()
// 						{
// 							Keys = new List<string>() { key }
// 						},
// 						result =>
// 						{
// 							if(result.Data != null && result.Data.ContainsKey(key)){
// 								GameState.DoctorID = result.Data[key].Value;
// 								GetUserDataRequest request = new GetUserDataRequest();
// 								request.Keys = new List<string>();
// 								request.Keys.Add(DataKey.PATIENT);
// 								request.PlayFabId = GameState.DoctorID;
// 								PlayFabClientAPI.GetUserData(request,
// 									result =>
// 									{
// 										if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
// 										{
// 											string str = result.Data[DataKey.PATIENT].Value;
// 											Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
// 											foreach(KeyValuePair<string, PatientData> pair in plist)
// 											{
// 												if(pair.Value.PFID == fabID){
// 													PlayFabClientAPI.GetTime(new GetTimeRequest(), result =>{
// 														if(result.Time > pair.Value.ExpireDate){
// 															failedAction.Invoke("License Expired.");
// 														}
// 														else{
// 															Debug.Log($"Accepted License Key");
// 															GameState.ExpireDate = pair.Value.ExpireDate;
// 															GameState.playfabID = fabID;
// 															successAction.Invoke();
// 														}
// 													},
// 													error =>
// 													{
// 														failedAction.Invoke("Can not get server time.");
// 													});
// 													return;
// 												}
// 											}
// 											failedAction.Invoke("Can not find patient data.");
// 											return;
// 										}
// 									},
// 									error =>
// 									{
// 										failedAction.Invoke(error.ToString());
// 										return;
// 									}
// 								);
// 								successAction.Invoke();
// 								return;
// 							}
// 							else{
// 								failedAction.Invoke("Can not get doctor ID.");
// 								return;
// 							}
// 						}, error =>{
// 							failedAction.Invoke("Can not get doctor ID.");
// 							return;
// 						});
// 					}
// 					else
// 						SignUpForDoctor(fabID, successAction, failedAction);
// 					return;
// 				}
// 				else{
// 					SignUpForDoctor(fabID, successAction, failedAction);
// 					return;
// 				}
// 			},
// 			error =>{
// 				failedAction.Invoke(error.ToString());
// 				return;
// 			});
// 		}, error =>
// 		{
// 			Debug.Log($"<color=red>{error.ToString()}</color>");
// 			failedAction.Invoke(error.ErrorMessage);
// 		});
// 	}

// 	public void ShowMessage(string msg)
// 	{
// 		errorText.text = msg;
// 		_msgexpiretime = _msgTime;
// 	}
// public void emailRecovery(string email)
//     {
//         PlayFabClientAPI.SendAccountRecoveryEmail(new SendAccountRecoveryEmailRequest()
// 		{
//             Email = email,
//             TitleId = PlayFabSettings.TitleId
// 		}, result =>
// 		{
// 			Debug.Log("Email sent!");
			
// 		}, error =>
// 		{
// 			Debug.Log($"Not sent");
// 			Debug.Log(error.ErrorMessage);
// 		});
        
//     }
	

// 	public void SetUserNameAndPassword(string eMail, string username, string password, UnityAction successAction, UnityAction<string> failedAction)
// 	{

// 		// AddUsernamePasswordRequest req = new AddUsernamePasswordRequest();
// 		// req.Username = username;
// 		// req.Password = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
// 		// req.Email = eMail;
// 		// PlayFabClientAPI.AddUsernamePassword(req,
// 		// 	result =>
// 		// 	{
// 		// 		GameState.username = username;
// 		// 		RemoveLocalRecords();
// 		// 		string namepassHash = GetNamePassHash(username, password);
// 		// 		GameState.passwordhash = GetHashString(password + SystemInfo.deviceUniqueIdentifier);
// 		// 		DataKey.SetPrefsString(DataKey.EXPIREDATE, GameState.ExpireDate.ToString(GameConst.STRFORMAT_DATETIME));
// 		// 		PlayerPrefs.SetString(DataKey.GetPrefKeyName (PropName_NamePassHash), namepassHash);
// 		// 		SetDisplayName(username);
// 		// 		PlayerPrefs.SetString(DataKey.GetPrefKeyName(DataKey.PLAYFABID), GameState.playfabID);
// 		// 		if(GameState.IsPatient()){
// 		// 			DataKey.SetPrefsString(DataKey.ROLE, GameState.userRole.ToString());
// 		// 			string key = DataKey.DOCTORID;
// 		// 			PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
// 		// 				Keys = new List<string>() { key }
// 		// 			},
// 		// 			result =>{
// 		// 				if (result.Data != null && result.Data.ContainsKey(key))
// 		// 				{
// 		// 					GameState.DoctorID = result.Data[key].Value;
// 		// 					DataKey.SetPrefsString(key, GameState.DoctorID);
// 		// 					PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
// 		// 					PatientDataManager.LoadPatientData(OnLoadHomePatientDataSuccess, null);
							
// 		// 					return;
// 		// 				}
// 		// 			},
// 		// 			error =>{
// 		// 				failedAction.Invoke(error.ToString());
// 		// 				return;
// 		// 			});
// 		// 		}
// 		// 		else{
// 		// 			PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest(), null, null);
// 		// 			successAction.Invoke();
// 		// 			return;
// 		// 		}
				
// 		// 	},
// 		// 	error =>
// 		// 	{
// 		// 		failedAction.Invoke(error.ToString());
// 		// 	}
// 		// );
// 		PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
// 		{
//             TitleId = "CD663",
//             Username = username,
//             Email = eMail,
//             Password = password,
//             RequireBothUsernameAndEmail = true
// 		}, result =>
// 		{
// 			Debug.Log("Registered!");
			
// 		}, error =>
// 		{
// 			Debug.Log($"Not Registered");
// 			Debug.Log(error.ErrorMessage);
// 		});
// 	}

// 	void OnLoadHomePatientDataSuccess(Dictionary<Int32, PatientData> plist)
// 	{
// 		PatientMgr.SetPatientList(plist);
// 		GameState.currentPatient = plist.ElementAt(0).Value;
// 		ChangeScene.LoadScene("ColorScreen");
// 	}

// 	void etUserRole(UnityAction<USERROLE> resultAction){
// 		string key = DataKey.ROLE;
// 		string regValue = DataKey.GetPrefsString(key);
// 		if(regValue == USERROLE.PATIENT.ToString()){
// 			GameState.userRole = USERROLE.PATIENT;
// 			Debug.Log($"UserRole: " + USERROLE.PATIENT);
// 			return;
// 		}
// 		else if(regValue == USERROLE.DOCTOR.ToString()){
// 			GameState.userRole = USERROLE.DOCTOR;
// 			Debug.Log($"UserRole: " + USERROLE.DOCTOR);
// 			return;
// 		}
// 		PlayFabClientAPI.GetUserData(new GetUserDataRequest(){
// 			Keys = new List<string>(){key}
// 		},
// 		result =>{
// 			if (result.Data != null && result.Data.ContainsKey(key))
// 			{
// 				if(result.Data[key].Value == USERROLE.PATIENT.ToString()){
// 					resultAction.Invoke(USERROLE.PATIENT);
// 				}
// 				else{
// 					resultAction.Invoke(USERROLE.DOCTOR);
// 				}
// 			}
// 			else{
// 				resultAction.Invoke(USERROLE.DOCTOR);
// 				PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
// 					Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
// 				},
// 				result =>{},
// 				error =>{});
// 			}
// 		},
// 		error =>{
// 			resultAction.Invoke(USERROLE.DOCTOR);
// 			PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest(){
// 				Data = new Dictionary<string, string>() { {key, USERROLE.DOCTOR.ToString() }}
// 			},
// 			result =>{},
// 			error =>{});
// 		});
// 	}

// 	void OnGetUserRole(USERROLE role){
// 		GameState.userRole = role;
// 		DataKey.SetPrefsString(DataKey.ROLE, role.ToString());
// 		Debug.Log($"UserRole: " + role);
// 	}

	

// 	bool GetDeviceId(out string android_id, out string ios_id, out string custom_id)
// 	{
// 		android_id = string.Empty;
// 		ios_id = string.Empty;
// 		custom_id = string.Empty;

// 		if (CheckForSupportedMobilePlatform())
// 		{
// #if UNITY_ANDROID
//             //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
//             AndroidJavaClass clsUnity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
//             AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject> ("currentActivity");
//             AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject> ("getContentResolver");
//             AndroidJavaClass clsSecure = new AndroidJavaClass ("android.provider.Settings$Secure");
//             android_id = clsSecure.CallStatic<string> ("getString", objResolver, "android_id");
// #endif

// #if UNITY_IPHONE
//             ios_id = UnityEngine.iOS.Device.vendorIdentifier;
// #endif
// 			return true;
// 		}
// 		else
// 		{
// 			custom_id = SystemInfo.deviceUniqueIdentifier;
// 			return false;
// 		}
// 	}

// 	bool CheckForSupportedMobilePlatform()
// 	{
// 		return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
// 	}

// 	/*
//         DISPLAYNAME
//     */

// 	void CheckDisplayName(string username, UnityAction completeAction)
// 	{
// 		PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
// 		{
// 			//PlayFabId = playfabID;
// 		}, result => {
// 			userAccountInfo = result.AccountInfo;

// 			if (result.AccountInfo.TitleInfo.DisplayName == null || result.AccountInfo.TitleInfo.DisplayName.Length == 0)
// 			{
// 				PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
// 				{
// 					DisplayName = username
// 				}, result => {
// 					Debug.Log($"Display name set to username");
// 					completeAction.Invoke();
// 				}, error => {
// 					Debug.Log($"Display name could not be set to username | {error.ErrorMessage}");
// 				});
// 			}
// 			else
// 			{
// 				completeAction.Invoke();
// 			}
// 		}, error => {
// 			Debug.Log($"Could not retrieve AccountInfo | {error.ErrorMessage}");
// 		});
// 	}

// 	public void SetDisplayName(string displayName)
// 	{
// 		PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
// 		{
// 			DisplayName = displayName
// 		}, result => {
// 			Debug.Log($"Display name set to username");
// 		}, error => {
// 			Debug.Log($"Display name could not be set to username | {error.ErrorMessage}");
// 		});
// 	}

// 	/*
//         USERDATA
//     */

// 	public void GetUserData(string key)
// 	{
// 		PlayFabClientAPI.GetUserData(new GetUserDataRequest()
// 		{
// 			//PlayFabId = playfabID,;
// 			Keys = new List<string>() {
// 					key
// 				}
// 		}, result => {
// 			Debug.Log($"User data retrieved successfully");
// 			if (result.Data.ContainsKey(key)) OnUserDataRetrieved.Invoke(key, result.Data[key].Value);
// 			else OnUserDataRetrieved.Invoke(key, null);
// 		}, error => {
// 			Debug.Log($"Could not retrieve user data | {error.ErrorMessage}");
// 		});
// 	}

// 	public void SetUserData(string key, string userData)
// 	{
// 		PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
// 		{
// 			Data = new Dictionary<string, string>() { { key, userData }
// 			}
// 		}, result => {
// 			Debug.Log($"{key} successfully updated");
// 		}, error => {
// 			Debug.Log($"{key} update unsuccessful | {error.ErrorMessage}");
// 		});
// 	}

// 	/*
//         STATISTICS & LEADERBOARDS
//     */

// 	public void GetStatistic(string statistic)
// 	{
// 		PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest()
// 		{
// 			StatisticNames = new List<string>() {
// 				statistic
// 			}
// 		}, result => {
// 			if (result.Statistics.Count > 0)
// 			{
// 				Debug.Log($"Successfully got {statistic} | {result.Statistics[0]}");
// 				if (result.Statistics != null) OnStatisticRetrieved.Invoke(statistic, result.Statistics[0]);
// 			}
// 			else
// 			{
// 				Debug.Log($"No existing statistic [{statistic}] for user");
// 			}
// 		}, error => {
// 			Debug.Log($"Could not retrieve {statistic} | {error.ErrorMessage}");
// 		});
// 	}

// 	public void SetStatistic(string statistic, int value)
// 	{
// 		PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest()
// 		{
// 			Statistics = new List<StatisticUpdate>() {
// 				new StatisticUpdate () {
// 					StatisticName = statistic,
// 						Value = value
// 				}
// 			}
// 		}, result => {
// 			Debug.Log($"{statistic} successfully updated");
// 			GetLeaderboard(statistic);
// 		}, error => {
// 			Debug.Log($"{statistic} update unsuccessful | {error.ErrorMessage}");
// 		});
// 	}

// 	public void GetLeaderboard(string statistic)
// 	{
// 		PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest()
// 		{
// 			StatisticName = statistic
// 		}, result => {
// 			Debug.Log($"Successfully got {statistic} leaderboard | 0.{result.Leaderboard[0].DisplayName} {result.Leaderboard[0].StatValue}");
// 			OnLeaderboardRetrieved.Invoke(statistic, result.Leaderboard);
// 		}, error => {
// 			Debug.Log($"Could not retrieve {statistic} leaderboard | {error.ErrorMessage}");
// 		});
// 	}

// 	public void GetLeaderboardDelayed(string statistic)
// 	{
// 		StartCoroutine(CheckLeaderboardDelay(statistic));
// 	}

	
// 	IEnumerator CheckLeaderboardDelay(string statistic)
// 	{
// 		yield return new WaitForSeconds(3);
// 		GetLeaderboard(statistic);
// 	}

// 	void DownloadUserData()
// 	{
// 		string datastr = DataKey.GetPrefsString(DataKey.PATIENT);
// 		if(!string.IsNullOrEmpty(datastr))
// 			return;
// 		GetUserDataRequest request = new GetUserDataRequest();
// 		request.Keys = new List<string>();
// 		request.Keys.Add(DataKey.PATIENT);
// 		PlayFabClientAPI.GetUserData(request,
// 			result =>
// 			{
// 				if (result.Data != null && result.Data.ContainsKey(DataKey.PATIENT))
// 				{
// 					string str = result.Data[DataKey.PATIENT].Value;
// 					DataKey.SetPrefsString(DataKey.PATIENT, str);
// 					Dictionary<string, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<string, PatientData>>(str);
// 					foreach(KeyValuePair<string, PatientData> pair in plist)
// 					{
// 						PatientDataMgr.LoadPatientData(pair.Key);
// 					}
// 				}
// 			},
// 			error =>
// 			{
// 				//failAction.Invoke(error.ToString());
// 			}
// 		);
// 	}

// 	void UploadUserData()
// 	{
// 		// if(!GameState.IsOnline || GameState.IsPatient())
// 		// 	return;
// 		string jsonstr = DataKey.GetPrefsString(DataKey.PATIENT);
// 		if(string.IsNullOrEmpty(jsonstr))
// 			return;
// 		UpdateUserDataRequest request = new UpdateUserDataRequest();
// 		request.Data = new Dictionary<string, string>();
// 		request.Data.Add(DataKey.PATIENT, jsonstr);
// 		request.Permission = UserDataPermission.Public;
// 		PlayFabClientAPI.UpdateUserData(request,
// 			result => {
// 				Dictionary<Int32, PatientData> plist = JsonConvert.DeserializeObject<Dictionary<Int32, PatientData>>(jsonstr);
// 				foreach(KeyValuePair<Int32, PatientData> pair in plist)
// 				{
// 					UpdateUserDataRequest patienrrequest = new UpdateUserDataRequest();
// 					patienrrequest.Data = new Dictionary<string, string>();
// 					patienrrequest.Data.Add(pair.Value.name, DataKey.GetPrefsString(pair.Value.name));
// 					PlayFabClientAPI.UpdateUserData(patienrrequest,
// 						result => {},
// 						error =>{}
// 					);
// 				}
// 			},
// 			error => {
// 			}
// 		);
// 	}

// 	void RemoveLocalRecords(){
		
// 		DataKey.DeletePrefsKey(DataKey.PATIENT);
// 		DataKey.DeletePrefsKey(DataKey.ROLE);
// 		DataKey.DeletePrefsKey(DataKey.EXPIREDATE);
// 		DataKey.DeletePrefsKey(DataKey.HOMECALIB);
// 		DataKey.DeletePrefsKey(DataKey.DOCTORID);
// 	}
// }
