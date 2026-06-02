using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.Services;

namespace RevitTrackingComparison.Core.DependencyInjection;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<ISnapshotComparer, SnapshotComparer>();
        return services;
    }
}