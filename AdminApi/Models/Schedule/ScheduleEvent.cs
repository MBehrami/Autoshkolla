using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Schedule
{
    /// <summary>
    /// A schedule event (Orar) linking instructor, candidate and vehicle at a specific date/time slot.
    /// </summary>
    public class ScheduleEvent
    {
        [Key]
        public int ScheduleEventId { get; set; }

        /// <summary>Event date in dd.MM.yyyy format.</summary>
        [Required]
        [StringLength(20)]
        public string EventDate { get; set; } = string.Empty;

        /// <summary>Start time HH:mm (e.g. 08:00).</summary>
        [Required]
        [StringLength(10)]
        public string StartTime { get; set; } = string.Empty;

        /// <summary>End time HH:mm (e.g. 08:45).</summary>
        [Required]
        [StringLength(10)]
        public string EndTime { get; set; } = string.Empty;

        [Required]
        public int InstructorUserId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}
