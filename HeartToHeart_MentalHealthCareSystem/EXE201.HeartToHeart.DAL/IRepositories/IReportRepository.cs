using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IReportRepository
    {
        Task<Report?> GetByIdAsync(Guid reportId);
        Task<IEnumerable<Report>> GetAllByPostIdAsync(Guid postId);
        Task<bool> CreateAsync(Report report);
        Task<bool> UpdateAsync(Report report);
        Task<bool> DeleteAsync(Guid reportId);
    }
}
