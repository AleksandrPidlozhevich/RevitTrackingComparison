using System.Text;
using RevitTrackingComparison.Core.Services;

namespace RevitTrackingComparison.Core.Tests.Services;

[TestFixture]
public sealed class SnapshotCsvExporterTests
{
    [Test]
    public void Export_writes_header_and_element_rows()
    {
        var snapshot = SnapshotTestData.CreateSnapshot(
            SnapshotTestData.Element(parameters: new Dictionary<string, string> { ["Base Offset"] = "0" }));

        using var writer = new StringWriter();
        SnapshotCsvExporter.Export(snapshot, writer);

        var lines = writer.ToString().TrimEnd().Split(Environment.NewLine);
        Assert.That(lines, Has.Length.EqualTo(2));
        Assert.That(lines[0], Is.EqualTo("Category,ElementId,UniqueId,Name,Base Offset"));
        Assert.That(lines[1], Is.EqualTo(
            $"Structural Foundations,{SnapshotTestData.DefaultElementId},{SnapshotTestData.DefaultUniqueId},\"Foundation - 24\"\" Concrete\",0"));
    }

    [Test]
    public void Export_empty_snapshot_writes_header_only()
    {
        var snapshot = SnapshotTestData.CreateSnapshot();

        using var writer = new StringWriter();
        SnapshotCsvExporter.Export(snapshot, writer);

        var lines = writer.ToString().TrimEnd().Split(Environment.NewLine);
        Assert.That(lines, Has.Length.EqualTo(1));
        Assert.That(lines[0], Is.EqualTo("Category,ElementId,UniqueId,Name"));
    }

    [Test]
    public void Export_quotes_values_containing_commas()
    {
        var snapshot = SnapshotTestData.CreateSnapshot(
            SnapshotTestData.Element(parameters: new Dictionary<string, string> { ["Comments"] = "A, B" }));

        using var writer = new StringWriter();
        SnapshotCsvExporter.Export(snapshot, writer);

        var csv = writer.ToString();
        Assert.That(csv, Does.Contain("\"A, B\""));
    }

    [Test]
    public void ExportToFile_writes_utf8_with_bom()
    {
        var snapshot = SnapshotTestData.CreateSnapshot(SnapshotTestData.Element());

        var filePath = Path.Combine(Path.GetTempPath(), $"snapshot-export-{Guid.NewGuid():N}.csv");
        try
        {
            SnapshotCsvExporter.ExportToFile(snapshot, filePath);

            var bytes = File.ReadAllBytes(filePath);
            Assert.That(bytes, Has.Length.GreaterThan(3));
            Assert.That(bytes[0], Is.EqualTo(0xEF));
            Assert.That(bytes[1], Is.EqualTo(0xBB));
            Assert.That(bytes[2], Is.EqualTo(0xBF));

            var content = Encoding.UTF8.GetString(bytes);
            Assert.That(content, Does.Contain(SnapshotTestData.DefaultUniqueId));
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Test]
    public void Export_throws_when_snapshot_is_null()
    {
        using var writer = new StringWriter();

        Assert.Throws<ArgumentNullException>(() => SnapshotCsvExporter.Export(null!, writer));
    }
}