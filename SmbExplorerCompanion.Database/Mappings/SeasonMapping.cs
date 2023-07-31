using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Database.Entities;

namespace SmbExplorerCompanion.Database.Mappings;

[Mapper]
public partial class SeasonMapping
{
    public partial SeasonDto SeasonToSeasonDto(Season season);
}