using Chronix.EventRepository.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Chronix.EventRepository.Registry;

public static class ProjectionRegistry
{
    public static IServiceCollection AddProjectionsFromAssemblyContaining(
        this IServiceCollection services, Type assemblyToScan)
    {
        var assembly = assemblyToScan.Assembly;

        var projectionTypes = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                ImplementsGenericProjectionBase(t));

        foreach (var type in projectionTypes)
        {
            services.AddScoped(typeof(IProjection), type); // Register as IProjection
        }

        return services;
    }

    private static bool ImplementsGenericProjectionBase(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ProjectionBase<,>))
                return true;

            type = type.BaseType!;
        }

        return false;
    }
}
