using Microsoft.Extensions.DependencyInjection;
using RevitTrackingComparison.UI.Services;

namespace RevitTrackingComparison.UI.DependencyInjection;

public static class UiServiceCollectionExtensions
{
    public static IServiceCollection AddUi(this IServiceCollection services)
    {
        services.AddSingleton<IComparisonView, ComparisonView>();
        return services;
    }
}