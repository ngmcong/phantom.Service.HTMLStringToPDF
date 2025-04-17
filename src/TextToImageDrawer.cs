using System.Drawing;
using System.Drawing.Imaging;

public class TextToImageDrawer
{
    public static void DrawImageFromText(string text, string outputPath, string fontName = "Arial"
        , float fontSize = 40, Color textColor = default, Color backgroundColor = default)
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
                decimal maxWidth = 1000;
                if (width > maxWidth)
                {
                    height *= Convert.ToInt32(Math.Ceiling(width / maxWidth));
                    width = Convert.ToInt32(maxWidth);
                }

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
    public static void DrawWrappedText(string text, string outputPath, string fontName = "Arial", float fontSize = 16
        , Color textColor = default, Color backgroundColor = default, int rectangleWidth = 200)
    {
        if (textColor == default)
            textColor = Color.Black;
        if (backgroundColor == default)
            backgroundColor = Color.White;

        using (var font = new Font(fontName, fontSize))
        using (var brush = new SolidBrush(textColor))
        using (var bitmap = new System.Drawing.Bitmap(1, 1)) // Initial height of 0
        using (var graphics = Graphics.FromImage(bitmap))
        {
            // Measure the text with the specified width to get the required height
            SizeF textSize = graphics.MeasureString(text, font, rectangleWidth);

            // Create a new bitmap with the calculated height
            using (var finalBitmap = new System.Drawing.Bitmap(rectangleWidth + 20, (int)textSize.Height + 20))
            using (var finalGraphics = Graphics.FromImage(finalBitmap))
            using (var backgroundBrush = new SolidBrush(backgroundColor))
            {
                finalGraphics.FillRectangle(backgroundBrush, 0, 0, finalBitmap.Width, finalBitmap.Height);

                // Define the layout rectangle
                RectangleF layoutRect = new RectangleF(10, 10, rectangleWidth, textSize.Height);

                // Draw the string within the layout rectangle
                finalGraphics.DrawString(text, font, brush, layoutRect);

                finalBitmap.Save(outputPath, ImageFormat.Png);
                Console.WriteLine($"Wrapped text image saved to '{outputPath}'.");
            }
        }
    }
}