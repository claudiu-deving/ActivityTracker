using Client.Services;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Windows;

namespace Client.Bootstrapper;

internal class CompositionRootBuilder
{
	private readonly ServiceCollection _serviceCollection;

	public CompositionRootBuilder()
	{
		_serviceCollection = new ServiceCollection();
	}
	public async Task<ServiceProvider> Build()
	{
		// Register services
		_serviceCollection.AddSingleton<IPathsProvider, PathsProvider>();
		_serviceCollection.AddSingleton<IAppPathsProvider, PathsProvider>();
		_serviceCollection.AddSingleton<IAppLogger, AppLogger>();
		_serviceCollection.AddSingleton<IActivityService, ActivityService>();	
		_serviceCollection.AddSingleton<IActivityGroupService, ActivityGroupService>();
		// Register view models
		_serviceCollection.AddSingleton<MainViewModel>();
		// Register views
		_serviceCollection.AddSingleton<MainWindow>();
		var serviceProvider = _serviceCollection.BuildServiceProvider();
		var viewModelInitialization =  await serviceProvider.GetService<MainViewModel>()!.Initialize();
		if(!viewModelInitialization.IsSuccess)
		{
			MessageBox.Show(viewModelInitialization.Message);
		}
		return serviceProvider;
	}
}
