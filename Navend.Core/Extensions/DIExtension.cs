using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Data.EfCore;
using Navend.Core.UOW;

namespace Navend.Core.Extensions;

public static class DIExtension {
    public static IServiceCollection AddEfCoreUnitOfWork<TContext>(this IServiceCollection services) where TContext: DbContext {
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();
        return services;
    }
} 