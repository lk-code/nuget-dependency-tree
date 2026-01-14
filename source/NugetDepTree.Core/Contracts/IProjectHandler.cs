using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetDepTree.Core.Contracts;

/// <summary>
/// contains methods to get the correct project provider for a given project content
/// </summary>
public interface IProjectHandler
{
    /// <summary>
    /// returns the correct project provider(s) for the given project content
    /// </summary>
    /// <param name="projectContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<IProjectProvider>> GetProviderForProjectAsync(string projectContent,
        CancellationToken cancellationToken = default);
}
