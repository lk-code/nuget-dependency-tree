using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Core.Extensions;

public static class TargetFrameworkExtensions
{
    public static TargetFramework? GetHighestTargetFramework(this IEnumerable<TargetFramework> targetFrameworks) =>
        targetFrameworks.OrderByDescTargetFramework()
            .FirstOrDefault();

    public static IEnumerable<string> OrderByTargetFramework(this IEnumerable<string> tf)
        => tf.OrderBy(x => Version.Parse(x[3..]));

    public static IEnumerable<TargetFramework> OrderByTargetFramework(this IEnumerable<TargetFramework> tf)
        => tf.OrderBy(x => Version.Parse(x.VersionValue[3..]));

    public static IEnumerable<string> OrderByDescTargetFramework(this IEnumerable<string> tf)
        => tf.OrderByDescending(x => Version.Parse(x[3..]));

    public static IEnumerable<TargetFramework> OrderByDescTargetFramework(this IEnumerable<TargetFramework> tf)
        => tf.OrderByDescending(x => Version.Parse(x.VersionValue[3..]));
}
