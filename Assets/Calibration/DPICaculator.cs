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
        return Application.persistentDataPath + "/DPI.txt";
    }

    public void OnSliderChange(float value)
    {
        _cardTrans.localScale = Vector3.one * value;

    }


    public void OnBtnComplete()
    {
        /*ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python.exe"; // Or the full path to python.exe if not in environment variables
        start.Arguments = "{path}/Python/card_callib_final.py"; // Add any arguments your script needs
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
        }*/
        try
        {
			float pixels = _cardTrans.localScale.x * 523 * Screen.width / _canvasScaler.referenceResolution.x;
			VisualFactor.dpi = pixels / 3.37f;
			string _dpiPath = GetDPIPath();
			File.WriteAllLines(_dpiPath, new string[] { GameState.currentPatient == null ? "Anonymous" : GameState.currentPatient.name, VisualFactor.dpi.ToString() });

			if (File.Exists(PatientMgr.GetPatientDataDir() + "/focus_final.csv") || GameConst.MODE_NOCAMERA)
				ChangeScene.LoadScene("DeviceSetting");
			else
				ChangeScene.LoadScene("ScreenDistance");
        }
        catch(Exception e)
        {
            UtilityFunc.AppendToLog(e.ToString());
        }
        
    }

    public static float ConvertCanvasSizeToArcSeconds(Canvas canvas, float pixelWidth, float UIscale, float distanceToScreenMM = 500)
    {
        float pixelInscreen = pixelWidth * UIscale * canvas.scaleFactor;
        float offsetMM = pixelInscreen * 25.4f / ScreenDPI.GetPPI();
        return offsetMM * 206265f / distanceToScreenMM;
    }

    public static float ConvertArcSecondToPixelOffset(Canvas canvas, float arcSec, float UIscale, float distanceToScreenMM = 500)
    {
        float MMInscreen = arcSec * distanceToScreenMM / 206265f;
        float pixelInscreen = MMInscreen * ScreenDPI.GetPPI() / 25.4f;
        float pixelValue = pixelInscreen / UIscale / canvas.scaleFactor;
        if (pixelValue > 0 && pixelValue < 1)
            return 1;
        else if (pixelValue < 0 && pixelValue > -1f)
            return -1;
        return pixelValue;
    }

    public static int ConvertScreenMMToPixel(Canvas canvas, float screenMM, float UIscale)
    {
        //return (int)(screenMM * ScreenDPI.GetPPI() / 25.4f / UIscale);
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
		return (int)(screenMM * ScreenDPI.GetPPI() / 25.4f / UIscale * scaler.referenceResolution.x / Screen.width / scaler.scaleFactor);
	}

}
