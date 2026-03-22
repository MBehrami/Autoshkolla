using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class ExamCategory
    {
        public int ExamCategoryId { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

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
