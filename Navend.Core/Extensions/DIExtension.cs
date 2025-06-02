using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;
using Navend.Core.CQRS;
using Navend.Core.Data.EfCore;
using Navend.Core.Decorator.Helper;
using Navend.Core.UOW;
using Navend.Core.UOW.Decorator;

namespace Navend.Core.Extensions;

public static class DIExtension
{
    public static IServiceCollection AddEfCoreUnitOfWork<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
    {
        var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
        services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();
        services.AddDecorator(typeof(ICommandHandler<>),typeof(CommandHandlerDecorator<>));
        return services;
    }

    public static IServiceCollection AddDecorator(this IServiceCollection services, Type closedInterfaceType, Type decoratorType)
    {
        if (closedInterfaceType == null)
            throw new ArgumentNullException(nameof(closedInterfaceType));

        if (decoratorType == null)
            throw new ArgumentNullException(nameof(decoratorType));

        if (!closedInterfaceType.IsInterface || !closedInterfaceType.IsGenericType)
            throw new ArgumentException("Only closed generic interfaces are supported.", nameof(closedInterfaceType));

        if (!decoratorType.GetCustomAttributes(typeof(DecoratorAttribute), true).Any())
            throw new InvalidOperationException($"{decoratorType.Name} must be annotated with [Decorator] attribute.");

        var interfaceTypeDef = closedInterfaceType.GetGenericTypeDefinition();
        var typeArguments = closedInterfaceType.GetGenericArguments();

        var assembly = decoratorType.Assembly;

        var handlerType = assembly.GetTypes()
            .FirstOrDefault(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == interfaceTypeDef) &&
                !t.GetCustomAttributes(typeof(DecoratorAttribute), true).Any());

        if (handlerType == null)
        {
            throw new InvalidOperationException(
                $"No non-decorator class found that implements {interfaceTypeDef.Name} in assembly {assembly.GetName().Name}.");
        }

        var closedHandlerType = handlerType.IsGenericType
            ? handlerType.MakeGenericType(typeArguments)
            : handlerType;

        var closedDecoratorType = decoratorType.MakeGenericType(typeArguments);

        services.AddScoped(closedHandlerType);
        services.AddScoped(closedInterfaceType, sp =>
        {
            var inner = sp.GetRequiredService(closedHandlerType);
            return ActivatorUtilities.CreateInstance(sp, closedDecoratorType, inner);
        });

        return services;
    }
}