using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public enum GENDER
{
	Male = 0,
	Female
}

public enum THERAPPYPLACE{
	Home = 0,
	Clinic
}

public enum GAMEMODE
{
	SingleGame = 0,
	SessionGame,
	DeviceSetting
}

public enum EYESIDE
{
	LEFT = 0,
	RIGHT,
	INVALID
}

public abstract class LocalKey{
	public const string WBCAMERANAME = "WebCameraName";
	public const string LASTONLINE = "LastOnLine";//"True" or "False"
}

public abstract class DataKey
{
	public static string PATIENT = "Patient";
	public static string DOCTORID = "DoctorID";
	public static string ROLE = "Role";
	public const string EXPIREDATE = "ExpireDate";
	public const string PLAYFABID = "PlayfabID";
	public const string HOMECALIB = "HomeCalib";
	
	public const string CLINICLIMIT = "ClinicLimit";
	public const string HOMELIMIT = "HomeLimit";
	public const string FreeLicenseDay = "FreeLinceseLimit";
	public const string HOMEPATIENT = "HomePatient";
	public const string SF_SESSIONRECORD = "_SessionRecord";

	public static string GetPrefKeyName(string orgkey){
		if(string.IsNullOrEmpty(GameState.username)){
			return GameConst.PATIENTNAME_ANONYMOUS + "_" + orgkey;
		}
		return GameState.username + "_" + orgkey;
	}

	public static void SetPrefsString(string key, string value)
	{
		PlayerPrefs.SetString(GetPrefKeyName(key), StringEncrypter.Crypt(value));
	}


	public static string GetPrefsString(string key, string defaultValue = "")
	{
		string value = PlayerPrefs.GetString(GetPrefKeyName(key), "");
		if(string.IsNullOrEmpty(value))
			return default;
		return StringEncrypter.Decrypt(value);
	}

	
	public static void DeletePrefsKey(string key)
	{
		PlayerPrefs.DeleteKey(GetPrefKeyName(key));
	}


}



public class PatientData
{
	public  string name;
	public byte age;
	public string licenseKey;
	public GENDER gender;
	public THERAPPYPLACE place;
	public string details;
	public CalibraionDetails cali = new CalibraionDetails();
	public List<byte> therapygames = new List<byte>();
	public string PFID;
	public DateTime ExpireDate;
	public PatientData(string nm, byte ag, GENDER gen, string dt, THERAPPYPLACE plc, DateTime expireDate,string licensekey)
	{
		name = nm;
		age = ag;
		gender = gen;
		details = dt;
		place = plc;
		ExpireDate = expireDate;
		licenseKey = licensekey;
		PFID = GameConst.PLAYFABID_CLINIC;
	}
	

	public string ToJSONString()
	{
		return "{ID = {ID}; NM = {name}; AG = {age}; GD = {gender.ToString()}; DT = {details};}";
	}

	public bool IsHome(){
		return place == THERAPPYPLACE.Home;
	}

	public bool IsClinic(){
		return place == THERAPPYPLACE.Clinic;
	}

	public void GetDataFromDoctorData(HomePatientData hdata){
		therapygames = hdata.therapygames;
	}

	public void PutDataToDoctorData(HomePatientData hdata){
		hdata.therapygames = therapygames;
	}
	
	public void ResetData(){
		cali = new CalibraionDetails();
		therapygames.Clear();
	}
}

public class HomePatientData{
	public List<byte> therapygames = new List<byte>();
}





/* public class HomePatientData{
	public CalibraionDetails cali = new CalibraionDetails();
} */
public class CalibraionDetails
{
	public float rd = 0.5f;
	public float cy = 0.5f;
	public float bg = 0.5f;
	public bool transparent = true;
	public string profileStr = ColorCalibration.PROFILE_DEFAULT;
}
public class GamePlay
{
	public string name;
	//public DateTime time;
	public int duration, sScr, eScr, sLvl = 1, eLvl;
	public static float GetConvertedScore(int score, string gamename){
		if(gamename == GameConst.GAMENAME_VERGENCE)
			return Mathf.Abs((float)score) / 10;
		else
			return score;
	}
}

public class DiagnosticRecord{
	public string result, unit, norm, comment;
}

public class StatisData
{
	public float maxScore, maxLevel, maxAvgTime;
}

