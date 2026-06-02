using System.Globalization;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Domain;
using RevitTrackingComparison.Persistence.Entities;

namespace RevitTrackingComparison.Persistence;

public sealed class LiteDbSnapshotStore : ISnapshotStore
{
    private const string Snapshots = "snapshots";
    private const string TimestampFormat = "yyyy.MM.dd_HH.mm.ss";
    private const int TimestampLength = 19;
    private const string LegacyTimestampFormat = "yyyyMMdd_HHmmss";
    private const int LegacyTimestampLength = 15;

    private readonly ILiteDbConnectionFactory _connectionFactory;
    private readonly LiteDbOptions _options;

    public LiteDbSnapshotStore(ILiteDbConnectionFactory connectionFactory, LiteDbOptions options)
    {
        _connectionFactory = connectionFactory;
        _options = options;
    }

    public bool HasSnapshots(string project)
    {
        var folder = ProjectFolder(Sanitize(project));
        return Directory.Exists(folder) && Directory.EnumerateFiles(folder, "*.db").Any();
    }

    // LiteDB has no async API; offload the blocking file I/O so the calling (UI/API) thread is free.
    public Task<SnapshotInfo> SaveAsync(string project, DocumentSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Save(project, snapshot), cancellationToken);
    }

    public Task<IReadOnlyList<SnapshotInfo>> ListAsync(string project, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => List(project), cancellationToken);
    }

    public Task<DocumentSnapshot?> LoadAsync(SnapshotInfo info, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Load(info), cancellationToken);
    }

    private SnapshotInfo Save(string project, DocumentSnapshot snapshot)
    {
        var safeProject = Sanitize(project);
        var folder = ProjectFolder(safeProject);
        Directory.CreateDirectory(folder);

        var fileName = UniqueFileName(folder, safeProject, snapshot.CapturedAt);
        var path = Path.Combine(folder, fileName);

        using (var db = _connectionFactory.Open(path))
        {
            db.GetCollection<SnapshotEntity>(Snapshots).Insert(snapshot.ToEntity());
        }

        return new SnapshotInfo { Project = project, Id = new SnapshotId(fileName), CapturedAt = snapshot.CapturedAt };
    }

    private IReadOnlyList<SnapshotInfo> List(string project)
    {
        var folder = ProjectFolder(Sanitize(project));
        if (!Directory.Exists(folder))
            return Array.Empty<SnapshotInfo>();

        return Directory.EnumerateFiles(folder, "*.db")
            .Select(path => new SnapshotInfo
            {
                Project = project,
                Id = new SnapshotId(Path.GetFileName(path)),
                CapturedAt = ParseTimestamp(Path.GetFileNameWithoutExtension(path)) ?? File.GetLastWriteTime(path)
            })
            .OrderByDescending(info => info.CapturedAt)
            .ToList();
    }

    private DocumentSnapshot? Load(SnapshotInfo info)
    {
        var path = Path.Combine(ProjectFolder(Sanitize(info.Project)), info.Id.Value);
        if (!File.Exists(path))
            return null;

        using var db = _connectionFactory.Open(path);
        var entity = db.GetCollection<SnapshotEntity>(Snapshots).Query().FirstOrDefault();
        return entity?.ToDomain();
    }

    private string ProjectFolder(string safeProject)
    {
        return Path.Combine(_options.SnapshotsFolder, safeProject);
    }

    private static string UniqueFileName(string folder, string safeProject, DateTime capturedAt)
    {
        var baseName = $"{safeProject}_{capturedAt.ToString(TimestampFormat, CultureInfo.InvariantCulture)}";
        var fileName = baseName + ".db";
        var index = 2;
        while (File.Exists(Path.Combine(folder, fileName)))
            fileName = $"{baseName}_{index++}.db";
        return fileName;
    }

    private static DateTime? ParseTimestamp(string fileNameWithoutExtension)
    {
        if (TryParseTrailingStamp(fileNameWithoutExtension, TimestampFormat, TimestampLength, out var parsed))
            return parsed;

        if (TryParseTrailingStamp(fileNameWithoutExtension, LegacyTimestampFormat, LegacyTimestampLength, out parsed))
            return parsed;

        return null;
    }

    private static bool TryParseTrailingStamp(
        string fileNameWithoutExtension,
        string format,
        int stampLength,
        out DateTime parsed)
    {
        parsed = default;
        if (fileNameWithoutExtension.Length < stampLength)
            return false;

        var stamp = fileNameWithoutExtension[^stampLength..];
        return DateTime.TryParseExact(
            stamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}