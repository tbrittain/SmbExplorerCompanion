using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class FranchiseRepository(SmbExplorerCompanionDbContext dbContext) : IGetAllRepository<FranchiseDto>, IAddRepository<FranchiseDto>
{
    public async Task<IEnumerable<FranchiseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var franchises = await dbContext.Franchises
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
        await dbContext.Franchises.AddAsync(franchise, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var franchiseDto = mapper.FranchiseToFranchiseDto(franchise);
        return franchiseDto;
    }
}