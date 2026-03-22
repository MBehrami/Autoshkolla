using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.ETestimi;
using AdminApi.Models.Helper;
using AdminApi.ViewModels.ETestimi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class CandidateAccountsController : ControllerBase
    {
        private sealed class ExamCategoryAccessVm
        {
            public int ExamCategoryId { get; set; }

            public string Code { get; set; } = string.Empty;

            public string Title { get; set; } = string.Empty;
        }

        private readonly AppDbContext _context;

        public CandidateAccountsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetList(string? search = null, bool? isActive = null)
        {
            try
            {
                var query = from a in _context.CandidateAccounts
                            join c in _context.Candidates on a.CandidateId equals c.CandidateId into candidateJoin
                            from c in candidateJoin.DefaultIfEmpty()
                            join cat in _context.Categories on c.CategoryId equals cat.CategoryId into categoryJoin
                            from cat in categoryJoin.DefaultIfEmpty()
                            select new
                            {
                                a.CandidateAccountId,
                                a.CandidateId,
                                AccountFirstName = a.FirstName,
                                AccountLastName = a.LastName,
                                CandidateFirstName = c != null ? c.FirstName : null,
                                CandidateLastName = c != null ? c.LastName : null,
                                a.PhoneNumber,
                                a.Email,
                                a.ValidTo,
                                a.IsActive,
                                CategoryName = cat != null ? cat.CategoryName : null
                            };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(x =>
                        (((x.CandidateFirstName ?? x.AccountFirstName ?? "") + " " + (x.CandidateLastName ?? x.AccountLastName ?? "")).ToLower().Contains(s)) ||
                        x.PhoneNumber.ToLower().Contains(s) ||
                        (x.Email ?? "").ToLower().Contains(s));
                }

                if (isActive.HasValue)
                    query = query.Where(x => x.IsActive == isActive.Value);

                var list = await query
                    .OrderBy(x => x.CandidateFirstName ?? x.AccountFirstName)
                    .ThenBy(x => x.CandidateLastName ?? x.AccountLastName)
                    .ToListAsync();

                var accountIds = list.Select(x => x.CandidateAccountId).ToList();
                var accountCategoryRows = await (from map in _context.CandidateAccountExamCategoryAccesses
                                                 join examCategory in _context.ExamCategories on map.ExamCategoryId equals examCategory.ExamCategoryId
                                                 where accountIds.Contains(map.CandidateAccountId) && map.IsActive
                                                 orderby examCategory.SortOrder, examCategory.Code
                                                 select new
                                                 {
                                                     map.CandidateAccountId,
                                                     examCategory.ExamCategoryId,
                                                     examCategory.Code,
                                                     examCategory.Title
                                                 }).ToListAsync();

                var accountCategoryLookup = accountCategoryRows
                    .GroupBy(x => x.CandidateAccountId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => new ExamCategoryAccessVm
                        {
                            ExamCategoryId = x.ExamCategoryId,
                            Code = x.Code,
                            Title = x.Title
                        }).ToList());

                return Ok(new
                {
                    data = list.Select(x =>
                    {
                        accountCategoryLookup.TryGetValue(x.CandidateAccountId, out var assignedExamCategories);
                        assignedExamCategories ??= new List<ExamCategoryAccessVm>();

                        return new
                        {
                            candidateAccountId = x.CandidateAccountId,
                            candidateId = x.CandidateId,
                            firstName = x.CandidateFirstName ?? x.AccountFirstName,
                            lastName = x.CandidateLastName ?? x.AccountLastName,
                            fullName = string.Join(" ", new[] { x.CandidateFirstName ?? x.AccountFirstName, x.CandidateLastName ?? x.AccountLastName }.Where(v => !string.IsNullOrWhiteSpace(v))).Trim(),
                            phoneNumber = x.PhoneNumber,
                            email = x.Email,
                            validTo = x.ValidTo,
                            isActive = x.IsActive,
                            categoryName = x.CategoryName,
                            examCategoryIds = assignedExamCategories.Select(c => c.ExamCategoryId).ToList(),
                            examCategories = assignedExamCategories.Select(c => new
                            {
                                examCategoryId = c.ExamCategoryId,
                                code = c.Code,
                                title = c.Title
                            }).ToList(),
                            examCategoryDisplay = string.Join(", ", assignedExamCategories.Select(c => c.Code))
                        };
                    }),
                    recordsTotal = list.Count,
                    recordsFiltered = list.Count
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetails(int id)
        {
            try
            {
                var data = await (from a in _context.CandidateAccounts
                                  join c in _context.Candidates on a.CandidateId equals c.CandidateId into candidateJoin
                                  from c in candidateJoin.DefaultIfEmpty()
                                  where a.CandidateAccountId == id
                                  select new
                                  {
                                      a.CandidateAccountId,
                                      a.CandidateId,
                                      firstName = c != null ? c.FirstName : a.FirstName,
                                      lastName = c != null ? c.LastName : a.LastName,
                                      fullName = string.Join(" ", new[] { c != null ? c.FirstName : a.FirstName, c != null ? c.LastName : a.LastName }.Where(v => !string.IsNullOrWhiteSpace(v))),
                                      a.PhoneNumber,
                                      a.Email,
                                      a.ValidTo,
                                      a.IsActive
                                  }).FirstOrDefaultAsync();

                if (data == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var examCategories = await (from map in _context.CandidateAccountExamCategoryAccesses
                                            join category in _context.ExamCategories on map.ExamCategoryId equals category.ExamCategoryId
                                            where map.CandidateAccountId == id && map.IsActive
                                            orderby category.SortOrder, category.Code
                                            select new
                                            {
                                                examCategoryId = category.ExamCategoryId,
                                                code = category.Code,
                                                title = category.Title
                                            }).ToListAsync();

                return Ok(new
                {
                    data.CandidateAccountId,
                    data.CandidateId,
                    data.firstName,
                    data.lastName,
                    data.fullName,
                    data.PhoneNumber,
                    data.Email,
                    data.ValidTo,
                    data.IsActive,
                    examCategoryIds = examCategories.Select(x => x.examCategoryId).ToList(),
                    examCategories,
                    examCategoryDisplay = string.Join(", ", examCategories.Select(x => x.code))
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(CandidateAccountCreateRequest request)
        {
            try
            {
                var examCategoryIds = request.ExamCategoryIds
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (examCategoryIds.Count == 0)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ju lutem zgjidhni te pakten nje kategori provimi." });

                var activeCategoryCount = await _context.ExamCategories
                    .CountAsync(c => c.IsActive && examCategoryIds.Contains(c.ExamCategoryId));

                if (activeCategoryCount != examCategoryIds.Count)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Nje ose me shume kategori te zgjedhura te provimit jane te pavlefshme ose joaktive." });

                if (request.CandidateId.HasValue)
                {
                    var candidateExists = await _context.Candidates.AnyAsync(c => c.CandidateId == request.CandidateId.Value);
                    if (!candidateExists)
                        return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kandidati nuk u gjet." });

                    var hasAccount = await _context.CandidateAccounts.AnyAsync(a => a.CandidateId == request.CandidateId.Value);
                    if (hasAccount)
                        return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Ky kandidat tashme ka nje llogari." });
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                        return Accepted(new Confirmation { Status = "error", ResponseMsg = "Emri dhe mbiemri kerkohen per krijimin manual te llogarise." });
                }

                var phone = request.PhoneNumber.Trim();
                var duplicatePhone = await _context.CandidateAccounts.AnyAsync(a => a.PhoneNumber == phone);
                if (duplicatePhone)
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Numri i telefonit eshte perdorur tashme." });

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var duplicateEmail = await _context.CandidateAccounts.AnyAsync(a => a.Email == request.Email.Trim());
                    if (duplicateEmail)
                        return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Email-i eshte perdorur tashme." });
                }

                var salt = PasswordHasher.GenerateSalt();
                var hashedPassword = PasswordHasher.HashPassword(request.InitialPassword, salt);
                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                var now = DateTime.UtcNow;

                var account = new CandidateAccount
                {
                    CandidateId = request.CandidateId,
                    FirstName = request.CandidateId.HasValue ? null : request.FirstName?.Trim(),
                    LastName = request.CandidateId.HasValue ? null : request.LastName?.Trim(),
                    PhoneNumber = phone,
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                    Password = hashedPassword,
                    PasswordSalt = salt,
                    ValidTo = request.ValidTo,
                    IsActive = true,
                    AddedBy = userId,
                    DateAdded = now
                };

                _context.CandidateAccounts.Add(account);
                await _context.SaveChangesAsync();

                var mappingRows = examCategoryIds.Select(categoryId => new CandidateAccountExamCategoryAccess
                {
                    CandidateAccountId = account.CandidateAccountId,
                    ExamCategoryId = categoryId,
                    IsActive = true,
                    AddedBy = userId,
                    DateAdded = now
                });

                _context.CandidateAccountExamCategoryAccesses.AddRange(mappingRows);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Llogaria e kandidatit u krijua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Update(int id, CandidateAccountUpdateRequest request)
        {
            try
            {
                var examCategoryIds = request.ExamCategoryIds
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (examCategoryIds.Count == 0)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ju lutem zgjidhni te pakten nje kategori provimi." });

                var activeCategoryCount = await _context.ExamCategories
                    .CountAsync(c => c.IsActive && examCategoryIds.Contains(c.ExamCategoryId));

                if (activeCategoryCount != examCategoryIds.Count)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Nje ose me shume kategori te zgjedhura te provimit jane te pavlefshme ose joaktive." });

                var account = await _context.CandidateAccounts.FirstOrDefaultAsync(a => a.CandidateAccountId == id);
                if (account == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                var now = DateTime.UtcNow;

                var phone = request.PhoneNumber.Trim();
                var duplicatePhone = await _context.CandidateAccounts.AnyAsync(a => a.CandidateAccountId != id && a.PhoneNumber == phone);
                if (duplicatePhone)
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Numri i telefonit eshte perdorur tashme." });

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var email = request.Email.Trim();
                    var duplicateEmail = await _context.CandidateAccounts.AnyAsync(a => a.CandidateAccountId != id && a.Email == email);
                    if (duplicateEmail)
                        return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Email-i eshte perdorur tashme." });
                    account.Email = email;
                }
                else
                {
                    account.Email = null;
                }

                account.PhoneNumber = phone;
                account.ValidTo = request.ValidTo;
                account.LastUpdatedDate = now;
                account.LastUpdatedBy = userId;

                var existingMappings = await _context.CandidateAccountExamCategoryAccesses
                    .Where(x => x.CandidateAccountId == id)
                    .ToListAsync();

                var existingByCategory = existingMappings.ToDictionary(x => x.ExamCategoryId, x => x);

                foreach (var categoryId in examCategoryIds)
                {
                    if (existingByCategory.TryGetValue(categoryId, out var mapping))
                    {
                        if (!mapping.IsActive)
                        {
                            mapping.IsActive = true;
                            mapping.LastUpdatedDate = now;
                            mapping.LastUpdatedBy = userId;
                        }
                    }
                    else
                    {
                        _context.CandidateAccountExamCategoryAccesses.Add(new CandidateAccountExamCategoryAccess
                        {
                            CandidateAccountId = id,
                            ExamCategoryId = categoryId,
                            IsActive = true,
                            AddedBy = userId,
                            DateAdded = now
                        });
                    }
                }

                foreach (var mapping in existingMappings.Where(x => !examCategoryIds.Contains(x.ExamCategoryId) && x.IsActive))
                {
                    mapping.IsActive = false;
                    mapping.LastUpdatedDate = now;
                    mapping.LastUpdatedBy = userId;
                }

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Llogaria e kandidatit u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> ToggleActive(int id)
        {
            try
            {
                var account = await _context.CandidateAccounts.FirstOrDefaultAsync(a => a.CandidateAccountId == id);
                if (account == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                account.IsActive = !account.IsActive;
                account.LastUpdatedDate = DateTime.UtcNow;
                account.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                await _context.SaveChangesAsync();

                return Ok(new Confirmation
                {
                    Status = "success",
                    ResponseMsg = account.IsActive ? "Llogaria e kandidatit u aktivizua." : "Llogaria e kandidatit u caktivizua."
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> SetPassword(int id, CandidateAccountSetPasswordRequest request)
        {
            try
            {
                var account = await _context.CandidateAccounts.FirstOrDefaultAsync(a => a.CandidateAccountId == id);
                if (account == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var salt = PasswordHasher.GenerateSalt();
                var hashedPassword = PasswordHasher.HashPassword(request.NewPassword, salt);
                account.Password = hashedPassword;
                account.PasswordSalt = salt;
                account.LastUpdatedDate = DateTime.UtcNow;
                account.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Fjalekalimi u rivendos me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetUnlinkedCandidates(string? search = null)
        {
            try
            {
                var query = _context.Candidates
                    .Where(c => !_context.CandidateAccounts.Any(a => a.CandidateId == c.CandidateId));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(c =>
                        ((c.FirstName ?? "") + " " + (c.LastName ?? "")).ToLower().Contains(s) ||
                        (c.PhoneNumber ?? "").ToLower().Contains(s));
                }

                var rawList = await query
                    .OrderBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .Select(c => new
                    {
                        c.CandidateId,
                        c.FirstName,
                        c.LastName,
                        phoneNumber = c.PhoneNumber
                    })
                    .ToListAsync();

                var list = rawList.Select(c => new
                {
                    c.CandidateId,
                    c.FirstName,
                    c.LastName,
                    fullName = string.Join(" ", new[] { c.FirstName, c.LastName }.Where(v => !string.IsNullOrWhiteSpace(v))),
                    c.phoneNumber
                });

                return Ok(new { data = list });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }
    }
}


