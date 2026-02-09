using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Candidate;
using AdminApi.Models.Helper;
using AdminApi.Models.User;
using AdminApi.ViewModels.Candidate;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CandidatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<Candidate> _candidateRepo;
        private readonly ISqlRepository<Category> _categoryRepo;
        private readonly ISqlRepository<CandidateInstallment> _installmentRepo;
        private readonly ILogger<CandidatesController> _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public CandidatesController(
            AppDbContext context,
            ISqlRepository<Candidate> candidateRepo,
            ISqlRepository<Category> categoryRepo,
            ISqlRepository<CandidateInstallment> installmentRepo,
            ILogger<CandidatesController> logger,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _context = context;
            _candidateRepo = candidateRepo;
            _categoryRepo = categoryRepo;
            _installmentRepo = installmentRepo;
            _logger = logger;
            _config = config;
            _env = env;
        }

        ///<summary>
        ///Get Categories List. If table is empty, inserts default categories (A1, A2, A, B, B+E, C1, C) and returns them. Instructor needs this for Candidates list filters.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                var list = await _context.Categories
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new { c.CategoryId, c.CategoryName })
                    .ToListAsync();

                if (list == null || list.Count == 0)
                {
                    try
                    {
                        var defaults = new[]
                        {
                            new Category { CategoryId = 1, CategoryName = "A1", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 2, CategoryName = "A2", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 3, CategoryName = "A", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 4, CategoryName = "B", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 5, CategoryName = "B+E", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 6, CategoryName = "C1", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false },
                            new Category { CategoryId = 7, CategoryName = "C", IsActive = true, AddedBy = 1, DateAdded = DateTime.Now, IsMigrationData = false }
                        };
                        foreach (var cat in defaults)
                        {
                            if (!await _context.Categories.AnyAsync(c => c.CategoryId == cat.CategoryId))
                            {
                                _context.Categories.Add(cat);
                            }
                        }
                        await _context.SaveChangesAsync();
                        list = await _context.Categories
                            .Where(c => c.IsActive == true)
                            .OrderBy(c => c.CategoryName)
                            .Select(c => new { c.CategoryId, c.CategoryName })
                            .ToListAsync();
                    }
                    catch
                    {
                        var fallback = new[]
                        {
                            new { CategoryId = 1, CategoryName = "A1" },
                            new { CategoryId = 2, CategoryName = "A2" },
                            new { CategoryId = 3, CategoryName = "A" },
                            new { CategoryId = 4, CategoryName = "B" },
                            new { CategoryId = 5, CategoryName = "B+E" },
                            new { CategoryId = 6, CategoryName = "C1" },
                            new { CategoryId = 7, CategoryName = "C" }
                        };
                        return Ok(new { data = fallback });
                    }
                }

                _logger.LogInformation("GetCategories: returning {Count} categories from database.", list.Count);
                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GetCategories: database error, returning fallback categories.");
                var fallback = new[]
                {
                    new { CategoryId = 1, CategoryName = "A1" },
                    new { CategoryId = 2, CategoryName = "A2" },
                    new { CategoryId = 3, CategoryName = "A" },
                    new { CategoryId = 4, CategoryName = "B" },
                    new { CategoryId = 5, CategoryName = "B+E" },
                    new { CategoryId = 6, CategoryName = "C1" },
                    new { CategoryId = 7, CategoryName = "C" }
                };
                return Ok(new { data = fallback });
            }
        }

        ///<summary>
        ///Get Active Instructors List
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetActiveInstructors()
        {
            try
            {
                // Assuming instructors are Users with a specific role (e.g., RoleName = "Instructor")
                // If you have a different way to identify instructors, adjust this query
                var list = await (from u in _context.Users
                                 join r in _context.UserRole on u.UserRoleId equals r.UserRoleId
                                 where u.IsActive == true && r.RoleName == "Instructor"
                                 select new { u.UserId, FullName = u.FullName })
                                 .ToListAsync();

                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create Candidate
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateCandidate(CandidateCreateRequest request)
        {
            try
            {
                // Validate installments
                int totalInstallments = request.Installments.Sum(i => i.Amount);
                if (totalInstallments > request.TotalServiceAmount)
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Sum of installments must not exceed Total service amount." });
                }

                // Create candidate
                var candidate = new Candidate
                {
                    SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim(),
                    FirstName = request.FirstName,
                    ParentName = request.ParentName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    PersonalNumber = request.PersonalNumber,
                    PhoneNumber = request.PhoneNumber,
                    PlaceOfBirth = request.PlaceOfBirth,
                    Address = request.Address,
                    CategoryId = request.CategoryId,
                    InstructorId = request.InstructorId,
                    VehicleType = request.VehicleType,
                    PaymentMethod = request.PaymentMethod,
                    PracticalHours = request.PracticalHours,
                    TotalServiceAmount = request.TotalServiceAmount,
                    DocWithdrawalAmount = request.DocWithdrawalAmount,
                    DocWithdrawalDate = string.IsNullOrWhiteSpace(request.DocWithdrawalDate) ? null : request.DocWithdrawalDate.Trim(),
                    DrivingPaymentAmount = request.DrivingPaymentAmount,
                    DrivingPaymentDate = string.IsNullOrWhiteSpace(request.DrivingPaymentDate) ? null : request.DrivingPaymentDate.Trim(),
                    AddedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1"),
                    DateAdded = DateTime.Now
                };

                await _candidateRepo.Insert(candidate);

                string candidateFullName = (candidate.FirstName ?? "") + " " + (candidate.LastName ?? "");

                // Auto daily report: Document Withdrawal Payment
                if (candidate.DocWithdrawalAmount.HasValue && candidate.DocWithdrawalAmount.Value > 0)
                {
                    try
                    {
                        string reportDate = !string.IsNullOrWhiteSpace(candidate.DocWithdrawalDate)
                            ? candidate.DocWithdrawalDate : DateTime.Now.ToString("dd.MM.yyyy");
                        await DailyReportsController.CreateAutoEntry(
                            _context, reportDate, "Income", candidateFullName,
                            candidate.DocWithdrawalAmount.Value,
                            "Terheqja e Dokumentave (Document Withdrawal)",
                            "CandidateDocWithdrawal", candidate.CandidateId, candidate.AddedBy);
                    }
                    catch (Exception autoEx) { _logger.LogWarning(autoEx, "Auto daily report (DocWithdrawal) failed"); }
                }

                // Auto daily report: Driving Payment
                if (candidate.DrivingPaymentAmount.HasValue && candidate.DrivingPaymentAmount.Value > 0)
                {
                    try
                    {
                        string reportDate = !string.IsNullOrWhiteSpace(candidate.DrivingPaymentDate)
                            ? candidate.DrivingPaymentDate : DateTime.Now.ToString("dd.MM.yyyy");
                        await DailyReportsController.CreateAutoEntry(
                            _context, reportDate, "Income", candidateFullName,
                            candidate.DrivingPaymentAmount.Value,
                            "Pagesa e Vozitjes (Driving Payment)",
                            "CandidateDrivingPayment", candidate.CandidateId, candidate.AddedBy);
                    }
                    catch (Exception autoEx) { _logger.LogWarning(autoEx, "Auto daily report (DrivingPayment) failed"); }
                }

                // Create installments + auto daily report entries
                foreach (var installment in request.Installments)
                {
                    if (installment.Amount > 0)
                    {
                        var candidateInstallment = new CandidateInstallment
                        {
                            CandidateId = candidate.CandidateId,
                            InstallmentNumber = installment.InstallmentNumber,
                            Amount = installment.Amount,
                            InstallmentDate = installment.InstallmentDate,
                            AddedBy = candidate.AddedBy,
                            DateAdded = DateTime.Now
                        };
                        await _installmentRepo.Insert(candidateInstallment);

                        // Auto-create income entry in Daily Report
                        try
                        {
                            string reportDate = !string.IsNullOrWhiteSpace(installment.InstallmentDate)
                                ? installment.InstallmentDate.Trim()
                                : DateTime.Now.ToString("dd.MM.yyyy");
                            await DailyReportsController.CreateAutoEntry(
                                _context,
                                reportDate,
                                "Income",
                                candidate.FirstName + " " + candidate.LastName,
                                installment.Amount,
                                $"Candidate installment #{installment.InstallmentNumber} - {request.PaymentMethod ?? "Cash"}",
                                "CandidateInstallment",
                                candidateInstallment.InstallmentId,
                                candidate.AddedBy
                            );
                        }
                        catch (Exception autoEx)
                        {
                            _logger.LogWarning(autoEx, "Failed to auto-create daily report entry for installment");
                        }
                    }
                }

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Candidates List with search and filters.
        ///Admin sees all; Instructor sees only candidates where InstructorId = current user.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetCandidatesList(string? search = null, int? categoryId = null, int? year = null)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                // Instructor: scope to own assigned candidates only
                var query = from c in _context.Candidates
                           join cat in _context.Categories on c.CategoryId equals cat.CategoryId into catGroup
                           from cat in catGroup.DefaultIfEmpty()
                           join u in _context.Users on c.InstructorId equals u.UserId into instructorGroup
                           from instructor in instructorGroup.DefaultIfEmpty()
                           select new CandidateInfo
                           {
                               CandidateId = c.CandidateId,
                               SerialNumber = c.SerialNumber,
                               FirstName = c.FirstName,
                               LastName = c.LastName,
                               PhoneNumber = c.PhoneNumber,
                               CategoryId = c.CategoryId,
                               CategoryName = cat != null ? cat.CategoryName : null,
                               InstructorId = c.InstructorId,
                               InstructorName = instructor != null ? instructor.FullName : null,
                               VehicleType = c.VehicleType,
                               PracticalHours = c.PracticalHours,
                               TotalServiceAmount = c.TotalServiceAmount,
                               DateAdded = c.DateAdded,
                               Year = c.DateAdded.Year
                           };

                if (role == "Instructor")
                    query = query.Where(c => c.InstructorId == currentUserId);

                // Apply search filter (FirstName, LastName, PersonalNumber)
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    var personalNumberSearch = search;
                    
                    // Get candidate IDs that match PersonalNumber
                    var personalNumberMatches = await _context.Candidates
                        .Where(c => c.PersonalNumber != null && c.PersonalNumber.Contains(personalNumberSearch))
                        .Select(c => c.CandidateId)
                        .ToListAsync();
                    
                    query = query.Where(c => 
                        (c.FirstName != null && c.FirstName.ToLower().Contains(searchLower)) ||
                        (c.LastName != null && c.LastName.ToLower().Contains(searchLower)) ||
                        personalNumberMatches.Contains(c.CandidateId));
                }

                // Apply category filter
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(c => c.CategoryId == categoryId.Value);
                }

                // Apply year filter
                if (year.HasValue && year.Value > 0)
                {
                    query = query.Where(c => c.Year == year.Value);
                }

                var list = await query.OrderByDescending(c => c.DateAdded).ToListAsync();

                // Collect lesson counts for all returned candidates in one query
                var candidateIds = list.Select(c => c.CandidateId).ToList();
                var lessonCounts = await _context.PracticalLessons
                    .Where(l => candidateIds.Contains(l.CandidateId))
                    .GroupBy(l => l.CandidateId)
                    .Select(g => new { CandidateId = g.Key, Count = g.Count() })
                    .ToListAsync();
                var countDict = lessonCounts.ToDictionary(x => x.CandidateId, x => x.Count);

                // For Instructor: also count only this instructor's lessons
                Dictionary<int, int> instructorCountDict = new();
                if (role == "Instructor")
                {
                    var instructorCounts = await _context.PracticalLessons
                        .Where(l => candidateIds.Contains(l.CandidateId) && l.InstructorUserId == currentUserId)
                        .GroupBy(l => l.CandidateId)
                        .Select(g => new { CandidateId = g.Key, Count = g.Count() })
                        .ToListAsync();
                    instructorCountDict = instructorCounts.ToDictionary(x => x.CandidateId, x => x.Count);
                }

                foreach (var c in list)
                {
                    c.PracticalLessonCount = countDict.GetValueOrDefault(c.CandidateId, 0);
                    c.CompletedHours = role == "Instructor"
                        ? instructorCountDict.GetValueOrDefault(c.CandidateId, 0)
                        : c.PracticalLessonCount;
                    c.RemainingHours = Math.Max(0, (c.PracticalHours ?? 0) - c.CompletedHours);
                }

                int totalRecords = list.Count;
                _logger.LogInformation("GetCandidatesList: returning {Count} candidates (search={Search}, categoryId={CatId}, year={Year})",
                    totalRecords, search ?? "(null)", categoryId ?? 0, year ?? 0);
                return Ok(new { data = list, recordsTotal = totalRecords, recordsFiltered = totalRecords });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCandidatesList failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single Candidate by ID with details. Instructor may only access candidates assigned to them.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetCandidateDetails(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";

                var candidate = await (from c in _context.Candidates
                                      join cat in _context.Categories on c.CategoryId equals cat.CategoryId
                                      join u in _context.Users on c.InstructorId equals u.UserId into instructorGroup
                                      from instructor in instructorGroup.DefaultIfEmpty()
                                      where c.CandidateId == id
                                      select new
                                      {
                                          CandidateId = c.CandidateId,
                                          SerialNumber = c.SerialNumber,
                                          FirstName = c.FirstName,
                                          ParentName = c.ParentName,
                                          LastName = c.LastName,
                                          DateOfBirth = c.DateOfBirth,
                                          PersonalNumber = c.PersonalNumber,
                                          PhoneNumber = c.PhoneNumber,
                                          PlaceOfBirth = c.PlaceOfBirth,
                                          Address = c.Address,
                                          CategoryId = c.CategoryId,
                                          CategoryName = cat.CategoryName,
                                          InstructorId = c.InstructorId,
                                          InstructorName = instructor != null ? instructor.FullName : null,
                                          VehicleType = c.VehicleType,
                                          PaymentMethod = c.PaymentMethod,
                                          PracticalHours = c.PracticalHours,
                                          TotalServiceAmount = c.TotalServiceAmount,
                                          DocWithdrawalAmount = c.DocWithdrawalAmount,
                                          DocWithdrawalDate = c.DocWithdrawalDate,
                                          DrivingPaymentAmount = c.DrivingPaymentAmount,
                                          DrivingPaymentDate = c.DrivingPaymentDate,
                                          DateAdded = c.DateAdded
                                      }).FirstOrDefaultAsync();

                if (candidate == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Candidate not found" });

                if (role == "Instructor" && candidate.InstructorId != currentUserId)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Not authorized to view this candidate" });

                // ── Instructor: return only limited fields ──
                if (role == "Instructor")
                {
                    var lessonsInstructor = await (from l in _context.PracticalLessons
                                                   join u in _context.Users on l.InstructorUserId equals u.UserId into uu
                                                   from u in uu.DefaultIfEmpty()
                                                   where l.CandidateId == id && l.InstructorUserId == currentUserId
                                                   orderby l.LessonDate ascending, l.Time ascending
                                                   select new
                                                   {
                                                       practicalLessonId = l.PracticalLessonId,
                                                       candidateId = l.CandidateId,
                                                       instructorUserId = l.InstructorUserId,
                                                       instructorName = u != null ? u.FullName : null,
                                                       lessonDate = l.LessonDate,
                                                       time = l.Time,
                                                       endTime = l.EndTime,
                                                       vehicle = l.Vehicle
                                                   }).ToListAsync();

                    return Ok(new
                    {
                        candidate = new
                        {
                            candidate.CandidateId,
                            candidate.SerialNumber,
                            candidate.FirstName,
                            candidate.LastName,
                            candidate.PhoneNumber,
                            candidate.Address,
                            candidate.VehicleType,
                            candidate.CategoryId,
                            candidate.CategoryName,
                            candidate.PracticalHours
                        },
                        installments = Array.Empty<object>(),
                        practicalLessons = lessonsInstructor,
                        drivingSessions = Array.Empty<object>(),
                        payments = Array.Empty<object>()
                    });
                }

                // ── Admin / SuperAdmin: full details ──
                var installments = await _context.CandidateInstallments
                    .Where(i => i.CandidateId == id)
                    .OrderBy(i => i.InstallmentNumber)
                    .Select(i => new
                    {
                        i.InstallmentId,
                        i.InstallmentNumber,
                        i.Amount,
                        i.InstallmentDate
                    })
                    .ToListAsync();

                var lessonsQuery = _context.PracticalLessons
                    .Where(l => l.CandidateId == id);
                var practicalLessons = await (from l in lessonsQuery
                                              join u in _context.Users on l.InstructorUserId equals u.UserId into uu
                                              from u in uu.DefaultIfEmpty()
                                              orderby l.LessonDate ascending, l.Time ascending
                                              select new
                                              {
                                                  practicalLessonId = l.PracticalLessonId,
                                                  candidateId = l.CandidateId,
                                                  instructorUserId = l.InstructorUserId,
                                                  instructorName = u != null ? u.FullName : null,
                                                  lessonDate = l.LessonDate,
                                                  time = l.Time,
                                                  endTime = l.EndTime,
                                                  vehicle = l.Vehicle
                                              }).ToListAsync();

                // ── Driving Sessions for this candidate ──
                var drivingSessions = await (from ds in _context.DrivingSessions
                                              join v in _context.Vehicles on ds.VehicleId equals v.VehicleId into vv
                                              from v in vv.DefaultIfEmpty()
                                              where ds.CandidateId == id
                                              orderby ds.DrivingDate descending, ds.DrivingTime descending
                                              select new
                                              {
                                                  ds.DrivingSessionId,
                                                  ds.DrivingDate,
                                                  ds.DrivingTime,
                                                  vehiclePlate = v != null ? v.PlateNumber : "",
                                                  vehicleBrand = v != null ? v.Brand : "",
                                                  ds.PaymentAmount,
                                                  ds.PaymentDate,
                                                  ds.Status,
                                                  ds.Examiner
                                              }).ToListAsync();

                // ── Aggregated Payments History ──
                var payments = new List<object>();

                // 1) Registration payment (from installments)
                foreach (var inst in installments)
                {
                    payments.Add(new
                    {
                        type = "Registration Installment",
                        description = $"Installment #{inst.InstallmentNumber}",
                        amount = inst.Amount,
                        date = inst.InstallmentDate,
                        status = (string?)null,
                        examiner = (string?)null,
                        sourceType = "CandidateInstallment",
                        sourceId = inst.InstallmentId
                    });
                }

                // 2) Document Withdrawal payment
                if (candidate.DocWithdrawalAmount.HasValue && candidate.DocWithdrawalAmount.Value > 0)
                {
                    payments.Add(new
                    {
                        type = "Document Withdrawal",
                        description = "Terheqja e Dokumentave",
                        amount = (decimal)candidate.DocWithdrawalAmount.Value,
                        date = candidate.DocWithdrawalDate ?? "",
                        status = (string?)null,
                        examiner = (string?)null,
                        sourceType = "CandidateDocWithdrawal",
                        sourceId = candidate.CandidateId
                    });
                }

                // 3) Driving Payment (single field on candidate)
                if (candidate.DrivingPaymentAmount.HasValue && candidate.DrivingPaymentAmount.Value > 0)
                {
                    payments.Add(new
                    {
                        type = "Driving Payment",
                        description = "Pagesa e Vozitjes",
                        amount = (decimal)candidate.DrivingPaymentAmount.Value,
                        date = candidate.DrivingPaymentDate ?? "",
                        status = (string?)null,
                        examiner = (string?)null,
                        sourceType = "CandidateDrivingPayment",
                        sourceId = candidate.CandidateId
                    });
                }

                // 4) Driving Session payments
                foreach (var ds in drivingSessions)
                {
                    if (ds.PaymentAmount > 0)
                    {
                        payments.Add(new
                        {
                            type = "Driving Session",
                            description = $"Session {ds.DrivingDate} {ds.DrivingTime} – {ds.vehiclePlate}",
                            amount = ds.PaymentAmount,
                            date = ds.PaymentDate ?? ds.DrivingDate,
                            status = ds.Status,
                            examiner = ds.Examiner,
                            sourceType = "DrivingSession",
                            sourceId = ds.DrivingSessionId
                        });
                    }
                }

                return Ok(new
                {
                    candidate = candidate,
                    installments = installments,
                    practicalLessons = practicalLessons,
                    drivingSessions = drivingSessions,
                    payments = payments
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Update Candidate
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{candidateId}")]
        public async Task<ActionResult> UpdateCandidate(int candidateId, CandidateCreateRequest request)
        {
            try
            {
                var existingCandidate = await _candidateRepo.SelectById(candidateId);
                if (existingCandidate == null)
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Candidate not found" });
                }

                // Validate installments
                int totalInstallments = request.Installments.Sum(i => i.Amount);
                if (totalInstallments > request.TotalServiceAmount)
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Sum of installments must not exceed Total service amount." });
                }

                // Track old values for daily report diff
                int? oldDocAmount = existingCandidate.DocWithdrawalAmount;
                int? oldDrivingAmount = existingCandidate.DrivingPaymentAmount;

                // Update candidate
                existingCandidate.SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim();
                existingCandidate.FirstName = request.FirstName;
                existingCandidate.ParentName = request.ParentName;
                existingCandidate.LastName = request.LastName;
                existingCandidate.DateOfBirth = request.DateOfBirth;
                existingCandidate.PersonalNumber = request.PersonalNumber;
                existingCandidate.PhoneNumber = request.PhoneNumber;
                existingCandidate.PlaceOfBirth = request.PlaceOfBirth;
                existingCandidate.Address = request.Address;
                existingCandidate.CategoryId = request.CategoryId;
                existingCandidate.InstructorId = request.InstructorId;
                existingCandidate.VehicleType = request.VehicleType;
                existingCandidate.PaymentMethod = request.PaymentMethod;
                existingCandidate.PracticalHours = request.PracticalHours;
                existingCandidate.TotalServiceAmount = request.TotalServiceAmount;
                existingCandidate.DocWithdrawalAmount = request.DocWithdrawalAmount;
                existingCandidate.DocWithdrawalDate = string.IsNullOrWhiteSpace(request.DocWithdrawalDate) ? null : request.DocWithdrawalDate.Trim();
                existingCandidate.DrivingPaymentAmount = request.DrivingPaymentAmount;
                existingCandidate.DrivingPaymentDate = string.IsNullOrWhiteSpace(request.DrivingPaymentDate) ? null : request.DrivingPaymentDate.Trim();
                existingCandidate.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                existingCandidate.LastUpdatedDate = DateTime.Now;

                await _candidateRepo.Update(existingCandidate);

                // Delete existing installments
                var existingInstallments = await _context.CandidateInstallments
                    .Where(i => i.CandidateId == candidateId)
                    .ToListAsync();
                foreach (var inst in existingInstallments)
                {
                    await _installmentRepo.Delete(inst.InstallmentId);
                }

                // Create new installments
                foreach (var installment in request.Installments)
                {
                    if (installment.Amount > 0)
                    {
                        var candidateInstallment = new CandidateInstallment
                        {
                            CandidateId = candidateId,
                            InstallmentNumber = installment.InstallmentNumber,
                            Amount = installment.Amount,
                            InstallmentDate = installment.InstallmentDate,
                            AddedBy = existingCandidate.AddedBy,
                            DateAdded = DateTime.Now
                        };
                        await _installmentRepo.Insert(candidateInstallment);
                    }
                }

                // Auto daily report entries for new/increased payment fields
                string fullName = (existingCandidate.FirstName ?? "") + " " + (existingCandidate.LastName ?? "");
                int currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "1");

                // Document Withdrawal: create entry only if amount is new or increased
                int newDocAmt = existingCandidate.DocWithdrawalAmount ?? 0;
                int prevDocAmt = oldDocAmount ?? 0;
                if (newDocAmt > prevDocAmt && (newDocAmt - prevDocAmt) > 0)
                {
                    try
                    {
                        string rptDate = !string.IsNullOrWhiteSpace(existingCandidate.DocWithdrawalDate)
                            ? existingCandidate.DocWithdrawalDate : DateTime.Now.ToString("dd.MM.yyyy");
                        await DailyReportsController.CreateAutoEntry(
                            _context, rptDate, "Income", fullName,
                            newDocAmt - prevDocAmt,
                            "Terheqja e Dokumentave (Document Withdrawal)",
                            "CandidateDocWithdrawal", existingCandidate.CandidateId, currentUserId);
                    }
                    catch (Exception autoEx) { _logger.LogWarning(autoEx, "Auto daily report (DocWithdrawal update) failed"); }
                }

                // Driving Payment: create entry only if amount is new or increased
                int newDrvAmt = existingCandidate.DrivingPaymentAmount ?? 0;
                int prevDrvAmt = oldDrivingAmount ?? 0;
                if (newDrvAmt > prevDrvAmt && (newDrvAmt - prevDrvAmt) > 0)
                {
                    try
                    {
                        string rptDate = !string.IsNullOrWhiteSpace(existingCandidate.DrivingPaymentDate)
                            ? existingCandidate.DrivingPaymentDate : DateTime.Now.ToString("dd.MM.yyyy");
                        await DailyReportsController.CreateAutoEntry(
                            _context, rptDate, "Income", fullName,
                            newDrvAmt - prevDrvAmt,
                            "Pagesa e Vozitjes (Driving Payment)",
                            "CandidateDrivingPayment", existingCandidate.CandidateId, currentUserId);
                    }
                    catch (Exception autoEx) { _logger.LogWarning(autoEx, "Auto daily report (DrivingPayment update) failed"); }
                }

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Updated" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get practical lessons for a candidate. Admin sees all; Instructor sees only their own.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet("{candidateId}")]
        public async Task<ActionResult> GetPracticalLessons(int candidateId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";
                var candidate = await _context.Candidates.FindAsync(candidateId);
                if (candidate == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Candidate not found" });
                if (role == "Instructor" && candidate.InstructorId != currentUserId)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Not authorized" });

                var query = _context.PracticalLessons.Where(l => l.CandidateId == candidateId);
                if (role == "Instructor")
                    query = query.Where(l => l.InstructorUserId == currentUserId);
                var list = await (from l in query
                                 join u in _context.Users on l.InstructorUserId equals u.UserId into uu
                                 from u in uu.DefaultIfEmpty()
                                 orderby l.LessonDate ascending, l.Time ascending
                                 select new
                                 {
                                     practicalLessonId = l.PracticalLessonId,
                                     candidateId = l.CandidateId,
                                     instructorUserId = l.InstructorUserId,
                                     instructorName = u != null ? u.FullName : null,
                                     lessonDate = l.LessonDate,
                                     time = l.Time,
                                     endTime = l.EndTime,
                                     vehicle = l.Vehicle
                                 }).ToListAsync();
                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Add a practical lesson. Instructor may add only for candidates assigned to them.
        ///Lesson duration is fixed at 45 minutes. EndTime is auto-calculated.
        ///Overlap check: same instructor or same vehicle on the same date must not overlap.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpPost]
        public async Task<ActionResult> AddPracticalLesson(AddPracticalLessonRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new Confirmation { Status = "error", ResponseMsg = "Invalid token – user id missing" });

                var role = User.FindFirst("role")?.Value ?? "";
                var candidate = await _context.Candidates.FindAsync(request.CandidateId);
                if (candidate == null)
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Candidate not found" });
                if (role == "Instructor" && candidate.InstructorId != currentUserId)
                    return StatusCode(403, new Confirmation { Status = "error", ResponseMsg = "Not authorized to add lessons for this candidate" });

                // Parse and validate required fields
                if (string.IsNullOrWhiteSpace(request.Time) || string.IsNullOrWhiteSpace(request.LessonDate))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Date and Time are required" });

                // Validate date format dd.MM.yyyy (max 10 chars)
                var lessonDate = request.LessonDate.Trim();
                if (lessonDate.Length > 10 || !System.Text.RegularExpressions.Regex.IsMatch(lessonDate, @"^\d{2}\.\d{2}\.\d{4}$"))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid date format. Use dd.MM.yyyy" });

                if (!TimeSpan.TryParse(request.Time, out var startTs))
                    return BadRequest(new Confirmation { Status = "error", ResponseMsg = "Invalid time format. Use HH:mm" });

                var endTs = startTs.Add(TimeSpan.FromMinutes(45));
                var endTime = $"{endTs.Hours:D2}:{endTs.Minutes:D2}";

                // Overlap check: same instructor on same date must not have overlapping time
                var sameDayLessons = await _context.PracticalLessons
                    .Where(l => l.LessonDate == request.LessonDate &&
                                (l.InstructorUserId == currentUserId ||
                                 (!string.IsNullOrEmpty(request.Vehicle) && l.Vehicle == request.Vehicle)))
                    .ToListAsync();

                foreach (var existing in sameDayLessons)
                {
                    if (!TimeSpan.TryParse(existing.Time, out var exStart)) continue;
                    var exEnd = !string.IsNullOrEmpty(existing.EndTime) && TimeSpan.TryParse(existing.EndTime, out var parsedEnd)
                        ? parsedEnd
                        : exStart.Add(TimeSpan.FromMinutes(45));

                    bool overlaps = startTs < exEnd && endTs > exStart;
                    if (overlaps)
                    {
                        if (existing.InstructorUserId == currentUserId)
                            return BadRequest(new Confirmation { Status = "error", ResponseMsg = $"Overlap: you already have a lesson at {existing.Time}–{existing.EndTime ?? exEnd.ToString(@"hh\:mm")} on {request.LessonDate}" });
                        if (!string.IsNullOrEmpty(request.Vehicle) && existing.Vehicle == request.Vehicle)
                            return BadRequest(new Confirmation { Status = "error", ResponseMsg = $"Overlap: vehicle '{request.Vehicle}' is booked at {existing.Time}–{existing.EndTime ?? exEnd.ToString(@"hh\:mm")} on {request.LessonDate}" });
                    }
                }

                var lesson = new PracticalLesson
                {
                    CandidateId = request.CandidateId,
                    InstructorUserId = currentUserId,
                    LessonDate = lessonDate,
                    Time = request.Time.Trim(),
                    EndTime = endTime,
                    Vehicle = request.Vehicle,
                    DateAdded = DateTime.UtcNow
                };
                _context.PracticalLessons.Add(lesson);
                await _context.SaveChangesAsync();

                // Return the created lesson so the UI can update immediately
                var instructorName = await _context.Users
                    .Where(u => u.UserId == currentUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    status = "success",
                    responseMsg = "Lesson added",
                    lesson = new
                    {
                        practicalLessonId = lesson.PracticalLessonId,
                        candidateId = lesson.CandidateId,
                        instructorUserId = lesson.InstructorUserId,
                        instructorName = instructorName,
                        lessonDate = lesson.LessonDate,
                        time = lesson.Time,
                        endTime = lesson.EndTime,
                        vehicle = lesson.Vehicle
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddPracticalLesson failed");
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Failed to save lesson. " + ex.Message });
            }
        }

        ///<summary>
        ///Get registered vehicle names for the practical lesson dropdown. Admin and Instructor.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetLessonVehicles()
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.PlateNumber)
                .Select(v => new { v.VehicleId, label = v.PlateNumber + " – " + v.Brand, v.PlateNumber })
                .ToListAsync();
            return Ok(new { data = vehicles });
        }

        ///<summary>
        ///Download a pre-filled PDF Application form for a Candidate.
        ///Overlays candidate data onto the official "aplikacioni.pdf" template.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> DownloadApplication(int id)
        {
            try
            {
                // ── Load candidate + category ──
                var data = await (from c in _context.Candidates
                                  join cat in _context.Categories on c.CategoryId equals cat.CategoryId into catG
                                  from cat in catG.DefaultIfEmpty()
                                  where c.CandidateId == id
                                  select new
                                  {
                                      c.CandidateId,
                                      c.SerialNumber,
                                      c.FirstName,
                                      c.ParentName,
                                      c.LastName,
                                      c.DateOfBirth,
                                      c.PersonalNumber,
                                      c.PlaceOfBirth,
                                      c.Address,
                                      CategoryName = cat != null ? cat.CategoryName : null
                                  }).FirstOrDefaultAsync();

                if (data == null)
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "Candidate not found" });

                // ── Locate template ──
                string templatePath = Path.Combine(_env.ContentRootPath, "Resources", "Templates", "aplikacioni.pdf");
                if (!System.IO.File.Exists(templatePath))
                    return NotFound(new Confirmation { Status = "error", ResponseMsg = "PDF template not found" });

                // ── Generate stamped PDF ──
                using var ms = new MemoryStream();
                using (var reader = new PdfReader(templatePath))
                using (var writer = new PdfWriter(ms))
                {
                    // Do not close output stream so we can read from ms
                    writer.SetCloseStream(false);
                    using var pdfDoc = new PdfDocument(reader, writer);
                    var page = pdfDoc.GetFirstPage();
                    var pageSize = page.GetPageSize();
                    float pageW = pageSize.GetWidth();   // ~595
                    float pageH = pageSize.GetHeight();   // ~839

                    var canvas = new PdfCanvas(page);
                    var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    float fontSize = 11f;

                    // Helper: draw text at absolute position (x from left, y from bottom)
                    void StampText(float x, float y, string? text, PdfFont f = null, float fs = 0)
                    {
                        if (string.IsNullOrWhiteSpace(text)) return;
                        f ??= font;
                        if (fs <= 0) fs = fontSize;
                        canvas.BeginText()
                              .SetFontAndSize(f, fs)
                              .MoveText(x, y)
                              .ShowText(text)
                              .EndText();
                    }

                    // ────────────────────────────────────────────────────
                    // FIELD COORDINATES (measured from bottom-left origin)
                    // These are calibrated for the Kosovo "Aplikacioni"
                    // scanned form at ~595 x 839 points.
                    // The form is a scanned image; coordinates may need
                    // minor adjustments for different scan resolutions.
                    // ────────────────────────────────────────────────────

                    // ── Header: Reg./Book No. (top-right area) ──
                    StampText(460, pageH - 80, data.SerialNumber ?? "", fontBold, 12f);

                    // ── APPLICANT'S DETAILS box ──
                    // The box occupies roughly the middle portion of the page.
                    // Fields are arranged as labeled rows in the form.

                    // Family Name / Mbiemri / Prezime
                    float detailsBaseY = pageH - 253;
                    StampText(200, detailsBaseY, data.LastName ?? "");

                    // Father's Name / Emri i babait
                    StampText(200, detailsBaseY - 24, data.ParentName ?? "");

                    // First Name / Emri / Ime
                    StampText(200, detailsBaseY - 48, data.FirstName ?? "");

                    // Date of Birth (dd.MM.yyyy)
                    StampText(200, detailsBaseY - 72, data.DateOfBirth ?? "");

                    // Place of Birth
                    StampText(200, detailsBaseY - 96, data.PlaceOfBirth ?? "");

                    // Municipality / Komuna – use address as fallback
                    string municipality = "/";
                    if (!string.IsNullOrWhiteSpace(data.Address))
                    {
                        // Try to extract municipality from address (e.g., "Street, City" → "City")
                        var parts = data.Address.Split(',', StringSplitOptions.TrimEntries);
                        municipality = parts.Length > 1 ? parts[^1] : parts[0];
                    }
                    StampText(200, detailsBaseY - 120, municipality);

                    // Personal Number / Numri personal
                    StampText(200, detailsBaseY - 144, data.PersonalNumber ?? "");

                    // ── Category marking ──
                    // The categories appear as checkboxes in a row.
                    // We place an "X" at the appropriate position.
                    if (!string.IsNullOrWhiteSpace(data.CategoryName))
                    {
                        // Category checkbox X-positions (approximate, in the category row)
                        float catY = pageH - 465;
                        var categoryPositions = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "A1", 90 },
                            { "A2", 131 },
                            { "A",  171 },
                            { "B",  210 },
                            { "B+E", 262 },
                            { "C1", 318 },
                            { "C",  365 }
                        };

                        if (categoryPositions.TryGetValue(data.CategoryName.Trim(), out float catX))
                        {
                            StampText(catX, catY, "X", fontBold, 13f);
                        }
                    }
                }

                ms.Position = 0;
                string lastName = (data.LastName ?? "Candidate").Replace(" ", "_");
                string firstName = (data.FirstName ?? "").Replace(" ", "_");
                string fileName = $"Application_{lastName}_{firstName}_{data.CandidateId}.pdf";

                return File(ms.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadApplication failed for candidate {Id}", id);
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = "Failed to generate application PDF: " + ex.Message });
            }
        }

        ///<summary>
        ///Get Available Years for filtering. Instructor sees years only from their assigned candidates.
        ///</summary>
        [Authorize(Roles = "SuperAdmin,Admin,Instructor")]
        [HttpGet]
        public async Task<ActionResult> GetAvailableYears()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var role = User.FindFirst("role")?.Value ?? "";
                var query = _context.Candidates.AsQueryable();
                if (role == "Instructor")
                    query = query.Where(c => c.InstructorId == currentUserId);
                var years = await query
                    .Select(c => c.DateAdded.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                return Ok(new { data = years });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }
    }
}
