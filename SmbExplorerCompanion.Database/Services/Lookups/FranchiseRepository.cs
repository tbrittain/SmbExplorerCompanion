using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class FranchiseRepository : IRepository<FranchiseDto>
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public FranchiseRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<FranchiseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var franchises = await _dbContext.Franchises
            .ToListAsync(cancellationToken: cancellationToken);
        var mapper = new FranchiseMapping();
        var franchiseDtos = franchises
            .Select(x => mapper.FranchiseToFranchiseDto(x))
            .ToList();
        return franchiseDtos;
    }

    public async Task<FranchiseDto> AddAsync(FranchiseDto entity,
        CancellationToken cancellationToken = default)
    {
        var mapper = new FranchiseMapping();
        var franchise = mapper.FranchiseDtoToFranchise(entity);
        await _dbContext.Franchises.AddAsync(franchise, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var franchiseDto = mapper.FranchiseToFranchiseDto(franchise);
        return franchiseDto;
    }
}