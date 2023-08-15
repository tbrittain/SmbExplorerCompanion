using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Mappings.Players;

[Mapper]
public partial class PlayerFieldingRankingMapping
{
    public partial PlayerFieldingRanking FromDto(PlayerFieldingRankingDto rankingDto);
}