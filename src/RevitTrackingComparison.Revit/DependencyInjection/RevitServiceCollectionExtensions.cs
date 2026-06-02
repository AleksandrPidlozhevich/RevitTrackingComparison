using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Revit.Commands;
using RevitTrackingComparison.Revit.Editing;
using RevitTrackingComparison.Revit.Events;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.Revit.Metadata;
using RevitTrackingComparison.Revit.Snapshots;

namespace RevitTrackingComparison.Revit.DependencyInjection;

public static class RevitServiceCollectionExtensions
{
    public static IServiceCollection AddRevitAdapters(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPluginLogger<>), typeof(NLogPluginLogger<>));
        services.AddSingleton<IPluginLoggerFactory, NLogPluginLoggerFactory>();

        services.AddSingleton<RevitSnapshotProvider>();
        services.AddSingleton<ISnapshotCommands, RevitSnapshotCommands>();
        services.AddSingleton<IModelEditor, RevitModelEditor>();
        services.AddSingleton<IModelMetadataProvider, RevitModelMetadataProvider>();

        services.AddSingleton<DocumentEventRouter>();
        services.AddSingleton<SnapshotHubLauncher>();
        return services;
    }
}