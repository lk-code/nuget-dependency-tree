using Foundation;
using UIKit;

namespace NugetDepTree;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override void BuildMenu(IUIMenuBuilder builder)
	{
		base.BuildMenu(builder);
    
		builder.RemoveMenu(UIMenuIdentifier.Edit.GetConstant());
		builder.RemoveMenu(UIMenuIdentifier.Font.GetConstant());
		builder.RemoveMenu(UIMenuIdentifier.Format.GetConstant());
		builder.RemoveMenu(UIMenuIdentifier.Services.GetConstant());
		builder.RemoveMenu(UIMenuIdentifier.Hide.GetConstant());
    
		builder.RemoveMenu(UIMenuIdentifier.Close.GetConstant());
		builder.RemoveMenu(UIMenuIdentifier.Document.GetConstant());

		builder.System.SetNeedsRebuild();
	}
}
