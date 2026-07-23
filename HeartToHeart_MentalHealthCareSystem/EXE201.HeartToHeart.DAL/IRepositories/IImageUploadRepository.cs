using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IImageUploadRepository
    {
        Task<ImageUpload?> GetByIdAsync(Guid id);
        Task<IEnumerable<ImageUpload>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<IEnumerable<ImageUpload>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<ImageUpload>> GetByEntityAsync(string entityType, Guid entityId);
        Task<ImageUpload> CreateAsync(ImageUpload imageUpload);
        Task<bool> UpdateAsync(ImageUpload imageUpload);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByUserIdAsync(Guid userId);
        Task<IEnumerable<ImageUpload>> GetActiveByEntityAsync(string entityType, Guid entityId);
    }
}
