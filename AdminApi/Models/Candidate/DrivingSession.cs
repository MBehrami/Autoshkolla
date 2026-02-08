using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    /// <summary>
    /// A driving session (Vozitje) linking a candidate, vehicle, and instructor on a specific date/time.
    /// </summary>
    public class DrivingSession
    {
        [Key]
        public int DrivingSessionId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        /// <summary>Instructor who created / is assigned to the session.</summary>
        public int? InstructorUserId { get; set; }

        /// <summary>Driving date in dd.MM.yyyy format.</summary>
        [Required]
        [StringLength(20)]
        public string DrivingDate { get; set; } = string.Empty;

        /// <summary>Start time HH:mm (e.g. 08:00, 08:15).</summary>
        [Required]
        [StringLength(10)]
        public string DrivingTime { get; set; } = string.Empty;

        /// <summary>Payment amount for this session.</summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>Payment date dd.MM.yyyy.</summary>
        [StringLength(20)]
        public string? PaymentDate { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}
