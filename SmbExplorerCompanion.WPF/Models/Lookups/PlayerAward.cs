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
}

public static class PlayerAwardExtensions
{
    public static PlayerAward FromCore(this PlayerAwardDto playerAwardDto)
    {
        return new PlayerAward
        {
            OriginalName = playerAwardDto.OriginalName,
            IsBuiltIn = playerAwardDto.IsBuiltIn,
            IsBattingAward = playerAwardDto.IsBattingAward,
            IsPitchingAward = playerAwardDto.IsPitchingAward,
            IsFieldingAward = playerAwardDto.IsFieldingAward,
            IsPlayoffAward = playerAwardDto.IsPlayoffAward
        };
    }
}