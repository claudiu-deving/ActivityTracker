using System.Runtime.InteropServices;
using System.Timers;
using System.Diagnostics.Eventing.Reader;
namespace ActivityTracker;

public class IdleTimer
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    [Flags]
    public enum ExecutionState : uint
    {
        ES_SYSTEM_REQUIRED = 0x00000001,
        ES_CONTINUOUS = 0x80000000
    }

    private DateTime _lastActivityTime;

    public void ResetTimer()
    {
        _lastActivityTime = DateTime.Now;
        SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_CONTINUOUS);
    }

    public TimeSpan GetIdleTime()
    {
        return CalculateSleepTime(GetSleepWakeEvents(DateTime.Today));
    }

    public void StopTimer()
    {
        SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
    }

    static EventRecord[] GetSleepWakeEvents(DateTime date)
    {
        DateTime startOfDay = date.Date;
        DateTime endOfDay = DateTime.Now;

        string query = $@"
            *[System[Provider[@Name='Microsoft-Windows-Power-Troubleshooter'] and (EventID=42 or EventID=1)]
            and TimeCreated[@SystemTime>='{startOfDay:o}' and @SystemTime<='{endOfDay:o}']]";
        var eventList = new List<EventRecord>();
        using var reader = new EventLogReader(new EventLogQuery("System", PathType.LogName, query));
        EventRecord eventInstance;
        while ((eventInstance = reader.ReadEvent()) != null)
        {
            eventList.Add(eventInstance);
        }
        return eventList.ToArray();
    }

    static TimeSpan CalculateSleepTime(EventRecord[] events)
    {
        TimeSpan totalSleepTime = TimeSpan.Zero;
        DateTime? sleepStart = null;

        foreach (var ev in events)
        {
            if (ev.Id == 42) // Sleep event
            {
                sleepStart = ev.TimeCreated;
            }
            else if (ev.Id == 1 && sleepStart.HasValue) // Wake event
            {
                TimeSpan sleepDuration = ev.TimeCreated.GetValueOrDefault() - sleepStart.Value;
                totalSleepTime += sleepDuration;
                sleepStart = null;
            }
        }

        // If the last event is a sleep event, calculate duration until now
        if (sleepStart.HasValue)
        {
            totalSleepTime += DateTime.Now - sleepStart.Value;
        }

        return totalSleepTime + TimeSpan.FromMinutes(events.Length * 5);
    }
}
