using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Attributes;
using Navend.Core.Step.Abstract;
using Navend.Core.Step.Concrete;
using Navend.Core.UOW.Decorators;

namespace Navend.Core.Step;

public static class Extensions
{
    public static IServiceCollection AddStepDecorator(this IServiceCollection services)
    {
        var stepInterfaceType = typeof(IStep<>);
        var stepContextBaseType = typeof(StepContext);
        var openDecoratorType = typeof(StepDecorator<>);

        var executingAssembly = Assembly.GetEntryAssembly();
        var fullName = executingAssembly?.FullName;
        if (string.IsNullOrEmpty(fullName)) return services;

        string rootNamespace = fullName.Split(',')[0].Split('.')[0];

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic &&
                        !string.IsNullOrEmpty(a.FullName) &&
                        a.FullName.StartsWith(rootNamespace))
            .ToList();

        foreach (var assembly in assemblies)
        {
            var types = new List<Type>();
            try
            {
                types = assembly.GetTypes().ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var stepTypes = types
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => !t.GetCustomAttributes(typeof(DecoratorAttribute), true).Any())
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == stepInterfaceType &&
                                    stepContextBaseType.IsAssignableFrom(i.GetGenericArguments()[0])) // T : StepContext
                        .Select(i => new { Service = i, Implementation = t }))
                .ToList();
            if (stepTypes != null && stepTypes.Any())
            {
                // StepContext registration
                var contextTypes = types
                    .Where(t => stepContextBaseType.IsAssignableFrom(t) && t != stepContextBaseType)
                    .ToList();

                foreach (var contextType in contextTypes)
                {
                    services.AddScoped(stepContextBaseType, contextType); // Add base context interface if used
                    services.AddScoped(contextType, contextType);         // Add itself
                }
                foreach (var pair in stepTypes)
                {
                    var genericArguments = pair.Service.GetGenericArguments();
                    var closedDecoratorType = openDecoratorType.MakeGenericType(genericArguments);

                    services.AddScoped(pair.Implementation);
                    services.AddScoped(pair.Service, sp =>
                    {
                        var inner = sp.GetRequiredService(pair.Implementation);
                        var orderProp = inner.GetType().GetProperty("Order");
                        object? orderValue = orderProp?.GetValue(inner);
                        int order = orderValue is int o ? o : 0;

                        var decorator = ActivatorUtilities.CreateInstance(sp, closedDecoratorType, inner);
                        var setOrderMethod = decorator.GetType().GetMethod("SetOrder", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (setOrderMethod != null)
                        {
                            setOrderMethod.Invoke(decorator, new object[] { order });
                        }

                        return decorator;
                    });
                }
            }
        }

        return services;
    }
}
