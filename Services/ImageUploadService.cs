using Amazon.S3;
using Amazon.S3.Model;
using BlazorAuthApp.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;

namespace BlazorAuthApp.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageUploadService> _logger;
        private readonly string _bucketName;

        // Allowed image types
        private readonly HashSet<string> _allowedExtensions = new()
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
        };

        private readonly HashSet<string> _allowedContentTypes = new()
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif",
            "image/webp", "image/bmp"
        };

        public ImageUploadService(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<ImageUploadService> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
            _bucketName = configuration["AWS:S3:blazor-auth-app-blog-images-bucket"]
                ?? throw new ArgumentNullException(nameof(_bucketName));
        }

        public async Task<string> UploadImageAsync(IBrowserFile file, string folderName = "blog-images")
        {
            try
            {
                // Validate file
                if (!ValidateImageFile(file))
                {
                    throw new ArgumentException("Invalid image file");
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.Name).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var key = $"{folderName}/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";

                // Upload to S3
                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead, // Make publicly accessible
                    Metadata =
                    {
                        ["original-filename"] = file.Name,
                        ["upload-date"] = DateTime.UtcNow.ToString("O")
                    }
                };

                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Return the public URL
                    var region = _configuration["AWS:Region"] ?? "us-east-1";
                    return $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";
                }

                throw new Exception($"S3 upload failed with status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", file.Name);
                throw;
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderName = "blog-images")
        {
            try
            {
                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                var newFileName = $"{Guid.NewGuid()}{fileExtension}";
                var key = $"{folderName}/{DateTime.UtcNow:yyyy/MM/dd}/{newFileName}";

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = imageStream,
                    ContentType = GetContentType(fileExtension),
                    CannedACL = S3CannedACL.PublicRead
                };

                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var region = _configuration["AWS:Region"] ?? "us-east-1";
                    return $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";
                }

                throw new Exception($"S3 upload failed with status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image stream");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Extract key from URL
                var uri = new Uri(imageUrl);
                var key = uri.AbsolutePath.TrimStart('/');

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public bool ValidateImageFile(IBrowserFile file)
        {
            if (file == null)
                return false;

            // Check file size (10MB max)
            if (file.Size > 10 * 1024 * 1024)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check content type
            if (!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
