using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.CQRS;
using Navend.Core.Data.EfCore;
using Navend.Core.Decorator.Helper;
using Navend.Core.UOW;
using Navend.Core.UOW.Decorator;

namespace Navend.Core.Extensions;

public static class DIExtension {
    public static IServiceCollection AddEfCoreUnitOfWork<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext: DbContext {
        var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
        services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();
        services.AddDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        return services;
    }

    private static IServiceCollection AddDecorator(this IServiceCollection services, Type typeService, Type typeImplementation) {
        var isDecorator = DecoratorHelper.IsDecorator(typeImplementation);
        if (!isDecorator) {
            throw new InvalidOperationException($"The service '{nameof(typeService)}' must be registered as a decorator and annotated with the [Decorator] attribute.");
        }
        services.AddScoped(serviceProvider => {
            serviceProvider.GetRequiredService(typeService);
            return serviceProvider;
        });
        return services;
    }
} 