using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IAIConversationService
    {
        Task<AIConversationDto> AddAsync(AIConversationDto dto);
        Task<IEnumerable<AIConversationDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<AIConversationDto?> GetByIdAsync(Guid id);
        Task<AIConversationDto> UpdateAsync(AIConversationDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
