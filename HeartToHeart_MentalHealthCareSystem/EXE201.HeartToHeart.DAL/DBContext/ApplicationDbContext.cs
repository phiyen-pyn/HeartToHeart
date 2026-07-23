using EXE201.HeartToHeart.DAL.Entities.Application;
using EXE201.HeartToHeart.DAL.Entities.Base;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EXE201.HeartToHeart.DAL.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        IdentityUserRole<Guid>,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>
        >
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            _httpContextAccessor = null;
        }
        // Application DbSets
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AnonymousPost> AnonymousPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DiaryEntry> DiaryEntries { get; set; }
        public DbSet<EmotionTrack> EmotionTracks { get; set; }
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<AIConversation> AIConversations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<MediaContent> MediaContents { get; set; }
        public DbSet<MediaContentTag> MediaContentTags { get; set; }
        public DbSet<ImageUpload> ImageUploads { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<CounselorAvailability> CounselorAvailabilities { get; set; }
        public DbSet<CounselorScheduleTemplate> CounselorScheduleTemplates { get; set; }
        public DbSet<AppointmentHistory> AppointmentHistories { get; set; }
        public DbSet<GoogleCalendarToken> GoogleCalendarTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity table names
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            // Configure entity relationships and constraints
            ConfigureIdentityEntities(builder);
            ConfigureApplicationEntities(builder);
            ConfigureEnhancedAppointmentEntities(builder);
        }

        private void ConfigureIdentityEntities(ModelBuilder builder)
        {
            // Configure RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
                entity.Property(rt => rt.JwtId).IsRequired().HasMaxLength(500);
                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();
            });
        }

        private void ConfigureApplicationEntities(ModelBuilder builder)
        {
            // Configure AnonymousPost
            builder.Entity<AnonymousPost>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithMany(u => u.AnonymousPosts)
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Comment
            builder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany() // No navigation property on User for Comments
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure DiaryEntry
            builder.Entity<DiaryEntry>(entity =>
            {
                entity.HasOne(d => d.User)
                      .WithMany(u => u.DiaryEntries)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure EmotionTrack
            builder.Entity<EmotionTrack>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany(u => u.EmotionTracks)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Counselor
            builder.Entity<Counselor>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithOne(u => u.CounselorProfile)
                      .HasForeignKey<Counselor>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ChatSession
            builder.Entity<ChatSession>(entity =>
            {
                entity.HasOne(cs => cs.User)
                      .WithMany() // No navigation property on User for ChatSessions
                      .HasForeignKey(cs => cs.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.Counselor)
                      .WithMany(c => c.ChatSessions)
                      .HasForeignKey(cs => cs.CounselorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ChatMessage
            builder.Entity<ChatMessage>(entity =>
            {
                entity.HasOne(cm => cm.Session)
                      .WithMany(cs => cs.ChatMessages)
                      .HasForeignKey(cm => cm.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cm => cm.Sender)
                      .WithMany() // No navigation property on User for ChatMessages
                      .HasForeignKey(cm => cm.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Subscription
            builder.Entity<Subscription>(entity =>
            {
                entity.HasOne(s => s.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PaymentTransaction
            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasOne(pt => pt.User)
                      .WithMany(u => u.PaymentTransactions)
                      .HasForeignKey(pt => pt.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pt => pt.Subscription)
                      .WithMany(s => s.PaymentTransactions)
                      .HasForeignKey(pt => pt.SubscriptionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AIConversation
            builder.Entity<AIConversation>(entity =>
            {
                entity.HasOne(ai => ai.User)
                      .WithMany(u => u.AIConversations)
                      .HasForeignKey(ai => ai.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Report
            builder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.Post)
                      .WithMany(p => p.Reports)
                      .HasForeignKey(r => r.PostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Reporter)
                      .WithMany(u => u.Reports)
                      .HasForeignKey(r => r.ReporterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Appointment
            builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.User)
                      .WithMany(u => u.UserAppointments)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Counselor)
                      .WithMany(c => c.Appointments)
                      .HasForeignKey(a => a.CounselorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Blog
            builder.Entity<Blog>(entity =>
            {
                entity.HasOne(b => b.User)
                      .WithMany(u => u.Blogs)
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(b => b.Title);
                entity.HasIndex(b => b.Category);
                entity.HasIndex(b => b.IsPublished);
                entity.HasIndex(b => b.PublishedAt);
            });

            // Configure BlogComment
            builder.Entity<BlogComment>(entity =>
            {
                entity.HasOne(bc => bc.Blog)
                      .WithMany(b => b.Comments)
                      .HasForeignKey(bc => bc.BlogId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bc => bc.User)
                      .WithMany(u => u.BlogComments)
                      .HasForeignKey(bc => bc.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(bc => bc.ParentComment)
                      .WithMany(bc => bc.Replies)
                      .HasForeignKey(bc => bc.ParentCommentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure BlogLike
            builder.Entity<BlogLike>(entity =>
            {
                entity.HasOne(bl => bl.Blog)
                      .WithMany(b => b.Likes)
                      .HasForeignKey(bl => bl.BlogId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bl => bl.User)
                      .WithMany(u => u.BlogLikes)
                      .HasForeignKey(bl => bl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ensure unique constraint: one user can only like a blog once
                entity.HasIndex(bl => new { bl.BlogId, bl.UserId }).IsUnique();
            });

            // Configure Tag
            builder.Entity<Tag>(entity =>
            {
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(t => t.Name).IsRequired();
            });

            // Configure BlogTag (Many-to-Many relationship)
            builder.Entity<BlogTag>(entity =>
            {
                entity.HasOne(bt => bt.Blog)
                      .WithMany(b => b.BlogTags)
                      .HasForeignKey(bt => bt.BlogId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bt => bt.Tag)
                      .WithMany(t => t.BlogTags)
                      .HasForeignKey(bt => bt.TagId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ensure unique constraint: one blog can have one instance of each tag
                entity.HasIndex(bt => new { bt.BlogId, bt.TagId }).IsUnique();
            });

            // Configure MediaContent
            builder.Entity<MediaContent>(entity =>
            {
                entity.Property(mc => mc.Title).IsRequired().HasMaxLength(200);
                entity.Property(mc => mc.Type).HasMaxLength(50);
                entity.Property(mc => mc.Url).HasMaxLength(500);
                entity.Property(mc => mc.Category).HasMaxLength(100);
                entity.Property(mc => mc.ThumbnailUrl).HasMaxLength(500);
                entity.Property(mc => mc.Source).HasMaxLength(100);

                // Add indexes for better performance
                entity.HasIndex(mc => mc.Category);
                entity.HasIndex(mc => mc.Type);
                entity.HasIndex(mc => mc.IsPremium);
                entity.HasIndex(mc => mc.PublishedAt);
                entity.HasIndex(mc => mc.ViewCount);
            });

            // Configure MediaContentTag (Many-to-Many relationship)
            builder.Entity<MediaContentTag>(entity =>
            {
                entity.HasOne(mct => mct.MediaContent)
                      .WithMany(mc => mc.MediaContentTags)
                      .HasForeignKey(mct => mct.MediaContentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mct => mct.Tag)
                      .WithMany(t => t.MediaContentTags)
                      .HasForeignKey(mct => mct.TagId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Ensure unique constraint: one media content can have one instance of each tag
                entity.HasIndex(mct => new { mct.MediaContentId, mct.TagId }).IsUnique();
            });

            // Configure ImageUpload
            builder.Entity<ImageUpload>(entity =>
            {
                entity.HasOne(i => i.User)
                      .WithMany() // No navigation property on User for ImageUploads
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(i => i.EntityType);
                entity.HasIndex(i => i.EntityId);
                entity.HasIndex(i => i.IsActive);
                entity.HasIndex(i => new { i.EntityType, i.EntityId });
            });
        }

        private void ConfigureEnhancedAppointmentEntities(ModelBuilder builder)
        {
            // Configure Holiday
            builder.Entity<Holiday>(entity =>
            {
                entity.HasIndex(h => h.Date);
                entity.HasIndex(h => h.CountryCode);
                entity.HasIndex(h => new { h.Date, h.CountryCode, h.IsActive });
                entity.HasIndex(h => new { h.Date, h.IsRecurring });

                entity.Property(h => h.Name).IsRequired().HasMaxLength(100);
                entity.Property(h => h.CountryCode).HasMaxLength(50).HasDefaultValue("VN");
                entity.Property(h => h.IsActive).HasDefaultValue(true);
                entity.Property(h => h.IsRecurring).HasDefaultValue(false);
            });

            // Configure CounselorAvailability
            builder.Entity<CounselorAvailability>(entity =>
            {
                entity.HasOne(ca => ca.Counselor)
                      .WithMany(c => c.CustomAvailabilities)
                      .HasForeignKey(ca => ca.CounselorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ca => new { ca.CounselorId, ca.Date }).IsUnique();
                entity.HasIndex(ca => ca.Date);
                entity.HasIndex(ca => ca.CounselorId);
                entity.HasIndex(ca => new { ca.CounselorId, ca.Date, ca.IsAvailable });

                entity.Property(ca => ca.IsAvailable).HasDefaultValue(true);
            });

            // Configure CounselorScheduleTemplate
            builder.Entity<CounselorScheduleTemplate>(entity =>
            {
                entity.HasOne(cst => cst.Counselor)
                      .WithMany(c => c.ScheduleTemplates)
                      .HasForeignKey(cst => cst.CounselorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cst => new { cst.CounselorId, cst.DayOfWeek }).IsUnique();
                entity.HasIndex(cst => cst.CounselorId);
                entity.HasIndex(cst => cst.DayOfWeek);

                entity.Property(cst => cst.IsAvailable).HasDefaultValue(true);
                entity.Property(cst => cst.DayOfWeek).IsRequired();
            });

            // Configure AppointmentHistory
            builder.Entity<AppointmentHistory>(entity =>
            {
                entity.HasOne(ah => ah.Appointment)
                      .WithMany(a => a.HistoryChanges)
                      .HasForeignKey(ah => ah.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ah => ah.ChangedByUser)
                      .WithMany(u => u.AppointmentHistoryChanges)
                      .HasForeignKey(ah => ah.ChangedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ah => ah.AppointmentId);
                entity.HasIndex(ah => ah.CreatedAt);
                entity.HasIndex(ah => ah.ChangedByUserId);
                entity.HasIndex(ah => new { ah.AppointmentId, ah.CreatedAt });

                entity.Property(ah => ah.Action).IsRequired().HasMaxLength(50);
                entity.Property(ah => ah.OldStatus).IsRequired().HasMaxLength(50);
                entity.Property(ah => ah.NewStatus).IsRequired().HasMaxLength(50);
            });

            // Configure GoogleCalendarToken
            builder.Entity<GoogleCalendarToken>(entity =>
            {
                entity.HasOne(gct => gct.User)
                      .WithMany() // No navigation property on User for GoogleCalendarTokens
                      .HasForeignKey(gct => gct.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(gct => gct.UserId).IsUnique();
                entity.HasIndex(gct => gct.ExpiresAt);
                entity.HasIndex(gct => new { gct.UserId, gct.IsRevoked });

                entity.Property(gct => gct.AccessToken).IsRequired().HasMaxLength(1000);
                entity.Property(gct => gct.RefreshToken).HasMaxLength(1000);
                entity.Property(gct => gct.Scope).HasMaxLength(500);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            UpdateAuditFields(); // ADDED: Handle audit fields
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            UpdateAuditFields(); // ADDED: Handle audit fields
            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is EntityBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (EntityBase)entry.Entity;

                // FIXED: Use Vietnam time instead of UTC
                var vietnamTime = GetVietnamTime();

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = vietnamTime;
                }

                entity.UpdatedAt = vietnamTime;
            }
        }

        // ADDED: Helper method to get Vietnam time
        private DateTime GetVietnamTime()
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        }

        // UPDATED: Method to handle audit fields with Guid converted to string
        private void UpdateAuditFields()
        {
            var currentUserId = GetCurrentUserId();

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntityBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (AuditableEntityBase)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        // Only set if not already set (preserve values set in business logic)
                        if (string.IsNullOrEmpty(entity.CreatedBy))
                        {
                            entity.CreatedBy = currentUserId?.ToString() ?? "System";
                        }
                        if (string.IsNullOrEmpty(entity.UpdatedBy))
                        {
                            entity.UpdatedBy = entity.CreatedBy ?? currentUserId?.ToString() ?? "System";
                        }
                        break;

                    case EntityState.Modified:
                        // Always update the UpdatedBy field, but don't touch CreatedBy
                        entity.UpdatedBy = currentUserId?.ToString() ?? entity.UpdatedBy ?? "System";
                        break;
                }
            }
        }

        // UPDATED: Method to get current userId from HttpContext
        private Guid? GetCurrentUserId()
        {
            if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                    return userId;
            }

            return null;
        }
    }
}