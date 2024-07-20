global using BiTLuZ.InfraLib;
using System.IO;

namespace Client.Services
{
	public class PathsProvider : IPathsProvider,IAppPathsProvider
	{
		public string GetAppFolder()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyServiceData");
		}

		public string GetGroupsJsonPath()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyServiceData", "groups.json");
		}
	}
}