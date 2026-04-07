using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AdditionalLessonsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<AdditionalLesson> _repo;
        private readonly ISqlRepository<AdditionalLessonInstallment> _installmentRepo;
        private readonly ILogger<AdditionalLessonsController> _logger;

        public AdditionalLessonsController(
            AppDbContext context,
            ISqlRepository<AdditionalLesson> repo,
            ISqlRepository<AdditionalLessonInstallment> installmentRepo,
            ILogger<AdditionalLessonsController> logger)
        {
            _context = context;
            _repo = repo;
            _installmentRepo = installmentRepo;
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetList(string? search = null, int? categoryId = null)
        {
            try
            {
                var query = from al in _context.AdditionalLessons
                            join cat in _context.Categories on al.CategoryId equals cat.CategoryId into catGroup
                            from cat in catGroup.DefaultIfEmpty()
                            join u in _context.Users on al.InstructorId equals u.UserId into instrGroup
                            from instructor in instrGroup.DefaultIfEmpty()
                            select new AdditionalLessonInfo
                            {
                                AdditionalLessonId = al.AdditionalLessonId,
                                FirstName = al.FirstName,
                                LastName = al.LastName,
                                PersonalNumber = al.PersonalNumber,
                                ContactNumber = al.ContactNumber,
                                CategoryId = al.CategoryId,
                                CategoryName = cat != null ? cat.CategoryName : null,
                                InstructorId = al.InstructorId,
                                InstructorName = instructor != null ? instructor.FullName : null,
                                VehicleType = al.VehicleType,
                                PaymentMethod = al.PaymentMethod,
                                PracticalHours = al.PracticalHours,
                                ServicePayment = al.ServicePayment,
                                AdditionalNotes = al.AdditionalNotes,
                                DateAdded = al.DateAdded
                            };

                if (!string.IsNullOrEmpty(search))
                {
                    var s = search.ToLower();
                    query = query.Where(a =>
                        (a.FirstName != null && a.FirstName.ToLower().Contains(s)) ||
                        (a.LastName != null && a.LastName.ToLower().Contains(s)) ||
                        (a.PersonalNumber != null && a.PersonalNumber.Contains(search)) ||
                        (a.ContactNumber != null && a.ContactNumber.Contains(search)));
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                    query = query.Where(a => a.CategoryId == categoryId.Value);

                var list = await query.OrderByDescending(a => a.DateAdded).ToListAsync();

                var ids = list.Select(x => x.AdditionalLessonId).ToList();
                var installmentSums = await _context.AdditionalLessonInstallments
                    .Where(i => ids.Contains(i.AdditionalLessonId))
                    .GroupBy(i => i.AdditionalLessonId)
                    .Select(g => new { AdditionalLessonId = g.Key, Total = g.Sum(i => i.Amount) })
                    .ToListAsync();

                var sumLookup = installmentSums.ToDictionary(x => x.AdditionalLessonId, x => x.Total);
                foreach (var item in list)
                {
                    item.TotalPaidAmount = sumLookup.ContainsKey(item.AdditionalLessonId) ? sumLookup[item.AdditionalLessonId] : 0;
                }

                return Ok(new { data = list, recordsTotal = list.Count, recordsFiltered = list.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetList failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var item = await (from al in _context.AdditionalLessons
                                 join cat in _context.Categories on al.CategoryId equals cat.CategoryId into catGroup
                                 from cat in catGroup.DefaultIfEmpty()
                                 join u in _context.Users on al.InstructorId equals u.UserId into instrGroup
                                 from instructor in instrGroup.DefaultIfEmpty()
                                 where al.AdditionalLessonId == id
                                 select new
                                 {
                                     al.AdditionalLessonId,
                                     al.FirstName,
                                     al.LastName,
                                     al.PersonalNumber,
                                     al.ContactNumber,
                                     al.CategoryId,
                                     CategoryName = cat != null ? cat.CategoryName : null,
                                     al.InstructorId,
                                     InstructorName = instructor != null ? instructor.FullName : null,
                                     al.VehicleType,
                                     al.PaymentMethod,
                                     al.PracticalHours,
                                     al.ServicePayment,
                                     al.AdditionalNotes,
                                     al.DateAdded
                                 }).FirstOrDefaultAsync();

                if (item == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Regjistrimi nuk u gjet" });

                var installments = await _context.AdditionalLessonInstallments
                    .Where(i => i.AdditionalLessonId == id)
                    .OrderBy(i => i.InstallmentNumber)
                    .Select(i => new
                    {
                        i.InstallmentId,
                        i.InstallmentNumber,
                        i.Amount,
                        i.InstallmentDate
                    })
                    .ToListAsync();

                return Ok(new { additionalLesson = item, installments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById failed for id {Id}", id);
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> Create(AdditionalLessonRequest request)
        {
            try
            {
                int totalInstallments = request.Installments.Sum(i => i.Amount);
                if (totalInstallments > (int)request.ServicePayment)
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Shuma e kesteve nuk duhet te tejkaloje shumen totale te sherbimit." });
                }

                var entity = new AdditionalLesson
                {
                    FirstName = request.FirstName ?? "",
                    LastName = request.LastName ?? "",
                    PersonalNumber = request.PersonalNumber,
                    ContactNumber = request.ContactNumber ?? "",
                    CategoryId = request.CategoryId,
                    InstructorId = request.InstructorId,
                    VehicleType = request.VehicleType,
                    PaymentMethod = request.PaymentMethod,
                    PracticalHours = request.PracticalHours,
                    ServicePayment = request.ServicePayment,
                    AdditionalNotes = request.AdditionalNotes,
                    AddedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1"),
                    DateAdded = DateTime.Now
                };

                await _repo.Insert(entity);

                foreach (var installment in request.Installments)
                {
                    if (installment.Amount > 0)
                    {
                        var inst = new AdditionalLessonInstallment
                        {
                            AdditionalLessonId = entity.AdditionalLessonId,
                            InstallmentNumber = installment.InstallmentNumber,
                            Amount = installment.Amount,
                            InstallmentDate = installment.InstallmentDate,
                            AddedBy = entity.AddedBy,
                            DateAdded = DateTime.Now
                        };
                        await _installmentRepo.Insert(inst);

                        try
                        {
                            string reportDate = !string.IsNullOrWhiteSpace(installment.InstallmentDate)
                                ? installment.InstallmentDate.Trim()
                                : DateTime.Now.ToString("dd.MM.yyyy");
                            await DailyReportsController.CreateAutoEntry(
                                _context,
                                reportDate,
                                "Income",
                                entity.FirstName + " " + entity.LastName,
                                installment.Amount,
                                $"Orë shtesë - kësti #{installment.InstallmentNumber} - {request.PaymentMethod ?? "Cash"}",
                                "AdditionalLessonInstallment",
                                inst.InstallmentId,
                                entity.AddedBy
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Auto daily-report entry failed for installment");
                        }
                    }
                }

                return Ok(new Confirmation { Status = "success", ResponseMsg = "U ruajt me sukses" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create failed");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(int id, AdditionalLessonRequest request)
        {
            try
            {
                var existing = await _repo.SelectById(id);
                if (existing == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Regjistrimi nuk u gjet" });

                int totalInstallments = request.Installments.Sum(i => i.Amount);
                if (totalInstallments > (int)request.ServicePayment)
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Shuma e kesteve nuk duhet te tejkaloje shumen totale te sherbimit." });
                }

                existing.FirstName = request.FirstName ?? "";
                existing.LastName = request.LastName ?? "";
                existing.PersonalNumber = request.PersonalNumber;
                existing.ContactNumber = request.ContactNumber ?? "";
                existing.CategoryId = request.CategoryId;
                existing.InstructorId = request.InstructorId;
                existing.VehicleType = request.VehicleType;
                existing.PaymentMethod = request.PaymentMethod;
                existing.PracticalHours = request.PracticalHours;
                existing.ServicePayment = request.ServicePayment;
                existing.AdditionalNotes = request.AdditionalNotes;
                existing.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                existing.LastUpdatedDate = DateTime.Now;

                await _repo.Update(existing);

                var existingInstallments = await _context.AdditionalLessonInstallments
                    .Where(i => i.AdditionalLessonId == id)
                    .ToListAsync();
                foreach (var inst in existingInstallments)
                {
                    await _installmentRepo.Delete(inst.InstallmentId);
                }

                foreach (var installment in request.Installments)
                {
                    if (installment.Amount > 0)
                    {
                        var inst = new AdditionalLessonInstallment
                        {
                            AdditionalLessonId = id,
                            InstallmentNumber = installment.InstallmentNumber,
                            Amount = installment.Amount,
                            InstallmentDate = installment.InstallmentDate,
                            AddedBy = existing.AddedBy,
                            DateAdded = DateTime.Now
                        };
                        await _installmentRepo.Insert(inst);
                    }
                }

                return Ok(new Confirmation { Status = "success", ResponseMsg = "U perditesua me sukses" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update failed for id {Id}", id);
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.SelectById(id);
                if (existing == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Regjistrimi nuk u gjet" });

                var installments = await _context.AdditionalLessonInstallments
                    .Where(i => i.AdditionalLessonId == id)
                    .ToListAsync();
                foreach (var inst in installments)
                {
                    await _installmentRepo.Delete(inst.InstallmentId);
                }

                await _repo.Delete(id);
                return Ok(new Confirmation { Status = "success", ResponseMsg = "U fshi me sukses" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete failed for id {Id}", id);
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }
    }
}
