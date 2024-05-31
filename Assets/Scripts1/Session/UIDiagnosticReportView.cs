using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DiagnoItemStructure{
    public string Name, unit, norm, comment;
    public DiagnoItemStructure(string nm, string un, string no, string com){
        Name = nm;
        unit = un;
        norm = no;
        comment = com;
    }
}
public class UIDiagnosticReportView : MonoBehaviour
{
    [SerializeField] RectTransform rowTmpl, gameheaderTmpl;
    [SerializeField] Text textTitle;
    static Dictionary<string, List<DiagnoItemStructure>> itemstructures = new Dictionary<string, List<DiagnoItemStructure>>();//key: game name
    static Dictionary<string, string> gamecomments = new Dictionary<string, string>();

    public void ShowRecord(DiagnoseRecord record, string titleText){
        textTitle.text = titleText;
        UtilityFunc.DeleteAllSideTransforms(rowTmpl.transform);
        UtilityFunc.DeleteAllSideTransforms(gameheaderTmpl.transform);
        Dictionary<string, DiagnoseTestItem> testItems = record.GetTestItems();
        int orgSiblingIndex = gameheaderTmpl.transform.GetSiblingIndex() + 1;
        int siblingIndex = orgSiblingIndex;
        List<string> testnamesToRemove = new List<string>();
        foreach(KeyValuePair<string, DiagnoseTestItem> pair in testItems){
            if(pair.Value.version < GameVersion.DIAGNOSTICS){
                testnamesToRemove.Add(pair.Key);
                continue;
            }
            if(!itemstructures.ContainsKey(pair.Key))
                LoadItemStructure(pair.Key);
            if(!itemstructures.ContainsKey(pair.Key) || !gamecomments.ContainsKey(pair.Key))
                continue;
            List<DiagnoItemStructure> structure =  itemstructures[pair.Key];
            if(structure.Count == 0)
                continue;
            GameObject newHeader = Instantiate(gameheaderTmpl.gameObject, gameheaderTmpl.transform.parent);
            newHeader.transform.SetSiblingIndex(siblingIndex++);
            newHeader.SetActive(true);
            newHeader.transform.Find("GameName").Find("Text").GetComponent<Text>().text = pair.Key;
            newHeader.transform.Find("Comment").Find("Text").GetComponent<Text>().text = gamecomments[pair.Key];
            int itemcount = 0;
            foreach(string str in pair.Value.strings){
                if(itemcount == structure.Count)
                    break;
                GameObject newObj = Instantiate(rowTmpl.gameObject, rowTmpl.transform.parent);
                newObj.transform.SetSiblingIndex(siblingIndex++);
                newObj.SetActive(true);
                newObj.transform.Find("Tests").Find("Text").GetComponent<Text>().text = structure[itemcount].Name;
                newObj.transform.Find("Result").Find("Text").GetComponent<Text>().text = str;
                newObj.transform.Find("Unit").Find("Text").GetComponent<Text>().text = structure[itemcount].unit;
                newObj.transform.Find("Norm").Find("Text").GetComponent<Text>().text = structure[itemcount].norm;
                newObj.transform.Find("Comment").Find("Text").GetComponent<Text>().text = structure[itemcount].comment;
                itemcount++;
            }
        }
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 sizeDelta = rt.sizeDelta;
        sizeDelta.y = 320 + (siblingIndex - orgSiblingIndex) * rowTmpl.rect.height;
        rt.sizeDelta = sizeDelta;
        if(siblingIndex == orgSiblingIndex)
            gameObject.SetActive(false);
        if(testnamesToRemove.Count > 0){
            foreach(string testname in testnamesToRemove)
                testItems.Remove(testname);
            PatientRecord pr = PatientDataMgr.GetPatientRecord();
            Dictionary<string, DiagnoseRecord> diagnoseRecords = pr.GetDiagnoseRecords();
            List<string> datestringsToRemove = new List<string>();
            foreach(KeyValuePair<string, DiagnoseRecord> pair in diagnoseRecords){
                if(pair.Value.GetTestItems().Count == 0)
                    datestringsToRemove.Add(pair.Key);
            }
            foreach(string datestring in datestringsToRemove)
                diagnoseRecords.Remove(datestring);
            PatientDataMgr.SavePatientData();
        }
    }

    void LoadItemStructure(string gamename){
         // Load the file as a TextAsset
        TextAsset textAsset = Resources.Load<TextAsset>($"Data/Diagnostics/{gamename}");

        if (textAsset != null)
        {
            // Get the text content from the TextAsset
            string fileContent = textAsset.text;
            List<DiagnoItemStructure> structures = new List<DiagnoItemStructure>();
            // Split the content into lines
            string[] lines = fileContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if(lines.Length == 0){
                Debug.LogError($"File is empty: Resource/Data/Diagnostics/{gamename}");
                return;
            }
            gamecomments[gamename] = lines[0];
            // Iterate through each line
            for (int i = 1; i < lines.Length; i++)
            {
                // Split each line into words separated by ';'
                string[] words = lines[i].Split(';');
                if(words.Length == 4){
                    // Process each word
                    structures.Add(new DiagnoItemStructure(words[0], words[1], words[2], words[3])); 
                }
            }
            itemstructures[gamename] = structures;
        }
        else
        {
            Debug.LogError($"File not found in Resource/Data/Diagnostics/{gamename}");
        }
    }
}
