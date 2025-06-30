using System.Reflection;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Services.ApplicationInfrastructure.Mediator;
using Application.Services.ApplicationInfrastructure.Mediator.PipelineBehaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSender();
        services.AddRequestHandlers(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<>), typeof(ValidationPipelineBehavior<>));
    }
}