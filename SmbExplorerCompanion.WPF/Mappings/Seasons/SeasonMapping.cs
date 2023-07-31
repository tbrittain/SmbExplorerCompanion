using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.Mappings.Seasons;

[Mapper]
public partial class SeasonMapping
{
    public partial Season FromDto(SeasonDto seasonDto);
}