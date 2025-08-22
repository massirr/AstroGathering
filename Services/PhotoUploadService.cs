using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AstroGathering.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AstroGathering.Services
{
    public class PhotoUploadService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public PhotoUploadService()
        {
            _connectionString = ConfigurationService.AzureStorageConnectionString;
            _containerName = ConfigurationService.AzureStorageContainerName;
        }

        public async Task<string> UploadPhotoAsync(Stream photoStream, string fileName)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Ensure container exists
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
                
                // Generate unique filename with timestamp
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}{fileExtension}";
                var blobClient = containerClient.GetBlobClient(uniqueFileName);
                
                // Set content type based on file extension
                var contentType = GetContentType(fileExtension);
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                };
                
                // Reset stream position to beginning
                photoStream.Position = 0;
                
                // Upload the file
                await blobClient.UploadAsync(photoStream, uploadOptions);
                
                // Return the public URL
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload photo to Azure Blob Storage: {ex.Message}", ex);
            }
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Try to get container properties to test connection
                await containerClient.GetPropertiesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
