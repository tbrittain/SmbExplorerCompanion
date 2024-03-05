using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Csv.Services;
using SmbExplorerCompanion.Database.Services;
using SmbExplorerCompanion.Database.Services.Imports;
using SmbExplorerCompanion.Database.Services.Lookups;
using SmbExplorerCompanion.Database.Services.Players;

namespace SmbExplorerCompanion.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<SmbExplorerCompanionDbContext>((provider, builder) =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                builder.UseLoggerFactory(loggerFactory);
            })
            .AddScoped<IRepository<FranchiseDto>, FranchiseRepository>()
            .AddScoped<IRepository<SeasonDto>, SeasonRepository>()
            .AddScoped<ISeasonSearchService, SeasonRepository>()
            .AddScoped<IRepository<PositionDto>, PositionRepository>()
            .AddScoped<IRepository<PlayerAwardDto>, PlayerAwardRepository>()
            .AddScoped<IRepository<PitcherRoleDto>, PitcherRoleRepository>()
            .AddScoped<IRepository<ChemistryDto>, ChemistryRepository>()
            .AddScoped<IRepository<BatHandednessDto>, BatHandednessRepository>()
            .AddScoped<IRepository<ThrowHandednessDto>, ThrowHandednessRepository>()
            .AddScoped<IRepository<TraitDto>, TraitRepository>()
            .AddScoped<IRepository<PlayerAwardDto>, AwardRepository>()
            .AddScoped<ITeamRepository, TeamRepository>()
            .AddScoped<IGeneralPlayerRepository, GeneralPlayerRepository>()
            .AddScoped<IPitcherCareerRepository, PitcherCareerRepository>()
            .AddScoped<IPitcherSeasonRepository, PitcherSeasonRepository>()
            .AddScoped<IPositionPlayerCareerRepository, PositionPlayerCareerRepository>()
            .AddScoped<IPositionPlayerSeasonRepository, PositionPlayerSeasonRepository>()
            .AddScoped<IAwardDelegationRepository, AwardDelegationRepository>()
            .AddScoped<ISearchRepository, SearchRepository>()
            .AddScoped<ISummaryRepository, SummaryRepository>()
            .AddTransient<CsvReaderService>()
            .AddTransient<CsvMappingRepository>()
            .AddTransient<ICsvImportRepository, CsvImportRepository>();

        return services;
    }
}