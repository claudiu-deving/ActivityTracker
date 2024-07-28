using Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Client.Services;

/// <summary>
/// Provides the groups for the viewmodel to display.
/// </summary>
public class GroupsProvider 
{
	public static ObservableCollection<GroupViewModel> GroupViewModels(List<Activity> activities, List<GroupDefinition> groupDefinition)
	{
		return new ObservableCollection<GroupViewModel>(GroupActivities(activities, groupDefinition));
	}
	private static List<GroupViewModel> GroupActivities(List<Activity> activities, List<GroupDefinition> groupDefinitions)
	{
		var result = new List<GroupViewModel>();
		TimeSpan time = TimeSpan.Zero;
		foreach (var groupDefinition in groupDefinitions)
		{
			result.Add(GroupByPattern(activities, time, groupDefinition));
		}
		result.Add(UngroupedGroup(activities));
		return result;
	}

	private static GroupViewModel UngroupedGroup(List<Activity> activities)
	{
		return GroupViewModel.Ungrouped(activities);
	}

	private static  GroupViewModel GroupByPattern(List<Activity> activities, TimeSpan time, GroupDefinition groupDefinition)
	{
		GroupViewModel groupViewModel= new(groupDefinition);

		var regex = new Regex(groupDefinition.Pattern, RegexOptions.IgnoreCase);
		var matchingActivities = activities.Where(a => regex.IsMatch(a.Name)).ToList();

		foreach (var activity in matchingActivities)
		{
			time += activity.TotalDuration;
			groupViewModel.Activities.Add(activity);
			activities.Remove(activity);
		}


		foreach (var group in groupDefinition.GroupDefinitions)
		{
			 regex = new Regex(group.Pattern, RegexOptions.IgnoreCase);
			 matchingActivities = groupViewModel.Activities.Where(a => regex.IsMatch(a.Name)).ToList();
			time = TimeSpan.Zero;
			groupViewModel.Groups.Add(GroupByPattern(matchingActivities, time, group));
		}

		return groupViewModel;
	}
}


public class GroupViewModel : ObservableObject
{
	private readonly GroupDefinition _groupDefinitionBase;
	private GroupViewModel()
	{
		_groupDefinitionBase = new GroupDefinition()
		{
			Name = "Ungrouped",
			SKColor = SKColor.Parse("000000"),
		};
	}
	public GroupViewModel(GroupDefinition groupDefinitionBase)
	{
		_groupDefinitionBase = groupDefinitionBase;
		Activities.CollectionChanged += Activities_CollectionChanged;
	}

	private void Activities_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		Duration =TimeSpan.FromMilliseconds( Activities.Sum(x => x.TotalDuration.TotalMilliseconds));
	}

	public string Name
	{
		get => _groupDefinitionBase.Name;
		set
		{
			if(_groupDefinitionBase is not null)
			{
				_groupDefinitionBase.Name = value;
				OnPropertyChanged();
			}
		}
	}

	public SKColor Color
	{
		get => _groupDefinitionBase.SKColor;
		set
		{
			_groupDefinitionBase.SKColor = value;
			OnPropertyChanged();
		}
	}


	public TimeSpan Duration { get; private set; }

	public ObservableCollection<Activity> Activities { get; set; } = [];

	public ObservableCollection<GroupViewModel> Groups { get; set; } = [];

	public static GroupViewModel Ungrouped(IEnumerable<Activity> activities) => new()
	{
		Activities = new(activities)
	};
}



