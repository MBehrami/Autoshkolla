using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Candidate;
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
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
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
                                            join c in _context.Candidates on e.CandidateId equals c.CandidateId into cg
                                            from c in cg.DefaultIfEmpty()
                                            join al in _context.AdditionalLessons on e.AdditionalLessonId equals al.AdditionalLessonId into alg
                                            from al in alg.DefaultIfEmpty()
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
                                                CandidateId = e.CandidateId ?? 0,
                                                CandidateName = c != null
                                                    ? c.FirstName + " " + c.LastName
                                                    : (al != null ? al.FirstName + " " + al.LastName : ""),
                                                e.VehicleId,
                                                VehiclePlate = v.PlateNumber,
                                                VehicleBrand = v.Brand,
                                                e.Notes,
                                                e.Status,
                                                e.AddedBy,
                                                e.DateAdded,
                                                EventType = "schedule"
                                            }).ToListAsync();

                // Determine current user for instructor-level access control
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";
                bool isInstructor = role == "Instructor";

                // For instructors: force filter to own events only (security)
                int effectiveInstructorId = isInstructor ? currentUserId : instructorId;

                // Apply filters in memory (dates are dd.MM.yyyy strings)
                var filtered = scheduleEvents.AsEnumerable();
                if (effectiveInstructorId > 0)
                    filtered = filtered.Where(e => e.InstructorUserId == effectiveInstructorId);
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
                                             join c in _context.Candidates on s.CandidateId equals c.CandidateId into cg
                                             from c in cg.DefaultIfEmpty()
                                             join v in _context.Vehicles on s.VehicleId equals v.VehicleId
                                             select new
                                             {
                                                 s.DrivingSessionId,
                                                 s.DrivingDate,
                                                 s.DrivingTime,
                                                 s.InstructorUserId,
                                                 s.CandidateId,
                                                 CandidateName = c != null ? (c.FirstName + " " + c.LastName) : s.ManualCandidateName,
                                                 s.VehicleId,
                                                 VehiclePlate = v.PlateNumber,
                                                 VehicleBrand = v.Brand,
                                                 s.Status,
                                                 s.PaymentAmount
                                             }).ToListAsync();

                var dsFiltered = drivingSessions.Where(e => !string.IsNullOrEmpty(e.DrivingDate) && !string.IsNullOrEmpty(e.DrivingTime)).AsEnumerable();
                if (effectiveInstructorId > 0)
                    dsFiltered = dsFiltered.Where(e => e.InstructorUserId == effectiveInstructorId);
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
                        candidateId = isInstructor ? 0 : (s.CandidateId ?? 0),
                        candidateName = isInstructor ? "Booked / Exam Slot" : s.CandidateName,
                        vehicleId = s.VehicleId,
                        vehiclePlate = s.VehiclePlate,
                        vehicleBrand = s.VehicleBrand,
                        notes = isInstructor ? "" : (s.Status ?? ""),
                        status = (string?)null,
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
                    status = e.Status,
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
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetInstructorsDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        public async Task<ActionResult> GetInstructorsDropdown()
        {
            try
            {
                var role = User.FindFirst("role")?.Value ?? "";
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");

                if (role == "Instructor")
                {
                    var self = await (from u in _context.Users
                                     join ip in _context.InstructorProfiles on u.UserId equals ip.UserId into ipg
                                     from ip in ipg.DefaultIfEmpty()
                                     where u.UserId == currentUserId
                                     select new
                                     {
                                         u.UserId,
                                         FullName = u.FullName
                                             + (ip != null && ip.PersonalNumber != null && ip.PersonalNumber != ""
                                                 ? " (" + ip.PersonalNumber + ")" : ""),
                                         PersonalNumber = ip != null ? ip.PersonalNumber : null
                                     }).FirstOrDefaultAsync();
                    return Ok(new { data = self != null ? new[] { self } : Array.Empty<object>() });
                }

                var list = await (from u in _context.Users
                                 join r in _context.UserRole on u.UserRoleId equals r.UserRoleId
                                 join ip in _context.InstructorProfiles on u.UserId equals ip.UserId into ipGroup
                                 from ip in ipGroup.DefaultIfEmpty()
                                 where u.IsActive == true && r.RoleName == "Instructor"
                                 select new
                                 {
                                     u.UserId,
                                     FullName = u.FullName
                                         + (ip != null && ip.PersonalNumber != null && ip.PersonalNumber != ""
                                             ? " (" + ip.PersonalNumber + ")" : ""),
                                     PersonalNumber = ip != null ? ip.PersonalNumber : null
                                 })
                                 .OrderBy(u => u.FullName)
                                 .ToListAsync();
                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetCandidatesDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        public async Task<ActionResult> GetCandidatesDropdown()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var query = _context.Candidates.AsQueryable();
                if (role == "Instructor")
                    query = query.Where(c => c.InstructorId == currentUserId);

                var candidates = await query
                    .OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
                    .Select(c => new
                    {
                        c.CandidateId,
                        AdditionalLessonId = (int?)null,
                        FullName = c.FirstName + " " + c.LastName
                            + (c.PersonalNumber != null && c.PersonalNumber != "" ? " (" + c.PersonalNumber + ")" : ""),
                        c.PersonalNumber,
                        IsAdditionalLesson = false
                    })
                    .ToListAsync();

                // Also include Additional Lesson candidates
                var alQuery = _context.AdditionalLessons.AsQueryable();
                if (role == "Instructor")
                    alQuery = alQuery.Where(al => al.InstructorId == currentUserId);

                var additionalCandidates = await alQuery
                    .OrderBy(al => al.FirstName).ThenBy(al => al.LastName)
                    .Select(al => new
                    {
                        CandidateId = 0,
                        AdditionalLessonId = (int?)al.AdditionalLessonId,
                        FullName = al.FirstName + " " + al.LastName
                            + (al.PersonalNumber != null && al.PersonalNumber != "" ? " (" + al.PersonalNumber + ")" : "")
                            + " [Orë Shtesë]",
                        al.PersonalNumber,
                        IsAdditionalLesson = true
                    })
                    .ToListAsync();

                var combined = candidates.Concat(additionalCandidates).ToList();
                return Ok(new { data = combined });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  GET  api/Schedules/GetVehiclesDropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
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
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        public async Task<ActionResult> CreateEvent([FromBody] AddScheduleEventRequest req)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                bool isAdditionalLesson = req.AdditionalLessonId.HasValue && req.AdditionalLessonId.Value > 0;

                // ── Validation ──
                if (string.IsNullOrWhiteSpace(req.EventDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Data eshte e detyrueshme." });
                if (string.IsNullOrWhiteSpace(req.StartTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Ora e fillimit eshte e detyrueshme." });
                if (!isAdditionalLesson && req.CandidateId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Kandidati eshte i detyrueshem." });
                if (req.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Automjeti eshte i detyrueshem." });

                // Date format
                string date = req.EventDate.Trim();
                if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Formati i dates eshte i pavlefshem (dd.MM.yyyy)." });

                // Time format – auto-calculate end time (start + 45 min)
                string startT = req.StartTime.Trim();
                if (!TimeSpan.TryParse(startT, out var startTs))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Formati i ores eshte i pavlefshem (HH:mm)." });
                var endTs = startTs.Add(TimeSpan.FromMinutes(45));
                string endT = $"{endTs.Hours:D2}:{endTs.Minutes:D2}";

                // Instructor: auto-assign self
                int instructorId = role == "Instructor" ? currentUserId : req.InstructorUserId;
                if (instructorId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Instruktori eshte i detyrueshem." });

                // Validate candidate / additional lesson exists
                string candidateName = "";
                if (isAdditionalLesson)
                {
                    var al = await _context.AdditionalLessons.FindAsync(req.AdditionalLessonId!.Value);
                    if (al == null)
                        return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Kandidati (orë shtesë) nuk u gjet." });
                    candidateName = al.FirstName + " " + al.LastName;
                }
                else
                {
                    var candidate = await _context.Candidates.FindAsync(req.CandidateId);
                    if (candidate == null)
                        return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Kandidati nuk u gjet." });
                    candidateName = candidate.FirstName + " " + candidate.LastName;
                }

                // Vehicle exists and active
                var vehicle = await _context.Vehicles.FindAsync(req.VehicleId);
                if (vehicle == null || !vehicle.IsActive)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Automjeti nuk u gjet ose eshte joaktiv." });

                // ── Duplicate check: same candidate, same date, same start time (skip cancelled) ──
                var duplicateExists = await _context.ScheduleEvents.AnyAsync(e =>
                    e.EventDate == date &&
                    e.StartTime == startT &&
                    e.Status != "Cancelled" &&
                    (isAdditionalLesson
                        ? (e.AdditionalLessonId == req.AdditionalLessonId)
                        : (e.CandidateId == req.CandidateId && e.CandidateId != null)));
                if (duplicateExists)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Ky kandidat tashme ka nje orar te regjistruar ne kete date dhe ore." });

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
                    CandidateId = isAdditionalLesson ? null : req.CandidateId,
                    AdditionalLessonId = isAdditionalLesson ? req.AdditionalLessonId : null,
                    VehicleId = req.VehicleId,
                    Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
                    AddedBy = currentUserId,
                    DateAdded = DateTime.UtcNow
                };

                await _scheduleRepo.Insert(ev);

                // Also create a PracticalLesson (if an active one doesn't already exist) so hours are tracked
                var lessonExists = await _context.PracticalLessons.AnyAsync(l =>
                    l.LessonDate == date && l.Time == startT &&
                    l.Status != "Cancelled" &&
                    (isAdditionalLesson
                        ? l.AdditionalLessonId == req.AdditionalLessonId
                        : l.CandidateId == req.CandidateId));
                if (!lessonExists)
                {
                    var vehiclePlateStr = vehicle.PlateNumber + (string.IsNullOrEmpty(vehicle.Brand) ? "" : " – " + vehicle.Brand);
                    var lesson = new PracticalLesson
                    {
                        CandidateId = isAdditionalLesson ? null : req.CandidateId,
                        AdditionalLessonId = isAdditionalLesson ? req.AdditionalLessonId : null,
                        InstructorUserId = instructorId,
                        LessonDate = date,
                        Time = startT,
                        EndTime = endT,
                        Vehicle = vehiclePlateStr,
                        DateAdded = DateTime.UtcNow
                    };
                    _context.PracticalLessons.Add(lesson);
                    await _context.SaveChangesAsync();
                }

                // Fetch instructor name for response
                var instructor = await _context.Users.FindAsync(instructorId);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Ngjarja e orarit u krijua.",
                    data = new
                    {
                        id = ev.ScheduleEventId,
                        eventDate = ev.EventDate,
                        startTime = ev.StartTime,
                        endTime = ev.EndTime,
                        instructorUserId = ev.InstructorUserId,
                        instructorName = instructor?.FullName ?? "",
                        candidateId = ev.CandidateId ?? 0,
                        candidateName = candidateName,
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
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  PUT api/Schedules/UpdateEvent/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        public async Task<ActionResult> UpdateEvent(int id, [FromBody] UpdateScheduleEventRequest req)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                bool isAdditionalLesson = req.AdditionalLessonId.HasValue && req.AdditionalLessonId.Value > 0;

                var ev = await _context.ScheduleEvents.FindAsync(id);
                if (ev == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Ngjarja nuk u gjet." });

                if (role == "Instructor" && ev.AddedBy != currentUserId)
                    return StatusCode(403, new Confirmation { Status = "error", ResponseMsg = "Mund te ndryshoni vetem ngjarjet qe i keni krijuar ju." });

                if (string.IsNullOrWhiteSpace(req.EventDate) || string.IsNullOrWhiteSpace(req.StartTime))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Data dhe ora e fillimit jane te detyrueshme." });

                string date = req.EventDate.Trim();
                string startT = req.StartTime.Trim();

                if (!Regex.IsMatch(date, @"^\d{2}\.\d{2}\.\d{4}$") ||
                    !DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Formati i dates eshte i pavlefshem." });
                if (!TimeSpan.TryParse(startT, out var startTs))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Formati i ores eshte i pavlefshem." });

                var endTs = startTs.Add(TimeSpan.FromMinutes(45));
                string endT = $"{endTs.Hours:D2}:{endTs.Minutes:D2}";

                int instructorId = role == "Instructor" ? currentUserId : req.InstructorUserId;
                if (instructorId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Instruktori eshte i detyrueshem." });

                if (!isAdditionalLesson && req.CandidateId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Kandidati eshte i detyrueshem." });
                if (req.VehicleId <= 0)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Automjeti eshte i detyrueshem." });

                // Conflict check (exclude self)
                var conflictMsg = await CheckConflicts(date, startT, endT, instructorId, req.VehicleId, excludeId: id);
                if (conflictMsg != null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = conflictMsg });

                // Resolve candidate name
                string candidateName = "";
                if (isAdditionalLesson)
                {
                    var al = await _context.AdditionalLessons.FindAsync(req.AdditionalLessonId!.Value);
                    if (al != null) candidateName = al.FirstName + " " + al.LastName;
                }
                else
                {
                    var cand = await _context.Candidates.FindAsync(req.CandidateId);
                    if (cand != null) candidateName = cand.FirstName + " " + cand.LastName;
                }

                ev.EventDate = date;
                ev.StartTime = startT;
                ev.EndTime = endT;
                ev.InstructorUserId = instructorId;
                ev.CandidateId = isAdditionalLesson ? null : req.CandidateId;
                ev.AdditionalLessonId = isAdditionalLesson ? req.AdditionalLessonId : null;
                ev.VehicleId = req.VehicleId;
                ev.Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim();

                _context.ScheduleEvents.Update(ev);
                await _context.SaveChangesAsync();

                var vehicle = await _context.Vehicles.FindAsync(ev.VehicleId);
                var instructor = await _context.Users.FindAsync(ev.InstructorUserId);

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Ngjarja u perditesua.",
                    data = new
                    {
                        id = ev.ScheduleEventId,
                        eventDate = ev.EventDate,
                        startTime = ev.StartTime,
                        endTime = ev.EndTime,
                        instructorUserId = ev.InstructorUserId,
                        instructorName = instructor?.FullName ?? "",
                        candidateId = ev.CandidateId ?? 0,
                        candidateName = candidateName,
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
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  DELETE api/Schedules/DeleteEvent?id=1
        // ─────────────────────────────────────────────────────────────
        [HttpDelete]
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var ev = await _context.ScheduleEvents.FindAsync(id);
                if (ev == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Ngjarja nuk u gjet." });

                if (role == "Instructor" && ev.AddedBy != currentUserId)
                    return StatusCode(403, new Confirmation { Status = "error", ResponseMsg = "Mund te fshini vetem ngjarjet qe i keni krijuar ju." });

                _context.ScheduleEvents.Remove(ev);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Ngjarja u fshi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteEvent failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        // ───── Conflict checker ─────
        private async Task<string?> CheckConflicts(string date, string startTime, string endTime,
            int instructorId, int vehicleId, int? excludeId)
        {
            if (!TimeSpan.TryParse(startTime, out var newStart) || !TimeSpan.TryParse(endTime, out var newEnd))
                return null;

            // Get all active schedule events on the same date (skip cancelled)
            var existing = await _context.ScheduleEvents
                .Where(e => e.EventDate == date
                    && (excludeId == null || e.ScheduleEventId != excludeId.Value)
                    && e.Status != "Cancelled")
                .ToListAsync();

            foreach (var e in existing)
            {
                if (!TimeSpan.TryParse(e.StartTime, out var eStart) || !TimeSpan.TryParse(e.EndTime, out var eEnd))
                    continue;

                bool overlaps = newStart < eEnd && newEnd > eStart;
                if (!overlaps) continue;

                if (e.InstructorUserId == instructorId)
                    return $"Konflikt instruktori: i rezervuar tashme {e.StartTime}–{e.EndTime} me date {date}.";
                if (e.VehicleId == vehicleId)
                    return $"Konflikt automjeti: i rezervuar tashme {e.StartTime}–{e.EndTime} me date {date}.";
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
                    return $"Konflikt instruktori me seancen e vozitjes ne {s.DrivingTime} me date {date}.";
                if (s.VehicleId == vehicleId)
                    return $"Konflikt automjeti me seancen e vozitjes ne {s.DrivingTime} me date {date}.";
            }

            return null; // No conflicts
        }
    }
}


