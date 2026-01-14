namespace NugetDepTree.Core.Contracts;

public interface IDependencyResolverFactory
{
    IDependencyResolver? GetForProjectAsset();
}
