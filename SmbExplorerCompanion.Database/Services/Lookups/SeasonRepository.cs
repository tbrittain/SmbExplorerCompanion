using Microsoft.EntityFrameworkCore;
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

    public async Task<IEnumerable<SeasonDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

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

    public Task<SeasonDto> AddAsync(SeasonDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async Task<SeasonDto?> GetByTeamSeasonIdAsync(int teamSeasonId, CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        var season = await _dbContext.SeasonTeamHistory
            .Include(x => x.Season)
            .ThenInclude(x => x.Franchise)
            .Where(x => x.Season.FranchiseId == franchiseId)
            .Where(x => x.Id == teamSeasonId)
            .Select(x => x.Season)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (season is null) return null;

        var mapper = new SeasonMapping();
        var seasonDto = mapper.SeasonToSeasonDto(season);

        return seasonDto;
    }
}