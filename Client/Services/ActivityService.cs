using Client.Models;
using System.IO;

namespace Client.Services;

public class ActivityService : IActivityService
{
	private readonly IAppPathsProvider _appPathsProvider;
	private readonly ISessionService _sessionService;
	private Session? _chosenSession;
	private readonly List<Session> _sessions;

	public ActivityService(IAppPathsProvider appPathsProvider, ISessionService sessionService)
	{
		_appPathsProvider = appPathsProvider;
		_sessionService = sessionService;
		_sessions = _sessionService.GetSessions();
		_chosenSession = _sessions.FirstOrDefault() ?? Session.Empty;
	}

	public ServiceResponse<List<Activity>> GetActivitiesForCurrent(string file)
	{
		try
		{
			_chosenSession = _sessions.Find(x => x.File.Equals(Path.Combine(_appPathsProvider.GetAppFolder(), file))) ?? Session.Empty;

			return ServiceResponse<List<Activity>>.Success(_chosenSession?.Activities ?? []);
		}
		catch (Exception ex)
		{
			return ServiceResponse<List<Activity>>.Fail(ex.Message);
		}
	}
}