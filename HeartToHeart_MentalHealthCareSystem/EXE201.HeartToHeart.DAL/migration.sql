IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [MediaContents] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Type] nvarchar(50) NULL,
    [Url] nvarchar(500) NULL,
    [Category] nvarchar(100) NULL,
    [IsPremium] bit NOT NULL,
    [ViewCount] int NOT NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [DurationSeconds] int NULL,
    [Source] nvarchar(100) NULL,
    [ExternalId] nvarchar(max) NULL,
    [PublishedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_MediaContents] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Description] nvarchar(500) NULL,
    [Level] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Tags] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(100) NULL,
    [Color] nvarchar(20) NULL,
    [UsageCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] nvarchar(20) NULL,
    [ProfilePicture] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [RoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MediaContentTags] (
    [Id] uniqueidentifier NOT NULL,
    [MediaContentId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_MediaContentTags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MediaContentTags_MediaContents_MediaContentId] FOREIGN KEY ([MediaContentId]) REFERENCES [MediaContents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MediaContentTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AIConversations] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [UserMessage] nvarchar(max) NOT NULL,
    [AIResponse] nvarchar(max) NULL,
    [EmotionDetected] nvarchar(50) NULL,
    [SentAt] datetime2 NOT NULL,
    [SentimentScore] real NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_AIConversations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AIConversations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AnonymousPosts] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsReported] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_AnonymousPosts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AnonymousPosts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Blogs] (
    [Id] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Summary] nvarchar(500) NULL,
    [FeaturedImage] nvarchar(500) NULL,
    [Category] nvarchar(100) NULL,
    [Tags] nvarchar(max) NULL,
    [IsPublished] bit NOT NULL,
    [PublishedAt] datetime2 NULL,
    [ViewCount] int NOT NULL,
    [LikeCount] int NOT NULL,
    [IsFeatured] bit NOT NULL,
    [IsPremium] bit NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Blogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Counselors] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Description] nvarchar(max) NULL,
    [Specialization] nvarchar(100) NULL,
    [LicenseNumber] nvarchar(100) NULL,
    [ExperienceYears] int NULL,
    [IsVerified] bit NOT NULL,
    [HourlyRate] decimal(18,2) NULL,
    [IsAvailable] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Counselors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Counselors_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [DiaryEntries] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Title] nvarchar(150) NULL,
    [Content] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_DiaryEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiaryEntries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [EmotionTracks] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Emotion] nvarchar(50) NULL,
    [Note] nvarchar(255) NULL,
    [IntensityLevel] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_EmotionTracks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmotionTracks_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ImageUploads] (
    [Id] uniqueidentifier NOT NULL,
    [FileName] nvarchar(200) NOT NULL,
    [FirebaseUrl] nvarchar(500) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [FileSize] bigint NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [EntityId] uniqueidentifier NULL,
    [IsActive] bit NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_ImageUploads] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ImageUploads_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notifications] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Title] nvarchar(255) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [SentAt] datetime2 NOT NULL,
    [NotificationType] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Token] nvarchar(500) NOT NULL,
    [JwtId] nvarchar(500) NOT NULL,
    [IsUsed] bit NOT NULL,
    [IsRevoked] bit NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Subscriptions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [PlanName] nvarchar(100) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subscriptions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Comments] (
    [Id] uniqueidentifier NOT NULL,
    [PostId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_AnonymousPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [AnonymousPosts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Reports] (
    [Id] uniqueidentifier NOT NULL,
    [PostId] uniqueidentifier NOT NULL,
    [ReporterId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Reports_AnonymousPosts_PostId] FOREIGN KEY ([PostId]) REFERENCES [AnonymousPosts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reports_Users_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [BlogComments] (
    [Id] uniqueidentifier NOT NULL,
    [BlogId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [Content] nvarchar(max) NOT NULL,
    [ParentCommentId] uniqueidentifier NULL,
    [IsApproved] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_BlogComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BlogComments_BlogComments_ParentCommentId] FOREIGN KEY ([ParentCommentId]) REFERENCES [BlogComments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BlogComments_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BlogComments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [BlogLikes] (
    [Id] uniqueidentifier NOT NULL,
    [BlogId] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_BlogLikes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BlogLikes_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BlogLikes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BlogTags] (
    [Id] uniqueidentifier NOT NULL,
    [BlogId] uniqueidentifier NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_BlogTags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BlogTags_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BlogTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Appointments] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CounselorId] uniqueidentifier NOT NULL,
    [AppointmentDate] datetime2 NOT NULL,
    [Reason] nvarchar(max) NULL,
    [Status] nvarchar(50) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [DurationMinutes] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Counselors_CounselorId] FOREIGN KEY ([CounselorId]) REFERENCES [Counselors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Appointments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ChatSessions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CounselorId] uniqueidentifier NOT NULL,
    [StartedAt] datetime2 NOT NULL,
    [EndedAt] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL,
    [SessionNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_ChatSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatSessions_Counselors_CounselorId] FOREIGN KEY ([CounselorId]) REFERENCES [Counselors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PaymentTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [SubscriptionId] uniqueidentifier NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [TransactionId] nvarchar(200) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentTransactions_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentTransactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ChatMessages] (
    [Id] uniqueidentifier NOT NULL,
    [SessionId] uniqueidentifier NOT NULL,
    [SenderId] uniqueidentifier NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [SentAt] datetime2 NOT NULL,
    [IsFromAI] bit NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_ChatSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [ChatSessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChatMessages_Users_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AIConversations_UserId] ON [AIConversations] ([UserId]);
GO

CREATE INDEX [IX_AnonymousPosts_UserId] ON [AnonymousPosts] ([UserId]);
GO

CREATE INDEX [IX_Appointments_CounselorId] ON [Appointments] ([CounselorId]);
GO

CREATE INDEX [IX_Appointments_UserId] ON [Appointments] ([UserId]);
GO

CREATE INDEX [IX_BlogComments_BlogId] ON [BlogComments] ([BlogId]);
GO

CREATE INDEX [IX_BlogComments_ParentCommentId] ON [BlogComments] ([ParentCommentId]);
GO

CREATE INDEX [IX_BlogComments_UserId] ON [BlogComments] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_BlogLikes_BlogId_UserId] ON [BlogLikes] ([BlogId], [UserId]);
GO

CREATE INDEX [IX_BlogLikes_UserId] ON [BlogLikes] ([UserId]);
GO

CREATE INDEX [IX_Blogs_Category] ON [Blogs] ([Category]);
GO

CREATE INDEX [IX_Blogs_IsPublished] ON [Blogs] ([IsPublished]);
GO

CREATE INDEX [IX_Blogs_PublishedAt] ON [Blogs] ([PublishedAt]);
GO

CREATE INDEX [IX_Blogs_Title] ON [Blogs] ([Title]);
GO

CREATE INDEX [IX_Blogs_UserId] ON [Blogs] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_BlogTags_BlogId_TagId] ON [BlogTags] ([BlogId], [TagId]);
GO

CREATE INDEX [IX_BlogTags_TagId] ON [BlogTags] ([TagId]);
GO

CREATE INDEX [IX_ChatMessages_SenderId] ON [ChatMessages] ([SenderId]);
GO

CREATE INDEX [IX_ChatMessages_SessionId] ON [ChatMessages] ([SessionId]);
GO

CREATE INDEX [IX_ChatSessions_CounselorId] ON [ChatSessions] ([CounselorId]);
GO

CREATE INDEX [IX_ChatSessions_UserId] ON [ChatSessions] ([UserId]);
GO

CREATE INDEX [IX_Comments_PostId] ON [Comments] ([PostId]);
GO

CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Counselors_UserId] ON [Counselors] ([UserId]);
GO

CREATE INDEX [IX_DiaryEntries_UserId] ON [DiaryEntries] ([UserId]);
GO

CREATE INDEX [IX_EmotionTracks_UserId] ON [EmotionTracks] ([UserId]);
GO

CREATE INDEX [IX_ImageUploads_EntityId] ON [ImageUploads] ([EntityId]);
GO

CREATE INDEX [IX_ImageUploads_EntityType] ON [ImageUploads] ([EntityType]);
GO

CREATE INDEX [IX_ImageUploads_EntityType_EntityId] ON [ImageUploads] ([EntityType], [EntityId]);
GO

CREATE INDEX [IX_ImageUploads_IsActive] ON [ImageUploads] ([IsActive]);
GO

CREATE INDEX [IX_ImageUploads_UserId] ON [ImageUploads] ([UserId]);
GO

CREATE INDEX [IX_MediaContents_Category] ON [MediaContents] ([Category]);
GO

CREATE INDEX [IX_MediaContents_IsPremium] ON [MediaContents] ([IsPremium]);
GO

CREATE INDEX [IX_MediaContents_PublishedAt] ON [MediaContents] ([PublishedAt]);
GO

CREATE INDEX [IX_MediaContents_Type] ON [MediaContents] ([Type]);
GO

CREATE INDEX [IX_MediaContents_ViewCount] ON [MediaContents] ([ViewCount]);
GO

CREATE UNIQUE INDEX [IX_MediaContentTags_MediaContentId_TagId] ON [MediaContentTags] ([MediaContentId], [TagId]);
GO

CREATE INDEX [IX_MediaContentTags_TagId] ON [MediaContentTags] ([TagId]);
GO

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
GO

CREATE INDEX [IX_PaymentTransactions_SubscriptionId] ON [PaymentTransactions] ([SubscriptionId]);
GO

CREATE INDEX [IX_PaymentTransactions_UserId] ON [PaymentTransactions] ([UserId]);
GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO

CREATE INDEX [IX_Reports_PostId] ON [Reports] ([PostId]);
GO

CREATE INDEX [IX_Reports_ReporterId] ON [Reports] ([ReporterId]);
GO

CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_Subscriptions_UserId] ON [Subscriptions] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Tags_Name] ON [Tags] ([Name]);
GO

CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);
GO

CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);
GO

CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]) WHERE [UserName] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250711042219_InitialCreate', N'8.0.5');
GO

COMMIT;
GO

