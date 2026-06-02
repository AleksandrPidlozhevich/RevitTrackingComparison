using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.Revit.Events;
using RevitTrackingComparison.Revit.Infrastructure;

namespace RevitTrackingComparison.Revit.Application;

public sealed class RevitTrackingApplication : IExternalApplication
{
    private const string RibbonTab = "TrackingComparison";
    private const string RibbonPanel = "Tracking";

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

            PluginLog.Info("Add-in started.");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to start add-in.");
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
            PluginLog.Info("Add-in stopped.");
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to stop add-in.");
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
        try
        {
            application.CreateRibbonTab(RibbonTab);
        }
        catch
        {
        }

        var panel = application.GetRibbonPanels(RibbonTab)
                        .FirstOrDefault(p => p.Name == RibbonPanel)
                    ?? application.CreateRibbonPanel(RibbonTab, RibbonPanel);

        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var buttonData = new PushButtonData(
            "ShowComparison",
            "Compare\nsnapshots",
            assemblyPath,
            typeof(Commands.ShowComparisonCommand).FullName)
        {
            ToolTip = "Capture a snapshot of the active document and compare it with the previous one."
        };

        panel.AddItem(buttonData);
    }
}