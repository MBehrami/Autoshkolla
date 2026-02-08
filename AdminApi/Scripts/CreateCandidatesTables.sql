-- Run this script against your database (e.g. AutoshkollaDb) to create
-- Categories, Candidates, and CandidateInstallments tables.
-- Example: sqlcmd -S localhost,1433 -d AutoshkollaDb -U sa -P "Str0ng!Passw0rd" -i CreateCandidatesTables.sql
-- Or run in SSMS / Azure Data Studio against the same DB as ApiConnStringMssql.

USE [AutoshkollaDb];
GO

-- Categories (required by Candidates.CategoryId)
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [CategoryId] INT IDENTITY(1,1) NOT NULL,
        [CategoryName] NVARCHAR(10) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL,
        [AddedBy] INT NOT NULL,
        [DateAdded] DATETIME2 NOT NULL,
        [IsMigrationData] BIT NOT NULL DEFAULT 0,
        [LastUpdatedDate] DATETIME2 NULL,
        [LastUpdatedBy] INT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId])
    );

    INSERT INTO [dbo].[Categories] ([CategoryName],[Description],[IsActive],[AddedBy],[DateAdded],[IsMigrationData])
    VALUES
        (N'A1', NULL, 1, 1, GETUTCDATE(), 1),
        (N'A2', NULL, 1, 1, GETUTCDATE(), 1),
        (N'A', NULL, 1, 1, GETUTCDATE(), 1),
        (N'B', NULL, 1, 1, GETUTCDATE(), 1),
        (N'B+E', NULL, 1, 1, GETUTCDATE(), 1),
        (N'C1', NULL, 1, 1, GETUTCDATE(), 1),
        (N'C', NULL, 1, 1, GETUTCDATE(), 1);
END
GO

-- Candidates
IF OBJECT_ID(N'dbo.Candidates', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Candidates] (
        [CandidateId] INT IDENTITY(1,1) NOT NULL,
        [SerialNumber] NVARCHAR(4) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [ParentName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [DateOfBirth] NVARCHAR(20) NULL,
        [PersonalNumber] NVARCHAR(10) NULL,
        [PhoneNumber] NVARCHAR(50) NULL,
        [PlaceOfBirth] NVARCHAR(200) NULL,
        [Address] NVARCHAR(500) NULL,
        [CategoryId] INT NOT NULL,
        [InstructorId] INT NULL,
        [VehicleType] NVARCHAR(20) NULL,
        [PaymentMethod] NVARCHAR(20) NULL,
        [PracticalHours] INT NULL,
        [TotalServiceAmount] INT NOT NULL,
        [AddedBy] INT NOT NULL,
        [DateAdded] DATETIME2 NOT NULL,
        [IsMigrationData] BIT NOT NULL DEFAULT 0,
        [LastUpdatedDate] DATETIME2 NULL,
        [LastUpdatedBy] INT NULL,
        CONSTRAINT [PK_Candidates] PRIMARY KEY ([CandidateId]),
        CONSTRAINT [FK_Candidates_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([CategoryId]),
        CONSTRAINT [FK_Candidates_Users] FOREIGN KEY ([InstructorId]) REFERENCES [dbo].[Users]([UserId])
    );
END
GO

-- CandidateInstallments
IF OBJECT_ID(N'dbo.CandidateInstallments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CandidateInstallments] (
        [InstallmentId] INT IDENTITY(1,1) NOT NULL,
        [CandidateId] INT NOT NULL,
        [InstallmentNumber] INT NOT NULL,
        [Amount] INT NOT NULL,
        [InstallmentDate] NVARCHAR(20) NULL,
        [AddedBy] INT NOT NULL,
        [DateAdded] DATETIME2 NOT NULL,
        [LastUpdatedDate] DATETIME2 NULL,
        [LastUpdatedBy] INT NULL,
        CONSTRAINT [PK_CandidateInstallments] PRIMARY KEY ([InstallmentId]),
        CONSTRAINT [FK_CandidateInstallments_Candidates] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([CandidateId])
    );
END
GO
