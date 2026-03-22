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

        /// <summary>Nullable for manual-entry candidates not in the database.</summary>
        public int? CandidateId { get; set; }

        /// <summary>Name typed manually when candidate is not in the system.</summary>
        [StringLength(200)]
        public string? ManualCandidateName { get; set; }

        [Required]
        public int VehicleId { get; set; }

        /// <summary>Instructor who created / is assigned to the session.</summary>
        public int? InstructorUserId { get; set; }

        /// <summary>Driving date in dd.MM.yyyy format. Null when on the waiting list.</summary>
        [StringLength(20)]
        public string? DrivingDate { get; set; }

        /// <summary>Start time HH:mm (e.g. 08:00, 08:15). Null when on the waiting list.</summary>
        [StringLength(10)]
        public string? DrivingTime { get; set; }

        /// <summary>Payment amount for this session.</summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>Payment date dd.MM.yyyy.</summary>
        [StringLength(20)]
        public string? PaymentDate { get; set; }

        /// <summary>Session status: null (not set), Kaloi, Deshtoi, Anuloi.</summary>
        [StringLength(20)]
        public string? Status { get; set; }

        /// <summary>Examiner name (Egzamineri) – free text, set during edit.</summary>
        [StringLength(100)]
        public string? Examiner { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}
