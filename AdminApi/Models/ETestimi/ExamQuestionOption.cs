using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class ExamQuestionOption
    {
        public int ExamQuestionOptionId { get; set; }

        [Required]
        public int ExamQuestionId { get; set; }

        [Required]
        [StringLength(1000)]
        public string OptionText { get; set; } = string.Empty;

        [Required]
        public bool IsCorrect { get; set; }

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
