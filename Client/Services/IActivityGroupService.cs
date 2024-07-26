using Client.Models;

namespace Client.Services;

public interface IGroupDefinitionService
{
	void AddGroup(GroupDefinition GroupDefinition);

	ServiceResponse<GroupDefinition> AddPattern(int groupId, string pattern);

	ServiceResponse<IEnumerable<GroupDefinition>> GetGroupDefinitions();

	List<Activity> GetRemainingActivities();

	void RegroupActivities();

	ServiceResponse<GroupDefinition> RemoveGroup(int groupId);

	ServiceResponse<GroupDefinition> RemovePattern(int groupId, string pattern);

	ServiceResponse<bool> Save();

	ServiceResponse<bool> UpdateGroupDefinitions(IEnumerable<GroupDefinition> GroupDefinitions);
}