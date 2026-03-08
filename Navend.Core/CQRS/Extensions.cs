using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;

namespace Navend.Core.CQRS.Extensions;

public static class Extensions {
    public static IServiceCollection AddCQRS(this IServiceCollection services) {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        services.AddCommands(assemblies);
        services.AddQueries(assemblies);
        return services;
    }

    private static IServiceCollection AddCommands(this IServiceCollection services, List<Assembly> assemblies) {
        var commandHandlerInterfaceType = typeof(ICommandHandler<>);
        return services.RegisterCQRSServices(assemblies, commandHandlerInterfaceType);
    }

    private static IServiceCollection AddQueries(this IServiceCollection services, List<Assembly> assemblies) {
        var queryHandlerInterfaceType = typeof(IQueryHandler<,>);
        return services.RegisterCQRSServices(assemblies, queryHandlerInterfaceType);
    }

    private static IServiceCollection RegisterCQRSServices(this IServiceCollection services, List<Assembly> assemblies, Type interfaceType) {
        var registrations = assemblies
            .SelectMany(GetDefinedTypesSafely)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.IsGenericTypeDefinition && !t.ContainsGenericParameters)
            .Where(t => !t.IsDefined(typeof(UOWAttribute), true))
            .Where(t => !t.IsDefined(typeof(DecoratorAttribute), true))
            .SelectMany(t => t.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                .Where(i => !i.ContainsGenericParameters)
                .Select(i => (Service: i, Impl: t.AsType())))
            .Distinct();

        foreach (var (serviceType, implType) in registrations) {
            if (services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implType)) {
                continue;
            }

            services.AddTransient(serviceType, implType);
        }

        return services;
    }

    private static IEnumerable<TypeInfo> GetDefinedTypesSafely(Assembly assembly) {
        try {
            return assembly.DefinedTypes;
        }
        catch (ReflectionTypeLoadException ex) {
            return ex.Types
                .Where(t => t is not null)
                .Select(t => t!.GetTypeInfo());
        }
        catch {
            return Enumerable.Empty<TypeInfo>();
        }
    }
}
