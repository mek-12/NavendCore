using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;

namespace Navend.Core.CQRS.Extensions;

public static class Extensions {
    public static IServiceCollection AddCQRS(this IServiceCollection services) {
        services.AddCommands();
        services.AddQueries();
        //services.AddEvents();
        return services;
    }
    private static IServiceCollection AddCommands(this IServiceCollection services) {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var commandHandlerInterfaceType = typeof(ICommandHandler<>);

        return services.RegisterCQRSServices(assemblies, commandHandlerInterfaceType);
    }

    private static IServiceCollection AddQueries(this IServiceCollection services) {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var queryHandlerInterfaceType = typeof(IQueryHandler<,>);

        return services.RegisterCQRSServices(assemblies, queryHandlerInterfaceType);
    }

    private static IServiceCollection RegisterCQRSServices(this IServiceCollection services, List<Assembly> assemblies, Type? type){
        var registrations = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.IsDefined(typeof(DecoratorAttribute), true))
            .SelectMany(t => t.ImplementedInterfaces
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == type)
                .Select(i => (Service: i, Impl: t.AsType())));

        foreach (var (serviceType, implType) in registrations) {
            services.AddTransient(serviceType, implType);
        }
        return services;
    }
}