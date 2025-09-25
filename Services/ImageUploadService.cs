using Amazon.S3;
using Amazon.S3.Model;
using BlazorAuthApp.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;
using System.Text.RegularExpressions;

namespace BlazorAuthApp.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImageUploadService> _logger;
        private readonly string _bucketName;
        private IAmazonS3? _regionSpecificClient;
        private readonly IAmazonS3 _defaultClient;

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
            _configuration = configuration;
            _logger = logger;
            _bucketName = configuration["AWS:S3:BucketName"]
                ?? throw new ArgumentNullException(nameof(_bucketName), "AWS:S3:BucketName configuration is missing");

            _defaultClient = s3Client ?? throw new ArgumentNullException(nameof(s3Client));

            _logger.LogInformation("Initialized ImageUploadService for bucket {BucketName}", _bucketName);
        }

        private async Task<IAmazonS3> GetCorrectS3ClientAsync()
        {
            if (_regionSpecificClient != null)
                return _regionSpecificClient;

            try
            {
                // Try to use the default client first
                var bucketRegion = await GetBucketRegionAsync(_defaultClient, _bucketName);
                var currentRegion = _defaultClient.Config.RegionEndpoint?.SystemName;

                if (string.IsNullOrEmpty(bucketRegion) ||
                    string.Equals(bucketRegion, currentRegion, StringComparison.OrdinalIgnoreCase))
                {
                    _regionSpecificClient = _defaultClient;
                    return _defaultClient;
                }

                _logger.LogInformation("Creating S3 client for bucket region {BucketRegion} instead of {CurrentRegion}",
                    bucketRegion, currentRegion);

                // Create new client with correct region
                var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(bucketRegion);

                // Use the same credentials from configuration
                var accessKey = _configuration["AWS:AccessKey"];
                var secretKey = _configuration["AWS:SecretKey"];

                if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                {
                    _regionSpecificClient = new AmazonS3Client(accessKey, secretKey, regionEndpoint);
                }
                else
                {
                    // Use default credential chain (EC2 role, environment variables, etc.)
                    _regionSpecificClient = new AmazonS3Client(regionEndpoint);
                }

                _logger.LogInformation("Successfully created S3 client for region {Region}", bucketRegion);
                return _regionSpecificClient;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not create region-specific S3 client. Using default client.");
                _regionSpecificClient = _defaultClient;
                return _defaultClient;
            }
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

                // Get the correct S3 client for the bucket's region
                var s3Client = await GetCorrectS3ClientAsync();

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
                    // Add server-side encryption for better security
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                    Metadata =
                    {
                        ["original-filename"] = file.Name,
                        ["upload-date"] = DateTime.UtcNow.ToString("O"),
                        ["content-length"] = file.Size.ToString()
                    }
                };

                var response = await s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Return the proper URL based on the region
                    return await GetObjectUrlAsync(s3Client, key);
                }

                throw new Exception($"S3 upload failed with status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName} to bucket: {BucketName}",
                    file.Name, _bucketName);
                throw;
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderName = "blog-images")
        {
            try
            {
                // Validate parameters
                if (imageStream == null)
                    throw new ArgumentNullException(nameof(imageStream));
                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException($"File extension {fileExtension} is not allowed");
                }

                // Get the correct S3 client for the bucket's region
                var s3Client = await GetCorrectS3ClientAsync();

                var newFileName = $"{Guid.NewGuid()}{fileExtension}";
                var key = $"{folderName}/{DateTime.UtcNow:yyyy/MM/dd}/{newFileName}";

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = imageStream,
                    ContentType = GetContentType(fileExtension),
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                    Metadata =
                    {
                        ["original-filename"] = fileName,
                        ["upload-date"] = DateTime.UtcNow.ToString("O")
                    }
                };

                var response = await s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return await GetObjectUrlAsync(s3Client, key);
                }

                throw new Exception($"S3 upload failed with status: {response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image stream for file: {FileName} to bucket: {BucketName}",
                    fileName, _bucketName);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Get the correct S3 client for the bucket's region
                var s3Client = await GetCorrectS3ClientAsync();

                // Extract key from URL - handle different URL formats
                var key = ExtractKeyFromUrl(imageUrl);
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("Could not extract key from URL: {ImageUrl}", imageUrl);
                    return false;
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await s3Client.DeleteObjectAsync(request);

                // DeleteObject returns 204 No Content for successful deletions
                var success = response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;

                if (success)
                {
                    _logger.LogInformation("Successfully deleted image: {Key} from bucket: {BucketName}",
                        key, _bucketName);
                }
                else
                {
                    _logger.LogWarning("Delete operation returned unexpected status: {StatusCode} for key: {Key}",
                        response.HttpStatusCode, key);
                }

                return success;
            }
            catch (AmazonS3Exception s3Ex)
            {
                // Log S3-specific errors but don't fail if object doesn't exist
                if (s3Ex.ErrorCode == "NoSuchKey")
                {
                    _logger.LogInformation("Object not found for deletion: {ImageUrl}", imageUrl);
                    return true; // Consider it successful if already deleted
                }

                _logger.LogError(s3Ex, "S3 error deleting image: {ImageUrl}", imageUrl);
                return false;
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

            // Check if file has content
            if (file.Size == 0)
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                return false;

            // Check content type
            if (string.IsNullOrEmpty(file.ContentType) ||
                !_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private async Task<string> GetBucketRegionAsync(IAmazonS3 client, string bucketName)
        {
            try
            {
                var response = await client.GetBucketLocationAsync(new GetBucketLocationRequest
                {
                    BucketName = bucketName
                });

                // S3 returns null or empty for us-east-1
                var region = response.Location.Value;
                if (string.IsNullOrEmpty(region) || region == "US")
                    return "us-east-1";

                return region.ToLowerInvariant();
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get bucket location for bucket: {BucketName}. Message: {Message}",
                    bucketName, ex.Message);

                // If the error message contains the correct endpoint, extract it
                if (ex.Message.Contains("endpoint") || ex.Message.Contains("s3."))
                {
                    // Try to extract region from error message
                    // Example: "...addressed using the specified endpoint: s3.eu-west-1.amazonaws.com"
                    var match = Regex.Match(ex.Message, @"s3[.-]([a-z0-9-]+)\.amazonaws\.com");
                    if (match.Success && match.Groups.Count > 1)
                    {
                        var extractedRegion = match.Groups[1].Value;
                        _logger.LogInformation("Extracted region {Region} from error message for bucket {BucketName}",
                            extractedRegion, bucketName);
                        return extractedRegion;
                    }
                }

                // Default region if we couldn't determine it
                return "us-east-1";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error getting bucket region for bucket: {BucketName}", bucketName);
                return "us-east-1";
            }
        }

        /// <summary>
        /// Gets the proper object URL based on bucket region and configuration
        /// </summary>
        private async Task<string> GetObjectUrlAsync(IAmazonS3 client, string key)
        {
            try
            {
                // Get bucket location to construct proper URL
                var locationResponse = await client.GetBucketLocationAsync(new GetBucketLocationRequest
                {
                    BucketName = _bucketName
                });

                var region = locationResponse.Location.Value;
                // S3 returns null or empty for us-east-1
                if (string.IsNullOrEmpty(region) || region == "US")
                    region = "us-east-1";

                // Construct the proper S3 URL based on region
                if (region == "us-east-1")
                {
                    return $"https://{_bucketName}.s3.amazonaws.com/{key}";
                }
                else
                {
                    return $"https://{_bucketName}.s3.{region}.amazonaws.com/{key}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine bucket region for URL construction, using fallback");

                // Try to use the client's configured region as fallback
                var clientRegion = client.Config.RegionEndpoint?.SystemName;
                if (!string.IsNullOrEmpty(clientRegion))
                {
                    if (clientRegion == "us-east-1")
                    {
                        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
                    }
                    else
                    {
                        return $"https://{_bucketName}.s3.{clientRegion}.amazonaws.com/{key}";
                    }
                }

                // Final fallback
                return $"https://{_bucketName}.s3.amazonaws.com/{key}";
            }
        }

        /// <summary>
        /// Extracts the object key from various S3 URL formats
        /// </summary>
        private string ExtractKeyFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);

                // Handle different S3 URL formats:
                // Virtual-hosted style: https://bucket-name.s3.region.amazonaws.com/key
                // Path style: https://s3.region.amazonaws.com/bucket-name/key

                if (uri.Host.Contains(".s3.") || uri.Host.Contains(".s3-"))
                {
                    // Virtual-hosted style
                    return uri.AbsolutePath.TrimStart('/');
                }
                else if (uri.Host.StartsWith("s3.") || uri.Host.StartsWith("s3-"))
                {
                    // Path style - remove bucket name from path
                    var pathParts = uri.AbsolutePath.TrimStart('/').Split('/', 2);
                    return pathParts.Length > 1 ? pathParts[1] : string.Empty;
                }

                // Fallback
                return uri.AbsolutePath.TrimStart('/');
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting key from URL: {ImageUrl}", imageUrl);
                return string.Empty;
            }
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