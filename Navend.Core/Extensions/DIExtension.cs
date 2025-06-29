using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;
using Navend.Core.CQRS;
using Navend.Core.Data.EfCore;
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
        services.AddOpenGenericDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        return services;
    }

    public static IServiceCollection AddOpenGenericDecorator(
        this IServiceCollection services,
        Type openInterfaceType,
        Type openDecoratorType)
    {
        if (!openInterfaceType.IsGenericTypeDefinition)
            throw new ArgumentException("openInterfaceType must be a generic type definition");

        if (!openDecoratorType.IsGenericTypeDefinition)
            throw new ArgumentException("openDecoratorType must be a generic type definition");

        if (!openDecoratorType.GetCustomAttributes(typeof(DecoratorAttribute), true).Any())
            throw new InvalidOperationException($"{openDecoratorType.Name} must be annotated with [Decorator] attribute.");

        // Tüm assembly'leri tara
        var allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x =>
            {
                try { return x.GetTypes(); }
                catch { return Array.Empty<Type>(); } // ReflectionTypeLoadException koruması
            })
            .ToList();

        // Interface'i implement eden, decorator olmayan handler'ları bul
        var handlerTypes = allTypes
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType))
            .Where(t => !t.GetCustomAttributes(typeof(DecoratorAttribute), true).Any())
            .ToList();

        if (!handlerTypes.Any())
            return services; // Hiç handler yoksa çık

        foreach (var handlerType in handlerTypes)
        {
            // Handler hangi interface'i uyguluyor (kapalı haliyle)?
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);

            var genericArguments = interfaceType.GetGenericArguments();
            var closedDecoratorType = openDecoratorType.MakeGenericType(genericArguments);

            // 1️⃣ Önce gerçek handler'ı kaydet
            services.AddScoped(handlerType);

            // 2️⃣ Decorator'ı interface'e kaydet
            services.AddScoped(interfaceType, sp =>
            {
                var inner = sp.GetRequiredService(handlerType);
                return ActivatorUtilities.CreateInstance(sp, closedDecoratorType, inner);
            });
        }

        return services;
    }
}