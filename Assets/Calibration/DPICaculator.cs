using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DPICaculator : MonoBehaviour
{
    [SerializeField] Transform _cardTrans;
    [SerializeField] CanvasScaler _canvasScaler;
    // Start is called before the first frame update
    void Start()
    {
        float scale = VisualFactor.dpi * 3.37f * _canvasScaler.referenceResolution.x / Screen.width / 523;
		_cardTrans.localScale = Vector3.one * scale;
		GetComponent<Slider>().value = scale;
	}

    public static string GetDPIPath()
    {
        return Application.dataPath + "/../Python/DPI.txt";
	}

    public void OnSliderChange(float value)
    {
		_cardTrans.localScale = Vector3.one * value;

	}

    public void OnBtnComplete()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python.exe"; // Or the full path to python.exe if not in environment variables
        start.Arguments = "\"D:\\PROJECTS\\perfect-vision-aman2\\Python\\card_callib_final.py\""; // Add any arguments your script needs
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
        }

        //      float pixels = _cardTrans.localScale.x * 523 * Screen.width / _canvasScaler.referenceResolution.x;
        //VisualFactor.dpi = pixels / 3.37f;
        //string _dpiPath = GetDPIPath();
        //File.WriteAllLines(_dpiPath, new string[] { GameState.currentPatient == null ? "Anonymous" : GameState.currentPatient.name, VisualFactor.dpi.ToString() });

        if (File.Exists(PatientMgr.GetPatientDataDir() + "/focus_final.csv"))
            ChangeScene.LoadScene("DeviceSetting");
        else
            ChangeScene.LoadScene("ScreenDistance");
	}
}
