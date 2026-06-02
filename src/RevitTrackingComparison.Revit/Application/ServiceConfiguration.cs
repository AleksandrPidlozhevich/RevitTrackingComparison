using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Core.DependencyInjection;
using RevitTrackingComparison.Persistence.DependencyInjection;
using RevitTrackingComparison.Revit.Events;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.Revit.Snapshots;
using RevitTrackingComparison.UI.DependencyInjection;

namespace RevitTrackingComparison.Revit.Application;

public static class ServiceConfiguration
{
    public static ServiceProvider Build(RevitContext context)
    {
        var services = new ServiceCollection();

        services.AddSingleton(context);
        services.AddSingleton<IRevitSnapshotProvider, RevitSnapshotProvider>();

        services.AddCore();
        services.AddLiteDbPersistence();
        services.AddUi();

        services.AddSingleton<DocumentEventRouter>();

        return services.BuildServiceProvider();
    }
}