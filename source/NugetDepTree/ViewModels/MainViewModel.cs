using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using NugetDepTree.Core.Contracts;
using NugetDepTree.Core.Extensions;
using NugetDepTree.Core.ProjectProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NugetDepTree.ViewModels;

public partial class MainViewModel(IProjectHandler projectHandler,
    IDependencyResolverFactory dependencyResolverFactory) : ViewModelBase
{
    private string? _projectFilePath;
    private bool _isNetLegacy = false;
    private bool _isNetSdk = false;
    private bool _isNetStandard = false;

    public string Log => string.Empty;

    [RelayCommand]
    public async Task OpenProjectAsync()
    {
        // Get the main window
        Window? mainWindow = GetMainWindow();
        if (mainWindow == null) return;

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
            // TODO: Process the selected project file path

            try
            {
                await LoadProject(projectFilePath);
            }
            catch (Exception ex)
            {
                // TODO: display error to user
            }
        }
    }

    private async Task LoadProject(string projectFilePath)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        string csprojContent = await System.IO.File.ReadAllTextAsync(projectFilePath);

        IProjectProvider? projectProvider = (await projectHandler.GetProviderForProjectAsync(csprojContent, cancellationToken)).FirstOrDefault();
        if (projectProvider is null)
            throw new InvalidOperationException("no provider returned. maybe invalid csproj");


        IEnumerable<TargetFramework> ver = await projectProvider!.GetTargetFrameworksAsync(cancellationToken);

        // TODO: handle all given versions
        TargetFramework? highestTargetFramework = ver.GetHighestTargetFramework();

        if (highestTargetFramework is null)
            throw new InvalidOperationException("no target framework found");

        _isNetLegacy = highestTargetFramework.IsNetLegacy();
        _isNetSdk = highestTargetFramework.IsNetSdk();
        _isNetStandard = highestTargetFramework.IsNetStandard();

        await LoadDependenciesAsync(highestTargetFramework);



        //// => Way for .NET Core (>6)


        //// check that a "obj" directory exists alongside the project file
        //string? projectDir = System.IO.Path.GetDirectoryName(projectFilePath);
        //if (projectDir == null)
        //    return;

        //string objDir = System.IO.Path.Combine(projectDir, "obj");
        //if (!System.IO.Directory.Exists(objDir))
        //{
        //    // obj directory does not exist, possibly the project has not been built yet
        //    // You might want to notify the user or handle this case accordingly
        //    return;
        //}

        //// check that a "project.assets.json" file exists in the "obj" directory
        //string assetsFilePath = System.IO.Path.Combine(objDir, "project.assets.json");
        //if (!System.IO.File.Exists(assetsFilePath))
        //{
        //    // assets file does not exist, possibly the project has not been built yet
        //    // You might want to notify the user or handle this case accordingly
        //    return;
        //}

        //// get the TargetFramework-Version from csproj and ensure that its net6.0 or higher
        //string[] targetFrameworks = await GetTargetFrameworksFromProject(projectFilePath);

        //string assetsContent = await System.IO.File.ReadAllTextAsync(assetsFilePath);
        //// load json from string as dynamic object
        //var json = System.Text.Json.JsonDocument.Parse(assetsContent);

        //var targets = json.RootElement.GetProperty("targets");
        //// get the highest .NET TargetFramework
        //var projectTargetFramework = targetFrameworks
        //    .Select(tf => tf.Trim())
        //    .Where(tf => tf.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        //    .OrderByDescending(tf => tf)
        //    .FirstOrDefault();

        //var targetFrameworkChild = targets.GetProperty(projectTargetFramework);
        //List<string> installedNugetsNames = new();
        //foreach (var property in targetFrameworkChild.EnumerateObject())
        //{
        //    installedNugetsNames.Add(property.Name);
        //}
    }

    private async Task LoadDependenciesAsync(TargetFramework targetFramework)
    {
        if (_projectFilePath is null)
            return;

        string projectFile = _projectFilePath;

        if (_isNetSdk)
        {
            // Load .NET SDK specific dependencies

            // 1. dotnet restore
            string absolutePathToProject = Path.Combine(Path.GetDirectoryName(projectFile)!, Path.GetFileName(projectFile));
            DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
            string temporaryObjPath = tempDirectory.FullName;

            string command = "dotnet";
            string arguments = $"restore {absolutePathToProject} -p:BaseIntermediateOutputPath={temporaryObjPath}";
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

            // 2. load project.assets.json
            string pathToProjectAssetsJson = Path.Combine(temporaryObjPath, "project.assets.json");
            string projectAssetsContent = await File.ReadAllTextAsync(pathToProjectAssetsJson);

            // 3. parse project.assets.json
            IDependencyResolver? dependencyResolver = dependencyResolverFactory.GetForProjectAsset();
            if (dependencyResolver is null)
                return;
            dependencyResolver.Load(projectAssetsContent);

            CancellationToken cancellationToken = CancellationToken.None;
            var blub = await dependencyResolver.GetDependencyTreeAsync(targetFramework,
                cancellationToken);
        }
        else if (_isNetStandard)
        {
            // Load .NET Standard specific dependencies
        }
        else if (_isNetLegacy)
        {
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
