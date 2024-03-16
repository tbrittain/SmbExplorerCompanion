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

    public async Task<PitcherRole> GetPitcherRoleById(int id)
    {
        var pitcherRoles = await _lookupCache.GetPitcherRoles();
        return pitcherRoles.Single(x => x.Id == id);
    }

    public async Task<Chemistry> GetChemistryById(int id)
    {
        var chemistry = await _lookupCache.GetChemistryTypes();
        return chemistry.Single(x => x.Id == id);
    }

    public async Task<Position> GetPositionById(int id)
    {
        var positions = await _lookupCache.GetPositions();
        return positions.Single(x => x.Id == id);
    }

    public async Task<ThrowHandedness> GetThrowHandednessById(int id)
    {
        var throwHandedness = await _lookupCache.GetThrowHandednessTypes();
        return throwHandedness.Single(x => x.Id == id);
    }

    public async Task<BatHandedness> GetBatHandednessById(int id)
    {
        var batHandedness = await _lookupCache.GetBatHandednessTypes();
        return batHandedness.Single(x => x.Id == id);
    }

    public async Task<Trait> GetTraitById(int id)
    {
        var traits = await _lookupCache.GetTraits();
        return traits.Single(x => x.Id == id);
    }

    public async Task<PitchType> GetPitchTypeById(int id)
    {
        var pitchTypes = await _lookupCache.GetPitchTypes();
        return pitchTypes.Single(x => x.Id == id);
    }

    public async Task<PlayerAward> GetPlayerAwardById(int id)
    {
        var playerAwards = await _lookupCache.GetPlayerAwards();
        return playerAwards.Single(x => x.Id == id);
    }
}