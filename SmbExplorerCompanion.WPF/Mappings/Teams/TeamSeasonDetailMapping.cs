using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

namespace SmbExplorerCompanion.WPF.Mappings.Teams;

[Mapper]
public partial class TeamSeasonDetailMapping
{
    public partial TeamPlayoffRoundResult FromPlayoffRoundDto(TeamPlayoffRoundResultDto playoffRoundResultDto);
    public partial TeamSeasonDetail FromTeamSeasonDetailDto(TeamSeasonDetailDto teamSeasonDetailDto);
}