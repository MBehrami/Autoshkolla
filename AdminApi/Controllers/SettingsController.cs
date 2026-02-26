using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminApi.Models;
using AdminApi.Models.Others;
using AdminApi.Models.Helper;
using System.Net.Http.Headers;
using AdminApi.ViewModels.User;
using System.Threading.Tasks;
using AdminApi.Services;
using Microsoft.EntityFrameworkCore;

namespace QuizplusApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SettingsController:ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISqlRepository<ErrorLog> _errorLogRepo;
        private readonly IMailService _mailService;

        public SettingsController(AppDbContext context,
                                ISqlRepository<ErrorLog> errorLogRepo,
                                IMailService mailService
                                )
        {
            _context=context;
            _errorLogRepo=errorLogRepo;
            _mailService=mailService;
        }

        ///<summary>
        ///Sent Welcome Email
        ///</summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SendWelcomeMail(WelcomeRequest request)
        {
            try
            {
                await _mailService.SendWelcomeEmailAsync(request);
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Please check your Email"});
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Sent Password Email
        ///</summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SendPasswordMail(ForgetPassword request)
        {
            try
            {
                await _mailService.SendPasswordEmailAsync(request);
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Please check your Email"});
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }              
        }

        ///<summary>
        ///Get Site Settings
        ///</summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSiteSettings()
        {
            try
            {
                var siteSettings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();

                if (siteSettings == null)
                {
                    // Return a safe default so the frontend never crashes on null fields
                    return Ok(new
                    {
                        siteSettingsId = 0,
                        siteTitle = "",
                        welComeMessage = "",
                        copyRightText = "© 2026 Autoshkolla Linda",
                        logoPath = "",
                        faviconPath = "",
                        appBarColor = "#455a64",
                        headerColor = "#455a64",
                        footerColor = "#455a64",
                        bodyColor = "#f5f5f5",
                        allowWelcomeEmail = true,
                        allowFaq = true,
                        allowRightClick = true,
                        clientUrl = "",
                        defaultEmail = "",
                        displayName = "",
                        password = "",
                        host = "",
                        port = 0,
                        version = 1,
                        isActive = true,
                        isMigrationData = false
                    });
                }

                return Ok(siteSettings);
            }
            catch (Exception ex)
            {
                // TEMP DEBUG: return full exception so missing columns are visible in browser Network tab
                return StatusCode(500, new { status = "error", message = ex.Message, detail = ex.ToString() });
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult DbInfo()
        {
            var conn = _context.Database.GetDbConnection();
            return Ok(new
            {
                Database = conn.Database,
                DataSource = conn.DataSource
            });
        }

        ///<summary>
        ///Update General Settings
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateGeneralSetting(SiteSettings model)
        {
            try
            {
                var objSettings=await _context.SiteSettings.SingleAsync(opt=>opt.SiteSettingsId==model.SiteSettingsId);
                objSettings.SiteTitle=model.SiteTitle;
                objSettings.WelComeMessage=model.WelComeMessage;
                objSettings.CopyRightText=model.CopyRightText;
                objSettings.AllowWelcomeEmail=model.AllowWelcomeEmail;
                objSettings.AllowFaq=model.AllowFaq;
                objSettings.Version=model.Version;
                objSettings.LogoPath=model.LogoPath;
                objSettings.FaviconPath=model.FaviconPath;             
                objSettings.LastUpdatedBy=model.LastUpdatedBy;
                objSettings.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(objSettings);                                           
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }
        ///<summary>
        ///Update Email Settings
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateEmailSetting(SiteSettings model)
        {
            try
            {
                var objSettings=await _context.SiteSettings.SingleAsync(opt=>opt.SiteSettingsId==model.SiteSettingsId);
                objSettings.DefaultEmail=model.DefaultEmail;
                objSettings.Password=model.Password;
                objSettings.Host=model.Host;
                objSettings.Port=model.Port;
                objSettings.LastUpdatedBy=model.LastUpdatedBy;
                objSettings.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(objSettings);                                           
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update Color Settings
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateColorSetting(SiteSettings model)
        {
            try
            {
                var objSettings=await _context.SiteSettings.SingleAsync(opt=>opt.SiteSettingsId==model.SiteSettingsId);           
                objSettings.AppBarColor=model.AppBarColor;
                objSettings.HeaderColor=model.HeaderColor;
                objSettings.FooterColor=model.FooterColor;
                objSettings.BodyColor=model.BodyColor;
                objSettings.LastUpdatedBy=model.LastUpdatedBy;
                objSettings.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(objSettings);                                           
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update Landing Settings
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateLandingSetting(SiteSettings model)
        {
            try
            {
                var objSettings=await _context.SiteSettings.SingleAsync(opt=>opt.SiteSettingsId==model.SiteSettingsId);           
                objSettings.HomeHeader1=model.HomeHeader1;
                objSettings.HomeDetail1=model.HomeDetail1;
                objSettings.HomePicture=model.HomePicture;
                objSettings.HomeHeader2=model.HomeHeader2;
                objSettings.HomeDetail2=model.HomeDetail2;
                objSettings.HomeBox1Header=model.HomeBox1Header;
                objSettings.HomeBox1Detail=model.HomeBox1Detail;
                objSettings.HomeBox2Header=model.HomeBox2Header;
                objSettings.HomeBox2Detail=model.HomeBox2Detail;
                objSettings.HomeBox3Header=model.HomeBox3Header;
                objSettings.HomeBox3Detail=model.HomeBox3Detail;
                objSettings.HomeBox4Header=model.HomeBox4Header;
                objSettings.HomeBox4Detail=model.HomeBox4Detail;
                objSettings.Feature1Header=model.Feature1Header;
                objSettings.Feature1Detail=model.Feature1Detail;
                objSettings.Feature1Picture=model.Feature1Picture;
                objSettings.Feature2Header=model.Feature2Header;
                objSettings.Feature2Detail=model.Feature2Detail;
                objSettings.Feature2Picture=model.Feature2Picture;
                objSettings.Feature3Header=model.Feature3Header;
                objSettings.Feature3Detail=model.Feature3Detail;
                objSettings.Feature3Picture=model.Feature3Picture;
                objSettings.Feature4Header=model.Feature4Header;
                objSettings.Feature4Detail=model.Feature4Detail;
                objSettings.Feature4Picture=model.Feature4Picture;
                objSettings.RegistrationText=model.RegistrationText;
                objSettings.ContactUsText=model.ContactUsText;
                objSettings.ContactUsTelephone=model.ContactUsTelephone;
                objSettings.ContactUsEmail=model.ContactUsEmail;
                objSettings.FooterText=model.FooterText;
                objSettings.FooterFacebook=model.FooterFacebook;
                objSettings.FooterTwitter=model.FooterTwitter;
                objSettings.FooterLinkedin=model.FooterLinkedin;
                objSettings.FooterInstagram=model.FooterInstagram;
                objSettings.LastUpdatedBy=model.LastUpdatedBy;
                objSettings.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(objSettings);                                           
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update Email Text Settings
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateEmailTextSetting(SiteSettings model)
        {
            try
            {
                var objSettings=await _context.SiteSettings.SingleAsync(opt=>opt.SiteSettingsId==model.SiteSettingsId);           
                objSettings.ForgetPasswordEmailSubject=model.ForgetPasswordEmailSubject;
                objSettings.ForgetPasswordEmailHeader=model.ForgetPasswordEmailHeader;
                objSettings.ForgetPasswordEmailBody=model.ForgetPasswordEmailBody;
                objSettings.WelcomeEmailSubject=model.WelcomeEmailSubject;
                objSettings.WelcomeEmailHeader=model.WelcomeEmailHeader;
                objSettings.WelcomeEmailBody=model.WelcomeEmailBody;              
                objSettings.LastUpdatedBy=model.LastUpdatedBy;
                objSettings.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(objSettings);                                           
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }
        ///<summary>
        ///Site Logo upload
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]   
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadLogo()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Logo");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0 && file.ContentType.StartsWith("image/"))
                {
                    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var fileName = Guid.NewGuid().ToString()+"_"+ ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    #pragma warning restore CS8602 // Dereference of a possibly null reference.

                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok(new { dbPath });                   
                }
                else
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Not an image" }); 
                }
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });               
            }
        }

        ///<summary>
        ///Site Favicon upload
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin")]   
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFavicon()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Favicon");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0 && file.ContentType.StartsWith("image/"))
                {
                    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var fileName = Guid.NewGuid().ToString()+"_"+ ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    #pragma warning restore CS8602 // Dereference of a possibly null reference.

                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok(new { dbPath });                   
                }
                else
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "Not an image" }); 
                }
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });               
            }
        }
        ///<summary>
        ///Get Error Log List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="SuperAdmin,Admin")]
        public async Task<ActionResult> GetErrorLogList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var query = _context.ErrorLogs.AsNoTracking().OrderByDescending(e => e.DateAdded);
                var totalRecords = await query.CountAsync();
                var list = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return Ok(new { data = list, recordsTotal = totalRecords, recordsFiltered = totalRecords, page, pageSize });
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create Error Log
        ///</summary>
        [AllowAnonymous]
        [HttpPost]       
        public async Task<ActionResult> CreateErrorLog([FromBody] ErrorLog model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { status = "validation_error", errors = ModelState });

            try
            {  
                model.DateAdded=DateTime.Now;
                await _errorLogRepo.Insert(model); 
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                                   
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }
    }
}
