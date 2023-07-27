using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

namespace SmbExplorerCompanion.WPF.Mappings.Teams;

[Mapper]
public partial class TeamOverviewSeasonHistoryMapping
{
    public partial TeamOverviewSeasonHistory FromDto(TeamTopPlayerHistoryDto teamTopPlayerHistoryDto);
}