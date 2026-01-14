using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.DependencyResolver;

namespace NugetDepTree.Core;

public class DependencyResolverFactory(IEnumerable<IDependencyResolver> dependencyResolvers) : IDependencyResolverFactory
{
    public IDependencyResolver? GetForProjectAsset()
        => dependencyResolvers.FirstOrDefault(r => r.GetType() == typeof(ProjectAssetDependencyResolver));
}
