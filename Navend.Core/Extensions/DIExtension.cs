using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;
using Navend.Core.Caching.Abstract;
using Navend.Core.Caching.Concrete;
using Navend.Core.Constants;
using Navend.Core.CQRS;
using Navend.Core.Data.EfCore;
using Navend.Core.Step;
using Navend.Core.UOW;
using Navend.Core.UOW.Decorator;
using StackExchange.Redis;

namespace Navend.Core.Extensions;

public static class DIExtension
{
    #region Cache
    public static void AddCaches(this IServiceCollection services, IConfiguration configuration, CacheTypes cacheTypes, params Assembly[] assemblies)
    {
        if (cacheTypes.HasFlag(CacheTypes.InMemory))
        {
            services.AddInMemoryCaches(assemblies);
            services.AddHostedService<CacheWarmUpHostedService>();
        }
        if (cacheTypes.HasFlag(CacheTypes.Redis))
        {
            services.AddRedisCache<object>(configuration);
            return;
        }
    }

    private static void AddInMemoryCaches(this IServiceCollection services, Assembly[] assemblies)
    {
        // Mevcut assembly'deki tüm sınıfları al.
        List<Type?> cacheServices = new List<Type?>();
        assemblies.ToList().ForEach(assembly =>
        {
            var _cacheServices = assemblies
                                .SelectMany(a => a.GetTypes())
                                .Where(t => t.IsClass && !t.IsAbstract)
                                .Where(t => IsSubclassOfRawGeneric(typeof(InMemoryCache<>), t))
                                .ToList();
            cacheServices.AddRange(_cacheServices);
        });

        foreach (var service in cacheServices)
        {
            // Sınıfın implement ettiği tüm interface'leri al
            var interfaces = service?.GetInterfaces();

            // Bu sınıfı, her interface ile register et (IBaseCacheService<T> ve ICacheWarmUpService)
            if (interfaces != null)
            {
                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType != typeof(ICacheWarmUpService) &&
                       (!interfaceType.IsGenericType ||
                         interfaceType.GetGenericTypeDefinition() != typeof(IBaseCache<>)) &&
                        service is not null)
                    {
                        services.AddSingleton(interfaceType, service);
                        services.AddSingleton<ICacheWarmUpService>(provider =>
                            (ICacheWarmUpService)provider.GetRequiredService(interfaceType));
                    }
                }
            }
        }
    }
    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        return toCheck
            .BaseTypes() // extension method aşağıda
            .Any(bt => bt.IsGenericType && bt.GetGenericTypeDefinition() == generic);
    }

    // BaseType zincirini IEnumerable ile çıkaran extension
    internal static IEnumerable<Type> BaseTypes(this Type type)
    {
        for (var bt = type.BaseType; bt != null && bt != typeof(object); bt = bt.BaseType)
        {
            yield return bt;
        }
    }
    public static IServiceCollection AddRedisCache<T>(this IServiceCollection services, IConfiguration config)
    {
        var options = config.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();

        var connectionString =
            $"{options.Host}:{options.Port},defaultDatabase={options.Database}" +
            (string.IsNullOrWhiteSpace(options.Password) ? "" : $",password={options.Password}");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(connectionString));

        services.AddSingleton<IRedisCache<T>>(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisCache<T>(mux, options.InstanceName);
        });

        return services;
    }
    #endregion
    #region UOW
    public static IServiceCollection AddEfCoreUnitOfWork<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : DbContext
    {
        var connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"];
        services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();
        services.AddOpenGenericDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        services.AddStepDecorator();
        return services;
    }
    #endregion

    #region Decorator
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
    #endregion
}