
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ColorCalibration
{
	public const string PrefName_Background = "BackgroundValue";
	public const string PrefName_Red = "RedValue";
	public const string PrefName_Cyan = "CyanValue";
	public const string PrefName_Transparet = "Transparent";
	public const string PrefName_Profile = "ColorCaliProfile";

	static Color redcolor = Color.black;
	static Color cyancolor = Color.black;
	static Color backcolor = Color.black;

	/// <summary>
	/// for calibration testing
	static bool testmode = false;
	public static string Color_RedLeft = "#ff0150";
	public static string Color_RedRight = "#fbff50";
	public static string Color_CyanLeft = "#00ffd1";
	public static string Color_CyanRight = "#00d1ff";
	public static string Color_BackLeft = "#e0e0e0";
	public static string Color_BackRight = "#ffffff";
	public const string PROFILE_DEFAULT = "#ff0200 - #fdffff";
	/// </summary>
	public static Color RedColor
	{
		get
		{
			if (redcolor == Color.black)
			{
				redcolor = GetRedColor(GameState.currentPatient == null? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName(ColorCalibration.PrefName_Red), 0.5f): GameState.currentPatient.cali.rd);
				return redcolor;
			}
			else
				return redcolor;
		}
		set
		{
			redcolor = value;
		}
	}

	public static Color CyanColor
	{
		get
		{
			if (cyancolor == Color.black)
			{
				cyancolor = GetCyanColor(GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName(ColorCalibration.PrefName_Cyan), 0.5f) : GameState.currentPatient.cali.cy);
				return cyancolor;
			}
			else
				return cyancolor;
		}
		set
		{
			cyancolor = value;
		}
	}
	public static Color BackColor
	{
		get
		{
			if (backcolor == Color.black)
			{
				backcolor = GetBackGroundColor(GameState.currentPatient == null ? PlayerPrefs.GetFloat(DataKey.GetPrefKeyName(ColorCalibration.PrefName_Background), 0.5f) : GameState.currentPatient.cali.bg);
				return backcolor;
			}
			else
				return backcolor;
		}
		set
		{
			backcolor = value;
		}
	}

	// ---------------------Current AmblyoPlay version--------------------
	/*public static Color GetBackGroundColor(float factor)
	{
		return Color.Lerp(new Color(0.9019f, 0.9019f, 0.9019f, 1), new Color(1, 1, 1, 1), factor);
	}
	public static Color GetRedColor(float factor)
	{
		if(factor < 0.5f)
			return Color.Lerp(new Color(1, 0, 0.3019f, 1), new Color(1, 0, 0, 1), factor * 2);
		else
			return Color.Lerp(new Color(1, 0, 0, 1), new Color(1, 0.3019f, 0, 1), factor * 2 - 1);
		//return Color.Lerp(new Color(1, 0, 0.3019f, 1), new Color(1, 0.3019f, 0, 1), factor);
	}

	public static Color GetCyanColor(float factor)
	{
		if (factor < 0.5f)
			return Color.Lerp(new Color(0, 1, 0.8196f, 1), new Color(0, 1, 1, 1), factor * 2);
		else
			return Color.Lerp(new Color(0, 1, 1, 1), new Color(0, 0.8196f, 1, 1), factor * 2 - 1);
		//return Color.Lerp(new Color(0, 1, 0.8196f, 1), new Color(0, 0.8196f, 1, 1), factor);
	}*/
	//---------------------Perfect Vision version--------------------
	public static Color GetBackGroundColor(float factor)
	{
		if (testmode)
		{
			Color leftcolor, rightcolor;
			if (ColorUtility.TryParseHtmlString(Color_BackLeft, out leftcolor) && ColorUtility.TryParseHtmlString(Color_BackRight, out rightcolor))
				return Color.Lerp(leftcolor, rightcolor, factor);
			else return Color.black;
		}
		else
			return Color.Lerp(new Color(0.8784f, 0.8784f, 0.8784f, 1), new Color(1, 1, 1, 1), factor);
	}
	public static Color GetRedColor(float factor)
	{
		if (testmode)
		{
			Color leftcolor, rightcolor;
			if (ColorUtility.TryParseHtmlString(Color_RedLeft, out leftcolor) && ColorUtility.TryParseHtmlString(Color_RedRight, out rightcolor))
				return Color.Lerp(leftcolor, rightcolor, factor);
			else return Color.black;
		}
		else
		{
			string profilestr = GameState.currentPatient == null ?
				PlayerPrefs.GetString(ColorCalibration.PrefName_Profile, PROFILE_DEFAULT)
				: GameState.currentPatient.cali.profileStr;
			if (string.IsNullOrEmpty(profilestr))
				profilestr = PROFILE_DEFAULT;
			string[] colorstrs = profilestr.Split(new char[] { '-', ' '}, System.StringSplitOptions.RemoveEmptyEntries);
			if(colorstrs.Length < 2)
				return Color.black;
			Color leftcolor, rightcolor;
			if (ColorUtility.TryParseHtmlString(colorstrs[0], out leftcolor) && ColorUtility.TryParseHtmlString(colorstrs[1], out rightcolor))
				return Color.Lerp(leftcolor, rightcolor, factor);
			else return Color.black;
		}
			
	}

	public static Color GetCyanColor(float factor)
	{

		if (testmode)
		{
			Color leftcolor, rightcolor;
			if (ColorUtility.TryParseHtmlString(Color_CyanLeft, out leftcolor) && ColorUtility.TryParseHtmlString(Color_CyanRight, out rightcolor))
			{
				if (factor < 0.5f)
					return Color.Lerp(leftcolor, new Color(0, 1, 1, 1), factor * 2);
				else
					return Color.Lerp(new Color(0, 1, 1, 1), rightcolor, factor * 2 - 1);
			}
			else return Color.black;
		}
		else if (factor < 0.5f)
			return Color.Lerp(new Color(0, 1, 0.8196f, 1), new Color(0, 1, 1, 1), factor * 2);
		else
			return Color.Lerp(new Color(0, 1, 1, 1), new Color(0, 0.8196f, 1, 1), factor * 2 - 1);
	}

	public static void OnPatientChanged()
	{
		if (GameState.currentPatient == null)
			return;
		backcolor = GetBackGroundColor(GameState.currentPatient.cali.bg);
		redcolor = GetRedColor(GameState.currentPatient.cali.rd);
		cyancolor = GetCyanColor(GameState.currentPatient.cali.cy);
	}
}
