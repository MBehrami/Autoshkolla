using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.ETestimi
{
    public class ExamSubmission
    {
        public int ExamSubmissionId { get; set; }

        [Required]
        public int CandidateAccountId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int ExamCategoryId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal ScorePercent { get; set; }

        [Required]
        public bool IsPassed { get; set; }

        [Required]
        public int TotalQuestions { get; set; }

        [Required]
        public int CorrectAnswers { get; set; }

        [Required]
        public int DurationSeconds { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}
