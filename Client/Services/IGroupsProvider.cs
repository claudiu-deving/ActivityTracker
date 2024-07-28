using Client.Models;
using System.Collections.ObjectModel;

namespace Client.Services
{
	public interface IGroupsProvider
	{
		ObservableCollection<GroupViewModel> GroupViewModels(List<Activity> activities, List<GroupDefinition> groupDefinition);
	}
}