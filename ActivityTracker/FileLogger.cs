namespace ActivityTracker;

public static class FileLogger
{

    public static void Log(string message)
    {
        string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string serviceDataPath = Path.Combine(localAppDataPath, "MyServiceData");
        string logFilePath = Path.Combine(serviceDataPath, "service.log");
        if (!Directory.Exists(serviceDataPath))
        {
            Directory.CreateDirectory(serviceDataPath);
        }
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}