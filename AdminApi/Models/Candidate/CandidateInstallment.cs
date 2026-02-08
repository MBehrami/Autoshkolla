using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    public class CandidateInstallment
    {
        [Key]
        public int InstallmentId { get; set; }
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int InstallmentNumber { get; set; } // 1, 2, or 3
        [Required]
        public int Amount { get; set; }
        [StringLength(20)]
        public string? InstallmentDate { get; set; } // Format: dd.MM.yyyy
        [Required]
        public int AddedBy { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}
