using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Players;
using SmbExplorerCompanion.Core.ValueObjects.Seasons;

namespace SmbExplorerCompanion.Core.Interfaces;

public record GetPitchingSeasonsFilters : PlayerFilters
{
    public bool IsPlayoffs { get; init; } = false;
    public bool OnlyRookies { get; init; } = false;
    public bool IncludeChampionAwards { get; init; } = true;
    public bool OnlyUserAssignableAwards { get; init; } = false;
    public int? PitcherRoleId { get; init; }
    public int? TeamId { get; init; }
}

public interface IPitcherSeasonRepository
{
    public Task<List<PlayerPitchingSeasonDto>> GetPitchingSeasons(
        GetPitchingSeasonsFilters filters,
        CancellationToken cancellationToken = default);
}