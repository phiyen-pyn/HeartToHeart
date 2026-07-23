using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId);
        Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<AppointmentDto>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10);
        Task<IEnumerable<AppointmentDto>> SearchAppointmentsAsync(AppointmentSearchDto searchDto);

        Task<(AppointmentDto? appointment, string? errorMessage)> CreateAppointmentAsync(Guid userId, CreateAppointmentDto createDto);

        Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentDto updateDto, Guid changedByUserId);
        Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, UpdateAppointmentStatusDto statusDto, Guid changedByUserId);
        Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid userId);
        Task<bool> DeleteAppointmentAsync(Guid appointmentId);

        Task<IEnumerable<CounselorAvailabilityDto>> GetCounselorAvailabilityAsync(DateTime date, string? specialization = null);
        Task<List<DateTime>> GetAvailableTimeSlotsAsync(Guid counselorId, DateTime date);

        Task<bool> ValidateAppointmentTimeAsync(Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null);
        Task<(bool isValid, string? errorMessage)> ValidateAppointmentTimeDetailedAsync(Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null);

        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetUserAppointmentsCountAsync(Guid userId);
        Task<int> GetCounselorAppointmentsCountAsync(Guid counselorId);

        Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(Guid userId, int days = 7);
        Task<IEnumerable<AppointmentDto>> GetAppointmentHistoryAsync(Guid userId, int months = 3);

        Task<bool> SetCounselorAvailabilityAsync(Guid counselorId, SetCounselorAvailabilityDto availabilityDto);
        Task<bool> SetCounselorScheduleTemplateAsync(Guid counselorId, IEnumerable<CounselorScheduleTemplateDto> scheduleTemplates);
        Task<CounselorStatusDto> GetCounselorStatusAsync(Guid counselorId);

        // NEW: Google Calendar integration methods
        Task<string?> GetGoogleCalendarAuthUrlAsync(Guid userId);
        Task<bool> HandleGoogleCalendarCallbackAsync(Guid userId, string authCode);
        Task<bool> IsGoogleCalendarAuthorizedAsync(Guid userId);
    }
}
