using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeviceSettingMgr : MonoBehaviour
{
	public GameObject backBtn;
	public GameObject nextBtn;
	private void Start()
	{
		/* if (GameState.IsPatient())
		{
			backBtn.SetActive(false);
			nextBtn.SetActive(true);
		}
		if(GameState.IsDoctor())
		{
            backBtn.SetActive(true);
            nextBtn.SetActive(false);
		} */
		Cursor.visible = true;
		GameState.currentGameMode = GAMEMODE.DeviceSetting;
		Directory.CreateDirectory(PatientMgr.GetPatientDataDir());
		/* string _dpiPath = Application.dataPath + "/../Python/DPI.txt";
		if (!File.Exists(_dpiPath))
			File.WriteAllLines(_dpiPath, new string[] { GameState.currentPatient == null ? GameConst.PATIENTNAME_ANONYMOUS : GameState.currentPatient.name, "100.0" });
		else
		{
			string[] strs = File.ReadAllLines(_dpiPath);
			strs[0] = GameState.currentPatient == null ? GameConst.PATIENTNAME_ANONYMOUS : GameState.currentPatient.name;
			File.WriteAllLines(_dpiPath, strs);
		} */
	}
	public void OnBtnBack()
	{
        //if (GameState.currentPatient == null)
        if (GameState.IsDoctor())
            ChangeScene.LoadScene("ModePanel");
		else
			ChangeScene.LoadScene(GameState.IsDoctor()? "Enrollment": "HomeTherapy");
	}
}
