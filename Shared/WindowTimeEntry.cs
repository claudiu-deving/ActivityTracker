namespace Shared
{
	public class WindowTimeEntry(string WindowName, Dictionary<DateTime, TimeSpan> Entries)
	{
		public string WindowName { get; } = WindowName;
		public Dictionary<DateTime, TimeSpan> Entries { get; } = Entries;
	}
}
