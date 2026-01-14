using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NugetDepTree.Core;
using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.ProjectProvider;

namespace NugetDepTree.Tests.Core;

public class ProjectHandlerTests
{
    private ServiceProvider _serviceProvider = null!;
    private IProjectHandler _projectHandler = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register ProjectProviders
        services.AddTransient<IProjectProvider, DotNetCoreProjectProvider>();
        services.AddTransient<IProjectProvider, DotNetLegacyProjectProvider>();

        // Register ProjectHandler
        services.AddTransient<IProjectHandler, ProjectHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _projectHandler = _serviceProvider.GetRequiredService<IProjectHandler>();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task GetProviderForProjectAsync_WithNetSdk10Target_ReturnsProviders()
    {
        // Arrange
        var projectContent = """
            <Project Sdk="Microsoft.NET.Sdk">
                <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                </PropertyGroup>
            </Project>
            """;

        // Act
        var providers = (await _projectHandler.GetProviderForProjectAsync(projectContent))
            .FirstOrDefault();

        // Act & Assert
        var provider = (await _projectHandler.GetProviderForProjectAsync(projectContent))
            .FirstOrDefault();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<DotNetCoreProjectProvider>();

        var targetFrameworks = await provider.GetTargetFrameworksAsync();

        targetFrameworks.Should().NotBeNull();
        targetFrameworks.Should().HaveCount(1);
        targetFrameworks.First().VersionValue.Should().Be("net10.0");
    }

    [Test]
    public async Task GetProviderForProjectAsync_WithMultipleNetSdkTargets_ReturnsProviders()
    {
        // Arrange
        var projectContent = """
            <Project Sdk="Microsoft.NET.Sdk">
                <PropertyGroup>
                    <TargetFrameworks>net10.0;net9.0; net6.0</TargetFrameworks>
                </PropertyGroup>
            </Project>
            """;

        // Act
        var providers = (await _projectHandler.GetProviderForProjectAsync(projectContent))
            .FirstOrDefault();

        // Act & Assert
        var provider = (await _projectHandler.GetProviderForProjectAsync(projectContent))
            .FirstOrDefault();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<DotNetCoreProjectProvider>();

        var targetFrameworks = (await provider.GetTargetFrameworksAsync()).ToList();

        targetFrameworks.Should().NotBeNull();
        targetFrameworks.Should().HaveCount(3);
        targetFrameworks[0].VersionValue.Should().Be("net6.0");
        targetFrameworks[1].VersionValue.Should().Be("net9.0");
        targetFrameworks[2].VersionValue.Should().Be("net10.0");
    }

    [Test]
    public async Task GetProviderForProjectAsync_WithNet48Target_ReturnsProviders()
    {
        // Arrange
        var projectContent = """
            <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                <PropertyGroup>
                    <ProjectGuid>{CB65F011-696B-430D-9A21-7C7CB1105AF4}</ProjectGuid>
                    <OutputType>Library</OutputType>
                </PropertyGroup>
                <PropertyGroup>
                    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
                </PropertyGroup>
            </Project>
            """;

        // Act & Assert
        var provider = (await _projectHandler.GetProviderForProjectAsync(projectContent))
            .FirstOrDefault();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<DotNetLegacyProjectProvider>();

        var targetFramework = await provider.GetTargetFrameworksAsync();

        targetFramework.Should().NotBeNull();
        targetFramework.Should().HaveCount(1);
        targetFramework.First().VersionValue.Should().Be("v4.8");
    }
}
