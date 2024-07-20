using Client.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Client.Services;

public class ActivityGroupService : IInitializable, IActivityGroupService
{
	private readonly List<ActivityGroup> _activityGroups;
	private readonly IPathsProvider _pathsProvider;
	private readonly IAppLogger _appLogger;
	private readonly IActivityService _activityService;
	private static int _instanceCounter;
	public ActivityGroupService(IPathsProvider pathsProvider, IAppLogger appLogger,IActivityService activityService)
	{
		_activityGroups = [];
		_pathsProvider = pathsProvider;
		_appLogger = appLogger;
		_activityService = activityService;
		_options = new JsonSerializerOptions
		{
			Converters = { new SKColorJsonConverter() }
		};
		_instanceCounter++;
		Console.WriteLine(_instanceCounter);
	}

	public async Task<bool> Initialize()
	{
		return await Task.Run(ReadFromFile);
	}

	private bool ReadFromFile()
	{
		try
		{
			var filePath = _pathsProvider.GetGroupsJsonPath();
			if (!File.Exists(filePath))
			{
				return false;
			}
			var jsonText = File.ReadAllText(filePath);
			var groups = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ActivityGroup>>(jsonText, _options);
			if (groups is null)
			{
				return false;
			}
			foreach (var group in groups)
			{
				_activityGroups.Add(group);
			}
			GetActivities();
			UpdateOtherGroup();
			return true;
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return false;
		}
	}

	public void AddGroup(ActivityGroup activityGroup)
	{
		_activityGroups.Add(activityGroup);
		UpdateOtherGroup();
	}

	private readonly JsonSerializerOptions _options;

	public ServiceResponse<bool> Save()
	{
		try
		{
			var jsonText = System.Text.Json.JsonSerializer.Serialize(_activityGroups.ToList<ActivityGroupBase>(), _options);
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

	public ServiceResponse<IEnumerable<ActivityGroup>> GetActivityGroups()
	{
		return ServiceResponse<IEnumerable<ActivityGroup>>.Success(_activityGroups);
	}

	public ServiceResponse<ActivityGroup> AddPattern(int groupId, string pattern)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(pattern))
			{
				return ServiceResponse<ActivityGroup>.Fail($"Pattern name cannot be empty");
			}
			var group = _activityGroups.FirstOrDefault(g => g.Id == groupId);
			if (group == null)
			{
				return ServiceResponse<ActivityGroup>.Fail($"Activity group with {groupId} id was not found");
			}
			group.Patterns.Add(new Pattern { Sentence = pattern });
			UpdateOtherGroup();
			return ServiceResponse<ActivityGroup>.Success(group);
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<ActivityGroup>.Fail(ex.Message);
		}
	}

	public ServiceResponse<ActivityGroup> RemoveGroup(int groupId)
	{
		try
		{
			var group = _activityGroups.FirstOrDefault(g => g.Id == groupId);
			if (group == null)
			{
				return ServiceResponse<ActivityGroup>.Fail($"Activity group with {groupId} id was not found");
			}
			if (_activityGroups.Remove(group))
			{
				UpdateOtherGroup();
				return ServiceResponse<ActivityGroup>.Success();
			}
			else
			{
				return ServiceResponse<ActivityGroup>.Fail($"Was unable to remove the group with id {groupId}");
			}
		}
		catch (Exception ex)
		{
			_appLogger.LogError(ex.Message);
			return ServiceResponse<ActivityGroup>.Fail(ex.Message);
		}
	}

	private void UpdateOtherGroup()
	{
		var totalDuration = TimeSpan.FromTicks(_remainingActivities.Sum(a => a.Duration.Ticks));

		var otherGroup = _activityGroups.OfType<ActivityGroup>().FirstOrDefault(g => g.Name == "Other");

		if (otherGroup == null && totalDuration > TimeSpan.Zero)
		{
			otherGroup = new ActivityGroup { Name = "Other" };
			otherGroup.TotalDuration = totalDuration;
			_activityGroups.Add(otherGroup);
		}
		else if (otherGroup is not null)
		{
			otherGroup.Activities = new List<Activity>(_remainingActivities);
			otherGroup.TotalDuration = totalDuration;
		}
		else
		{
			return;
		}
	}
	private List<Activity> _remainingActivities;
	public void RegroupActivities()
	{
		if (!GetActivities())
		{
			return;
		}

		foreach (var group in _activityGroups.OfType<ActivityGroup>().Where(x => !x.Name.Equals("Other")))
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

	private bool GetActivities()
	{
		var getActivitiesResponse = _activityService.GetActivities();
		if (!getActivitiesResponse.IsSuccess || getActivitiesResponse.Data is null)
		{
			_appLogger.LogError(getActivitiesResponse.Message);
			return false;
		}
		_remainingActivities = getActivitiesResponse.Data;
		return true;
	}
}