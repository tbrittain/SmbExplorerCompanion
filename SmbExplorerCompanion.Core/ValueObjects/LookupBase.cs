namespace SmbExplorerCompanion.Core.ValueObjects;

public abstract class LookupBase
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}