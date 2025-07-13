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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Statuses] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Scope] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Statuses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Tenders] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Deadline] datetime2 NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [StatusId] uniqueidentifier NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Tenders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [VendorId] uniqueidentifier NULL,
        [IsLocked] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Vendors] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ContactEmail] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [IsBlacklisted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE TABLE [Bids] (
        [Id] uniqueidentifier NOT NULL,
        [TenderId] uniqueidentifier NOT NULL,
        [VendorId] uniqueidentifier NOT NULL,
        [BidAmount] decimal(18,2) NOT NULL,
        [StatusId] uniqueidentifier NOT NULL,
        [Comments] nvarchar(max) NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Bids] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Bids_Tenders_TenderId] FOREIGN KEY ([TenderId]) REFERENCES [Tenders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] ON;
    EXEC(N'INSERT INTO [Categories] ([Id], [CreatedAt], [Description], [Name], [UpdatedAt])
    VALUES (''54d06027-6965-456b-8b6e-1bc5e1e3c2af'', ''2024-01-01T00:00:00.0000000Z'', N''Professional services'', N''Consulting'', ''2024-01-01T00:00:00.0000000Z''),
    (''c6c2ae11-0b1a-4220-99eb-bc69f2c46d77'', ''2024-01-01T00:00:00.0000000Z'', N''Software and hardware'', N''IT'', ''2024-01-01T00:00:00.0000000Z''),
    (''e2ae4a2e-5a74-4f2a-b8ba-1b0bfc859101'', ''2024-01-01T00:00:00.0000000Z'', N''Civil and structural works'', N''Construction'', ''2024-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'Name', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Name', N'Scope', N'SortOrder', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Statuses]'))
        SET IDENTITY_INSERT [Statuses] ON;
    EXEC(N'INSERT INTO [Statuses] ([Id], [CreatedAt], [Name], [Scope], [SortOrder], [UpdatedAt])
    VALUES (''41d9b6d9-fd37-4894-a63e-65892a0cfe19'', ''2024-01-01T00:00:00.0000000Z'', N''Pending'', N''Bid'', 1, ''2024-01-01T00:00:00.0000000Z''),
    (''8b6cc0d4-c1ce-4991-a5fb-84f7ef404ac1'', ''2024-01-01T00:00:00.0000000Z'', N''Closed'', N''Tender'', 2, ''2024-01-01T00:00:00.0000000Z''),
    (''a1a3f451-a73b-484e-8fcb-0c21821f4d48'', ''2024-01-01T00:00:00.0000000Z'', N''Open'', N''Tender'', 1, ''2024-01-01T00:00:00.0000000Z''),
    (''d7122c1f-e7e8-4476-a2e2-19a2d906f6af'', ''2024-01-01T00:00:00.0000000Z'', N''Approved'', N''Bid'', 2, ''2024-01-01T00:00:00.0000000Z''),
    (''e022521e-ec06-4cf2-83f8-61eb0cd8a0f2'', ''2024-01-01T00:00:00.0000000Z'', N''Rejected'', N''Bid'', 3, ''2024-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Name', N'Scope', N'SortOrder', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[Statuses]'))
        SET IDENTITY_INSERT [Statuses] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bids_TenderId_VendorId] ON [Bids] ([TenderId], [VendorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250711085448_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250711085448_InitialCreate', N'9.0.7');
END;

COMMIT;
GO

