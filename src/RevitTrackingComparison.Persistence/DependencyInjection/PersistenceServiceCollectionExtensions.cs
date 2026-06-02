using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Persistence.Config;

namespace RevitTrackingComparison.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddLiteDbPersistence(
        this IServiceCollection services,
        Action<LiteDbOptions>? configure = null)
    {
        var options = new LiteDbOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ILiteDbConnectionFactory, LiteDbConnectionFactory>();
        services.AddSingleton<ISnapshotStore, LiteDbSnapshotStore>();
        services.AddSingleton<ICaptureSettingsStore, JsonCaptureSettingsStore>();
        return services;
    }
}