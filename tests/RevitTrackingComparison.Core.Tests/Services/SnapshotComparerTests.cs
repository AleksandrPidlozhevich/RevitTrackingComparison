using RevitTrackingComparison.Core.Domain.Diff;
using RevitTrackingComparison.Core.Services;

namespace RevitTrackingComparison.Core.Tests.Services;

[TestFixture]
public sealed class SnapshotComparerTests
{
    private SnapshotComparer _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new SnapshotComparer();

    [Test]
    public void Compare_identical_snapshots_returns_no_changes()
    {
        var element = SnapshotTestData.Element(parameters: new Dictionary<string, string> { ["Height"] = "3000" });
        var from = SnapshotTestData.CreateSnapshot(element);
        var to = SnapshotTestData.CreateSnapshot(element);

        var diff = _sut.Compare(from, to);

        Assert.That(diff.HasChanges, Is.False);
        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Modified, Is.Empty);
    }

    [Test]
    public void Compare_detects_added_elements()
    {
        var from = SnapshotTestData.CreateSnapshot();
        var to = SnapshotTestData.CreateSnapshot(
            SnapshotTestData.Element(SnapshotTestData.AddedElementUniqueId, name: SnapshotTestData.AddedElementName));

        var diff = _sut.Compare(from, to);

        Assert.That(diff.Added, Has.Count.EqualTo(1));
        Assert.That(diff.Added[0].ChangeType, Is.EqualTo(ChangeType.Added));
        Assert.That(diff.Added[0].UniqueId, Is.EqualTo(SnapshotTestData.AddedElementUniqueId));
        Assert.That(diff.Added[0].After!.Name, Is.EqualTo(SnapshotTestData.AddedElementName));
        Assert.That(diff.Removed, Is.Empty);
        Assert.That(diff.Modified, Is.Empty);
    }

    [Test]
    public void Compare_detects_removed_elements()
    {
        var from = SnapshotTestData.CreateSnapshot(SnapshotTestData.Element());
        var to = SnapshotTestData.CreateSnapshot();

        var diff = _sut.Compare(from, to);

        Assert.That(diff.Removed, Has.Count.EqualTo(1));
        Assert.That(diff.Removed[0].ChangeType, Is.EqualTo(ChangeType.Removed));
        Assert.That(diff.Removed[0].UniqueId, Is.EqualTo(SnapshotTestData.DefaultUniqueId));
        Assert.That(diff.Added, Is.Empty);
        Assert.That(diff.Modified, Is.Empty);
    }

    [Test]
    public void Compare_detects_modified_parameters()
    {
        var from = SnapshotTestData.CreateSnapshot(
            SnapshotTestData.Element(parameters: new Dictionary<string, string>
            {
                ["Height"] = "3000",
                ["Width"] = "200"
            }));
        var to = SnapshotTestData.CreateSnapshot(
            SnapshotTestData.Element(parameters: new Dictionary<string, string>
            {
                ["Height"] = "3200",
                ["Width"] = "200"
            }));

        var diff = _sut.Compare(from, to);

        Assert.That(diff.Modified, Has.Count.EqualTo(1));
        Assert.That(diff.Modified[0].ChangeType, Is.EqualTo(ChangeType.Modified));
        Assert.That(diff.Modified[0].ChangedParameters, Has.Count.EqualTo(1));
        Assert.That(diff.Modified[0].ChangedParameters[0].Name, Is.EqualTo("Height"));
        Assert.That(diff.Modified[0].ChangedParameters[0].OldValue, Is.EqualTo("3000"));
        Assert.That(diff.Modified[0].ChangedParameters[0].NewValue, Is.EqualTo("3200"));
    }

    [Test]
    public void Compare_unchanged_parameters_does_not_mark_element_modified()
    {
        var parameters = new Dictionary<string, string> { ["Height"] = "3000" };
        var from = SnapshotTestData.CreateSnapshot(SnapshotTestData.Element(parameters: parameters));
        var to = SnapshotTestData.CreateSnapshot(SnapshotTestData.Element(parameters: new Dictionary<string, string>(parameters)));

        var diff = _sut.Compare(from, to);

        Assert.That(diff.Modified, Is.Empty);
    }

    [Test]
    public void Compare_sets_snapshot_ids_on_diff()
    {
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();
        var from = SnapshotTestData.CreateSnapshot(fromId);
        var to = SnapshotTestData.CreateSnapshot(toId);

        var diff = _sut.Compare(from, to);

        Assert.That(diff.FromSnapshotId, Is.EqualTo(fromId));
        Assert.That(diff.ToSnapshotId, Is.EqualTo(toId));
    }

    [Test]
    public void Compare_throws_when_from_is_null()
    {
        var to = SnapshotTestData.CreateSnapshot();

        Assert.Throws<ArgumentNullException>(() => _sut.Compare(null!, to));
    }

    [Test]
    public void Compare_throws_when_to_is_null()
    {
        var from = SnapshotTestData.CreateSnapshot();

        Assert.Throws<ArgumentNullException>(() => _sut.Compare(from, null!));
    }
}
