using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Navend.Core.Step.Abstract;
using Navend.Core.Step.Concrete;

namespace Navend.Core.Step;

public static class Extensions
{
    public static IServiceCollection AddSteps(this IServiceCollection services)
    {
        var stepInterfaceType = typeof(IStep<>);
        var baseContextType = typeof(StepContext);

        var executingAssembly = Assembly.GetExecutingAssembly();
        var fullName = executingAssembly.FullName;
        if (string.IsNullOrEmpty(fullName)) return services;
        string rootNamespace = fullName.Split(',')[0].Split('.')[0];
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic &&
                        !string.IsNullOrEmpty(a.FullName) &&
                        !a.FullName.StartsWith(rootNamespace))
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
                Console.Write(ex.ToString());
            }

            // IStep<TContext> türünden interface’leri bul
            var stepInterfaces = types
                .Where(t => t.IsInterface && t.IsGenericType)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == stepInterfaceType))
                .Where(t => baseContextType.IsAssignableFrom(t.GetGenericArguments().First()))
                .ToList();

            foreach (var stepInterface in stepInterfaces)
            {
                var implementations = types
                    .Where(t => t.IsClass && !t.IsAbstract && stepInterface.IsAssignableFrom(t))
                    .ToList();

                foreach (var impl in implementations)
                {
                    services.AddScoped(stepInterface, impl);
                }
            }
        }

        return services;
    }
}
