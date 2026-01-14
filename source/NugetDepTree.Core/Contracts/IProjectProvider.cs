using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Core.Contracts;

public interface IProjectProvider
{
    Task<bool> IsValidForProjectAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TargetFramework>> GetTargetFrameworksAsync(CancellationToken cancellationToken = default);
    void LoadFromProjectContent(string csprojContent);
}
