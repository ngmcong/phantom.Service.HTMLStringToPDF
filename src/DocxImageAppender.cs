using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using WP = DocumentFormat.OpenXml.Drawing.Wordprocessing;

public class DocxImageAppender
{
    /*
     * string docxFile = "your_document.docx"; // Replace with your DOCX file path
        string imageFile = "your_image.jpg";   // Replace with your image file path

        // Create a dummy DOCX if it doesn't exist
        if (!File.Exists(docxFile))
        {
            using (WordprocessingDocument.Create(docxFile, WordprocessingDocumentType.Document)) { }
        }

        if (File.Exists(imageFile))
        {
            AppendImage(docxFile, imageFile);
            Console.WriteLine($"Image appended to '{docxFile}'.");
        }
        else
        {
            Console.WriteLine($"Image file '{imageFile}' not found.");
        }
     */
    public static void AppendImage(string docxFilePath, string imageFilePath)
    {
        using (WordprocessingDocument doc = WordprocessingDocument.Open(docxFilePath, true))
        {
            MainDocumentPart? mainPart = doc.MainDocumentPart;
            if (mainPart?.Document?.Body != null)
            {
                // 1. Add an Image Part
                ImagePart imagePart = mainPart.AddImagePart(GetImagePartType(imageFilePath));

                // 2. Feed Image Data
                using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open))
                {
                    imagePart.FeedData(fileStream);
                }

                // 3. Create Image Relationship ID
                string imagePartId = mainPart.GetIdOfPart(imagePart);

                // 4. Create Drawing Elements
                ImageInfo imageInfo = GetImageInfo(imageFilePath);

                Drawing drawing = new Drawing(
                    new WP.Inline(
                        new WP.Extent() { Cx = imageInfo.Cx, Cy = imageInfo.Cy },
                        new WP.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new WP.DocProperties() { Id = (UInt32Value)1U, Name = "Picture 1" },
                        new A.Graphic(
                            new A.GraphicData(
                                new PIC.Picture(
                                    new PIC.NonVisualPictureProperties(
                                        new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = "Picture 1" },
                                        new PIC.NonVisualPictureDrawingProperties()),
                                    new PIC.BlipFill(
                                        new A.Blip() { Embed = imagePartId, CompressionState = A.BlipCompressionValues.Print },
                                        new A.Stretch(new A.FillRectangle())),
                                    new PIC.ShapeProperties(
                                        new A.Transform2D(
                                            new A.Offset() { X = 0L, Y = 0L },
                                            new A.Extents() { Cx = imageInfo.Cx, Cy = imageInfo.Cy }),
                                        new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U, EditId = "503F079F" });

                // 5. Append Drawing to a Paragraph
                var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(drawing));
                mainPart.Document.Body.AppendChild(paragraph);

                // Save changes automatically when using the 'using' statement
            }
        }
    }

    private static PartTypeInfo GetImagePartType(string imageFilePath)
    {
        string extension = System.IO.Path.GetExtension(imageFilePath).ToLower();
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return ImagePartType.Jpeg;
            case ".png":
                return ImagePartType.Png;
            case ".gif":
                return ImagePartType.Gif;
            default:
                throw new ArgumentException("Unsupported image file type: " + extension);
        }
    }

    private static ImageInfo GetImageInfo(string imageFilePath)
    {
        if (OperatingSystem.IsWindows())
        {
            #pragma warning disable CA1416
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(imageFilePath))
            {
                long emuPerInch = 914400;
                return new ImageInfo
                {
                    Cx = (long)(image.Width * emuPerInch / image.HorizontalResolution),
                    Cy = (long)(image.Height * emuPerInch / image.VerticalResolution)
                };
            }
        }
        else
        {
            throw new PlatformNotSupportedException("Image processing is only supported on Windows.");
        }
    }

    private class ImageInfo
    {
        public long Cx { get; set; }
        public long Cy { get; set; }
    }
}