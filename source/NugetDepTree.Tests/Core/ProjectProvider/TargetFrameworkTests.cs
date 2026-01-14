using AwesomeAssertions;
using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Tests.Core.ProjectProvider;

public class TargetFrameworkTests
{
    [TestCase("v4.8", true, false, false)]
    [TestCase("v4.7.2", true, false, false)]
    [TestCase("v4.6.1", true, false, false)]
    [TestCase("v4.5", true, false, false)]
    [TestCase("netcoreapp3.1", false, true, false)]
    [TestCase("net5.0", false, true, false)]
    [TestCase("net6.0", false, true, false)]
    [TestCase("net7.0", false, true, false)]
    [TestCase("net8.0", false, true, false)]
    [TestCase("net9.0", false, true, false)]
    [TestCase("net10.0", false, true, false)]
    [TestCase("netstandard2.0", false, false, true)]
    [TestCase("netstandard2.1", false, false, true)]
    [TestCase("netstandard1.6", false, false, true)]
    public void TargetFramework_DetectsFrameworkTypeCorrectly(string frameworkVersion,
        bool expectedLegacy,
        bool expectedSdk,
        bool expectedStandard)
    {
        // Arrange
        var targetFramework = new TargetFramework(frameworkVersion);

        // Act
        var isLegacy = targetFramework.IsNetLegacy();
        var isSdk = targetFramework.IsNetSdk();
        var isStandard = targetFramework.IsNetStandard();

        // Assert
        isLegacy.Should().Be(expectedLegacy, $"IsNetLegacy() failed for {frameworkVersion}");
        isSdk.Should().Be(expectedSdk, $"IsNetSdk() failed for {frameworkVersion}");
        isStandard.Should().Be(expectedStandard, $"IsNetStandard() failed for {frameworkVersion}");
    }
}
