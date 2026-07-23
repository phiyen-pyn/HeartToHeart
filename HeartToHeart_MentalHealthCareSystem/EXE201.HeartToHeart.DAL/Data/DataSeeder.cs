using EXE201.HeartToHeart.Common.Constants;
using EXE201.HeartToHeart.DAL.Entities;
using EXE201.HeartToHeart.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using EXE201.HeartToHeart.DAL.DBContext;
using EXE201.HeartToHeart.DAL.Entities.Identity;

namespace EXE201.HeartToHeart.DAL.Seeders
{
    public static class DataSeeder
    {
        // Role permissions mapping - Updated with Staff role
        private static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [Roles.Guest] = new[] {
            Permissions.BrowseBlogs,
            Permissions.ViewAnonymousPosts
        },

            [Roles.Member] = new[] { // Changed from User to Member
            Permissions.BrowseBlogs,
            Permissions.ViewAnonymousPosts,
            Permissions.PostAnonymously,
            Permissions.ChatWithAI,
            Permissions.UseDiary,
            Permissions.TrackEmotions,
            Permissions.ViewBasicMedia,
            Permissions.UpdateProfile,
            Permissions.ChangePassword
        },

            [Roles.Premium] = new[] {
            // Include all Member permissions
            Permissions.BrowseBlogs,
            Permissions.ViewAnonymousPosts,
            Permissions.PostAnonymously,
            Permissions.ChatWithAI,
            Permissions.UseDiary,
            Permissions.TrackEmotions,
            Permissions.ViewBasicMedia,
            Permissions.UpdateProfile,
            Permissions.ChangePassword,
            // Premium specific permissions
            Permissions.ChatWithCounselor,
            Permissions.AccessPremiumMedia,
            Permissions.BookAppointments,
            Permissions.PrioritySupport,
            Permissions.AccessAdvancedFeatures
        },

            [Roles.Counselor] = new[] {
            // Basic permissions for platform usage
            Permissions.BrowseBlogs,
            Permissions.UpdateProfile,
            Permissions.ChangePassword,
            // Counselor specific permissions
            Permissions.ViewSchedules,
            Permissions.ChatWithAssignedUsers,
            Permissions.ManageAppointments,
            Permissions.ViewClientProgress,
            Permissions.AccessCounselorResources,
            Permissions.CreateSessionNotes,
            Permissions.ManageClientCases
        },

            [Roles.Staff] = new[] { // New Staff role
            // Basic permissions
            Permissions.BrowseBlogs,
            Permissions.UpdateProfile,
            Permissions.ChangePassword,
            // Staff specific permissions
            Permissions.ModerateContent,
            Permissions.ManageReports,
            Permissions.ViewUserData,
            Permissions.ManageBasicUsers,
            Permissions.AccessStaffTools,
            Permissions.HandleSupport
        },

            [Roles.Admin] = new[] {
            // All permissions for admin
            Permissions.BrowseBlogs,
            Permissions.ViewAnonymousPosts,
            Permissions.PostAnonymously,
            Permissions.ChatWithAI,
            Permissions.UseDiary,
            Permissions.TrackEmotions,
            Permissions.ViewBasicMedia,
            Permissions.UpdateProfile,
            Permissions.ChangePassword,
            Permissions.ChatWithCounselor,
            Permissions.AccessPremiumMedia,
            Permissions.BookAppointments,
            Permissions.PrioritySupport,
            Permissions.AccessAdvancedFeatures,
            Permissions.ViewSchedules,
            Permissions.ChatWithAssignedUsers,
            Permissions.ManageAppointments,
            Permissions.ViewClientProgress,
            Permissions.AccessCounselorResources,
            Permissions.CreateSessionNotes,
            Permissions.ManageClientCases,
            Permissions.ModerateContent,
            Permissions.ManageReports,
            Permissions.ViewUserData,
            Permissions.ManageBasicUsers,
            Permissions.AccessStaffTools,
            Permissions.HandleSupport,
            Permissions.ManageAllUsers,
            Permissions.ManageSubscriptions,
            Permissions.ViewAllData,
            Permissions.DeletePosts,
            Permissions.BanUsers,
            Permissions.ManageCounselors,
            Permissions.ViewAnalytics,
            Permissions.ManageMediaContent,
            Permissions.SystemConfiguration,
            Permissions.ManageRoles,
            Permissions.ManageStaff
        }
        };

        // Role definitions - Updated with Staff role
        private static readonly List<(string Name, string Description, int Level)> RoleDefinitions = new()
    {
        (Roles.Guest, "Non-registered users with limited access", 1),
        (Roles.Member, "Registered users with basic features", 2),
        (Roles.Premium, "Premium subscribers with enhanced features", 3),
        (Roles.Counselor, "Licensed mental health professionals", 4),
        (Roles.Staff, "Platform staff members for content moderation and support", 5),
        (Roles.Admin, "System administrators with full access", 6)
    };

        public static async Task SeedAsync(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            // Check if seeding is enabled
            var seedingEnabled = configuration.GetValue<bool>("SeedData:Enabled", true);
            if (!seedingEnabled)
            {
                return;
            }

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Role Claims (Permissions)
            await SeedRoleClaimsAsync(roleManager);

            // Seed Default Admin User Only
            await SeedDefaultAdminAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            foreach (var (name, description, level) in RoleDefinitions)
            {
                if (!await roleManager.RoleExistsAsync(name))
                {
                    var role = new ApplicationRole(name)
                    {
                        Id = Guid.NewGuid(),
                        Description = description,
                        Level = level
                    };

                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Role '{name}' created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create role '{name}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedRoleClaimsAsync(RoleManager<ApplicationRole> roleManager)
        {
            foreach (var (roleName, permissions) in RolePermissions)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var existingClaims = await roleManager.GetClaimsAsync(role);

                    foreach (var permission in permissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                        {
                            var result = await roleManager.AddClaimAsync(role,
                                new System.Security.Claims.Claim("permission", permission));

                            if (result.Succeeded)
                            {
                                Console.WriteLine($"Permission '{permission}' added to role '{roleName}'.");
                            }
                        }
                    }
                }
            }
        }

        private static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
        {
            // Create default admin with your specified email
            var adminEmail = "hearttoheartsu25.exe201@gmail.com";
            var adminPassword = "Admin@123!"; // You can change this or make it configurable

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true, // Pre-confirmed for admin
                    FirstName = "HeartToHeart",
                    LastName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    Console.WriteLine($"Default admin user created: {adminEmail}");
                    Console.WriteLine($"Default admin password: {adminPassword}");
                    Console.WriteLine("Please change the default password after first login!");
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", errors)}");
                }
            }
            else
            {
                Console.WriteLine($"Admin user already exists: {adminEmail}");
            }
        }
    }
}