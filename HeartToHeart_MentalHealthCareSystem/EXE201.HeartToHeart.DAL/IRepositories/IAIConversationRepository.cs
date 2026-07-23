using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IAIConversationRepository
    {
        Task<AIConversation> AddAsync(AIConversation conversation);
        Task<IEnumerable<AIConversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<AIConversation?> GetByIdAsync(Guid id);
        Task<AIConversation> UpdateAsync(AIConversation conversation);
        Task<bool> DeleteAsync(Guid id);
    }
}
