using System.Reflection;
using Application.Services.ApplicationInfrastructure.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSender();
        services.AddRequestHandlers(Assembly.GetExecutingAssembly());
    }
}