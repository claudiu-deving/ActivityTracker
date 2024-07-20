using Client.Models;
using System.IO;
using System.Text.Json;

namespace Client.Services;

public class ActivityService(IAppPathsProvider appPathsProvider) : IActivityService
{
	private readonly List<Activity> _activities = [];
	private readonly List<string> _files = [];
	private string _currentFile = string.Empty;

	private ServiceResponse<List<Activity>> LoadActivities()
	{
		try
		{
			var directory = appPathsProvider.GetAppFolder();
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			var filteredJsonFiles = Directory.GetFiles(directory, "window_times_filtered_*.json");
			foreach (var file in filteredJsonFiles)
			{
				_files.Add(file);
			}

			var currentActivityFile = _files.FirstOrDefault();

			_currentFile = currentActivityFile?? Path.Combine(directory, $"window_times_filtered_{DateTime.Now:dd.MM.yy}.json");

			return ReadCurrentActivityFile(_currentFile);
		}
		catch (Exception ex)
		{
			return ServiceResponse<List<Activity>>.Fail(ex.Message);
		}
	}

	private ServiceResponse<List<Activity>> ReadCurrentActivityFile(string file)
	{
		try
		{
			_activities.Clear();
			if (!File.Exists(file))
			{
				File.WriteAllText(file, JsonSerializer.Serialize(new Dictionary<string, TimeSpan>()));
			}
			var jsonText = File.ReadAllText(file);

			var dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, TimeSpan>>(jsonText);

			if (dictionary is null)
			{
				return ServiceResponse<List<Activity>>.Fail($"No activities found in the file {file}");
			}

			foreach (var kvp in dictionary)
			{
				_activities.Add(new Activity { Name = kvp.Key, Duration = kvp.Value });
			}
			return ServiceResponse<List<Activity>>.Success(_activities);
		}
		catch (Exception ex)
		{
			return ServiceResponse<List<Activity>>.Fail(ex.Message);
		}
	}

	public ServiceResponse<List<Activity>> GetActivities()
	{
		try
		{
			if (_activities.Count > 0)
			{
				return ServiceResponse<List<Activity>>.Success(_activities);
			}
			else
			{
				return LoadActivities();
			}
		}
		catch (Exception ex)
		{
			return ServiceResponse<List<Activity>>.Fail(ex.Message);
		}
	}

	public void SetCurrentActivityFile(string file)
	{
		_currentFile = file;

		ReadCurrentActivityFile(_currentFile);
	}
}