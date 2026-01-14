using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.Extensions;
using NugetDepTree.Core.ProjectProvider;
using NugetDepTree.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NugetDepTree.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IProjectHandler _projectHandler;
    private readonly IDependencyResolverFactory _dependencyResolverFactory;
    private readonly ILogService _logService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly StringBuilder _logBuilder = new();

    private string? _projectFilePath;
    private bool _isNetLegacy = false;
    private bool _isNetSdk = false;
    private bool _isNetStandard = false;

    [ObservableProperty]
    private string _log = string.Empty;

    [ObservableProperty]
    private int _logCaretIndex = 0;

    public MainViewModel(
        IProjectHandler projectHandler,
        IDependencyResolverFactory dependencyResolverFactory,
        ILogService logService,
        ILogger<MainViewModel> logger)
    {
        _projectHandler = projectHandler;
        _dependencyResolverFactory = dependencyResolverFactory;
        _logService = logService;
        _logger = logger;

        // Subscribe to log messages
        _logService.LogMessageReceived += OnLogMessageReceived;

        _logger.LogInformation("Application started");
    }

    private void OnLogMessageReceived(object? sender, string message)
    {
        // Update log on UI thread
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _logBuilder.AppendLine(message);
            Log = _logBuilder.ToString();
            // Set caret to end to auto-scroll
            LogCaretIndex = Log.Length;
        });
    }

    [RelayCommand]
    public async Task OpenProjectAsync()
    {
        _logger.LogInformation("Opening project file dialog...");

        // Get the main window
        Window? mainWindow = GetMainWindow();
        if (mainWindow == null)
        {
            _logger.LogError("Main window not found");
            return;
        }

        // Define file type filters
        FilePickerFileType[] fileTypeFilters = new[]
        {
            new FilePickerFileType("Project Files")
            {
                Patterns = new[] { "*.csproj", "*.vbproj", "*.fsproj" }
            },
            new FilePickerFileType("All Files")
            {
                Patterns = new[] { "*.*" }
            }
        };

        // Show open file dialog
        IReadOnlyList<IStorageFile> files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a project file",
            AllowMultiple = false,
            FileTypeFilter = fileTypeFilters
        });

        if (files.Any())
        {
            var projectFilePath = files.First().Path.LocalPath;
            _projectFilePath = projectFilePath;
            _logger.LogInformation("Selected project file: {ProjectPath}", projectFilePath);

            try
            {
                await LoadProject(projectFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project: {ProjectPath}", projectFilePath);
            }
        }
        else
        {
            _logger.LogInformation("No project file selected");
        }
    }

    private async Task LoadProject(string projectFilePath)
    {
        _logger.LogInformation("Loading project");

        CancellationToken cancellationToken = CancellationToken.None;
        string csprojContent = await System.IO.File.ReadAllTextAsync(projectFilePath);

        _logger.LogDebug("Project content loaded, detecting provider...");

        IProjectProvider? projectProvider = (await _projectHandler.GetProviderForProjectAsync(csprojContent, cancellationToken)).FirstOrDefault();
        if (projectProvider is null)
        {
            _logger.LogError("No provider found for project");
            throw new InvalidOperationException("no provider returned. maybe invalid csproj");
        }

        _logger.LogInformation("Provider found: {ProviderType}", projectProvider.GetType().Name);

        IEnumerable<TargetFramework> ver = await projectProvider!.GetTargetFrameworksAsync(cancellationToken);

        // TODO: handle all given versions
        TargetFramework? highestTargetFramework = ver.GetHighestTargetFramework();

        if (highestTargetFramework is null)
        {
            _logger.LogError("No target framework found in project");
            throw new InvalidOperationException("no target framework found");
        }

        _isNetLegacy = highestTargetFramework.IsNetLegacy();
        _isNetSdk = highestTargetFramework.IsNetSdk();
        _isNetStandard = highestTargetFramework.IsNetStandard();

        _logger.LogInformation("Target framework: {Framework} (Legacy: {IsLegacy}, SDK: {IsSdk}, Standard: {IsStandard})",
            highestTargetFramework.VersionValue, _isNetLegacy, _isNetSdk, _isNetStandard);

        await LoadDependenciesAsync(highestTargetFramework);
    }

    private async Task LoadDependenciesAsync(TargetFramework targetFramework)
    {
        if (_projectFilePath is null)
            return;

        _logger.LogInformation("Loading dependencies for target framework: {Framework}", targetFramework.VersionValue);

        string projectFile = _projectFilePath;

        if (_isNetSdk)
        {
            _logger.LogInformation("Loading .NET SDK dependencies...");

            // 1. dotnet restore
            string absolutePathToProject = Path.Combine(Path.GetDirectoryName(projectFile)!, Path.GetFileName(projectFile));
            DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
            string temporaryObjPath = tempDirectory.FullName;

            _logger.LogDebug("Temporary obj path: {TempPath}", temporaryObjPath);

            string command = "dotnet";
            string arguments = $"restore {absolutePathToProject} -p:BaseIntermediateOutputPath={temporaryObjPath}";
            
            _logger.LogInformation("Running: {Command} {Arguments}", command, arguments);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi)!;

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                _logger.LogDebug("dotnet restore output: {Output}", stdout);
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                _logger.LogWarning("dotnet restore errors: {Errors}", stderr);
            }

            _logger.LogInformation("dotnet restore completed with exit code: {ExitCode}", process.ExitCode);

            // 2. load project.assets.json
            string pathToProjectAssetsJson = Path.Combine(temporaryObjPath, "project.assets.json");
            _logger.LogDebug("Loading project.assets.json from: {AssetsPath}", pathToProjectAssetsJson);

            string projectAssetsContent = await File.ReadAllTextAsync(pathToProjectAssetsJson);

            // 3. parse project.assets.json
            IDependencyResolver? dependencyResolver = _dependencyResolverFactory.GetForProjectAsset();
            if (dependencyResolver is null)
            {
                _logger.LogError("No dependency resolver found");
                return;
            }

            _logger.LogInformation("Parsing dependencies...");
            dependencyResolver.Load(projectAssetsContent);

            CancellationToken cancellationToken = CancellationToken.None;
            var dependencies = await dependencyResolver.GetDependencyTreeAsync(targetFramework, cancellationToken);

            _logger.LogInformation("Dependencies loaded successfully");
        }
        else if (_isNetStandard)
        {
            _logger.LogInformation("Loading .NET Standard dependencies...");
            // Load .NET Standard specific dependencies
        }
        else if (_isNetLegacy)
        {
            _logger.LogInformation("Loading .NET Legacy dependencies...");
            // Load .NET Legacy specific dependencies
        }
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}
