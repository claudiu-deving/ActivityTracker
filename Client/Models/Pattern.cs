using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Client.Models;

public class Pattern
{
    [JsonConstructor]
    public Pattern() { }

    public Pattern(string sentence)
    {
        Sentence = sentence;
    }

    public string Sentence { get; set; } = string.Empty;

    public ObservableCollection<Activity> Activities { get; set; } = [];
}
