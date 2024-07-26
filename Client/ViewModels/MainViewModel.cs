using Client.Models;
using Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;

using SkiaSharp;

using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;

using Xceed.Wpf.Toolkit;

namespace Client.ViewModels;

public partial class MainViewModel : ObservableObject, IInitializable
{
	private readonly IGroupDefinitionService _GroupDefinitionService;
	private readonly IActivityService _activityService;
	private readonly Dictionary<Color, SKColor> _colorsMapping = [];

	[ObservableProperty]
	private ObservableCollection<Activity> _activities;

	[ObservableProperty]
	private ObservableCollection<GroupDefinition> _GroupDefinitions;

	private ColorItem _colorItem;

	private string _currentActivity = string.Empty;

	public string CurrentActivity
	{
		get => _currentActivity;
		set
		{
			if (!_currentActivity.Equals(value))
			{
				_currentActivity = value;
				OnPropertyChanged();
				_activityService.SetCurrentActivity(value);
				_GroupDefinitionService.RegroupActivities();
				UpdateGroupDefinitions();
			}
		}
	}

	[ObservableProperty]
	private string _newPatternInput;

	[ObservableProperty]
	private ObservableCollection<Activity> _remainingActivities;

	[ObservableProperty]
	private GroupDefinition _selectedGroup;

	public MainViewModel(IGroupDefinitionService GroupDefinitionService, IActivityService activityService)
	{
		_GroupDefinitionService = GroupDefinitionService;
		_activityService = activityService;
		Activities = [];
		_remainingActivities = [];
		GroupDefinitions = [];
		GroupDefinitions.CollectionChanged += GroupDefinitions_CollectionChanged;
		var colors = typeof(SKColors)
				 .GetFields(BindingFlags.Static | BindingFlags.Public)
				 .Select(fld =>
				 {
					 var skColor = (SKColor)fld.GetValue(null);
					 var colorItem = new ColorItem(new Color() { R = skColor.Red, G = skColor.Green, B = skColor.Blue, A = skColor.Alpha }, fld.Name);
					 _colorsMapping[colorItem.Color ?? Colors.Black] = skColor;
					 return colorItem;
				 })
				 .ToList();
		AvailableColors = new(colors);
		AddPatternCommand = new LocalRelayCommand(AddPattern, CanAddPattern);
	}

	[ObservableProperty]
	private ObservableCollection<string> _activityFiles;

	public ObservableCollection<ColorItem> AvailableColors { get; }
	public LocalRelayCommand AddPatternCommand { get; }

	public ColorItem ColorItem
	{
		get => _colorItem;
		set
		{
			_colorItem = value;
			OnPropertyChanged();
			SelectedGroup.SKColor = _colorsMapping[value.Color ?? Colors.Black];
			RedrawGraph();
		}
	}

	public ObservableCollection<ISeries> Series { get; set; } = [];

	public LabelVisual Title { get; set; } =
	new LabelVisual
	{
		Text = "My chart title",
		TextSize = 25,
		Padding = new LiveChartsCore.Drawing.Padding(15),
		Paint = new SolidColorPaint(SKColors.DarkSlateGray)
	};

	public async Task<ServiceResponse<bool>> Initialize()
	{
		return await Task.Run(async () =>
		{
			var serviceInitializationResponse = await ((IInitializable)_GroupDefinitionService).Initialize();
			if (!serviceInitializationResponse.IsSuccess)
			{
				return ServiceResponse<bool>.Fail(serviceInitializationResponse.Message);
			}
			_GroupDefinitionService.RegroupActivities();
			var getActivitiesResponse = _activityService.GetActivities();
			if (!getActivitiesResponse.IsSuccess || getActivitiesResponse.Data is null)
			{
				return ServiceResponse<bool>.Fail(getActivitiesResponse.Message);
			}
			Activities = new(getActivitiesResponse.Data);
			UpdateGroupDefinitions();
			InitializeActivityFiles();
			RedrawGraph();
			return ServiceResponse<bool>.Success(true);
		});
	}

	private void InitializeActivityFiles()
	{
		var filesRetrievalResponse = _activityService.GetActivityNames();
		if (!filesRetrievalResponse.IsSuccess || filesRetrievalResponse.Data is null)
		{
			MessageBox.Show(filesRetrievalResponse.Message);
		}
		ActivityFiles = new(filesRetrievalResponse.Data!);
	}

	private void GroupDefinitions_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		RedrawGraph();
	}

	[RelayCommand]
	private void AddGroup()
	{
		var newGroup = new GroupDefinition { Name = "New Group" };
		_GroupDefinitionService.AddGroup(newGroup);
		SelectedGroup = newGroup;
		Refresh();
	}

	private void AddPattern()
	{
		SelectedGroup.Patterns.Add(new Pattern(NewPatternInput));
		NewPatternInput = string.Empty;
		Refresh();
	}

	private bool CanAddPattern() => SelectedGroup != null && !string.IsNullOrWhiteSpace(NewPatternInput);

	private void RedrawGraph()
	{
		Series.Clear();
		foreach (var group in GroupDefinitions)
		{
			if (group.TotalDuration == TimeSpan.Zero) continue;
			var series = new PieSeries<double>()
			{
				Values = [Math.Round(Math.Floor(group.TotalDuration.TotalMinutes / 60) + (group.TotalDuration.TotalMinutes % 60 / 100.0), 2)],
				Name = group.Name,
				DataLabelsPaint = new SolidColorPaint(SKColors.Black),
				DataLabelsSize = 18,
				DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
				DataLabelsFormatter = point => $"{group.Name}: {point.Coordinate.PrimaryValue} ",
				Fill = new SolidColorPaint(group.SKColor)
			};
			Series.Add(series);
		}
	}

	[RelayCommand]
	private void RemoveGroup()
	{
		if (SelectedGroup != null)
		{
			if (!_GroupDefinitionService.RemoveGroup(SelectedGroup.Id).IsSuccess)
			{
				//TODO: Present error message
			}
			SelectedGroup = null;
			Refresh();
		}
	}

	private void Refresh()
	{
		_GroupDefinitionService.RegroupActivities();
		UpdateGroupDefinitions();
		RedrawGraph();
	}

	[RelayCommand]
	private void RemovePattern(string pattern)
	{
		if (SelectedGroup != null)
		{
			var patternToRemove = SelectedGroup.Patterns.FirstOrDefault(x => x.Sentence.Equals(pattern));
			if (patternToRemove != null)
			{
				_GroupDefinitionService.RemovePattern(SelectedGroup.Id, patternToRemove.Sentence);
			}
			Refresh();
		}
	}

	[RelayCommand]
	private void RenameGroup()
	{
		if (SelectedGroup != null)
		{
			var patternToRemove = GroupDefinitions.FirstOrDefault(x => x.Id.Equals(SelectedGroup.Id));
			if (patternToRemove != null)
			{
				patternToRemove.Name = NewPatternInput;
			}
			Refresh();
		}
	}

	[RelayCommand]
	private void SaveGroupsToFile()
	{
		if (!_GroupDefinitionService.Save().IsSuccess)
		{
			//TODO: Present error message
		}
	}

	private bool UpdateGroupDefinitions()
	{
		var response = _GroupDefinitionService.GetGroupDefinitions();
		RemainingActivities = new(_GroupDefinitionService.GetRemainingActivities());
		if (response.IsSuccess && response.Data is not null)
		{
			GroupDefinitions = new(response.Data);
			return true;
		}
		else
		{
			//TODO: Present error message
			return false;
		}
	}
}