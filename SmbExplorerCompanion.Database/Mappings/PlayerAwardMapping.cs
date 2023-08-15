using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Mappings;

[Mapper]
public partial class PlayerAwardMapping
{
    public partial PlayerAwardDto PlayerAwardToPlayerAwardDto(PlayerAward playerAward);
    public partial PlayerAward PlayerAwardDtoToPlayerAward(PlayerAwardDto playerAwardDto);
}