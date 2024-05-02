using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record PlayerAward : PlayerAwardBase
{
    public string OriginalName { get; init; } = default!;
    public bool IsBuiltIn { get; init; }
    public bool IsBattingAward { get; init; }
    public bool IsPitchingAward { get; init; }
    public bool IsFieldingAward { get; init; }
    public bool IsPlayoffAward { get; init; }
    public bool IsUserAssignable { get; init; }
}

public static class PlayerAwardExtensions
{
    public static PlayerAward FromCore(this PlayerAwardDto x)
    {
        return new PlayerAward
        {
            Id = x.Id,
            Name = x.Name,
            Importance = x.Importance,
            OmitFromGroupings = x.OmitFromGroupings,
            OriginalName = x.OriginalName,
            IsBuiltIn = x.IsBuiltIn,
            IsBattingAward = x.IsBattingAward,
            IsPitchingAward = x.IsPitchingAward,
            IsFieldingAward = x.IsFieldingAward,
            IsPlayoffAward = x.IsPlayoffAward,
            IsUserAssignable = x.IsUserAssignable
        };
    }
}