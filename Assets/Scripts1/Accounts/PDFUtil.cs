using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
public abstract class PDFUtil
{
	public static iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
	public static bool CreatePDFHeader(string pathname, out Document document, out PdfWriter writer)
	{
		document = new Document();
		writer = null;
		try
		{
			writer = PdfWriter.GetInstance(document, new FileStream(pathname, FileMode.Create));
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
			return false;
		}
		document.SetPageSize(iTextSharp.text.PageSize.A4);
		writer.PageEvent = MyPdfPageEventHandler.Instance; //This will trigger the code above
		document.Open();
		//Report Header

		iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bfntHead, 24, 1, iTextSharp.text.BaseColor.YELLOW);
		Paragraph prgHeading = new Paragraph();
		prgHeading.Alignment = Element.ALIGN_CENTER;
		Chunk chkTitle = new Chunk("BinoPlay Therapy Record", fntHead);
		chkTitle.SetBackground(new iTextSharp.text.BaseColor(74, 5, 82), 185, 10, 185, 30);
		prgHeading.Add(chkTitle);
		prgHeading.SpacingAfter = 30;
		document.Add(prgHeading);

		//Author
		Paragraph prgAuthor = new Paragraph();
		iTextSharp.text.pdf.BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
		iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 14, 2, iTextSharp.text.BaseColor.BLACK);
		prgAuthor.Alignment = Element.ALIGN_RIGHT;
		prgAuthor.Add(new Chunk("Doctor : " + GameState.username, fntAuthor));
		prgAuthor.Add(new Chunk("\nDate : " + DateTime.Now.ToShortDateString(), fntAuthor));
		prgAuthor.SpacingAfter = 0;

		document.Add(prgAuthor);

		//Add a line seperation
		Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(3.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
		p.SpacingBefore = 0;
		p.SpacingAfter = 20;
		document.Add(p);

		//Add patient info
		if(GameState.currentPatient != null)
		{
			iTextSharp.text.Font fntPatient = new iTextSharp.text.Font(bfntHead, 12, 0, iTextSharp.text.BaseColor.BLACK);
			Paragraph prgPatient = new Paragraph();
			prgPatient.Alignment = Element.ALIGN_LEFT;

			prgPatient.Add(new Chunk("Patient Name: " + PatientMgr.GetCurrentPatientName(), fntPatient));
			prgPatient.Add(new Chunk("\nAge: " + GameState.currentPatient.age.ToString(), fntPatient));
			prgPatient.Add(new Chunk("\nGender: " + GameState.currentPatient.gender.ToString(), fntPatient));
			prgPatient.Add(new Chunk("\nDetails: " + GameState.currentPatient.details.ToString(), fntPatient));
			document.Add(prgPatient);
			document.Add(new Paragraph("\n"));
			//document.Add(new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_LEFT, 1))));
		}
		
		
		return true;
	}

	public static void AddSectionBar(string sectionname, Document document)
	{
		//Add Progression Anylysis
		iTextSharp.text.Font fntSubTitle = new iTextSharp.text.Font(bfntHead, 18, 1, iTextSharp.text.BaseColor.WHITE);
		Paragraph prgSubTitle = new Paragraph();
		prgSubTitle.Alignment = Element.ALIGN_CENTER;
		Chunk chkSubTitle = new Chunk(sectionname + "\n", fntSubTitle);
		chkSubTitle.SetBackground(new iTextSharp.text.BaseColor(55, 55, 85), 185, 10, 185, 10);
		prgSubTitle.Add(chkSubTitle);
		prgSubTitle.SpacingAfter = 40;
		document.Add(prgSubTitle);
	}

	public static void AddSubSection(string subsectionname, Document document)
	{
		iTextSharp.text.Font fntHighscore = new iTextSharp.text.Font(bfntHead, 16, 1, iTextSharp.text.BaseColor.BLACK);
		Paragraph prgHighscore = new Paragraph();
		prgHighscore.Alignment = Element.ALIGN_CENTER;
		prgHighscore.SpacingAfter = 10;
		Chunk chkhighscore = new Chunk(subsectionname, fntHighscore);
		prgHighscore.Add(chkhighscore);
		document.Add(prgHighscore);
	}

	public static void ShowPower10(int power, PdfContentByte contentByte, float topX, float topY)
	{
		contentByte.BeginText();
		contentByte.SetFontAndSize(PDFUtil.bfntHead, 12);
		contentByte.ShowTextAligned(Element.ALIGN_LEFT, "x10", topX, topY + 5, 0);
		contentByte.SetFontAndSize(PDFUtil.bfntHead, 8);
		contentByte.ShowTextAligned(Element.ALIGN_LEFT, (-power).ToString(), topX + 17, topY + 15, 0);
		contentByte.EndText();
	}
}
