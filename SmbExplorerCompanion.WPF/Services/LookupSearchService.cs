using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Services;

public class LookupSearchService
{
    private readonly LookupCache _lookupCache;

    public LookupSearchService(LookupCache lookupCache)
    {
        _lookupCache = lookupCache;
    }
    
    public async Task<PitcherRole?> GetPitcherRoleById(int id)
    {
        var pitcherRoles = await _lookupCache.GetPitcherRoles();
        return pitcherRoles.FirstOrDefault(x => x.Id == id);
    }
    
    public async Task<Chemistry?> GetChemistryById(int id)
    {
        var chemistry = await _lookupCache.GetChemistryTypes();
        return chemistry.FirstOrDefault(x => x.Id == id);
    }

    public async Task<Position?> GetPositionById(int id)
    {
        var positions = await _lookupCache.GetPositions();
        return positions.FirstOrDefault(x => x.Id == id);
    }
    
    public async Task<ThrowHandedness?> GetThrowHandednessById(int id)
    {
        var throwHandedness = await _lookupCache.GetThrowHandednessTypes();
        return throwHandedness.FirstOrDefault(x => x.Id == id);
    }
    
    public async Task<BatHandedness?> GetBatHandednessById(int id)
    {
        var batHandedness = await _lookupCache.GetBatHandednessTypes();
        return batHandedness.FirstOrDefault(x => x.Id == id);
    }
}