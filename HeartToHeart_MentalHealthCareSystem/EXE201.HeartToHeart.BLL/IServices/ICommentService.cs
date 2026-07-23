using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetAllByPostIdAsync(List<string> role, Guid postId);
        Task<CommentDto?> GetByIdAsync(List<string> role, Guid commentId);
        Task<bool> CreateAsync(CommentDto dto);
        Task<bool> UpdateAsync(Guid commentId, CommentDto dto);
        Task<bool> DeleteAsync(Guid commentId);
    }
}
