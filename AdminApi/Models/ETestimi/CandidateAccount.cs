using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class CandidateAccount
    {
        public int CandidateAccountId { get; set; }

        public int? CandidateId { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty;

        public string? PasswordSalt { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public int? LastUpdatedBy { get; set; }

        public ICollection<CandidateAccountExamCategoryAccess> ExamCategoryAccesses { get; set; } = new List<CandidateAccountExamCategoryAccess>();
    }
}
