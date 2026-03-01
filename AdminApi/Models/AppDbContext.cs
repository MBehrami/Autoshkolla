using AdminApi.Models.Helper;
using AdminApi.Models.Menu;
using AdminApi.Models.Others;
using AdminApi.Models.User;
using CandidateModel = AdminApi.Models.Candidate;
using VehicleModel = AdminApi.Models.Vehicle;
using ScheduleModel = AdminApi.Models.Schedule;
using ReportModel = AdminApi.Models.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminApi.Models
{
    public class AppDbContext:DbContext
    {     
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }
        public virtual DbSet<LogHistory> LogHistory { get; set; }
        public virtual DbSet<AppMenu> Menu { get; set; }
        public virtual DbSet<MenuGroup> MenuGroup { get; set; }
        public virtual DbSet<MenuGroupWiseMenuMapping> MenuGroupWiseMenuMapping { get; set; }
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<SiteSettings> SiteSettings { get; set; }
        public virtual DbSet<CandidateModel.Category> Categories { get; set; }
        public virtual DbSet<CandidateModel.Candidate> Candidates { get; set; }
        public virtual DbSet<CandidateModel.CandidateInstallment> CandidateInstallments { get; set; }
        public virtual DbSet<CandidateModel.PracticalLesson> PracticalLessons { get; set; }
        public virtual DbSet<CandidateModel.DrivingSession> DrivingSessions { get; set; }
        public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }
        public virtual DbSet<VehicleModel.Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehicleModel.VehicleFuel> VehicleFuels { get; set; }
        public virtual DbSet<VehicleModel.VehicleService> VehicleServices { get; set; }
        public virtual DbSet<ScheduleModel.ScheduleEvent> ScheduleEvents { get; set; }
        public virtual DbSet<ReportModel.DailyReportEntry> DailyReportEntries { get; set; }
        public virtual DbSet<ReportModel.DailyReportStatus> DailyReportStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Seed();//use this for Sql server,Mysql,Sqlite and PostgreSql
        }

    }
}
