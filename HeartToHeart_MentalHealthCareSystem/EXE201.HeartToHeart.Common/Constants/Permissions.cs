using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.Common.Constants
{
    public static class Permissions
    {
        // Guest permissions (non-registered users)
        public const string BrowseBlogs = "browse_blogs";
        public const string ViewAnonymousPosts = "view_anonymous_posts";

        // Member permissions (registered users)
        public const string PostAnonymously = "post_anonymously";
        public const string ChatWithAI = "chat_with_ai";
        public const string UseDiary = "use_diary";
        public const string TrackEmotions = "track_emotions";
        public const string ViewBasicMedia = "view_basic_media";
        public const string UpdateProfile = "update_profile";
        public const string ChangePassword = "change_password";

        // Premium permissions (paid subscription users)
        public const string ChatWithCounselor = "chat_with_counselor";
        public const string AccessPremiumMedia = "access_premium_media";
        public const string BookAppointments = "book_appointments";
        public const string PrioritySupport = "priority_support";
        public const string AccessAdvancedFeatures = "access_advanced_features";

        // Counselor permissions (mental health professionals)
        public const string ViewSchedules = "view_schedules";
        public const string ChatWithAssignedUsers = "chat_with_assigned_users";
        public const string ManageAppointments = "manage_appointments";
        public const string ViewClientProgress = "view_client_progress";
        public const string AccessCounselorResources = "access_counselor_resources";
        public const string CreateSessionNotes = "create_session_notes";
        public const string ManageClientCases = "manage_client_cases";

        // Staff permissions (platform staff)
        public const string ModerateContent = "moderate_content";
        public const string ManageReports = "manage_reports";
        public const string ViewUserData = "view_user_data";
        public const string ManageBasicUsers = "manage_basic_users";
        public const string AccessStaffTools = "access_staff_tools";
        public const string HandleSupport = "handle_support";

        // Admin permissions (system administrators)
        public const string ManageAllUsers = "manage_all_users";
        public const string ManageSubscriptions = "manage_subscriptions";
        public const string ViewAllData = "view_all_data";
        public const string DeletePosts = "delete_posts";
        public const string BanUsers = "ban_users";
        public const string ManageCounselors = "manage_counselors";
        public const string ViewAnalytics = "view_analytics";
        public const string ManageMediaContent = "manage_media_content";
        public const string SystemConfiguration = "system_configuration";
        public const string ManageRoles = "manage_roles";
        public const string ManageStaff = "manage_staff";
    }
}
