using System.Collections.Generic;

namespace AdminApi.Models.Helper
{
    public class SidebarItems
    {
        public int Id{get;set;}
        public string? Title{get;set;}
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<ChildItem>? ChildItems {get;set;}
    } 

    public class ChildItem
    {
        public int Id{get;set;}
        public required string Title{get;set;}
        public required string Route{get;set;}
    }

    public class MenuItems
    {
        public int Id{get;set;}
        public string? Title{get;set;}
        public bool IsParentSelected{get;set;}
        public int groupId{get;set;}
        public List<Child>? Children{get;set;}
    }

    public class Child
    {
        public int Id{get;set;}
        public int groupId{get;set;}
        public required string Title{get;set;}
        public bool IsSelected{get;set;}
    }  
}