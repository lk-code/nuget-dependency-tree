
using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Core.Contracts;

public interface IDependencyResolver
{
    void Load(string content);
    Task<string[]> GetDependencyTreeAsync(TargetFramework targetFramework, CancellationToken cancellationToken = default);
}
