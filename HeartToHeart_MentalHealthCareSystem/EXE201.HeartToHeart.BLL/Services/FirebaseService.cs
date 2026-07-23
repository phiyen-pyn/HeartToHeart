using EXE201.HeartToHeart.BLL.Configuration;
using EXE201.HeartToHeart.BLL.IServices;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseConfig _firebaseConfig;
        private readonly ILogger<FirebaseService> _logger;
        private readonly StorageClient _storageClient;

        public FirebaseService(IOptions<FirebaseConfig> firebaseConfig, ILogger<FirebaseService> logger)
        {
            _firebaseConfig = firebaseConfig.Value;
            _logger = logger;

            // Initialize Google Cloud Storage client with service account
            var credential = GoogleCredential.FromFile(_firebaseConfig.ServiceAccountKeyPath);
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is null or empty");

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var objectName = $"{folderName}/{fileName}";

                // Upload to Google Cloud Storage
                using var stream = file.OpenReadStream();
                var storageObject = await _storageClient.UploadObjectAsync(
                    bucket: _firebaseConfig.StorageBucket,
                    objectName: objectName,
                    contentType: file.ContentType,
                    source: stream
                );

                // Generate the public URL (Firebase Storage rules already allow public read)
                var downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_firebaseConfig.StorageBucket}/o/{Uri.EscapeDataString(objectName)}?alt=media";

                _logger.LogInformation("Image uploaded successfully to Firebase: {ObjectName}", objectName);
                return downloadUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Firebase");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                var objectName = GetObjectNameFromUrl(imageUrl);
                if (string.IsNullOrEmpty(objectName))
                    return false;

                await _storageClient.DeleteObjectAsync(_firebaseConfig.StorageBucket, objectName);

                _logger.LogInformation("Image deleted successfully from Firebase: {ObjectName}", objectName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Firebase: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public async Task<bool> ValidateImageFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return false;

                // Check file size
                var maxSizeInBytes = _firebaseConfig.MaxFileSizeInMB * 1024 * 1024;
                if (file.Length > maxSizeInBytes)
                    return false;

                // Check file type
                var allowedContentTypes = _firebaseConfig.AllowedFileTypes;
                if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
                    return false;

                // Check file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image file");
                return false;
            }
        }

        private string GetObjectNameFromUrl(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return string.Empty;

                // Extract object name from Firebase Storage URL
                var uri = new Uri(imageUrl);
                var segments = uri.Segments;

                // Find the 'o' segment and get the next one
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    if (segments[i].TrimEnd('/') == "o")
                    {
                        var objectName = Uri.UnescapeDataString(segments[i + 1]);
                        return objectName.Split('?')[0]; // Remove query parameters
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting object name from URL: {ImageUrl}", imageUrl);
                return string.Empty;
            }
        }

        public string GetFileNameFromUrl(string imageUrl)
        {
            try
            {
                var objectName = GetObjectNameFromUrl(imageUrl);
                if (string.IsNullOrEmpty(objectName))
                    return string.Empty;

                return Path.GetFileName(objectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting filename from URL: {ImageUrl}", imageUrl);
                return string.Empty;
            }
        }
    }
}