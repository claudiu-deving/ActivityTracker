using Client.Models;

namespace Client.Services;

public class ActivityRepository
{
	private readonly IGroupDefinitionService _GroupDefinitionService;
	private readonly IActivityService _activityService;
	private readonly ISessionService _sessionService;
	private readonly IGroupsProvider _groupsProvider;

	public ActivityRepository(
		IGroupDefinitionService GroupDefinitionService,
		IActivityService activityService,
		ISessionService sessionService,
		IGroupsProvider groupsProvider)
	{
		_GroupDefinitionService = GroupDefinitionService;
		_activityService = activityService;
		_sessionService = sessionService;
		_groupsProvider = groupsProvider;
	}

	public List<ParentGroupDefinition> GetGroups(int sessionId)
	{
		var currentSession = _sessionService.GetSessions().Find(x => x.Id == sessionId) ?? Session.Empty;

		var activities = currentSession.Activities;
		var groupDefinitions = _GroupDefinitionService.GetGroupDefinitions();
		if (!groupDefinitions.IsSuccess) return [];
		_groupsProvider.GroupViewModels(activities, groupDefinitions.Data.ToList());
	
	}
}