using Client.Models;
using System.Collections.ObjectModel;

namespace Client.Services;

public class ActivityRepository
{
	private readonly IGroupDefinitionService _groupDefinitionService;
	private readonly ISessionService _sessionService;
	private Session? _currentSession;
	public delegate void OnGroupDefinitionChanged();
	public event OnGroupDefinitionChanged? GroupDefinitionChanged;
	public ActivityRepository(
		IGroupDefinitionService GroupDefinitionService,
		ISessionService sessionService)
	{
		_groupDefinitionService = GroupDefinitionService;
		_sessionService = sessionService;
		_currentSession = _sessionService.GetSessions().FirstOrDefault();
	}


	public ObservableCollection<GroupViewModel> GetGroups(int sessionId)
	{
		 _currentSession = _sessionService.GetSessions().Find(x => x.Id == sessionId) ?? Session.Empty;

		var activities = _currentSession.Activities;
		var groupDefinitions = _groupDefinitionService.GetGroupDefinitions();
		if (!groupDefinitions.IsSuccess || groupDefinitions.Data is null) return [];
		return	GroupsProvider.GroupViewModels(activities, groupDefinitions.Data.ToList());
	}
}