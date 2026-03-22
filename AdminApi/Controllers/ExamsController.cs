using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.ETestimi;
using AdminApi.Models.Helper;
using AdminApi.Services;
using AdminApi.ViewModels.ETestimi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ExamsController : ControllerBase
    {
        private const string CategoryAccessDeniedMessage = "Nuk keni akses në këtë kategori, kontaktoni administratorin.";
        private static readonly string[] AllowedImageMimeTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(AppDbContext context, IFileService fileService, ILogger<ExamsController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetCategoriesAdmin()
        {
            var data = await _context.ExamCategories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Code)
                .ToListAsync();
            return Ok(new { data });
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> UploadQuestionImage()
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Nuk u sigurua ndonjë fajll." });

                var (success, fileGuid, extension, errorMessage) = await _fileService.SaveFileAsync(file, AllowedImageMimeTypes, 5 * 1024 * 1024);
                
                if (!success)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = errorMessage });

                return Ok(new { status = "success", imageGuid = fileGuid, imageExtension = extension, responseMsg = "Fotoja u ngarkua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate ngarkimit te fotografise." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateCategory(ExamCategoryUpsertRequest request)
        {
            try
            {
                var code = request.Code.Trim().ToUpper();
                if (await _context.ExamCategories.AnyAsync(c => c.Code == code))
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Kodi i kategorise ekziston tashme." });

                var entity = new ExamCategory
                {
                    Code = code,
                    Title = request.Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    SortOrder = request.SortOrder,
                    IsActive = true,
                    AddedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1"),
                    DateAdded = DateTime.UtcNow
                };

                _context.ExamCategories.Add(entity);
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Kategoria u krijua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateCategory(int id, ExamCategoryUpsertRequest request)
        {
            try
            {
                var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.ExamCategoryId == id);
                if (category == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

                var code = request.Code.Trim().ToUpper();
                if (await _context.ExamCategories.AnyAsync(c => c.ExamCategoryId != id && c.Code == code))
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Kodi i kategorise ekziston tashme." });

                category.Code = code;
                category.Title = request.Title.Trim();
                category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
                category.SortOrder = request.SortOrder;
                category.LastUpdatedDate = DateTime.UtcNow;
                category.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Kategoria u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> ToggleCategory(int id)
        {
            try
            {
                var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.ExamCategoryId == id);
                if (category == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

                category.IsActive = !category.IsActive;
                category.LastUpdatedDate = DateTime.UtcNow;
                category.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Statusi i kategorise u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetExamsAdmin(int? categoryId = null)
        {
            var query = _context.Exams.AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(e => e.ExamCategoryId == categoryId.Value);

            var data = await query
                .OrderBy(e => e.ExamCategoryId)
                .ThenBy(e => e.SortOrder)
                .ThenBy(e => e.ExamId)
                .ToListAsync();
            return Ok(new { data });
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateExam(ExamUpsertRequest request)
        {
            try
            {
                if (!await _context.ExamCategories.AnyAsync(c => c.ExamCategoryId == request.ExamCategoryId))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

                var entity = new Exam
                {
                    ExamCategoryId = request.ExamCategoryId,
                    Title = request.Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    DurationMinutes = request.DurationMinutes <= 0 ? 45 : request.DurationMinutes,
                    PassPercent = request.PassPercent <= 0 ? 85 : request.PassPercent,
                    SortOrder = request.SortOrder,
                    IsActive = true,
                    AddedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1"),
                    DateAdded = DateTime.UtcNow
                };

                _context.Exams.Add(entity);
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Provimi u krijua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateExam(int id, ExamUpsertRequest request)
        {
            try
            {
                var entity = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == id);
                if (entity == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk u gjet." });

                if (!await _context.ExamCategories.AnyAsync(c => c.ExamCategoryId == request.ExamCategoryId))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

                entity.ExamCategoryId = request.ExamCategoryId;
                entity.Title = request.Title.Trim();
                entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
                entity.DurationMinutes = request.DurationMinutes <= 0 ? 45 : request.DurationMinutes;
                entity.PassPercent = request.PassPercent <= 0 ? 85 : request.PassPercent;
                entity.SortOrder = request.SortOrder;
                entity.LastUpdatedDate = DateTime.UtcNow;
                entity.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Provimi u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> ToggleExam(int id)
        {
            try
            {
                var entity = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == id);
                if (entity == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk u gjet." });

                entity.IsActive = !entity.IsActive;
                entity.LastUpdatedDate = DateTime.UtcNow;
                entity.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Statusi i provimit u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{examId}")]
        public async Task<ActionResult> GetQuestionsAdmin(int examId)
        {
            try
            {
                var questions = await _context.ExamQuestions
                    .Where(q => q.ExamId == examId)
                    .Include(q => q.QuestionOptions)
                    .OrderBy(q => q.SortOrder)
                    .ThenBy(q => q.ExamQuestionId)
                    .Select(q => new
                    {
                        examQuestionId = q.ExamQuestionId,
                        examId = q.ExamId,
                        text = q.Text,
                        imageGuid = q.ImageGuid,
                        sortOrder = q.SortOrder,
                        isActive = q.IsActive,
                        options = q.QuestionOptions
                            .OrderBy(o => o.SortOrder)
                            .ThenBy(o => o.ExamQuestionOptionId)
                            .Select(o => new
                            {
                                examQuestionOptionId = o.ExamQuestionOptionId,
                                text = o.OptionText,
                                isCorrect = o.IsCorrect,
                                sortOrder = o.SortOrder,
                                isActive = o.IsActive
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(new { data = questions });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate ngarkimit te pyetjeve." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateQuestion(ExamQuestionUpsertRequest request)
        {
            try
            {
                if (!await _context.Exams.AnyAsync(e => e.ExamId == request.ExamId))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk u gjet." });

                if (request.Options == null || request.Options.Count < 2)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kerkohen te pakten dy opsione." });

                if (request.Options.Count(o => o.IsCorrect) != 1)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Saktesisht nje opsion duhet te shenohet i sakte." });

                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                var question = new ExamQuestion
                {
                    ExamId = request.ExamId,
                    Text = request.Text.Trim(),
                    ImageGuid = string.IsNullOrWhiteSpace(request.ImageGuid) ? null : request.ImageGuid.Trim(),
                    SortOrder = request.SortOrder,
                    IsActive = true,
                    AddedBy = userId,
                    DateAdded = DateTime.UtcNow
                };

                _context.ExamQuestions.Add(question);
                await _context.SaveChangesAsync();

                foreach (var opt in request.Options)
                {
                    _context.ExamQuestionOptions.Add(new ExamQuestionOption
                    {
                        ExamQuestionId = question.ExamQuestionId,
                        OptionText = opt.OptionText.Trim(),
                        IsCorrect = opt.IsCorrect,
                        SortOrder = opt.SortOrder,
                        IsActive = true,
                        AddedBy = userId,
                        DateAdded = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Pyetja u krijua me sukses." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam question");
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{questionId}")]
        public async Task<ActionResult> UpdateQuestion(int questionId, ExamQuestionUpsertRequest request)
        {
            try
            {
                var question = await _context.ExamQuestions.FirstOrDefaultAsync(q => q.ExamQuestionId == questionId);
                if (question == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Pyetja nuk u gjet." });

                if (request.Options == null || request.Options.Count < 2)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kerkohen te pakten dy opsione." });

                if (request.Options.Count(o => o.IsCorrect) != 1)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Saktesisht nje opsion duhet te shenohet i sakte." });

                var userId = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                
                // Handle image: if new image is provided and different from old, delete old image
                if (!string.IsNullOrWhiteSpace(request.ImageGuid) && request.ImageGuid != question.ImageGuid)
                {
                    // Delete old image if it exists
                    if (!string.IsNullOrWhiteSpace(question.ImageGuid))
                    {
                        _fileService.DeleteFile(question.ImageGuid);
                    }
                    question.ImageGuid = request.ImageGuid.Trim();
                }
                // If no image provided in request, keep the existing one or set to null
                else if (string.IsNullOrWhiteSpace(request.ImageGuid) && !string.IsNullOrWhiteSpace(question.ImageGuid))
                {
                    // Client explicitly cleared the image
                    _fileService.DeleteFile(question.ImageGuid);
                    question.ImageGuid = null;
                }
                
                question.ExamId = request.ExamId;
                question.Text = request.Text.Trim();
                question.SortOrder = request.SortOrder;
                question.LastUpdatedDate = DateTime.UtcNow;
                question.LastUpdatedBy = userId;

                var oldOptions = _context.ExamQuestionOptions.Where(o => o.ExamQuestionId == questionId);
                _context.ExamQuestionOptions.RemoveRange(oldOptions);
                await _context.SaveChangesAsync();

                foreach (var opt in request.Options)
                {
                    _context.ExamQuestionOptions.Add(new ExamQuestionOption
                    {
                        ExamQuestionId = question.ExamQuestionId,
                        OptionText = opt.OptionText.Trim(),
                        IsCorrect = opt.IsCorrect,
                        SortOrder = opt.SortOrder,
                        IsActive = true,
                        AddedBy = userId,
                        DateAdded = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Pyetja u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("{questionId}")]
        public async Task<ActionResult> ToggleQuestion(int questionId)
        {
            try
            {
                var question = await _context.ExamQuestions.FirstOrDefaultAsync(q => q.ExamQuestionId == questionId);
                if (question == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Pyetja nuk u gjet." });

                question.IsActive = !question.IsActive;
                question.LastUpdatedDate = DateTime.UtcNow;
                question.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Statusi i pyetjes u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "Candidate,SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult> GetCategoriesForCandidate()
        {
            List<object> data;

            if (User.IsInRole("Candidate"))
            {
                var candidateAccount = await GetActiveCandidateAccountAsync();
                if (candidateAccount == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                data = await (from map in _context.CandidateAccountExamCategoryAccesses
                              join category in _context.ExamCategories on map.ExamCategoryId equals category.ExamCategoryId
                              where map.CandidateAccountId == candidateAccount.CandidateAccountId && map.IsActive && category.IsActive
                              orderby category.SortOrder, category.Code
                              select (object)new { id = category.Code, title = category.Title, description = category.Description })
                    .ToListAsync();
            }
            else
            {
                data = await _context.ExamCategories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Code)
                    .Select(c => (object)new { id = c.Code, title = c.Title, description = c.Description })
                    .ToListAsync();
            }

            return Ok(new { data });
        }

        [Authorize(Roles = "Candidate,SuperAdmin,Admin")]
        [HttpGet("{categoryCode}")]
        public async Task<ActionResult> GetExamsForCandidate(string categoryCode)
        {
            var code = categoryCode.Trim().ToUpper();
            var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
            if (category == null)
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

            if (User.IsInRole("Candidate"))
            {
                var candidateAccount = await GetActiveCandidateAccountAsync();
                if (candidateAccount == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var hasAccess = await HasCategoryAccessAsync(candidateAccount.CandidateAccountId, category.ExamCategoryId);
                if (!hasAccess)
                    return CategoryAccessDenied();
            }

            var data = await _context.Exams
                .Where(e => e.ExamCategoryId == category.ExamCategoryId && e.IsActive)
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.ExamId)
                .Select(e => new { id = e.ExamId, title = e.Title })
                .ToListAsync();

            return Ok(new { data });
        }

        [Authorize(Roles = "Candidate,SuperAdmin,Admin")]
        [HttpGet("{categoryCode}/{examId}")]
        public async Task<ActionResult> GetExamForCandidate(string categoryCode, int examId)
        {
            var code = categoryCode.Trim().ToUpper();
            var category = await _context.ExamCategories.FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
            if (category == null)
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

            if (User.IsInRole("Candidate"))
            {
                var candidateAccount = await GetActiveCandidateAccountAsync();
                if (candidateAccount == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var hasAccess = await HasCategoryAccessAsync(candidateAccount.CandidateAccountId, category.ExamCategoryId);
                if (!hasAccess)
                    return CategoryAccessDenied();
            }

            var exam = await _context.Exams
                .Where(e => e.ExamId == examId && e.ExamCategoryId == category.ExamCategoryId && e.IsActive)
                .Select(e => new
                {
                    e.ExamId,
                    e.Title,
                    e.DurationMinutes,
                    e.PassPercent,
                })
                .FirstOrDefaultAsync();

            if (exam == null)
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk u gjet." });

            var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
            var filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "files");

            string? BuildQuestionImageUrl(string? imageGuid, string? imagePath)
            {
                if (!string.IsNullOrWhiteSpace(imageGuid))
                {
                    if (Directory.Exists(filesDirectory))
                    {
                        var matchedFile = Directory.GetFiles(filesDirectory, $"{imageGuid}.*").FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(matchedFile))
                        {
                            var fileName = Path.GetFileName(matchedFile);
                            return $"{baseUrl}/files/{fileName}";
                        }
                    }

                    return $"{baseUrl}/files/{imageGuid}.jpg";
                }

                if (string.IsNullOrWhiteSpace(imagePath))
                    return null;

                if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return imagePath;

                if (imagePath.StartsWith('/'))
                    return $"{baseUrl}{imagePath}";

                return imagePath;
            }

            var questionsRaw = await _context.ExamQuestions
                .Where(q => q.ExamId == exam.ExamId && q.IsActive)
                .OrderBy(q => q.SortOrder)
                .ThenBy(q => q.ExamQuestionId)
                .Select(q => new
                {
                    q.ExamQuestionId,
                    q.Text,
                    q.ImageGuid,
                    q.ImagePath,
                    Options = _context.ExamQuestionOptions
                        .Where(o => o.ExamQuestionId == q.ExamQuestionId && o.IsActive)
                        .OrderBy(o => o.SortOrder)
                        .ThenBy(o => o.ExamQuestionOptionId)
                        .Select(o => new { o.OptionText, o.IsCorrect })
                        .ToList()
                })
                .ToListAsync();

            var questions = questionsRaw.Select(q =>
            {
                var correctIndex = q.Options.FindIndex(o => o.IsCorrect);

                return new
                {
                    id = q.ExamQuestionId,
                    text = q.Text,
                    image = BuildQuestionImageUrl(q.ImageGuid, q.ImagePath),
                    options = q.Options.Select(o => o.OptionText).ToList(),
                    correctOptionIndex = correctIndex >= 0 ? correctIndex : 0
                };
            }).ToList();

            var rules = new[]
            {
                "Lexoni me kujdes çdo pyetje para se të zgjidhni përgjigjen.",
                "Zgjidhni vetëm një përgjigje për secilën pyetje.",
                "Mos rifreskoni faqen gjatë një tentative aktive.",
                "Dorëzojeni testin në fund për të parë përgjigjet e sakta dhe të gabuara."
            };

            return Ok(new
            {
                data = new
                {
                    category = code,
                    examId = exam.ExamId,
                    title = exam.Title,
                    durationMinutes = exam.DurationMinutes,
                    passPercent = exam.PassPercent,
                    rules,
                    questions
                }
            });
        }

        [Authorize(Roles = "Candidate")]
        [HttpPost]
        public async Task<ActionResult> SubmitExam(SubmitExamRequest request)
        {
            try
            {
                var accountId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                if (accountId <= 0)
                    return Unauthorized();

                var candidateAccount = await _context.CandidateAccounts
                    .FirstOrDefaultAsync(a => a.CandidateAccountId == accountId && a.IsActive);

                if (candidateAccount == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria e kandidatit nuk u gjet." });

                var code = request.CategoryCode.Trim().ToUpper();
                var category = await _context.ExamCategories
                    .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

                if (category == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kategoria nuk u gjet." });

                var hasAccess = await HasCategoryAccessAsync(accountId, category.ExamCategoryId);
                if (!hasAccess)
                    return CategoryAccessDenied();

                var exam = await _context.Exams
                    .FirstOrDefaultAsync(e => e.ExamId == request.ExamId && e.ExamCategoryId == category.ExamCategoryId && e.IsActive);

                if (exam == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk u gjet." });

                var questions = await _context.ExamQuestions
                    .Where(q => q.ExamId == exam.ExamId && q.IsActive)
                    .OrderBy(q => q.SortOrder)
                    .ThenBy(q => q.ExamQuestionId)
                    .Select(q => new
                    {
                        q.ExamQuestionId,
                        CorrectIndex = _context.ExamQuestionOptions
                            .Where(o => o.ExamQuestionId == q.ExamQuestionId && o.IsActive)
                            .OrderBy(o => o.SortOrder)
                            .ThenBy(o => o.ExamQuestionOptionId)
                            .Select(o => o.IsCorrect)
                            .ToList()
                    })
                    .ToListAsync();

                if (questions.Count == 0)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Provimi nuk ka pyetje aktive." });

                var questionMap = questions.ToDictionary(
                    q => q.ExamQuestionId,
                    q => q.CorrectIndex.FindIndex(x => x));

                var normalizedAnswers = new Dictionary<int, int>();
                foreach (var item in request.Answers)
                {
                    normalizedAnswers[item.QuestionId] = item.SelectedOptionIndex;
                }

                if (normalizedAnswers.Keys.Any(k => !questionMap.ContainsKey(k)))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Pergjigjet e derguara nuk perputhen me pyetjet e provimit." });

                var correctAnswers = 0;
                foreach (var pair in questionMap)
                {
                    if (!normalizedAnswers.TryGetValue(pair.Key, out var selectedOption))
                        continue;

                    if (pair.Value >= 0 && selectedOption == pair.Value)
                        correctAnswers++;
                }

                var totalQuestions = questionMap.Count;
                var rawScore = totalQuestions == 0 ? 0m : (correctAnswers * 100m) / totalQuestions;
                var scorePercent = Math.Round(rawScore, 2);
                var isPassed = scorePercent >= exam.PassPercent;

                var submission = new ExamSubmission
                {
                    CandidateAccountId = accountId,
                    ExamId = exam.ExamId,
                    ExamCategoryId = category.ExamCategoryId,
                    ScorePercent = scorePercent,
                    IsPassed = isPassed,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    DurationSeconds = request.DurationSeconds < 0 ? 0 : request.DurationSeconds,
                    SubmittedAt = DateTime.UtcNow,
                    AddedBy = accountId,
                    DateAdded = DateTime.UtcNow,
                    IsMigrationData = false
                };

                _context.ExamSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    data = new
                    {
                        submissionId = submission.ExamSubmissionId,
                        scorePercent,
                        isPassed,
                        totalQuestions,
                        correctAnswers,
                        passPercentRequired = exam.PassPercent
                    }
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        private async Task<CandidateAccount?> GetActiveCandidateAccountAsync()
        {
            var accountId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (accountId <= 0)
                return null;

            return await _context.CandidateAccounts
                .FirstOrDefaultAsync(a => a.CandidateAccountId == accountId && a.IsActive);
        }

        private async Task<bool> HasCategoryAccessAsync(int candidateAccountId, int examCategoryId)
        {
            return await _context.CandidateAccountExamCategoryAccesses.AnyAsync(x =>
                x.CandidateAccountId == candidateAccountId &&
                x.ExamCategoryId == examCategoryId &&
                x.IsActive);
        }

        private ActionResult CategoryAccessDenied()
        {
            return StatusCode(403, new Confirmation
            {
                Status = "forbidden",
                ResponseMsg = CategoryAccessDeniedMessage
            });
        }

        [Authorize(Roles = "Candidate")]
        [HttpGet]
        public async Task<ActionResult> GetCandidateStats()
        {
            try
            {
                var accountId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                if (accountId <= 0)
                    return Unauthorized();

                var submissions = await _context.ExamSubmissions
                    .Where(s => s.CandidateAccountId == accountId)
                    .ToListAsync();

                var categories = await _context.ExamCategories
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Code)
                    .Select(c => new { c.ExamCategoryId, c.Code })
                    .ToListAsync();

                var totalAttempts = submissions.Count;
                var passedAttempts = submissions.Count(s => s.IsPassed);
                var averageScore = totalAttempts == 0 ? 0m : Math.Round(submissions.Average(s => s.ScorePercent), 2);
                var passRate = totalAttempts == 0 ? 0m : Math.Round((passedAttempts * 100m) / totalAttempts, 2);

                var categoryPerformance = categories.Select(c =>
                {
                    var categorySubmissions = submissions.Where(s => s.ExamCategoryId == c.ExamCategoryId).ToList();
                    var attempts = categorySubmissions.Count;
                    var categoryAverage = attempts == 0 ? 0m : Math.Round(categorySubmissions.Average(s => s.ScorePercent), 2);

                    return new CandidateCategoryStatsItem
                    {
                        CategoryCode = c.Code,
                        Attempts = attempts,
                        AverageScorePercent = categoryAverage
                    };
                }).ToList();

                var response = new CandidateDashboardStatsResponse
                {
                    TotalAttempts = totalAttempts,
                    PassedAttempts = passedAttempts,
                    PassRatePercent = passRate,
                    AverageScorePercent = averageScore,
                    CategoryPerformance = categoryPerformance
                };

                return Ok(new { data = response });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }
    }
}


