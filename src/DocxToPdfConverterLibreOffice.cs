using System.Diagnostics;
// Potentially using a NuGet package for cleaner process handling (example name)
// using SomeProcessHelperLibrary;

public class DocxToPdfConverterLibreOffice
{
    private readonly string _libreOfficePath;

    public DocxToPdfConverterLibreOffice(string libreOfficePath)
    {
        _libreOfficePath = libreOfficePath;
    }

    public bool ConvertDocxToPdf(string docxFilePath, string pdfFilePath)
    {
        string command = $"--headless --convert-to pdf \"{docxFilePath}\" --outdir \"{Path.GetDirectoryName(pdfFilePath)}\"";

        ProcessStartInfo psi = new ProcessStartInfo(_libreOfficePath, command);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        try
        {
            using (Process? process = Process.Start(psi))
            {
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0 && File.Exists(pdfFilePath))
                    {
                        return true;
                    }
                    else
                    {
                        string error = process.StandardError.ReadToEnd();
                        Console.WriteLine($"LibreOffice conversion error: {error}");
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting LibreOffice: {ex.Message}");
            return false;
        }
        return false;
    }
}