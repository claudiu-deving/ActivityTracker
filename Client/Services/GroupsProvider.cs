using Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Client.Services;

/// <summary>
/// Provides the groups for the viewmodel to display.
/// </summary>
public class GroupsProvider : IGroupsProvider
{
	public ObservableCollection<GroupViewModelBase> GroupViewModels(List<Activity> activities, List<ParentGroupDefinition> groupDefinition)
	{
		//TODO: Implement Logic
		return new ObservableCollection<GroupViewModelBase>(GroupActivities(activities, groupDefinition));
	}
	private List<GroupViewModelBase> GroupActivities(List<Activity> activities, List<ParentGroupDefinition> groupDefinitions)
	{
		var result = new List<GroupViewModelBase>();
		TimeSpan time = TimeSpan.Zero;
		foreach (var groupDefinition in groupDefinitions)
		{
			result.Add(GroupByPattern(activities, time, groupDefinition));
		}

		return result;
	}

	private static GroupViewModelBase GroupByPattern(List<Activity> activities, TimeSpan time, ParentGroupDefinition groupDefinition)
	{
		foreach (GroupDefinitionBase group in groupDefinition.GroupDefinitions)
		{
			if (group is ParentGroupDefinition parentGroupDefinition)
			{
				return GroupByPattern(activities, time, parentGroupDefinition);
			}
			else if (group is ChildGroupDefinition childGroupDefinition)
			{
				return GroupByPattern(activities, time, childGroupDefinition);
			}
			else
			{
				throw new NotSupportedException();
			}
		}
		return new GroupViewModelBase(new GroupDefinition());
	}
	private static ChildGroupViewModel GroupByPattern(List<Activity> activities, TimeSpan time, ChildGroupDefinition groupDefinition)
	{
		foreach (var pattern in groupDefinition.Patterns)
		{
			var regex = new Regex(pattern.Sentence, RegexOptions.IgnoreCase);
			var matchingActivities = activities.Where(a => regex.IsMatch(a.Name)).ToList();

			foreach (var activity in matchingActivities)
			{
				time += activity.Duration;
				pattern.Activities.Add(activity);
				activities.Remove(activity);
			}
		}

		return (ChildGroupViewModel)ViewModelFactory.Create(groupDefinition);
	}
}

public class GroupViewModelBase : ObservableObject
{
	private readonly GroupDefinitionBase _groupDefinitionBase;

	public GroupViewModelBase(GroupDefinitionBase groupDefinitionBase)
	{
		_groupDefinitionBase = groupDefinitionBase;
	}

	public string Name
	{
		get => _groupDefinitionBase.Name;
		set
		{
			_groupDefinitionBase.Name = value;
			OnPropertyChanged();
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


	public TimeSpan Duratio => _groupDefinitionBase.Duration;

}

public static class ViewModelFactory
{
	public static GroupViewModelBase Create(GroupDefinitionBase groupDefinitionBase)=>groupDefinitionBase switch
	{
		ChildGroupDefinition childDef => new ChildGroupViewModel(childDef),
		ParentGroupDefinition parentDef => new ParentGroupViewModel(parentDef),
		_ => throw new NotImplementedException()
	};
}


public class ParentGroupViewModel : GroupViewModelBase
{
	public ParentGroupViewModel(ParentGroupDefinition parentGroupDefinition) : base(parentGroupDefinition)
	{
		Groups = new ObservableCollection<ChildGroupViewModel>(
			parentGroupDefinition.GroupDefinitions.Select(x => (ViewModelFactory.Create(x) as ChildGroupViewModel)!));
	}
	public ObservableCollection<ChildGroupViewModel> Groups { get; set; } = [];

}

public class ChildGroupViewModel : GroupViewModelBase
{
	public ChildGroupViewModel(ChildGroupDefinition childGroupDefinition) : base(childGroupDefinition)
	{
		Patterns = new(childGroupDefinition.Patterns);
	}
	public ObservableCollection<Pattern> Patterns { get; set; }
}

