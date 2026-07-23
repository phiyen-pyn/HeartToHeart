using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EXE201.HeartToHeart.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageUploadController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ImageUploadController> _logger;

        public ImageUploadController(IImageUploadService imageUploadService, ILogger<ImageUploadController> logger)
        {
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] CreateImageUploadDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _imageUploadService.UploadImageAsync(createDto, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in upload image endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("upload-and-assign")]
        public async Task<IActionResult> UploadAndAssignImage([FromForm] CreateImageUploadDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _imageUploadService.UploadAndAssignImageAsync(createDto, userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in upload and assign image endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var createDto = new CreateImageUploadDto
                {
                    File = file,
                    EntityType = "profile",
                    EntityId = userId // For profile pictures, EntityId is the user's ID
                };

                var result = await _imageUploadService.UploadAndAssignImageAsync(createDto, userId);

                if (result.Success)
                    return Ok(new { Message = "Profile picture updated successfully", Data = result.Data });

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost("blog/{blogId}/featured-image")]
        public async Task<IActionResult> UploadBlogFeaturedImage(Guid blogId, IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var createDto = new CreateImageUploadDto
                {
                    File = file,
                    EntityType = "blog",
                    EntityId = blogId
                };

                var result = await _imageUploadService.UploadAndAssignImageAsync(createDto, userId);

                if (result.Success)
                    return Ok(new { Message = "Blog featured image updated successfully", Data = result.Data });

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blog featured image");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImageById(Guid id)
        {
            try
            {
                var image = await _imageUploadService.GetImageByIdAsync(id);
                if (image == null)
                    return NotFound(new { Message = "Image not found" });

                return Ok(image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image by ID: {Id}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImages([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var images = await _imageUploadService.GetAllImagesAsync(page, pageSize);
                var totalCount = await _imageUploadService.GetTotalImagesCountAsync();

                return Ok(new
                {
                    Images = images,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all images");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetImagesByUserId(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Users can only see their own images, unless they are admin
                if (currentUserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var images = await _imageUploadService.GetImagesByUserIdAsync(userId, page, pageSize);
                var totalCount = await _imageUploadService.GetImageCountByUserIdAsync(userId);

                return Ok(new
                {
                    Images = images,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images by user ID: {UserId}", userId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetImagesByEntity(string entityType, Guid entityId)
        {
            try
            {
                var images = await _imageUploadService.GetImagesByEntityAsync(entityType, entityId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images by entity: {EntityType}, {EntityId}", entityType, entityId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("entity/{entityType}/{entityId}/active")]
        public async Task<IActionResult> GetActiveImagesByEntity(string entityType, Guid entityId)
        {
            try
            {
                var images = await _imageUploadService.GetActiveImagesByEntityAsync(entityType, entityId);
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active images by entity: {EntityType}, {EntityId}", entityType, entityId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(Guid id, [FromBody] UpdateImageUploadDto updateDto)
        {
            try
            {
                var result = await _imageUploadService.UpdateImageAsync(id, updateDto);

                if (result)
                    return Ok(new { Message = "Image updated successfully" });

                return BadRequest(new { Message = "Failed to update image" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image: {Id}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            try
            {
                var result = await _imageUploadService.DeleteImageAsync(id);

                if (result)
                    return Ok(new { Message = "Image deleted successfully" });

                return BadRequest(new { Message = "Failed to delete image" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {Id}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPut("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeleteImage(Guid id)
        {
            try
            {
                var result = await _imageUploadService.SoftDeleteImageAsync(id);

                if (result)
                    return Ok(new { Message = "Image soft deleted successfully" });

                return BadRequest(new { Message = "Failed to soft delete image" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting image: {Id}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetTotalImagesCount()
        {
            try
            {
                var count = await _imageUploadService.GetTotalImagesCountAsync();
                return Ok(new { TotalCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total images count");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}/count")]
        public async Task<IActionResult> GetImageCountByUserId(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Users can only see their own image count, unless they are admin
                if (currentUserId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var count = await _imageUploadService.GetImageCountByUserIdAsync(userId);
                return Ok(new { TotalCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image count by user ID: {UserId}", userId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token");
            }
            return userId;
        }
    }
}