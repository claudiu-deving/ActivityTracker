using Client.Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		CompositionRootBuilder builder = new CompositionRootBuilder();
		var serviceProvider =	await	builder.Build();
		var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}
}

