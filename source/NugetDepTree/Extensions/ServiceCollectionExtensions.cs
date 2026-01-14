using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NugetDepTree.Logging;
using NugetDepTree.ViewModels;

namespace NugetDepTree.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUiServices(this IServiceCollection services)
    {
        // Register LogService as singleton
        services.AddSingleton<ILogService, LogService>();

        // Register logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        // Register UiLoggerProvider as singleton
        services.AddSingleton<ILoggerProvider, UiLoggerProvider>();

        services.AddTransient<MainViewModel>();

        return services;
    }
}
