using Client.Models;
using System.Collections.ObjectModel;

namespace Client.Services
{
	public interface IGroupsProvider
	{
		ObservableCollection<GroupViewModelBase> GroupViewModels(List<Activity> activities, List<ParentGroupDefinition> groupDefinition);
	}
}