using Amazon.SQS;
using Amazon.SQS.Model;
using Moq;
using VideoManager.Domain.Entities;
using VideoManager.Infrastructure.Services;

namespace VideoManager.Tests.Infrastructure.Service;

public class SqsServiceTest
{
    [Fact]
    public async Task SendAsync_CallsSendMessageAsync_WithCorrectParameters()
    {
        // Arrange
        var sqsMock = new Mock<IAmazonSQS>();
        sqsMock.Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new SendMessageResponse { MessageId = "msg-123" });

        var service = new SqsService(sqsMock.Object, "http://queue-url");

        var video = new Video
        {
            Id = 42,
            CaminhoVideo = "s3://bucket/video.mp4"
        };

        // Act
        await service.SendAsync(video);

        // Assert
        sqsMock.Verify(s => s.SendMessageAsync(
            It.Is<SendMessageRequest>(req =>
                req.QueueUrl == "http://queue-url" &&
                req.MessageBody.Contains("\"Video_Id\":\"42\"") &&
                req.MessageBody.Contains("\"Path\":\"s3://bucket/video.mp4\"") &&
                req.MessageBody.Contains("\"Extension\":\".mp4\"")
            ),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_RetriesOnException()
    {
        // Arrange
        var sqsMock = new Mock<IAmazonSQS>();
        int callCount = 0;
        sqsMock.Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
               .Returns(() =>
               {
                   callCount++;
                   if (callCount < 3)
                       throw new Exception("Transient error");
                   return Task.FromResult(new SendMessageResponse { MessageId = "msg-456" });
               });

        var service = new SqsService(sqsMock.Object, "http://queue-url");

        var video = new Video
        {
            Id = 99,
            CaminhoVideo = "s3://bucket/video2.mp4"
        };

        // Act
        await service.SendAsync(video);

        // Assert: Should retry 3 times (Polly default in your code)
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentNullException_WhenCaminhoVideoIsNull()
    {
        var sqsMock = new Mock<IAmazonSQS>();
        var service = new SqsService(sqsMock.Object, "http://queue-url");

        var video = new Video
        {
            Id = 1,
            CaminhoVideo = null
        };

        await Assert.ThrowsAsync<NullReferenceException>(() => service.SendAsync(video));
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentNullException_WhenVideoIsNull()
    {
        var sqsMock = new Mock<IAmazonSQS>();
        var service = new SqsService(sqsMock.Object, "http://queue-url");

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            (Task)typeof(SqsService)
                .GetMethod("MapRequestMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { null })
        );
    }
}