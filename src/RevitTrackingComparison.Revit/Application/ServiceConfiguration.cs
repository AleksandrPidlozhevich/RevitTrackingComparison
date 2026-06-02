using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.DependencyInjection;
using RevitTrackingComparison.Persistence.DependencyInjection;
using RevitTrackingComparison.Revit.DependencyInjection;
using RevitTrackingComparison.Revit.Infrastructure;
using RevitTrackingComparison.UI.DependencyInjection;

namespace RevitTrackingComparison.Revit.Application;

public static class ServiceConfiguration
{
    public static ServiceProvider Build(RevitContext context)
    {
        PluginLog.Initialize();

        var services = new ServiceCollection();
        services.AddSingleton(context);

        services.AddCore();
        services.AddLiteDbPersistence();
        services.AddUi();
        services.AddRevitAdapters();

        return services.BuildServiceProvider();
    }
}
