namespace SmbExplorerCompanion.WPF.Models.Lookups;

public abstract record LookupBase
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
}