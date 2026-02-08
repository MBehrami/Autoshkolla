using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Models.Schedule;
using AdminApi.ViewModels.Schedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<ScheduleEvent> _scheduleRepo;
        private readonly ILogger<SchedulesController> _logger;

        public SchedulesController(
            AppDbContext context,
            ISqlRepository<ScheduleEvent> scheduleRepo,
            ILogger<SchedulesController> logger)
        {
            _context = context;
            _scheduleRepo = scheduleRepo;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetEvents?dateFrom=26.01.2026&dateTo=01.02.2026
        //       &instructorId=0&vehicleId=0
        //  Returns schedule events + driving sessions merged
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> GetEvents(string? dateFrom = null, string? dateTo = null,
            int instructorId = 0, int vehicleId = 0)
        {
            try
            {
                // Parse date range
                DateTime? fromDt = null, toDt = null;
                if (!string.IsNullOrWhiteSpace(dateFrom))
                    if (DateTime.TryParseExact(dateFrom, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fd))
                        fromDt = fd;
                if (!string.IsNullOrWhiteSpace(dateTo))
                    if (DateTime.TryParseExact(dateTo, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var td))
                        toDt = td;

                // ── Schedule Events ──
                var scheduleEvents = await (from e in _context.ScheduleEvents
                                            join c in _context.Candidates on e.CandidateId equals c.CandidateId
                                            join v in _context.Vehicles on e.VehicleId equals v.VehicleId
                                            join u in _context.Users on e.InstructorUserId equals u.UserId
                                            select new
                                            {
                                                e.ScheduleEventId,
                                                e.EventDate,
                                                e.StartTime,
                                                e.EndTime,
                                                e.InstructorUserId,
                                                InstructorName = u.FullName,
                                                e.CandidateId,
                                                CandidateName = c.FirstName + " " + c.LastName,
                                                e.VehicleId,
                                                VehiclePlate = v.PlateNumber,
                                                VehicleBrand = v.Brand,
                                                e.Notes,
                                                e.AddedBy,
                                                e.DateAdded,
                                                EventType = "schedule"
                                            }).ToListAsync();

                // Apply filters in memory (dates are dd.MM.yyyy strings)
                var filtered = scheduleEvents.AsEnumerable();
                if (instructorId > 0)
                    filtered = filtered.Where(e => e.InstructorUserId == instructorId);
                if (vehicleId > 0)
                    filtered = filtered.Where(e => e.VehicleId == vehicleId);
                if (fromDt.HasValue || toDt.HasValue)
                {
                    filtered = filtered.Where(e =>
                    {
                        if (!DateTime.TryParseExact(e.EventDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                            return false;
                        if (fromDt.HasValue && d < fromDt.Value) return false;
                        if (toDt.HasValue && d > toDt.Value) return false;
                        return true;
                    });
                }

                // ── Driving Sessions (read-only events for the calendar) ──
                var drivingSessions = await (from s in _context.DrivingSessions
                                             join c in _context.Candidates on s.CandidateId equals c.CandidateId
                                             join v in _context.Vehicles on s.VehicleId equals v.VehicleId
                                             select new
                                             {
                                                 s.DrivingSessionId,
                                                 s.DrivingDate,
                                                 s.DrivingTime,
                                                 s.InstructorUserId,
                                                 s.CandidateId,
                                                 CandidateName = c.FirstName + " " + c.LastName,
                                                 s.VehicleId,
                                                 VehiclePlate = v.PlateNumber,
                                                 VehicleBrand = v.Brand,
                                                 s.Status,
                                                 s.PaymentAmount
                                             }).ToListAsync();

                var dsFiltered = drivingSessions.AsEnumerable();
                if (instructorId > 0)
                    dsFiltered = dsFiltered.Where(e => e.InstructorUserId == instructorId);
                if (vehicleId > 0)
                    dsFiltered = dsFiltered.Where(e => e.VehicleId == vehicleId);
                if (fromDt.HasValue || toDt.HasValue)
                {
                    dsFiltered = dsFiltered.Where(e =>
                    {
                        if (!DateTime.TryParseExact(e.DrivingDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                            return false;
                        if (fromDt.HasValue && d < fromDt.Value) return false;
                        if (toDt.HasValue && d > toDt.Value) return false;
                        return true;
                    });
                }

                // Determine current user role for data visibility
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";
                bool isInstructor = role == "Instructor";

                // Map driving sessions to a common event shape
                // Instructor: see only vehicle info (plate + brand) – no candidate details
                // Admin: see full details
                var dsEvents = dsFiltered.Select(s =>
                {
                    // Calculate end time = start + 45 min
                    string endTime = s.DrivingTime;
                    if (TimeSpan.TryParse(s.DrivingTime, out var ts))
                        endTime = ts.Add(TimeSpan.FromMinutes(45)).ToString(@"hh\:mm");

                    return new
                    {
                        id = s.DrivingSessionId,
                        eventDate = s.DrivingDate,
                        startTime = s.DrivingTime,
                        endTime,
                        instructorUserId = s.InstructorUserId ?? 0,
                        instructorName = (string?)null,
                        candidateId = isInstructor ? 0 : s.CandidateId,
                        candidateName = isInstructor ? "Booked / Exam Slot" : s.CandidateName,
                        vehicleId = s.VehicleId,
                        vehiclePlate = s.VehiclePlate,
                        vehicleBrand = s.VehicleBrand,
                        notes = isInstructor ? "" : (s.Status ?? ""),
                        eventType = "driving-session"
                    };
                }).ToList();

                // Map schedule events to same shape
                var schEvents = filtered.Select(e => new
                {
                    id = e.ScheduleEventId,
                    eventDate = e.EventDate,
                    startTime = e.StartTime,
                    endTime = e.EndTime,
                    instructorUserId = e.InstructorUserId,
                    instructorName = (string?)e.InstructorName,
                    candidateId = e.CandidateId,
                    candidateName = e.CandidateName,
                    vehicleId = e.VehicleId,
                    vehiclePlate = e.VehiclePlate,
                    vehicleBrand = e.VehicleBrand,
                    notes = e.Notes ?? "",
                    eventType = e.EventType
                }).ToList();

                // Combine and return
                var allEvents = schEvents.Concat(dsEvents)
                    .OrderBy(e => e.eventDate).ThenBy(e => e.startTime)
                    .ToList();

                return Ok(new { dataCount = allEvents.Count, data = allEvents });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEvents failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetInstructorsDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> GetInstructorsDropdown()
        {
            try
            {
                var list = await (from u in _context.Users
                                 join r in _context.UserRole on u.UserRoleId equals r.UserRoleId
                                 where u.IsActive == true && r.RoleName == "Instructor"
                                 select new { u.UserId, u.FullName })
                                 .OrderBy(u => u.FullName)
                                 .ToListAsync();
                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetCandidatesDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
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
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetVehiclesDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> GetVehiclesDropdown()
        {
            var list = await _context.Vehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.PlateNumber)
                .Select(v => new { v.VehicleId, v.PlateNumber, v.Brand })
                .ToListAsync();
            return Ok(new { data = list });
        }

        // ─────────────────────────────────────────────────────────────
        //  POST api/Schedules/CreateEvent
        // ─────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> CreateEvent([FromBody] AddScheduleEventRequest req)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                // ── Validation ──
                if (string.IsNullOrWhiteSpace(req.EventDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Date is required." });
                if (string.IsNullOrWhiteSpace(req.StartTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Start time is required." });
                if (string.IsNullOrWhiteSpace(req.EndTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "End time is required." });
                if (req.CandidateId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate is required." });
                if (req.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Vehicle is required." });

                // Date format
                string date = req.EventDate.Trim();
                if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format (dd.MM.yyyy)." });

                // Time format
                string startT = req.StartTime.Trim();
                string endT = req.EndTime.Trim();
                if (!TimeSpan.TryParse(startT, out var startTs) || !TimeSpan.TryParse(endT, out var endTs))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid time format (HH:mm)." });
                if (endTs <= startTs)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "End time must be after start time." });

                // Instructor: auto-assign self
                int instructorId = role == "Instructor" ? currentUserId : req.InstructorUserId;
                if (instructorId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Instructor is required." });

                // Candidate exists
                var candidate = await _context.Candidates.FindAsync(req.CandidateId);
                if (candidate == null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate not found." });

                // Vehicle exists and active
                var vehicle = await _context.Vehicles.FindAsync(req.VehicleId);
                if (vehicle == null || !vehicle.IsActive)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Vehicle not found or inactive." });

                // ── Conflict check ──
                var conflictMsg = await CheckConflicts(date, startT, endT, instructorId, req.VehicleId, excludeId: null);
                if (conflictMsg != null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = conflictMsg });

                var ev = new ScheduleEvent
                {
                    EventDate = date,
                    StartTime = startT,
                    EndTime = endT,
                    InstructorUserId = instructorId,
                    CandidateId = req.CandidateId,
                    VehicleId = req.VehicleId,
                    Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                await _scheduleRepo.Insert(ev);

                // Fetch instructor name for response
                var instructor = await _context.Users.FindAsync(instructorId);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Schedule event created.",
                    data = new
                    {
                        id = ev.ScheduleEventId,
                        eventDate = ev.EventDate,
                        startTime = ev.StartTime,
                        endTime = ev.EndTime,
                        instructorUserId = ev.InstructorUserId,
                        instructorName = instructor?.FullName ?? "",
                        candidateId = ev.CandidateId,
                        candidateName = candidate.FirstName + " " + candidate.LastName,
                        vehicleId = ev.VehicleId,
                        vehiclePlate = vehicle.PlateNumber,
                        vehicleBrand = vehicle.Brand,
                        notes = ev.Notes ?? "",
                        eventType = "schedule"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateEvent failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  PUT api/Schedules/UpdateEvent/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> UpdateEvent(int id, [FromBody] UpdateScheduleEventRequest req)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var ev = await _context.ScheduleEvents.FindAsync(id);
                if (ev == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Event not found." });

                // Instructor can only edit their own events
                if (role == "Instructor" && ev.AddedBy != currentUserId)
                    return StatusCode(403, new Confirmation { Status = "error", ResponseMsg = "You can only edit events you created." });

                // Validation
                if (string.IsNullOrWhiteSpace(req.EventDate) || string.IsNullOrWhiteSpace(req.StartTime) || string.IsNullOrWhiteSpace(req.EndTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Date and times are required." });

                string date = req.EventDate.Trim();
                string startT = req.StartTime.Trim();
                string endT = req.EndTime.Trim();

                if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format." });
                if (!TimeSpan.TryParse(startT, out var startTs) || !TimeSpan.TryParse(endT, out var endTs))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid time format." });
                if (endTs <= startTs)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "End time must be after start time." });

                int instructorId = role == "Instructor" ? currentUserId : req.InstructorUserId;
                if (instructorId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Instructor is required." });

                if (req.CandidateId <= 0 || req.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate and Vehicle are required." });

                // Conflict check (exclude self)
                var conflictMsg = await CheckConflicts(date, startT, endT, instructorId, req.VehicleId, excludeId: id);
                if (conflictMsg != null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = conflictMsg });

                ev.EventDate = date;
                ev.StartTime = startT;
                ev.EndTime = endT;
                ev.InstructorUserId = instructorId;
                ev.CandidateId = req.CandidateId;
                ev.VehicleId = req.VehicleId;
                ev.Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();

                _context.ScheduleEvents.Update(ev);
                await _context.SaveChangesAsync();

                var candidate = await _context.Candidates.FindAsync(ev.CandidateId);
                var vehicle = await _context.Vehicles.FindAsync(ev.VehicleId);
                var instructor = await _context.Users.FindAsync(ev.InstructorUserId);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Event updated.",
                    data = new
                    {
                        id = ev.ScheduleEventId,
                        eventDate = ev.EventDate,
                        startTime = ev.StartTime,
                        endTime = ev.EndTime,
                        instructorUserId = ev.InstructorUserId,
                        instructorName = instructor?.FullName ?? "",
                        candidateId = ev.CandidateId,
                        candidateName = (candidate?.FirstName ?? "") + " " + (candidate?.LastName ?? ""),
                        vehicleId = ev.VehicleId,
                        vehiclePlate = vehicle?.PlateNumber ?? "",
                        vehicleBrand = vehicle?.Brand ?? "",
                        notes = ev.Notes ?? "",
                        eventType = "schedule"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateEvent failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  DELETE api/Schedules/DeleteEvent?id=1
        // ─────────────────────────────────────────────────────────────
        [HttpDelete]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var ev = await _context.ScheduleEvents.FindAsync(id);
                if (ev == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Event not found." });

                if (role == "Instructor" && ev.AddedBy != currentUserId)
                    return StatusCode(403, new Confirmation { Status = "error", ResponseMsg = "You can only delete events you created." });

                _context.ScheduleEvents.Remove(ev);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Event deleted." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteEvent failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        // ───── Conflict checker ─────
        private async Task<string?> CheckConflicts(string date, string startTime, string endTime,
            int instructorId, int vehicleId, int? excludeId)
        {
            if (!TimeSpan.TryParse(startTime, out var newStart) || !TimeSpan.TryParse(endTime, out var newEnd))
                return null;

            // Get all schedule events on the same date
            var existing = await _context.ScheduleEvents
                .Where(e => e.EventDate == date && (excludeId == null || e.ScheduleEventId != excludeId.Value))
                .ToListAsync();

            foreach (var e in existing)
            {
                if (!TimeSpan.TryParse(e.StartTime, out var eStart) || !TimeSpan.TryParse(e.EndTime, out var eEnd))
                    continue;

                bool overlaps = newStart < eEnd && newEnd > eStart;
                if (!overlaps) continue;

                if (e.InstructorUserId == instructorId)
                    return $"Instructor conflict: already booked {e.StartTime}–{e.EndTime} on {date}.";
                if (e.VehicleId == vehicleId)
                    return $"Vehicle conflict: already booked {e.StartTime}–{e.EndTime} on {date}.";
            }

            // Also check driving sessions for vehicle/instructor overlap
            var drivingSessions = await _context.DrivingSessions
                .Where(s => s.DrivingDate == date)
                .ToListAsync();

            foreach (var s in drivingSessions)
            {
                if (!TimeSpan.TryParse(s.DrivingTime, out var dsStart))
                    continue;
                var dsEnd = dsStart.Add(TimeSpan.FromMinutes(45));

                bool overlaps = newStart < dsEnd && newEnd > dsStart;
                if (!overlaps) continue;

                if (s.InstructorUserId.HasValue && s.InstructorUserId.Value == instructorId)
                    return $"Instructor conflict with driving session at {s.DrivingTime} on {date}.";
                if (s.VehicleId == vehicleId)
                    return $"Vehicle conflict with driving session at {s.DrivingTime} on {date}.";
            }

            return null; // No conflicts
        }
    }
}
