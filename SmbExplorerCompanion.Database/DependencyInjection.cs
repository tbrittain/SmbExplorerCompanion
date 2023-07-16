using Microsoft.Extensions.DependencyInjection;
using SmbExplorerCompanion.Core.Entities;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Services;

namespace SmbExplorerCompanion.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<SmbExplorerCompanionDbContext>()
            .AddScoped<IRepository<FranchiseDto>, FranchiseRepository>();

        return services;
    }
}