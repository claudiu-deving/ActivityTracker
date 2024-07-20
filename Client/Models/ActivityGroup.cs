using LiveChartsCore.SkiaSharpView;

namespace Client.Models;

public class ActivityGroup : ActivityGroupBase
{
    public ActivityGroup()
    {
        Patterns = [];
        Patterns.CollectionChanged += Patterns_CollectionChanged;
    }
    private bool _onlyOnce;
    private void Patterns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_onlyOnce) return;
        if (Patterns.Any())
        {
            Name = Patterns.First().Sentence;
        }
        _onlyOnce = true;
    }


    private TimeSpan _totalDuration;

    public TimeSpan TotalDuration
    {
        get => _totalDuration;
        set
        {
            _totalDuration = value;
            OnPropertyChanged();
        }
    }
    public List<Activity> Activities { get; set; } = [];
}
