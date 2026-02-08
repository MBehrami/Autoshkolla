using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AdminApi.Models;
using AdminApi.Models.Helper;
using AdminApi.Models.User;
using AdminApi.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ISqlRepository<Users> _userRepo;
        private readonly ISqlRepository<UserRole> _userRoleRepo;
        private readonly ISqlRepository<LogHistory> _logHistoryRepo;


        public UsersController(IConfiguration config,
                                AppDbContext context, 
                                ISqlRepository<Users> userRepo,
                                ISqlRepository<UserRole> userRoleRepo,
                                ISqlRepository<LogHistory> logHistoryRepo)
        {
            _config=config;
            _context = context;
            _userRepo = userRepo;
            _userRoleRepo=userRoleRepo;
            _logHistoryRepo=logHistoryRepo;
        }


        ///<summary>
        ///Get Log in Detail
        ///</summary>
        [AllowAnonymous] 
        [HttpPost]      
        public async Task<ActionResult> GetLoginInfo(UserInfo credential)
        {
            try
            {
                var user=await (from u in _context.Users join r in _context.UserRole on u.UserRoleId 
                equals r.UserRoleId where u.IsActive.Equals(true) && u.Email.Equals(credential.Email)
                select new {u.UserId,r.UserRoleId,r.RoleName,u.FullName,u.Mobile,u.Email,u.ImagePath,u.Password,u.PasswordSalt}).FirstOrDefaultAsync();                                              
                if(user!=null)
                {
                    bool isPasswordValid=PasswordHasher.VerifyPassword(credential.Password,user.PasswordSalt,user.Password);
                    if(isPasswordValid)
                    {
                        UserInfo userInfo=new UserInfo{UserId=user.UserId,UserRoleId=user.UserRoleId,RoleName=user.RoleName,FullName=user.FullName,
                        Mobile=user.Mobile,Email=user.Email,ImagePath=user.ImagePath,Password=user.Password};
                        var tokenString=GenerateJwtToken(userInfo);
                        return Ok(new Response{token=tokenString,Obj=userInfo});
                    }
                    else
                    {
                        return Accepted(new Confirmation { Status = "incorrect", ResponseMsg = "Incorrect Password!" });
                    }                                                                         
                }  
               return Accepted(new Confirmation { Status = "incorrect", ResponseMsg = "Incorrect Email!" });               
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Get User Info for Forget password option
        ///</summary>
        [AllowAnonymous]
        [HttpGet("{email}")]      
        public async Task<ActionResult> GetUserInfoForForgetPassword(string email)
        {
            try
            {              
                var user=await _context.Users.SingleOrDefaultAsync(q=>q.Email==email);
                if(user!=null)
                {
                    user.ForgetPasswordRef=Guid.NewGuid().ToString();
                    await _context.SaveChangesAsync();
                    return Ok(user);
                }
                else
                {
                    return Accepted(new Confirmation{Status="error",ResponseMsg="There is no user for this email"});
                }
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation{Status="error",ResponseMsg=ex.Message});           
            }
        }

        ///<summary>
        ///Student Registration
        ///</summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> StudentRegistration(Users model)
        {
            try
            {
                
                var chkDuplicate=await _context.Users.SingleOrDefaultAsync(p=>p.Email==model.Email);
                if(chkDuplicate==null)
                {
                    string salt=PasswordHasher.GenerateSalt();
                    string hashedPassword=PasswordHasher.HashPassword(model.Password,salt);
                    model.Password=hashedPassword;
                    model.PasswordSalt=salt;
                    model.UserRoleId=2;
                    model.DateAdded=DateTime.Now;
                    model.IsActive=true;
                    await _userRepo.Insert(model);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Registered" });
                }
                else
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "This email already have a user" });
                }
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create Log History after login
        ///</summary>
        [AllowAnonymous]
        [HttpPost]       
        public async Task<ActionResult> CreateLoginHistory(LogHistory model)
        {
            try
            {  
                model.LogDate=DateTime.Now;    
                model.LogInTime=DateTime.Now;
                model.LogCode=Guid.NewGuid().ToString();      
                await _logHistoryRepo.Insert(model);
                return Ok(new Confirmation { Status = "success", ResponseMsg = model.LogCode });              
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update Login History
        ///</summary>
        [AllowAnonymous]
        [HttpPatch("{logCode}")]       
        public async Task<ActionResult> UpdateLoginHistory(string logCode)
        {
            try
            {
                var objLogHistory=await _context.LogHistory.SingleAsync(opt=>opt.LogCode==logCode);
                objLogHistory.LogOutTime=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Get date wise Login summary
        ///</summary>
        [AllowAnonymous]
        [HttpGet("{id}")]    
        public async Task<ActionResult> GetLogInSummaryByDate(int id)
        {
            try
            {
                var objUser=await _context.Users.Where(e=>e.UserId==id).FirstAsync();
                if(objUser.UserRoleId==1)
                {
                    var list=await _context.LogHistory.GroupBy(e=>e.LogInTime.Date.ToString()).OrderByDescending(e=>e.Key).Take(10)
                    .Select(e => new{ e.Key, Count = e.Count() }).ToListAsync();
                    var userList=list.Select(s=>new UserLog{Date=s.Key,Count=s.Count});
                    return Ok(userList);
                }   
                else
                {
                    var list=await _context.LogHistory.Where(e=>e.UserId==id).GroupBy(e=>e.LogInTime.Date.ToString()).OrderByDescending(e=>e.Key).Take(10)
                    .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();
                    var userList=list.Select(s=>new UserLog{Date=s.Key,Count=s.Count});
                    return Ok(userList);
                }                                                                      
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get month wise Login summary
        ///</summary>
        [AllowAnonymous] 
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLogInSummaryByMonth(int id)
        {
            try
            {              
                var objUser=await _context.Users.Where(e=>e.UserId==id).FirstAsync();
                if(objUser.UserRoleId==1)
                {
                    var list=await _context.LogHistory.GroupBy(e=>e.LogInTime.Month)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Month=s.Key,Count=s.Count});              
                    return Ok(userList);
                }
                else
                {
                    var list=await _context.LogHistory.Where(e=>e.UserId==id).GroupBy(e=>e.LogInTime.Month)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Month=s.Key,Count=s.Count});              
                    return Ok(userList);
                }              
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get month wise Login summary
        ///</summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLogInSummaryByYear(int id)
        {
            try
            {     
                var objUser=await _context.Users.Where(e=>e.UserId==id).FirstAsync();
                if(objUser.UserRoleId==1)
                {
                    var list=await _context.LogHistory.GroupBy(e=>e.LogInTime.Year)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Year=s.Key,Count=s.Count});             
                    return Ok(userList);
                }
                else
                {
                    var list=await _context.LogHistory.Where(e=>e.UserId==id).GroupBy(e=>e.LogInTime.Year)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Year=s.Key,Count=s.Count});             
                    return Ok(userList);
                }                     
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Browser Count
        ///</summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetBrowserCount(int id)
        {
            try
            {     
                var objUser=await _context.Users.Where(e=>e.UserId==id).FirstAsync();
                if(objUser.UserRoleId==1)
                {
                    var list=await _context.LogHistory.GroupBy(e=>e.Browser)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Browser=s.Key,Count=s.Count});             
                    return Ok(userList);
                }
                else
                {
                    var list=await _context.LogHistory.Where(e=>e.UserId==id).GroupBy(e=>e.Browser)
                        .Select(e => new { e.Key, Count = e.Count() }).ToListAsync();                       
                    var userList=list.Select(s=>new UserLog{Browser=s.Key,Count=s.Count});             
                    return Ok(userList);
                }                     
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Role wise User Count
        ///</summary>
        [AllowAnonymous]     
        [HttpGet] 
        public async Task<ActionResult> GetRoleWiseUser()
        {
            try
            {              
                var query=_context.Users.Join(_context.UserRole,
                        user=>user.UserRoleId,
                        role=>role.UserRoleId,
                        (user,role)=>new
                        {
                            UserId=user.UserId,
                            RoleName=role.RoleName
                        }).GroupBy(e=>e.RoleName)
                        .Select(e => new { e.Key, Count = e.Count() });
                var list=await query.ToListAsync();        
                var userList=list.Select(s=>new UserLog{RoleName=s.Key,Count=s.Count});             
                return Ok(userList);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Role List
        ///</summary>
        [Authorize(Roles="Admin,User")]     
        [HttpGet]        
        public async Task<ActionResult> GetUserRoleList()
        {
            try
            {
                var list=await (from r in _context.UserRole join m in _context.MenuGroup on 
                r.MenuGroupId equals m.MenuGroupID 
                select new {r.UserRoleId,r.RoleName,r.RoleDesc,m.MenuGroupName,m.MenuGroupID}).ToListAsync();

                var roleInfoList=list.Select(s=>new RoleInfo{UserRoleId=s.UserRoleId,RoleName=s.RoleName,RoleDesc=s.RoleDesc,MenuGroupName=s.MenuGroupName,MenuGroupID=s.MenuGroupID});

                int totalRecords=roleInfoList.Count();
                return Ok(new {data=roleInfoList, recordsTotal = totalRecords, recordsFiltered = totalRecords});
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single Role by ID
        ///</summary>
        [HttpGet("{id}")]       
        [Authorize(Roles="Admin,User")]
        public async Task<ActionResult> GetSingleRole(int id)
        {
            try
            {
                var singleRole=await _userRoleRepo.SelectById(id);
                return Ok(singleRole);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Delete Single Role by ID
        ///</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles="Admin,User")]
        public async Task<ActionResult> DeleteSingleRole(int id)
        {
            try
            {
                var checkList=await _context.Users.Where(opt=>opt.UserRoleId==id).ToListAsync();
                if(checkList.Count==0)
                {
                    await _userRoleRepo.Delete(id);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Deleted" });
                }
                else
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "This role has assinged user. Not allowed to delete." });
                }
                          
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create User Role
        ///</summary>
        [Authorize(Roles="Admin,User")]
        [HttpPost]       
        public async Task<ActionResult> CreateUserRole(UserRole model)
        {
            try
            {                  
                var objCheck=await _context.UserRole.SingleOrDefaultAsync(opt=>opt.RoleName==model.RoleName);
                if(objCheck==null)
                {
                    model.DateAdded=DateTime.Now;
                    model.IsActive=true;
                    await _userRoleRepo.Insert(model);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                  
                }
                else if(objCheck!=null)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Role name" });
                }
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Something Unexpected" });                      
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "unknown", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update User Role
        ///</summary>
        [Authorize(Roles="Admin,User")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateUserRole(UserRole model)
        {
            try
            {
                var objUserRole=await _context.UserRole.SingleAsync(opt=>opt.UserRoleId==model.UserRoleId);
                var objCheck=await _context.UserRole.SingleAsync(opt=>opt.RoleName==model.RoleName);

                if(objCheck!=null && objCheck.RoleName!=objUserRole.RoleName)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate role" });
                }
                else
                {
                    objUserRole.RoleName=model.RoleName;
                    objUserRole.RoleDesc=model.RoleDesc;
                    objUserRole.MenuGroupId=model.MenuGroupId;
                    objUserRole.LastUpdatedBy=model.LastUpdatedBy;
                    objUserRole.LastUpdatedDate=DateTime.Now;
                    await _context.SaveChangesAsync();
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });
                }                                             
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Get User List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="Admin,User")]
        public async Task<ActionResult> GetUserList()
        {
            try
            {
                var list=await (from u in _context.Users join r in _context.UserRole on 
                u.UserRoleId equals r.UserRoleId
                select new {u.UserId,u.UserRoleId,u.FullName,r.RoleName,u.Mobile,u.Email,u.DateOfBirth,
                u.Password,u.ImagePath}).ToListAsync();

                var userInfoList=list.Select(s=>new UserInfo
                {UserId=s.UserId,UserRoleId=s.UserRoleId,RoleName=s.RoleName,FullName=s.FullName,Mobile=s.Mobile,
                Email=s.Email,DateOfBirth=s.DateOfBirth,Password=s.Password,ImagePath=s.ImagePath});

                int totalRecords=userInfoList.Count();
                return Ok(new {data=userInfoList, recordsTotal = totalRecords, recordsFiltered = totalRecords});
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single User by ID
        ///</summary>
        [HttpGet("{id}")]
        [Authorize(Roles="Admin,User")]
        public async Task<ActionResult> GetSingleUser(int id)
        {
            try
            {
                var singleUser=await _userRepo.SelectById(id);
                return Ok(singleUser);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single User by hash
        ///</summary>
        [HttpGet("{hash}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetSingleUserByHash(string hash)
        {
            try
            {
                var singleUser=await _context.Users.SingleOrDefaultAsync(q=>q.ForgetPasswordRef==hash);
                return Ok(singleUser);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Delete Single User by ID
        ///</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles="Admin,User")]
        public async Task<ActionResult> DeleteSingleUser(int id)
        {
            try
            {
                await _userRepo.Delete(id);
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Deleted" });
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create User
        ///</summary>
        [Authorize(Roles="Admin,User")]
        [HttpPost]       
        public async Task<ActionResult> CreateUser(Users model)
        {
            var objCheck=await _context.Users.SingleOrDefaultAsync(opt=>opt.Email==model.Email);
            try
            {
                if(objCheck==null)
                {  
                    string salt=PasswordHasher.GenerateSalt();
                    string hashedPassword=PasswordHasher.HashPassword(model.Password,salt);
                    model.Password=hashedPassword;
                    model.PasswordSalt=salt;                 
                    model.DateAdded=DateTime.Now;
                    model.IsActive=true;
                    model.IsPasswordChange=false;
                    await _userRepo.Insert(model);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });
                }
                else if(objCheck!=null)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "This email already have a user" });
                }
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Something Unexpected" });          
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update User
        ///</summary>
        [Authorize(Roles="Admin,User")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateUser(Users model)
        {
            try
            {          
                var objUser=await _context.Users.SingleAsync(opt=>opt.UserId==model.UserId);
                objUser.UserRoleId=model.UserRoleId;
                objUser.FullName=model.FullName;
                objUser.Mobile=model.Mobile;
                objUser.Email=model.Email;
                objUser.DateOfBirth=model.DateOfBirth;
                objUser.ImagePath=model.ImagePath;
                objUser.LastUpdatedBy=model.LastUpdatedBy;
                objUser.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                                                     
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update User Profile
        ///</summary>
        [Authorize(Roles="Admin,User")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateUserProfile(UserInfo model)
        {
            try
            {
                var objUser=await _context.Users.SingleAsync(opt=>opt.UserId==model.UserId);               
                objUser.FullName=model.FullName;
                objUser.Mobile=model.Mobile;
                #pragma warning disable CS8601 // Possible null reference assignment.
                objUser.Email=model.Email;
                #pragma warning restore CS8601 // Possible null reference assignment.
                objUser.DateOfBirth=model.DateOfBirth;
                objUser.ImagePath=model.ImagePath;
                objUser.LastUpdatedBy=model.LastUpdatedBy;
                objUser.LastUpdatedDate=DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                         
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Change User Password
        ///</summary>
        [AllowAnonymous]
        [HttpPatch]       
        public async Task<ActionResult> ChangeUserPassword(UserInfo model)
        {
            try
            {
                string salt=PasswordHasher.GenerateSalt();
                string hashedPassword=PasswordHasher.HashPassword(model.Password,salt);
                var objUser=await _context.Users.SingleAsync(opt=>opt.UserId==model.UserId);              
                objUser.Password=hashedPassword;
                objUser.PasswordSalt=salt;
                objUser.LastPasswordChangeDate=DateTime.Now;
                objUser.PasswordChangedBy=model.UserId;
                objUser.IsPasswordChange=true;            
                await _context.SaveChangesAsync();
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                          
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Dashboard User Status. Allowed for Admin, User, and Instructor so Dashboard loads without 403.
        ///</summary>
        [Authorize(Roles="Admin,User,Instructor")]
        [HttpGet]
        public async Task<ActionResult> UserStatus()
        {
            try
            {                
                int totalUser=await _context.Users.CountAsync();    
                int activeUser=await _context.Users.Where(q=>q.IsActive==true).CountAsync();
                int inActiveUser=await _context.Users.Where(q=>q.IsActive==false).CountAsync();               
                int adminUser=await (from u in _context.Users join ur in _context.UserRole
                              on u.UserRoleId equals ur.UserRoleId where ur.RoleName=="Admin" 
                              select new {ur.RoleName}).CountAsync(); 

                UserStatus objStatus=new UserStatus{TotalUser=totalUser,ActiveUser=activeUser,InActiveUser=inActiveUser,AdminUser=adminUser};
                return Ok(objStatus);        
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Get Browsing Log
        ///</summary>
        [AllowAnonymous]     
        [HttpGet("{id}")]        
        public async Task<ActionResult> GetBrowseList(int id)
        {
            try
            {
                var objUser=await _context.Users.Where(e=>e.UserId==id).FirstAsync();

                var list=await (from l in _context.LogHistory join u in _context.Users on 
                    l.UserId equals u.UserId 
                    select new {u.UserId,u.FullName,u.Email,l.LogInTime,l.LogOutTime,l.Ip,l.Browser,l.BrowserVersion,l.Platform}).ToListAsync();

                if(objUser.UserRoleId!=1)
                {
                    list=list.Where(q=>q.UserId==id).ToList();
                }             

                var browseList=list.Select(s=>new Browse{FullName=s.FullName,Email=s.Email,LogInTime=s.LogInTime.ToString(),
                LogOutTime=s.LogOutTime.ToString(),Ip=s.Ip,Browser=s.Browser,
                BrowserVersion=s.BrowserVersion,Platform=s.Platform}).OrderByDescending(q=>q.LogInTime);

                int totalRecords=browseList.Count();
                return Ok(new {data=browseList, recordsTotal = totalRecords, recordsFiltered = totalRecords});
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Notification List
        ///</summary>    
        [AllowAnonymous]
        [HttpGet("{id}")]        
        public async Task<ActionResult> GetNotificationList(int id)
        {
            try
            {
                var list=await (from l in _context.LogHistory join u in _context.Users on 
                l.UserId equals u.UserId where l.LogDate>=DateTime.Now.AddDays(-3) && u.UserId==id
                select new {u.UserId,u.FullName,u.Email,l.LogInTime,l.LogOutTime,
                l.Ip,l.Browser,l.BrowserVersion,l.Platform}).ToListAsync();

                var browseList=list.Select(s=>new Browse{UserId=s.UserId,FullName=s.FullName,Email=s.Email,LogInTime=s.LogInTime.ToString(),
                LogOutTime=s.LogOutTime.ToString(),Ip=s.Ip,Browser=s.Browser,BrowserVersion=s.BrowserVersion,Platform=s.Platform}).OrderByDescending(q=>q.LogInTime);

                int totalRecords=browseList.Count();
                return Ok(new {data=browseList, recordsTotal = totalRecords, recordsFiltered = totalRecords});
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Profile picture upload
        ///</summary>
        [Authorize(Roles="Admin,User")]   
        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                if (file.Length > 0)
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
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });               
            }
        }

        string GenerateJwtToken(UserInfo userInfo)
        {
            #pragma warning disable CS8604 // Possible null reference argument.
            var securityKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            #pragma warning restore CS8604 // Possible null reference argument.

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserId.ToString()),
                new Claim("fullName", userInfo.FullName==null?"":userInfo.FullName.ToString()),
                new Claim("role",userInfo.RoleName==null?"":userInfo.RoleName),
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
