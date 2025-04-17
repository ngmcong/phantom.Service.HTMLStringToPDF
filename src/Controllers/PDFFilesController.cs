using System.Security.Cryptography;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;
using phantom.Core.Restful;

namespace phantom.Service.HTMLStringToPDF
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PDFFilesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public PDFFilesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertFromString([FromBody] string htmlString)
        {
            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4Plus,
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = htmlString,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };
            byte[] fileBytes = converter.Convert(doc);

            await Task.CompletedTask;

            string fileName = "my_document.pdf";
            string contentType = "application/pdf";
            return File(fileBytes, contentType, fileName); ;
        }

        private async Task StringToFile(string filePath, string fileContent)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter stream = new StreamWriter(fileStream))
            {
                stream.Write(fileContent);
                stream.Close();
                await stream.DisposeAsync();
                fileStream.Close();
                await fileStream.DisposeAsync();
            }
        }
        private async Task<string> FileToString(string filePath)
        {
            string fileContent;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader stream = new StreamReader(fileStream))
            {
                fileContent = await stream.ReadToEndAsync();
                stream.Close();
                stream.Dispose();
                fileStream.Close();
                await fileStream.DisposeAsync();
            }
            return fileContent;
        }
        private string TextOnly(string text) => string.Join("", text.Split(new char[] { '\r', '\n', ' ', '\t', '•', (char)160, (char)61623 }, StringSplitOptions.RemoveEmptyEntries));

        class GeminyPart
        {
            public List<GeminyPartText>? parts { get; set; }
        }
        class GeminyPartText
        {
            public string? text { get; set; }
        }
        class GeminyReturnBody
        {
            public List<Candidate>? candidates { get; set; }
            public GeminyReturnMeta? usageMetadata { get; set; }
            public string? modelVersion { get; set; }
        }
        class Candidate
        {
            public GeminyPart? content { get; set; }
            public string? finishReason { get; set; }
        }
        class GeminyReturnMeta
        {
            public int? promptTokenCount { get; set; }
            public int? candidatesTokenCount { get; set; }
            public int? totalTokenCount { get; set; }
            public List<PromptTokensDetail>? promptTokensDetails { get; set; }
            public List<PromptTokensDetail>? candidatesTokensDetails { get; set; }
        }
        class PromptTokensDetail
        {
            public string? modality { get; set; }
            public int? tokenCount { get; set; }
        }
        [HttpGet]
        public async Task<object> SignDOCXFile()
        {
            var filePath = "E:\\Downloads\\Permanently Keep Current OS Version using Group Policy.docx";
            var originalData = DocxToTextConverterWithListsRevised.ConvertDocxToText(filePath)!;
            RSAParameters publicKey;
            RSAParameters privateKey;
            if (System.IO.File.Exists("PublicKey.xml") == false)
            {
                // Generate a new RSA key pair
                using (RSA rsa = RSA.Create())
                {
                    // Generate a new RSA key pair with a specified key size (e.g., 2048 bits)
                    rsa.KeySize = 2048;

                    // Get the public and private keys
                    publicKey = rsa.ExportParameters(false);
                    privateKey = rsa.ExportParameters(true);

                    await StringToFile("PublicKey.xml", publicKey.ToXmlString(false));
                    await StringToFile("PrivateKey.xml", privateKey.ToXmlString(true));
                }
            }
            else
            {
                publicKey = RsaKeyConverter.FromXmlString(await FileToString("PublicKey.xml"));
                privateKey = RsaKeyConverter.FromXmlString(await FileToString("PrivateKey.xml"));
            }

            var germinyKey = _configuration.GetSection("Keys")["GerminyKey"];

            string questionString = "Does you have any advices to make this paragraph better: " + originalData;
            RestfulHelper restfulHelper = new RestfulHelper();
            restfulHelper.BaseUrl = "https://generativelanguage.googleapis.com";
            var germinyBody = new
            {
                contents = new List<GeminyPart>
                {
                    new GeminyPart
                    {
                        parts = new List<GeminyPartText>
                        {
                            new GeminyPartText
                            {
                                text = questionString
                            }
                        }
                    }
                }
            };
            var aiAdvicesRetVal = await restfulHelper.PostAsync<GeminyReturnBody>($"v1beta/models/gemini-1.5-flash:generateContent?key={germinyKey}", germinyBody);

            originalData = TextOnly(originalData);
            var signatureString = DigitalSignature.SignData(TextOnly(originalData), privateKey);

            var signedFilePath = filePath.Replace(".docx", " - Copy.docx");
            if (System.IO.File.Exists(signedFilePath)) System.IO.File.Delete(signedFilePath);
            System.IO.File.Copy(filePath, signedFilePath);
            //DocxParagraphExpander.AddNewParagraph(signedFilePath, $"Signature: {signatureString}");

            // You can customize font, size, and colors
            if (System.IO.File.Exists("text_image_c3_again.png")) System.IO.File.Delete("text_image_c3_again.png");
            TextToImageDrawer.DrawWrappedText($"Signature: {signatureString}", "text_image_c3_again.png", fontSize: 8, rectangleWidth: 500);
            DocxImageAppender.AppendImage(signedFilePath, "text_image_c3_again.png");

            // Ideally, get this path from configuration
            string libreOfficePath = "E:\\Downloads\\LibreOfficePortable\\App\\libreoffice\\program\\soffice.exe"; // Example path
            DocxToPdfConverterLibreOffice converter = new DocxToPdfConverterLibreOffice(libreOfficePath);
            string docxFile = signedFilePath;
            string pdfFile = signedFilePath.Replace(".docx", ".pdf");
            if (System.IO.File.Exists(pdfFile)) System.IO.File.Delete(pdfFile);
            if (System.IO.File.Exists(pdfFile) == false && converter.ConvertDocxToPdf(docxFile, pdfFile))
            {
                Console.WriteLine($"Successfully converted '{docxFile}' to '{pdfFile}'.");
            }
            else
            {
                Console.WriteLine($"Conversion failed.");
            }

            var extractedText = PdfTextExtractor.ExtractText(pdfFile);
            extractedText = TextOnly(extractedText!);

            if (!string.IsNullOrEmpty(extractedText))
            {
                Console.WriteLine("Extracted Text:\n" + extractedText + $" [{DigitalSignature.VerifyData(extractedText, signatureString, publicKey)}]");
            }
            else
            {
                Console.WriteLine("No text extracted or an error occurred.");
            }

            return new
            {
                Content = originalData,
                Signature = signatureString,
            };
        }

        [HttpGet]
        public async Task<bool> VerifyData()
        {
            if (System.IO.File.Exists("PublicKey.xml") == false)
            {
                return false;
            }
            var publicKey = RsaKeyConverter.FromXmlString(await FileToString("PublicKey.xml"));
            string signatureString = "B2twvKhDtT4Rz4TURn4DIxcDuWa1ZsvWwedERVMNZW+R1jm2/wbLh9wEtaAm02VdnNW6Gp8EEWVPyoeVfkWIoyqN002wDOzLHtvqTDXvesChlAZoJtbCEArwGHep1nJgMB7zqAfVRVW73dnnyGwcB/cDsZLE5RhrgIqcIFwuQ4QDIdF/fXTMSvSExPY9DgxFRjVLcdQW85K9muuQ1L5rnYXm/huppaCEowy9vqrfcyLrYmSTcOPbGEcWCJmgxJi+unVDbVL1NDBcYgmpMaIc5tF/EXgDt+4AKebzJgM3prVznXvv8p9iExinPPKfjQaP/1KXFD6rQB0Sw9Dyt3tFzw==";
            var filePath = "E:\\Downloads\\Permanently Keep Current OS Version using Group Policy - Copy.pdf";
            var originalData = PdfTextExtractor.ExtractText(filePath)!;
            //originalData = originalData.Substring(0, originalData.IndexOf(originalData.Split('\n').First(x => x.StartsWith("Signature: "))));
            originalData = TextOnly(originalData);
            return DigitalSignature.VerifyData(originalData, signatureString, publicKey);
        }
    }
}