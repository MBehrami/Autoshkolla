using System;
using AdminApi.Models.Menu;
using AdminApi.Models.Others;
using AdminApi.Models.User;
using AdminApi.Models.Candidate;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Models.Helper
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<MenuGroup>(b=>
            {               
                b.HasKey(e=>e.MenuGroupID); 
                b.Property(b=>b.MenuGroupID).HasIdentityOptions(startValue:3);                       
                b.HasData(new MenuGroup
                        {
                            MenuGroupID=1,
                            MenuGroupName="Super Admin Group",
                            IsActive=true,
                            DateAdded=DateTime.Now,
                            AddedBy=1,
                            IsMigrationData=true  
                        },
                        new MenuGroup
                        {
                            MenuGroupID=2,
                            MenuGroupName="User Group",
                            IsActive=true,
                            DateAdded=DateTime.Now,
                            AddedBy=1,
                            IsMigrationData=true  
                        });
            });
            modelBuilder.Entity<UserRole>(b=>
            {
                b.HasKey(e=>e.UserRoleId);  
                b.Property(b=>b.UserRoleId).HasIdentityOptions(startValue:3);        
                b.HasData(
                    new UserRole
                    {
                        UserRoleId=1,
                        RoleName="Admin",
                        MenuGroupId=1,
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        AddedBy=1,
                        IsMigrationData=true
                    },
                    new UserRole
                    {
                        UserRoleId=2,
                        RoleName="User",
                        MenuGroupId=2,
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        AddedBy=1,
                        IsMigrationData=true
                    });          
            });

            modelBuilder.Entity<Users>(b=>{
                b.HasKey(e=>e.UserId);  
                b.Property(b=>b.UserId).HasIdentityOptions(startValue:3);              
                b.HasData(
                    new Users
                    {
                        UserId=1,
                        UserRoleId=1,
                        FullName="John Doe",
                        Email="admin@vueadmin.com",
                        Password="c7JZ+u1vrHp/FQqdD69JQhBTDoLnCNba6dqVyKpEXu4=",
                        PasswordSalt="3fdcuIxZk3sdxmjJPM0FuA==",
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        AddedBy=1,
                        IsPasswordChange=false,
                        IsMigrationData=true
                    },
                    new Users
                    {
                        UserId=2,
                        UserRoleId=2,
                        FullName="Helen Smith",
                        Email="user@vueadmin.com",
                        Password="AJNri22q/JA3fbO4m0QIRYxEChegS461t7J1f7C/td8=",
                        PasswordSalt="gLtMyAEbVeLjZGj312VXyg==",
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        AddedBy=1,
                        IsPasswordChange=false,
                        IsMigrationData=true
                    });
            });

            modelBuilder.Entity<AppMenu>(b=>{
                b.HasKey(e=>e.MenuID);  
                b.Property(b=>b.MenuID).HasIdentityOptions(startValue:15);              
                b.HasData(
                    new AppMenu
                    {
                        MenuID=1,
                        ParentID=0,
                        MenuTitle="Menus",
                        URL="",
                        IsSubMenu=1,
                        SortOrder=2,
                        IconClass="mdi-menu",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=2,
                        ParentID=1,
                        MenuTitle="All Menu",
                        URL="/menu",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=3,
                        ParentID=1,
                        MenuTitle="Menu Group",
                        URL="/menu-group",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=4,
                        ParentID=0,
                        MenuTitle="Users",
                        URL="",
                        IsSubMenu=1,
                        SortOrder=3,
                        IconClass="mdi-account",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=5,
                        ParentID=4,
                        MenuTitle="All User",
                        URL="/users",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=6,
                        ParentID=4,
                        MenuTitle="Roles",
                        URL="/user-role",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=7,
                        ParentID=0,
                        MenuTitle="Logs",
                        URL="",
                        IsSubMenu=1,
                        SortOrder=4,
                        IconClass="mdi-history",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=8,
                        ParentID=7,
                        MenuTitle="Browsing Log",
                        URL="/browse-log",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=9,
                        ParentID=7,
                        MenuTitle="Error Log",
                        URL="/error-log",
                        IsSubMenu=0,
                        SortOrder=0,
                        IconClass="",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=10,
                        ParentID=0,
                        MenuTitle="Dashboard",
                        URL="/dashboard",
                        IsSubMenu=0,
                        SortOrder=1,
                        IconClass="mdi-home",
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },                   
                    new AppMenu
                    {
                        MenuID=13,
                        ParentID=0,
                        MenuTitle="App Settings",
                        URL="/settings",
                        IsSubMenu=0,
                        SortOrder=7,
                        IconClass="mdi-cog",
                        IsActive=false,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=14,
                        ParentID=0,
                        MenuTitle="Candidates",
                        URL="/candidates",
                        IsSubMenu=0,
                        SortOrder=8,
                        IconClass="mdi-account-group",
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    },
                    new AppMenu
                    {
                        MenuID=15,
                        ParentID=0,
                        MenuTitle="Instructors",
                        URL="/instructors",
                        IsSubMenu=0,
                        SortOrder=9,
                        IconClass="mdi-account-tie",
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                 
                    });
            });


            modelBuilder.Entity<MenuGroupWiseMenuMapping>(b=>{
                b.HasKey(e=>e.MenuGroupWiseMenuMappingId);  
                b.Property(b=>b.MenuGroupWiseMenuMappingId).HasIdentityOptions(startValue:13);                
                b.HasData(
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=1,
                        MenuGroupId=1,
                        MenuId=2,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=2,
                        MenuGroupId=1,
                        MenuId=3,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=3,
                        MenuGroupId=1,
                        MenuId=5,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=4,
                        MenuGroupId=1,
                        MenuId=6,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=5,
                        MenuGroupId=1,
                        MenuId=8,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=6,
                        MenuGroupId=1,
                        MenuId=9,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    }, 
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=7,
                        MenuGroupId=1,
                        MenuId=10,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },               
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=8,
                        MenuGroupId=1,
                        MenuId=11,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=9,
                        MenuGroupId=1,
                        MenuId=12,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=10,
                        MenuGroupId=1,
                        MenuId=13,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=11,
                        MenuGroupId=1,
                        MenuId=14,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=12,
                        MenuGroupId=1,
                        MenuId=15,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    },
                    new MenuGroupWiseMenuMapping
                    {
                        MenuGroupWiseMenuMappingId=13,
                        MenuGroupId=2,
                        MenuId=10,                  
                        IsActive=true,
                        DateAdded=DateTime.Now,
                        IsMigrationData=true,
                        AddedBy=1                  
                    });
            });

            modelBuilder.Entity<SiteSettings>(b=>{
                b.HasKey(e=>e.SiteSettingsId);  
                b.Property(b=>b.SiteSettingsId).HasIdentityOptions(startValue:2);              
                b.HasData(
                    new SiteSettings
                    {
                        SiteSettingsId=1,
                        SiteTitle="Vue Admin",
                        WelComeMessage="Hello there,Sign in to start your task!",                       
                        CopyRightText="© 2024 Copyright Vue Admin",
                        DefaultEmail="",
                        Password="",
                        Host="smtp.gmail.com",
                        Port=587,
                        LogoPath="",
                        FaviconPath="", 
                        AppBarColor="#363636",
                        HeaderColor="#F5F5F5",
                        FooterColor="#FFFFFF", 
                        BodyColor="#F9F9F9",
                        AllowWelcomeEmail=true,
                        AllowFaq=true,
                        AllowRightClick=true,                    
                        HomeHeader1="Lorem ipsum dolor sit amet !",
                        HomeDetail1="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                        HomePicture="",
                        HomeHeader2="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam",
                        HomeDetail2="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        HomeBox1Header="Lorem ipsum",
                        HomeBox1Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        HomeBox2Header="Lorem ipsum",
                        HomeBox2Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        HomeBox3Header="Lorem ipsum",
                        HomeBox3Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        HomeBox4Header="Lorem ipsum",
                        HomeBox4Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                        Feature1Header="Lorem ipsum",
                        Feature1Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        Feature1Picture="",
                        Feature2Header="Lorem ipsum",
                        Feature2Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        Feature2Picture="",
                        Feature3Header="Lorem ipsum",
                        Feature3Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        Feature3Picture="",
                        Feature4Header="Lorem ipsum",
                        Feature4Detail="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        Feature4Picture="",
                        RegistrationText="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                        ContactUsText="Lorem ipsum dolor sit amet consectetur adipisicing elit. Iste explicabo commodi quisquam asperiores dolore ad enim provident veniam perferendis voluptate, perspiciatis. ",
                        ContactUsTelephone="+xx (xx) xxxxx-xxxx",
                        ContactUsEmail="email@email.com",
                        FooterText="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.",
                        FooterFacebook="",
                        FooterTwitter="",
                        FooterLinkedin="",
                        FooterInstagram="",
                        ForgetPasswordEmailSubject="Forget Password",
                        ForgetPasswordEmailHeader="Forget Password Header",
                        ForgetPasswordEmailBody="Forget Password Body",
                        WelcomeEmailSubject="Welcome",
                        WelcomeEmailHeader="Welcome Header",
                        WelcomeEmailBody="Welcome Body",
                        Version=1,
                        IsActive=true,
                        DateAdded=DateTime.Now,                       
                        AddedBy=1,                       
                        IsMigrationData=true
                    });
            });

            modelBuilder.Entity<LogHistory>().HasKey(b=>b.LogId);

            modelBuilder.Entity<Category>(b=>{
                b.HasKey(e=>e.CategoryId);
                b.Property(b=>b.CategoryId).HasIdentityOptions(startValue:8);
                b.HasData(
                    new Category{CategoryId=1,CategoryName="A1",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=2,CategoryName="A2",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=3,CategoryName="A",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=4,CategoryName="B",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=5,CategoryName="B+E",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=6,CategoryName="C1",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true},
                    new Category{CategoryId=7,CategoryName="C",IsActive=true,AddedBy=1,DateAdded=DateTime.Now,IsMigrationData=true}
                );
            });
        }
    }
}