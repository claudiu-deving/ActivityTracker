using SkiaSharp;

namespace Client.Models;

public class GroupDefinitionBase
{
	public string Name { get; set; } = string.Empty;
	public SKColor SKColor { get; set; }
	public int Id { get; set; }
	public TimeSpan Duration { get; set; }
}
public class ChildGroupDefinition:GroupDefinitionBase
{
	public List<Pattern> Patterns { get; set; } = [];
}

public class ParentGroupDefinition : GroupDefinitionBase
{
	public List<GroupDefinitionBase> GroupDefinitions { get; set; } = [];
}