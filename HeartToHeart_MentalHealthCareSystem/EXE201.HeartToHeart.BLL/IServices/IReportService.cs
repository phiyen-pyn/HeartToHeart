using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDto>> GetAllByPostIdAsync(Guid postId);
        Task<ReportDto?> GetByIdAsync(Guid reportId);
        Task<bool> CreateAsync(ReportDto dto);
        Task<bool> UpdateAsync(Guid reportId, ReportDto dto);
        Task<bool> DeleteAsync(Guid reportId);
    }
}
