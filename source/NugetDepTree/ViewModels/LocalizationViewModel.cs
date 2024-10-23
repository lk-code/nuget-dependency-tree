namespace NugetDepTree.ViewModels;

public partial class LocalizationViewModel : BaseViewModel
{
	public string LocalizedText => NugetDepTree.Resources.Strings.AppResources.HelloMessage;
}
