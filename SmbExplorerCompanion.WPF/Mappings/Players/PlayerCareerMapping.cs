using Riok.Mapperly.Abstractions;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Mappings.Players;

[Mapper]
public partial class PlayerCareerMapping
{
    public partial PlayerBattingCareer FromBattingDto(PlayerCareerBattingDto careerDto);
    public partial PlayerPitchingCareer FromPitchingDto(PlayerCareerPitchingDto careerDto);
}