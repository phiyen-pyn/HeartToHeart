using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly ICounselorAvailabilityRepository _counselorAvailabilityRepository;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            ICounselorRepository counselorRepository,
            IUserRepository userRepository,
            IHolidayRepository holidayRepository,
            ICounselorAvailabilityRepository counselorAvailabilityRepository,
            IGoogleCalendarService googleCalendarService,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _counselorRepository = counselorRepository;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
            _counselorAvailabilityRepository = counselorAvailabilityRepository;
            _googleCalendarService = googleCalendarService;
            _logger = logger;
        }

        /// <summary>
        /// Maps appointments to DTOs and includes Google Meet links with better error handling
        /// </summary>
        private async Task<IEnumerable<AppointmentDto>> MapToAppointmentDtosWithMeetLinksAsync(IEnumerable<Appointment> appointments)
        {
            var appointmentDtos = new List<AppointmentDto>();

            foreach (var appointment in appointments)
            {
                var appointmentDto = MapToAppointmentDto(appointment);

                // Get Google Meet link if calendar event exists
                if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    try
                    {
                        var meetLink = await _googleCalendarService.GetGoogleMeetLinkAsync(
                            appointment.GoogleCalendarEventId, appointment.UserId);
                        appointmentDto.GoogleMeetLink = meetLink;

                        if (!string.IsNullOrEmpty(meetLink))
                        {
                            _logger.LogDebug("Successfully retrieved Google Meet link for appointment {AppointmentId}", appointment.Id);
                        }
                        else
                        {
                            _logger.LogWarning("No Google Meet link found for appointment {AppointmentId} with calendar event {EventId}",
                                appointment.Id, appointment.GoogleCalendarEventId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Google Meet link for appointment {AppointmentId} with calendar event {EventId}. " +
                            "This may be due to token expiration or calendar event not found.",
                            appointment.Id, appointment.GoogleCalendarEventId);

                        // Set to null to indicate no meet link available
                        appointmentDto.GoogleMeetLink = null;

                        // Continue without the meet link rather than failing the entire operation
                    }
                }
                else
                {
                    _logger.LogDebug("No Google Calendar event ID for appointment {AppointmentId}", appointment.Id);
                    appointmentDto.GoogleMeetLink = null;
                }

                appointmentDtos.Add(appointmentDto);
            }

            return appointmentDtos;
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null) return null;

                var appointmentDto = MapToAppointmentDto(appointment);

                // AUTOMATICALLY INCLUDE GOOGLE MEET LINK
                if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    try
                    {
                        var meetLink = await _googleCalendarService.GetGoogleMeetLinkAsync(
                            appointment.GoogleCalendarEventId, appointment.UserId);
                        appointmentDto.GoogleMeetLink = meetLink;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Google Meet link for appointment {AppointmentId}", appointmentId);
                        appointmentDto.GoogleMeetLink = null;
                    }
                }

                return appointmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment by ID: {AppointmentId}", appointmentId);
                return null;
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var appointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(userId, page, pageSize);
                return await MapToAppointmentDtosWithMeetLinksAsync(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user appointments: {UserId}", userId);
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetCounselorAppointmentsAsync(Guid counselorId, int page = 1, int pageSize = 10)
        {
            try
            {
                var appointments = await _appointmentRepository.GetAppointmentsByCounselorIdAsync(counselorId, page, pageSize);
                return await MapToAppointmentDtosWithMeetLinksAsync(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor appointments: {CounselorId}", counselorId);
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        public async Task<IEnumerable<AppointmentDto>> SearchAppointmentsAsync(AppointmentSearchDto searchDto)
        {
            try
            {
                var appointments = await _appointmentRepository.SearchAppointmentsAsync(searchDto);
                return appointments.Select(MapToAppointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching appointments");
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        public async Task<(AppointmentDto? appointment, string? errorMessage)> CreateAppointmentAsync(Guid userId, CreateAppointmentDto createDto)
        {
            try
            {
                _logger.LogInformation("Creating appointment for user {UserId} with counselor {CounselorId} at {AppointmentDate}",
                    userId, createDto.CounselorId, createDto.AppointmentDate);

                // Check if user is authorized with Google Calendar
                var isAuthorized = await _googleCalendarService.IsUserAuthorizedAsync(userId);
                if (!isAuthorized)
                {
                    var message = "Google Calendar authorization required. Please authorize access to your calendar first.";
                    _logger.LogWarning(message);
                    return (null, message);
                }

                // Validate user exists and is active
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    var message = user == null ? $"User not found: {userId}" : "User account is not active";
                    _logger.LogWarning(message);
                    return (null, message);
                }

                // Validate counselor exists and is available
                var counselor = await _counselorRepository.GetCounselorByIdAsync(createDto.CounselorId);
                if (counselor == null || !counselor.IsAvailable || !counselor.IsVerified || !counselor.User.IsActive)
                {
                    var message = counselor == null ? $"Counselor not found: {createDto.CounselorId}" : "Counselor is not available for bookings";
                    _logger.LogWarning(message);
                    return (null, message);
                }

                // FIXED: Parse appointment date properly - frontend sends Vietnam time without timezone info
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                DateTime appointmentDateUtc;

                // Frontend always sends Vietnam time as YYYY-MM-DDTHH:mm:ss format without timezone
                // Treat all incoming dates as Vietnam time and convert to UTC for storage
                if (createDto.AppointmentDate.Kind == DateTimeKind.Unspecified)
                {
                    // Frontend sends Vietnam time without timezone info, convert to UTC
                    appointmentDateUtc = TimeZoneInfo.ConvertTimeToUtc(createDto.AppointmentDate, vietnamTimeZone);
                    _logger.LogInformation("Converted Vietnam time {VietnamTime} to UTC {UtcTime}",
                        createDto.AppointmentDate, appointmentDateUtc);
                }
                else if (createDto.AppointmentDate.Kind == DateTimeKind.Local)
                {
                    // If local time, convert to UTC
                    appointmentDateUtc = createDto.AppointmentDate.ToUniversalTime();
                }
                else
                {
                    // Already UTC
                    appointmentDateUtc = createDto.AppointmentDate;
                }

                _logger.LogInformation("Appointment date conversion: Original = {Original}, UTC = {UTC}",
                    createDto.AppointmentDate, appointmentDateUtc);

                // Validate appointment time with detailed validation
                var (isValidTime, validationMessage) = await ValidateAppointmentTimeDetailedAsync(
                    createDto.CounselorId, appointmentDateUtc, createDto.DurationMinutes);

                if (!isValidTime)
                {
                    _logger.LogWarning("Invalid appointment time: {AppointmentDate}. Reason: {ValidationMessage}",
                        appointmentDateUtc, validationMessage);
                    return (null, validationMessage);
                }

                // Check user's appointment limits
                var userActiveAppointments = await GetActiveAppointmentCountForUserAsync(userId);
                if (userActiveAppointments >= 5)
                {
                    var message = "You have reached the maximum number of active appointments (5)";
                    _logger.LogWarning(message);
                    return (null, message);
                }

                // Create appointment in database first - store in UTC
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CounselorId = createDto.CounselorId,
                    AppointmentDate = appointmentDateUtc, // Store as UTC
                    Reason = createDto.Reason,
                    DurationMinutes = createDto.DurationMinutes,
                    Status = AppointmentConstants.Status.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var success = await _appointmentRepository.CreateAppointmentAsync(appointment);
                if (!success)
                {
                    var message = "Failed to create appointment in database";
                    _logger.LogError(message);
                    return (null, message);
                }

                // Create Google Calendar event with Meet link
                var counselorName = $"{counselor.User.FirstName} {counselor.User.LastName}";
                var counselorEmail = counselor.User.Email!;

                // Convert back to Vietnam time for Google Calendar (ensure DateTimeKind.Unspecified)
                var vietnamAppointmentDate = TimeZoneInfo.ConvertTimeFromUtc(appointmentDateUtc, vietnamTimeZone);
                // Specify as Unspecified to match frontend expectations
                vietnamAppointmentDate = DateTime.SpecifyKind(vietnamAppointmentDate, DateTimeKind.Unspecified);

                _logger.LogInformation("Sending Vietnam time to GoogleCalendarService: {VietnamTime} (Kind: {Kind})",
                    vietnamAppointmentDate, vietnamAppointmentDate.Kind);

                // Pass Vietnam time to calendar service
                var calendarDto = new CreateAppointmentDto
                {
                    CounselorId = createDto.CounselorId,
                    AppointmentDate = vietnamAppointmentDate, // Use Vietnam time for calendar service
                    Reason = createDto.Reason,
                    DurationMinutes = createDto.DurationMinutes
                };

                var calendarEventId = await _googleCalendarService.CreateCalendarEventAsync(userId, calendarDto, counselorName, counselorEmail);

                string? googleMeetLink = null;

                if (!string.IsNullOrEmpty(calendarEventId))
                {
                    // Update appointment with calendar event ID
                    appointment.GoogleCalendarEventId = calendarEventId;
                    await _appointmentRepository.UpdateAppointmentAsync(appointment);

                    _logger.LogInformation("Successfully created Google Calendar event {EventId} for appointment {AppointmentId}",
                        calendarEventId, appointment.Id);

                    // Get the Google Meet link
                    googleMeetLink = await _googleCalendarService.GetGoogleMeetLinkAsync(calendarEventId, userId);

                    if (!string.IsNullOrEmpty(googleMeetLink))
                    {
                        _logger.LogInformation("Google Meet link created: {MeetLink} for appointment {AppointmentId}",
                            googleMeetLink, appointment.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retrieve Google Meet link for appointment {AppointmentId}", appointment.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to create Google Calendar event for appointment {AppointmentId}", appointment.Id);
                }

                // Create history record
                await CreateAppointmentHistoryAsync(appointment.Id, "Created", "", AppointmentConstants.Status.Pending, userId,
                    "Appointment created with Google Calendar integration and Meet link");

                _logger.LogInformation("Successfully created appointment {AppointmentId} for user {UserId}", appointment.Id, userId);

                // Reload appointment with navigation properties
                var createdAppointment = await _appointmentRepository.GetAppointmentByIdAsync(appointment.Id);
                if (createdAppointment == null)
                {
                    return (null, "Failed to retrieve created appointment");
                }

                // Map to DTO and include Google Meet link
                var appointmentDto = MapToAppointmentDto(createdAppointment);
                appointmentDto.GoogleMeetLink = googleMeetLink;

                return (appointmentDto, null);
            }
            catch (Exception ex)
            {
                var message = $"Unexpected error creating appointment for user: {userId}";
                _logger.LogError(ex, message);
                return (null, "An unexpected error occurred while creating the appointment");
            }
        }

        public async Task<bool> UpdateAppointmentAsync(Guid appointmentId, UpdateAppointmentDto updateDto, Guid changedByUserId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found for update: {AppointmentId}", appointmentId);
                    return false;
                }

                // Capture original values for history tracking
                var originalStatus = appointment.Status;
                var originalDate = appointment.AppointmentDate;
                var originalDuration = appointment.DurationMinutes;

                // Check if appointment can be updated
                if (appointment.Status == AppointmentConstants.Status.Completed ||
                    appointment.Status == AppointmentConstants.Status.Cancelled)
                {
                    _logger.LogWarning("Cannot update appointment with status: {Status}", appointment.Status);
                    return false;
                }

                // Track what changed
                var changes = new List<string>();
                bool shouldUpdateCalendar = false;

                // Update appointment date if provided
                if (updateDto.AppointmentDate.HasValue && updateDto.AppointmentDate.Value != originalDate)
                {
                    var durationToUse = updateDto.DurationMinutes ?? appointment.DurationMinutes;
                    var (isValidTime, validationMessage) = await ValidateAppointmentTimeDetailedAsync(
                        appointment.CounselorId,
                        updateDto.AppointmentDate.Value,
                        durationToUse,
                        appointmentId);

                    if (!isValidTime)
                    {
                        _logger.LogWarning("Invalid appointment time for update: {AppointmentDate}. Reason: {ValidationMessage}",
                            updateDto.AppointmentDate.Value, validationMessage);
                        return false;
                    }

                    appointment.AppointmentDate = updateDto.AppointmentDate.Value;
                    appointment.Status = AppointmentConstants.Status.Rescheduled;
                    changes.Add($"Date changed from {originalDate:yyyy-MM-dd HH:mm} to {appointment.AppointmentDate:yyyy-MM-dd HH:mm}");
                    shouldUpdateCalendar = true;
                }

                // Update other properties
                if (!string.IsNullOrEmpty(updateDto.Reason) && updateDto.Reason != appointment.Reason)
                {
                    appointment.Reason = updateDto.Reason;
                    changes.Add("Reason updated");
                    shouldUpdateCalendar = true;
                }

                if (updateDto.DurationMinutes.HasValue && updateDto.DurationMinutes.Value != originalDuration)
                {
                    appointment.DurationMinutes = updateDto.DurationMinutes.Value;
                    changes.Add($"Duration changed from {originalDuration} to {appointment.DurationMinutes} minutes");
                    shouldUpdateCalendar = true;
                }

                if (!string.IsNullOrEmpty(updateDto.Notes))
                {
                    appointment.Notes = updateDto.Notes;
                    changes.Add("Notes updated");
                }

                appointment.UpdatedAt = DateTime.UtcNow;

                var success = await _appointmentRepository.UpdateAppointmentAsync(appointment);

                // Update Google Calendar event if necessary
                if (success && shouldUpdateCalendar && !string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    var calendarUpdateSuccess = await _googleCalendarService.UpdateCalendarEventAsync(
                        appointment.GoogleCalendarEventId, appointment.UserId, updateDto);

                    if (!calendarUpdateSuccess)
                    {
                        _logger.LogWarning("Failed to update Google Calendar event {EventId} for appointment {AppointmentId}",
                            appointment.GoogleCalendarEventId, appointmentId);
                    }
                }

                // Create history record if update was successful and changes were made
                if (success && changes.Any())
                {
                    await CreateAppointmentHistoryAsync(
                        appointmentId,
                        "Updated",
                        originalStatus,
                        appointment.Status,
                        changedByUserId,
                        string.Join("; ", changes));
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment: {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, UpdateAppointmentStatusDto statusDto, Guid changedByUserId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found for status update: {AppointmentId}", appointmentId);
                    return false;
                }

                if (!AppointmentConstants.IsValidStatus(statusDto.Status))
                {
                    _logger.LogWarning("Invalid appointment status: {Status}", statusDto.Status);
                    return false;
                }

                var oldStatus = appointment.Status;
                var newStatus = AppointmentConstants.GetNormalizedStatus(statusDto.Status);

                appointment.Status = newStatus;

                if (!string.IsNullOrEmpty(statusDto.Notes))
                    appointment.Notes = statusDto.Notes;

                appointment.UpdatedAt = DateTime.UtcNow;

                var success = await _appointmentRepository.UpdateAppointmentAsync(appointment);

                // Update Google Calendar event status
                if (success && !string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    if (newStatus == AppointmentConstants.Status.Cancelled)
                    {
                        await _googleCalendarService.CancelCalendarEventAsync(appointment.GoogleCalendarEventId, appointment.UserId);
                    }
                    // For other status changes, you might want to update the calendar event description or color
                }

                // Create history record
                if (success && oldStatus != newStatus)
                {
                    var notes = $"Status changed from {oldStatus} to {newStatus}";
                    if (!string.IsNullOrEmpty(statusDto.Notes))
                        notes += $". Notes: {statusDto.Notes}";

                    await CreateAppointmentHistoryAsync(
                        appointmentId,
                        "StatusChanged",
                        oldStatus,
                        newStatus,
                        changedByUserId,
                        notes);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status: {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<bool> CancelAppointmentAsync(Guid appointmentId, Guid cancelledByUserId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found for cancellation: {AppointmentId}", appointmentId);
                    return false;
                }

                // Check if user is authorized to cancel (user or counselor)
                if (appointment.UserId != cancelledByUserId && appointment.Counselor.UserId != cancelledByUserId)
                {
                    _logger.LogWarning("User not authorized to cancel appointment: {UserId}, {AppointmentId}", cancelledByUserId, appointmentId);
                    return false;
                }

                // Check if appointment can be cancelled
                if (appointment.Status == AppointmentConstants.Status.Completed ||
                    appointment.Status == AppointmentConstants.Status.Cancelled)
                {
                    _logger.LogWarning("Cannot cancel appointment with status: {Status}", appointment.Status);
                    return false;
                }

                // Check cancellation time policy (at least 2 hours before appointment)
                var hoursUntilAppointment = (appointment.AppointmentDate - DateTime.UtcNow).TotalHours;
                if (hoursUntilAppointment < 2)
                {
                    _logger.LogWarning("Cannot cancel appointment less than 2 hours before scheduled time");
                    return false;
                }

                var oldStatus = appointment.Status;
                appointment.Status = AppointmentConstants.Status.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;

                var success = await _appointmentRepository.UpdateAppointmentAsync(appointment);

                // Cancel Google Calendar event
                if (success && !string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    var calendarCancelSuccess = await _googleCalendarService.CancelCalendarEventAsync(
                        appointment.GoogleCalendarEventId, appointment.UserId);

                    if (!calendarCancelSuccess)
                    {
                        _logger.LogWarning("Failed to cancel Google Calendar event {EventId} for appointment {AppointmentId}",
                            appointment.GoogleCalendarEventId, appointmentId);
                    }
                }

                // Create history record
                if (success)
                {
                    var cancelledByText = appointment.UserId == cancelledByUserId ? "user" :
                                         appointment.Counselor.UserId == cancelledByUserId ? "counselor" : "admin";

                    await CreateAppointmentHistoryAsync(
                        appointmentId,
                        "Cancelled",
                        oldStatus,
                        AppointmentConstants.Status.Cancelled,
                        cancelledByUserId,
                        $"Appointment cancelled by {cancelledByText}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment: {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<bool> DeleteAppointmentAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null) return false;

                // Delete Google Calendar event first
                if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    await _googleCalendarService.DeleteCalendarEventAsync(appointment.GoogleCalendarEventId, appointment.UserId);
                }

                return await _appointmentRepository.DeleteAppointmentAsync(appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment: {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<IEnumerable<CounselorAvailabilityDto>> GetCounselorAvailabilityAsync(DateTime date, string? specialization = null)
        {
            try
            {
                var counselors = string.IsNullOrEmpty(specialization)
                    ? await _counselorRepository.GetAvailableCounselorsAsync()
                    : await _counselorRepository.GetCounselorsBySpecializationAsync(specialization);

                var availabilityList = new List<CounselorAvailabilityDto>();

                foreach (var counselor in counselors)
                {
                    // FIXED: Only include counselors that are generally available AND verified
                    if (!counselor.IsAvailable || !counselor.IsVerified || !counselor.User.IsActive)
                    {
                        _logger.LogDebug("Skipping counselor {CounselorId}: IsAvailable={IsAvailable}, IsVerified={IsVerified}, UserActive={UserActive}",
                            counselor.Id, counselor.IsAvailable, counselor.IsVerified, counselor.User.IsActive);
                        continue;
                    }

                    var availableSlots = await GetAvailableTimeSlotsAsync(counselor.Id, date);
                    var counselorAvailability = await _counselorAvailabilityRepository.GetCounselorAvailabilityForDateAsync(counselor.Id, date);

                    availabilityList.Add(new CounselorAvailabilityDto
                    {
                        CounselorId = counselor.Id,
                        CounselorName = $"{counselor.User.FirstName} {counselor.User.LastName}",
                        Specialization = counselor.Specialization,
                        HourlyRate = counselor.HourlyRate,
                        AvailableSlots = availableSlots,
                        CustomStartTime = counselorAvailability?.StartTime,
                        CustomEndTime = counselorAvailability?.EndTime,
                        IsAvailableOnDate = counselorAvailability?.IsAvailable ?? true,
                        UnavailabilityReason = counselorAvailability?.Notes
                    });
                }

                _logger.LogInformation("Found {Count} available counselors for date {Date}", availabilityList.Count, date);
                return availabilityList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor availability for date: {Date}", date);
                return Enumerable.Empty<CounselorAvailabilityDto>();
            }
        }
        public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(Guid counselorId, DateTime date)
        {
            try
            {
                var availableSlots = new List<DateTime>();

                // Check if it's a holiday
                var isHoliday = await _holidayRepository.IsHolidayAsync(date);
                if (isHoliday)
                {
                    _logger.LogInformation("No slots available on holiday: {Date}", date);
                    return availableSlots;
                }

                // Get counselor custom availability for this day
                var counselorAvailability = await _counselorAvailabilityRepository.GetCounselorAvailabilityForDateAsync(counselorId, date);

                // If counselor has set themselves unavailable for this day, return empty
                if (counselorAvailability != null && !counselorAvailability.IsAvailable)
                {
                    _logger.LogInformation("Counselor {CounselorId} is not available on {Date}", counselorId, date);
                    return availableSlots;
                }

                // Get existing appointments for the counselor on the specified date
                var existingAppointments = await _appointmentRepository.GetCounselorAppointmentsForDateAsync(counselorId, date);

                // Determine working hours (use custom if available, otherwise default)
                var startTime = counselorAvailability?.StartTime ?? TimeSpan.FromHours(AppointmentConstants.BusinessRules.WorkingHourStart);
                var endTime = counselorAvailability?.EndTime ?? TimeSpan.FromHours(AppointmentConstants.BusinessRules.WorkingHourEnd);

                // If no custom availability and it's Sunday, use default schedule template
                if (counselorAvailability == null && date.DayOfWeek == DayOfWeek.Sunday)
                {
                    var scheduleTemplate = await _counselorAvailabilityRepository.GetCounselorScheduleTemplateAsync(counselorId);
                    var sundayTemplate = scheduleTemplate.FirstOrDefault(st => st.DayOfWeek == 0);

                    if (sundayTemplate == null || !sundayTemplate.IsAvailable)
                    {
                        _logger.LogInformation("Counselor {CounselorId} is not available on Sundays", counselorId);
                        return availableSlots;
                    }

                    startTime = sundayTemplate.StartTime;
                    endTime = sundayTemplate.EndTime;
                }

                var workingStart = date.Date.Add(startTime);
                var workingEnd = date.Date.Add(endTime);

                for (var slot = workingStart; slot < workingEnd; slot = slot.AddMinutes(AppointmentConstants.BusinessRules.SlotIntervalMinutes))
                {
                    var slotEnd = slot.AddMinutes(AppointmentConstants.BusinessRules.DefaultDurationMinutes);

                    // Check if this slot conflicts with any existing appointment
                    var hasConflict = existingAppointments.Any(apt =>
                        slot < apt.AppointmentDate.AddMinutes(apt.DurationMinutes) &&
                        slotEnd > apt.AppointmentDate);

                    if (!hasConflict)
                    {
                        availableSlots.Add(slot);
                    }
                }

                return availableSlots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available time slots for counselor: {CounselorId}, Date: {Date}", counselorId, date);
                return new List<DateTime>();
            }
        }

        // Continue with validation methods and helper methods...
        public async Task<bool> ValidateAppointmentTimeAsync(Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null)
        {
            var (isValid, _) = await ValidateAppointmentTimeDetailedAsync(counselorId, appointmentDate, durationMinutes, excludeAppointmentId);
            return isValid;
        }

        public async Task<(bool isValid, string? errorMessage)> ValidateAppointmentTimeDetailedAsync(
            Guid counselorId, DateTime appointmentDate, int durationMinutes, Guid? excludeAppointmentId = null)
        {
            try
            {
                // FIXED: Add counselor verification and availability checks
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return (false, "Counselor not found");
                }

                if (!counselor.IsVerified)
                {
                    return (false, "Counselor is not verified");
                }

                if (!counselor.IsAvailable)
                {
                    return (false, "Counselor is not available for bookings");
                }

                if (!counselor.User.IsActive)
                {
                    return (false, "Counselor's user account is not active");
                }

                // FIXED: Proper timezone handling for validation
                DateTime localAppointmentDate;

                // If appointment date is in UTC, convert to Vietnam timezone
                if (appointmentDate.Kind == DateTimeKind.Utc)
                {
                    var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    localAppointmentDate = TimeZoneInfo.ConvertTimeFromUtc(appointmentDate, vietnamTimeZone);
                }
                else
                {
                    // If it's unspecified or local, treat as Vietnam time
                    localAppointmentDate = appointmentDate;
                }

                _logger.LogDebug("Validating appointment time: Original = {OriginalTime} ({Kind}), Vietnam Local = {LocalTime}",
                    appointmentDate, appointmentDate.Kind, localAppointmentDate);

                // Check if appointment date is in the future (compare in Vietnam timezone)
                var currentVietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                if (localAppointmentDate <= currentVietnamTime)
                {
                    return (false, "Appointment date must be in the future");
                }

                // Check minimum advance booking time (24 hours) - use Vietnam timezone
                var hoursUntilAppointment = (localAppointmentDate - currentVietnamTime).TotalHours;
                if (hoursUntilAppointment < AppointmentConstants.BusinessRules.MinimumAdvanceBookingHours)
                {
                    return (false, $"Appointment must be booked at least {AppointmentConstants.BusinessRules.MinimumAdvanceBookingHours} hours in advance");
                }

                // Check maximum advance booking time (90 days)
                var daysUntilAppointment = (localAppointmentDate - currentVietnamTime).TotalDays;
                if (daysUntilAppointment > AppointmentConstants.BusinessRules.MaximumAdvanceBookingDays)
                {
                    return (false, $"Appointment cannot be booked more than {AppointmentConstants.BusinessRules.MaximumAdvanceBookingDays} days in advance");
                }

                // Check duration limits
                if (durationMinutes < AppointmentConstants.BusinessRules.MinimumDurationMinutes ||
                    durationMinutes > AppointmentConstants.BusinessRules.MaximumDurationMinutes)
                {
                    return (false, $"Duration must be between {AppointmentConstants.BusinessRules.MinimumDurationMinutes} and {AppointmentConstants.BusinessRules.MaximumDurationMinutes} minutes");
                }

                // Check if it's a holiday (use Vietnam date)
                var isHoliday = await _holidayRepository.IsHolidayAsync(localAppointmentDate.Date);
                if (isHoliday)
                {
                    var holidayInfo = await _holidayRepository.GetHolidayInfoAsync(localAppointmentDate.Date);
                    return (false, $"Appointments cannot be booked on {holidayInfo?.Name ?? "holidays"}");
                }

                // Get counselor availability for this day (use Vietnam date)
                var counselorAvailability = await _counselorAvailabilityRepository.GetCounselorAvailabilityForDateAsync(counselorId, localAppointmentDate.Date);

                // Check if counselor is available on this day
                if (counselorAvailability != null && !counselorAvailability.IsAvailable)
                {
                    return (false, "Counselor is not available on this date");
                }

                // Check counselor's weekly schedule template if no specific availability is set
                if (counselorAvailability == null)
                {
                    var scheduleTemplates = await _counselorAvailabilityRepository.GetCounselorScheduleTemplateAsync(counselorId);
                    var dayTemplate = scheduleTemplates.FirstOrDefault(st => st.DayOfWeek == (int)localAppointmentDate.DayOfWeek);

                    if (dayTemplate != null && !dayTemplate.IsAvailable)
                    {
                        return (false, $"Counselor is not available on {localAppointmentDate.DayOfWeek}s");
                    }
                }

                // FIXED: Check if appointment is during working hours using Vietnam time
                var appointmentTime = localAppointmentDate.TimeOfDay;
                var workingStart = counselorAvailability?.StartTime ?? TimeSpan.FromHours(AppointmentConstants.BusinessRules.WorkingHourStart);
                var workingEnd = counselorAvailability?.EndTime ?? TimeSpan.FromHours(AppointmentConstants.BusinessRules.WorkingHourEnd);

                // Use schedule template if no custom availability
                if (counselorAvailability == null)
                {
                    var scheduleTemplates = await _counselorAvailabilityRepository.GetCounselorScheduleTemplateAsync(counselorId);
                    var dayTemplate = scheduleTemplates.FirstOrDefault(st => st.DayOfWeek == (int)localAppointmentDate.DayOfWeek);

                    if (dayTemplate != null)
                    {
                        workingStart = dayTemplate.StartTime;
                        workingEnd = dayTemplate.EndTime;
                    }
                }

                if (appointmentTime < workingStart || appointmentTime >= workingEnd)
                {
                    return (false, $"Appointment must be during working hours ({workingStart:hh\\:mm} - {workingEnd:hh\\:mm})");
                }

                // Check if appointment ends within working hours
                var appointmentEndTime = localAppointmentDate.AddMinutes(durationMinutes).TimeOfDay;
                if (appointmentEndTime > workingEnd)
                {
                    return (false, "Appointment duration exceeds working hours");
                }

                // FIXED: Check for conflicting appointments using original UTC time
                var hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
                    counselorId, appointmentDate, durationMinutes, excludeAppointmentId);

                if (hasConflict)
                {
                    return (false, "This time slot conflicts with an existing appointment");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating appointment time");
                return (false, "An error occurred while validating the appointment time");
            }
        }

        public async Task<int> GetTotalAppointmentsCountAsync()
        {
            try
            {
                return await _appointmentRepository.GetTotalAppointmentsCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total appointments count");
                return 0;
            }
        }

        public async Task<int> GetTotalCountByMonthAsync(int year, int month)
        {
            try
            {
                return await _appointmentRepository.GetTotalCountByMonthAsync(year, month);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total appointments count for {Year}-{Month}", year, month);
                return 0;
            }
        }

        public async Task<int> GetUserAppointmentsCountAsync(Guid userId)
        {
            try
            {
                return await _appointmentRepository.GetTotalAppointmentsCountByUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user appointments count: {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetCounselorAppointmentsCountAsync(Guid counselorId)
        {
            try
            {
                return await _appointmentRepository.GetTotalAppointmentsCountByCounselorAsync(counselorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor appointments count: {CounselorId}", counselorId);
                return 0;
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(Guid userId, int days = 7)
        {
            try
            {
                var appointments = await _appointmentRepository.GetUpcomingAppointmentsAsync(userId, days);
                return appointments.Select(MapToAppointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming appointments for user: {UserId}", userId);
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointmentHistoryAsync(Guid userId, int months = 3)
        {
            try
            {
                var appointments = await _appointmentRepository.GetAppointmentHistoryAsync(userId, months);
                return appointments.Select(MapToAppointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment history for user: {UserId}", userId);
                return Enumerable.Empty<AppointmentDto>();
            }
        }

        public async Task<bool> SetCounselorAvailabilityAsync(Guid counselorId, SetCounselorAvailabilityDto availabilityDto)
        {
            try
            {
                // FIXED: Validate that counselor exists and is verified
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    _logger.LogWarning("Cannot set availability: Counselor not found {CounselorId}", counselorId);
                    return false;
                }

                if (!counselor.IsVerified)
                {
                    _logger.LogWarning("Cannot set availability: Counselor not verified {CounselorId}", counselorId);
                    return false;
                }

                var availability = new CounselorAvailability
                {
                    Id = Guid.NewGuid(), // FIXED: Ensure ID is set
                    CounselorId = counselorId,
                    Date = availabilityDto.Date.Date,
                    IsAvailable = availabilityDto.IsAvailable,
                    StartTime = availabilityDto.StartTime,
                    EndTime = availabilityDto.EndTime,
                    Notes = availabilityDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _counselorAvailabilityRepository.SetCounselorAvailabilityAsync(availability);

                if (result)
                {
                    _logger.LogInformation("Set availability for counselor {CounselorId} on {Date}: Available={IsAvailable}",
                        counselorId, availabilityDto.Date.Date, availabilityDto.IsAvailable);
                }
                else
                {
                    _logger.LogWarning("Failed to set availability for counselor {CounselorId} on {Date}",
                        counselorId, availabilityDto.Date.Date);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counselor availability for {CounselorId} on {Date}", counselorId, availabilityDto.Date);
                return false;
            }
        }

        public async Task<CounselorStatusDto> GetCounselorStatusAsync(Guid counselorId)
        {
            try
            {
                var counselor = await _counselorRepository.GetCounselorByIdAsync(counselorId);
                if (counselor == null)
                {
                    return new CounselorStatusDto
                    {
                        CounselorId = counselorId,
                        Exists = false,
                        IsVerified = false,
                        IsAvailable = false,
                        IsUserActive = false,
                        Message = "Counselor not found"
                    };
                }

                return new CounselorStatusDto
                {
                    CounselorId = counselorId,
                    Exists = true,
                    IsVerified = counselor.IsVerified,
                    IsAvailable = counselor.IsAvailable,
                    IsUserActive = counselor.User?.IsActive ?? false,
                    Message = GetCounselorStatusMessage(counselor)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor status for {CounselorId}", counselorId);
                return new CounselorStatusDto
                {
                    CounselorId = counselorId,
                    Exists = false,
                    Message = "Error retrieving counselor status"
                };
            }
        }

        private string GetCounselorStatusMessage(Counselor counselor)
        {
            if (!counselor.User.IsActive)
                return "User account is inactive";

            if (!counselor.IsVerified)
                return "Counselor is not verified";

            if (!counselor.IsAvailable)
                return "Counselor is not available for bookings";

            return "Counselor is active and available";
        }

        public async Task<bool> SetCounselorScheduleTemplateAsync(Guid counselorId, IEnumerable<CounselorScheduleTemplateDto> scheduleTemplates)
        {
            try
            {
                foreach (var template in scheduleTemplates)
                {
                    var scheduleTemplate = new CounselorScheduleTemplate
                    {
                        CounselorId = counselorId,
                        DayOfWeek = template.DayOfWeek,
                        IsAvailable = template.IsAvailable,
                        StartTime = template.StartTime,
                        EndTime = template.EndTime
                    };

                    await _counselorAvailabilityRepository.SetCounselorScheduleTemplateAsync(scheduleTemplate);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting counselor schedule template for {CounselorId}", counselorId);
                return false;
            }
        }

        // Google Calendar specific methods
        public async Task<string?> GetGoogleCalendarAuthUrlAsync(Guid userId)
        {
            return await _googleCalendarService.GetAuthorizationUrlAsync(userId);
        }

        public async Task<bool> HandleGoogleCalendarCallbackAsync(Guid userId, string authCode)
        {
            return await _googleCalendarService.HandleAuthCallbackAsync(userId, authCode);
        }

        public async Task<bool> IsGoogleCalendarAuthorizedAsync(Guid userId)
        {
            return await _googleCalendarService.IsUserAuthorizedAsync(userId);
        }

        private async Task<int> GetActiveAppointmentCountForUserAsync(Guid userId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(userId, 1, 100);
            return appointments.Count(a => a.Status == AppointmentConstants.Status.Pending ||
                                         a.Status == AppointmentConstants.Status.Confirmed ||
                                         a.Status == AppointmentConstants.Status.Rescheduled);
        }

        private async Task CreateAppointmentHistoryAsync(Guid appointmentId, string action, string oldStatus, string newStatus, Guid changedByUserId, string? notes = null)
        {
            try
            {
                await _appointmentRepository.CreateAppointmentHistoryAsync(
                    appointmentId, action, oldStatus, newStatus, changedByUserId, notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create appointment history for {AppointmentId}", appointmentId);
                // Don't fail the main operation if history creation fails
            }
        }

        public async Task<AppointmentDto?> GetAppointmentWithMeetLinkAsync(Guid appointmentId)
        {
            try
            {
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null) return null;

                var appointmentDto = MapToAppointmentDto(appointment);

                // Get Google Meet link if calendar event exists
                if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
                {
                    var meetLink = await _googleCalendarService.GetGoogleMeetLinkAsync(
                        appointment.GoogleCalendarEventId, appointment.UserId);
                    appointmentDto.GoogleMeetLink = meetLink;
                }

                return appointmentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment with meet link: {AppointmentId}", appointmentId);
                return null;
            }
        }
        private static AppointmentDto MapToAppointmentDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                CounselorId = appointment.CounselorId,
                AppointmentDate = TimeZoneInfo.ConvertTimeFromUtc(appointment.AppointmentDate, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                Reason = appointment.Reason,
                Status = appointment.Status,
                Notes = appointment.Notes,
                DurationMinutes = appointment.DurationMinutes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt,
                UserName = $"{appointment.User.FirstName} {appointment.User.LastName}",
                UserEmail = appointment.User.Email,
                CounselorName = $"{appointment.Counselor.User.FirstName} {appointment.Counselor.User.LastName}",
                CounselorSpecialization = appointment.Counselor.Specialization,
                GoogleCalendarEventId = appointment.GoogleCalendarEventId,
                GoogleMeetLink = null // Will be populated separately if needed
            };
        }
    }
}