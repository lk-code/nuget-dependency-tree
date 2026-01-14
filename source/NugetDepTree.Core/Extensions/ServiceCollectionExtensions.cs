
using Microsoft.Extensions.DependencyInjection;
using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.DependencyResolver;
using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IProjectHandler, ProjectHandler>();

        services.AddTransient<IProjectProvider, DotNetCoreProjectProvider>();
        services.AddTransient<IProjectProvider, DotNetLegacyProjectProvider>();

        services.AddSingleton<IDependencyResolverFactory, DependencyResolverFactory>();

        services.AddTransient<IDependencyResolver, ProjectAssetDependencyResolver>();

        return services;
    }
}
