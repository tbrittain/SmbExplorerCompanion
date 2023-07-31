using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Csv.Services;
using SmbExplorerCompanion.Database.Services;

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
            .AddScoped<ITeamRepository, TeamRepository>()
            .AddScoped<IPlayerRepository, PlayerRepository>()
            .AddTransient<CsvReaderService>()
            .AddTransient<CsvMappingRepository>()
            .AddTransient<ICsvImportRepository, CsvImportRepository>();

        return services;
    }
}