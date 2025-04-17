using System.Drawing;
using System.Drawing.Imaging;

public class TextToImageDrawer
{
    public static void DrawImageFromText(string text, string outputPath, string fontName = "Arial", float fontSize = 40, Color textColor = default, Color backgroundColor = default)
    {
        if (OperatingSystem.IsWindows())
        {
            #pragma warning disable CA1416
            // Set default colors if not provided
            if (textColor == default)
                textColor = Color.Black;
            if (backgroundColor == default)
                backgroundColor = Color.White;

            // Measure the text to determine the image size
            using (var tempBitmap = new Bitmap(1, 1))
            using (var tempGraphics = Graphics.FromImage(tempBitmap))
            using (var font = new Font(fontName, fontSize))
            {
                SizeF textSize = tempGraphics.MeasureString(text, font);
                int width = (int)textSize.Width + 20; // Add some padding
                int height = (int)textSize.Height + 20; // Add some padding

                using (var bitmap = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(bitmap))
                using (var textFont = new Font(fontName, fontSize))
                using (var textBrush = new SolidBrush(textColor))
                using (var backgroundBrush = new SolidBrush(backgroundColor))
                {
                    graphics.FillRectangle(backgroundBrush, 0, 0, width, height);
                    graphics.DrawString(text, textFont, textBrush, new PointF(10, 10)); // Adjust position as needed
                    bitmap.Save(outputPath, ImageFormat.Png);
                    Console.WriteLine($"Image '{outputPath}' created successfully with text: '{text}'");
                }
            }
        }
        else
        {
            throw new PlatformNotSupportedException("Image processing is only supported on Windows.");
        }
    }
}