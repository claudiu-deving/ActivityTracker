using Client.Models;

namespace Client.Services;

public interface IActivityGroupService
{
	void AddGroup(ActivityGroup activityGroup); 
	ServiceResponse<ActivityGroup> AddPattern(int groupId, string pattern);
	ServiceResponse<IEnumerable<ActivityGroup>> GetActivityGroups();
	List<Activity> GetRemainingActivities();
	void RegroupActivities();
	ServiceResponse<ActivityGroup> RemoveGroup(int groupId);
	ServiceResponse<bool> Save();
}