using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Report
{
    /// <summary>
    /// A single ledger entry in the Daily Report.
    /// EntryType: "Income" (Hyrjet) or "Expense" (Daljet).
    /// </summary>
    public class DailyReportEntry
    {
        [Key]
        public int DailyReportEntryId { get; set; }

        /// <summary>Entry date in dd.MM.yyyy format.</summary>
        [Required]
        [StringLength(20)]
        public string EntryDate { get; set; } = string.Empty;

        /// <summary>"Income" or "Expense".</summary>
        [Required]
        [StringLength(20)]
        public string EntryType { get; set; } = string.Empty;

        /// <summary>Serial number within the day (auto-incremented per day+type).</summary>
        [Required]
        public int SerialNumber { get; set; }

        /// <summary>Full name (from candidate or free text).</summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>Amount (positive for income/expense, negative for reversal).</summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>Payment reason / description.</summary>
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Source type for auto-entries: "CandidateInstallment", "DrivingSession", "Manual", "Reversal".</summary>
        [StringLength(50)]
        public string? SourceType { get; set; }

        /// <summary>Source record ID (e.g. CandidateInstallment ID or DrivingSession ID).</summary>
        public int? SourceId { get; set; }

        /// <summary>If this entry is a reversal, references the original entry ID.</summary>
        public int? ReversalOfEntryId { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }

    /// <summary>
    /// Tracks whether a day's report is Open or Closed.
    /// </summary>
    public class DailyReportStatus
    {
        [Key]
        public int DailyReportStatusId { get; set; }

        /// <summary>Date in dd.MM.yyyy format.</summary>
        [Required]
        [StringLength(20)]
        public string ReportDate { get; set; } = string.Empty;

        /// <summary>"Open" or "Closed".</summary>
        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "Open";

        public int? ClosedBy { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
