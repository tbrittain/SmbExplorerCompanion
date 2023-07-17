using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmbExplorerCompanion.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}