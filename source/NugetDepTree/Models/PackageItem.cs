namespace NugetDepTree.Models;

public partial class PackageItem : ObservableRecipient
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private ObservableCollection<PackageItem> _children = new();

    public PackageItem(string name)
    {
        this.Name = name;
    }
}