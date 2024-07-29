using Client.Models;
using Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Shared;
using SkiaSharp;

using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media;

using Xceed.Wpf.Toolkit;

namespace Client.ViewModels;

public partial class MainViewModel : ObservableObject, IInitializable
{
	private readonly IGroupDefinitionService _GroupDefinitionService;
	private readonly IActivityService _activityService;
	private readonly Dictionary<Color, SKColor> _colorsMapping = [];
	private readonly ActivityRepository _repository;
	[ObservableProperty]
	private ObservableCollection<Session> _sessions;

	[ObservableProperty]
	private ObservableCollection<GroupViewModel> _groups = [];

	private ColorItem _colorItem;

	private Session? _currentSession;

	public Session? CurrentSession
	{
		get => _currentSession;
		set
		{
			if (_currentSession is not null && !_currentSession.Equals(value))
			{
				_currentSession = value;
				UpdateSessionGraph();
			}
			else
			{
				_currentSession = value;
			}
			OnPropertyChanged();
		}
	}

	[ObservableProperty]
	private string _newPatternInput;

	[ObservableProperty]
	private ObservableCollection<Activity> _remainingActivities;

	[ObservableProperty]
	private GroupViewModel _selectedGroup;

	[ObservableProperty]
	private ObservableCollection<ActivityViewModel> _activities=[];

	private readonly Random _random = new();
	private SolidColorBrush GetRandomColor()
	{
		return new SolidColorBrush(Color.FromRgb(
		(byte)_random.Next(256),
		(byte)_random.Next(256),
		(byte)_random.Next(256)));
	}
	[ObservableProperty]
	private DateTime _firstDate;
	[ObservableProperty]
	private DateTime _lastDate;

	[ObservableProperty]
	private double _lastDateWidth;

	[ObservableProperty]
	private double _totalDuration;

	public MainViewModel(ActivityRepository repository, ISessionService sessionService)
	{
		_repository = repository;
		Sessions = new(sessionService.GetSessions());
		CurrentSession = Sessions.FirstOrDefault(x => x.Created.Day.Equals(DateTime.UtcNow.AddDays(0).Day));
		UpdateSessionGraph();
		//UpdateGroups();
		//var colors = typeof(SKColors)
		//		 .GetFields(BindingFlags.Static | BindingFlags.Public)
		//		 .Select(fld =>
		//		 {
		//			 var skColor = (SKColor)fld.GetValue(null);
		//			 var colorItem = new ColorItem(new Color() { R = skColor.Red, G = skColor.Green, B = skColor.Blue, A = skColor.Alpha }, fld.Name);
		//			 _colorsMapping[colorItem.Color ?? Colors.Black] = skColor;
		//			 return colorItem;
		//		 })
		//		 .ToList();
		//AvailableColors = new(colors);
		//AddPatternCommand = new LocalRelayCommand(AddPattern, CanAddPattern);
	}

	private void UpdateSessionGraph()
	{
		var activities = new List<ActivityViewModel>();
		if (CurrentSession is not null)
		{
			TotalDuration = CurrentSession.Activities.Sum(x => x.TotalDuration.TotalSeconds);

			LastDateWidth = CurrentSession.Activities.Last().Durations.Last().Value.TotalSeconds / TotalDuration * 100;

			FirstDate = CurrentSession.Activities.Min(x => x.Durations.Min(x => x.Key)).AddSeconds(-CurrentSession.Activities.First().Durations.First().Value.TotalSeconds);
			LastDate = CurrentSession.Activities.Max(x => x.Durations.Max(x => x.Key));
			IdleDuration = (LastDate - FirstDate).TotalSeconds + CurrentSession.Activities.Last().Durations.Last().Value.TotalSeconds - TotalDuration;
			foreach (var activity in CurrentSession.Activities)
			{
				var color = GetRandomColor();

				foreach (var entry in activity.Durations)
				{
					var width = entry.Value.TotalSeconds / (TotalDuration - IdleDuration) * 100;
					activities.Add(new ActivityViewModel(activity.Name, entry.Key, entry.Value, color, width));
				}
			}

		}
		Activities = new ObservableCollection<ActivityViewModel>(activities.OrderBy(x => x.CreatedDate));
	}

	private void UpdateGroups()
	{
		Groups.Clear();

		if (CurrentSession is not null)
		{
			var groups = _repository.GetGroups(CurrentSession.Id);
			foreach (var group in groups)
			{
				Groups.Add(group);
			}
		}
	}

	[ObservableProperty]
	private ObservableCollection<string> _activityFiles;
	[ObservableProperty]
	private double _idleDuration;

	public ObservableCollection<ColorItem> AvailableColors { get; }
	public LocalRelayCommand AddPatternCommand { get; }

	public ColorItem ColorItem
	{
		get => _colorItem;
		set
		{
			_colorItem = value;
			OnPropertyChanged();
			SelectedGroup.Color = _colorsMapping[value.Color ?? Colors.Black];
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
			//TODO: Implement
			return ServiceResponse<bool>.Success(true);
		});
	}

	private void InitializeActivityFiles()
	{

	}

	[RelayCommand]
	private void AddGroup()
	{
		var newGroup = new GroupDefinition { Name = "New Group" };
		_GroupDefinitionService.AddGroup(newGroup);

		Refresh();
	}

	private void AddPattern()
	{
		NewPatternInput = string.Empty;
		Refresh();
	}

	private bool CanAddPattern() => SelectedGroup != null && !string.IsNullOrWhiteSpace(NewPatternInput);

	private void RedrawGraph()
	{
		Series.Clear();
		foreach (var group in Groups)
		{
			if (group.Duration == TimeSpan.Zero) continue;
			var series = new PieSeries<double>()
			{
				Values = [Math.Round(Math.Floor(group.Duration.TotalMinutes / 60) + (group.Duration.TotalMinutes % 60 / 100.0), 2)],
				Name = group.Name,
				DataLabelsPaint = new SolidColorPaint(SKColors.Black),
				DataLabelsSize = 18,
				DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
				DataLabelsFormatter = point => $"{group.Name}: {point.Coordinate.PrimaryValue} ",
				Fill = new SolidColorPaint(group.Color)
			};
			Series.Add(series);
		}
	}

	[RelayCommand]
	private void RemoveGroup()
	{
		if (SelectedGroup != null)
		{
			SelectedGroup = null;
			Refresh();
		}
	}

	private void Refresh()
	{
		UpdateGroups();
		RedrawGraph();
	}

	[RelayCommand]
	private void RemovePattern(string pattern)
	{
		if (SelectedGroup != null)
		{
			Refresh();
		}
	}

	[RelayCommand]
	private void RenameGroup()
	{
		if (SelectedGroup != null)
		{

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

}

public class ActivityViewModel(string name, DateTime createdDate, TimeSpan duration,SolidColorBrush solidColorBrush, double width)
{
	public string Name => name;
	public DateTime CreatedDate => createdDate;
	public TimeSpan Duration => duration;
	public SolidColorBrush Color => solidColorBrush;
	public double Width => width;

	public override string ToString()
	{
		return $"{CreatedDate:dd-hh:mm:ss} - {Duration}: {Name}";
	}
}