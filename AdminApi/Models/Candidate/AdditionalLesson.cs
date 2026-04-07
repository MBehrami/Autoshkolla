using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    public class AdditionalLesson
    {
        [Key]
        public int AdditionalLessonId { get; set; }

        public int? LinkedCandidateId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PersonalNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public int? InstructorId { get; set; }

        [StringLength(20)]
        public string? VehicleType { get; set; }

        [StringLength(20)]
        public string? PaymentMethod { get; set; }

        public int? PracticalHours { get; set; }

        public decimal ServicePayment { get; set; }

        public string? AdditionalNotes { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
    }
}
