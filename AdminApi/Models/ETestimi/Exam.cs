using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class Exam
    {
        public int ExamId { get; set; }

        [Required]
        public int ExamCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DurationMinutes { get; set; } = 45;

        public int PassPercent { get; set; } = 85;

        public int SortOrder { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public int? LastUpdatedBy { get; set; }
    }
}
