using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Mappings.Lookups;

[Mapper]
public partial class PlayerAwardMapping
{
    public partial PlayerAward FromDto(PlayerAwardDto awardDto);
}