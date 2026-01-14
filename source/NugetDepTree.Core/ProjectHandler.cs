using NugetDepTree.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace NugetDepTree.Core;

/// <inheritdoc/>
public class ProjectHandler(ILogger<ProjectHandler> logger,
    IEnumerable<IProjectProvider> projectProviders) : IProjectHandler
{
    /// <inheritdoc/>
    public async Task<IEnumerable<IProjectProvider>> GetProviderForProjectAsync(string projectContent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Determining project provider for project content.");

        var tasks = projectProviders.Select(async provider =>
        {
            try
            {
                provider.LoadFromProjectContent(projectContent);
                var isValid = await provider.IsValidForProjectAsync(cancellationToken).ConfigureAwait(false);
                return (provider, isValid);
            }
            catch
            {
                return (provider, isValid: false);
            }
        });

        var results = await Task.WhenAll(tasks)
            .ConfigureAwait(false);

        var provider = results.Where(result => result.provider is not null && result.isValid)
            .Select(result => result.provider);

        return provider;
    }
}
