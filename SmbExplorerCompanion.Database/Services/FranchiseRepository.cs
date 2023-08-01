using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services;

public class FranchiseRepository : IRepository<FranchiseDto>
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public FranchiseRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<IEnumerable<FranchiseDto>, Exception>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var franchises = await _dbContext.Franchises
                .ToListAsync(cancellationToken: cancellationToken);
            var mapper = new FranchiseMapping();
            var franchiseDtos = franchises
                .Select(x => mapper.FranchiseToFranchiseDto(x))
                .ToList();
            return franchiseDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<FranchiseDto, None, Exception>> GetByIdAsync(int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var franchise =
                await _dbContext.Franchises.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (franchise is null) return new None();

            var mapper = new FranchiseMapping();
            var franchiseDto = mapper.FranchiseToFranchiseDto(franchise);
            return franchiseDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<FranchiseDto, Exception>> AddAsync(FranchiseDto entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mapper = new FranchiseMapping();
            var franchise = mapper.FranchiseDtoToFranchise(entity);
            await _dbContext.Franchises.AddAsync(franchise, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var franchiseDto = mapper.FranchiseToFranchiseDto(franchise);
            return franchiseDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<IEnumerable<FranchiseDto>, Exception>> AddRangeAsync(IEnumerable<FranchiseDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async Task<OneOf<FranchiseDto, None, Exception>> UpdateAsync(FranchiseDto entity,
        CancellationToken cancellationToken = default)
    {
        if (entity.Id == default) return new None();
        var existingFranchise =
            await _dbContext.Franchises.SingleOrDefaultAsync(x => x.Id == entity.Id,
                cancellationToken);
        if (existingFranchise is null) return new None();

        var mapper = new FranchiseMapping();
        var franchise = mapper.FranchiseDtoToFranchise(entity);
        _dbContext
            .Entry(existingFranchise)
            .CurrentValues.SetValues(franchise);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var franchiseDto = mapper.FranchiseToFranchiseDto(franchise);
        return franchiseDto;
    }

    public Task<OneOf<FranchiseDto, None, Exception>> DeleteAsync(int id,
        CancellationToken cancellationToken = default)
    {
        // TODO: This is a tricky one since we will need to delete all the related entities as well
        // we may want to think about a soft delete instead, as space is not really a concern,
        // and then the user can restore the franchise if they want to
        throw new NotImplementedException();
    }
}