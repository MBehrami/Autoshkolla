using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Models.Report;
using AdminApi.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DailyReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<DailyReportEntry> _entryRepo;
        private readonly ISqlRepository<DailyReportStatus> _statusRepo;
        private readonly ILogger<DailyReportsController> _logger;

        public DailyReportsController(
            AppDbContext context,
            ISqlRepository<DailyReportEntry> entryRepo,
            ISqlRepository<DailyReportStatus> statusRepo,
            ILogger<DailyReportsController> logger)
        {
            _context = context;
            _entryRepo = entryRepo;
            _statusRepo = statusRepo;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DailyReports/GetEntries?date=26.01.2026
        //       &month=01&year=2026&entryType=Income&search=John
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetEntries(
            string? date = null,
            int? month = null,
            int? year = null,
            string? entryType = null,
            string? search = null)
        {
            try
            {
                var query = _context.DailyReportEntries.AsQueryable();

                // Filter by exact date
                if (!string.IsNullOrWhiteSpace(date))
                {
                    string d = date.Trim();
                    query = query.Where(e => e.EntryDate == d);
                }

                // Filter by entry type
                if (!string.IsNullOrWhiteSpace(entryType) && entryType != "All")
                {
                    query = query.Where(e => e.EntryType == entryType.Trim());
                }

                // Search by name
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string s = search.Trim().ToLower();
                    query = query.Where(e => e.FullName.ToLower().Contains(s));
                }

                var entries = await query
                    .OrderByDescending(e => e.DailyReportEntryId)
                    .ToListAsync();

                // Apply month/year filter in memory (dates are dd.MM.yyyy strings)
                var filtered = entries.AsEnumerable();
                if (month.HasValue || year.HasValue)
                {
                    filtered = filtered.Where(e =>
                    {
                        if (!DateTime.TryParseExact(e.EntryDate, "dd.MM.yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                            return false;
                        if (month.HasValue && dt.Month != month.Value) return false;
                        if (year.HasValue && dt.Year != year.Value) return false;
                        return true;
                    });
                }

                var result = filtered.Select(e => new
                {
                    e.DailyReportEntryId,
                    e.EntryDate,
                    e.EntryType,
                    e.SerialNumber,
                    e.FullName,
                    e.Amount,
                    e.Description,
                    e.SourceType,
                    e.SourceId,
                    e.ReversalOfEntryId,
                    e.AddedBy,
                    e.DateAdded,
                    IsAutoEntry = e.SourceType != null && e.SourceType != "Manual" && e.SourceType != "Reversal"
                }).ToList();

                // Totals
                var incomeTotal = result.Where(r => r.EntryType == "Income").Sum(r => r.Amount);
                var expenseTotal = result.Where(r => r.EntryType == "Expense").Sum(r => r.Amount);

                return Ok(new
                {
                    dataCount = result.Count,
                    incomeTotal,
                    expenseTotal,
                    balance = incomeTotal - expenseTotal,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEntries failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DailyReports/GetDayStatus?date=26.01.2026
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetDayStatus(string? date = null)
        {
            try
            {
                string targetDate = date ?? DateTime.Now.ToString("dd.MM.yyyy");
                var status = await _context.DailyReportStatuses
                    .FirstOrDefaultAsync(s => s.ReportDate == targetDate);

                return Ok(new
                {
                    reportDate = targetDate,
                    status = status?.Status ?? "Open",
                    closedBy = status?.ClosedBy,
                    closedAt = status?.ClosedAt
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  POST api/DailyReports/CreateEntry
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateEntry([FromBody] AddDailyReportEntryRequest req)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(req.EntryDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Date is required." });
                if (string.IsNullOrWhiteSpace(req.EntryType) ||
                    (req.EntryType != "Income" && req.EntryType != "Expense"))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Entry type must be 'Income' or 'Expense'." });
                if (string.IsNullOrWhiteSpace(req.FullName))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Full name is required." });
                if (req.Amount <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Amount must be greater than 0." });
                if (string.IsNullOrWhiteSpace(req.Description))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Description is required." });

                string date = req.EntryDate.Trim();
                if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format (dd.MM.yyyy)." });

                // Check day status — don't allow adding to closed day
                var dayStatus = await _context.DailyReportStatuses
                    .FirstOrDefaultAsync(s => s.ReportDate == date);
                if (dayStatus != null && dayStatus.Status == "Closed")
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "This day's report is closed. No new entries allowed." });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                // Calculate serial number for this day + type
                int nextSerial = await GetNextSerialNumber(date, req.EntryType);

                var entry = new DailyReportEntry
                {
                    EntryDate = date,
                    EntryType = req.EntryType.Trim(),
                    SerialNumber = nextSerial,
                    FullName = req.FullName.Trim(),
                    Amount = req.Amount,
                    Description = req.Description.Trim(),
                    SourceType = "Manual",
                    SourceId = null,
                    ReversalOfEntryId = null,
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                await _entryRepo.Insert(entry);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Entry created.",
                    data = new
                    {
                        entry.DailyReportEntryId,
                        entry.EntryDate,
                        entry.EntryType,
                        entry.SerialNumber,
                        entry.FullName,
                        entry.Amount,
                        entry.Description,
                        entry.SourceType,
                        entry.SourceId,
                        entry.ReversalOfEntryId,
                        entry.AddedBy,
                        entry.DateAdded,
                        IsAutoEntry = false
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateEntry failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  POST api/DailyReports/ReverseEntry
        //  Creates a reversal (adjustment) entry instead of deleting
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReverseEntry([FromBody] ReverseDailyReportEntryRequest req)
        {
            try
            {
                if (req.OriginalEntryId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Original entry ID is required." });

                var original = await _context.DailyReportEntries.FindAsync(req.OriginalEntryId);
                if (original == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Original entry not found." });

                // Check if already reversed
                var alreadyReversed = await _context.DailyReportEntries
                    .AnyAsync(e => e.ReversalOfEntryId == req.OriginalEntryId);
                if (alreadyReversed)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "This entry has already been reversed." });

                // Check day status
                var dayStatus = await _context.DailyReportStatuses
                    .FirstOrDefaultAsync(s => s.ReportDate == original.EntryDate);
                if (dayStatus != null && dayStatus.Status == "Closed")
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "This day's report is closed. Cannot reverse entries." });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                int nextSerial = await GetNextSerialNumber(original.EntryDate, original.EntryType);

                string reason = string.IsNullOrWhiteSpace(req.Reason)
                    ? $"Reversal of entry #{original.SerialNumber}"
                    : req.Reason.Trim();

                var reversal = new DailyReportEntry
                {
                    EntryDate = original.EntryDate,
                    EntryType = original.EntryType,
                    SerialNumber = nextSerial,
                    FullName = original.FullName,
                    Amount = -original.Amount,
                    Description = reason,
                    SourceType = "Reversal",
                    SourceId = null,
                    ReversalOfEntryId = original.DailyReportEntryId,
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                await _entryRepo.Insert(reversal);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Reversal entry created.",
                    data = new
                    {
                        reversal.DailyReportEntryId,
                        reversal.EntryDate,
                        reversal.EntryType,
                        reversal.SerialNumber,
                        reversal.FullName,
                        reversal.Amount,
                        reversal.Description,
                        reversal.SourceType,
                        reversal.ReversalOfEntryId,
                        reversal.AddedBy,
                        reversal.DateAdded,
                        IsAutoEntry = false
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReverseEntry failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  POST api/DailyReports/ToggleDayStatus?date=26.01.2026
        //  Toggles day between Open and Closed
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleDayStatus(string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Date is required." });

                string d = date.Trim();
                if (!Regex.IsMatch(d, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(d, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format." });

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                var existing = await _context.DailyReportStatuses
                    .FirstOrDefaultAsync(s => s.ReportDate == d);

                if (existing == null)
                {
                    // No status record yet — create one as Closed
                    var newStatus = new DailyReportStatus
                    {
                        ReportDate = d,
                        Status = "Closed",
                        ClosedBy = currentUserId,
                        ClosedAt = DateTime.UtcNow
                    };
                    await _statusRepo.Insert(newStatus);
                    return Ok(new { status = "success", dayStatus = "Closed", responseMsg = "Day closed." });
                }
                else
                {
                    // Toggle
                    if (existing.Status == "Open")
                    {
                        existing.Status = "Closed";
                        existing.ClosedBy = currentUserId;
                        existing.ClosedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        existing.Status = "Open";
                        existing.ClosedBy = null;
                        existing.ClosedAt = null;
                    }

                    _context.DailyReportStatuses.Update(existing);
                    await _context.SaveChangesAsync();

                    return Ok(new { status = "success", dayStatus = existing.Status, responseMsg = $"Day {existing.Status.ToLower()}." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ToggleDayStatus failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Helper: Get next serial number for date + type
        // ─────────────────────────────────────────────────────────────
        private async Task<int> GetNextSerialNumber(string date, string entryType)
        {
            var maxSerial = await _context.DailyReportEntries
                .Where(e => e.EntryDate == date && e.EntryType == entryType)
                .MaxAsync(e => (int?)e.SerialNumber) ?? 0;
            return maxSerial + 1;
        }

        // ─────────────────────────────────────────────────────────────
        //  PUBLIC STATIC: Create auto-entry (called from other controllers)
        // ─────────────────────────────────────────────────────────────
        public static async Task CreateAutoEntry(
            AppDbContext context,
            string entryDate,
            string entryType,
            string fullName,
            decimal amount,
            string description,
            string sourceType,
            int sourceId,
            int addedBy)
        {
            if (amount <= 0) return; // Don't create entries for zero/negative amounts

            // Get next serial
            var maxSerial = await context.DailyReportEntries
                .Where(e => e.EntryDate == entryDate && e.EntryType == entryType)
                .MaxAsync(e => (int?)e.SerialNumber) ?? 0;

            var entry = new DailyReportEntry
            {
                EntryDate = entryDate,
                EntryType = entryType,
                SerialNumber = maxSerial + 1,
                FullName = fullName,
                Amount = amount,
                Description = description,
                SourceType = sourceType,
                SourceId = sourceId,
                ReversalOfEntryId = null,
                AddedBy = addedBy,
                DateAdded = DateTime.UtcNow
            };

            context.DailyReportEntries.Add(entry);
            await context.SaveChangesAsync();
        }
    }
}
