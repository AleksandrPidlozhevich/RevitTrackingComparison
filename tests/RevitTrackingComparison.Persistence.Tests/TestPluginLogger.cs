using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Persistence.Tests;

// No-op logger so the store under test can be constructed without an NLog dependency.
internal sealed class TestPluginLogger<T> : IPluginLogger<T>
{
    public void Info(string message) { }

    public void Warn(string message) { }

    public void Error(Exception? exception, string message) { }
}
