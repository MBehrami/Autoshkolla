using System;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Models.User;
using AdminApi.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class InstructorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<Users> _userRepo;

        public InstructorsController(AppDbContext context, ISqlRepository<Users> userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        /// <summary>Create instructor and auto-create user account (email=login, initial password, role=Instructor).</summary>
        [HttpPost]
        public async Task<ActionResult> CreateInstructor(InstructorCreateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Email is required." });
                if (string.IsNullOrWhiteSpace(request.InitialPassword))
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Initial password is required." });

                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.Trim());
                if (existing != null)
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "This email already has a user." });

                var instructorRole = await _context.UserRole.FirstOrDefaultAsync(r => r.RoleName == "Instructor");
                if (instructorRole == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Instructor role not found." });

                var addedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                var fullName = string.Join(" ", new[] { request.FirstName, request.ParentName, request.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
                if (string.IsNullOrEmpty(fullName)) fullName = request.Email;

                var salt = PasswordHasher.GenerateSalt();
                var hashedPassword = PasswordHasher.HashPassword(request.InitialPassword!, salt);

                var user = new Users
                {
                    UserRoleId = instructorRole.UserRoleId,
                    FullName = fullName,
                    Email = request.Email.Trim(),
                    Password = hashedPassword,
                    PasswordSalt = salt,
                    Mobile = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    IsActive = true,
                    AddedBy = addedBy,
                    DateAdded = DateTime.UtcNow,
                    IsPasswordChange = false
                };
                await _userRepo.Insert(user);

                var profile = new InstructorProfile
                {
                    UserId = user.UserId,
                    FirstName = request.FirstName,
                    ParentName = request.ParentName,
                    LastName = request.LastName,
                    PersonalNumber = request.PersonalNumber,
                    ScheduleType = request.ScheduleType,
                    LicenseNumber = request.LicenseNumber,
                    LicenseValidityDate = request.LicenseValidityDate,
                    LicensePhotoPath = request.LicensePhotoPath,
                    DateAdded = DateTime.UtcNow
                };
                _context.InstructorProfiles.Add(profile);
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>List instructors. FullName = FirstName + LastName only. Search by First Name, Last Name.</summary>
        [HttpGet]
        public async Task<ActionResult> GetInstructorsList(string? search = null)
        {
            try
            {
                var query = from u in _context.Users
                           join r in _context.UserRole on u.UserRoleId equals r.UserRoleId
                           join p in _context.InstructorProfiles on u.UserId equals p.UserId into profileGroup
                           from p in profileGroup.DefaultIfEmpty()
                           where r.RoleName == "Instructor"
                           select new
                           {
                               u.UserId,
                               FirstName = p != null ? p.FirstName : null,
                               LastName = p != null ? p.LastName : null,
                               u.Mobile,
                               ScheduleType = p != null ? p.ScheduleType : null,
                               LicenseNumber = p != null ? p.LicenseNumber : null,
                               LicenseValidityDate = p != null ? p.LicenseValidityDate : null,
                               u.IsActive
                           };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(x =>
                        (x.FirstName != null && x.FirstName.ToLower().Contains(s)) ||
                        (x.LastName != null && x.LastName.ToLower().Contains(s)) ||
                        (x.Mobile != null && x.Mobile.Contains(search)));
                }

                var raw = await query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToListAsync();
                var list = raw.Select(x =>
                {
                    var fullName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
                    return new InstructorListItem
                    {
                        UserId = x.UserId,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        FullName = string.IsNullOrEmpty(fullName) ? null : fullName,
                        PhoneNumber = x.Mobile,
                        ScheduleType = x.ScheduleType,
                        LicenseNumber = x.LicenseNumber,
                        LicenseValidityDate = x.LicenseValidityDate,
                        IsActive = x.IsActive
                    };
                }).ToList();
                return Ok(new { data = list, recordsTotal = list.Count, recordsFiltered = list.Count });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>Get single instructor by UserId for view/edit.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetInstructorDetails(int id)
        {
            try
            {
                var u = await _context.Users
                    .Where(x => x.UserId == id)
                    .Select(x => new { x.UserId, x.FullName, x.Email, x.Mobile, x.DateOfBirth, x.IsActive, x.UserRoleId })
                    .FirstOrDefaultAsync();
                if (u == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Instructor not found." });

                var r = await _context.UserRole.Where(x => x.UserRoleId == u.UserRoleId).Select(x => x.RoleName).FirstOrDefaultAsync();
                if (r != "Instructor")
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "User is not an instructor." });

                var p = await _context.InstructorProfiles.FirstOrDefaultAsync(x => x.UserId == id);
                return Ok(new
                {
                    userId = u.UserId,
                    firstName = p?.FirstName ?? (u.FullName ?? ""),
                    parentName = p?.ParentName,
                    lastName = p?.LastName,
                    email = u.Email,
                    phoneNumber = u.Mobile,
                    dateOfBirth = u.DateOfBirth,
                    isActive = u.IsActive,
                    personalNumber = p?.PersonalNumber,
                    scheduleType = p?.ScheduleType,
                    licenseNumber = p?.LicenseNumber,
                    licenseValidityDate = p?.LicenseValidityDate,
                    licensePhotoPath = p?.LicensePhotoPath
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>Update instructor and profile.</summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateInstructor(int id, InstructorUpdateRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
                if (user == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Instructor not found." });

                var role = await _context.UserRole.Where(x => x.UserRoleId == user.UserRoleId).Select(x => x.RoleName).FirstOrDefaultAsync();
                if (role != "Instructor")
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "User is not an instructor." });

                var fullName = string.Join(" ", new[] { request.FirstName, request.ParentName, request.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
                if (!string.IsNullOrEmpty(fullName)) user.FullName = fullName;
                user.Mobile = request.PhoneNumber;
                user.Email = request.Email ?? user.Email;
                user.DateOfBirth = request.DateOfBirth;
                user.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                user.LastUpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var profile = await _context.InstructorProfiles.FirstOrDefaultAsync(x => x.UserId == id);
                if (profile != null)
                {
                    profile.FirstName = request.FirstName;
                    profile.ParentName = request.ParentName;
                    profile.LastName = request.LastName;
                    profile.PersonalNumber = request.PersonalNumber;
                    profile.ScheduleType = request.ScheduleType;
                    profile.LicenseNumber = request.LicenseNumber;
                    profile.LicenseValidityDate = request.LicenseValidityDate;
                    profile.LicensePhotoPath = request.LicensePhotoPath;
                    profile.LastUpdatedBy = user.LastUpdatedBy;
                    profile.LastUpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.InstructorProfiles.Add(new InstructorProfile
                    {
                        UserId = id,
                        FirstName = request.FirstName,
                        ParentName = request.ParentName,
                        LastName = request.LastName,
                        PersonalNumber = request.PersonalNumber,
                        ScheduleType = request.ScheduleType,
                        LicenseNumber = request.LicenseNumber,
                        LicenseValidityDate = request.LicenseValidityDate,
                        LicensePhotoPath = request.LicensePhotoPath,
                        DateAdded = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();

                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Updated" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        /// <summary>Toggle instructor Active/Inactive. Inactive instructors cannot log in.</summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult> ToggleInstructorActive(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
                if (user == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Instructor not found." });

                var role = await _context.UserRole.Where(x => x.UserRoleId == user.UserRoleId).Select(x => x.RoleName).FirstOrDefaultAsync();
                if (role != "Instructor")
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "User is not an instructor." });

                user.IsActive = !user.IsActive;
                user.LastUpdatedBy = int.Parse(User.FindFirst("sub")?.Value ?? "1");
                user.LastUpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { isActive = user.IsActive, status = "success", responseMsg = "Status updated." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }
    }
}
