using Amazon.SQS;
using Amazon.SQS.Model;
using Polly;
using Polly.Retry;
using System.IO.Compression;
using System.Text.Json;
using VideoManager.Domain.Entities;
using VideoManager.Domain.Interfaces;
using Policy = Polly.Policy;

namespace VideoManager.Infrastructure.Services;

public class SqsService(IAmazonSQS sqsClient ,string queueUrl) : ISqsService
{
    private readonly IAmazonSQS _sqsClient = sqsClient;
    private readonly string _queueUrl = queueUrl ?? throw new Exception("Url não mapeada");

    private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, time) =>
                {
                    Console.WriteLine($"Erro ao enviar para o SQS. Tentando novamente em {time.TotalSeconds}s...");
                });

    public async Task SendAsync(Video video)
    {
        VideoContent message = MapRequestMessage(video);

        var messageJson = JsonSerializer.Serialize(message);

        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageJson
        };

        await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _sqsClient.SendMessageAsync(request);
            Console.WriteLine($"Mensagem enviada para SQS com ID: {response.MessageId}");
        });
    }

    private static VideoContent MapRequestMessage(Video video)
    {
        var base64Content = GetArchive(video);

        var message = new VideoContent
        {
            VideoId = video.Id.ToString(),
            Content = base64Content,
            Extension = "zip"
        };
        return message;
    }

    private static string GetArchive(Video video)
    {
        if (video == null)
            throw new ArgumentNullException(nameof(video), "Video cannot be null.");
        

        if (video.Conteudo == null)
            throw new ArgumentNullException(nameof(video.Conteudo), "Video content cannot be null.");

        using var outputStream = new MemoryStream();
        using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
        {
            var zipEntry = archive.CreateEntry("video.mp4", CompressionLevel.Optimal);
            using var zipStream = zipEntry.Open();
            zipStream.Write(video.Conteudo, 0, video.Conteudo.Length);
        }

        var zippedBytes = outputStream.ToArray();
        var base64Content = Convert.ToBase64String(zippedBytes);

        return base64Content;
    }
}
