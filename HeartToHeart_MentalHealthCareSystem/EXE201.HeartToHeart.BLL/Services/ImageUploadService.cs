using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IImageUploadRepository _imageUploadRepository;
        private readonly IFirebaseService _firebaseService;
        private readonly IUserService _userService;
        private readonly IBlogService _blogService;
        private readonly ILogger<ImageUploadService> _logger;

        public ImageUploadService(
            IImageUploadRepository imageUploadRepository,
            IFirebaseService firebaseService,
            IUserService userService,
            IBlogService blogService,
            ILogger<ImageUploadService> logger)
        {
            _imageUploadRepository = imageUploadRepository;
            _firebaseService = firebaseService;
            _userService = userService;
            _blogService = blogService;
            _logger = logger;
        }

        public async Task<ImageUploadResponse> UploadAndAssignImageAsync(CreateImageUploadDto createDto, Guid userId)
        {
            try
            {
                // First, upload the image
                var uploadResponse = await UploadImageAsync(createDto, userId);
                if (!uploadResponse.Success)
                {
                    return uploadResponse;
                }

                // Then assign to the appropriate entity
                var assignmentSuccess = await AssignImageToEntityAsync(
                    uploadResponse.Data.Url,
                    createDto.EntityType,
                    createDto.EntityId,
                    userId);

                if (!assignmentSuccess)
                {
                    _logger.LogWarning("Image uploaded but failed to assign to entity. EntityType: {EntityType}, EntityId: {EntityId}",
                        createDto.EntityType, createDto.EntityId);

                    uploadResponse.Message += " (Warning: Image uploaded but not assigned to entity)";
                }

                return uploadResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading and assigning image for user: {UserId}", userId);
                return new ImageUploadResponse
                {
                    Success = false,
                    Message = "Failed to upload and assign image"
                };
            }
        }

        private async Task<bool> AssignImageToEntityAsync(string imageUrl, string entityType, Guid? entityId, Guid userId)
        {
            if (!entityId.HasValue)
                return false;

            try
            {
                switch (entityType?.ToLower())
                {
                    case "profile":
                        return await AssignProfileImageAsync(imageUrl, userId);

                    case "blog":
                        return await AssignBlogFeaturedImageAsync(imageUrl, entityId.Value, userId);

                    case "diaryentry":
                        // Add diary entry image assignment logic
                        return await AssignDiaryEntryImageAsync(imageUrl, entityId.Value, userId);

                    case "mediacontent":
                        // Add media content image assignment logic
                        return await AssignMediaContentImageAsync(imageUrl, entityId.Value, userId);

                    case "anonymouspost":
                        // Add anonymous post image assignment logic
                        return await AssignAnonymousPostImageAsync(imageUrl, entityId.Value, userId);

                    default:
                        _logger.LogWarning("Unknown entity type for image assignment: {EntityType}", entityType);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning image to entity. EntityType: {EntityType}, EntityId: {EntityId}",
                    entityType, entityId);
                return false;
            }
        }

        private async Task<bool> AssignProfileImageAsync(string imageUrl, Guid userId)
        {
            try
            {
                var updateDto = new UpdateUserProfileDto
                {
                    ProfilePicture = imageUrl
                };

                return await _userService.UpdateUserProfileAsync(userId, updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning profile image for user: {UserId}", userId);
                return false;
            }
        }

        private async Task<bool> AssignBlogFeaturedImageAsync(string imageUrl, Guid blogId, Guid userId)
        {
            try
            {
                var updateDto = new UpdateBlogDto
                {
                    FeaturedImage = imageUrl
                };

                return await _blogService.UpdateBlogAsync(blogId, updateDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning featured image to blog: {BlogId}", blogId);
                return false;
            }
        }

        private async Task<bool> AssignDiaryEntryImageAsync(string imageUrl, Guid diaryEntryId, Guid userId)
        {
            // Implement diary entry image assignment
            // This would depend on your DiaryEntry service structure
            try
            {
                // Example implementation - adjust based on your DiaryEntry service
                // var updateDto = new UpdateDiaryEntryDto { ImageUrl = imageUrl };
                // return await _diaryEntryService.UpdateDiaryEntryAsync(diaryEntryId, updateDto, userId);

                _logger.LogInformation("Diary entry image assignment not implemented yet");
                return true; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning image to diary entry: {DiaryEntryId}", diaryEntryId);
                return false;
            }
        }

        private async Task<bool> AssignMediaContentImageAsync(string imageUrl, Guid mediaContentId, Guid userId)
        {
            // Implement media content image assignment
            try
            {
                // Example implementation - adjust based on your MediaContent service
                // var updateDto = new UpdateMediaContentDto { ImageUrl = imageUrl };
                // return await _mediaContentService.UpdateMediaContentAsync(mediaContentId, updateDto, userId);

                _logger.LogInformation("Media content image assignment not implemented yet");
                return true; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning image to media content: {MediaContentId}", mediaContentId);
                return false;
            }
        }

        private async Task<bool> AssignAnonymousPostImageAsync(string imageUrl, Guid anonymousPostId, Guid userId)
        {
            // Implement anonymous post image assignment
            try
            {
                // Example implementation - adjust based on your AnonymousPost service
                // var updateDto = new UpdateAnonymousPostDto { ImageUrl = imageUrl };
                // return await _anonymousPostService.UpdateAnonymousPostAsync(anonymousPostId, updateDto, userId);

                _logger.LogInformation("Anonymous post image assignment not implemented yet");
                return true; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning image to anonymous post: {AnonymousPostId}", anonymousPostId);
                return false;
            }
        }

        // Keep the original upload method for backward compatibility
        public async Task<ImageUploadResponse> UploadImageAsync(CreateImageUploadDto createDto, Guid userId)
        {
            try
            {
                // Validate file
                var isValid = await _firebaseService.ValidateImageFileAsync(createDto.File);
                if (!isValid)
                {
                    return new ImageUploadResponse
                    {
                        Success = false,
                        Message = "Invalid file. Please check file size and format."
                    };
                }

                // Determine folder name based on entity type
                var folderName = GetFolderName(createDto.EntityType);

                // Upload to Firebase
                var imageUrl = await _firebaseService.UploadImageAsync(createDto.File, folderName);

                // Create entity
                var imageUpload = new ImageUpload
                {
                    Id = Guid.NewGuid(),
                    FileName = createDto.File.FileName,
                    FirebaseUrl = imageUrl,
                    ContentType = createDto.File.ContentType,
                    FileSize = createDto.File.Length,
                    EntityType = createDto.EntityType,
                    EntityId = createDto.EntityId,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save to database
                var createdImage = await _imageUploadRepository.CreateAsync(imageUpload);

                return new ImageUploadResponse
                {
                    Success = true,
                    Message = "Image uploaded successfully",
                    Data = MapToImageUploadDto(createdImage)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user: {UserId}", userId);
                return new ImageUploadResponse
                {
                    Success = false,
                    Message = "Failed to upload image"
                };
            }
        }

        // ... rest of your existing methods remain the same ...

        public async Task<ImageUploadDto?> GetImageByIdAsync(Guid id)
        {
            try
            {
                var image = await _imageUploadRepository.GetByIdAsync(id);
                return image == null ? null : MapToImageUploadDto(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image by ID: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<ImageUploadDto>> GetAllImagesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var images = await _imageUploadRepository.GetAllAsync(page, pageSize);
                return images.Select(MapToImageUploadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all images");
                return Enumerable.Empty<ImageUploadDto>();
            }
        }

        public async Task<IEnumerable<ImageUploadDto>> GetImagesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var images = await _imageUploadRepository.GetByUserIdAsync(userId, page, pageSize);
                return images.Select(MapToImageUploadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images by user ID: {UserId}", userId);
                return Enumerable.Empty<ImageUploadDto>();
            }
        }

        public async Task<IEnumerable<ImageUploadDto>> GetImagesByEntityAsync(string entityType, Guid entityId)
        {
            try
            {
                var images = await _imageUploadRepository.GetByEntityAsync(entityType, entityId);
                return images.Select(MapToImageUploadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images by entity: {EntityType}, {EntityId}", entityType, entityId);
                return Enumerable.Empty<ImageUploadDto>();
            }
        }

        public async Task<IEnumerable<ImageUploadDto>> GetActiveImagesByEntityAsync(string entityType, Guid entityId)
        {
            try
            {
                var images = await _imageUploadRepository.GetActiveByEntityAsync(entityType, entityId);
                return images.Select(MapToImageUploadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active images by entity: {EntityType}, {EntityId}", entityType, entityId);
                return Enumerable.Empty<ImageUploadDto>();
            }
        }

        public async Task<bool> UpdateImageAsync(Guid id, UpdateImageUploadDto updateDto)
        {
            try
            {
                var image = await _imageUploadRepository.GetByIdAsync(id);
                if (image == null)
                    return false;

                // Update properties
                if (!string.IsNullOrEmpty(updateDto.EntityType))
                    image.EntityType = updateDto.EntityType;

                if (updateDto.EntityId.HasValue)
                    image.EntityId = updateDto.EntityId;

                if (updateDto.IsActive.HasValue)
                    image.IsActive = updateDto.IsActive.Value;

                image.UpdatedAt = DateTime.UtcNow;

                return await _imageUploadRepository.UpdateAsync(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image: {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteImageAsync(Guid id)
        {
            try
            {
                var image = await _imageUploadRepository.GetByIdAsync(id);
                if (image == null)
                    return false;

                // Delete from Firebase
                await _firebaseService.DeleteImageAsync(image.FirebaseUrl);

                // Delete from database
                return await _imageUploadRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Id}", id);
                return false;
            }
        }

        public async Task<bool> SoftDeleteImageAsync(Guid id)
        {
            try
            {
                return await _imageUploadRepository.SoftDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting image: {Id}", id);
                return false;
            }
        }

        public async Task<int> GetTotalImagesCountAsync()
        {
            try
            {
                return await _imageUploadRepository.GetTotalCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total images count");
                return 0;
            }
        }

        public async Task<int> GetImageCountByUserIdAsync(Guid userId)
        {
            try
            {
                return await _imageUploadRepository.GetCountByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image count by user ID: {UserId}", userId);
                return 0;
            }
        }

        private string GetFolderName(string? entityType)
        {
            return entityType?.ToLower() switch
            {
                "blog" => "blogs",
                "profile" => "profiles",
                "diaryentry" => "diary-entries",
                "mediacontent" => "media-content",
                "anonymouspost" => "anonymous-posts",
                _ => "general"
            };
        }

        private static ImageUploadDto MapToImageUploadDto(ImageUpload image)
        {
            return new ImageUploadDto
            {
                Id = image.Id,
                FileName = image.FileName,
                Url = image.FirebaseUrl,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                EntityType = image.EntityType,
                EntityId = image.EntityId,
                IsActive = image.IsActive,
                UserId = image.UserId,
                CreatedAt = image.CreatedAt,
                UpdatedAt = image.UpdatedAt
            };
        }
    }
}