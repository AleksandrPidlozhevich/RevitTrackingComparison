using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.DependencyInjection;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.Persistence.DependencyInjection;
using RevitTrackingComparison.Revit.Editing;
using RevitTrackingComparison.Revit.Events;
using RevitTrackingComparison.Revit.Metadata;
using RevitTrackingComparison.Revit.Snapshots;
using RevitTrackingComparison.UI.DependencyInjection;

namespace RevitTrackingComparison.Revit.Application;

public static class ServiceConfiguration
{
    public static ServiceProvider Build(RevitContext context)
    {
        PluginLog.Initialize();

        var services = new ServiceCollection();

        services.AddSingleton(context);
        services.AddSingleton(typeof(IPluginLogger<>), typeof(NLogPluginLogger<>));
        services.AddSingleton<IPluginLoggerFactory, NLogPluginLoggerFactory>();
        services.AddSingleton<RevitSnapshotProvider>();
        services.AddSingleton<ISnapshotCommands, RevitSnapshotCommands>();
        services.AddSingleton<IModelEditor, RevitModelEditor>();
        services.AddSingleton<IModelMetadataProvider, RevitModelMetadataProvider>();

        services.AddCore();
        services.AddLiteDbPersistence();
        services.AddUi();

        services.AddSingleton<DocumentEventRouter>();

        return services.BuildServiceProvider();
    }
}