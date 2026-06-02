using System.IO;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Persistence;

namespace RevitTrackingComparison.Persistence.Tests;

[TestFixture]
public sealed class LiteDbSnapshotStoreTests
{
    private string _root = null!;
    private LiteDbOptions _options = null!;
    private LiteDbSnapshotStore _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _root = Path.Combine(Path.GetTempPath(), "RTC_Tests_" + Guid.NewGuid().ToString("N"));
        _options = new LiteDbOptions { RootFolder = _root };
        _sut = new LiteDbSnapshotStore(
            new LiteDbConnectionFactory(), _options, new TestPluginLogger<LiteDbSnapshotStore>());
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // Best effort: a leftover temp folder must not fail the test run.
        }
    }

    [Test]
    public void HasSnapshots_returns_false_for_unknown_project()
    {
        Assert.That(_sut.HasSnapshots("unknown.rvt"), Is.False);
    }

    [Test]
    public async Task ListAsync_returns_empty_for_unknown_project()
    {
        var result = await _sut.ListAsync("unknown.rvt");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SaveAsync_then_LoadAsync_round_trips_the_snapshot()
    {
        var snapshot = TestSnapshots.Create(
            TestSnapshots.Element(
                uniqueId: "uid-1",
                elementId: 42,
                category: "Walls",
                name: "Basic Wall",
                parameters: new Dictionary<string, string> { ["Height"] = "3000", ["Comments"] = "ok" }));

        var info = await _sut.SaveAsync(snapshot.DocumentKey, snapshot);
        var loaded = await _sut.LoadAsync(info);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Id, Is.EqualTo(snapshot.Id));
        Assert.That(loaded.DocumentKey, Is.EqualTo(snapshot.DocumentKey));
        Assert.That(loaded.Title, Is.EqualTo(snapshot.Title));
        // LiteDB stores DateTime as UTC; compare the instant, not the Kind.
        Assert.That(loaded.CapturedAt.ToUniversalTime(), Is.EqualTo(snapshot.CapturedAt.ToUniversalTime()));
        Assert.That(loaded.Elements, Has.Count.EqualTo(1));

        var element = loaded.Elements[0];
        Assert.That(element.UniqueId, Is.EqualTo("uid-1"));
        Assert.That(element.ElementId, Is.EqualTo(42));
        Assert.That(element.Category, Is.EqualTo("Walls"));
        Assert.That(element.Name, Is.EqualTo("Basic Wall"));
        Assert.That(element.Parameters, Is.EquivalentTo(
            new Dictionary<string, string> { ["Height"] = "3000", ["Comments"] = "ok" }));
    }

    [Test]
    public async Task SaveAsync_returns_info_with_project_db_filename_and_capturedAt()
    {
        var snapshot = TestSnapshots.Create(TestSnapshots.Element());

        var info = await _sut.SaveAsync(snapshot.DocumentKey, snapshot);

        Assert.That(info.Project, Is.EqualTo(snapshot.DocumentKey));
        Assert.That(info.FileName, Does.EndWith(".db"));
        Assert.That(info.CapturedAt, Is.EqualTo(snapshot.CapturedAt));
    }

    [Test]
    public async Task HasSnapshots_returns_true_after_save()
    {
        var snapshot = TestSnapshots.Create(TestSnapshots.Element());

        await _sut.SaveAsync(snapshot.DocumentKey, snapshot);

        Assert.That(_sut.HasSnapshots(snapshot.DocumentKey), Is.True);
    }

    [Test]
    public async Task ListAsync_returns_snapshots_newest_first()
    {
        var older = TestSnapshots.Create(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TestSnapshots.Element());
        var newer = TestSnapshots.Create(new DateTime(2025, 2, 1, 12, 0, 0, DateTimeKind.Utc),
            TestSnapshots.Element());

        await _sut.SaveAsync("p.rvt", older);
        await _sut.SaveAsync("p.rvt", newer);

        var result = await _sut.ListAsync("p.rvt");

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].CapturedAt, Is.EqualTo(newer.CapturedAt));
        Assert.That(result[1].CapturedAt, Is.EqualTo(older.CapturedAt));
    }

    [Test]
    public async Task SaveAsync_twice_with_same_timestamp_creates_two_files()
    {
        var first = TestSnapshots.Create(TestSnapshots.Element());
        var second = TestSnapshots.Create(TestSnapshots.Element());

        var firstInfo = await _sut.SaveAsync("p.rvt", first);
        var secondInfo = await _sut.SaveAsync("p.rvt", second);

        Assert.That(secondInfo.FileName, Is.Not.EqualTo(firstInfo.FileName));
        Assert.That(await _sut.ListAsync("p.rvt"), Has.Count.EqualTo(2));
    }

    [Test]
    public async Task LoadAsync_returns_null_for_missing_file()
    {
        var info = new SnapshotInfo
        {
            Project = "p.rvt",
            FileName = "p.rvt_2025.01.01_12.00.00.db",
            CapturedAt = TestSnapshots.DefaultCapturedAt
        };

        var loaded = await _sut.LoadAsync(info);

        Assert.That(loaded, Is.Null);
    }

    [Test]
    public async Task SaveAsync_sanitizes_invalid_characters_in_project_name()
    {
        const string project = "Wing A:Level/1.rvt";
        var snapshot = TestSnapshots.Create(
            TestSnapshots.DefaultCapturedAt, project, TestSnapshots.Element());

        var info = await _sut.SaveAsync(project, snapshot);
        var loaded = await _sut.LoadAsync(info);

        Assert.That(_sut.HasSnapshots(project), Is.True);
        Assert.That(loaded, Is.Not.Null);
        Assert.That(info.Project, Is.EqualTo(project)); // original (unsanitized) key is preserved on the handle
    }

    [Test]
    public async Task ListAsync_parses_legacy_timestamp_format()
    {
        const string project = "legacy.rvt";
        var folder = Path.Combine(_options.SnapshotsFolder, project);
        Directory.CreateDirectory(folder);
        // Legacy name: yyyyMMdd_HHmmss. List only reads the file name, so the contents are irrelevant.
        File.WriteAllBytes(Path.Combine(folder, $"{project}_20250101_120000.db"), Array.Empty<byte>());

        var result = await _sut.ListAsync(project);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].CapturedAt, Is.EqualTo(new DateTime(2025, 1, 1, 12, 0, 0)));
    }
}
