using Application;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Gui;

public partial class App : Avalonia.Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		var collection = new ServiceCollection();
		collection.AddApplicationServices();

		var serviceProvider = collection.BuildServiceProvider();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = ActivatorUtilities.CreateInstance<MainWindow>(serviceProvider);

		base.OnFrameworkInitializationCompleted();
	}
}