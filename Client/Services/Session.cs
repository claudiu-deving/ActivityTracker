using Client.Models;
using System.IO;

namespace Client.Services;

/// <summary>
/// This represents the total of activities performed in a day.
/// </summary>
/// <param name="file"></param>
/// <param name="id"></param>
/// <param name="activities"></param>
public class Session(string file, int id, IEnumerable<Activity> activities)
{
	public string Name { get; }=Path.GetFileName(file);
	public string File { get; } = file;
	public DateTime Created { get; } = System.IO.File.GetCreationTimeUtc(file);
	public int Id { get; } = id;

	public List<Activity> Activities { get; } = activities.ToList();
	public TimeSpan TotalDuration { get; } = TimeSpan.FromSeconds(activities.Sum(x => x.TotalDuration.TotalSeconds));

	public static Session Empty => new(string.Empty, 0, []);
}