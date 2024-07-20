using SkiaSharp;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.Models;

public class ActivityGroupBase : INotifyPropertyChanged
{
    public ObservableCollection<Pattern> Patterns { get; set; } = [];
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();

        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public SKColor SKColor { get; set; }

    public int Id { get; set; }
}
