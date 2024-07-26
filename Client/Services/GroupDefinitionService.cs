using Client.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Client.Services;

public class GroupDefinitionService : IInitializable, IGroupDefinitionService
{
	private List<ParentGroupDefinition> _groupDefinitions;
	private readonly IPathsProvider _pathsProvider;
	private readonly IAppLogger _appLogger;
	private readonly IActivityService _activityService;

	public GroupDefinitionService(IPathsProvider pathsProvider, IAppLogger appLogger, IActivityService activityService)
	{
		_groupDefinitions = [];
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
		var ungroupedUpdated = UpdateUngrouped();
		if (!fileReadResponse.IsSuccess)
		{
			return fileReadResponse;
		}
		if (!ungroupedUpdated)
		{
			return ServiceResponse<bool>.Fail("Unable to update the ungrouped group");
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
			var groups = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ParentGroupDefinition>>(jsonText, _options);
			if (groups is null)
			{
				File.WriteAllText(filePath, "[]");
				return ServiceResponse<bool>.Fail("Was unable to read the groups from the file");
			}
			foreach (var group in groups)
			{
				_groupDefinitions.Add(group);
			}
			_indexCounter = _groupDefinitions.Count;

			return ServiceResponse<bool>.Success(true);
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<bool>.Fail(ex.Message);
		}
	}

	public void AddParentGroup(ParentGroupDefinition parentGroupDefinition)
	{
		parentGroupDefinition.Id = _indexCounter++;
		_groupDefinitions.Add(parentGroupDefinition);
		UpdateUngrouped();
	}
	public void AddParentGroup(ParentGroupDefinition parentGroupDefinition, int grandParentId)
	{
		parentGroupDefinition.Id = _indexCounter++;
		FindByRecursive(_groupDefinitions,grandParentId);
		UpdateUngrouped();
	}

	private GroupDefinitionBase FindByRecursive(List<GroupDefinitionBase> groupDefinitions, int grandParentId)
	{
		if (groupDefinitions.Find(x => x.Id == grandParentId) is not ParentGroupDefinition foundGroupDefinition)
		{
			foreach (var definition in groupDefinitions)
			{
				return FindByRecursive(definition.GroupDefinitions, grandParentId);
			}
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
			var jsonText = System.Text.Json.JsonSerializer.Serialize(_groupDefinitions.ToList<GroupDefinitionBase>(), _options);
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

	public ServiceResponse<IEnumerable<ParentGroupDefinition>> GetGroupDefinitions()
	{
		return ServiceResponse<IEnumerable<ParentGroupDefinition>>.Success(_groupDefinitions);
	}

	public ServiceResponse<GroupDefinition> RemovePattern(int groupId, string pattern)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				return ServiceResponse<GroupDefinition>.Fail($"Pattern name cannot be empty");
			}
			var group = _groupDefinitions.FirstOrDefault(g => g.Id == groupId);
			if (group is null)
			{
				return ServiceResponse<GroupDefinition>.Fail($"Activity group with {groupId} id was not found");
			}
			var existingPattern = group.Patterns.FirstOrDefault(x => x.Sentence.Equals(pattern));
			if (existingPattern is null)
			{
				return ServiceResponse<GroupDefinition>.Fail($"Pattern {pattern} in group with {groupId} id was not found");
			}
			_remainingActivities.AddRange(existingPattern.Activities);
			group.Patterns.Remove(existingPattern);
			UpdateUngrouped();
			return ServiceResponse<GroupDefinition>.Success(group);
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<GroupDefinition>.Fail(ex.Message);
		}
	}

	public ServiceResponse<GroupDefinition> AddPattern(int groupId, string pattern)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				return ServiceResponse<GroupDefinition>.Fail($"Pattern name cannot be empty");
			}
			var group = _groupDefinitions.FirstOrDefault(g => g.Id == groupId);
			if (group is null)
			{
				return ServiceResponse<GroupDefinition>.Fail($"Activity group with {groupId} id was not found");
			}
			group.Patterns.Add(new Pattern { Sentence = pattern });
			UpdateUngrouped();
			return ServiceResponse<GroupDefinition>.Success(group);
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<GroupDefinition>.Fail(ex.Message);
		}
	}

	public ServiceResponse<GroupDefinition> RemoveGroup(int groupId)
	{
		try
		{
			var group = _groupDefinitions.FirstOrDefault(g => g.Id == groupId);
			if (group is null)
			{
				return ServiceResponse<GroupDefinition>.Fail($"Activity group with {groupId} id was not found");
			}
			_remainingActivities.AddRange(group.Activities);
			if (_groupDefinitions.Remove(group))
			{
				UpdateUngrouped();
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

	private bool UpdateUngrouped()
	{
		var totalDuration = TimeSpan.FromTicks(_remainingActivities.Sum(a => a.Duration.Ticks));

		var ungroupedGroup = _groupDefinitions.OfType<GroupDefinition>().FirstOrDefault(g => g.Name == Ungrouped);

		if (ungroupedGroup is null && totalDuration > TimeSpan.Zero && _groupDefinitions.Count != 0)
		{
			ungroupedGroup = new GroupDefinition
			{
				Name = Ungrouped,
				TotalDuration = totalDuration
			};
			_groupDefinitions.Add(ungroupedGroup);
		}
		else if (ungroupedGroup is null && _groupDefinitions.Count == 0)
		{
			ungroupedGroup = new GroupDefinition
			{
				Name = Ungrouped,
				TotalDuration = totalDuration
			};
			_groupDefinitions.Add(ungroupedGroup);
		}
		else if (ungroupedGroup is not null)
		{
			ungroupedGroup.Activities = new List<Activity>(_remainingActivities);
			ungroupedGroup.TotalDuration = totalDuration;
		}
		else
		{
			return false;
		}
		return true;
	}

	private List<Activity> _remainingActivities = [];
	private int _indexCounter;

	public void RegroupActivities()
	{
		if (!GetActivities())
		{
			return;
		}

		foreach (var group in _groupDefinitions.OfType<GroupDefinition>().Where(x => !x.Name.Equals(Ungrouped)))
		{
			group.Activities.Clear();
			foreach (var pattern in group.Patterns)
			{
				var regex = new Regex(pattern.Sentence, RegexOptions.IgnoreCase);
				var matchingActivities = _remainingActivities.Where(a => regex.IsMatch(a.Name)).ToList();

				foreach (var activity in matchingActivities)
				{
					group.Activities.Add(activity);
					pattern.Activities.Add(activity);
					_remainingActivities.Remove(activity);
				}
			}
			group.TotalDuration = TimeSpan.FromTicks(group.Activities.Sum(a => a.Duration.Ticks));
		}
	}



	

	public List<Activity> GetRemainingActivities()
	{
		return _remainingActivities;
	}

	public ServiceResponse<bool> UpdateGroupDefinitions(IEnumerable<GroupDefinition> GroupDefinitions)
	{
		try
		{
			_groupDefinitions = GroupDefinitions.ToList();
		}
		catch (Exception ex)
		{
			return ServiceResponse<bool>.Fail(ex.Message);
		}
		return ServiceResponse<bool>.Success();
	}
}