using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Repostories
{
    public class AIConversationRepository : IAIConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public AIConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AIConversation> AddAsync(AIConversation conversation)
        {
            _context.AIConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<IEnumerable<AIConversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.AIConversations
                .Where(c => c.UserId == userId  && c.IsActive == true)
                .OrderBy(c => c.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<AIConversation?> GetByIdAsync(Guid id)
        {
            return await _context.AIConversations
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<AIConversation> UpdateAsync(AIConversation conversation)
        {
            _context.AIConversations.Update(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var conversation = await _context.AIConversations.FindAsync(id);
            if (conversation == null)
                return false;

            _context.AIConversations.Remove(conversation);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
