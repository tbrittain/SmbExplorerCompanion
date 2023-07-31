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
                .ToListAsync(cancellationToken);

            var championshipWinners = await _dbContext.ChampionshipWinners
                .ToListAsync(cancellationToken);

            var mapper = new SeasonMapping();
            var seasonDtos = seasons.Select(season => mapper.SeasonToSeasonDto(season)).ToList();

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

    public Task<OneOf<SeasonDto, None, Exception>> GetByIdAsync(int Id, CancellationToken cancellationToken = default)
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

    public Task<OneOf<SeasonDto, None, Exception>> DeleteAsync(int Id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}