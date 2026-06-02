using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Core.Abstractions;
using RevitTrackingComparison.Revit.Events;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Application;

public sealed class RevitTrackingApplication : IExternalApplication
{
    private const string RibbonPanel = "Snapshot tracking";

    private static readonly IPluginLogger Logger = PluginLog.For(nameof(RevitTrackingApplication));

    public static ServiceProvider? Services { get; private set; }

    private RevitContext? _context;
    private DocumentEventRouter? _router;

    public Result OnStartup(UIControlledApplication application)
    {
        try
        {
            _context = new RevitContext();
            Services = ServiceConfiguration.Build(_context);

            application.ViewActivated += OnViewActivated;

            _router = Services.GetRequiredService<DocumentEventRouter>();
            _router.Subscribe(application.ControlledApplication);

            CreateRibbon(application);

            Logger.Info("Add-in started.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to start add-in.");
            return Result.Failed;
        }
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            application.ViewActivated -= OnViewActivated;
            _router?.Dispose();
            Services?.Dispose();
            Services = null;
            Logger.Info("Add-in stopped.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to stop add-in.");
        }

        return Result.Succeeded;
    }

    private void OnViewActivated(object? sender, ViewActivatedEventArgs e)
    {
        if (_context is not null && sender is UIApplication uiApp)
            _context.UiApplication = uiApp;
    }

    private static void CreateRibbon(UIControlledApplication application)
    {
        // Single-argument overload always targets the built-in Add-Ins tab (localized name in the UI).
        var panel = application.GetRibbonPanels()
                        .FirstOrDefault(p => p.Name == RibbonPanel)
                    ?? application.CreateRibbonPanel(RibbonPanel);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var hubButton = new PushButtonData(
            "ShowSnapshotHub",
            "Snapshots",
            assemblyPath,
            typeof(Commands.ShowSnapshotHubCommand).FullName)
        {
            ToolTip = "Open the snapshot hub: take a snapshot, edit capture settings, or compare snapshots."
        };

        if (panel.AddItem(hubButton) is PushButton pushButton)
            RibbonIconLoader.ApplyTo(pushButton);

        Logger.Info($"Ribbon panel '{RibbonPanel}' registered on Add-Ins tab.");
    }
}