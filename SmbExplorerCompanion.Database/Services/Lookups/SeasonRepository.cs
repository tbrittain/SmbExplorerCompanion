using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class SeasonRepository : IRepository<SeasonDto>, ISeasonSearchService
{
    private readonly SmbExplorerCompanionDbContext _dbContext;
    private readonly IApplicationContext _applicationContext;

    public SeasonRepository(SmbExplorerCompanionDbContext dbContext, IApplicationContext applicationContext)
    {
        _dbContext = dbContext;
        _applicationContext = applicationContext;
    }

    public async Task<OneOf<IEnumerable<SeasonDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        try
        {
            var seasons = await _dbContext.Seasons
                .Where(x => x.FranchiseId == franchiseId)
                .Include(x => x.Franchise)
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

    public Task<OneOf<SeasonDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<SeasonDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
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

    public async Task<OneOf<SeasonDto, None, Exception>> GetByTeamSeasonIdAsync(int teamSeasonId, CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        try
        {
            var season = await _dbContext.SeasonTeamHistory
                .Include(x => x.Season)
                .ThenInclude(x => x.Franchise)
                .Where(x => x.Season.FranchiseId == franchiseId)
                .Where(x => x.Id == teamSeasonId)
                .Select(x => x.Season)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (season is null) return new None();

            var mapper = new SeasonMapping();
            var seasonDto = mapper.SeasonToSeasonDto(season);

            return seasonDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}