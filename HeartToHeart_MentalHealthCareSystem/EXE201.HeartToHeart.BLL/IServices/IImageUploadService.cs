using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IImageUploadService
    {
        // New method for uploading and assigning images
        Task<ImageUploadResponse> UploadAndAssignImageAsync(CreateImageUploadDto createDto, Guid userId);

        // Original upload method (kept for backward compatibility)
        Task<ImageUploadResponse> UploadImageAsync(CreateImageUploadDto createDto, Guid userId);

        // Retrieval methods
        Task<ImageUploadDto?> GetImageByIdAsync(Guid id);
        Task<IEnumerable<ImageUploadDto>> GetAllImagesAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<ImageUploadDto>> GetImagesByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<ImageUploadDto>> GetImagesByEntityAsync(string entityType, Guid entityId);
        Task<IEnumerable<ImageUploadDto>> GetActiveImagesByEntityAsync(string entityType, Guid entityId);

        // Update methods
        Task<bool> UpdateImageAsync(Guid id, UpdateImageUploadDto updateDto);

        // Delete methods
        Task<bool> DeleteImageAsync(Guid id);
        Task<bool> SoftDeleteImageAsync(Guid id);

        // Count methods
        Task<int> GetTotalImagesCountAsync();
        Task<int> GetImageCountByUserIdAsync(Guid userId);
    }
}
