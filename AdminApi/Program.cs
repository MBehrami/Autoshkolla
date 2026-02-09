using System.Reflection;
using System.Text;
using AdminApi.Models;
using AdminApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


var AllowSpecificOrigins = "_allowSpecificOrigins";

//Sql Server Connection String
builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseSqlServer(builder.Configuration["ConnectionStrings:ApiConnStringMssql"]));

//Mysql Server Connection String
/* builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseMySql
(builder.Configuration["ConnectionStrings:ApiConnStringMysql"],
ServerVersion.AutoDetect(builder.Configuration["ConnectionStrings:ApiConnStringMysql"]))); */

//Sqlite Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseSqlite(builder.Configuration["ConnectionStrings:ApiConnStringSqlite"]));

//PostgreSql Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseNpgsql(builder.Configuration["ConnectionStrings:ApiConnStringPostgreSql"]));

//Oracle Connection String
//builder.Services.AddDbContextPool<AppDbContext>(opt=>opt.UseOracle(builder.Configuration["ConnectionStrings:ApiConnStringOracle"]));

builder.Services.AddScoped(typeof(ISqlRepository<>), typeof(SqlRepository<>));

builder.Services.AddCors(options=>
{
    options.AddPolicy(name:AllowSpecificOrigins,builder=>
        {
            builder.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

builder.Services.AddTransient<IMailService,MailService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options=>{
    options.RequireHttpsMetadata=false;
    options.SaveToken=true;
    // Disable default claim-type mapping so "sub" and "role" stay as-is in ClaimsPrincipal
    options.MapInboundClaims=false;
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters=new TokenValidationParameters
        {
            ValidateIssuer=true,
            ValidateAudience=true,
            ValidateLifetime=true,
            ValidateIssuerSigningKey=true,
            ValidIssuer=builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ClockSkew = TimeSpan.Zero,
            // Tell ASP.NET to look for "role" claim for [Authorize(Roles=...)]
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
#pragma warning restore CS8604 // Possible null reference argument.
});

IdentityModelEventSource.ShowPII = true;

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Admin API v2",
        Version = "v2",
        Description = "API to communicate with Client Project"
    });               
    options.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme()
    {
            Name = "Authorization",  
            Type = SecuritySchemeType.ApiKey,  
            Scheme = "Bearer",  
            BearerFormat = "JWT",  
            In = ParameterLocation.Header,  
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement  
    {  
        {  
                new OpenApiSecurityScheme  
                {  
                    Reference = new OpenApiReference  
                    {  
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"  
                    }  
                },  
                new string[] {}  

        }  
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.Configure<FormOptions>(o => {
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartBodyLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = int.MaxValue;
});

builder.WebHost.UseUrls("http://localhost:5002");


var app = builder.Build();

// Ensure Candidates-related tables exist (creates them if missing when no migration was run)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
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
                VALUES (N'A1',NULL,1,1,GETUTCDATE(),1),(N'A2',NULL,1,1,GETUTCDATE(),1),(N'A',NULL,1,1,GETUTCDATE(),1),(N'B',NULL,1,1,GETUTCDATE(),1),(N'B+E',NULL,1,1,GETUTCDATE(),1),(N'C1',NULL,1,1,GETUTCDATE(),1),(N'C',NULL,1,1,GETUTCDATE(),1);
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Tables may already exist */ }
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.Candidates', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Candidates] (
                    [CandidateId] INT IDENTITY(1,1) NOT NULL,
                    [SerialNumber] NVARCHAR(4) NULL,
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
            -- Make SerialNumber nullable for existing DBs
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'SerialNumber' AND IS_NULLABLE = 'NO')
            BEGIN
                ALTER TABLE [dbo].[Candidates] ALTER COLUMN [SerialNumber] NVARCHAR(4) NULL;
            END
            -- Add new payment columns
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'DocWithdrawalAmount')
            BEGIN
                ALTER TABLE [dbo].[Candidates] ADD [DocWithdrawalAmount] INT NULL;
            END
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'DocWithdrawalDate')
            BEGIN
                ALTER TABLE [dbo].[Candidates] ADD [DocWithdrawalDate] NVARCHAR(20) NULL;
            END
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'DrivingPaymentAmount')
            BEGIN
                ALTER TABLE [dbo].[Candidates] ADD [DrivingPaymentAmount] INT NULL;
            END
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Candidates' AND COLUMN_NAME = 'DrivingPaymentDate')
            BEGIN
                ALTER TABLE [dbo].[Candidates] ADD [DrivingPaymentDate] NVARCHAR(20) NULL;
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
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
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }
    // Ensure Instructor menu group exists (so Instructor role gets only Candidates in sidebar)
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuGroup] WHERE [MenuGroupID] = 3)
            BEGIN
                SET IDENTITY_INSERT [dbo].[MenuGroup] ON;
                INSERT INTO [dbo].[MenuGroup] ([MenuGroupID],[MenuGroupName],[IsActive],[DateAdded],[AddedBy],[IsMigrationData])
                VALUES (3, N'Instructor Group', 1, GETUTCDATE(), 1, 0);
                SET IDENTITY_INSERT [dbo].[MenuGroup] OFF;
            END
            IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuGroupWiseMenuMapping] WHERE [MenuGroupId] = 3 AND [MenuId] = 14)
            BEGIN
                INSERT INTO [dbo].[MenuGroupWiseMenuMapping] ([MenuGroupId],[MenuId],[IsActive],[IsMigrationData],[DateAdded],[AddedBy])
                VALUES (3, 14, 1, 1, GETUTCDATE(), 1);
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* MenuGroup may already exist */ }
    // Ensure Instructor role exists and uses Instructor Group (MenuGroupId=3) so sidebar shows only Candidates
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM [dbo].[UserRole] WHERE [RoleName] = N'Instructor')
            BEGIN
                INSERT INTO [dbo].[UserRole] ([RoleName],[RoleDesc],[MenuGroupId],[IsActive],[AddedBy],[DateAdded],[IsMigrationData])
                VALUES (N'Instructor', N'Driving instructor', 3, 1, 1, GETUTCDATE(), 0);
            END
            ELSE
            BEGIN
                UPDATE [dbo].[UserRole] SET [MenuGroupId] = 3 WHERE [RoleName] = N'Instructor';
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Role may already exist */ }
    // Ensure Instructors menu item exists for Admin (MenuGroupId=1)
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM [dbo].[Menu] WHERE [MenuID] = 15)
            BEGIN
                SET IDENTITY_INSERT [dbo].[Menu] ON;
                INSERT INTO [dbo].[Menu] ([MenuID],[ParentID],[MenuTitle],[URL],[IsSubMenu],[SortOrder],[IconClass],[IsActive],[DateAdded],[AddedBy],[IsMigrationData])
                VALUES (15, 0, N'Instructors', N'/instructors', 0, 9, N'mdi-account-tie', 1, GETUTCDATE(), 1, 1);
                SET IDENTITY_INSERT [dbo].[Menu] OFF;
            END
            IF NOT EXISTS (SELECT 1 FROM [dbo].[MenuGroupWiseMenuMapping] WHERE [MenuGroupId] = 1 AND [MenuId] = 15)
            BEGIN
                INSERT INTO [dbo].[MenuGroupWiseMenuMapping] ([MenuGroupId],[MenuId],[IsActive],[IsMigrationData],[DateAdded],[AddedBy])
                VALUES (1, 15, 1, 1, GETUTCDATE(), 1);
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Menu may already exist */ }
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.InstructorProfiles', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[InstructorProfiles] (
                    [InstructorProfileId] INT IDENTITY(1,1) NOT NULL,
                    [UserId] INT NOT NULL,
                    [FirstName] NVARCHAR(100) NULL,
                    [ParentName] NVARCHAR(100) NULL,
                    [LastName] NVARCHAR(100) NULL,
                    [PersonalNumber] NVARCHAR(10) NULL,
                    [ScheduleType] NVARCHAR(20) NULL,
                    [LicenseNumber] NVARCHAR(50) NULL,
                    [LicenseValidityDate] NVARCHAR(20) NULL,
                    [LicensePhotoPath] NVARCHAR(500) NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    [LastUpdatedDate] DATETIME2 NULL,
                    [LastUpdatedBy] INT NULL,
                    CONSTRAINT [PK_InstructorProfiles] PRIMARY KEY ([InstructorProfileId]),
                    CONSTRAINT [FK_InstructorProfiles_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
                );
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.PracticalLessons', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PracticalLessons] (
                    [PracticalLessonId] INT IDENTITY(1,1) NOT NULL,
                    [CandidateId] INT NOT NULL,
                    [InstructorUserId] INT NOT NULL,
                    [LessonDate] NVARCHAR(20) NOT NULL,
                    [Time] NVARCHAR(10) NOT NULL,
                    [EndTime] NVARCHAR(10) NULL,
                    [Vehicle] NVARCHAR(100) NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_PracticalLessons] PRIMARY KEY ([PracticalLessonId]),
                    CONSTRAINT [FK_PracticalLessons_Candidates] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([CandidateId]),
                    CONSTRAINT [FK_PracticalLessons_Users] FOREIGN KEY ([InstructorUserId]) REFERENCES [dbo].[Users]([UserId])
                );
            END
            -- Add EndTime column if it doesn't exist (for DBs created before this change)
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PracticalLessons' AND COLUMN_NAME = 'EndTime')
            BEGIN
                ALTER TABLE [dbo].[PracticalLessons] ADD [EndTime] NVARCHAR(10) NULL;
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }

    // ── Vehicle tables ──
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Vehicles] (
                    [VehicleId] INT IDENTITY(1,1) NOT NULL,
                    [PlateNumber] NVARCHAR(20) NOT NULL,
                    [ChassisNumber] NVARCHAR(50) NULL,
                    [Color] NVARCHAR(30) NULL,
                    [Type] NVARCHAR(50) NULL,
                    [Brand] NVARCHAR(50) NULL,
                    [RegistrationDate] NVARCHAR(20) NULL,
                    [ExpiryDate] NVARCHAR(20) NULL,
                    [CertificateNumber] NVARCHAR(50) NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    [IsActive] BIT NOT NULL DEFAULT 1,
                    [IsMigrationData] BIT NOT NULL DEFAULT 0,
                    [LastUpdatedDate] DATETIME2 NULL,
                    [LastUpdatedBy] INT NULL,
                    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([VehicleId])
                );
            END
            -- Add IsActive column if missing (for existing DBs)
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Vehicles' AND COLUMN_NAME = 'IsActive')
            BEGIN
                ALTER TABLE [dbo].[Vehicles] ADD [IsActive] BIT NOT NULL DEFAULT 1;
            END

            IF OBJECT_ID(N'dbo.VehicleFuels', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VehicleFuels] (
                    [VehicleFuelId] INT IDENTITY(1,1) NOT NULL,
                    [FillDate] NVARCHAR(20) NOT NULL,
                    [VehicleId] INT NOT NULL,
                    [FuelAmount] DECIMAL(18,2) NOT NULL,
                    [FuelType] NVARCHAR(20) NOT NULL,
                    [StaffUserId] INT NOT NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_VehicleFuels] PRIMARY KEY ([VehicleFuelId]),
                    CONSTRAINT [FK_VehicleFuels_Vehicles] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles]([VehicleId]),
                    CONSTRAINT [FK_VehicleFuels_Users] FOREIGN KEY ([StaffUserId]) REFERENCES [dbo].[Users]([UserId])
                );
            END

            IF OBJECT_ID(N'dbo.VehicleServices', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[VehicleServices] (
                    [VehicleServiceId] INT IDENTITY(1,1) NOT NULL,
                    [VehicleId] INT NOT NULL,
                    [ServiceDate] NVARCHAR(20) NULL,
                    [Description] NVARCHAR(500) NULL,
                    [Cost] DECIMAL(18,2) NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_VehicleServices] PRIMARY KEY ([VehicleServiceId]),
                    CONSTRAINT [FK_VehicleServices_Vehicles] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles]([VehicleId])
                );
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Tables may already exist */ }

    // ── Driving Sessions table ──
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.DrivingSessions', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DrivingSessions] (
                    [DrivingSessionId] INT IDENTITY(1,1) NOT NULL,
                    [CandidateId] INT NOT NULL,
                    [VehicleId] INT NOT NULL,
                    [InstructorUserId] INT NULL,
                    [DrivingDate] NVARCHAR(20) NOT NULL,
                    [DrivingTime] NVARCHAR(10) NOT NULL,
                    [PaymentAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
                    [PaymentDate] NVARCHAR(20) NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    [Status] NVARCHAR(20) NULL,
                    [Examiner] NVARCHAR(100) NULL,
                    CONSTRAINT [PK_DrivingSessions] PRIMARY KEY ([DrivingSessionId]),
                    CONSTRAINT [FK_DrivingSessions_Candidates] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([CandidateId]),
                    CONSTRAINT [FK_DrivingSessions_Vehicles] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles]([VehicleId])
                );
            END
            -- Add Status and Examiner columns if missing (for existing DBs)
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DrivingSessions' AND COLUMN_NAME = 'Status')
            BEGIN
                ALTER TABLE [dbo].[DrivingSessions] ADD [Status] NVARCHAR(20) NULL;
            END
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DrivingSessions' AND COLUMN_NAME = 'Examiner')
            BEGIN
                ALTER TABLE [dbo].[DrivingSessions] ADD [Examiner] NVARCHAR(100) NULL;
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }

    // ── Schedule Events table ──
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.ScheduleEvents', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ScheduleEvents] (
                    [ScheduleEventId] INT IDENTITY(1,1) NOT NULL,
                    [EventDate] NVARCHAR(20) NOT NULL,
                    [StartTime] NVARCHAR(10) NOT NULL,
                    [EndTime] NVARCHAR(10) NOT NULL,
                    [InstructorUserId] INT NOT NULL,
                    [CandidateId] INT NOT NULL,
                    [VehicleId] INT NOT NULL,
                    [Notes] NVARCHAR(500) NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_ScheduleEvents] PRIMARY KEY ([ScheduleEventId]),
                    CONSTRAINT [FK_ScheduleEvents_Candidates] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([CandidateId]),
                    CONSTRAINT [FK_ScheduleEvents_Vehicles] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles]([VehicleId])
                );
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }

    // ── Daily Report tables ──
    try
    {
        _ = db.Database.ExecuteSqlRawAsync(@"
            IF OBJECT_ID(N'dbo.DailyReportEntries', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DailyReportEntries] (
                    [DailyReportEntryId] INT IDENTITY(1,1) NOT NULL,
                    [EntryDate] NVARCHAR(20) NOT NULL,
                    [EntryType] NVARCHAR(20) NOT NULL,
                    [SerialNumber] INT NOT NULL,
                    [FullName] NVARCHAR(200) NOT NULL,
                    [Amount] DECIMAL(18,2) NOT NULL,
                    [Description] NVARCHAR(500) NOT NULL,
                    [SourceType] NVARCHAR(50) NULL,
                    [SourceId] INT NULL,
                    [ReversalOfEntryId] INT NULL,
                    [AddedBy] INT NOT NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_DailyReportEntries] PRIMARY KEY ([DailyReportEntryId])
                );
            END

            IF OBJECT_ID(N'dbo.DailyReportStatuses', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[DailyReportStatuses] (
                    [DailyReportStatusId] INT IDENTITY(1,1) NOT NULL,
                    [ReportDate] NVARCHAR(20) NOT NULL,
                    [Status] NVARCHAR(10) NOT NULL DEFAULT 'Open',
                    [ClosedBy] INT NULL,
                    [ClosedAt] DATETIME2 NULL,
                    CONSTRAINT [PK_DailyReportStatuses] PRIMARY KEY ([DailyReportStatusId])
                );
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Tables may already exist */ }

    // ── SuperAdmin role + assign admin@vueadmin.com ──
    try
    {
        db.Database.ExecuteSqlRawAsync(@"
            -- Create SuperAdmin role if it doesn't exist
            IF NOT EXISTS (SELECT 1 FROM [dbo].[UserRole] WHERE [RoleName] = N'SuperAdmin')
            BEGIN
                INSERT INTO [dbo].[UserRole] ([RoleName], [RoleDesc], [MenuGroupId], [IsActive], [AddedBy], [DateAdded], [IsMigrationData])
                VALUES (N'SuperAdmin', N'Full system access', 1, 1, 1, GETDATE(), 0);
            END
        ").GetAwaiter().GetResult();

        db.Database.ExecuteSqlRawAsync(@"
            -- Assign admin@vueadmin.com to SuperAdmin role
            DECLARE @saRoleId INT;
            SELECT @saRoleId = [UserRoleId] FROM [dbo].[UserRole] WHERE [RoleName] = N'SuperAdmin';
            IF @saRoleId IS NOT NULL
            BEGIN
                UPDATE [dbo].[Users]
                SET [UserRoleId] = @saRoleId,
                    [FullName] = N'John Doe'
                WHERE [Email] = N'admin@vueadmin.com';
            END
        ").GetAwaiter().GetResult();
    }
    catch (Exception ex) { Console.WriteLine($"SuperAdmin setup error: {ex.Message}"); }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors(AllowSpecificOrigins);

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
    RequestPath = new PathString("/Resources")
});
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
    options.RoutePrefix=string.Empty;
});

app.Run();
