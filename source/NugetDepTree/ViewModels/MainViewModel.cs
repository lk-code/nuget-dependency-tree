using DotNetFileParser;
using DotNetFileParser.Contracts;
using DotNetFileParser.Contracts.FluentDescriptors;
using NugetDepTree.Models;

namespace NugetDepTree.ViewModels;

public partial class MainViewModel() : BaseViewModel
{
    private const string SUPPORTED_FILE_TYPES = ".sln, .slnx, .csproj";

    [ObservableProperty] private bool _isLoading = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CloseCurrentCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenProjectFileCommand))]
    private bool _projectFileLoaded = false;

    [ObservableProperty] private ObservableCollection<PackageItem> _packageItemTree = new();

    [RelayCommand(CanExecute = nameof(CanOpenProjectFile))]
    private async Task OpenProjectFile()
    {
        string? content = null;
        string? fileName = null;
        
        try
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select a comic file",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".sln", ".slnx", ".csproj" } }, // file extension
                        { DevicePlatform.macOS, new[] { "sln", "slnx", "csproj" } }, // UTType values
                    }),
            };

            FileResult? result = await FilePicker.Default.PickAsync(options);
                
            if (result is not null)
            {
                fileName = result.FileName;
                
                using var stream = await result.OpenReadAsync();
                using var reader = new StreamReader(stream);
                content = await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        try
        {
            if (content is not null)
            {
                fileName = fileName ?? "unknown filename";
                
                await this.LoadProjectFileAsync(fileName,
                    content);
            }
        }
        catch (Exception ex)
        {
            // error while processing
        }
    }

    private bool CanOpenProjectFile()
    {
        return !ProjectFileLoaded;
    }

    [RelayCommand(CanExecute = nameof(CanCloseCurrent))]
    private async Task CloseCurrent()
    {
        ClearCurrent();
    }

    private bool CanCloseCurrent()
    {
        return ProjectFileLoaded;
    }

    private void ClearCurrent()
    {
        this.PackageItemTree.Clear();
        this.ProjectFileLoaded = false;
    }

    private async Task LoadProjectFileAsync(string fileName,
        string content)
    {
        this.IsLoading = true;

        try
        {
            var fileExtension = Path.GetExtension(fileName);
            IParserSelector parser = fileExtension.ToLowerInvariant() switch
            {
                ".sln" => DotNetParsers.GetForSlnSolution(),
                ".slnx" => DotNetParsers.GetForSlnxSolution(),
                ".csproj" => DotNetParsers.GetForCsprojProject(),
                _ => throw new NotSupportedException($"File extension '{fileExtension}' is not supported.")
            };

            var projectFile = await parser.Parse()
                .FromContentAsync(fileName, content);
            
            await this.LoadFileAsync(projectFile);

            this.ProjectFileLoaded = true;
        }
        catch (Exception err)
        {
            int i = 0;
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    private async Task LoadFileAsync(IProjectDescriptor projectFile)
    {
        string fileExtension = Path.GetExtension(projectFile.FileName);
        IParserSelector parser = fileExtension.ToLowerInvariant() switch
        {
            ".sln" => DotNetParsers.GetForSlnSolution(),
            ".slnx" => DotNetParsers.GetForSlnxSolution(),
            ".csproj" => DotNetParsers.GetForCsprojProject(),
            _ => throw new NotSupportedException($"File extension '{fileExtension}' is not supported.")
        };
        
        
        // var solution = CsProjFileParser.Parse(projectFile.Path);
        // var projects = solution.Projects.ToList();
        //
        // PackageItemTree.Clear();
        //
        // var solutionItem = new PackageItem(Path.GetFileName(solution.Path));
        // foreach (var project in projects)
        // {
        //     solutionItem.Children.Add(new PackageItem(Path.GetFileName(project.Path)));
        //
        //     // load project dependencies
        //     // load project nugets
        // }
        //
        // PackageItemTree.Add(solutionItem);
    }

    // private async Task LoadProjectAsync(StorageFile projectFile)
    // {
    //     var solution = CsProjFileParser.Parse(projectFile.Path);
    //     var projects = solution.Projects.ToList();
    //
    //     PackageItemTree.Clear();
    //
    //     var solutionItem = new PackageItem(Path.GetFileName(solution.Path));
    //     foreach (var project in projects)
    //     {
    //         solutionItem.Children.Add(new PackageItem(Path.GetFileName(project.Path)));
    //
    //         // load project dependencies
    //         // load project nugets
    //     }
    //
    //     PackageItemTree.Add(solutionItem);
    // }
    //
    // private async Task LoadSlnSolutionAsync(StorageFile projectFile)
    // {
    //     var solution = SlnFileParser.Parse(projectFile.Path);
    //     var projects = solution.Projects.ToList();
    //
    //     PackageItemTree.Clear();
    //
    //     var solutionItem = new PackageItem(Path.GetFileName(solution.Path));
    //     foreach (var project in projects)
    //     {
    //         solutionItem.Children.Add(new PackageItem(Path.GetFileName(project.Path)));
    //
    //         // load project dependencies
    //         // load project nugets
    //     }
    //
    //     PackageItemTree.Add(solutionItem);
    // }
    //
    // private async Task LoadSlnxSolutionAsync(StorageFile projectFile)
    // {
    //     var solution = SlnxFileParser.Parse(projectFile.Path);
    //     var projects = solution.Projects.ToList();
    //
    //     PackageItemTree.Clear();
    //
    //     var solutionItem = new PackageItem(Path.GetFileName(solution.Path));
    //     foreach (var project in projects)
    //     {
    //         solutionItem.Children.Add(new PackageItem(Path.GetFileName(project.Path)));
    //
    //         // load project dependencies
    //         // load project nugets
    //     }
    //
    //     PackageItemTree.Add(solutionItem);
    // }
}