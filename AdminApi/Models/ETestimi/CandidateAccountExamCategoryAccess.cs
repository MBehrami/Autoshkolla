using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class CandidateAccountExamCategoryAccess
    {
        [Required]
        public int CandidateAccountId { get; set; }

        [Required]
        public int ExamCategoryId { get; set; }

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

        public CandidateAccount? CandidateAccount { get; set; }

        public ExamCategory? ExamCategory { get; set; }
    }
}
