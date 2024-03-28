using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.ValueObjects.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public record GetPitchingSeasonsFilters : SeasonPlayerFilters
{
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