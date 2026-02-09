using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using AdminApi.Models;
using AdminApi.Models.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AdminApi.Models.Helper;
using AdminApi.ViewModels.Menu;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MenuController:ControllerBase
    {     
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ISqlRepository<AppMenu> _menuRepo;
        private readonly ISqlRepository<MenuGroup> _menuGroupRepo;
        private readonly ISqlRepository<MenuGroupWiseMenuMapping> _menuGroupWiseMenuMappingRepo;

        public MenuController(AppDbContext context, 
                            IConfiguration config,
                            ISqlRepository<AppMenu> menuRepo,
                            ISqlRepository<MenuGroup> menuGroupRepo,
                            ISqlRepository<MenuGroupWiseMenuMapping> menuGroupWiseMenuMappingRepo)
        {         
            _context = context;
            _config=config;
            _menuRepo = menuRepo;
            _menuGroupRepo = menuGroupRepo;
            _menuGroupWiseMenuMappingRepo=menuGroupWiseMenuMappingRepo;
        }

        ///<summary>
        ///App Side bar menus. Instructor role gets only Candidates (MenuGroupId=3).
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin,User,Instructor")]
        [HttpGet("{roleId}")]
        public async Task<ActionResult> GetSidebarMenu(int roleId)
        {
            var roleRow = await _context.UserRole.Where(ur => ur.UserRoleId == roleId).Select(ur => new { ur.MenuGroupId }).FirstOrDefaultAsync();
            if (roleRow == null || !roleRow.MenuGroupId.HasValue)
                return Ok(new List<SidebarItems>());

            int menuGroupId = roleRow.MenuGroupId.Value;

            var parentMenus=await (from m in _context.Menu 
            where m.ParentID.Equals(0)
            orderby m.SortOrder
            select new {m.MenuID,m.ParentID,m.MenuTitle,m.URL,m.SortOrder,m.IconClass,m.IsActive}).ToListAsync();

            List<AppMenu> parentMenuList=parentMenus.Select(x=>new AppMenu(){
                MenuID=x.MenuID,
                ParentID=x.ParentID,
                MenuTitle=x.MenuTitle,
                URL=x.URL,
                SortOrder=x.SortOrder,
                IconClass=x.IconClass,
                IsActive=x.IsActive
            }).ToList();

            List<SidebarItems> sidebarInitial=new List<SidebarItems>();

            foreach(AppMenu m in parentMenuList)
            {
                var childMenus=await (from mm in _context.Menu
                where mm.ParentID.Equals(m.MenuID)
                where _context.MenuGroupWiseMenuMapping.Any(mwm => mwm.MenuGroupId == menuGroupId && mwm.MenuId == mm.MenuID)
                select new {mm.MenuID,mm.MenuTitle,mm.URL}).ToListAsync();

                if(childMenus.Count>0)
                {
                    sidebarInitial.Add(new SidebarItems{Id=m.MenuID,Title=m.MenuTitle,Icon=m.IconClass,Route=m.URL,Order=m.SortOrder,IsActive=m.IsActive,
                    ChildItems=childMenus.Select(x=>new ChildItem(){Id=x.MenuID,Title=x.MenuTitle,Route=x.URL}).ToList()});
                }
                else
                {
                    var checkMenu=await _context.MenuGroupWiseMenuMapping
                        .Where(mwm => mwm.MenuGroupId == menuGroupId && mwm.MenuId.Equals(m.MenuID))
                        .Select(mwm => new { mwm.MenuId }).FirstOrDefaultAsync();
                    if(checkMenu!=null)
                    {
                        sidebarInitial.Add(new SidebarItems{Id=m.MenuID,Title=m.MenuTitle,Icon=m.IconClass,Route=m.URL,Order=m.SortOrder,IsActive=m.IsActive,
                        ChildItems=[]});
                    }
                }               
            }
            return Ok(sidebarInitial);
        }

        ///<summary>
        ///Filter menus by menuGroupId
        ///</summary>      
        [Authorize(Roles="SuperAdmin,Admin,User")]
        [HttpGet("{menuGroupId}")]
        public async Task<ActionResult> GetAllMenu(int menuGroupId)
        {
            var parentMenus=await (from m in _context.Menu 
            where m.ParentID.Equals(0) orderby m.SortOrder
            select new {m.MenuID,m.ParentID,m.MenuTitle,m.URL,m.SortOrder,m.IconClass}).ToListAsync();

            List<AppMenu> parentMenuList=parentMenus.Select(x=>new AppMenu(){
                MenuID=x.MenuID,
                ParentID=x.ParentID,
                MenuTitle=x.MenuTitle
            }).ToList();

            List<MenuItems> sidebarInitial=new List<MenuItems>();

            foreach(AppMenu m in parentMenuList)
            {
                var childMenus=await (from mm in _context.Menu where mm.ParentID.Equals(m.MenuID)
                select new {mm.MenuID,mm.MenuTitle,
                IsSelected=mm.MenuID.Equals((from mwm in _context.MenuGroupWiseMenuMapping 
                                    where mwm.MenuGroupId.Equals(menuGroupId) && mwm.MenuId.Equals(mm.MenuID) 
                                    select new {mwm.MenuId}).First().MenuId)?true:false }).ToListAsync();

                if(childMenus.Count>0)
                {
                    sidebarInitial.Add(new MenuItems{Id=m.MenuID,Title=m.MenuTitle,groupId=menuGroupId,IsParentSelected=true,
                    Children=childMenus.Select(x=>new Child(){Id=x.MenuID,groupId=menuGroupId,Title=x.MenuTitle,IsSelected=x.IsSelected}).ToList()});
                }               
                else
                {
                    var checkMenu=(from mwm in _context.MenuGroupWiseMenuMapping 
                        where mwm.MenuGroupId.Equals(menuGroupId) && mwm.MenuId.Equals(m.MenuID) 
                        select new {mwm.MenuId}).FirstOrDefault();
                    sidebarInitial.Add(new MenuItems{Id=m.MenuID,Title=m.MenuTitle,groupId=menuGroupId,IsParentSelected=m.MenuID.Equals(checkMenu!=null?checkMenu.MenuId:null)?true:false,
                    Children=[]});
                }               
            }

            return Ok(sidebarInitial);
        }

        ///<summary>
        ///Assign App Menu
        ///</summary>
        [HttpPost]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> MenuAssign(MenuOperation model)
        {
            try
            {              
                var objCheck=await _context.MenuGroupWiseMenuMapping.SingleOrDefaultAsync(opt=>opt.MenuGroupId==model.MenuGroupId && opt.MenuId==model.MenuId);             
                if(model.IsSelected==true)
                {                  
                    if(objCheck==null)
                    {
                        var objAssign=new MenuGroupWiseMenuMapping{MenuGroupId=model.MenuGroupId,
                        MenuId=model.MenuId,IsActive=true,DateAdded=DateTime.Now,AddedBy=model.AddedBy};

                        var obj=await _menuGroupWiseMenuMappingRepo.Insert(objAssign);
                        return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Assigned" });                          
                    }
                }
                else if(model.IsSelected==false)
                {                                     
                    if(objCheck!=null)
                    {
                        var obj=await _menuGroupWiseMenuMappingRepo.Delete(objCheck.MenuGroupWiseMenuMappingId); 
                        return Ok(new Confirmation { Status = "delete", ResponseMsg = "Successfully UnAssigned" });                                             
                    }
                }
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Something Unexpected" });
               
            }
            catch(Exception ex)
            {
                return Accepted(new Confirmation { Status = "unknown", ResponseMsg = ex.Message });  
            }
            
        }

        ///<summary>
        ///Get App Menu List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetMenuList()
        {
            try
            {              
                var menuList=await (from m in _context.Menu orderby m.ParentID,m.SortOrder 
                select new{m.MenuID,m.MenuTitle,m.URL,m.IsSubMenu,m.SortOrder,m.IconClass,m.ParentID,
                ParentMenuName=(from mm in _context.Menu where mm.MenuID.Equals(m.ParentID) select mm.MenuTitle).FirstOrDefault()}).ToListAsync();

                int totalRecords=menuList.Count();
                return Ok(new {data=menuList, recordsTotal = totalRecords, recordsFiltered = totalRecords});           
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get App Parent Menu List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetParentMenuList()
        {
            try
            {
                var rootMenu=new List<DropDownHelper>{
                    new DropDownHelper{id="0",text="Root"}
                };
                var parentMenus=await (from m in _context.Menu where m.ParentID==0 && m.IsSubMenu==1
                select new {m.MenuID,m.MenuTitle}).ToListAsync();

                List<DropDownHelper> parentMenuList=parentMenus.Select(q=>new DropDownHelper{id=q.MenuID.ToString(),text=q.MenuTitle}).ToList();

                var returnList=parentMenuList.Union(rootMenu).ToList().OrderBy(q=>q.id);

                return Ok(returnList);           
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single Menu by ID
        ///</summary>
        [HttpGet("{id}")]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetSingleMenu(int id)
        {
            try
            {
                var singleMenu=await _menuRepo.SelectById(id);
                return Ok(singleMenu);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Delete Single Menu by ID
        ///</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> DeleteSingleMenu(int id)
        {
            try
            {      
                await _menuRepo.Delete(id);
                return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Deleted" });                                             
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create App Menu
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin,User")]
        [HttpPost]       
        public async Task<ActionResult> CreateMenu(AppMenu model)
        {
            try
            {
                var objCheck=await _context.Menu.Where(opt=>opt.MenuTitle==model.MenuTitle||
                (opt.SortOrder==model.SortOrder&&model.ParentID==0&&opt.SortOrder>0)||
                (model.ParentID==0&&model.SortOrder<=0)).FirstOrDefaultAsync();

                if(objCheck==null)
                {                  
                    model.DateAdded=DateTime.Now;
                    var obj=await _menuRepo.Insert(model);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });                                     
                }
                else if(objCheck.MenuTitle==model.MenuTitle)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Menu Title" });
                }
                else if(objCheck.SortOrder==model.SortOrder)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Order No." });
                }
                else if(model.SortOrder<=0)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Order No. can't be lest than or equal to 0" });
                }
                
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Something Unexpected" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update App Menu
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin,User")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateMenu(AppMenu model)
        {
            try
            { 
                var objMenu=await _context.Menu.SingleAsync(opt=>opt.MenuID==model.MenuID);
                var objCheck=await _context.Menu.Where(opt=>opt.MenuTitle==model.MenuTitle).FirstOrDefaultAsync();
                var objSortOrderCheck=await _context.Menu.SingleOrDefaultAsync(opt=>opt.SortOrder==model.SortOrder&&model.ParentID==0&&opt.SortOrder>0);

                if(objCheck!=null && objCheck.MenuTitle!=objMenu.MenuTitle)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Menu Title" });
                }
                else if(model.ParentID==0 && model.SortOrder<=0)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Order No. must greater than 0" });
                }
                else if(objSortOrderCheck!=null && objSortOrderCheck.SortOrder!=objMenu.SortOrder)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Order No." });
                }                   
                else
                {                        
                    objMenu.ParentID=model.ParentID; 
                    objMenu.MenuTitle=model.MenuTitle;
                    objMenu.URL=model.URL;
                    objMenu.IsSubMenu=model.IsSubMenu;
                    objMenu.SortOrder=model.SortOrder;
                    objMenu.IconClass=model.IconClass; 
                    objMenu.IsActive=model.IsActive; 
                    objMenu.LastUpdatedBy=model.LastUpdatedBy;
                    objMenu.LastUpdatedDate=DateTime.Now;
                    await _context.SaveChangesAsync();
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Updated" });                        
                }                                                                                 
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Get Menu Group List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetMenuGroupList()
        {
            try
            {
                var list=await _context.MenuGroup.ToListAsync();
                var menuGroupList=list.Select(s=>new MenuGroup{MenuGroupID=s.MenuGroupID,MenuGroupName=s.MenuGroupName});
                int totalRecords=menuGroupList.Count();
                return Ok(new {data=menuGroupList, recordsTotal = totalRecords, recordsFiltered = totalRecords});
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Get Single Menu Group by ID
        ///</summary>
        [HttpGet("{id}")]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetSingleMenuGroup(int id)
        {
            try
            {
                var singleMenuGrp=await _menuGroupRepo.SelectById(id);
                return Ok(singleMenuGrp);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Delete Single Menu Group by ID
        ///</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> DeleteSingleMenuGroup(int id)
        {
            try
            { 
                var checkList=await _context.UserRole.Where(opt=>opt.MenuGroupId==id).ToListAsync(); 
                if(checkList.Count==0)
                {
                    await _menuGroupRepo.Delete(id);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Deleted" });
                }
                else
                {
                    return Accepted(new Confirmation { Status = "error", ResponseMsg = "This menu group has assinged user role. Not allowed to delete." });
                }                                                          
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }

        ///<summary>
        ///Create Menu Group
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin,User")]
        [HttpPost]       
        public async Task<ActionResult> CreateMenuGroup(MenuGroup model)
        {
            try
            {
                var objCheck=await _context.MenuGroup.SingleOrDefaultAsync(opt=>opt.MenuGroupName==model.MenuGroupName);
                if(objCheck==null)
                {
                    model.DateAdded=DateTime.Now;
                    model.IsActive=true;                   
                    await _menuGroupRepo.Insert(model);
                    return Ok(new Confirmation { Status = "success", ResponseMsg = "Successfully Saved" });        
                }
                else if(objCheck!=null)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Menu Group name" });
                }
                return Accepted(new Confirmation { Status = "error", ResponseMsg = "Something Unexpected" });
            }
            catch (Exception ex)
            {
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });             
            }
        }

        ///<summary>
        ///Update Menu Group
        ///</summary>
        [Authorize(Roles="SuperAdmin,Admin,User")]
        [HttpPatch]       
        public async Task<ActionResult> UpdateMenuGroup(MenuGroup model)
        {
            try
            {
                var objMenuGroup=await _context.MenuGroup.SingleAsync(opt=>opt.MenuGroupID==model.MenuGroupID);
                var objCheck=await _context.MenuGroup.SingleOrDefaultAsync(opt=>opt.MenuGroupName==model.MenuGroupName);

                if(objCheck!=null && objCheck.MenuGroupName!=objMenuGroup.MenuGroupName)
                {
                    return Accepted(new Confirmation { Status = "duplicate", ResponseMsg = "Duplicate Menu Group name" });
                }
                else
                {
                    objMenuGroup.MenuGroupName=model.MenuGroupName;          
                    objMenuGroup.LastUpdatedBy=model.LastUpdatedBy;
                    objMenuGroup.LastUpdatedDate=DateTime.Now;
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
        ///Get Menu Group wise Menu Mapping List
        ///</summary>
        [HttpGet]
        [Authorize(Roles="SuperAdmin,Admin,User")]
        public async Task<ActionResult> GetMenuGroupWiseMenuMappingList()
        {
            try
            {
                var list=await _context.MenuGroupWiseMenuMapping.ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {              
                return Accepted(new Confirmation { Status = "error", ResponseMsg = ex.Message });
            }
        }
    }
}
