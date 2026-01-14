using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.ProjectProvider;
using System.Text.Json;

namespace NugetDepTree.Core.DependencyResolver;

public class ProjectAssetDependencyResolver() : IDependencyResolver
{
    private string _projectAssetsContent = string.Empty;

    public void Load(string content) => _projectAssetsContent = content;

    public async Task<string[]> GetDependencyTreeAsync(TargetFramework targetFramework, CancellationToken cancellationToken = default)
    {
        string targetFrameworkVersion = targetFramework.VersionValue;
        JsonDocument json = JsonDocument.Parse(_projectAssetsContent);

        // get targets => {TargetFramework} => {NuGetPaclageName} => dependencies => list of nuget dependencies

        var targetsElement = json.RootElement.GetProperty("targets");
        var targetVersionElement = targetsElement.GetProperty(targetFrameworkVersion);
        var packageDependencies = new List<string>();
        foreach (var packageElement in targetVersionElement.EnumerateObject())
        {
            packageDependencies.Add(packageElement.Name);
        }

        return [];
    }
}
