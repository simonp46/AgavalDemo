using GestorInventario.Application.Common.Behaviors;
using GestorInventario.Application.Common.Validation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GestorInventario.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var validatorRegistrations = assembly.DefinedTypes
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .SelectMany(type => type.ImplementedInterfaces
                .Where(service => service.IsGenericType &&
                    service.GetGenericTypeDefinition() == typeof(IRequestValidator<>))
                .Select(service => new
                {
                    Service = service,
                    Implementation = type.AsType()
                }));

        foreach (var registration in validatorRegistrations)
        {
            services.AddTransient(registration.Service, registration.Implementation);
        }

        return services;
    }
}
