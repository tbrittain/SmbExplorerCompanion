using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.WPF.Models.Lookups;
using Chemistry = SmbExplorerCompanion.WPF.Models.Lookups.Chemistry;
using PitcherRole = SmbExplorerCompanion.WPF.Models.Lookups.PitcherRole;
using Position = SmbExplorerCompanion.WPF.Models.Lookups.Position;

namespace SmbExplorerCompanion.WPF.Services;

public class LookupCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly IMediator _mediator;

    public LookupCache(IMemoryCache memoryCache, IMediator mediator)
    {
        _memoryCache = memoryCache;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PitcherRole>> GetPitcherRoles()
    {
        const string key = "PitcherRoles";
        if (_memoryCache.TryGetValue(key, out var pitcherRoles) && 
            pitcherRoles is List<PitcherRole> x)
        {
            return x;
        }

        var pitcherRolesList = await _mediator.Send(new GetPitcherRolesRequest());
        x = pitcherRolesList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }

    public async Task<IReadOnlyList<Chemistry>> GetChemistryTypes()
    {
        const string key = "Chemistry";
        if (_memoryCache.TryGetValue(key, out var chemistry) && 
            chemistry is List<Chemistry> x)
        {
            return x;
        }

        var chemistryList = await _mediator.Send(new GetChemistryRequest());
        x = chemistryList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }

    public async Task<IReadOnlyList<Position>> GetPositions()
    {
        const string key = "Positions";
        if (_memoryCache.TryGetValue(key, out var positions) && 
            positions is List<Position> x)
        {
            return x;
        }

        var positionsList = await _mediator.Send(new GetPositionsRequest());
        x = positionsList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }

    public async Task<IReadOnlyList<BatHandedness>> GetBatHandednessTypes()
    {
        const string key = "BatHandedness";
        if (_memoryCache.TryGetValue(key, out var batHandedness) && 
            batHandedness is List<BatHandedness> x)
        {
            return x;
        }

        var batHandednessTypes = await _mediator.Send(new GetBatHandednessRequest());
        x = batHandednessTypes
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }

    public async Task<IReadOnlyList<ThrowHandedness>> GetThrowHandednessTypes()
    {
        const string key = "ThrowHandedness";
        if (_memoryCache.TryGetValue(key, out var throwHandedness) && 
            throwHandedness is List<ThrowHandedness> x)
        {
            return x;
        }

        var throwHandednessTypes = await _mediator.Send(new GetThrowHandednessRequest());
        x = throwHandednessTypes
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }
    
    public async Task<IReadOnlyList<Trait>> GetTraits()
    {
        const string key = "Traits";
        if (_memoryCache.TryGetValue(key, out var traits) && 
            traits is List<Trait> x)
        {
            return x;
        }

        var traitsList = await _mediator.Send(new GetTraitsRequest());
        x = traitsList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }
    
    public async Task<IReadOnlyList<PitchType>> GetPitchTypes()
    {
        const string key = "PitchTypes";
        if (_memoryCache.TryGetValue(key, out var pitchTypes) && 
            pitchTypes is List<PitchType> x)
        {
            return x;
        }

        var pitchTypesList = await _mediator.Send(new GetPitchTypesRequest());
        x = pitchTypesList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }

    public async Task<IReadOnlyList<PlayerAward>> GetPlayerAwards()
    {
        const string key = "PlayerAwards";
        if (_memoryCache.TryGetValue(key, out var playerAwards) &&
            playerAwards is List<PlayerAward> x)
        {
            return x;
        }

        var playerAwardsList = await _mediator.Send(GetPlayerAwardsRequest.AllAwards);
        x = playerAwardsList
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }
}