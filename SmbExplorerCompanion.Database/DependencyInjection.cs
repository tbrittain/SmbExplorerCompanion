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
            .AddScoped<IGetAllRepository<FranchiseDto>, FranchiseRepository>()
            .AddScoped<IAddRepository<FranchiseDto>, FranchiseRepository>()
            .AddScoped<IGetAllRepository<SeasonDto>, SeasonRepository>()
            .AddScoped<ISeasonSearchService, SeasonRepository>()
            .AddScoped<IGetAllRepository<PositionDto>, PositionRepository>()
            .AddScoped<IGetAllRepository<PlayerAwardDto>, PlayerAwardRepository>()
            .AddScoped<IAddRepository<PlayerAwardDto>, PlayerAwardRepository>()
            .AddScoped<IGetAllRepository<PitcherRoleDto>, PitcherRoleRepository>()
            .AddScoped<IGetAllRepository<ChemistryDto>, ChemistryRepository>()
            .AddScoped<IGetAllRepository<BatHandednessDto>, BatHandednessRepository>()
            .AddScoped<IGetAllRepository<PitchTypeDto>, PitchTypesRepository>()
            .AddScoped<IGetAllRepository<ThrowHandednessDto>, ThrowHandednessRepository>()
            .AddScoped<IGetAllRepository<TraitDto>, TraitRepository>()
            .AddScoped<IGetAllRepository<PlayerAwardDto>, AwardRepository>()
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