public class SavedGameData
{
	public int score, level;
	public SavedGameData(int sc, int lvl)
	{
		score = sc;
		level = lvl;
	}
}

public abstract class VisualFactor
{
	public static float dpi = 100;//screen pixels per inch
	const float distanceToScreen = 50;//50cm
	public static void LoadFactor()
	{
		string _dpiPath = DPICaculator.GetDPIPath();

		if (File.Exists(_dpiPath))
		{
			string[] str = File.ReadAllLines(_dpiPath);
			dpi = float.Parse(str[1]);
		}
	}

	public static float CanvasToCM(float pixInCanvas, CanvasScaler scaler = null)
	{
		if(scaler == null)
			return pixInCanvas / dpi * 2.54f;
		else
			return pixInCanvas * Screen.width / scaler.referenceResolution.x / dpi * 2.54f;
	}

	public static float ScreenCMToArcSecond(float CMscreen)
	{
		return CMscreen * 360 * 3600 / distanceToScreen * 2 * Mathf.PI;
	}
}

public abstract class StringEncrypter
{

	static SymmetricAlgorithm symmetricAlgorithm = System.Security.Cryptography.Rijndael.Create();// DES.Create();
	static byte[] Key;
	public static void SetKey(string playfabID){
		if(string.IsNullOrEmpty(playfabID)){
			Key = null;
			return;
		}
		else
			Key = Encoding.ASCII.GetBytes(playfabID);
			
		/* byte[] orgbytes = Encoding.UTF8.GetBytes(playfabID);
		int orgLen = orgbytes.Length;
		int keysize = symmetricAlgorithm.KeySize / 8;
		byte[] key = new byte[keysize];
		for(int i = 0; i < keysize; i++)
			key[i] = orgbytes[i % orgLen];
		symmetricAlgorithm.Key = key; */
	}

	static string EncodeNonAsciiCharacters( string value ) {
		byte[] bytearray = Encoding.Unicode.GetBytes(value);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytearray)
    		sb.AppendFormat("{0:x2}", b);
        return sb.ToString();
    }

    static string DecodeEncodedNonAsciiCharacters( string value ) {
       byte[] bytearray = new byte[value.Length / 2];
	   for(int i = 0; i < value.Length / 2; i++){
			bytearray[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber);
	   }
	   return Encoding.Unicode.GetString(bytearray);
    }
	
	public static string Crypt(string text)
	{
		if(Key.Length == 0){
			Debug.LogError("Key is null.");
			return "";
		}

		if (!String.IsNullOrEmpty(text))
		{
			/* byte[] plaintextBytes = System.Text.Encoding.Unicode.GetBytes(text);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length);
				}

				result = Encoding.Unicode.GetString(memoryStream.ToArray());
			}
			
			string str = EncodeNonAsciiCharacters(result);
			return str;
			 */
			byte[] bytearrayText = Encoding.Unicode.GetBytes(text);
			for(int i = 0; i < bytearrayText.Length; i++)
				bytearrayText[i] ^= Key[i % Key.Length];
			StringBuilder sb = new StringBuilder();
			foreach (byte b in bytearrayText)
				sb.AppendFormat("{0:x2}", b);
			return sb.ToString();
		}
		else return "";
	}

	public static string Decrypt(string text)
	{
		/* string result = null;
		text = DecodeEncodedNonAsciiCharacters(text);

		if (!String.IsNullOrEmpty(text))
		{
			byte[] encryptedBytes = Encoding.Unicode.GetBytes(text);

			using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Read))
				{
					byte[] decryptedBytes = new byte[encryptedBytes.Length];
					cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
					result = Encoding.Unicode.GetString(decryptedBytes);
				}
			}
		}

		return result; */

		if(string.IsNullOrEmpty(text))
			return "";
		if(Key.Length == 0){
			Debug.LogError("key is null");
			return "";
		}

		byte[] bytearray = new byte[text.Length / 2];
		for(int i = 0; i < text.Length / 2; i++){
				bytearray[i] = byte.Parse(text.Substring(i * 2, 2), NumberStyles.HexNumber);
		}
		for(int i = 0; i < bytearray.Length; i++)
			bytearray[i] ^= Key[i % Key.Length];
		return Encoding.Unicode.GetString(bytearray);
	}
}

