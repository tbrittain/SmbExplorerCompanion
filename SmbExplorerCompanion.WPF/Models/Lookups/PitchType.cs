using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record PitchType : LookupBase;

public static class PitchTypeExtensions
{
    public static PitchType FromCore(this PitchTypeDto pitchTypeDto)
    {
        return new PitchType
        {
            Id = pitchTypeDto.Id,
            Name = pitchTypeDto.Name
        };
    }
}