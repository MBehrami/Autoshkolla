using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    public class AdditionalLessonInstallment
    {
        [Key]
        public int InstallmentId { get; set; }

        [Required]
        public int AdditionalLessonId { get; set; }

        [Required]
        public int InstallmentNumber { get; set; }

        [Required]
        public int Amount { get; set; }

        [StringLength(20)]
        public string? InstallmentDate { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}
