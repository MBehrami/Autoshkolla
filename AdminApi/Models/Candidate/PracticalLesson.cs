using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    /// <summary>
    /// A practical driving lesson linked to a candidate and the instructor who created it.
    /// </summary>
    public class PracticalLesson
    {
        [Key]
        public int PracticalLessonId { get; set; }
        [Required]
        public int CandidateId { get; set; }
        [Required]
        public int InstructorUserId { get; set; }
        /// <summary>Format dd.MM.yyyy</summary>
        [Required]
        [StringLength(20)]
        public string LessonDate { get; set; } = string.Empty;
        /// <summary>Time slot e.g. 08:00, 09:00</summary>
        [Required]
        [StringLength(10)]
        public string Time { get; set; } = string.Empty;
        [StringLength(100)]
        public string? Vehicle { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }
    }
}
