using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

public class DocxToTextConverterWithListsRevised
{
    internal class NumbericKeyValue
    {
        public int? LevelIndex { get; set; }
        public string? NumberId { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is NumbericKeyValue p) return NumberId == p?.NumberId && LevelIndex == p?.LevelIndex;
            return false;
        }
        public override int GetHashCode()
        {
            return $"{LevelIndex}.{NumberId}".GetHashCode();
        }
    }
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
                    Dictionary<NumbericKeyValue, int> keyValuePairs = new Dictionary<NumbericKeyValue, int>();
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
        , Dictionary<NumbericKeyValue, int> keyValuePairs)
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
                if (levelIndex == 0) levelIndex = np.Elements<NumberingLevelReference>().FirstOrDefault()?.Val?.Value ?? 0;

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
                        var numbericKeyValue = new NumbericKeyValue
                        {
                            NumberId = numberingId,
                            LevelIndex = levelIndex,
                        };
                        if (keyValuePairs.ContainsKey(numbericKeyValue) == false) keyValuePairs.Add(numbericKeyValue, 1);
                        else levelValue = ++keyValuePairs[numbericKeyValue];
                        var formatString = suffix!.Replace("%1", "{0}") + "\t";
                        switch (format)
                        {
                            case "bullet":
                                return "• ";
                            case "decimal":
                                return $"{levelValue}.\t";
                            case "lowerLetter":
                                return string.Format(formatString, (char)('a' + (levelValue - 1)));
                            case "upperLetter":
                                return string.Format(formatString, (char)('A' + (levelValue - 1)));
                            case "lowerRoman":
                                string[] romanLower = { "", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x" };
                                return (levelValue <= 10) ? string.Format(formatString, romanLower[levelValue]) : $"{levelValue}. ";
                            case "upperRoman":
                                string[] romanUpper = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };
                                return (levelValue <= 10) ? string.Format(formatString, romanUpper[levelValue]) : $"{levelValue}. ";
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