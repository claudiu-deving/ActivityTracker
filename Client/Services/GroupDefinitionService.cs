using Client.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Client.Services;

public class GroupDefinitionService : IInitializable, IGroupDefinitionService
{
	public ObservableCollection<GroupDefinition> GroupDefinitions { get; set; } = [];
	private readonly IPathsProvider _pathsProvider;
	private readonly IAppLogger _appLogger;
	private readonly IActivityService _activityService;

	public GroupDefinitionService(IPathsProvider pathsProvider, IAppLogger appLogger, IActivityService activityService)
	{
		_pathsProvider = pathsProvider;
		_appLogger = appLogger;
		_activityService = activityService;
		_options = new JsonSerializerOptions
		{
			Converters = { new SKColorJsonConverter() }
		};
	}

	public async Task<ServiceResponse<bool>> Initialize()
	{
		var fileReadResponse = await Task.Run(ReadFromFile);
		if (!fileReadResponse.IsSuccess)
		{
			return fileReadResponse;
		}
		return ServiceResponse<bool>.Success();
	}

	/// <summary>
	/// Reads the group definitions from file
	/// </summary>
	/// <returns></returns>
	private ServiceResponse<bool> ReadFromFile()
	{
		try
		{
			var filePath = _pathsProvider.GetGroupsJsonPath();
			if (!File.Exists(filePath))
			{
				File.WriteAllText(filePath, "[]");
			}
			var jsonText = File.ReadAllText(filePath);
			var groups = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<GroupDefinition>>(jsonText, _options);
			if (groups is null)
			{
				File.WriteAllText(filePath, "[]");
				return ServiceResponse<bool>.Fail("Was unable to read the groups from the file");
			}
			foreach (var group in groups)
			{
				GroupDefinitions.Add(group);
			}
			_indexCounter = GroupDefinitions.Count;

			return ServiceResponse<bool>.Success(true);
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<bool>.Fail(ex.Message);
		}
	}

	public void AddGroup(GroupDefinition parentGroupDefinition)
	{
		parentGroupDefinition.Id = _indexCounter++;
		GroupDefinitions.Add(parentGroupDefinition);
	}
	public void AddSubGroup(GroupDefinition groupDefinition,int parentGroupDefinitionId)
	{
		groupDefinition.Id = _indexCounter++;
		var parentgroupDefinition = FindByRecursive(GroupDefinitions, parentGroupDefinitionId);
		parentgroupDefinition.GroupDefinitions.Add(groupDefinition);
	}
	private GroupDefinition FindByRecursive(IEnumerable<GroupDefinition> groupDefinitions, int parentGroupDefinitionId)
	{
		if (groupDefinitions.FirstOrDefault(x => x.Id == parentGroupDefinitionId) is not GroupDefinition foundGroupDefinition)
		{
			foreach (var definition in groupDefinitions)
			{
				return FindByRecursive(definition.GroupDefinitions, parentGroupDefinitionId);
			}
			return new GroupDefinition();
		}
		else
		{
			return foundGroupDefinition;
		}
	}

	private readonly JsonSerializerOptions _options;

	public ServiceResponse<bool> Save()
	{
		try
		{
			var jsonText = System.Text.Json.JsonSerializer.Serialize(GroupDefinitions.ToList<GroupDefinition>(), _options);
			var path = _pathsProvider.GetGroupsJsonPath();
			File.WriteAllText(path, jsonText);
			return ServiceResponse<bool>.Success();
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<bool>.Fail(ex.Message);
		}
	}

	public ServiceResponse<IEnumerable<GroupDefinition>> GetGroupDefinitions()
	{
		return ServiceResponse<IEnumerable<GroupDefinition>>.Success(GroupDefinitions);
	}




	public ServiceResponse<GroupDefinition> RemoveGroup(int groupId)
	{
		try
		{
			var group = GroupDefinitions.FirstOrDefault(g => g.Id == groupId);
			if (group is null)
			{
				return ServiceResponse<GroupDefinition>.Fail($"Activity group with {groupId} id was not found");
			}
			if (GroupDefinitions.Remove(group))
			{
				return ServiceResponse<GroupDefinition>.Success();
			}
			else
			{
				return ServiceResponse<GroupDefinition>.Fail($"Was unable to remove the group with id {groupId}");
			}
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<GroupDefinition>.Fail(ex.Message);
		}
	}

	public const string Ungrouped = "Ungrouped";


	private List<Activity> _remainingActivities = [];
	private int _indexCounter;
}