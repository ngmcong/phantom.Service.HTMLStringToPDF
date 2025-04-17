using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

public class PdfTextExtractor
{
    public static string? ExtractText(string pdfFilePath)
    {
        StringBuilder text = new StringBuilder();

        try
        {
            using (PdfReader reader = new PdfReader(pdfFilePath))
            {
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        PdfCanvasProcessor processor = new PdfCanvasProcessor(strategy);
                        processor.ProcessPageContent(pdfDoc.GetPage(i));
                        string pageText = strategy.GetResultantText();
                        text.AppendLine(pageText);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
            return null;
        }

        return text.ToString();
    }
}