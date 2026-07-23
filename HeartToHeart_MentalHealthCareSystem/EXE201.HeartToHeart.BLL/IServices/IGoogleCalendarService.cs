using EXE201.HeartToHeart.DAL.Models;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.IServices
{
    public interface IGoogleCalendarService
    {
        Task<string?> CreateCalendarEventAsync(Guid userId, CreateAppointmentDto appointmentDto, string counselorName, string counselorEmail);
        Task<bool> UpdateCalendarEventAsync(string eventId, Guid userId, UpdateAppointmentDto updateDto);
        Task<bool> DeleteCalendarEventAsync(string eventId, Guid userId);
        Task<bool> CancelCalendarEventAsync(string eventId, Guid userId);
        Task<IEnumerable<Event>> GetCounselorBusyTimesAsync(string counselorEmail, DateTime startDate, DateTime endDate);
        Task<bool> SendAppointmentReminderAsync(string eventId, Guid userId, int minutesBefore = 60);
        Task<string?> GetAuthorizationUrlAsync(Guid userId);
        Task<bool> HandleAuthCallbackAsync(Guid userId, string authCode);
        Task<bool> IsUserAuthorizedAsync(Guid userId);

        // NEW: Add this method to the interface
        Task<string?> GetGoogleMeetLinkAsync(string eventId, Guid userId);
    }
}
