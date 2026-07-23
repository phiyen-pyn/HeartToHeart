using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllByPostIdAsync(Guid postId);
        Task<Comment?> GetByIdAsync(Guid commentId);
        Task<bool> CreateAsync(Comment comment);
        Task<bool> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(Guid commentId);
    }
}
