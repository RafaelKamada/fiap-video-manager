using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoManager.Domain.Interfaces;
using VideoManager.Infrastructure.Repositories; 

namespace VideoManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddScoped<IVideoRepository>(provider => 
            new VideoRepository(
                configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found")));

        // Configura��o do AWS SQS
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonSQS>();

        // Configura��o do servi�o SQS personalizado
        services.AddScoped<ISqsService, SqsService>();

        return services;
    }
}