using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Candidate;
using AdminApi.Models.Helper;
using AdminApi.ViewModels.Candidate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DrivingSessionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<DrivingSession> _sessionRepo;
        private readonly ILogger<DrivingSessionsController> _logger;

        // Allowed 15-minute time slots 08:00 – 15:00
        private static readonly HashSet<string> AllowedTimeSlots = BuildTimeSlots();
        // Allowed status values
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
            { "Kaloi", "Deshtoi", "Anuloi" };

        public DrivingSessionsController(
            AppDbContext context,
            ISqlRepository<DrivingSession> sessionRepo,
            ILogger<DrivingSessionsController> logger)
        {
            _context = context;
            _sessionRepo = sessionRepo;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DrivingSessions/GetSessionsByDate?date=26.01.2026&status=Kaloi
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetSessionsByDate(string? date = null, string? status = null)
        {
            try
            {
                string targetDate = date ?? DateTime.Now.ToString("dd.MM.yyyy");

                var query = _context.DrivingSessions.AsQueryable();

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                if (role == "Instructor")
                    query = query.Where(s => s.InstructorUserId == currentUserId);

                query = query.Where(s => s.DrivingDate == targetDate);

                // Status filter
                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(s => s.Status == status);

                var sessions = await (from s in query
                                      join c in _context.Candidates on s.CandidateId equals c.CandidateId
                                      join v in _context.Vehicles on s.VehicleId equals v.VehicleId
                                      orderby s.DrivingTime
                                      select new
                                      {
                                          s.DrivingSessionId,
                                          s.CandidateId,
                                          CandidateName = c.FirstName + " " + c.LastName,
                                          s.VehicleId,
                                          VehiclePlate = v.PlateNumber,
                                          VehicleBrand = v.Brand,
                                          s.DrivingDate,
                                          s.DrivingTime,
                                          s.PaymentAmount,
                                          s.PaymentDate,
                                          s.Status,
                                          s.Examiner,
                                          s.InstructorUserId,
                                          s.DateAdded
                                      }).ToListAsync();

                return Ok(new { dataCount = sessions.Count, data = sessions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSessionsByDate failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DrivingSessions/GetSessionsByDateRange?from=...&to=...&status=...
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetSessionsByDateRange(string? from = null, string? to = null, string? status = null)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var query = _context.DrivingSessions.AsQueryable();
                if (role == "Instructor")
                    query = query.Where(s => s.InstructorUserId == currentUserId);

                // Status filter at DB level
                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(s => s.Status == status);

                var allSessions = await (from s in query
                                         join c in _context.Candidates on s.CandidateId equals c.CandidateId
                                         join v in _context.Vehicles on s.VehicleId equals v.VehicleId
                                         orderby s.DrivingDate, s.DrivingTime
                                         select new
                                         {
                                             s.DrivingSessionId,
                                             s.CandidateId,
                                             CandidateName = c.FirstName + " " + c.LastName,
                                             s.VehicleId,
                                             VehiclePlate = v.PlateNumber,
                                             VehicleBrand = v.Brand,
                                             s.DrivingDate,
                                             s.DrivingTime,
                                             s.PaymentAmount,
                                             s.PaymentDate,
                                             s.Status,
                                             s.Examiner,
                                             s.InstructorUserId,
                                             s.DateAdded
                                         }).ToListAsync();

                // Date range filtering in memory (dates are dd.MM.yyyy strings)
                DateTime? fromDt = null, toDt = null;
                if (!string.IsNullOrWhiteSpace(from))
                    if (DateTime.TryParseExact(from, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fd))
                        fromDt = fd;
                if (!string.IsNullOrWhiteSpace(to))
                    if (DateTime.TryParseExact(to, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var td))
                        toDt = td;

                var filtered = allSessions;
                if (fromDt.HasValue || toDt.HasValue)
                {
                    filtered = allSessions.Where(s =>
                    {
                        if (!DateTime.TryParseExact(s.DrivingDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var sd))
                            return false;
                        if (fromDt.HasValue && sd < fromDt.Value) return false;
                        if (toDt.HasValue && sd > toDt.Value) return false;
                        return true;
                    }).ToList();
                }

                return Ok(new { dataCount = filtered.Count, data = filtered });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSessionsByDateRange failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DrivingSessions/GetSessionStats?date=26.01.2026
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetSessionStats(string? date = null)
        {
            try
            {
                string targetDate = date ?? DateTime.Now.ToString("dd.MM.yyyy");
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var query = _context.DrivingSessions.Where(s => s.DrivingDate == targetDate);
                if (role == "Instructor")
                    query = query.Where(s => s.InstructorUserId == currentUserId);

                var totalSessions = await query.CountAsync();
                var totalPayments = await query.SumAsync(s => s.PaymentAmount);

                return Ok(new
                {
                    date = targetDate,
                    totalSessions,
                    totalPayments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSessionStats failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/DrivingSessions/GetCandidatesDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetCandidatesDropdown()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var query = _context.Candidates.AsQueryable();
                if (role == "Instructor")
                    query = query.Where(c => c.InstructorId == currentUserId);

                var list = await query
                    .OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
                    .Select(c => new { c.CandidateId, FullName = c.FirstName + " " + c.LastName })
                    .ToListAsync();

                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCandidatesDropdown failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  POST api/DrivingSessions/CreateDrivingSession
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateDrivingSession([FromBody] AddDrivingSessionRequest request)
        {
            try
            {
                if (request.CandidateId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate is required." });
                if (request.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Vehicle is required." });
                if (string.IsNullOrWhiteSpace(request.DrivingDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Driving date is required." });
                if (string.IsNullOrWhiteSpace(request.DrivingTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Driving time is required." });

                if (!Regex.IsMatch(request.DrivingDate.Trim(), @"^\d{2}\.\d{2}\.\d{4}$"))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format. Use dd.MM.yyyy." });
                if (!DateTime.TryParseExact(request.DrivingDate.Trim(), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date." });

                string time = request.DrivingTime.Trim();
                if (!AllowedTimeSlots.Contains(time))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = $"Invalid time slot: {time}. Allowed: 08:00 – 15:00 in 15-min intervals." });

                var candidate = await _context.Candidates.FindAsync(request.CandidateId);
                if (candidate == null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate not found." });

                var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                if (vehicle == null || !vehicle.IsActive)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Vehicle not found or inactive." });

                if (!string.IsNullOrWhiteSpace(request.PaymentDate))
                {
                    if (!Regex.IsMatch(request.PaymentDate.Trim(), @"^\d{2}\.\d{2}\.\d{4}$") ||
                        !DateTime.TryParseExact(request.PaymentDate.Trim(), "dd.MM.yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid payment date format. Use dd.MM.yyyy." });
                }

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var session = new DrivingSession
                {
                    CandidateId = request.CandidateId,
                    VehicleId = request.VehicleId,
                    InstructorUserId = role == "Instructor" ? currentUserId : (int?)null,
                    DrivingDate = request.DrivingDate.Trim(),
                    DrivingTime = request.DrivingTime.Trim(),
                    PaymentAmount = request.PaymentAmount,
                    PaymentDate = string.IsNullOrWhiteSpace(request.PaymentDate) ? null : request.PaymentDate.Trim(),
                    Status = null,
                    Examiner = null,
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                await _sessionRepo.Insert(session);

                // Auto-create income entry in Daily Report if payment > 0
                if (session.PaymentAmount > 0)
                {
                    try
                    {
                        string reportDate = !string.IsNullOrWhiteSpace(session.PaymentDate)
                            ? session.PaymentDate
                            : session.DrivingDate;
                        await DailyReportsController.CreateAutoEntry(
                            _context,
                            reportDate,
                            "Income",
                            candidate.FirstName + " " + candidate.LastName,
                            session.PaymentAmount,
                            $"Driving session payment - {session.DrivingDate} {session.DrivingTime}",
                            "DrivingSession",
                            session.DrivingSessionId,
                            currentUserId
                        );
                    }
                    catch (Exception autoEx)
                    {
                        _logger.LogWarning(autoEx, "Failed to auto-create daily report entry for driving session");
                    }
                }

                var result = new
                {
                    session.DrivingSessionId,
                    session.CandidateId,
                    CandidateName = candidate.FirstName + " " + candidate.LastName,
                    session.VehicleId,
                    VehiclePlate = vehicle.PlateNumber,
                    VehicleBrand = vehicle.Brand,
                    session.DrivingDate,
                    session.DrivingTime,
                    session.PaymentAmount,
                    session.PaymentDate,
                    session.Status,
                    session.Examiner,
                    session.InstructorUserId,
                    session.DateAdded
                };

                return Ok(new { status = "success", responseMsg = "Driving session created.", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateDrivingSession failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  PUT  api/DrivingSessions/UpdateDrivingSession/{id}
        //  Updates Status and Examiner only (post-creation edit)
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDrivingSession(int id, [FromBody] UpdateDrivingSessionRequest request)
        {
            try
            {
                var session = await _context.DrivingSessions.FindAsync(id);
                if (session == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Session not found." });

                // Validate status if provided
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (!AllowedStatuses.Contains(request.Status.Trim()))
                        return BadRequest(new Confirmation { Status = "error", ResponseMsg = $"Invalid status. Allowed: Kaloi, Deshtoi, Anuloi." });
                    session.Status = request.Status.Trim();
                }
                else
                {
                    session.Status = null;
                }

                // Examiner is free text, just trim
                session.Examiner = string.IsNullOrWhiteSpace(request.Examiner) ? null : request.Examiner.Trim();

                _context.DrivingSessions.Update(session);
                await _context.SaveChangesAsync();

                // Return updated data with joins
                var candidate = await _context.Candidates.FindAsync(session.CandidateId);
                var vehicle = await _context.Vehicles.FindAsync(session.VehicleId);

                var result = new
                {
                    session.DrivingSessionId,
                    session.CandidateId,
                    CandidateName = (candidate?.FirstName ?? "") + " " + (candidate?.LastName ?? ""),
                    session.VehicleId,
                    VehiclePlate = vehicle?.PlateNumber ?? "",
                    VehicleBrand = vehicle?.Brand ?? "",
                    session.DrivingDate,
                    session.DrivingTime,
                    session.PaymentAmount,
                    session.PaymentDate,
                    session.Status,
                    session.Examiner,
                    session.InstructorUserId,
                    session.DateAdded
                };

                return Ok(new { status = "success", responseMsg = "Session updated.", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDrivingSession failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  DELETE api/DrivingSessions/DeleteDrivingSession?id=1
        // ─────────────────────────────────────────────────────────────
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDrivingSession(int id)
        {
            try
            {
                var session = await _context.DrivingSessions.FindAsync(id);
                if (session == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Session not found." });

                _context.DrivingSessions.Remove(session);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Session deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteDrivingSession failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ───── Helper: build allowed time slots 08:00 – 15:00 every 15 min ─────
        private static HashSet<string> BuildTimeSlots()
        {
            var slots = new HashSet<string>();
            for (int h = 8; h <= 15; h++)
            {
                for (int m = 0; m < 60; m += 15)
                {
                    if (h == 15 && m > 0) break;
                    slots.Add($"{h:D2}:{m:D2}");
                }
            }
            return slots;
        }
    }
}
