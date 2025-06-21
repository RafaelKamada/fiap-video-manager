using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoManager.Application.Commands;
using VideoManager.Application.Commands.Interfaces;

namespace VideoManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddScoped<IAddVideoCommand, AddVideoCommand>();
        services.AddScoped<ISendEmailCommand, SendEmailCommand>();
        services.AddScoped<IUpdateStatusCommand, UpdateStatusCommand>();

        return services;
    }
}
