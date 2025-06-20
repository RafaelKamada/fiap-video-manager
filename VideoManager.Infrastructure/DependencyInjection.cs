using Amazon.Runtime;
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
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "test";
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "test";
        var serviceUrl = Environment.GetEnvironmentVariable("AWS_SERVICE_URL") ?? "http://localhost:9324";
        var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? "us-east-1";

        var config = new AmazonSQSConfig
        {
            ServiceURL = serviceUrl,
            UseHttp = true,
            AuthenticationRegion = region
        };

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(credentials, config));

        // Configura��o do servi�o SQS personalizado
        services.AddScoped<ISqsService, SqsService>();

        return services;
    }
}