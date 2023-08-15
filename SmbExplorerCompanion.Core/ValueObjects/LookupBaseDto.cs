namespace SmbExplorerCompanion.Core.ValueObjects;

public abstract class LookupBaseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}