using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SmbExplorerCompanion.Core.Commands.Queries.Lookups;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.WPF.Services;

public class LookupsContext : ILookupsContext
{
    private readonly IMemoryCache _memoryCache;
    private readonly IMediator _mediator;

    public LookupsContext(IMemoryCache memoryCache, IMediator mediator)
    {
        _memoryCache = memoryCache;
        _mediator = mediator;
    }

    public async Task<IReadOnlyList<PitcherRoleDto>> GetPitcherRoles()
    {
        const string key = "PitcherRoles";
        if (_memoryCache.TryGetValue(key, out var pitcherRoles) && 
            pitcherRoles is List<PitcherRoleDto> pitcherRolesList)
        {
            return pitcherRolesList;
        }

        var result = await _mediator.Send(new GetPitcherRolesRequest());
        if (result.TryPickT1(out var exception, out pitcherRolesList))
        {
            throw exception;
        }

        _memoryCache.Set(key, pitcherRolesList);
        return pitcherRolesList;
    }

    public async Task<IReadOnlyList<ChemistryDto>> GetChemistryTypes()
    {
        const string key = "Chemistry";
        if (_memoryCache.TryGetValue(key, out var chemistry) && 
            chemistry is List<ChemistryDto> chemistryList)
        {
            return chemistryList;
        }

        var result = await _mediator.Send(new GetChemistryRequest());
        if (result.TryPickT1(out var exception, out chemistryList))
        {
            throw exception;
        }

        _memoryCache.Set(key, chemistryList);
        return chemistryList;
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositions()
    {
        const string key = "Positions";
        if (_memoryCache.TryGetValue(key, out var positions) && 
            positions is List<PositionDto> positionsList)
        {
            return positionsList;
        }

        var result = await _mediator.Send(new GetPositionsRequest());
        if (result.TryPickT1(out var exception, out positionsList))
        {
            throw exception;
        }

        _memoryCache.Set(key, positionsList);
        return positionsList;
    }

    public async Task<IReadOnlyList<BatHandednessDto>> GetBatHandednessTypes()
    {
        const string key = "BatHandedness";
        if (_memoryCache.TryGetValue(key, out var batHandedness) && 
            batHandedness is List<BatHandednessDto> batHandednessTypes)
        {
            return batHandednessTypes;
        }

        var result = await _mediator.Send(new GetBatHandednessRequest());
        if (result.TryPickT1(out var exception, out batHandednessTypes))
        {
            throw exception;
        }

        _memoryCache.Set(key, batHandednessTypes);
        return batHandednessTypes;
    }

    public async Task<IReadOnlyList<ThrowHandednessDto>> GetThrowHandednessTypes()
    {
        const string key = "ThrowHandedness";
        if (_memoryCache.TryGetValue(key, out var throwHandedness) && 
            throwHandedness is List<ThrowHandednessDto> throwHandednessTypes)
        {
            return throwHandednessTypes;
        }

        var result = await _mediator.Send(new GetThrowHandednessRequest());
        if (result.TryPickT1(out var exception, out throwHandednessTypes))
        {
            throw exception;
        }

        _memoryCache.Set(key, throwHandednessTypes);
        return throwHandednessTypes;
    }
}