namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record PlayerAward : PlayerAwardBase
{
    public string OriginalName { get; init; } = default!;
    public bool IsBuiltIn { get; init; }
    public bool IsBattingAward { get; init; }
    public bool IsPitchingAward { get; init; }
    public bool IsFieldingAward { get; init; }
    public bool IsPlayoffAward { get; init; }
}