using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services;

public class SeasonRepository : IRepository<SeasonDto>
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public SeasonRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<IEnumerable<SeasonDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var seasons = await _dbContext.Seasons
                .Include(x => x.Franchise)
                .ToListAsync(cancellationToken);

            var championshipWinners = await _dbContext.ChampionshipWinners
                .ToListAsync(cancellationToken);

            var mapper = new SeasonMapping();
            var seasonDtos = seasons.Select(season => mapper.SeasonToSeasonDto(season)).ToList();

            foreach (var seasonDto in seasonDtos)
            {
                var season = seasons.Single(s => s.Id == seasonDto.Id);
                seasonDto.FranchiseId = season.Franchise.Id;
            }

            // In theory, we should be able to do this with a single LINQ query, but the association between
            // Season and ChampionshipWinner is not working properly. I'm not sure why, but this will need to be
            // revisited later.
            foreach (var championshipWinner in championshipWinners)
            {
                var seasonDto = seasonDtos.FirstOrDefault(season => season.Id == championshipWinner.SeasonId);
                if (seasonDto is not null)
                {
                    seasonDto.ChampionshipWinnerId = championshipWinner.Id;
                }
            }

            return seasonDtos;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<SeasonDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<SeasonDto, Exception>> AddAsync(SeasonDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<SeasonDto>, Exception>> AddRangeAsync(IEnumerable<SeasonDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<SeasonDto, None, Exception>> UpdateAsync(SeasonDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<SeasonDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}