using AdminApi.Models.Helper;
using AdminApi.Models.Menu;
using AdminApi.Models.Others;
using AdminApi.Models.User;
using CandidateModel = AdminApi.Models.Candidate;
using VehicleModel = AdminApi.Models.Vehicle;
using ScheduleModel = AdminApi.Models.Schedule;
using ReportModel = AdminApi.Models.Report;
using ETestimiModel = AdminApi.Models.ETestimi;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Models
{
    public class AppDbContext : DbContext
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
        public virtual DbSet<CandidateModel.AdditionalLesson> AdditionalLessons { get; set; }
        public virtual DbSet<CandidateModel.AdditionalLessonInstallment> AdditionalLessonInstallments { get; set; }
        public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

        public virtual DbSet<VehicleModel.Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehicleModel.VehicleFuel> VehicleFuels { get; set; }
        public virtual DbSet<VehicleModel.VehicleService> VehicleServices { get; set; }
        public virtual DbSet<ScheduleModel.ScheduleEvent> ScheduleEvents { get; set; }
        public virtual DbSet<ReportModel.DailyReportEntry> DailyReportEntries { get; set; }
        public virtual DbSet<ReportModel.DailyReportStatus> DailyReportStatuses { get; set; }

        public virtual DbSet<ETestimiModel.CandidateAccount> CandidateAccounts { get; set; }
        public virtual DbSet<ETestimiModel.CandidateAccountExamCategoryAccess> CandidateAccountExamCategoryAccesses { get; set; }
        public virtual DbSet<ETestimiModel.ExamCategory> ExamCategories { get; set; }
        public virtual DbSet<ETestimiModel.Exam> Exams { get; set; }
        public virtual DbSet<ETestimiModel.ExamQuestion> ExamQuestions { get; set; }
        public virtual DbSet<ETestimiModel.ExamQuestionOption> ExamQuestionOptions { get; set; }
        public virtual DbSet<ETestimiModel.ExamSubmission> ExamSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ETestimiModel.CandidateAccount>()
                .HasIndex(x => x.CandidateId)
                .IsUnique()
                .HasFilter("[CandidateId] IS NOT NULL");

            modelBuilder.Entity<ETestimiModel.CandidateAccount>()
                .HasIndex(x => x.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<ETestimiModel.CandidateAccount>()
                .HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<ETestimiModel.CandidateAccount>()
                .HasOne<CandidateModel.Candidate>()
                .WithMany()
                .HasForeignKey(x => x.CandidateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ETestimiModel.CandidateAccountExamCategoryAccess>()
                .ToTable("CandidateAccountExamCategoryAccesses")
                .HasKey(x => new { x.CandidateAccountId, x.ExamCategoryId });

            modelBuilder.Entity<ETestimiModel.CandidateAccountExamCategoryAccess>()
                .HasOne(x => x.CandidateAccount)
                .WithMany(x => x.ExamCategoryAccesses)
                .HasForeignKey(x => x.CandidateAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ETestimiModel.CandidateAccountExamCategoryAccess>()
                .HasOne(x => x.ExamCategory)
                .WithMany()
                .HasForeignKey(x => x.ExamCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ETestimiModel.CandidateAccountExamCategoryAccess>()
                .HasIndex(x => x.ExamCategoryId);

            modelBuilder.Entity<ETestimiModel.CandidateAccountExamCategoryAccess>()
                .HasIndex(x => new { x.CandidateAccountId, x.IsActive });

            modelBuilder.Entity<ETestimiModel.ExamCategory>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<ETestimiModel.Exam>()
                .HasOne<ETestimiModel.ExamCategory>()
                .WithMany()
                .HasForeignKey(x => x.ExamCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ETestimiModel.ExamQuestion>()
                .HasOne<ETestimiModel.Exam>()
                .WithMany()
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ETestimiModel.ExamQuestionOption>()
                .HasOne<ETestimiModel.ExamQuestion>()
                .WithMany(q => q.QuestionOptions)
                .HasForeignKey(x => x.ExamQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ETestimiModel.ExamSubmission>()
                .HasOne<ETestimiModel.CandidateAccount>()
                .WithMany()
                .HasForeignKey(x => x.CandidateAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ETestimiModel.ExamSubmission>()
                .HasOne<ETestimiModel.Exam>()
                .WithMany()
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ETestimiModel.ExamSubmission>()
                .HasOne<ETestimiModel.ExamCategory>()
                .WithMany()
                .HasForeignKey(x => x.ExamCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ETestimiModel.ExamSubmission>()
                .HasIndex(x => new { x.CandidateAccountId, x.SubmittedAt });

            modelBuilder.Entity<ETestimiModel.ExamSubmission>()
                .HasIndex(x => new { x.CandidateAccountId, x.ExamCategoryId });

            modelBuilder.Seed();
        }
    }
}
