using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using TMPro;
public class CoverResultView : MonoBehaviour
{
    public CoverJSonResultView _viewtmpl;
	[SerializeField] TextMeshProUGUI _leftMean, _leftStd, _rightMean, _rightStd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowResult(CoverUncoverResultData resultdata)
    {
       	if(!resultdata.isValid)
			return;
			_leftMean.text = resultdata.leftMean.ToString();
			_leftStd.text = resultdata.leftStd.ToString();
			_rightMean.text = resultdata.rightMean.ToString();
			_rightStd.text = resultdata.rightStd.ToString();

	}

    void CreateTableFromFile(string title, string filepathname)
    {
		GameObject go = (GameObject)Instantiate(_viewtmpl.gameObject, _viewtmpl.transform.position, _viewtmpl.transform.rotation);
		go.name = title;
		go.transform.SetParent(_viewtmpl.transform.parent);
		go.transform.localScale = _viewtmpl.transform.localScale;
		go.SetActive(true);
		CoverJSonResultView tableView = go.GetComponent<CoverJSonResultView>();
        if (tableView)
            tableView.ShowJSonContent(title, filepathname);
	}

    public void PrintAndShowPDF(CoverUncoverResultData resultdata)
    {
		if(resultdata == null || !resultdata.isValid)
			return;
		string dir = PatientMgr.GetTherapyResultDir(); // If directory does not exist, create it.
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		string path = dir + PatientMgr.GetCurrentPatientName() + ".pdf";

		PdfWriter writer;
		Document document;
		if (!PDFUtil.CreatePDFHeader(path, out document, out writer))
			return;
		PDFUtil.AddSectionBar("CoverUncover Result", document);
		
		PDFUtil.AddSubSection("Eye statistics", document);
		PdfPTable table = new PdfPTable(3);
		table.WidthPercentage = 50000f / (document.PageSize.Width - document.RightMargin - document.LeftMargin);
		iTextSharp.text.Font cellFont = new iTextSharp.text.Font(PDFUtil.bfntHead, 11, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
		iTextSharp.text.Font cellFontBold = new iTextSharp.text.Font(PDFUtil.bfntHead, 11, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
		BaseColor headerColor = new BaseColor(230, 255, 253);
		BaseColor valueColor = new BaseColor(189, 178, 253);
		float cellheight = 20;
		PdfPCell cell = new PdfPCell(new Phrase("Eye", cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = headerColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase("Mean", cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = headerColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase("Standard Deviation", cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = headerColor; table.AddCell(cell);

		cell = new PdfPCell(new Phrase("Left Eye", cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase(resultdata.leftMean.ToString(), cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase(resultdata.leftStd.ToString(), cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);

		cell = new PdfPCell(new Phrase("Right Eye", cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase(resultdata.rightMean.ToString(), cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);
		cell = new PdfPCell(new Phrase(resultdata.rightStd.ToString(), cellFontBold));	cell.HorizontalAlignment = Element.ALIGN_CENTER; cell.FixedHeight = cellheight;	cell.VerticalAlignment = Element.ALIGN_MIDDLE; cell.BackgroundColor = valueColor; table.AddCell(cell);

		document.Add(table);
		/* DrawTableToPDF(document, "Before Diagnose (LEFT)", PatientMgr.GetPatientDataDir() + "/beforeleft.json");
		document.NewPage();
		DrawTableToPDF(document, "After Diagnose (LEFT)", PatientMgr.GetPatientDataDir() + "/afterleft.json");
		document.NewPage();
		DrawTableToPDF(document, "Before Diagnose (RIGHT)", PatientMgr.GetPatientDataDir() + "/beforeright.json");
		document.NewPage();
		DrawTableToPDF(document, "After Diagnose (RIGHT)", PatientMgr.GetPatientDataDir() + "/afterright.json"); */
		document.Close();
		UtilityFunc.StartProcessByFile(path);
	}

	void DrawTableToPDF(Document document, string title, string filepath)
	{
		if (!File.Exists(filepath))
			return;

		//Similarity table
		PDFUtil.AddSubSection(title, document);
		PdfPTable table = new PdfPTable(7);
		table.WidthPercentage = 50000f / (document.PageSize.Width - document.RightMargin - document.LeftMargin);
		iTextSharp.text.Font cellFont = new iTextSharp.text.Font(PDFUtil.bfntHead, 11, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);
		iTextSharp.text.Font cellFontBold = new iTextSharp.text.Font(PDFUtil.bfntHead, 11, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);
		BaseColor cellcolor1 = new BaseColor(175, 161, 253);
		BaseColor cellcolor2 = new BaseColor(189, 178, 253);

		string[] columnstrs = new string[] { "Factor", "mean", "std", "min", "max", "median", "range" };
		BaseColor[] columncolors = {new BaseColor(188, 165, 140),
		new BaseColor(237, 161, 145),
		new BaseColor(237, 95, 171),
		new BaseColor(169, 95, 254),
		new BaseColor(103, 109, 254),
		new BaseColor(103, 234, 185),
		new BaseColor(181, 234, 145)};
		int count = 0;
		foreach (string str in columnstrs)
		{
			PdfPCell cell = new PdfPCell(new Phrase(str, cellFontBold));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = columncolors[count++];
			table.AddCell(cell);
		}

		string text = File.ReadAllText(filepath);
		float cellheight = 20;
		Dictionary<string, CoverStatisRowData> dic = JsonConvert.DeserializeObject<Dictionary<string, CoverStatisRowData>>(text);
		count = 0;
		foreach (KeyValuePair<string, CoverStatisRowData> pair in dic)
		{
			CoverStatisRowData row = pair.Value;
			PdfPCell cell = new PdfPCell(new Phrase(pair.Key, cellFontBold));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0? cellcolor1: cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.mean), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.std), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.min), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.max), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.median), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(string.Format("{0,12:F3}", row.range), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = cellheight;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			count++;
		}		

		document.Add(table);
		document.Add(new Paragraph("\n\n"));
	}
}
