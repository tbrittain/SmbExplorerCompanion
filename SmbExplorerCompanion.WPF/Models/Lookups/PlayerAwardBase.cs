using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record PlayerAwardBase : LookupBase
{
    public int Importance { get; init; }
    public bool OmitFromGroupings { get; init; }

    public virtual bool Equals(PlayerAwardBase? other)
    {
        // Only compare the Id property since that's the only property that's guaranteed to be unique
        return other is not null && Id == other.Id;
    }

    override public string ToString()
    {
        return Name;
    }

    override public int GetHashCode()
    {
        return Id;
    }
}

public static class PlayerAwardBaseExtensions
{
    public static PlayerAwardBase FromCore(this PlayerAwardBaseDto playerAwardBaseDto)
    {
        return new PlayerAwardBase
        {
            Id = playerAwardBaseDto.Id,
            Name = playerAwardBaseDto.Name,
            Importance = playerAwardBaseDto.Importance,
            OmitFromGroupings = playerAwardBaseDto.OmitFromGroupings
        };
    }
}