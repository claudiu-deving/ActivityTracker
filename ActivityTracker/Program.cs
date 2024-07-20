using Newtonsoft.Json;

using System;
using System.Runtime.InteropServices;
using System.Text;
namespace ActivityTracker;

[StructLayout(LayoutKind.Sequential)]
struct LASTINPUTINFO
{
    public uint cbSize;
    public uint dwTime;
}

public delegate void IdleTimeElapsedEventHandler(IdleTimeEventArgs e);

class Program
{
    private static string _lastWindow;
    private static DateTime _lastTime;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    static async Task Main()
    {
        _idleTimer = new IdleTimer();
        RecordActiveWindow();
    }
    public const string IDLE_TIMER = "IDLE_TIMER";
    private static void IdleTimer_IdleTimeElapsed(IdleTimeEventArgs e)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(e.IdleTime);
        if (_windowTimes.ContainsKey(IDLE_TIMER))
        {
            _windowTimes[IDLE_TIMER] += timeSpan;
        }
        else
        {
            _windowTimes[IDLE_TIMER] = timeSpan;
        }
    }

    private static void RecordActiveWindow()
    {
        while (true)
        {
            IntPtr handle = GetForegroundWindow();
            int length = GetWindowTextLength(handle);
            StringBuilder windowTitle = new StringBuilder(length + 1);
            GetWindowText(handle, windowTitle, windowTitle.Capacity);

            string currentWindow = windowTitle.ToString();
            ProcessWindowInfo(currentWindow);

            if (_windowTimes.ContainsKey(IDLE_TIMER))
            {
                _windowTimes[IDLE_TIMER] += _idleTimer.GetIdleTime();
            }
            else
            {
                _windowTimes[IDLE_TIMER] = _idleTimer.GetIdleTime();
            }
            System.Threading.Thread.Sleep(1000); // Check every second
        }
    }

    private static void ProcessWindowInfo(string currentWindow)
    {
        DateTime currentTime = DateTime.Now;

        if (currentWindow != _lastWindow)
        {
            if (!string.IsNullOrEmpty(_lastWindow))
            {
                TimeSpan timeSpent = currentTime - _lastTime;
                if (_windowTimes.ContainsKey(_lastWindow))
                {
                    _windowTimes[_lastWindow] += timeSpent;
                }
                else
                {
                    _windowTimes[_lastWindow] = timeSpent;
                }
            }

            _lastWindow = currentWindow;
            _lastTime = currentTime;
        }

        SaveWindowTimes();
    }
    private static Dictionary<string, TimeSpan> _windowTimes = [];
    private static IdleTimer _idleTimer;

    private static void SaveWindowTimes()
    {
        try
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string serviceDataPath = Path.Combine(localAppDataPath, "MyServiceData");

            if (!Directory.Exists(serviceDataPath))
            {
                Directory.CreateDirectory(serviceDataPath);
            }

            string json = JsonConvert.SerializeObject(_windowTimes, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(serviceDataPath, $"window_times_{DateTime.Now:dd.MM.yy}.json"), json);
            string jsonFiltered = JsonConvert.SerializeObject(_windowTimes.Where(x => x.Value > TimeSpan.FromSeconds(120)).ToDictionary(x => x.Key, x => x.Value), Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(Path.Combine(serviceDataPath, $"window_times_filtered_{DateTime.Now:dd.MM.yy}.json"), jsonFiltered);
        }
        catch (Exception ex)
        {
            FileLogger.Log(ex.Message);
        }
    }

    private class WindowTimeEntry
    {
        public string WindowName { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
