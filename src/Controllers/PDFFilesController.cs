﻿using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
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

        [HttpGet]
        public async Task<object> SignDOCXFile()
        {
            var filePath = "E:\\Downloads\\Permanently Keep Current OS Version using Group Policy - Copy.docx";
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
            var signatureString = DigitalSignature.SignData(originalData, privateKey);
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
            string signatureString = "K9I8GLcIZ8GaX6VIAa6r4Siu8L+xl/L1z/8gqPhcHbz+6OxU4DclB5r3Nympp5tc8/3572QhwYnrW1lWFavVdE55aCgoAnTEmkKaYVv6kGTH6vNjLxs7Fe5sKniAm9UuVnxMoU88yOROZKiAcFri1D61cnk169/kkA5h2BqsAVdG8CL4bh6dv8eizVZvUP0bLjjUFb95epOIA7E5OjHfRrRZPMv13/6LM9oAu14oFi/cBi6OnRffNIrKi3M/awkggBbgolSjhgxXL0dw6D3fdv1Jkk0W3dg9mf4RFAaxYhyFCHvM4DISXQVUf9XPBvdiXh7oKpxT/wyRl/FSBdIevQ==";
            var filePath = "E:\\Downloads\\Permanently Keep Current OS Version using Group Policy - Copy.docx";
            var originalData = DocxToTextConverterWithListsRevised.ConvertDocxToText(filePath)!;
            return DigitalSignature.VerifyData(originalData, signatureString, publicKey);
        }
    }
}