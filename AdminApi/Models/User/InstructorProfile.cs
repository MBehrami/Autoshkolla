using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.User
{
    /// <summary>
    /// Extended profile for users with Instructor role. Linked to Users by UserId.
    /// </summary>
    public class InstructorProfile
    {
        [Key]
        public int InstructorProfileId { get; set; }
        [Required]
        public int UserId { get; set; }
        [StringLength(100)]
        public string? FirstName { get; set; }
        [StringLength(100)]
        public string? ParentName { get; set; }
        [StringLength(100)]
        public string? LastName { get; set; }
        [StringLength(10)]
        public string? PersonalNumber { get; set; }
        /// <summary>Full-time / Part-time (orar te plote / honorar)</summary>
        [StringLength(20)]
        public string? ScheduleType { get; set; }
        [StringLength(50)]
        public string? LicenseNumber { get; set; }
        /// <summary>Format dd.MM.yyyy</summary>
        [StringLength(20)]
        public string? LicenseValidityDate { get; set; }
        [StringLength(500)]
        public string? LicensePhotoPath { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}
