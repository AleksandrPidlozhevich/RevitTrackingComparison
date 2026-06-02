using LiteDB;

namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbConnectionFactory : ILiteDbConnectionFactory
{
    public LiteDatabase Open(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Path cannot be empty.", nameof(fullPath));

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        return new LiteDatabase($"Filename={fullPath};Connection=shared");
    }
}
