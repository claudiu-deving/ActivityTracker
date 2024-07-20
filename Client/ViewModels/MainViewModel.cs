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
	private readonly IActivityGroupService _activityGroupService;
	private readonly IActivityService _activityService;
	private readonly Dictionary<Color, SKColor> _colorsMapping = [];

	[ObservableProperty]
	private ObservableCollection<Activity> _activities;

	[ObservableProperty]
	private ObservableCollection<ActivityGroup> _activityGroups;

	private ColorItem _colorItem;

	private string _currentActivityFile=string.Empty;
	public string CurrentActivityFile
	{
		get => _currentActivityFile;
		set
		{
			if (!_currentActivityFile.Equals(value))
			{
				_currentActivityFile = value;
				OnPropertyChanged();
				_activityService.SetCurrentActivityFile(value);
				_activityGroupService.RegroupActivities();
				UpdateActivityGroups();
			}
		}
	}

	[ObservableProperty]
	private string _newPatternInput;

	[ObservableProperty]
	private ObservableCollection<Activity> _remainingActivities;

	[ObservableProperty]
	private ActivityGroup _selectedGroup;
	public MainViewModel(IActivityGroupService activityGroupService,IActivityService activityService)
	{
		_activityGroupService = activityGroupService;
		_activityService = activityService;
		Activities = [];
		_remainingActivities = [];
		ActivityGroups = [];
		ActivityGroups.CollectionChanged += ActivityGroups_CollectionChanged;
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

	}

	[ObservableProperty]
	private ObservableCollection<string> _activityFiles;
	public ObservableCollection<ColorItem> AvailableColors { get; }
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
		return await Task.Run(async () => {
			var serviceInitializationResponse = await ((IInitializable)_activityGroupService).Initialize();
			if (!serviceInitializationResponse.IsSuccess)
			{
				return ServiceResponse<bool>.Fail(serviceInitializationResponse.Message);
			}
			_activityGroupService.RegroupActivities();
			 var getActivitiesResponse =_activityService.GetActivities();
			if (!getActivitiesResponse.IsSuccess || getActivitiesResponse.Data is null)
			{
				return ServiceResponse<bool>.Fail(getActivitiesResponse.Message);
			}
			Activities = new(getActivitiesResponse.Data);
			UpdateActivityGroups();
			InitializeActivityFiles();
			RedrawGraph();
			return ServiceResponse<bool>.Success(true);
		});
	}

	private void InitializeActivityFiles()
	{
		var filesRetrievalResponse = _activityService.GetActivityFiles();
		if(!filesRetrievalResponse.IsSuccess || filesRetrievalResponse.Data is null)
		{
			MessageBox.Show(filesRetrievalResponse.Message);
		}
		ActivityFiles = new(filesRetrievalResponse.Data!);
	}

	private void ActivityGroups_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		RedrawGraph();
	}

	[RelayCommand]
	private void AddGroup()
	{
		var newGroup = new ActivityGroup { Name = "New Group" };
		_activityGroupService.AddGroup(newGroup);
		SelectedGroup = newGroup;
		UpdateActivityGroups();
	}

	[RelayCommand]
	private void AddPattern()
	{
		if (SelectedGroup != null && !string.IsNullOrWhiteSpace(NewPatternInput))
		{
			SelectedGroup.Patterns.Add(new Pattern(NewPatternInput));
			NewPatternInput = string.Empty;
			_activityGroupService.RegroupActivities();
		}
	}
	private void RedrawGraph()
	{
		Series.Clear();
		foreach (var group in ActivityGroups)
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
			if (!_activityGroupService.RemoveGroup(SelectedGroup.Id).IsSuccess)
			{
				//TODO: Present error message
			}
			SelectedGroup = null;
			_activityGroupService.RegroupActivities();
			UpdateActivityGroups();
		}
	}

	[RelayCommand]
	private void RemovePattern(string pattern)
	{
		if (SelectedGroup != null)
		{
			var patternToRemove = SelectedGroup.Patterns.FirstOrDefault(x => x.Sentence.Equals(pattern));
			if (patternToRemove != null)
			{
				SelectedGroup.Patterns.Remove(patternToRemove);
			}
			_activityGroupService.RegroupActivities();
		}
	}

	[RelayCommand]
	private void RenameGroup()
	{
		if (SelectedGroup != null)
		{
			SelectedGroup.Name = NewPatternInput;
			NewPatternInput = string.Empty;
			_activityGroupService.RegroupActivities();
		}
	}

	[RelayCommand]
	private void SaveGroupsToFile()
	{
		if (!_activityGroupService.Save().IsSuccess)
		{
			//TODO: Present error message
		}
	}

	private bool UpdateActivityGroups()
	{
		var response = _activityGroupService.GetActivityGroups();
		RemainingActivities = new ( _activityGroupService.GetRemainingActivities());
		if (response.IsSuccess && response.Data is not null)
		{
			ActivityGroups = new(response.Data);
			return true;
		}
		else
		{
			//TODO: Present error message
			return false;
		}
	}

}
