using Client.Models;

namespace Client.Services
{
	public interface IActivityService
	{
		ServiceResponse<List<Activity>> GetActivitiesForCurrent(string file);
	}
}