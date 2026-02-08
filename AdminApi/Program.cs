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
            ClockSkew = TimeSpan.Zero
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
                    [Vehicle] NVARCHAR(100) NULL,
                    [DateAdded] DATETIME2 NOT NULL,
                    CONSTRAINT [PK_PracticalLessons] PRIMARY KEY ([PracticalLessonId]),
                    CONSTRAINT [FK_PracticalLessons_Candidates] FOREIGN KEY ([CandidateId]) REFERENCES [dbo].[Candidates]([CandidateId]),
                    CONSTRAINT [FK_PracticalLessons_Users] FOREIGN KEY ([InstructorUserId]) REFERENCES [dbo].[Users]([UserId])
                );
            END
        ").GetAwaiter().GetResult();
    }
    catch { /* Table may already exist */ }
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
