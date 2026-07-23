using EXE201.HeartToHeart.DAL.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface ICounselorAvailabilityRepository
    {
        Task<CounselorAvailability?> GetCounselorAvailabilityForDateAsync(Guid counselorId, DateTime date);
        Task<IEnumerable<CounselorAvailability>> GetCounselorAvailabilityRangeAsync(Guid counselorId, DateTime fromDate, DateTime toDate);
        Task<bool> SetCounselorAvailabilityAsync(CounselorAvailability availability);
        Task<bool> UpdateCounselorAvailabilityAsync(CounselorAvailability availability);
        Task<bool> DeleteCounselorAvailabilityAsync(Guid availabilityId);
        Task<IEnumerable<CounselorScheduleTemplate>> GetCounselorScheduleTemplateAsync(Guid counselorId);
        Task<bool> SetCounselorScheduleTemplateAsync(CounselorScheduleTemplate template);
        Task<bool> UpdateCounselorScheduleTemplateAsync(CounselorScheduleTemplate template);
        Task<bool> DeleteCounselorScheduleTemplateAsync(Guid templateId);
    }
}
