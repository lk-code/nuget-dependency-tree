using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.Exceptions;
using System.Xml.Linq;

namespace NugetDepTree.Core.ProjectProvider;

/// <inheritdoc/>
public class DotNetLegacyProjectProvider : IProjectProvider
{
    private string? _csprojContent;
    private string[] _csprojNamespaces = new[]
    {
        "http://schemas.microsoft.com/developer/msbuild/2003"
    };

    public async Task<bool> IsValidForProjectAsync(CancellationToken cancellationToken = default)
    {
        string? targetFrameworkVersion = GetTargetFrameworkVersion();

        if (string.IsNullOrWhiteSpace(targetFrameworkVersion))
            return false;

        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TargetFramework>> GetTargetFrameworksAsync(CancellationToken cancellationToken = default)
    {
        string? targetFrameworkVersion = GetTargetFrameworkVersion();

        if (!string.IsNullOrWhiteSpace(targetFrameworkVersion))
            return [
                new(targetFrameworkVersion)];

        return [];
    }

    private string? GetTargetFrameworkVersion()
    {
        XElement? projectElement = GetProjektRootElement();

        string? targetFrameworkVersion = projectElement?
            .Elements()
            .Where(e => e.Name.LocalName == "PropertyGroup")
            .Select(pg => pg.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "TargetFrameworkVersion"))
            .FirstOrDefault(tf => tf != null)?
            .Value;
        return targetFrameworkVersion;
    }

    private XElement GetProjektRootElement()
    {
        if (string.IsNullOrWhiteSpace(_csprojContent))
            throw new ProjectException(ExceptionType.PROJECT_CONTENT_EMPTY);

        XDocument xmlProject = XDocument.Parse(_csprojContent!);

        XElement? projectElement = xmlProject.Root;
        if (projectElement is null
            || (projectElement is not null && projectElement.Name.LocalName != "Project"))
            throw new ProjectException(ExceptionType.PROJECT_CONTENT_INVALID, "No Project-Tag found.");

        return projectElement!;
    }

    /// <inheritdoc/>
    public void LoadFromProjectContent(string csprojContent) => _csprojContent = csprojContent;
}
