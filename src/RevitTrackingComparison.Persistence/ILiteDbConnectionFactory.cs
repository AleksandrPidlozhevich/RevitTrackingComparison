using LiteDB;

namespace RevitTrackingComparison.Persistence;

public interface ILiteDbConnectionFactory
{
    LiteDatabase Open(string documentKey);
}