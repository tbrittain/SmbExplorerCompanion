using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Lookups;

public record PitcherRole : LookupBase
{
}

public static class PitcherRoleExtensions
{
    public static PitcherRole FromCore(this PitcherRoleDto pitcherRoleDto)
    {
        return new PitcherRole
        {
            Id = pitcherRoleDto.Id,
            Name = pitcherRoleDto.Name
        };
    }
}