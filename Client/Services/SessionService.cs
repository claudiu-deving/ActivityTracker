using System.IO;

namespace Client.Services;

public class SessionService : ISessionService
{
	private readonly IAppPathsProvider _appPathsProvider;
	private List<Session>? _sessions;

	public SessionService(IAppPathsProvider appPathsProvider)
	{
		_appPathsProvider = appPathsProvider;
	}

	public List<Session> GetSessions()
	{
		if (_sessions is not null) return _sessions;

		_sessions = [];
		var directory = _appPathsProvider.GetAppFolder();
		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
			return _sessions;
		}
		var filteredJsonFiles = Directory.GetFiles(directory, "window_times_*.json");
		int counter = 0;
		foreach (var file in filteredJsonFiles)
		{
			_sessions.Add(SessionFactory.Create(file, counter));
			counter++;
		}

		return _sessions;
	}
}