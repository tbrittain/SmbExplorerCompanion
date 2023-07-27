using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

namespace SmbExplorerCompanion.WPF.Mappings.Teams;

[Mapper]
public partial class HistoricalTeamMapping
{
    public partial HistoricalTeam FromDto(HistoricalTeamDto historicalTeamDto);
}