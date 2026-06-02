using RevitTrackingComparison.Core.Domain;

namespace RevitTrackingComparison.Core.Abstractions;

public interface IRevitSnapshotProvider
{
    DocumentSnapshot? CaptureActiveDocument();
}