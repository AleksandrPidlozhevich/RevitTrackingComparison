using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitTrackingComparison.Revit.Infrastructure;

// Loads the ribbon icon from the PNG embedded in this assembly (Revit doesn't resolve WPF pack URIs
// reliably, and embedding avoids depending on the loose file at deploy time).
// The icon must be normalised to 96 DPI: the source PNG is ~6 DPI, so WPF reports its DIP size as
// 32 * (96 / 6) ≈ 512, which overflows the button slot (sized by DIP, not pixels) and renders blank.
internal static class RibbonIconLoader
{
    private const string EmbeddedResourceName = "RevitTrackingComparison.Revit.Resources.ab-testing.png";
    private const double TargetDpi = 96d;

    public static void ApplyTo(PushButton button)
    {
        var large = Load(32);
        var small = Load(16);
        if (large is null)
            return;

        button.LargeImage = large;
        button.Image = small ?? large;
    }

    private static ImageSource? Load(int pixelSize)
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedResourceName);
            if (stream is null)
            {
                PluginLog.Warn($"Embedded ribbon icon '{EmbeddedResourceName}' was not found.");
                return null;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.DecodePixelWidth = pixelSize;
            image.DecodePixelHeight = pixelSize;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return Normalize(image);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to load embedded ribbon icon.");
            return null;
        }
    }

    // Rebuilds the image at 96 DPI so its DIP size equals its pixel size (see class note).
    private static BitmapSource Normalize(BitmapImage image)
    {
        var bgra = image.Format == PixelFormats.Bgra32
            ? (BitmapSource)image
            : new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);

        var stride = bgra.PixelWidth * 4;
        var pixels = new byte[stride * bgra.PixelHeight];
        bgra.CopyPixels(pixels, stride, 0);

        var normalized = BitmapSource.Create(
            bgra.PixelWidth, bgra.PixelHeight,
            TargetDpi, TargetDpi,
            PixelFormats.Bgra32, null, pixels, stride);
        normalized.Freeze();
        return normalized;
    }
}