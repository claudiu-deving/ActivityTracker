namespace Client.Models;

public class Activity
{
	public Activity(string name,  Dictionary<DateTime, TimeSpan> durations)
	{
		Name = name;
		TotalDuration = TimeSpan.FromMilliseconds( durations.Values.Select(x=>x.TotalMilliseconds).Sum());
		Durations = durations;
	}

	public string Name { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }

    public Dictionary<DateTime, TimeSpan> Durations { get; set; }
}