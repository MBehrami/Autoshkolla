using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }
        [StringLength(4)]
        public string? SerialNumber { get; set; } // Nr. Rendor - max 4 digits (optional)
        [Required]
        [StringLength(100)]
        public string? FirstName { get; set; }
        [StringLength(100)]
        public string? ParentName { get; set; }
        [Required]
        [StringLength(100)]
        public string? LastName { get; set; }
        [StringLength(20)]
        public string? DateOfBirth { get; set; } // Format: dd.MM.yyyy
        [StringLength(10)]
        public string? PersonalNumber { get; set; } // Max 10 digits
        [StringLength(50)]
        public string? PhoneNumber { get; set; }
        [StringLength(200)]
        public string? PlaceOfBirth { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public int? InstructorId { get; set; } // Reference to Users table (Instructor)
        [StringLength(20)]
        public string? VehicleType { get; set; } // Manual / Automatic
        [StringLength(20)]
        public string? PaymentMethod { get; set; } // Bank / Cash
        public int? PracticalHours { get; set; }
        [Required]
        public int TotalServiceAmount { get; set; }

        /// <summary>Document Withdrawal Payment (Terheqja e Dokumentave)</summary>
        public int? DocWithdrawalAmount { get; set; }
        [StringLength(20)]
        public string? DocWithdrawalDate { get; set; }

        /// <summary>Driving Payment (Pagesa e vozitjes)</summary>
        public int? DrivingPaymentAmount { get; set; }
        [StringLength(20)]
        public string? DrivingPaymentDate { get; set; }

        [Required]
        public int AddedBy { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }
        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}
