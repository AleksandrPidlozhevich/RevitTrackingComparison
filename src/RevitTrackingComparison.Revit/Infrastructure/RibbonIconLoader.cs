using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitTrackingComparison.Revit.Infrastructure;

/// <summary>
/// Loads the ribbon icon for Revit push buttons from the single PNG embedded in this assembly
/// (<c>Resources\ab-testing.png</c>); Revit does not resolve WPF pack URIs reliably, and embedding
/// avoids depending on the loose file being copied alongside the add-in at deploy time.
/// </summary>
/// <remarks>
/// The ribbon sizes icons by their device-independent (DIP) dimensions and expects 32x32 / 16x16.
/// Our PNG was authored at ~6 DPI, so WPF reports its DIP size as 32 * (96 / 6) ≈ 512, which overflows
/// the button slot and renders blank. Every decoded image is therefore normalised to 96 DPI so the
/// DIP size equals the pixel size. The 16x16 small image is decoded from the same 32x32 source.
/// </remarks>
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

    /// <summary>
    /// Rebuilds the decoded image at 96 DPI so its DIP size equals its pixel size; the source PNG's
    /// low embedded resolution would otherwise make the icon overflow the ribbon slot and disappear.
    /// </summary>
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
