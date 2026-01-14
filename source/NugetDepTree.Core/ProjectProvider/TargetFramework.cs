using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NugetDepTree.Core.ProjectProvider;

[DebuggerDisplay("{nameof(TargetFramework)} is {VersionValue}")]
public partial class TargetFramework(string targetFrameworkValue)
{
    public string VersionValue { get; init; } = targetFrameworkValue;

    [GeneratedRegex(@"^v\d+\.\d+(\.\d+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NetLegacyRegex();

    [GeneratedRegex(@"^(netcoreapp|net)\d+\.\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NetSdkRegex();

    [GeneratedRegex(@"^netstandard\d+\.\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex NetStandardRegex();

    /// <summary>
    /// true, if the target framework is a .NET Legacy framework (e.g., net48, net472)
    /// </summary>
    /// <returns></returns>
    public bool IsNetLegacy() => NetLegacyRegex().IsMatch(VersionValue);

    /// <summary>
    /// true, if the target framework is a .NET SDK framework (e.g., netcoreapp3.1, net5.0, net6.0, net7.0, net8.0, net10.0)
    /// </summary>
    /// <returns></returns>
    public bool IsNetSdk() => NetSdkRegex().IsMatch(VersionValue);

    /// <summary>
    /// true, if the target framework is a .NET Standard framework (e.g., netstandard2.0, netstandard2.1)
    /// </summary>
    /// <returns></returns>
    public bool IsNetStandard() => NetStandardRegex().IsMatch(VersionValue);
}
