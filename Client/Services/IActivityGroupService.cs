using Client.Models;
using System.Collections.ObjectModel;

namespace Client.Services;

public interface IGroupDefinitionService
{
	ObservableCollection<GroupDefinition> GroupDefinitions { get; set; }

	void AddGroup(GroupDefinition GroupDefinition);

	ServiceResponse<IEnumerable<GroupDefinition>> GetGroupDefinitions();

	ServiceResponse<bool> Save();
}