using LiteDB;

namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbConnectionFactory : ILiteDbConnectionFactory
{
    private readonly LiteDbOptions _options;

    public LiteDbConnectionFactory(LiteDbOptions options)
    {
        _options = options;
    }

    public LiteDatabase Open(string documentKey)
    {
        if (string.IsNullOrWhiteSpace(documentKey))
            throw new ArgumentException("documentKey cannot be empty.", nameof(documentKey));

        Directory.CreateDirectory(_options.DatabaseFolder);
        var fileName = Sanitize(documentKey) + ".db";
        var path = Path.Combine(_options.DatabaseFolder, fileName);
        return new LiteDatabase($"Filename={path};Connection=shared");
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}