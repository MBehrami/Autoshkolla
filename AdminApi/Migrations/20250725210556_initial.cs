using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdminApi.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactId);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    ErrorLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: true),
                    StatusText = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AddedBy = table.Column<int>(type: "int", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.ErrorLogId);
                });

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    FaqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.FaqId);
                });

            migrationBuilder.CreateTable(
                name: "LogHistory",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrowserVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogHistory", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    MenuID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    MenuTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    URL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSubMenu = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.MenuID);
                });

            migrationBuilder.CreateTable(
                name: "MenuGroup",
                columns: table => new
                {
                    MenuGroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuGroup", x => x.MenuGroupID);
                });

            migrationBuilder.CreateTable(
                name: "MenuGroupWiseMenuMapping",
                columns: table => new
                {
                    MenuGroupWiseMenuMappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuGroupId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuGroupWiseMenuMapping", x => x.MenuGroupWiseMenuMappingId);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    SiteSettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WelComeMessage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CopyRightText = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FaviconPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AppBarColor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HeaderColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FooterColor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BodyColor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AllowWelcomeEmail = table.Column<bool>(type: "bit", nullable: false),
                    AllowFaq = table.Column<bool>(type: "bit", nullable: false),
                    AllowRightClick = table.Column<bool>(type: "bit", nullable: false),
                    ClientUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    HomeHeader1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeDetail1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomePicture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HomeHeader2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeDetail2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox1Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox1Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox2Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox2Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox3Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox3Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox4Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeBox4Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature1Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature1Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature1Picture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Feature2Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature2Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature2Picture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Feature3Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature3Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature3Picture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Feature4Header = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature4Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feature4Picture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistrationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUsText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUsTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUsEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterFacebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterTwitter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterLinkedin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterInstagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForgetPasswordEmailSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForgetPasswordEmailHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForgetPasswordEmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelcomeEmailSubject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelcomeEmailHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WelcomeEmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.SiteSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MenuGroupId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRoleId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ForgetPasswordRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedBy = table.Column<int>(type: "int", nullable: false),
                    IsMigrationData = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPasswordChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordChangedBy = table.Column<int>(type: "int", nullable: true),
                    IsPasswordChange = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "FaqId", "AddedBy", "DateAdded", "Description", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate", "Title" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8942), "Vue Admin is a single page admin template developed by Vue with .Net core 8 API. It’s covered most common features that you need to start a project.", true, true, null, null, "What are the purposes of this app?" },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8945), "The most amazing part of this template is, you have five popular Relational database connectivity options here. You have flexibility to choose Sql server, Mysql, Sqlite, PostgreSql and Oracle 12c+.", true, true, null, null, "Why this app differs from others?" }
                });

            migrationBuilder.InsertData(
                table: "Menu",
                columns: new[] { "MenuID", "AddedBy", "DateAdded", "IconClass", "IsActive", "IsMigrationData", "IsSubMenu", "LastUpdatedBy", "LastUpdatedDate", "MenuTitle", "ParentID", "SortOrder", "URL" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8032), "mdi-menu", false, true, 1, null, null, "Menus", 0, 2, "" },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8036), "", false, true, 0, null, null, "All Menu", 1, 0, "/menu" },
                    { 3, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8039), "", false, true, 0, null, null, "Menu Group", 1, 0, "/menu-group" },
                    { 4, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8042), "mdi-account", false, true, 1, null, null, "Users", 0, 3, "" },
                    { 5, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8044), "", false, true, 0, null, null, "All User", 4, 0, "/users" },
                    { 6, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8046), "", false, true, 0, null, null, "Roles", 4, 0, "/user-role" },
                    { 7, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8049), "mdi-history", false, true, 1, null, null, "Logs", 0, 4, "" },
                    { 8, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8051), "", false, true, 0, null, null, "Browsing Log", 7, 0, "/browse-log" },
                    { 9, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8055), "", false, true, 0, null, null, "Error Log", 7, 0, "/error-log" },
                    { 10, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8083), "mdi-home", true, true, 0, null, null, "Dashboard", 0, 1, "/dashboard" },
                    { 11, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8085), "mdi-frequently-asked-questions", false, true, 0, null, null, "FAQ", 0, 5, "/faq" },
                    { 12, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8088), "mdi-contacts", false, true, 0, null, null, "Contact", 0, 6, "/contact" },
                    { 13, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8090), "mdi-cog", false, true, 0, null, null, "App Settings", 0, 7, "/settings" }
                });

            migrationBuilder.InsertData(
                table: "MenuGroup",
                columns: new[] { "MenuGroupID", "AddedBy", "DateAdded", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate", "MenuGroupName" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(6753), true, true, null, null, "Super Admin Group" },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(6805), true, true, null, null, "User Group" }
                });

            migrationBuilder.InsertData(
                table: "MenuGroupWiseMenuMapping",
                columns: new[] { "MenuGroupWiseMenuMappingId", "AddedBy", "DateAdded", "IsActive", "IsMigrationData", "MenuGroupId", "MenuId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8413), true, true, 1, 2 },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8421), true, true, 1, 3 },
                    { 3, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8424), true, true, 1, 5 },
                    { 4, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8426), true, true, 1, 6 },
                    { 5, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8428), true, true, 1, 8 },
                    { 6, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8430), true, true, 1, 9 },
                    { 7, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8432), true, true, 1, 10 },
                    { 8, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8434), true, true, 1, 11 },
                    { 9, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8436), true, true, 1, 12 },
                    { 10, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8438), true, true, 1, 13 },
                    { 11, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8439), true, true, 2, 10 }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "SiteSettingsId", "AddedBy", "AllowFaq", "AllowRightClick", "AllowWelcomeEmail", "AppBarColor", "BodyColor", "ClientUrl", "ContactUsEmail", "ContactUsTelephone", "ContactUsText", "CopyRightText", "DateAdded", "DefaultEmail", "DisplayName", "FaviconPath", "Feature1Detail", "Feature1Header", "Feature1Picture", "Feature2Detail", "Feature2Header", "Feature2Picture", "Feature3Detail", "Feature3Header", "Feature3Picture", "Feature4Detail", "Feature4Header", "Feature4Picture", "FooterColor", "FooterFacebook", "FooterInstagram", "FooterLinkedin", "FooterText", "FooterTwitter", "ForgetPasswordEmailBody", "ForgetPasswordEmailHeader", "ForgetPasswordEmailSubject", "HeaderColor", "HomeBox1Detail", "HomeBox1Header", "HomeBox2Detail", "HomeBox2Header", "HomeBox3Detail", "HomeBox3Header", "HomeBox4Detail", "HomeBox4Header", "HomeDetail1", "HomeDetail2", "HomeHeader1", "HomeHeader2", "HomePicture", "Host", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate", "LogoPath", "Password", "Port", "RegistrationText", "SiteTitle", "Version", "WelComeMessage", "WelcomeEmailBody", "WelcomeEmailHeader", "WelcomeEmailSubject" },
                values: new object[] { 1, 1, true, true, true, "#363636", "#F9F9F9", null, "email@email.com", "+xx (xx) xxxxx-xxxx", "Lorem ipsum dolor sit amet consectetur adipisicing elit. Iste explicabo commodi quisquam asperiores dolore ad enim provident veniam perferendis voluptate, perspiciatis. ", "© 2024 Copyright Vue Admin", new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(8725), "", null, "", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Lorem ipsum", "", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Lorem ipsum", "", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Lorem ipsum", "", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Lorem ipsum", "", "#FFFFFF", "", "", "", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.", "", "Forget Password Body", "Forget Password Header", "Forget Password", "#F5F5F5", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", "Lorem ipsum", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", "Lorem ipsum", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", "Lorem ipsum", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", "Lorem ipsum", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Lorem ipsum dolor sit amet !", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam", "", "smtp.gmail.com", true, true, null, null, "", "", 587, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", "Vue Admin", 1, "Hello there,Sign in to start your task!", "Welcome Body", "Welcome Header", "Welcome" });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "UserRoleId", "AddedBy", "DateAdded", "IsActive", "IsMigrationData", "LastUpdatedBy", "LastUpdatedDate", "MenuGroupId", "RoleDesc", "RoleName" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7121), true, true, null, null, 1, null, "Admin" },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7125), true, true, null, null, 2, null, "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "AddedBy", "DateAdded", "DateOfBirth", "Email", "ForgetPasswordRef", "FullName", "ImagePath", "IsActive", "IsMigrationData", "IsPasswordChange", "LastPasswordChangeDate", "LastUpdatedBy", "LastUpdatedDate", "Mobile", "Password", "PasswordChangedBy", "PasswordSalt", "UserRoleId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7701), null, "admin@vueadmin.com", null, "John Doe", null, true, true, false, null, null, null, null, "c7JZ+u1vrHp/FQqdD69JQhBTDoLnCNba6dqVyKpEXu4=", null, "3fdcuIxZk3sdxmjJPM0FuA==", 1 },
                    { 2, 1, new DateTime(2025, 7, 25, 23, 5, 56, 423, DateTimeKind.Local).AddTicks(7707), null, "user@vueadmin.com", null, "Helen Smith", null, true, true, false, null, null, null, null, "AJNri22q/JA3fbO4m0QIRYxEChegS461t7J1f7C/td8=", null, "gLtMyAEbVeLjZGj312VXyg==", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "LogHistory");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "MenuGroup");

            migrationBuilder.DropTable(
                name: "MenuGroupWiseMenuMapping");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
