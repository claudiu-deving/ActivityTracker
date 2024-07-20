using Client.Models;

namespace Client.Services
{
	public interface IActivityService
	{
		void SetCurrentActivityFile(string file);
		ServiceResponse<List<Activity>> GetActivities();
	}
}