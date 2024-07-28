using SkiaSharp;

namespace Client.Models;

public class GroupDefinition
{
	public string Name { get; set; } = string.Empty;
	public string Pattern { get; set; } = string.Empty;	
	public SKColor SKColor { get; set; }
	public int Id { get; set; }
	public List<GroupDefinition> GroupDefinitions { get; set; } = [];
}