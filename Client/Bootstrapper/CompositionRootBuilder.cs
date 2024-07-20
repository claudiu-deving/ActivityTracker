using Client.Services;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

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

		_serviceCollection.AddSingleton<IActivityGroupService, ActivityGroupService>();
		// Register view models
		_serviceCollection.AddSingleton<MainViewModel>();
		// Register views
		_serviceCollection.AddSingleton<MainWindow>();
		var serviceProvider = _serviceCollection.BuildServiceProvider();

		foreach (var serviceType in InitializableServices())
		{
			if (serviceProvider.GetService(serviceType as Type) is IInitializable initializable)
			{
				await initializable.Initialize();
			}
		}
		return serviceProvider;
	}

	private  static IEnumerable InitializableServices()
	{
		yield return typeof(IActivityGroupService);
		yield return typeof(MainViewModel);
	}
}
