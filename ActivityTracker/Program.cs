using Newtonsoft.Json;
using Shared;
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
	private static WindowTimeEntry _lastEntry;
	private static DateTime _lastTime;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	static void Main()
	{
		_idleTimer = new IdleTimer();
		LoadWindowTimes();
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
			if (string.IsNullOrEmpty(currentWindow))
			{
				currentWindow = _lastWindow;
			}
			ProcessWindowInfo2(currentWindow);
			Console.WriteLine(currentWindow);
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

		if (Changed(currentWindow))
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
	private static void ProcessWindowInfo2(string currentWindow)
	{
		if (!Changed(currentWindow)) return;
		DateTime currentTime = DateTime.Now;
	
		if (FirstEntry())
		{
			_lastWindow = currentWindow;
			_lastTime = currentTime;
			return;
		}
		TimeSpan timeSpent = currentTime - _lastTime;
	
		if (_entries.FirstOrDefault(x=>x.WindowName.Equals(_lastWindow)) is WindowTimeEntry existingEntry)
		{
			if (existingEntry.Entries.ContainsKey(currentTime))
			{
				existingEntry.Entries[currentTime] += timeSpent;
			}
			else
			{
				existingEntry.Entries[currentTime] = timeSpent;
			}
		}
		else
		{
			_entries.Add(new WindowTimeEntry(_lastWindow, new Dictionary<DateTime, TimeSpan>()
			{
				[DateTime.Now] = timeSpent
			}));
		}
		_lastWindow = currentWindow;
		_lastTime = currentTime;
		SaveWindowTimes();

	}

	private static bool FirstEntry()
	{
		return string.IsNullOrEmpty(_lastWindow);
	}

	private static bool Changed(string currentWindow)
	{
		return currentWindow != _lastWindow;
	}

	private static Dictionary<string, TimeSpan> _windowTimes = [];
	private static HashSet<WindowTimeEntry> _entries = [];
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

			string json = JsonConvert.SerializeObject(_entries, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(Path.Combine(serviceDataPath, $"window_times_{DateTime.Now:dd.MM.yy}.json"), json);
	
		}
		catch (Exception ex)
		{
			FileLogger.Log(ex.Message);
		}
	}

	private static void LoadWindowTimes()
	{
		try
		{
			string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string serviceDataPath = Path.Combine(localAppDataPath, "MyServiceData");

			if (!Directory.Exists(serviceDataPath))
			{
				return;
			}

			var path = Path.Combine(serviceDataPath, $"window_times_{DateTime.Now:dd.MM.yy}.json");

			if (!File.Exists(path))
			{
				return;
			}
			string json = File.ReadAllText(path);

			_entries = JsonConvert.DeserializeObject<HashSet<WindowTimeEntry>>(json)??[];
		}
		catch (Exception ex)
		{
			FileLogger.Log(ex.Message);
		}
	}
}
