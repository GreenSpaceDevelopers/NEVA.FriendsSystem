using System.Reflection;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.ApplicationInfrastructure.Mediator;

public static class MediatorInjector
{
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceType = typeof(IRequestHandler<>);

        var handlerTypes = assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(interfaces => interfaces.IsGenericType && interfaces.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => new { Service = i, Implementation = t }))
            .ToList();

        foreach (var pair in handlerTypes)
        {
            services.AddTransient(pair.Service, pair.Implementation);
        }

        return services;
    }

    public static IServiceCollection AddSender(this IServiceCollection services)
    {
        services.AddSingleton<ISender, Sender>();
        return services;
    }
}