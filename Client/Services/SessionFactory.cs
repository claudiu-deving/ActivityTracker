using Client.Models;
using System.IO;
using System.Text.Json;

namespace Client.Services;

public static class SessionFactory
{
	public static Session Create(string sessionFile, int id)
	{
		if (string.IsNullOrEmpty(sessionFile) || !File.Exists(sessionFile))
		{
			throw new ArgumentException("The provided file path is not valid");
		}

		var activities = ReadCurrentActivityFile(sessionFile);
		return new Session(sessionFile, id, activities);
	}

	private static List<Activity> ReadCurrentActivityFile(string session)
	{
		var activities = new List<Activity>();
		if (session == null) return activities;
		if (!File.Exists(session))
		{
			File.WriteAllText(session, JsonSerializer.Serialize(new Dictionary<string, TimeSpan>()));
		}
		var jsonText = File.ReadAllText(session);

		var dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, TimeSpan>>(jsonText);

		if (dictionary is null)
		{
			return activities;
		}

		foreach (var kvp in dictionary)
		{
			activities.Add(new Activity { Name = kvp.Key, Duration = kvp.Value });
		}
		return activities;
	}
}
