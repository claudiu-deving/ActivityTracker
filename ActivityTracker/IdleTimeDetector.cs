using System.Runtime.InteropServices;
namespace ActivityTracker;

class IdleTimeDetector
{
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public static uint GetIdleTime()
    {
        LASTINPUTINFO lastInPut = new LASTINPUTINFO();
        lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
        if (!GetLastInputInfo(ref lastInPut))
        {
            throw new Exception("GetLastInputInfo failed");
        }

        return ((uint)Environment.TickCount - lastInPut.dwTime);
    }
}
