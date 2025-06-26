using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoManager.Domain.Interfaces;
using VideoManager.Infrastructure.Repositories;
using VideoManager.Infrastructure.Services;

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

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localstack:4566",
                UseHttp = true,
                AuthenticationRegion = "us-east-1"
            };

            return new AmazonSQSClient("test", "test", config);
        });

        //services.AddDefaultAWSOptions(configuration.GetAWSOptions());

        services.AddScoped<ISqsService>(provider =>
        {
            var sqsClient = provider.GetRequiredService<Amazon.SQS.IAmazonSQS>();
            var queueUrl = configuration["Sqs:QueueUrl"];

            if (string.IsNullOrEmpty(queueUrl)) throw new ArgumentNullException("Sqs:QueueUrl", "Queue URL not configured in appsettings.");
            
            return new SqsService(sqsClient, queueUrl);
        });

        services.AddScoped<IEmailService>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var apiKey = configuration["SendGrid:ApiKey"];
            var fromEmail = configuration["SendGrid:FromEmail"];
            var fromName = configuration["SendGrid:FromName"];

            return new EmailService(apiKey ?? string.Empty, fromEmail ?? string.Empty, fromName ?? string.Empty);
        });

        // Registra o serviço de armazenamento S3
        services.AddScoped<IStorageService, S3StorageService>();

        return services;
    }
}