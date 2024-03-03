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

        var result = await _mediator.Send(new GetPitcherRolesRequest());
        if (result.TryPickT1(out var exception, out var pitcherRolesList))
        {
            throw exception;
        }

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

        var result = await _mediator.Send(new GetChemistryRequest());
        if (result.TryPickT1(out var exception, out var chemistryList))
        {
            throw exception;
        }

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

        var result = await _mediator.Send(new GetPositionsRequest());
        if (result.TryPickT1(out var exception, out var positionsList))
        {
            throw exception;
        }

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

        var result = await _mediator.Send(new GetBatHandednessRequest());
        if (result.TryPickT1(out var exception, out var batHandednessTypes))
        {
            throw exception;
        }

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

        var result = await _mediator.Send(new GetThrowHandednessRequest());
        if (result.TryPickT1(out var exception, out var throwHandednessTypes))
        {
            throw exception;
        }

        x = throwHandednessTypes
            .Select(y => y.FromCore())
            .ToList();

        _memoryCache.Set(key, x);
        return x;
    }
}