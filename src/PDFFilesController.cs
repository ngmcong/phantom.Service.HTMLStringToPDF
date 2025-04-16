using System.Net.Mime;
using DinkToPdf;
using Microsoft.AspNetCore.Mvc;

namespace phantom.Service.HTMLStringToPDF
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PDFFilesController : ControllerBase
    {
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
    }
}