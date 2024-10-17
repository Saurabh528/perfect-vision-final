using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;

public class CalorimeterController : MonoBehaviour
{
    [SerializeField] GameObject rocordTmpl, objPreview;
    List<Transform> selectedItems = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        ShowRecords();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowRecords(){
        UtilityFunc.DeleteAllSideTransforms(rocordTmpl.transform, false);
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
    public static float APP_VERSION = 1.0f;
    public static string csvFileName = $"{UtilityFunc.GetCalorimeterDataDir()}/Records.csv";
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

    public CalorimeterData(){
        version = APP_VERSION;
        records = new Dictionary<string, CalorimeterRecord>();
    }

    static CalorimeterData _instance;
    public static CalorimeterData Instance{
        get {
            if(_instance != null)
                return _instance;
            if(File.Exists(csvFileName)){
                string text = File.ReadAllText(csvFileName);
                _instance = JsonConvert.DeserializeObject<CalorimeterData>(text);
                return _instance;
            }
            _instance = new CalorimeterData();
            _instance.Save(csvFileName);
            return _instance;
        }
    }

    public void AddRecord(string datetimeStr, CalorimeterRecord record){
        records[datetimeStr] = record;        
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
