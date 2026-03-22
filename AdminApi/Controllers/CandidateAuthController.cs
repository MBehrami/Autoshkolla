using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.ETestimi;
using AdminApi.Models.Helper;
using AdminApi.ViewModels.ETestimi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CandidateAuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public CandidateAuthController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(CandidateLoginRequest request)
        {
            try
            {
                var phone = request.PhoneNumber.Trim();
                var account = await _context.CandidateAccounts
                    .FirstOrDefaultAsync(a => a.PhoneNumber == phone);

                if (account == null)
                    return Accepted(new Confirmation { Status = "incorrect", ResponseMsg = "Numri i telefonit ose fjalëkalimi është i pasaktë." });

                if (!account.IsActive)
                    return Accepted(new Confirmation { Status = "inactive", ResponseMsg = "Llogaria eshte e caktivizuar." });

                if (account.ValidTo.Date < DateTime.UtcNow.Date)
                    return Accepted(new Confirmation { Status = "expired", ResponseMsg = "Vlefshmeria e llogarise ka skaduar." });

                var isPasswordValid = PasswordHasher.VerifyPassword(request.Password, account.PasswordSalt ?? string.Empty, account.Password);
                if (!isPasswordValid)
                    return Accepted(new Confirmation { Status = "incorrect", ResponseMsg = "Numri i telefonit ose fjalëkalimi është i pasaktë." });

                var candidate = account.CandidateId.HasValue
                    ? await _context.Candidates.Where(c => c.CandidateId == account.CandidateId.Value)
                        .Select(c => new
                        {
                            c.CandidateId,
                            c.FirstName,
                            c.LastName,
                            c.CategoryId
                        })
                        .FirstOrDefaultAsync()
                    : null;

                if (account.CandidateId.HasValue && candidate == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kandidati i lidhur nuk u gjet." });

                var fullName = string.Join(" ", new[] { candidate?.FirstName ?? account.FirstName, candidate?.LastName ?? account.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

                var profile = new
                {
                    candidateAccountId = account.CandidateAccountId,
                    candidateId = candidate?.CandidateId,
                    fullName,
                    phoneNumber = account.PhoneNumber,
                    email = account.Email,
                    validTo = account.ValidTo,
                    categoryId = candidate?.CategoryId,
                    roleName = "Candidate"
                };

                var tokenString = GenerateJwtToken(account.CandidateAccountId, profile.fullName);

                account.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new Response { token = tokenString, Obj = profile });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "Candidate")]
        [HttpGet]
        public async Task<ActionResult> Me()
        {
            try
            {
                var accountId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                if (accountId <= 0)
                    return Unauthorized();

                var account = await _context.CandidateAccounts.FirstOrDefaultAsync(a => a.CandidateAccountId == accountId);
                if (account == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria nuk u gjet." });

                var candidate = account.CandidateId.HasValue
                    ? await _context.Candidates.Where(c => c.CandidateId == account.CandidateId.Value)
                        .Select(c => new { c.CandidateId, c.FirstName, c.LastName, c.CategoryId })
                        .FirstOrDefaultAsync()
                    : null;

                if (account.CandidateId.HasValue && candidate == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Kandidati i lidhur nuk u gjet." });

                var fullName = string.Join(" ", new[] { candidate?.FirstName ?? account.FirstName, candidate?.LastName ?? account.LastName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
                return Ok(new
                {
                    candidateAccountId = account.CandidateAccountId,
                    candidateId = candidate?.CandidateId,
                    fullName,
                    phoneNumber = account.PhoneNumber,
                    email = account.Email,
                    validTo = account.ValidTo,
                    categoryId = candidate?.CategoryId,
                    isActive = account.IsActive
                });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        [Authorize(Roles = "Candidate")]
        [HttpPatch]
        public async Task<ActionResult> ChangePassword(CandidateChangePasswordRequest request)
        {
            try
            {
                var accountId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var account = await _context.CandidateAccounts.FirstOrDefaultAsync(a => a.CandidateAccountId == accountId);
                if (account == null)
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Llogaria nuk u gjet." });

                var isPasswordValid = PasswordHasher.VerifyPassword(request.CurrentPassword, account.PasswordSalt ?? string.Empty, account.Password);
                if (!isPasswordValid)
                    return Accepted(new Confirmation { Status = "incorrect", ResponseMsg = "Fjalekalimi aktual eshte i pasakte." });

                var salt = PasswordHasher.GenerateSalt();
                var hashedPassword = PasswordHasher.HashPassword(request.NewPassword, salt);
                account.Password = hashedPassword;
                account.PasswordSalt = salt;
                account.LastUpdatedDate = DateTime.UtcNow;
                account.LastUpdatedBy = accountId;

                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Fjalekalimi u perditesua me sukses." });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Ndodhi nje gabim gjate perpunimit te kerkeses." });
            }
        }

        private string GenerateJwtToken(int candidateAccountId, string fullName)
        {
#pragma warning disable CS8604
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
#pragma warning restore CS8604

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, candidateAccountId.ToString()),
                new Claim("fullName", fullName ?? string.Empty),
                new Claim("role", "Candidate"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(180),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


