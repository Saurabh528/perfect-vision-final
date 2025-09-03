using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using System;

public class CalorimeterController : MonoBehaviour
{
    [SerializeField] GameObject rocordTmpl, objPreview;
    [SerializeField] TextMeshProUGUI textTitle;
    List<Transform> selectedItems = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        if (GameState.currentPatient == null)
            textTitle.text = "Please select patient from Enrollment page";
        else
            textTitle.text = $"{GameState.currentPatient.name}'s Calorimeter Records";

		ShowRecords();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowRecords(){
        UtilityFunc.DeleteAllSideTransforms(rocordTmpl.transform, false);
        if (CalorimeterData.Instance == null)
            return;
        CalorimeterData cData = CalorimeterData.Instance;
        foreach(KeyValuePair<string, CalorimeterRecord> pair in cData.records){
            GameObject newObj = Instantiate(rocordTmpl, rocordTmpl.transform.position, rocordTmpl.transform.rotation, rocordTmpl.transform.parent);
            newObj.name = pair.Key;
            newObj.transform.localScale = rocordTmpl.transform.localScale;
            newObj.SetActive(true);
            string filename = $"{UtilityFunc.GetCalorimeterDataDir()}/{pair.Key}";
            if(File.Exists(filename)){
                Texture2D texture = new Texture2D(1, 1);
                bool loaded = texture.LoadImage(File.ReadAllBytes(filename));
                if(loaded)
                    newObj.transform.Find("ScreenShot").GetComponent<RawImage>().texture = texture;
            }
            newObj.transform.Find("GameName").GetComponent<TextMeshProUGUI>().text = pair.Key;
            newObj.transform.Find("RGBText").GetComponent<TextMeshProUGUI>().text = UtilityFunc.UIntColor2RGBString(pair.Value.RGB);
            newObj.transform.Find("RedCaliText").GetComponent<TextMeshProUGUI>().text = UtilityFunc.UIntColor2RGBString(pair.Value.CalibRed);
            newObj.transform.Find("CyanCaliText").GetComponent<TextMeshProUGUI>().text = UtilityFunc.UIntColor2RGBString(pair.Value.CalibCyan);
            newObj.transform.Find("BackCaliText").GetComponent<TextMeshProUGUI>().text = UtilityFunc.UIntColor2RGBString(pair.Value.CalibBack);
        }
    }

    public void OnClickCaloriScreenShot(GameObject rawImageObj){
        Texture texture = rawImageObj.GetComponent<RawImage>().texture;
        if(texture){
            objPreview.SetActive(true);
            objPreview.transform.Find("RawImage").GetComponent<RawImage>().texture = texture;
        }
    }

    public void OnBtnDelete(){
        Transform parent = rocordTmpl.transform.parent;
        selectedItems.Clear();
        for(int i = 0; i < parent.childCount; i++){
            Transform child = parent.GetChild(i);
            if(child != rocordTmpl.transform && child.Find("Toggle").GetComponent<Toggle>().isOn)
                selectedItems.Add(child);
        }
        if(selectedItems.Count == 0){
            PopupUI.ShowNotification("Select records to delete first.");
            return;
        }
        PopupUI.ShowQuestionBox("Are you sure to delete selected records?", DeleteSelectedRecords);
        
    }

    public void DeleteSelectedRecords(){
        CalorimeterData cData = CalorimeterData.Instance;
        foreach(Transform child in selectedItems){
            cData.DeleteRecord(child.name);
        }
        selectedItems.Clear();
        cData.Save(CalorimeterData.csvFileName);
        ShowRecords();
    }
}

public class CalorimeterRecord{
    public uint RGB, CalibRed, CalibCyan, CalibBack;

    public CalorimeterRecord(uint rgb, uint caliR, uint caliC, uint caliBack){
        RGB = rgb;
        CalibRed = caliR;
        CalibCyan = caliC;
        CalibBack = caliBack;
    }
}

[JsonObject(MemberSerialization.Fields)]
public class CalorimeterData{
    public static float APP_VERSION = 1.1f;
    public static string csvFileName;
    public float version;
    public Dictionary<string, CalorimeterRecord> records;//(datetime, record)
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (records == null || version < APP_VERSION)
        {
            records = new Dictionary<string, CalorimeterRecord>();
        }
    }

    public static void ClearData()
    {
		_instance = null;
        if(GameState.currentPatient != null)
		    Directory.CreateDirectory(UtilityFunc.GetCalorimeterDataDir());
	}

    public static void DeleteAllData()
    {
        if (GameState.currentPatient == null)
            return;
        string dirPath = UtilityFunc.GetCalorimeterDataDir();
        if (Directory.Exists(dirPath))
        {
			// Get all files in the directory
			string[] files = Directory.GetFiles(dirPath);

			// Loop through and delete each file
			foreach (string file in files)
			{
				File.Delete(file);
			}
            _instance = null;
		}
        else
            ClearData();
	}

    public CalorimeterData(){
        version = APP_VERSION;
        records = new Dictionary<string, CalorimeterRecord>();
    }

    static CalorimeterData _instance;
    public static CalorimeterData Instance{
        get {
            if (GameState.currentPatient == null)
                return null;
            if(_instance != null)
                return _instance;

			csvFileName = $"{UtilityFunc.GetCalorimeterDataDir()}/Records.csv";
			if (File.Exists(csvFileName)){
                string text = File.ReadAllText(csvFileName);
                _instance = JsonConvert.DeserializeObject<CalorimeterData>(text);
                //Delete all data if version mismatch
                if(_instance.version < CalorimeterData.APP_VERSION){
                    CalorimeterData.DeleteAllData();
                    _instance = new CalorimeterData();
                    _instance.Save(csvFileName);
                }
                return _instance;
            }
            _instance = new CalorimeterData();
            _instance.Save(csvFileName);
            return _instance;
        }
    }

    public void AddRecord(string gamename, CalorimeterRecord record){
        records[gamename] = record;        
    }

    public void DeleteRecord(string filename){
        if(records.ContainsKey(filename))
            records.Remove(filename);
        string absoluteFileName = $"{UtilityFunc.GetCalorimeterDataDir()}/{filename}";
        if(File.Exists(absoluteFileName))
            File.Delete(absoluteFileName);
    }

    public void Save(string filepath){
        File.WriteAllText(filepath, JsonConvert.SerializeObject(this));
    }
}
