using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Runtime.ConstrainedExecution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.CQRS;
using Navend.Core.Data.EfCore;
using Navend.Core.Decorator.Helper;
using Navend.Core.UOW;
using Navend.Core.UOW.Decorator;

namespace Navend.Core.Extensions;

public static class DIExtension {
    public static IServiceCollection AddEfCoreUnitOfWork<TContext>(this IServiceCollection services) where TContext: DbContext {
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork<TContext>>();
        services.AddDecorator(typeof(ICommandHandler<>), typeof(CommandHandlerDecorator<>));
        return services;
    }

    public static IServiceCollection AddDecorator(this IServiceCollection services, Type typeService, Type typeImplementation) {
        var isDecorator = DecoratorHelper.IsDecorator(typeImplementation);
        if (!isDecorator) {
            throw new InvalidOperationException($"The service '{nameof(typeService)}' must be registered as a decorator and annotated with the [Decorator] attribute.");
        }
        services.AddScoped(serviceProvider => {
            serviceProvider.GetRequiredService(typeService);
        });
        return services;
    }
} 