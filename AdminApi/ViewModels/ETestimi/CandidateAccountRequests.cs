using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.ViewModels.ETestimi
{
    public class CandidateAccountCreateRequest
    {
        public int? CandidateId { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(200)]
        public string InitialPassword { get; set; } = string.Empty;

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> ExamCategoryIds { get; set; } = new();
    }

    public class CandidateAccountUpdateRequest
    {
        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> ExamCategoryIds { get; set; } = new();
    }

    public class CandidateAccountSetPasswordRequest
    {
        [Required]
        [StringLength(200)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class CandidateLoginRequest
    {
        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Password { get; set; } = string.Empty;
    }

    public class CandidateChangePasswordRequest
    {
        [Required]
        [StringLength(200)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ExamCategoryUpsertRequest
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class ExamUpsertRequest
    {
        [Required]
        public int ExamCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DurationMinutes { get; set; } = 45;

        public int PassPercent { get; set; } = 85;

        public int SortOrder { get; set; }
    }

    public class ExamQuestionOptionItem
    {
        [Required]
        [StringLength(1000)]
        public string OptionText { get; set; } = string.Empty;

        [Required]
        public bool IsCorrect { get; set; }

        public int SortOrder { get; set; }
    }

    public class ExamQuestionUpsertRequest
    {
        [Required]
        public int ExamId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;

        [StringLength(36)]
        public string? ImageGuid { get; set; }

        public int SortOrder { get; set; }

        [Required]
        public List<ExamQuestionOptionItem> Options { get; set; } = new();
    }

    public class ExamSubmitAnswerItem
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int SelectedOptionIndex { get; set; }
    }

    public class SubmitExamRequest
    {
        [Required]
        [StringLength(20)]
        public string CategoryCode { get; set; } = string.Empty;

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int DurationSeconds { get; set; }

        [Required]
        public List<ExamSubmitAnswerItem> Answers { get; set; } = new();
    }

    public class CandidateCategoryStatsItem
    {
        public string CategoryCode { get; set; } = string.Empty;

        public int Attempts { get; set; }

        public decimal AverageScorePercent { get; set; }
    }

    public class CandidateDashboardStatsResponse
    {
        public int TotalAttempts { get; set; }

        public int PassedAttempts { get; set; }

        public decimal PassRatePercent { get; set; }

        public decimal AverageScorePercent { get; set; }

        public List<CandidateCategoryStatsItem> CategoryPerformance { get; set; } = new();
    }
}
