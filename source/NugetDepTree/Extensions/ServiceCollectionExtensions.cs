using Microsoft.Extensions.DependencyInjection;
using NugetDepTree.ViewModels;

namespace NugetDepTree.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUiServices(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();

        return services;
    }
}
