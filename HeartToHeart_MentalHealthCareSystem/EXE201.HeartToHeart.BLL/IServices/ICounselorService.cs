using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface ICounselorService
    {
        Task<(bool success, string message)> VerifyCounselorAsync(Guid counselorId, Guid verifiedByUserId);
        Task<(bool success, string message)> UnverifyCounselorAsync(Guid counselorId, Guid unverifiedByUserId);
        Task<(bool success, string message)> SetCounselorGeneralAvailabilityAsync(Guid counselorId, bool isAvailable, Guid updatedByUserId);
        Task<(bool success, string message)> UpdateCounselorProfileAsync(Guid counselorId, UpdateCounselorDto updateDto, Guid updatedByUserId);
        Task<CounselorDto?> GetCounselorAsync(Guid counselorId);
        Task<IEnumerable<CounselorDto>> GetCounselorsAsync(CounselorFilterDto? filter = null);
        Task<CounselorDto?> GetCounselorByUserIdAsync(Guid userId);
        Task<CounselorStatusDto> GetCounselorStatusAsync(Guid counselorId);
        Task<int> GetTotalCounselorsCountAsync();
        Task<int> GetTotalCounselorsCountByMonthAsync(int year, int month);
        Task<int> GetVerifiedCounselorsCountAsync();
        Task<int> GetVerifiedCounselorsCountByMonthAsync(int year, int month);
        Task<IEnumerable<AppointmentDto>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10);
    }

}
