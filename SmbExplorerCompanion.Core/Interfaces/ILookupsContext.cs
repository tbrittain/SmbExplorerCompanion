using SmbExplorerCompanion.Core.Entities.Lookups;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface ILookupsContext
{
    Task<IReadOnlyList<PitcherRoleDto>> GetPitcherRoles();
    Task<IReadOnlyList<ChemistryDto>> GetChemistryTypes();
    Task<IReadOnlyList<PositionDto>> GetPositions();
    Task<IReadOnlyList<BatHandednessDto>> GetBatHandednessTypes();
    Task<IReadOnlyList<ThrowHandednessDto>> GetThrowHandednessTypes();
}