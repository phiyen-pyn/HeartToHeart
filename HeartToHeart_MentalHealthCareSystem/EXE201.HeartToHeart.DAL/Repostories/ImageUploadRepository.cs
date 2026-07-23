using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class ImageUploadRepository : IImageUploadRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageUploadRepository> _logger;

        public ImageUploadRepository(ApplicationDbContext context, ILogger<ImageUploadRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ImageUpload?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.ImageUploads
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image upload by ID: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<ImageUpload>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                return await _context.ImageUploads
                    .Include(i => i.User)
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all image uploads. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return Enumerable.Empty<ImageUpload>();
            }
        }

        public async Task<IEnumerable<ImageUpload>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                return await _context.ImageUploads
                    .Include(i => i.User)
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image uploads by user ID: {UserId}", userId);
                return Enumerable.Empty<ImageUpload>();
            }
        }

        public async Task<IEnumerable<ImageUpload>> GetByEntityAsync(string entityType, Guid entityId)
        {
            try
            {
                return await _context.ImageUploads
                    .Include(i => i.User)
                    .Where(i => i.EntityType == entityType && i.EntityId == entityId)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image uploads by entity: {EntityType}, {EntityId}", entityType, entityId);
                return Enumerable.Empty<ImageUpload>();
            }
        }

        public async Task<IEnumerable<ImageUpload>> GetActiveByEntityAsync(string entityType, Guid entityId)
        {
            try
            {
                return await _context.ImageUploads
                    .Include(i => i.User)
                    .Where(i => i.EntityType == entityType && i.EntityId == entityId && i.IsActive)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active image uploads by entity: {EntityType}, {EntityId}", entityType, entityId);
                return Enumerable.Empty<ImageUpload>();
            }
        }

        public async Task<ImageUpload> CreateAsync(ImageUpload imageUpload)
        {
            try
            {
                _context.ImageUploads.Add(imageUpload);
                await _context.SaveChangesAsync();
                return imageUpload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating image upload");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ImageUpload imageUpload)
        {
            try
            {
                _context.ImageUploads.Update(imageUpload);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image upload: {Id}", imageUpload.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var imageUpload = await _context.ImageUploads.FindAsync(id);
                if (imageUpload == null)
                    return false;

                _context.ImageUploads.Remove(imageUpload);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image upload: {Id}", id);
                return false;
            }
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            try
            {
                var imageUpload = await _context.ImageUploads.FindAsync(id);
                if (imageUpload == null)
                    return false;

                imageUpload.IsActive = false;
                imageUpload.UpdatedAt = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting image upload: {Id}", id);
                return false;
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _context.ImageUploads.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total image uploads count");
                return 0;
            }
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.ImageUploads
                    .Where(i => i.UserId == userId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image uploads count by user ID: {UserId}", userId);
                return 0;
            }
        }
    }
}
