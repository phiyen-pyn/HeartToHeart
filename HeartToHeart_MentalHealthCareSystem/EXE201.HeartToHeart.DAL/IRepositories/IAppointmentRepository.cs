using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.IRepositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetAppointmentByIdAsync(Guid appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByUserIdAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Appointment>> GetAppointmentsByCounselorIdAsync(Guid counselorId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status, int page = 1, int pageSize = 10);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Appointment>> SearchAppointmentsAsync(AppointmentSearchDto searchDto);
        Task<IEnumerable<Appointment>> GetCounselorAppointmentsForDateAsync(Guid counselorId, DateTime date);
        Task<bool> HasConflictingAppointmentAsync(Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null);
        Task<bool> CreateAppointmentAsync(Appointment appointment);
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(Guid appointmentId);
        Task<bool> AppointmentExistsAsync(Guid appointmentId);

        // NEW: Methods for enhanced functionality
        Task<IEnumerable<Appointment>> GetCounselorAppointmentsForDateRangeAsync(Guid counselorId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(Guid userId, int days = 7);
        Task<IEnumerable<Appointment>> GetAppointmentHistoryAsync(Guid userId, int months = 3);
        Task<IEnumerable<Appointment>> GetCounselorTodayAppointmentsAsync(Guid counselorId);
        Task<bool> UserHasAppointmentWithCounselorAsync(Guid userId, Guid counselorId);
        Task<int> GetCounselorAppointmentCountForDateAsync(Guid counselorId, DateTime date);
        Task<Dictionary<string, int>> GetAppointmentStatisticsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // History tracking
        Task<bool> CreateAppointmentHistoryAsync(Guid appointmentId, string action, string oldStatus, string newStatus, Guid changedByUserId, string? notes = null);
        Task<IEnumerable<AppointmentHistory>> GetAppointmentHistoryAsync(Guid appointmentId);

        // Count
        Task<int> GetTotalAppointmentsCountAsync();
        Task<int> GetTotalCountByMonthAsync(int year, int month);
        Task<int> GetTotalAppointmentsCountByUserAsync(Guid userId);
        Task<int> GetTotalAppointmentsCountByCounselorAsync(Guid counselorId);
    }
}
