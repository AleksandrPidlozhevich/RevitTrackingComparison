using System.IO;
using RevitTrackingComparison.Persistence;

namespace RevitTrackingComparison.Persistence.Tests;

[TestFixture]
public sealed class LiteDbConnectionFactoryTests
{
    private readonly LiteDbConnectionFactory _sut = new();

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Open_throws_on_empty_path(string? path)
    {
        Assert.Throws<ArgumentException>(() => _sut.Open(path!));
    }

    [Test]
    public void Open_creates_the_database_file_and_missing_directory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "RTC_Tests_" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(dir, "nested", "test.db");

        try
        {
            using (var db = _sut.Open(path))
            {
                db.GetCollection("c").Insert(new LiteDB.BsonDocument { ["x"] = 1 });
            }

            Assert.That(File.Exists(path), Is.True);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }
}