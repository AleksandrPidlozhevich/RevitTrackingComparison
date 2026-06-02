using LiteDB;

namespace RevitTrackingComparison.Persistence;

public interface ILiteDbConnectionFactory
{
    /// <summary>Opens (creating the parent folder if needed) the LiteDB file at the given path.</summary>
    LiteDatabase Open(string fullPath);
}
