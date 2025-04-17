using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

public class DocxParagraphExpander
{
    public static void AddNewParagraph(string filePath, string newParagraphText)
    {
        using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true))
        {
            MainDocumentPart? mainPart = doc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                Paragraph para = new Paragraph(new Run(new Text(newParagraphText)));
                mainPart.Document.Body.AppendChild(para);
            }
        }
    }

    //public static void Main(string[] args)
    //{
    //    string docxFile = "your_document.docx"; // Replace with your DOCX file path
    //    string textToAdd = "This is the expanded paragraph content.";
    //    AddNewParagraph(docxFile, textToAdd);
    //}
}