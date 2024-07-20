namespace ActivityTracker;

public class IdleTimeEventArgs(uint idleTime) : EventArgs
{
    public uint IdleTime { get; set; } = idleTime;
}
