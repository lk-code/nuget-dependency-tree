using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.Exceptions;
using System.Xml.Linq;

namespace NugetDepTree.Core.ProjectProvider;

/// <inheritdoc/>
public class DotNetCoreProjectProvider : IProjectProvider
{
    private string? _csprojContent;

    public async Task<bool> IsValidForProjectAsync(CancellationToken cancellationToken = default)
    {
        XElement? projectElement = GetProjektRootElement();

        // check for Sdk-Attribute on Project-element
        string? projectRootSdkAttributeValue = projectElement.Attribute("Sdk")?.Value;

        if (string.IsNullOrWhiteSpace(projectRootSdkAttributeValue))
            return false;

        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TargetFramework>> GetTargetFrameworksAsync(CancellationToken cancellationToken = default)
    {
        string[] targetFrameworkVersion = GetTargetFrameworkVersions();

        return targetFrameworkVersion.Select(x => new TargetFramework(x))
            .ToArray();
    }

    private string[] GetTargetFrameworkVersions()
    {
        XElement? projectElement = GetProjektRootElement();

        string? targetFrameworkVersion = projectElement?
            .Elements()
            .Where(e => e.Name.LocalName == "PropertyGroup")
            .Select(pg => pg.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "TargetFramework"))
            .FirstOrDefault(tf => tf != null)?
            .Value;
        if (targetFrameworkVersion is not null)
            return [targetFrameworkVersion];

        string? targetFrameworkVersions = projectElement?
            .Elements()
            .Where(e => e.Name.LocalName == "PropertyGroup")
            .Select(pg => pg.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "TargetFrameworks"))
            .FirstOrDefault(tf => tf != null)?
            .Value;

        if (targetFrameworkVersions is not null)
            return targetFrameworkVersions.Split(";")
                .Select(x => x.Trim())
                .OrderBy(x => Version.Parse(x[3..]))
                .ToArray();

        return [];
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
