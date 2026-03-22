using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class ExamQuestion
    {
        public int ExamQuestionId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;

        [StringLength(500)]
        [Obsolete("Use ImageGuid instead. ImagePath is deprecated and will be removed in a future version.")]
        public string? ImagePath { get; set; }

        [StringLength(36)]
        public string? ImageGuid { get; set; }

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

        // Navigation property
        public ICollection<ExamQuestionOption>? QuestionOptions { get; set; }
    }
}
