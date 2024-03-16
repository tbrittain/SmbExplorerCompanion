using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record Position : LookupBase
{
    public bool IsPrimaryPosition { get; set; }
}

public static class PositionExtensions
{
    public static Position FromCore(this PositionDto positionDto)
    {
        return new Position
        {
            Id = positionDto.Id,
            Name = positionDto.Name,
            IsPrimaryPosition = positionDto.IsPrimaryPosition
        };
    }
}