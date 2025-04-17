using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

public class DocxToTextConverterWithListsRevised
{
    public static string? ConvertDocxToText(string filePath)
    {
        StringBuilder text = new StringBuilder();

        try
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                if (doc.MainDocumentPart != null)
                {
                    var numberingPart = doc.MainDocumentPart.NumberingDefinitionsPart;
                    var numberingInstances = numberingPart?.Numbering?.Elements<NumberingInstance>();
                    var abstractNums = numberingPart?.Numbering?.Elements<AbstractNum>();
                    Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
                    foreach (var element in doc.MainDocumentPart.Document.Body!.Elements())
                    {
                        if (element is Paragraph p)
                        {
                            string prefix = GetListPrefix(p, numberingInstances, abstractNums
                                , keyValuePairs);
                            text.AppendLine($"{prefix}{p.InnerText}");
                        }
                        else if (element is Table tbl)
                        {
                            foreach (var row in tbl.Elements<TableRow>())
                            {
                                foreach (var cell in row.Elements<TableCell>())
                                {
                                    text.Append(cell.InnerText);
                                    text.Append("\t");
                                }
                                text.AppendLine();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting DOCX to text with lists (Revised): {ex.Message}");
            return null;
        }

        return text.ToString();
    }
    private static string GetListPrefix(Paragraph p, IEnumerable<NumberingInstance>? numberingInstances, IEnumerable<AbstractNum>? abstractNums
        , Dictionary<string, int> keyValuePairs)
    {
        var pp = p.Elements<ParagraphProperties>().FirstOrDefault();
        if (pp != null)
        {
            var np = pp.Elements<NumberingProperties>().FirstOrDefault();
            if (np != null && np.NumberingId != null) // Now we get NumberingId from ParagraphProperties
            {
                string? numberingId = np.NumberingId.Val;

                var lo = np.Elements<LevelOverride>().FirstOrDefault();
                int levelIndex = lo?.LevelIndex?.Value ?? 0; // Default to level 0

                var numberingInstance = numberingInstances?.FirstOrDefault(ni => ni.NumberID == numberingId);
                var abstractNum = abstractNums?.FirstOrDefault(nd => nd.AbstractNumberId?.Value == numberingInstance?.AbstractNumId?.Val?.Value); // Corrected property name

                if (abstractNum != null)
                {
                    Level? nl = abstractNum.Elements<Level>().ElementAtOrDefault(levelIndex);
                    if (nl != null)
                    {
                        string? format = nl.NumberingFormat?.Val?.ToString();
                        string? suffix = nl.LevelText?.Val ?? "";

                        // Logic to determine the current list item number (simplified)
                        // This part might need more sophisticated handling for complex lists
                        int levelValue = 1; // Default to 1, you might need to track this
                        if (keyValuePairs.ContainsKey(numberingId!) == false) keyValuePairs.Add(numberingId!, 1);
                        else levelValue = ++keyValuePairs[numberingId!];

                        switch (format)
                        {
                            case "bullet":
                                return "• ";
                            case "decimal":
                                return $"{levelValue}.\t";
                            case "lowerLetter":
                                return string.Format(suffix!.Replace("%1", "{0}") + "\t", (char)('a' + (levelValue - 1)));
                            case "upperLetter":
                                return $"{(char)('A' + (levelValue - 1))}.\t";
                            case "lowerRoman":
                                string[] romanLower = { "", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x" };
                                return (levelValue <= 10) ? $"{romanLower[levelValue]}.\t" : $"{levelValue}. ";
                            case "upperRoman":
                                string[] romanUpper = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
                                return (levelValue <= 10) ? $"{romanUpper[levelValue]}.\t" : $"{levelValue}. ";
                            default:
                                return "";
                        }
                    }
                }
            }
        }
        return "";
    }
}