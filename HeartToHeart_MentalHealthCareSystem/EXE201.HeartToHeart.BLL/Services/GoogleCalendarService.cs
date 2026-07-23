using EXE201.HeartToHeart.BLL.IServices;
using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.IRepositories;
using EXE201.HeartToHeart.DAL.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EXE201.HeartToHeart.BLL.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly IGoogleCalendarTokenRepository _tokenRepository;
        private readonly ICounselorRepository _counselorRepository;
        private readonly IUserRepository _userRepository;

        private readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private readonly string ApplicationName = "HeartToHeart";

        public GoogleCalendarService(
            IConfiguration configuration,
            ILogger<GoogleCalendarService> logger,
            IGoogleCalendarTokenRepository tokenRepository,
            ICounselorRepository counselorRepository,
            IUserRepository userRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _tokenRepository = tokenRepository;
            _counselorRepository = counselorRepository;
            _userRepository = userRepository;
        }

        public async Task<string?> CreateCalendarEventAsync(Guid userId, CreateAppointmentDto appointmentDto, string counselorName, string counselorEmail)
        {
            try
            {
                _logger.LogInformation("Creating Google Calendar event for user {UserId} with counselor {CounselorName}", userId, counselorName);

                var service = await GetCalendarServiceAsync(userId);
                if (service == null)
                {
                    _logger.LogError("Failed to get calendar service for user {UserId}", userId);
                    return null;
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", userId);
                    return null;
                }

                // FIXED: AppointmentService already converts to Vietnam time before passing here
                // Just use the provided time directly without additional conversion
                DateTime vietnamStartTime = appointmentDto.AppointmentDate;

                _logger.LogInformation("Using appointment date directly as Vietnam time: {VietnamTime} (Kind: {Kind})",
                    vietnamStartTime, appointmentDto.AppointmentDate.Kind);

                var vietnamEndTime = vietnamStartTime.AddMinutes(appointmentDto.DurationMinutes);

                _logger.LogInformation("Creating calendar event: Vietnam Start = {VietnamStart}, Vietnam End = {VietnamEnd}, Original Kind = {Kind}",
                    vietnamStartTime, vietnamEndTime, appointmentDto.AppointmentDate.Kind);

                // FIXED: Convert Vietnam time to UTC and use RFC3339 format for Google Calendar
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var utcStartTime = TimeZoneInfo.ConvertTimeToUtc(vietnamStartTime, vietnamTimeZone);
                var utcEndTime = TimeZoneInfo.ConvertTimeToUtc(vietnamEndTime, vietnamTimeZone);

                _logger.LogInformation("Converting to UTC for Google Calendar - Vietnam Start: {VietnamStart}, UTC Start: {UtcStart}, Vietnam End: {VietnamEnd}, UTC End: {UtcEnd}",
                    vietnamStartTime, utcStartTime, vietnamEndTime, utcEndTime);

                var calendarEvent = new Event()
                {
                    Summary = $"Tư vấn tâm lý với {counselorName}",
                    Description = $"Phiên tư vấn tâm lý\n\nLý do: {appointmentDto.Reason}\n\nThời lượng: {appointmentDto.DurationMinutes} phút\n\nTham gia cuộc họp bằng liên kết Google Meet bên dưới.",
                    Start = new EventDateTime()
                    {
                        DateTime = utcStartTime,
                        // No timezone specified means UTC
                    },
                    End = new EventDateTime()
                    {
                        DateTime = utcEndTime,
                        // No timezone specified means UTC
                    },
                    Attendees = new EventAttendee[]
                    {
                        new EventAttendee()
                        {
                            Email = user.Email,
                            DisplayName = $"{user.FirstName} {user.LastName}",
                            ResponseStatus = "accepted"
                        },
                        new EventAttendee()
                        {
                            Email = counselorEmail,
                            DisplayName = counselorName,
                            ResponseStatus = "needsAction"
                        }
                    },
                    Reminders = new Event.RemindersData()
                    {
                        UseDefault = false,
                        Overrides = new EventReminder[]
                        {
                            new EventReminder() { Method = "email", Minutes = 60 },
                            new EventReminder() { Method = "popup", Minutes = 15 }
                        }
                    },
                    ConferenceData = new ConferenceData()
                    {
                        CreateRequest = new CreateConferenceRequest()
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey()
                            {
                                Type = "hangoutsMeet"
                            }
                        }
                    },
                    Location = "Online - Google Meet",
                    ColorId = "2", // Green color for counseling sessions
                    Visibility = "private",
                    Status = "confirmed"
                };

                var request = service.Events.Insert(calendarEvent, "primary");
                request.ConferenceDataVersion = 1; // Enable Google Meet integration
                request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;

                var createdEvent = await request.ExecuteAsync();

                _logger.LogInformation("Successfully created Google Calendar event {EventId} for user {UserId}", createdEvent.Id, userId);

                // Log Google Meet link if created
                if (createdEvent.ConferenceData?.EntryPoints != null)
                {
                    foreach (var entryPoint in createdEvent.ConferenceData.EntryPoints)
                    {
                        if (entryPoint.EntryPointType == "video")
                        {
                            _logger.LogInformation("Google Meet link created: {MeetLink} for event {EventId}", entryPoint.Uri, createdEvent.Id);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("No Google Meet link was created for event {EventId}", createdEvent.Id);
                }

                return createdEvent.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating calendar event for user {UserId}", userId);
                return null;
            }
        }

        public async Task<string?> GetAuthorizationUrlAsync(Guid userId)
        {
            try
            {
                // Validate configuration
                var clientId = _configuration["GoogleCalendar:ClientId"];
                var clientSecret = _configuration["GoogleCalendar:ClientSecret"];
                var redirectUri = _configuration["GoogleCalendar:RedirectUri"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                {
                    _logger.LogError("Google Calendar configuration is missing. ClientId: {HasClientId}, ClientSecret: {HasClientSecret}, RedirectUri: {RedirectUri}",
                        !string.IsNullOrEmpty(clientId), !string.IsNullOrEmpty(clientSecret), redirectUri);
                    return null;
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes,
                    DataStore = new GoogleCalendarTokenStore(_tokenRepository)
                });

                // Create the authorization code request
                var authRequestUrl = flow.CreateAuthorizationCodeRequest(redirectUri);
                authRequestUrl.State = userId.ToString();

                // Build the URL first
                var baseUrl = authRequestUrl.Build();

                // Add additional parameters using UriBuilder to avoid duplication
                var uriBuilder = new UriBuilder(baseUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

                // Only add parameters if they don't already exist to avoid duplication
                if (!query.AllKeys.Contains("access_type"))
                {
                    query["access_type"] = "offline"; // Request offline access for refresh tokens
                }
                if (!query.AllKeys.Contains("prompt"))
                {
                    query["prompt"] = "consent"; // Force consent screen
                }
                if (!query.AllKeys.Contains("include_granted_scopes"))
                {
                    query["include_granted_scopes"] = "true"; // Include previously granted scopes
                }

                uriBuilder.Query = query.ToString();
                var finalUrl = uriBuilder.ToString();

                _logger.LogInformation("Generated authorization URL for user {UserId}. Using RedirectUri: {RedirectUri}", userId, redirectUri);
                return finalUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating authorization URL for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> HandleAuthCallbackAsync(Guid userId, string authCode)
        {
            try
            {
                _logger.LogInformation("Processing auth callback for user {UserId} with code length: {CodeLength}", userId, authCode?.Length ?? 0);

                if (string.IsNullOrEmpty(authCode))
                {
                    _logger.LogError("Authorization code is null or empty for user {UserId}", userId);
                    return false;
                }

                // Validate configuration
                var clientId = _configuration["GoogleCalendar:ClientId"];
                var clientSecret = _configuration["GoogleCalendar:ClientSecret"];
                var redirectUri = _configuration["GoogleCalendar:RedirectUri"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                {
                    _logger.LogError("Google Calendar configuration is missing during callback for user {UserId}", userId);
                    return false;
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes,
                    DataStore = new GoogleCalendarTokenStore(_tokenRepository)
                });

                _logger.LogInformation("Using redirect URI for token exchange: {RedirectUri}", redirectUri);

                var token = await flow.ExchangeCodeForTokenAsync(userId.ToString(), authCode, redirectUri, CancellationToken.None);

                if (token == null)
                {
                    _logger.LogError("Token exchange returned null for user {UserId}", userId);
                    return false;
                }

                // Log token details for debugging (without sensitive data)
                _logger.LogInformation("Token exchange successful for user {UserId}. AccessToken length: {AccessTokenLength}, RefreshToken present: {HasRefreshToken}, ExpiresIn: {ExpiresIn}",
                    userId, token.AccessToken?.Length ?? 0, !string.IsNullOrEmpty(token.RefreshToken), token.ExpiresInSeconds);

                // ENHANCED: Handle case where refresh token might be null
                var refreshToken = token.RefreshToken;
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("No refresh token received for user {UserId}. This might happen if user was already authorized. Checking existing token.", userId);
                    var existingToken = await _tokenRepository.GetTokenAsync(userId);
                    if (existingToken != null && !string.IsNullOrEmpty(existingToken.RefreshToken))
                    {
                        refreshToken = existingToken.RefreshToken;
                        _logger.LogInformation("Using existing refresh token for user {UserId}", userId);
                    }
                    else
                    {
                        _logger.LogWarning("No refresh token available for user {UserId}. User may need to re-authorize with prompt=consent", userId);
                    }
                }

                var saveResult = await _tokenRepository.SaveTokenAsync(userId, token.AccessToken, refreshToken, token.ExpiresInSeconds);

                if (saveResult)
                {
                    _logger.LogInformation("Successfully saved Google Calendar token for user {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to save Google Calendar token for user {UserId}", userId);
                    return false;
                }
            }
            catch (Google.GoogleApiException gex)
            {
                _logger.LogError(gex, "Google API error handling auth callback for user {UserId}: {Error}", userId, gex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling auth callback for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsUserAuthorizedAsync(Guid userId)
        {
            try
            {
                var token = await _tokenRepository.GetTokenAsync(userId);
                if (token == null || token.IsRevoked)
                {
                    _logger.LogInformation("No valid token found for user {UserId}", userId);
                    return false;
                }

                // Check if token is expired and try to refresh automatically
                if (token.IsExpiredWithBuffer(5)) // 5 minute buffer
                {
                    _logger.LogInformation("Token expired for user {UserId}, attempting to refresh", userId);

                    var refreshed = await RefreshTokenAsync(userId, token);
                    if (refreshed)
                    {
                        _logger.LogInformation("Successfully refreshed token for user {UserId}", userId);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to refresh token for user {UserId}", userId);
                        return false;
                    }
                }

                _logger.LogDebug("User {UserId} has valid Google Calendar authorization", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authorization for user {UserId}", userId);
                return false;
            }
        }

        public async Task<string?> GetGoogleMeetLinkAsync(string eventId, Guid userId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                if (service == null)
                {
                    _logger.LogWarning("Failed to get calendar service for user {UserId}", userId);
                    return null;
                }

                var calendarEvent = await service.Events.Get("primary", eventId).ExecuteAsync();
                if (calendarEvent == null)
                {
                    _logger.LogWarning("Calendar event {EventId} not found", eventId);
                    return null;
                }

                // Check for Google Meet link in conference data
                if (calendarEvent.ConferenceData?.EntryPoints != null)
                {
                    foreach (var entryPoint in calendarEvent.ConferenceData.EntryPoints)
                    {
                        if (entryPoint.EntryPointType == "video" && !string.IsNullOrEmpty(entryPoint.Uri))
                        {
                            _logger.LogDebug("Found Google Meet link for event {EventId}: {MeetLink}", eventId, entryPoint.Uri);
                            return entryPoint.Uri;
                        }
                    }
                }

                // Alternative: Check for hangout link (older format)
                if (!string.IsNullOrEmpty(calendarEvent.HangoutLink))
                {
                    _logger.LogDebug("Found Hangout link for event {EventId}: {HangoutLink}", eventId, calendarEvent.HangoutLink);
                    return calendarEvent.HangoutLink;
                }

                _logger.LogDebug("No Google Meet link found for event {EventId}", eventId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google Meet link for event {EventId}", eventId);
                return null;
            }
        }

        public async Task<bool> UpdateCalendarEventAsync(string eventId, Guid userId, UpdateAppointmentDto updateDto)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                if (service == null) return false;

                var existingEvent = await service.Events.Get("primary", eventId).ExecuteAsync();
                if (existingEvent == null) return false;

                bool updated = false;

                if (updateDto.AppointmentDate.HasValue)
                {
                    // FIXED: AppointmentService already converts to Vietnam time before passing here
                    // Just use the provided time directly without additional conversion
                    DateTime vietnamStartTime = updateDto.AppointmentDate.Value;

                    _logger.LogInformation("Update: Using appointment date directly as Vietnam time: {VietnamTime} (Kind: {Kind})",
                        vietnamStartTime, updateDto.AppointmentDate.Value.Kind);

                    var duration = updateDto.DurationMinutes ?? 60;
                    var vietnamEndTime = vietnamStartTime.AddMinutes(duration);

                    existingEvent.Start = new EventDateTime()
                    {
                        DateTime = vietnamStartTime,
                        TimeZone = "Asia/Ho_Chi_Minh",
                    };
                    existingEvent.End = new EventDateTime()
                    {
                        DateTime = vietnamEndTime,
                        TimeZone = "Asia/Ho_Chi_Minh",
                    };
                    updated = true;

                    _logger.LogInformation("Updated appointment time to {VietnamTime} for event {EventId}", vietnamStartTime, eventId);
                }

                if (!string.IsNullOrEmpty(updateDto.Reason))
                {
                    existingEvent.Description = $"Phiên tư vấn tâm lý\n\nLý do: {updateDto.Reason}\n\nThời lượng: {updateDto.DurationMinutes ?? 60} phút\n\nTham gia cuộc họp bằng liên kết Google Meet bên dưới.";
                    updated = true;
                }

                if (updated)
                {
                    var request = service.Events.Update(existingEvent, "primary", eventId);
                    request.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
                    await request.ExecuteAsync();

                    _logger.LogInformation("Updated Google Calendar event {EventId}", eventId);
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating calendar event {EventId}", eventId);
                return false;
            }
        }

        public async Task<bool> DeleteCalendarEventAsync(string eventId, Guid userId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                if (service == null) return false;

                await service.Events.Delete("primary", eventId).ExecuteAsync();

                _logger.LogInformation("Deleted Google Calendar event {EventId}", eventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting calendar event {EventId}", eventId);
                return false;
            }
        }

        public async Task<bool> CancelCalendarEventAsync(string eventId, Guid userId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                if (service == null) return false;

                var existingEvent = await service.Events.Get("primary", eventId).ExecuteAsync();
                if (existingEvent == null) return false;

                existingEvent.Status = "cancelled";
                existingEvent.Summary = $"[ĐÃ HỦY] {existingEvent.Summary}";

                var request = service.Events.Update(existingEvent, "primary", eventId);
                request.SendUpdates = EventsResource.UpdateRequest.SendUpdatesEnum.All;
                await request.ExecuteAsync();

                _logger.LogInformation("Cancelled Google Calendar event {EventId}", eventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling calendar event {EventId}", eventId);
                return false;
            }
        }

        private async Task<CalendarService?> GetCalendarServiceAsync(Guid userId)
        {
            try
            {
                var tokenInfo = await _tokenRepository.GetTokenAsync(userId);
                if (tokenInfo == null || tokenInfo.IsRevoked)
                {
                    _logger.LogWarning("No valid calendar token found for user {UserId}", userId);
                    return null;
                }

                // Automatic token refresh if expired
                if (tokenInfo.IsExpiredWithBuffer(5)) // 5 minute buffer
                {
                    _logger.LogInformation("Token expired for user {UserId}, attempting automatic refresh", userId);

                    var refreshed = await RefreshTokenAsync(userId, tokenInfo);
                    if (!refreshed)
                    {
                        _logger.LogWarning("Failed to refresh token for user {UserId}", userId);
                        return null;
                    }

                    // Get the refreshed token
                    tokenInfo = await _tokenRepository.GetTokenAsync(userId);
                    if (tokenInfo == null)
                    {
                        _logger.LogWarning("Failed to get refreshed token for user {UserId}", userId);
                        return null;
                    }
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["GoogleCalendar:ClientId"],
                    ClientSecret = _configuration["GoogleCalendar:ClientSecret"]
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes,
                    DataStore = new GoogleCalendarTokenStore(_tokenRepository)
                });

                // Create token response with proper expiry calculation
                var expiresInSeconds = Math.Max(0, (long)(tokenInfo.ExpiresAt - DateTime.UtcNow).TotalSeconds);
                var tokenResponse = new TokenResponse
                {
                    AccessToken = tokenInfo.AccessToken,
                    RefreshToken = tokenInfo.RefreshToken,
                    ExpiresInSeconds = expiresInSeconds,
                    Scope = tokenInfo.Scope,
                    TokenType = "Bearer"
                };

                var credential = new UserCredential(flow, userId.ToString(), tokenResponse);

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating calendar service for user {UserId}", userId);
                return null;
            }
        }

        private async Task<bool> RefreshTokenAsync(Guid userId, GoogleCalendarToken tokenInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenInfo.RefreshToken))
                {
                    _logger.LogWarning("No refresh token available for user {UserId}", userId);
                    return false;
                }

                var clientSecrets = new ClientSecrets
                {
                    ClientId = _configuration["GoogleCalendar:ClientId"],
                    ClientSecret = _configuration["GoogleCalendar:ClientSecret"]
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                    Scopes = Scopes
                });

                var newToken = await flow.RefreshTokenAsync(userId.ToString(), tokenInfo.RefreshToken, CancellationToken.None);

                // Save the refreshed token
                var success = await _tokenRepository.SaveTokenAsync(
                    userId,
                    newToken.AccessToken,
                    newToken.RefreshToken ?? tokenInfo.RefreshToken, // Keep old refresh token if new one not provided
                    newToken.ExpiresInSeconds);

                if (success)
                {
                    _logger.LogInformation("Successfully refreshed and saved token for user {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to save refreshed token for user {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token for user {UserId}. Token may be invalid or revoked.", userId);

                // If refresh fails, the user needs to re-authorize
                await _tokenRepository.RevokeTokenAsync(userId);
                return false;
            }
        }

        // Additional helper methods
        public async Task<IEnumerable<Event>> GetCounselorBusyTimesAsync(string counselorEmail, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting busy times for counselor {Email} from {Start} to {End}",
                    counselorEmail, startDate, endDate);
                return new List<Event>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting counselor busy times for {Email}", counselorEmail);
                return new List<Event>();
            }
        }

        public async Task<bool> SendAppointmentReminderAsync(string eventId, Guid userId, int minutesBefore = 60)
        {
            try
            {
                var service = await GetCalendarServiceAsync(userId);
                if (service == null) return false;

                var existingEvent = await service.Events.Get("primary", eventId).ExecuteAsync();
                if (existingEvent == null) return false;

                existingEvent.Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = new EventReminder[]
                    {
                        new EventReminder() { Method = "email", Minutes = minutesBefore },
                        new EventReminder() { Method = "popup", Minutes = 15 }
                    }
                };

                var request = service.Events.Update(existingEvent, "primary", eventId);
                await request.ExecuteAsync();

                _logger.LogInformation("Set reminder for calendar event {EventId}", eventId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting reminder for calendar event {EventId}", eventId);
                return false;
            }
        }
    }
}