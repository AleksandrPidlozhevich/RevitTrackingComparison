using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;

namespace RevitTrackingComparison.Revit.Infrastructure;

// The single bridge between Revit-instantiated IExternalCommands (no DI) and the container.
// Revit creates commands by reflection, so they cannot take constructor dependencies; instead a
// command resolves exactly one service here and delegates to it. Keeping this the only service-locator
// point means the rest of the code stays on constructor injection.
internal static class CommandHost
{
    private static readonly IPluginLogger Logger = PluginLog.For(nameof(CommandHost));

    private static IServiceProvider? _services;

    public static void SetServices(IServiceProvider services) => _services = services;

    public static void ClearServices() => _services = null;

    // The action returns null on success or a user-facing error message; this maps it to a Result and
    // sets the command's out message.
    public static Result Run<TService>(ref string message, Func<TService, string?> action)
        where TService : notnull
    {
        if (_services is null)
        {
            message = "Plugin is not initialized.";
            return Result.Failed;
        }

        try
        {
            var error = action(_services.GetRequiredService<TService>());
            if (error is null)
                return Result.Succeeded;

            message = error;
            return Result.Failed;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unhandled error while running a command.");
            message = "The command failed unexpectedly.";
            return Result.Failed;
        }
    }
}
