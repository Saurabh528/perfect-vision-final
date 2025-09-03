using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using iTextSharp.text.pdf.qrcode;
using UnityEngine.UI;


public abstract class UtilityFunc
{
	static string logFilePath{
		get{
			return Application.persistentDataPath + "/Application.log";
		}
	}
	public static string ComputeSha256Hash(string rawData)
	{
		// Create a SHA256   
		using (SHA256 sha256Hash = SHA256.Create())
		{
			// ComputeHash - returns byte array  
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

			// Convert byte array to a string   
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}

	public static void DeleteAllSideTransforms(Transform trans, bool nameInclude = true)
	{
		//if nameInclude = true, only delete items with name containing trans's name
		if (trans == null || trans.parent == null)
			return;
		foreach(Transform t in trans.parent)
		{
			if (t != trans && (!nameInclude || t.name.Contains(trans.name)))
				GameObject.Destroy(t.gameObject);
		}
	}

	public static void DeleteAllChildTransforms(Transform parent)
	{
		foreach (Transform child in parent)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	public static string ColorChannelToName(ColorChannel channel)
	{
		if (channel == ColorChannel.CC_Background)
			return "Background";
		else if (channel == ColorChannel.CC_Red)
			return "Red";
		else if (channel == ColorChannel.CC_Cyan)
			return "Blue";
		return "";
	}

	public static UInt32 Color2Int(UnityEngine.Color c)//0xrrggbbaa
	{
		Color32 c32 = c;
		return (UInt32)(c32[0] << 24) +
			(UInt32)(c32[1] << 16) +
			(UInt32)(c32[2] << 8) +
			(UInt32)c32[3];
	}

	public static void UInt2RGB(uint color32, out byte r, out byte g, out byte b, out byte a)
	{
		r = (byte)(color32 >> 24);
		g = (byte)((color32 >> 16) & 0xff);
		b = (byte)((color32 >> 8) & 0xff);
		a = (byte)(color32 & 0xff);
	}

	public static string UIntColor2RGBString(uint color32){
		byte r, g, b, a;
		UInt2RGB(color32, out r, out g, out b, out a);
		return $"({r}, {g}, {b})";
	}

	public static UnityEngine.Color UInt2Color(uint color32){//0xrrggbbaa
		byte r, g, b, a;
		UInt2RGB(color32, out r, out g, out b, out a);
		return new UnityEngine.Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255);
	}

	public static void StartProcessByFile(string fileName)
	{
		if (!File.Exists(fileName))
			return;
		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = fileName;
		psi.UseShellExecute = true;
		psi.WindowStyle = ProcessWindowStyle.Normal;
		Process.Start(psi);
	}

	public static System.Drawing.Image Texture2Image(Texture2D texture)
	{
		byte[] bytes = texture.EncodeToPNG();
		return System.Drawing.Image.FromStream(new MemoryStream(bytes));
	}

	public static void SetRawImageFromFile(RawImage image, string fileName){
		if(string.IsNullOrEmpty(fileName))
			return;
		Texture2D texture = new Texture2D(2, 2);
		byte[] imageData = File.ReadAllBytes(fileName);
        texture.LoadImage(imageData);
		if(texture != null)
			image.texture = texture;
	}

	public static string Base64Encode(string text)
	{
		var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
		return System.Convert.ToBase64String(textBytes);
	}
	public static string Base64Decode(string base64)
	{
		var base64Bytes = System.Convert.FromBase64String(base64);
		return System.Text.Encoding.UTF8.GetString(base64Bytes);
	}

	public static string GetAbsolutePath(string projectrelative){
		return $"{Application.dataPath}/../{projectrelative}";
	}

	public static string GetFullDirFromApp(string relative){
		return System.IO.Path.GetFullPath(Application.dataPath + "/../" + relative);
	}

	public static string GetPythonPath(){
		if(Application.platform == RuntimePlatform.WindowsEditor)
			return "python.exe";
		//else if(Application.platform == RuntimePlatform.OSXEditor)
		//	return "/Library/Frameworks/Python.framework/Versions/3.11/bin/python3";
		else if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			return GetFullDirFromApp("/Python/PerfectVision/bin/python3");
		else return "";
	}

	public static string GetPythonExecutablePath(string path, string gamename){
		string executablePath;
		if(Application.platform == RuntimePlatform.WindowsPlayer)
			executablePath = Path.Combine(path, $"{gamename}.exe");
		else
			executablePath = UtilityFunc.GetPythonPath();
		if (!File.Exists(executablePath))
		{
			UtilityFunc.AppendToLog($"Executable not found at {executablePath}");
			return "";  // Stop further execution if the file does not exist
		}
		return executablePath;
	}

	public static string GetPlatformSpecificExecutableExtension(){
		return (Application.platform == RuntimePlatform.WindowsPlayer)?".exe":"";
	}

	public static void AppendToLog(string text)
	{
		try
		{
			// Ensure the directory exists
			string directory = Path.GetDirectoryName(logFilePath);
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Append text to the log file
			using (StreamWriter sw = File.AppendText(logFilePath))
			{
				sw.WriteLine($"{System.DateTime.Now}: {text}");
			}
		}
		catch (System.Exception ex)
		{
			UnityEngine.Debug.LogError("Failed to write to log file: " + ex.Message);
		}
	}

	public static void CopyRectTransform(RectTransform source, RectTransform target)
    {
        if (source == null || target == null)
        {
            return;
        }

        // Copy anchors
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;

        // Copy pivot
        target.pivot = source.pivot;

        // Copy anchored position
        target.anchoredPosition = source.anchoredPosition;

        // Copy size delta
        target.sizeDelta = source.sizeDelta;

        // Copy local scale
        target.localScale = source.localScale;

        // Copy rotation
        target.localRotation = source.localRotation;

        // Optionally, you can copy other properties such as offsets
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
    }

	public static string ConvertSec2MMSS(float time){
		int sec = (int)time;
		int minutes = (int)(sec / 60);
		int seconds = (int)(sec % 60);
		return $"{minutes:00}:{seconds:00}";
	}

	public static string GetCalorimeterDataDir()
	{
		return PatientMgr.GetPatientDataDir() + "/CalorimeterRecords";
	}

	public static void PlayAudioClipFromList(AudioClip[] clips, string text, AudioSource source){
		if(source.loop == true)
			source.loop = false;
		foreach(AudioClip clip in clips){
			if(clip.name == text){
				if(source.isPlaying)
					source.Stop();
				source.clip = clip;
				source.Play();
				return;
			}
		}
	}

	public static void DeleteDir(String targetDirectory)
	{
		try
		{
			DirectoryInfo dir = new DirectoryInfo(targetDirectory);

			// Delete all files
			foreach (FileInfo file in dir.GetFiles())
			{
				file.Delete();
			}

			// Delete all subdirectories
			foreach (DirectoryInfo subDir in dir.GetDirectories())
			{
				subDir.Delete(true); // true ensures recursive deletion
			}

			Console.WriteLine("All files and subdirectories removed successfully.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
}
