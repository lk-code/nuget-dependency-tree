using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NugetDepTree.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private string? _projectFilePath;

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

            await LoadProject(projectFilePath);
        }
    }

    private async Task LoadProject(string projectFilePath)
    {
        // => Way for .NET Core (>6)

        // check that a "obj" directory exists alongside the project file
        string? projectDir = System.IO.Path.GetDirectoryName(projectFilePath);
        if (projectDir == null)
            return;

        string objDir = System.IO.Path.Combine(projectDir, "obj");
        if (!System.IO.Directory.Exists(objDir))
        {
            // obj directory does not exist, possibly the project has not been built yet
            // You might want to notify the user or handle this case accordingly
            return;
        }

        // check that a "project.assets.json" file exists in the "obj" directory
        string assetsFilePath = System.IO.Path.Combine(objDir, "project.assets.json");
        if (!System.IO.File.Exists(assetsFilePath))
        {
            // assets file does not exist, possibly the project has not been built yet
            // You might want to notify the user or handle this case accordingly
            return;
        }

        // get the TargetFramework-Version from csproj and ensure that its net6.0 or higher
        string[] targetFrameworks = await GetTargetFrameworksFromProject(projectFilePath);

        string assetsContent = await System.IO.File.ReadAllTextAsync(assetsFilePath);
        // load json from string as dynamic object
        var json = System.Text.Json.JsonDocument.Parse(assetsContent);

        var targets = json.RootElement.GetProperty("targets");
        // get the highest .NET TargetFramework
        var projectTargetFramework = targetFrameworks
            .Select(tf => tf.Trim())
            .Where(tf => tf.StartsWith("net", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(tf => tf)
            .FirstOrDefault();

        var targetFrameworkChild = targets.GetProperty(projectTargetFramework);
        List<string> installedNugetsNames = new();
        foreach (var property in targetFrameworkChild.EnumerateObject())
        {
            installedNugetsNames.Add(property.Name);
        }

        int i = 0;
    }

    private async Task<string[]> GetTargetFrameworksFromProject(string projectFilePath)
    {
        string csprojContent = await System.IO.File.ReadAllTextAsync(projectFilePath);

        // load string to xml
        var csprojXml = System.Xml.Linq.XDocument.Parse(csprojContent);

        // get TargetFramework or TargetFrameworks element
        var targetFrameworkElement = csprojXml.Descendants("TargetFramework").FirstOrDefault();
        if (targetFrameworkElement != null)
            return new[] { targetFrameworkElement.Value.Trim() };
        else
            return csprojXml.Descendants("TargetFrameworks")
                .FirstOrDefault()?
                .Value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();
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
