using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VideoManager.Domain.Interfaces;

namespace VideoManager.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _serviceUrl;

    public S3StorageService(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:S3:BucketName"] ?? "videos";
        _serviceUrl = configuration["AWS:S3:ServiceURL"] ?? "http://localhost:4566";

        var config = new AmazonS3Config
        {
            ServiceURL = _serviceUrl,
            ForcePathStyle = true,
            UseHttp = _serviceUrl.StartsWith("http://")
        };

        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"] ?? "test",
            configuration["AWS:SecretKey"] ?? "test",
            config);
    }


    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(request);
        return fileName;
    }

    public string GetFileUrl(string fileName)
    {
        return $"{_serviceUrl}/{_bucketName}/{fileName}";
    }

    public async Task<MemoryStream> DownloadFileAsync(string fileKey)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            using var response = await _s3Client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();

            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            throw new FileNotFoundException($"O arquivo {fileKey} não foi encontrado no bucket {_bucketName}.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao baixar o arquivo {fileKey} do S3. Detalhes: {ex.Message}", ex);
        }
    }
}
