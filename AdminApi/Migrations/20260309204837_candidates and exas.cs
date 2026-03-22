using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdminApi.Migrations
{
    /// <inheritdoc />
    public partial class candidatesandexas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DeleteData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 12);

            migrationBuilder.CreateTable(
                name: "CandidateInstallments",
                columns: table => new
                {
                    InstallmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    InstallmentDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateInstallments", x => x.InstallmentId);
                });

            migrationBuilder.CreateTable(
                name: "Candidates",
                columns: table => new
                {
                    CandidateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonalNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: true),
                    VehicleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PracticalHours = table.Column<int>(type: "int", nullable: true),
                    TotalServiceAmount = table.Column<int>(type: "int", nullable: false),
                    DocWithdrawalAmount = table.Column<int>(type: "int", nullable: true),
                    DocWithdrawalDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DrivingPaymentAmount = table.Column<int>(type: "int", nullable: true),
                    DrivingPaymentDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidates", x => x.CandidateId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportEntries",
                columns: table => new
                {
                    DailyReportEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    ReversalOfEntryId = table.Column<int>(type: "int", nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportEntries", x => x.DailyReportEntryId);
                });

            migrationBuilder.CreateTable(
                name: "DailyReportStatuses",
                columns: table => new
                {
                    DailyReportStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClosedBy = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportStatuses", x => x.DailyReportStatusId);
                });

            migrationBuilder.CreateTable(
                name: "DrivingSessions",
                columns: table => new
                {
                    DrivingSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    InstructorUserId = table.Column<int>(type: "int", nullable: true),
                    DrivingDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DrivingTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Examiner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrivingSessions", x => x.DrivingSessionId);
                });

            migrationBuilder.CreateTable(
                name: "ExamCategories",
                columns: table => new
                {
                    ExamCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCategories", x => x.ExamCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "InstructorProfiles",
                columns: table => new
                {
                    InstructorProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PersonalNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ScheduleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LicenseValidityDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LicensePhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorProfiles", x => x.InstructorProfileId);
                });

            migrationBuilder.CreateTable(
                name: "PracticalLessons",
                columns: table => new
                {
                    PracticalLessonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    InstructorUserId = table.Column<int>(type: "int", nullable: false),
                    LessonDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Time = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Vehicle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticalLessons", x => x.PracticalLessonId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleEvents",
                columns: table => new
                {
                    ScheduleEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InstructorUserId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleEvents", x => x.ScheduleEventId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleFuels",
                columns: table => new
                {
                    VehicleFuelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FillDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    FuelAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StaffUserId = table.Column<int>(type: "int", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleFuels", x => x.VehicleFuelId);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChassisNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpiryDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleServices",
                columns: table => new
                {
                    VehicleServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ServiceDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleServices", x => x.VehicleServiceId);
                });

            migrationBuilder.CreateTable(
                name: "CandidateAccounts",
                columns: table => new
                {
                    CandidateAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateAccounts", x => x.CandidateAccountId);
                    table.ForeignKey(
                        name: "FK_CandidateAccounts_Candidates_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Candidates",
                        principalColumn: "CandidateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    ExamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamCategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    PassPercent = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.ExamId);
                    table.ForeignKey(
                        name: "FK_Exams_ExamCategories_ExamCategoryId",
                        column: x => x.ExamCategoryId,
                        principalTable: "ExamCategories",
                        principalColumn: "ExamCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    ExamQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.ExamQuestionId);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestionOptions",
                columns: table => new
                {
                    ExamQuestionOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamQuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestionOptions", x => x.ExamQuestionOptionId);
                    table.ForeignKey(
                        name: "FK_ExamQuestionOptions_ExamQuestions_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "ExamQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "AddedBy", "CategoryName", "DateAdded", "Description", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate" },
                values: new object[,]
                {
                    { 1, 1, "A1", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2817), null, true, true, null, null },
                    { 2, 1, "A2", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2820), null, true, true, null, null },
                    { 3, 1, "A", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2822), null, true, true, null, null },
                    { 4, 1, "B", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2823), null, true, true, null, null },
                    { 5, 1, "B+E", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2824), null, true, true, null, null },
                    { 6, 1, "C1", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2826), null, true, true, null, null },
                    { 7, 1, "C", new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2827), null, true, true, null, null }
                });

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2105));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2109));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2111));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2113));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2114));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2116));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2118));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2119));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2121));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2123));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2124));

            migrationBuilder.InsertData(
                table: "Menu",
                columns: new[] { "MenuID", "AddedBy", "DateAdded", "IconClass", "IsActive", "IsMigrationData", "IsSubMenu", "LastUpdatedBy", "LastUpdatedDate", "MenuTitle", "ParentID", "SortOrder", "URL" },
                values: new object[,]
                {
                    { 14, 1, new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2126), "mdi-account-group", true, true, 0, null, null, "Candidates", 0, 8, "/candidates" },
                    { 15, 1, new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2128), "mdi-account-tie", true, true, 0, null, null, "Instructors", 0, 9, "/instructors" }
                });

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1084));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1105));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2267));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2269));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2271));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2272));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2274));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2275));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2276));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2278));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2279));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2280));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 11,
                columns: new[] { "DateAdded", "MenuGroupId", "MenuId" },
                values: new object[] { new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2282), 1, 14 });

            migrationBuilder.InsertData(
                table: "MenuGroupWiseMenuMapping",
                columns: new[] { "MenuGroupWiseMenuMappingId", "AddedBy", "DateAdded", "IsActive", "IsMigrationData", "MenuGroupId", "MenuId" },
                values: new object[,]
                {
                    { 12, 1, new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2283), true, true, 1, 15 },
                    { 13, 1, new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2285), true, true, 2, 10 }
                });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "SiteSettingsId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(2465));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1338));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1341));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1773));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2026, 3, 9, 21, 48, 36, 993, DateTimeKind.Local).AddTicks(1778));

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAccounts_CandidateId",
                table: "CandidateAccounts",
                column: "CandidateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAccounts_Email",
                table: "CandidateAccounts",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateAccounts_PhoneNumber",
                table: "CandidateAccounts",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamCategories_Code",
                table: "ExamCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestionOptions_ExamQuestionId",
                table: "ExamQuestionOptions",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_ExamId",
                table: "ExamQuestions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ExamCategoryId",
                table: "Exams",
                column: "ExamCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateAccounts");

            migrationBuilder.DropTable(
                name: "CandidateInstallments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "DailyReportEntries");

            migrationBuilder.DropTable(
                name: "DailyReportStatuses");

            migrationBuilder.DropTable(
                name: "DrivingSessions");

            migrationBuilder.DropTable(
                name: "ExamQuestionOptions");

            migrationBuilder.DropTable(
                name: "InstructorProfiles");

            migrationBuilder.DropTable(
                name: "PracticalLessons");

            migrationBuilder.DropTable(
                name: "ScheduleEvents");

            migrationBuilder.DropTable(
                name: "VehicleFuels");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleServices");

            migrationBuilder.DropTable(
                name: "Candidates");

            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "ExamCategories");

            migrationBuilder.DeleteData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 13);

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactId);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    FaqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.FaqId);
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "FaqId", "AddedBy", "DateAdded", "Description", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate", "Title" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8942), "Vue Admin is a single page admin template developed by Vue with .Net core 8 API. It’s covered most common features that you need to start a project.", true, true, null, null, "What are the purposes of this app?" },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8945), "The most amazing part of this template is, you have five popular Relational database connectivity options here. You have flexibility to choose Sql server, Mysql, Sqlite, PostgreSql and Oracle 12c+.", true, true, null, null, "Why this app differs from others?" }
                });

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8032));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8036));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8039));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8042));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8044));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8046));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8049));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8051));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8055));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8083));

            migrationBuilder.UpdateData(
                table: "Menu",
                keyColumn: "MenuID",
                keyValue: 13,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8090));

            migrationBuilder.InsertData(
                table: "Menu",
                columns: new[] { "MenuID", "AddedBy", "DateAdded", "IconClass", "IsActive", "IsMigrationData", "IsSubMenu", "LastUpdatedBy", "LastUpdatedDate", "MenuTitle", "ParentID", "SortOrder", "URL" },
                values: new object[,]
                {
                    { 11, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8085), "mdi-frequently-asked-questions", false, true, 0, null, null, "FAQ", 0, 5, "/faq" },
                    { 12, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8088), "mdi-contacts", false, true, 0, null, null, "Contact", 0, 6, "/contact" }
                });

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(6753));

            migrationBuilder.UpdateData(
                table: "MenuGroup",
                keyColumn: "MenuGroupID",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(6805));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8413));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8421));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 3,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8424));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 4,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8426));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 5,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8428));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 6,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 7,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8432));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 8,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8434));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 9,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8436));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 10,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8438));

            migrationBuilder.UpdateData(
                table: "MenuGroupWiseMenuMapping",
                keyColumn: "MenuGroupWiseMenuMappingId",
                keyValue: 11,
                columns: new[] { "DateAdded", "MenuGroupId", "MenuId" },
                values: new object[] { new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8439), 2, 10 });

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "SiteSettingsId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8725));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7121));

            migrationBuilder.UpdateData(
                table: "UserRole",
                keyColumn: "UserRoleId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7125));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7701));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "DateAdded",
                value: new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7707));
        }
    }
}
