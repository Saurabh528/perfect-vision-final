using System.Collections;
using System.Collections.Generic;
using UI.Tables;
using UnityEngine;
using UnityEngine.UI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

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
    DiagnoseRecord currentRecord;
    public void ShowRecord(DiagnoseRecord record, string titleText){
        currentRecord = record;
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
                newObj.transform.Find("Result").Find("Text").GetComponent<Text>().text = GetResultValueString(str);
                newObj.transform.Find("Unit").Find("Text").GetComponent<Text>().text = structure[itemcount].unit;
                newObj.transform.Find("Norm").Find("Text").GetComponent<Text>().text = structure[itemcount].norm;
                newObj.transform.Find("Comment").Find("Text").GetComponent<Text>().text = structure[itemcount].comment == "?"?GetCommentString(str):structure[itemcount].comment;

                //resize columns
                if(string.IsNullOrEmpty(structure[itemcount].unit) && string.IsNullOrEmpty(structure[itemcount].norm)){
                    newObj.transform.Find("Unit").gameObject.SetActive(false);
                    newObj.transform.Find("Norm").gameObject.SetActive(false);
                    newObj.transform.Find("Result").GetComponent<TableCell>().columnSpan = 3;
                }
                else if(string.IsNullOrEmpty(structure[itemcount].unit)){
                    newObj.transform.Find("Unit").gameObject.SetActive(false);
                    newObj.transform.Find("Result").GetComponent<TableCell>().columnSpan = 2;
                }
                itemcount++;
            }
        }
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 sizeDelta = rt.sizeDelta;
        sizeDelta.y = 436 + (siblingIndex - orgSiblingIndex) * rowTmpl.rect.height;
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

    string GetResultValueString(string result){
        if(string.IsNullOrEmpty(result))
            return "";
        else if(result.Contains(":")){
            string []words = result.Split(':', System.StringSplitOptions.RemoveEmptyEntries);
            if(words.Length > 0)
                return words[0];
            else
                return "";
        }
        else
            return result;
    }

    string GetCommentString(string result){
        if(string.IsNullOrEmpty(result))
            return "";
        else if(result.Contains(":")){
            string []words = result.Split(':', System.StringSplitOptions.RemoveEmptyEntries);
            if(words.Length == 2)
                return words[1];
            else
                return "";
        }
        else
            return "";
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
            string[] lines = fileContent.Split(new[] { '\r', '\n' });
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

    public void DownloadPDF(){
        if(currentRecord == null)
            return;
        if (GameState.currentPatient == null)
			return;
        string dir = PatientMgr.GetTherapyResultDir(); // If directory does not exist, create it.
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = dir + GameState.currentPatient.name + "-Diagnosis.pdf";

        PdfWriter writer;
		Document document;
		if (!PDFUtil.CreatePDFHeader(path, out document, out writer))
			return;
		iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
		
		PDFUtil.AddSectionBar(textTitle.text, document);
        Dictionary<string, DiagnoseTestItem> testItems = currentRecord.GetTestItems();

        //int rowCountInTable = 0;
		//PDFUtil.AddSubSection("Session HighScore", document);
		PdfPTable table = GetNewTable(document);
        //rowCountInTable++;
        foreach(KeyValuePair<string, DiagnoseTestItem> pair in testItems){
            if(pair.Value.version < GameVersion.DIAGNOSTICS){
                continue;
            }
            if(!itemstructures.ContainsKey(pair.Key))
                LoadItemStructure(pair.Key);
            if(!itemstructures.ContainsKey(pair.Key) || !gamecomments.ContainsKey(pair.Key))
                continue;
            List<DiagnoItemStructure> structure =  itemstructures[pair.Key];
            if(structure.Count == 0)
                continue;
           /*  GameObject newHeader = Instantiate(gameheaderTmpl.gameObject, gameheaderTmpl.transform.parent);
            newHeader.transform.SetSiblingIndex(siblingIndex++);
            newHeader.SetActive(true);
            newHeader.transform.Find("GameName").Find("Text").GetComponent<Text>().text = pair.Key;
            newHeader.transform.Find("Comment").Find("Text").GetComponent<Text>().text = gamecomments[pair.Key]; */
            //insert game row
            AddGameRowToPDF(table, pair.Key, gamecomments[pair.Key]);
            //rowCountInTable++;
             int itemcount = 0;
            foreach(string str in pair.Value.strings){
                if(itemcount == structure.Count)
                    break;
                AddTestRowToPDF(table, structure[itemcount].Name, GetResultValueString(str), structure[itemcount].unit, structure[itemcount].norm, structure[itemcount].comment == "?"?GetCommentString(str):structure[itemcount].comment);
                //rowCountInTable++;
                /* float pos = writer.GetVerticalPosition(true);
                if(pos < 30 * (rowCountInTable + 1) + document.BottomMargin){
                    document.Add(table);
                    document.NewPage();
                    table = GetNewTable(document);
                    rowCountInTable = 1;
                    if(itemcount + 1 < structure.Count){
                        AddGameRowToPDF(table, pair.Key, gamecomments[pair.Key]);
                        rowCountInTable++;
                    }
                } */
                itemcount++;
            }
        }
        document.Add(table);
		document.Add(new Paragraph("\n"));
        document.Close();
        //view pdf
		UtilityFunc.StartProcessByFile(path);
    }

    void AddTableHeaderToPDF(PdfPTable table){
        iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(bfntHead, 12, 1, iTextSharp.text.BaseColor.WHITE);
        int alignHor = Element.ALIGN_CENTER;
        float height = 30;
        int alignVer = Element.ALIGN_MIDDLE;
        BaseColor backColor = BaseColor.BLUE;

        PdfPCell cell = new PdfPCell(new Phrase("Tests", cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase("Result", cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase("Unit", cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase("Norm", cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase("Comment", cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);
    }

    void AddGameRowToPDF(PdfPTable table, string name, string comment){
        iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        iTextSharp.text.Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
        int alignHor = Element.ALIGN_CENTER;
        float height = 30;
        int alignVer = Element.ALIGN_MIDDLE;
        BaseColor backColor = BaseColor.WHITE;

        PdfPCell cell = new PdfPCell(new Phrase(name, cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase(comment, cellFont));
        cell.Colspan = 4;
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);
    }

    void AddTestRowToPDF(PdfPTable table, string test, string result, string unit, string norm, string comment){
        iTextSharp.text.pdf.BaseFont bfntHead = GameResource.SerifFont;
        iTextSharp.text.Font cellFont = new iTextSharp.text.Font(bfntHead, 12, iTextSharp.text.Font.NORMAL);
        int alignHor = Element.ALIGN_CENTER;
        float height = 30;
        int alignVer = Element.ALIGN_MIDDLE;
        BaseColor backColor = BaseColor.WHITE;

        PdfPCell cell = new PdfPCell(new Phrase(test, cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);

        cell = new PdfPCell(new Phrase(result, cellFont));
        if(string.IsNullOrEmpty(unit) && string.IsNullOrEmpty(norm))
            cell.Colspan = 3;
        else if(string.IsNullOrEmpty(unit))
            cell.Colspan = 2;
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);
        if(!string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(norm)){
            cell = new PdfPCell(new Phrase(unit, cellFont));
            cell.HorizontalAlignment = alignHor;
            cell.FixedHeight = height;
            cell.VerticalAlignment = alignVer;
            cell.BackgroundColor = backColor;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(norm, cellFont));
            cell.HorizontalAlignment = alignHor;
            cell.FixedHeight = height;
            cell.VerticalAlignment = alignVer;
            cell.BackgroundColor = backColor;
            table.AddCell(cell);
        }
        else if(!string.IsNullOrEmpty(unit)){
            cell = new PdfPCell(new Phrase(unit, cellFont));
            cell.Colspan = 2;
            cell.HorizontalAlignment = alignHor;
            cell.FixedHeight = height;
            cell.VerticalAlignment = alignVer;
            cell.BackgroundColor = backColor;
            table.AddCell(cell);
        }

        cell = new PdfPCell(new Phrase(comment, cellFont));
        cell.HorizontalAlignment = alignHor;
        cell.FixedHeight = height;
        cell.VerticalAlignment = alignVer;
        cell.BackgroundColor = backColor;
        table.AddCell(cell);
        
    }

    PdfPTable GetNewTable(Document document){
        PdfPTable table = new PdfPTable(5);
		table.WidthPercentage = 50000f / (document.PageSize.Width - document.RightMargin * 1.5f - document.LeftMargin * 1.5f);
        float[] columnWidths = new float[] { 3f, 1f, 1f, 2f, 3f};
        table.SetWidths(columnWidths);
        AddTableHeaderToPDF(table);
        return table;
    }
}
